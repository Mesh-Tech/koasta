package io.meshtech.koasta.fragment

import android.animation.ObjectAnimator
import android.annotation.SuppressLint
import android.app.Activity
import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.*
import androidx.core.widget.addTextChangedListener
import androidx.fragment.app.DialogFragment
import androidx.navigation.fragment.findNavController
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.OrderStatusActivity
import io.meshtech.koasta.activity.PaymentFlowActivity
import io.meshtech.koasta.billing.BillingManagerActivityRequest
import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.*
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import java.math.BigDecimal
import java.text.NumberFormat
import java.util.*

open class OrderLineBaseViewHolder(val view: View) : RecyclerView.ViewHolder(view) {}

open class OrderLineSimpleViewHolder(view: View) : OrderLineBaseViewHolder(view) {
  private val title = view.findViewById<TextView>(R.id.order_line_cell_title)
  private val price = view.findViewById<TextView>(R.id.order_line_cell_price)

  open fun bind(line: EstimateReceiptLine) {
    title.text = line.title
    price.text = NumberFormat.getCurrencyInstance(Locale.UK).format(line.total)
  }
}

open class OrderSummaryViewHolder(view: View) : OrderLineBaseViewHolder(view) {
  private val title = view.findViewById<TextView>(R.id.order_summary_cell_title)
  private val price = view.findViewById<TextView>(R.id.order_summary_cell_price)

  open fun bind(order: EstimateOrderResult) {
    val quantity = order.receiptLines.filter { it.quantity > 0 }.sumBy { it.quantity }
    title.text = view.resources.getQuantityString(R.plurals.order_bar_cart_quantity, quantity, quantity)
    if (order.receiptTotal.compareTo(BigDecimal.ZERO) == 0) {
      price.text = view.resources.getString(R.string.order_total_price_free)
    } else {
      price.text = NumberFormat.getCurrencyInstance(Locale.UK).format(order.receiptTotal)
    }
  }
}

class OrderLineViewHolder(view: View) : OrderLineSimpleViewHolder(view) {
  private val quantity = view.findViewById<TextView>(R.id.order_line_cell_quantity)

  override fun bind(line: EstimateReceiptLine) {
    super.bind(line)
    quantity.text = line.quantity.toString()
  }
}

class OrderConfirmationListAdapter(context: Context,
                                   private val estimate: EstimateOrderResult) : RecyclerView.Adapter<OrderLineBaseViewHolder>() {
  private val inflater = LayoutInflater.from(context)

  override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): OrderLineBaseViewHolder {
    return if (viewType == 0) {
      val view = inflater.inflate(R.layout.layout_order_line_cell, parent, false)
      OrderLineViewHolder(view)
    } else if (viewType == 1) {
      val view = inflater.inflate(R.layout.layout_order_line_simple_cell, parent, false)
      OrderLineSimpleViewHolder(view)
    } else {
      val view = inflater.inflate(R.layout.layout_order_summary_cell, parent, false)
      OrderSummaryViewHolder(view)
    }
  }

  override fun getItemViewType(position: Int): Int {
    if (position == estimate.receiptLines.size) {
      return 2
    }

    if (estimate.receiptLines[position].quantity == 0 && estimate.receiptLines[position].amount.compareTo(BigDecimal.ZERO) == 0) {
      return 1
    }

    return 0
  }

  override fun getItemCount(): Int {
    return estimate.receiptLines.size + 1
  }

  override fun onBindViewHolder(holder: OrderLineBaseViewHolder, position: Int) {
    if (holder is OrderLineSimpleViewHolder) {
      val value = estimate.receiptLines[position]
      holder.bind(value)
    } else if (holder is OrderSummaryViewHolder) {
      holder.bind(estimate)
    }
  }
}

class OrderConfirmationFragment: DialogFragment(), CoroutineScope by MainScope() {
  private lateinit var venue: Venue
  private var total = BigDecimal(0)
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private val billingManager: IBillingManager by inject()
  private lateinit var order: Order
  private lateinit var estimate: EstimateOrderResult
  private lateinit var recycler: RecyclerView
  private lateinit var title: TextView
  private lateinit var subtitle: TextView
  private lateinit var orderButton: Button
  private lateinit var nativeOrderButton: View
  private lateinit var freeOrderButton: Button
  private lateinit var progress: ProgressBar
  private lateinit var paymentOptions: ViewGroup
  private lateinit var tableNumberCustomisations: ViewGroup
  private lateinit var tableNumber: EditText
  private lateinit var orderNotes: EditText
  private var isRequestingOrder = false
  private var isRequestingPayment = false
  private var paymentAmountInPence: Int = 0
  private var job: Job? = null

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    arguments?.let {
    }
    billingManager.initialise(requireActivity())
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_order_confirmation, container, false)
  }

  @SuppressLint("SetTextI18n")
  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    (activity as PaymentFlowActivity).supportActionBar?.title = getString(R.string.order_confirmation_title)
    recycler = view.findViewById(R.id.order_confirmation_recycler)
    title = view.findViewById(R.id.order_confirmation_venue_label)
    subtitle = view.findViewById(R.id.order_confirmation_summary_label)
    orderButton = view.findViewById(R.id.order_confirmation_order_button)
    freeOrderButton = view.findViewById(R.id.order_confirmation_free_order_button)
    nativeOrderButton = view.findViewById(R.id.order_confirmation_order_gpay_button)
    progress = view.findViewById(R.id.order_confirmation_loading_spinner)
    paymentOptions = view.findViewById(R.id.order_confirmation_payment_options)
    tableNumberCustomisations = view.findViewById(R.id.order_confirmation_customisations_table_number)
    tableNumber = view.findViewById(R.id.order_confirmation_table_number)
    orderNotes = view.findViewById(R.id.order_confirmation_order_notes)
    recycler.layoutManager = LinearLayoutManager(context)

    orderButton.setOnClickListener {
      if (validateForm()) {
        isRequestingPayment = false
        isRequestingOrder = true
        hidePaymentOptions()
        billingManager.requestPayment(requireActivity(), total)
      }
    }

    freeOrderButton.setOnClickListener {
      if (validateForm()) {
        isRequestingPayment = false
        isRequestingOrder = true
        hidePaymentOptions()
        sendOrder(null, null)
      }
    }

    tableNumber.addTextChangedListener {
      if (venue.realServingType() != VenueServingType.BAR_SERVICE) {
        val newNumber = it?.toString() ?: ""
        if (newNumber.isEmpty()) {
          order.table = null
          if (venue.realServingType() == VenueServingType.BAR_AND_TABLE_SERVICE) {
            order.servingType = VenueServingType.BAR_SERVICE.value
          } else {
            nativeOrderButton.isEnabled = false
            orderButton.isEnabled = false
            freeOrderButton.isEnabled = false
          }
        } else {
          order.table = newNumber
          if (venue.realServingType() == VenueServingType.BAR_AND_TABLE_SERVICE) {
            order.servingType = VenueServingType.TABLE_SERVICE.value
          } else {
            nativeOrderButton.isEnabled = true
            orderButton.isEnabled = true
            freeOrderButton.isEnabled = true
          }
        }
      }
    }

    orderNotes.addTextChangedListener {
      order.orderNotes = it?.toString()
    }

    if (caches.cachedOrder == null || caches.cachedOrder!!.orderLines.isEmpty()) {
      findNavController().popBackStack()
      return
    }

    order = caches.cachedOrder!!
    estimate = caches.cachedEstimate!!
    val venueId = order.orderLines[0].venueId
    venue = caches.venueCache[venueId] ?: return
    val ctx = context ?: return

    title.text = venue.venueName
    total = estimate.receiptTotal
    var quantity = 0

    estimate.receiptLines.forEach {
      quantity += it.quantity
    }

    subtitle.text = "${resources.getQuantityString(R.plurals.order_bar_cart_quantity, quantity, quantity)} · ${NumberFormat.getCurrencyInstance(Locale.UK).format(total)}"

    paymentAmountInPence = total.multiply(BigDecimal(1000)).toInt()

    if (venue.realServingType() == VenueServingType.BAR_AND_TABLE_SERVICE) {
      order.servingType = VenueServingType.BAR_SERVICE.value
      tableNumberCustomisations.visibility = View.VISIBLE
    } else if (venue.realServingType() == VenueServingType.BAR_SERVICE) {
      tableNumberCustomisations.visibility = View.GONE
    } else if (venue.realServingType() == VenueServingType.TABLE_SERVICE) {
      tableNumberCustomisations.visibility = View.VISIBLE
    }

    recycler.adapter = OrderConfirmationListAdapter(ctx, estimate)
    recycler.invalidate()

    if (caches.flags.flags.googlePay == true) {
      billingManager.prepareNativePaymentFunctionality(requireActivity()) { showGooglePay ->
        determinePaymentOptions(showGooglePay)
      }
    } else {
      determinePaymentOptions(false)
    }

    nativeOrderButton.setOnClickListener {
      hidePaymentOptions()
      billingManager.requestNativePayment(requireActivity(), total, venue)
    }
  }

  private fun validateForm(): Boolean {
    if (venue.realServingType() == VenueServingType.TABLE_SERVICE && (order.table ?: "").isEmpty()) {
      return false
    }

    return true
  }

  private fun determinePaymentOptions(showGooglePay: Boolean) {
    when {
      estimate.receiptTotal.compareTo(BigDecimal.ZERO) == 0 -> {
        freeOrderButton.visibility = View.VISIBLE
      }
      showGooglePay -> {
        nativeOrderButton.visibility = View.VISIBLE
        orderButton.visibility = View.VISIBLE
      }
      else -> {
        orderButton.visibility = View.VISIBLE
      }
    }

    paymentOptions.visibility = View.VISIBLE
    displayPaymentOptions()
  }

  override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
    super.onActivityResult(requestCode, resultCode, data)

    if (requestCode == BillingManagerActivityRequest.REQUEST_CARD_PAYMENT.value) {
      billingManager.handleRequestCardPaymentResult(venue, total, resultCode, data) {
        if (!it) {
          handleFailedOrder()
        }
      }
    } else if (requestCode == BillingManagerActivityRequest.VERIFY_CARD_PAYMENT.value) {
      billingManager.handleVerifyCardPaymentResult(resultCode, data) { nonce, verificationToken ->
        if (nonce == null || verificationToken == null) {
          handleFailedOrder()
        } else {
          job = sendOrder(nonce, verificationToken)
        }
      }
    } else if (requestCode == BillingManagerActivityRequest.REQUEST_GOOGLE_PAYMENT.value) {
      if (resultCode == Activity.RESULT_OK) {
        billingManager.resumeNativePayment(requireActivity(), total, data!!) { paymentReference ->
          if (paymentReference == null) {
            handleFailedOrder()
          } else {
            sendOrder(paymentReference, null)
          }
        }
      } else {
        handleFailedOrder()
      }
    }
  }

  private fun sendOrder(paymentReference: String?, verificationToken: String?) = launch {
    val realOrder = Order(order.orderLines, paymentReference, verificationToken, order.nonce, order.orderNotes, order.servingType, order.table)
    val results = api.sendOrder(realOrder)

    if (results.data == null) {
      handleFailedOrder()
    } else {
      handleSuccessOrder(results.data)
    }
  }

  private fun handleFailedOrder() {
    Toast.makeText(context, getString(R.string.order_failed), Toast.LENGTH_LONG).show()
    displayPaymentOptions()
  }

  @Suppress("UNUSED_PARAMETER")
  private fun handleSuccessOrder(result: SendOrderResult) {
    val intent = Intent(requireActivity(), OrderStatusActivity::class.java)
    intent.putExtra(OrderStatusActivity.BUNDLE_KEY_ORDER_ID, result.orderId)
    requireActivity().finish()
    requireActivity().startActivity(intent)
  }

  override fun onDestroyView() {
    super.onDestroyView()
    job?.cancel()
    job = null
  }

  private fun displayPaymentOptions() {
    nativeOrderButton.isEnabled = validateForm()
    orderButton.isEnabled = validateForm()
    freeOrderButton.isEnabled = validateForm()

    run {
      val from = 0.0f
      val to = 1.0f
      val slider = ObjectAnimator.ofFloat(paymentOptions, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }

    run {
      val from = 1.0f
      val to = 0.0f
      val slider = ObjectAnimator.ofFloat(progress, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }
  }

  private fun hidePaymentOptions() {
    nativeOrderButton.isEnabled = false
    orderButton.isEnabled = false
    freeOrderButton.isEnabled = false

    run {
      val from = 1.0f
      val to = 0.0f
      val slider = ObjectAnimator.ofFloat(paymentOptions, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }

    run {
      val from = 0.0f
      val to = 1.0f
      val slider = ObjectAnimator.ofFloat(progress, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }
  }
}

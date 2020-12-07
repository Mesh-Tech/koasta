package io.meshtech.koasta.activity

import android.annotation.SuppressLint
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.Bundle
import android.view.LayoutInflater
import android.view.MenuItem
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import io.meshtech.koasta.R
import io.meshtech.koasta.fragment.OrdersFragment
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.HistoricalLineItem
import io.meshtech.koasta.net.model.HistoricalOrder
import io.meshtech.koasta.net.model.OrderStatus
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import java.math.BigDecimal
import java.math.RoundingMode
import java.text.NumberFormat
import java.text.SimpleDateFormat
import java.util.*

class OrderStatusViewHolder(val view: View) : RecyclerView.ViewHolder(view) {
  private val cell = view.findViewById<ViewGroup>(R.id.order_line)
  private val lineTitle = view.findViewById<TextView>(R.id.order_line_cell_title)
  private val linePrice = view.findViewById<TextView>(R.id.order_line_cell_price)
  private val summary = view.findViewById<ViewGroup>(R.id.order_summary)
  private val summaryTitle = view.findViewById<TextView>(R.id.order_summary_card_title)
  private val summarySubtitle = view.findViewById<TextView>(R.id.order_summary_card_subtitle)
  private val summaryComment = view.findViewById<TextView>(R.id.order_summary_card_comment)
  private val status = view.findViewById<ViewGroup>(R.id.order_status)
  private val statusTitle = view.findViewById<TextView>(R.id.order_status_card_title)
  private val statusSubtitle = view.findViewById<TextView>(R.id.order_status_card_subtitle)
  private val total = view.findViewById<ViewGroup>(R.id.order_total)
  private val totalTitle = view.findViewById<TextView>(R.id.order_total_title)
  private val totalPrice = view.findViewById<TextView>(R.id.order_total_price)
  private val formatter: SimpleDateFormat by lazy {
    SimpleDateFormat("dd/MM/yyyy '·' h:mm a", Locale.UK)
  }

  @SuppressLint("SetTextI18n")
  fun bind(line: HistoricalLineItem) {
    cell.visibility = View.VISIBLE
    summary.visibility = View.GONE
    status.visibility = View.GONE
    total.visibility = View.GONE

    if (line.quantity > 0) {
      lineTitle.text = "${line.quantity}x ${line.productName}"
      linePrice.text = NumberFormat.getCurrencyInstance(Locale.UK).format(
        BigDecimal(line.quantity).setScale(2, RoundingMode.HALF_EVEN).multiply(line.amount)
      )
    } else {
      lineTitle.text = line.productName
      linePrice.text = NumberFormat.getCurrencyInstance(Locale.UK).format(line.amount)
    }
  }

  fun bind(order: HistoricalOrder, type: Int) {
    when(type) {
      0 -> {
        cell.visibility = View.GONE
        summary.visibility = View.GONE
        status.visibility = View.VISIBLE
        total.visibility = View.GONE

        when (OrderStatus.fromValue(order.orderStatus)) {
          OrderStatus.ORDERED -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_ordered)
            var subtitle = view.context.getString(R.string.string_order_status_subtitle_ordered)
            subtitle += if (order.servingType == 1) {
              "\n\n" + view.context.getString(R.string.string_order_status_table_service)
            } else {
              "\n\n" + view.context.getString(R.string.string_order_status_bar_service)
            }

            statusSubtitle.text = subtitle
          }
          OrderStatus.IN_PROGRESS -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_in_progress)

            var subtitle = view.context.getString(R.string.string_order_status_subtitle_in_progress)
            subtitle += if (order.servingType == 1) {
              "\n\n" + view.context.getString(R.string.string_order_status_table_service)
            } else {
              "\n\n" + view.context.getString(R.string.string_order_status_bar_service)
            }

            statusSubtitle.text = subtitle
          }
          OrderStatus.READY -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_ready)
            if (order.servingType == 1) {
              statusSubtitle.text = view.context.getString(R.string.string_order_status_subtitle_ready_table_service)
            } else {
              statusSubtitle.text = view.context.getString(R.string.string_order_status_subtitle_ready_bar_service)
            }
          }
          OrderStatus.REJECTED -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_rejected)
            statusSubtitle.text = view.context.getString(R.string.string_order_status_subtitle_rejected)
          }
          OrderStatus.COMPLETE -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_complete)
            statusSubtitle.text = view.context.getString(R.string.string_order_status_subtitle_complete)
          }
          OrderStatus.PAYMENT_PENDING -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_payment_pending)
            statusSubtitle.text = view.context.getString(R.string.string_order_status_subtitle_payment_pending)
          }
          OrderStatus.PAYMENT_FAILED -> {
            statusTitle.text = view.context.getString(R.string.string_order_status_title_payment_failed)
            statusSubtitle.text = view.context.getString(R.string.string_order_status_subtitle_payment_failed)
          }
          else -> {
            statusTitle.text = null
            statusSubtitle.text = null
          }
        }
      }
      1 -> {
        cell.visibility = View.GONE
        summary.visibility = View.VISIBLE
        status.visibility = View.GONE
        total.visibility = View.GONE
        summaryTitle.text = order.venueName
        summarySubtitle.text = order.orderNumber.toString()
        summaryComment.text = view.context.getString(R.string.order_status_summary_comment, formatter.format(order.orderTimeStamp))
      }
      2 -> {
        cell.visibility = View.GONE
        summary.visibility = View.GONE
        status.visibility = View.GONE
        total.visibility = View.VISIBLE

        var totalValue = order.total
        var quantity = 0

        order.lineItems.forEach {
          quantity += it.quantity
        }

        totalTitle.text = view.resources.getQuantityString(R.plurals.order_bar_cart_quantity, quantity, quantity)
        totalPrice.text = NumberFormat.getCurrencyInstance(Locale.UK).format(totalValue)
      }
      else -> {}
    }
  }
}

class OrderStatusListAdapter(context: Context, private val order: HistoricalOrder) : RecyclerView.Adapter<OrderStatusViewHolder>() {
  private val inflater = LayoutInflater.from(context)

  override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): OrderStatusViewHolder {
    val view = inflater.inflate(R.layout.layout_order_status_item, parent, false)
    return OrderStatusViewHolder(view)
  }

  override fun getItemCount(): Int {
    return order.lineItems.size + 3
  }

  override fun getItemId(position: Int): Long = when (position) {
    0 -> -1
    1 -> -2
    else -> {
      when(position) {
        order.lineItems.size -> -3
        else -> order.lineItems[position].id.toLong()
      }
    }
  }

  override fun onBindViewHolder(holder: OrderStatusViewHolder, position: Int) = when (position) {
    0 -> {
      holder.bind(order, 0)
    }
    1 -> {
      holder.bind(order, 1)
    }
    else -> {
      if (position - 2 >= order.lineItems.size) {
        holder.bind(order, 2)
      } else {
        val value = order.lineItems[position - 2]
        holder.bind(value)
      }
    }
  }
}

class OrderStatusActivity : AppCompatActivity(), CoroutineScope by MainScope() {
  companion object {
    const val BUNDLE_KEY_ORDER_ID = "order-id"
    const val BROADCAST_EVENT_ORDER_UPDATED = "order-updated"
  }

  private val api: IApi by inject()
  private var job: Job? = null
  private var order: HistoricalOrder? = null
  private var orderId: Int = 0
  private var receiver: BroadcastReceiver? = null
  private lateinit var recycler: RecyclerView

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_order_status)
    setSupportActionBar(findViewById(R.id.toolbar))

    supportActionBar?.setDisplayHomeAsUpEnabled(true)
    supportActionBar?.title = ""

    orderId = intent.getIntExtra(BUNDLE_KEY_ORDER_ID, -1)

    if (orderId == -1) {
      finish()
      return
    }

    recycler = findViewById(R.id.recycler)
    recycler.layoutManager = LinearLayoutManager(this)
    job = fetchData()
  }

  override fun onResume() {
    super.onResume()
    receiver = object : BroadcastReceiver() {
      override fun onReceive(context: Context?, intent: Intent?) {
        fetchData()
      }
    }

    registerReceiver(receiver!!, IntentFilter(OrdersFragment.BROADCAST_EVENT_ORDERS_UPDATED))
  }

  override fun onPause() {
    unregisterReceiver(receiver!!)
    super.onPause()
  }

  private fun fetchData() = launch {
    val result = api.getOrder(orderId)
    if (result.data != null) {
      bind(result.data)
    } else {
      showError()
    }
  }

  private fun showError() {
    Toast.makeText(this, getString(R.string.order_error), Toast.LENGTH_LONG).show()
  }

  private fun bind(newOrder: HistoricalOrder) {
    val order = newOrder.withLineItem(HistoricalLineItem(newOrder.serviceCharge, -1, "Service charge", 0))
    val dateFormatter = SimpleDateFormat("h:mm a", Locale.UK)
    supportActionBar?.title = dateFormatter.format(order.orderTimeStamp)
    recycler.adapter = OrderStatusListAdapter(this, order)
    recycler.invalidate()
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id: Int = item.itemId
    return if (id == android.R.id.home) {
      onBackPressed()
      true
    } else {
      super.onOptionsItemSelected(item)
    }
  }
}

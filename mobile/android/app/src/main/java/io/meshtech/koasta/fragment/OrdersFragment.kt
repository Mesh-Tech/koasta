package io.meshtech.koasta.fragment

import android.annotation.SuppressLint
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.os.Bundle
import android.view.Gravity
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import android.widget.Toast
import androidx.fragment.app.Fragment
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout
import hirondelle.date4j.DateTime
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.OrderStatusActivity
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.HistoricalOrder
import io.meshtech.koasta.net.model.OrderStatus
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import java.text.NumberFormat
import java.util.*


interface OnOrderItemClickListener {
  fun onOrderItemClick(order: HistoricalOrder)
}

class OrderViewHolder(val view: View) : RecyclerView.ViewHolder(view) {
  @SuppressLint("SetTextI18n")
  fun bind(order: HistoricalOrder) {
    val title = view.findViewById<TextView>(R.id.order_item_title)
    val subtitle = view.findViewById<TextView>(R.id.order_item_subtitle)
    val comment = view.findViewById<TextView>(R.id.order_item_comment)
    val price = view.findViewById<TextView>(R.id.order_item_price)

    val total = order.total
    var quantity = 0

    order.lineItems.forEach {
      quantity += it.quantity
    }

    title.text = order.venueName
    subtitle.text = view.resources.getQuantityString(R.plurals.order_bar_cart_quantity, quantity, quantity)
    when (OrderStatus.fromValue(order.orderStatus)) {
      OrderStatus.UNKNOWN -> {
        comment.text = "Unknown"
        comment.alpha = 0.8f
        comment.setTextColor(view.resources.getColor(R.color.colorForeground, null))
      }
      OrderStatus.ORDERED -> {
        comment.text = "Pending"
        comment.alpha = 0.8f
        comment.setTextColor(view.resources.getColor(R.color.colorForeground, null))
      }
      OrderStatus.IN_PROGRESS -> {
        comment.text = "In Progress"
        comment.alpha = 1.0f
        comment.setTextColor(view.resources.getColor(R.color.colorOrange, null))
      }
      OrderStatus.READY -> {
        comment.text = "Ready to Collect"
        comment.alpha = 1.0f
        comment.setTextColor(view.resources.getColor(R.color.colorPaleGreen, null))
      }
      OrderStatus.REJECTED -> {
        comment.text = "Cancelled"
        comment.alpha = 1.0f
        comment.setTextColor(view.resources.getColor(R.color.colorPrimaryBright, null))
      }
      OrderStatus.COMPLETE -> {
        comment.text = "Collected"
        comment.alpha = 1.0f
        comment.setTextColor(view.resources.getColor(R.color.colorPaleGreen, null))
      }
      OrderStatus.PAYMENT_PENDING -> {
        comment.text = "Incomplete"
        comment.alpha = 1.0f
        comment.setTextColor(view.resources.getColor(R.color.colorPrimaryBright, null))
      }
      OrderStatus.PAYMENT_FAILED -> {
        comment.text = "Incomplete"
        comment.alpha = 1.0f
        comment.setTextColor(view.resources.getColor(R.color.colorPrimaryBright, null))
      }
    }
    price.text = NumberFormat.getCurrencyInstance(Locale.UK).format(total)
  }

  fun bind(title: String) {
    val titleView = view.findViewById<TextView>(R.id.section_header_title)
    titleView.text = title
  }
}

data class OrderListItem(val order: HistoricalOrder?, val title: String?, val id: Int)

class OrderListAdapter(context: Context,
                       private val values: List<HistoricalOrder>,
                       private val listener: OnOrderItemClickListener) : RecyclerView.Adapter<OrderViewHolder>() {
  private val inflater = LayoutInflater.from(context)
  private val items: List<OrderListItem>

  init {
    val today = mutableListOf<HistoricalOrder>()
    val yesterday = mutableListOf<HistoricalOrder>()
    val older = mutableListOf<HistoricalOrder>()

    val todayDate = DateTime.forInstant(Date().time, TimeZone.getDefault())
      .changeTimeZone(TimeZone.getDefault(), TimeZone.getTimeZone("UTC"))
    val yesterdayDate = todayDate.minusDays(1)

    values.forEach {
      val date = DateTime.forInstant(it.orderTimeStamp.time, TimeZone.getTimeZone("UTC"))
      if (date.isSameDayAs(todayDate)) {
        today.add(it)
      } else if (date.isSameDayAs(yesterdayDate)) {
        yesterday.add(it)
      } else {
        older.add(it)
      }
    }

    today.sortByDescending { it.orderTimeStamp }
    yesterday.sortByDescending { it.orderTimeStamp }
    older.sortByDescending { it.orderTimeStamp }

    val allItems = mutableListOf<OrderListItem>()

    if (today.isNotEmpty()) {
      allItems.add(OrderListItem(null, "Today", -1))
      today.forEach {
        allItems.add(OrderListItem(it, null, it.orderId))
      }
    }

    if (yesterday.isNotEmpty()) {
      allItems.add(OrderListItem(null, "Yesterday", -2))
      yesterday.forEach {
        allItems.add(OrderListItem(it, null, it.orderId))
      }
    }

    if (older.isNotEmpty()) {
      allItems.add(OrderListItem(null, "Older", -3))
      older.forEach {
        allItems.add(OrderListItem(it, null, it.orderId))
      }
    }

    items = allItems
  }

  override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): OrderViewHolder {
    val view = inflater.inflate(if (viewType == 2) R.layout.layout_order_item else R.layout.layout_section_header, parent, false)
    return OrderViewHolder(view)
  }

  override fun getItemViewType(position: Int): Int {
    return if (items[position].id >= 0) {
      2
    } else {
      1
    }
  }

  override fun getItemCount(): Int {
    return items.size
  }

  override fun getItemId(position: Int): Long {
    return items[position].id.toLong()
  }

  override fun onBindViewHolder(holder: OrderViewHolder, position: Int) {
    val value = items[position]

    if (getItemViewType(position) == 2) {
      holder.view.setOnClickListener {
        listener.onOrderItemClick(value.order!!)
      }
      holder.bind(value.order!!)
    } else {
      holder.bind(value.title!!)
    }
  }
}

class OrdersFragment : Fragment(), CoroutineScope by MainScope(), OnOrderItemClickListener {
  private var job: Job? = null
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private var orders: List<HistoricalOrder> = emptyList()
  private var hasLoaded = false
  private var receiver: BroadcastReceiver? = null
  private lateinit var loading: ViewGroup
  private lateinit var empty: SwipeRefreshLayout
  private lateinit var recycler: RecyclerView
  private lateinit var refresh: SwipeRefreshLayout

  companion object {
    const val BROADCAST_EVENT_ORDERS_UPDATED = "orders-updated"

    @JvmStatic
    fun newInstance() =
      OrdersFragment().apply {
        arguments = Bundle().apply {
        }
      }
  }

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    arguments?.let {
    }
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    // Inflate the layout for this fragment
    return inflater.inflate(R.layout.fragment_orders, container, false)
  }

  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)

    loading = view.findViewById(R.id.loading)
    empty = view.findViewById(R.id.empty)
    refresh = view.findViewById(R.id.refresh)
    recycler = view.findViewById(R.id.recycler)
    recycler.layoutManager = LinearLayoutManager(requireActivity())

    refresh.setOnRefreshListener {
      dataRefresh()
    }

    empty.setOnRefreshListener {
      dataRefresh()
    }

    if (!hasLoaded) {
      showLoadingState()
      dataRefresh()
    } else if (orders.isEmpty()) {
      showEmptyState()
    } else {
      showLoadedState()
    }
  }

  override fun onResume() {
    super.onResume()
    receiver = object : BroadcastReceiver() {
      override fun onReceive(context: Context?, intent: Intent?) {
        dataRefresh()
      }
    }

    requireActivity().registerReceiver(receiver!!, IntentFilter(BROADCAST_EVENT_ORDERS_UPDATED))
  }

  override fun onPause() {
    requireActivity().unregisterReceiver(receiver!!)
    super.onPause()
  }

  private fun dataRefresh() {
    if (caches.orderCache.isEmpty()) {
      job = doRefresh()
    } else {
      orders = caches.orderCache
      showLoadedState()
    }
  }

  private fun doRefresh() = launch {
    val result = api.getOrders()
    when {
      result.data == null -> {
        showFetchError()
      }
      result.data.isEmpty() -> {
        showEmptyState()
      }
      else -> {
        orders = result.data
        caches.orderCache = orders
        showLoadedState()
      }
    }
  }

  private fun showLoadingState() {
    loading.visibility = View.VISIBLE
    refresh.visibility = View.GONE
    empty.visibility = View.GONE
    refresh.isRefreshing = false
    empty.isRefreshing = false
  }

  private fun showEmptyState() {
    loading.visibility = View.GONE
    refresh.visibility = View.GONE
    empty.visibility = View.VISIBLE
    refresh.isRefreshing = false
    empty.isRefreshing = false
  }

  private fun showLoadedState() {
    loading.visibility = View.GONE
    refresh.visibility = View.VISIBLE
    empty.visibility = View.GONE
    refresh.isRefreshing = false
    empty.isRefreshing = false
    recycler.adapter = OrderListAdapter(requireActivity(), orders, this)
    recycler.invalidate()
  }

  private fun showFetchError() {
    val toast = Toast.makeText(requireActivity(), R.string.orders_fetch_failed, Toast.LENGTH_LONG)
    toast.setGravity(Gravity.TOP or Gravity.CENTER_HORIZONTAL, 0, 0)
    toast.show()
  }

  override fun onDestroyView() {
    super.onDestroyView()
    job?.cancel()
    job = null
  }

  override fun onOrderItemClick(order: HistoricalOrder) {
    val intent = Intent(requireActivity(), OrderStatusActivity::class.java)
    intent.putExtra(OrderStatusActivity.BUNDLE_KEY_ORDER_ID, order.orderId)
    requireActivity().startActivity(intent)
  }
}

package io.meshtech.koasta.fragment

import android.animation.ObjectAnimator
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ProgressBar
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AlertDialog
import androidx.core.animation.doOnEnd
import androidx.fragment.app.Fragment
import androidx.navigation.fragment.findNavController
import androidx.viewpager2.adapter.FragmentStateAdapter
import androidx.viewpager2.widget.ViewPager2
import com.google.android.material.tabs.TabLayout
import com.google.android.material.tabs.TabLayoutMediator
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.PaymentFlowActivity
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.*
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import java.lang.ref.WeakReference
import java.math.BigDecimal
import java.math.RoundingMode
import java.text.NumberFormat
import java.util.*


interface VenueMenuFragmentListener {
  fun showOrderBar()
  fun hideOrderBar()
  fun setOrderBarLoading(loading: Boolean)
  fun getVenue(): Venue
  fun getMenu(idx: Int): Menu
  fun getMenuCount(): Int
  fun addProductToCart(product: Product)
  fun removeAllOfProductFromCart(product: Product)
  fun getCart(): Map<Int, Int>
  fun getSelectionQuantity(id: Int): Int
}

class VenueMenuPagerAdapter(fragment: Fragment, fragmentListener: VenueMenuFragmentListener) : FragmentStateAdapter(fragment) {
  private val listener: WeakReference<VenueMenuFragmentListener> = WeakReference(fragmentListener)

  override fun getItemCount(): Int = listener.get()?.getMenuCount() ?: 0
  override fun createFragment(position: Int): Fragment {
    val fragment = VenueProductListFragment.newInstance(position)
    fragment.setListener(listener)

    return fragment
  }
}

class VenueMenuFragment : VenueMenuFragmentListener, Fragment(), CoroutineScope by MainScope() {
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private var job: Job? = null
  private var venueId: Int = -1
  private var initialMenuId: Int = -1
  private var venue: Venue? = null
  private var menus: List<Menu> = emptyList()
  private lateinit var orderBar: ViewGroup
  private lateinit var viewPager: ViewPager2
  private lateinit var tabLayout: TabLayout
  private lateinit var orderBarQuantity: TextView
  private lateinit var orderBarAmount: TextView
  private lateinit var orderBarConfirmButton: Button
  private lateinit var progress: ProgressBar
  private var cart = mutableMapOf<Int, Int>()
  private var productIndex = mutableMapOf<Int, Product>()

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    arguments?.let {
      venueId = it.getInt(PaymentFlowActivity.VENUE_ID)
      initialMenuId = it.getInt(PaymentFlowActivity.INITIAL_MENU_ID, -1)
    }
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_venue_menu, container, false)
  }

  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    orderBar = view.findViewById(R.id.order_bar_layout)
    orderBar.translationY = orderBar.height.toFloat()
    orderBarQuantity = orderBar.findViewById(R.id.order_bar_details_items)
    orderBarAmount = orderBar.findViewById(R.id.order_bar_details_price)
    orderBarConfirmButton = orderBar.findViewById(R.id.order_bar_view_order_button)
    progress = orderBar.findViewById(R.id.progress_bar)
    tabLayout = view.findViewById(R.id.tab_layout)
    viewPager = view.findViewById(R.id.pager)

    orderBarConfirmButton.setOnClickListener {
      confirmOrder()
    }

    job = fetchData()
  }

  private fun fetchData() = launch {
    var venue: Venue? = null

    if (caches.venueCache.containsKey(venueId)) {
      venue = caches.venueCache[venueId]!!
    } else {
      val result = api.getVenue(venueId)
      if (result.data != null) {
        venue = result.data
      }
    }

    if (venue == null) {
      showError()
    } else {
      val menus: List<Menu>
      if (caches.venueMenuCache.containsKey(venueId)) {
        menus = caches.venueMenuCache[venueId] ?: emptyList()
        bind(venue, menus)
      } else {
        val menuResult = api.getMenus(venueId)
        if (menuResult.data == null) {
          showError()
        } else {
          menus = menuResult.data
          bind(venue, menus)
        }
      }
    }
  }

  private fun showError() {
    val ctx = activity ?: return
    Toast.makeText(ctx, getString(R.string.venue_menu_error), Toast.LENGTH_LONG).show()
  }

  private fun bind(venue: Venue, menus: List<Menu>) {
    caches.venueCache[venueId] = venue
    caches.venueMenuCache[venueId] = menus
    this.venue = venue
    this.menus = menus
    (activity as PaymentFlowActivity).supportActionBar?.title = venue.venueName
    viewPager.adapter = VenueMenuPagerAdapter(this, this)
    TabLayoutMediator(tabLayout, viewPager) { tab, position ->
      tab.text = menus[position].menuName
    }.attach()

    menus.forEach {
      it.products.forEach { product ->
        productIndex[product.productId] = product
      }
    }

    if (initialMenuId != -1) {
      val idx = menus.indexOfFirst { it.menuId == initialMenuId }
      if (idx >= 0) {
        viewPager.currentItem = idx
      }
    }

    if (caches.cachedCart.isNotEmpty()) {
      cart.putAll(caches.cachedCart)
      updateOrderBar()
      orderBar.visibility = View.VISIBLE
      orderBar.translationY = 0.0f
    }
  }

  private fun updateOrderBar() {
    var total = BigDecimal(0).setScale(2, RoundingMode.HALF_EVEN)
    var quantity = 0

    cart.forEach {
      val (productId, productQuantity) = it
      total = total.add(
        productIndex[productId]!!.price.multiply(
          BigDecimal(productQuantity).setScale(2, RoundingMode.HALF_EVEN)
        )
      )
      quantity += productQuantity
    }

    orderBarQuantity.text = resources.getQuantityString(R.plurals.order_bar_cart_quantity, quantity, quantity)
    orderBarAmount.text = NumberFormat.getCurrencyInstance(Locale.UK).format(total)
  }

  override fun showOrderBar() {
    updateOrderBar()
    orderBar.visibility = View.VISIBLE

    val from = orderBar.height.toFloat()
    val to = 0.0f
    val slider = ObjectAnimator.ofFloat(orderBar, "translationY", from, to);
    slider.duration = 300
    slider.start()
  }

  override fun hideOrderBar() {
    val from = 0.0f
    val to = orderBar.height.toFloat()
    val slider = ObjectAnimator.ofFloat(orderBar, "translationY", from, to)
    slider.duration = 300
    slider.doOnEnd {
      orderBar.visibility = View.INVISIBLE
      updateOrderBar()
    }
    slider.start()
  }

  override fun setOrderBarLoading(loading: Boolean) {
    if (loading) {
      orderBarConfirmButton.isEnabled = false
      progress.visibility = View.VISIBLE
    } else {
      orderBarConfirmButton.isEnabled = true
      progress.visibility = View.GONE
    }
  }

  override fun getVenue(): Venue {
    return venue!!
  }

  override fun getMenu(idx: Int): Menu {
    return this.menus[idx]
  }

  override fun getMenuCount(): Int {
    return this.menus.count()
  }

  override fun addProductToCart(product: Product) {
    if (venue?.isOpen != true) {
      Toast.makeText(requireActivity(), getString(R.string.venue_closed_error), Toast.LENGTH_LONG).show()
      return
    }

    val shouldShow = cart.isEmpty()

    cart[product.productId] = ((cart[product.productId] ?: 0) + 1)

    if (shouldShow) {
      showOrderBar()
    } else {
      updateOrderBar()
    }
  }

  override fun removeAllOfProductFromCart(product: Product) {
    if (venue?.isOpen != true) {
      return
    }

    cart.remove(product.productId)

    if (cart.isEmpty()) {
      hideOrderBar()
    } else {
      updateOrderBar()
    }
  }

  override fun getCart(): Map<Int, Int> = cart
  override fun getSelectionQuantity(id: Int): Int = cart[id] ?: 0

  override fun onDestroyView() {
    job?.cancel()
    job = null
    super.onDestroyView()
  }

  private fun confirmOrder() {
    if (cart.isEmpty()) {
      return
    }

    val lines = cart.map { OrderLine(venueId, it.key, it.value) }
    val order = DraftOrder(lines)

    setOrderBarLoading(true)
    job = doFetchEstimate(order)
  }

  private fun doFetchEstimate(order: DraftOrder) = launch {
    val estimate = api.requestOrderEstimate(order)

    if (estimate.error != null || estimate.data == null) {
      onEstimateError()
    } else {
      onEstimateSuccess(estimate.data, order)
    }
  }

  private fun onEstimateSuccess(estimate: EstimateOrderResult, draft: DraftOrder) {
    caches.cachedOrder = Order(draft.orderLines, "", "", UUID.randomUUID().toString(), null, venue?.servingType ?: VenueServingType.BAR_SERVICE.value, null)
    caches.cachedCart = cart
    caches.cachedEstimate = estimate

    val ageRestricted = cart.any {
      productIndex[it.key]!!.ageRestricted
    }

    if (ageRestricted) {
      AlertDialog.Builder(requireActivity())
        .setTitle(getString(R.string.age_confirm_dialog_title))
        .setMessage(getString(R.string.age_confirm_dialog_message))
        .setPositiveButton(getString(R.string.button_continue)) { _, _ ->
          findNavController().navigate(R.id.nav_venue_menu_to_order_confirmation)
          setOrderBarLoading(false)
        }
        .setNegativeButton(android.R.string.cancel) { _, _ ->
          setOrderBarLoading(false)
        }
        .setIcon(R.drawable.icon_age_warning)
        .show()
    } else {
      findNavController().navigate(R.id.nav_venue_menu_to_order_confirmation)
      setOrderBarLoading(false)
    }
  }

  private fun onEstimateError() {
    val ctx = activity ?: return
    Toast.makeText(ctx, getString(R.string.venue_menu_estimate_error), Toast.LENGTH_LONG).show()
    setOrderBarLoading(false)
  }
}

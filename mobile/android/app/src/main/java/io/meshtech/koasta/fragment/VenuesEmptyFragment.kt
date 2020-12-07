package io.meshtech.koasta.fragment

import android.annotation.SuppressLint
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import androidx.fragment.app.Fragment
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.MainActivity

class VenuesEmptyFragment() : Fragment(), IVenuesFragment {
  private lateinit var locationCard: ViewGroup
  private var visibleCards: Array<VenuesCard> = emptyArray()
  private var viewCreated = false
  private lateinit var locationCardEnableLocationButton: Button
  private lateinit var refresh: SwipeRefreshLayout

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_venues_empty, container, false)
  }

  @SuppressLint("ClickableViewAccessibility")
  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    viewCreated = true
    refresh = view as SwipeRefreshLayout
    locationCard = view.findViewById(R.id.venues_empty_location_card)
    locationCardEnableLocationButton = view.findViewById(R.id.enable_location_card_button)

    visibleCards.forEach {
      when (it) {
        VenuesCard.location -> locationCard.visibility = View.VISIBLE
      }
    }

    refresh.setOnRefreshListener {
      val parent = activity
      if (parent is MainActivity) {
        parent.dataRefresh()
      }
    }

    visibleCards = emptyArray()

    registerAgainstActivity()

    locationCardEnableLocationButton.setOnClickListener {
      val parent = activity
      if (parent is MainActivity) {
        parent.requestLocation()
      }
    }
  }

  private fun registerAgainstActivity() {
    val parent = activity
    if (parent !is MainActivity) {
      return
    }

    parent.registerFragment(this)
  }

  companion object {
    /**
     * Use this factory method to create a new instance of
     * this fragment.
     */
    @JvmStatic
    fun newInstance() =
      VenuesEmptyFragment().apply { arguments = Bundle().apply {} }
  }

  override fun showCard(card: VenuesCard) {
    if (!viewCreated) {
      visibleCards += card
    } else {
      locationCard.visibility = View.VISIBLE
    }
  }

  override fun hideCard(card: VenuesCard) {
    if (!viewCreated) {
      visibleCards = visibleCards.filterNot { it != card }.toTypedArray()
    } else {
      locationCard.visibility = View.GONE
    }
  }

  override fun invalidateData() {}
}

package io.meshtech.koasta.fragment

import android.annotation.SuppressLint
import android.os.Bundle
import androidx.fragment.app.Fragment
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ScrollView
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.MainActivity

class VenuesLoadingFragment : Fragment(), IVenuesFragment {
  private lateinit var scrollView: ScrollView
  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    // Inflate the layout for this fragment
    return inflater.inflate(R.layout.fragment_venues_loading, container, false)
  }

  @SuppressLint("ClickableViewAccessibility")
  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    scrollView = view.findViewById(R.id.venues_loading_scrollview)
    scrollView.setOnTouchListener { _, _ ->
      true
    }
    registerAgainstActivity()
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
      VenuesLoadingFragment().apply {
        arguments = Bundle().apply {}
      }
  }

  override fun showCard(card: VenuesCard) {}
  override fun hideCard(card: VenuesCard) {}
  override fun invalidateData() {}
}

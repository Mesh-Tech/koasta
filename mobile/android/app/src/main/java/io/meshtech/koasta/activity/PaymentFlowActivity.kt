package io.meshtech.koasta.activity

import android.content.Intent
import android.os.Bundle
import android.view.MenuItem
import androidx.appcompat.app.AppCompatActivity
import androidx.navigation.Navigation
import androidx.navigation.findNavController
import androidx.navigation.fragment.NavHostFragment
import io.meshtech.koasta.R
import io.meshtech.koasta.billing.BillingManagerActivityRequest

class PaymentFlowActivity : AppCompatActivity() {
  companion object {
    const val VENUE_ID: String = "venueId"
    const val INITIAL_MENU_ID: String = "menuId"
  }
  private val navController by lazy { findNavController(R.id.payment_flow_fragment) }

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_payment_flow)
    setSupportActionBar(findViewById(R.id.toolbar))

    if (!intent.hasExtra(VENUE_ID)) {
      return
    }

    val venueId = intent.getIntExtra(VENUE_ID, -1)

    val bundle = Bundle()
    bundle.putInt(VENUE_ID, venueId)
    navController.setGraph(R.navigation.payment_flow_nav_graph, bundle)
    supportActionBar?.setDisplayHomeAsUpEnabled(true)
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id = item.itemId

    if (id == android.R.id.home) {
      val ctrl = Navigation.findNavController(this, R.id.payment_flow_fragment)
      if (!ctrl.popBackStack()) {
        onBackPressed()
      }
      return true
    }

    return super.onOptionsItemSelected(item)
  }

  /**
   * This is needed in order to test payment flows without activating our payment provider's UI
   */
  fun delegateActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
    if (requestCode == BillingManagerActivityRequest.REQUEST_CARD_PAYMENT.value
      || requestCode == BillingManagerActivityRequest.VERIFY_CARD_PAYMENT.value
      || requestCode == BillingManagerActivityRequest.REQUEST_GOOGLE_PAYMENT.value) {
      val navHostFragment = supportFragmentManager.fragments.first() as? NavHostFragment
      if(navHostFragment != null) {
        val childFragments = navHostFragment.childFragmentManager.fragments
        childFragments.forEach { fragment ->
          fragment.onActivityResult(requestCode, resultCode, data)
        }
      }
    }
  }

  override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
    super.onActivityResult(requestCode, resultCode, data)
    delegateActivityResult(requestCode, resultCode, data)
  }
}

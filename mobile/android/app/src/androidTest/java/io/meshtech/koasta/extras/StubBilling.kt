package io.meshtech.koasta.extras

import android.app.Activity
import android.content.Intent
import io.meshtech.koasta.activity.PaymentFlowActivity
import io.meshtech.koasta.billing.BillingManagerActivityRequest
import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.net.model.Venue
import java.lang.ref.WeakReference
import java.math.BigDecimal

class StubBilling: IBillingManager {
  var success = true
  private var realActivity = WeakReference<PaymentFlowActivity>(null)

  override fun initialise(activity: Activity) {}

  override fun requestPayment(activity: Activity, total: BigDecimal) {
    val realActivity = if (activity is PaymentFlowActivity) {
      activity
    } else {
      return
    }

    this.realActivity = WeakReference(realActivity)

    realActivity.delegateActivityResult(
      BillingManagerActivityRequest.REQUEST_CARD_PAYMENT.value, 0, null
    )
  }

  override fun handleRequestCardPaymentResult(
    venue: Venue,
    total: BigDecimal,
    resultCode: Int,
    data: Intent?,
    callback: (Boolean) -> Unit
  ) {
    callback(success)
    realActivity.get()?.delegateActivityResult(
      BillingManagerActivityRequest.VERIFY_CARD_PAYMENT.value, 0, null
    )
  }

  override fun handleVerifyCardPaymentResult(
    resultCode: Int,
    data: Intent?,
    callback: (String?, String?) -> Unit
  ) {
    callback("a", "b")
  }
}

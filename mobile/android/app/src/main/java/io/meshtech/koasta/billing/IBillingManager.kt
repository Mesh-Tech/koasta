package io.meshtech.koasta.billing

import android.app.Activity
import android.content.Intent
import io.meshtech.koasta.net.model.Venue
import java.math.BigDecimal

enum class BillingManagerActivityRequest(val value: Int) {
  REQUEST_GOOGLE_PAYMENT(1337),
  REQUEST_CARD_PAYMENT(7331),
  VERIFY_CARD_PAYMENT(7332)
}

enum class BillingManagerActivityResult(val value: Int) {
  PAYMENT_CONFIRM_READY(0), PAYMENT_CONFIRM_FAILED(1)
}

interface IBillingManager {
  fun initialise(activity: Activity)
  fun requestPayment(activity: Activity, total: BigDecimal)
  fun prepareNativePaymentFunctionality(activity: Activity, callback: (Boolean) -> Unit)
  fun requestNativePayment(activity: Activity, total: BigDecimal, venue: Venue)
  fun resumeNativePayment(activity: Activity, total: BigDecimal, data: Intent, callback: (String?) -> Unit)
  fun handleRequestCardPaymentResult(venue: Venue, total: BigDecimal, resultCode: Int, data: Intent?, callback: (Boolean) -> Unit)
  fun handleVerifyCardPaymentResult(resultCode: Int, data: Intent?, callback: (String?, String?) -> Unit)
  fun handleRequestGooglePaymentResult(venue: Venue, total: BigDecimal, resultCode: Int, data: Intent?, callback: (Boolean) -> Unit)
}

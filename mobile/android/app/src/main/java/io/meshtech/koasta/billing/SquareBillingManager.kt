package io.meshtech.koasta.billing

import android.app.Activity
import android.content.Intent
import com.google.android.gms.wallet.*
import io.meshtech.koasta.BuildConfig
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.net.model.Venue
import sqip.*
import sqip.GooglePay.requestGooglePayNonce
import java.lang.ref.WeakReference
import java.math.BigDecimal


class SquareBillingManager(private val sessionManager: ISessionManager): IBillingManager {
  private var cardResult: CardEntryActivityResult.Success? = null
  private var currentActivity: WeakReference<Activity>? = null
  private var paymentsClient: PaymentsClient? = null

  override fun initialise(activity: Activity) {
    this.currentActivity = WeakReference(activity)
  }

  override fun requestPayment(activity: Activity, total: BigDecimal) {
    CardEntry.startCardEntryActivity(activity, true, BillingManagerActivityRequest.REQUEST_CARD_PAYMENT.value);
  }

  override fun prepareNativePaymentFunctionality(activity: Activity, callback: (Boolean) -> Unit) {
    paymentsClient = Wallet.getPaymentsClient(
      activity,
      Wallet.WalletOptions.Builder().setEnvironment(
        if (BuildConfig.PAYMENTS_SANDBOX) WalletConstants.ENVIRONMENT_TEST else WalletConstants.ENVIRONMENT_PRODUCTION
      ).build()
    )

    paymentsClient!!.isReadyToPay(GooglePay.createIsReadyToPayRequest())
      .addOnCompleteListener(activity) { task -> callback(task.isSuccessful) }
  }

  override fun requestNativePayment(activity: Activity, total: BigDecimal, venue: Venue) {
    AutoResolveHelper.resolveTask(
      paymentsClient!!.loadPaymentData(
        GooglePay.createPaymentDataRequest(
          venue.externalLocationId ?: "",
          TransactionInfo.newBuilder().setTotalPriceStatus(
            WalletConstants.TOTAL_PRICE_STATUS_ESTIMATED
          ).setCurrencyCode("GBP").setTotalPrice(total.toString()).build()
        )
      ),
      activity,
      BillingManagerActivityRequest.REQUEST_GOOGLE_PAYMENT.value
    )
  }

  override fun resumeNativePayment(activity: Activity, total: BigDecimal, data: Intent, callback: (String?) -> Unit) {
    val paymentData = PaymentData.getFromIntent(data);
    val googlePayToken = paymentData?.paymentMethodToken?.token
    if (googlePayToken == null) {
      callback(null)
      return
    }

    requestGooglePayNonce(googlePayToken).enqueue { result: GooglePayNonceResult ->
      if (result.isSuccess()) {
        val nonce = result.getSuccessValue().nonce
        callback(nonce)
      } else if (result.isError()) {
        callback(null)
      }
    }
  }

  override fun handleRequestCardPaymentResult(venue: Venue, total: BigDecimal, resultCode: Int, data: Intent?, callback: (Boolean) -> Unit) {
    val activity: Activity? = currentActivity?.get()

    if (activity == null) {
      callback(false)
      return
    }

    CardEntry.handleActivityResult(data) { result ->
      if (result.isSuccess()) {
        val resultData  = result.getSuccessValue()
        cardResult = resultData
        val params = VerificationParameters(
          resultData.nonce,
          BuyerAction.Charge(
            Money(total.multiply(BigDecimal(100)).toInt(), Currency.GBP)
          ),
          SquareIdentifier.LocationToken(venue.externalLocationId ?: ""),
          Contact.Builder().build(sessionManager.firstName ?: "")
        )
        BuyerVerification.verify(activity, params, BillingManagerActivityRequest.VERIFY_CARD_PAYMENT.value)
        callback(true)
      } else if (result.isCanceled()) {
        callback(false)
      }
    }
  }

  override fun handleVerifyCardPaymentResult(resultCode: Int, data: Intent?, callback: (String?, String?) -> Unit) {
    val activity: Activity? = currentActivity?.get()

    if (activity == null || cardResult == null) {
      callback(null, null)
      return
    }

    val nonce = cardResult?.nonce.orEmpty()

    BuyerVerification.handleActivityResult(data) { result ->
      if (result.isSuccess()) {
        val verificationToken = result.getSuccessValue().verificationToken
        callback(nonce, verificationToken)
      } else {
        callback(null, null)
      }
    }
  }

  override fun handleRequestGooglePaymentResult(venue: Venue, total: BigDecimal, resultCode: Int, data: Intent?, callback: (Boolean) -> Unit) {

  }
}

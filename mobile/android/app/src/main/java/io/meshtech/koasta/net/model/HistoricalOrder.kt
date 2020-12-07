package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass
import java.math.BigDecimal
import java.util.*

enum class OrderStatus(val value: Int) {
  UNKNOWN(0),
  ORDERED(1),
  IN_PROGRESS(2),
  READY(3),
  COMPLETE(4),
  REJECTED(5),
  PAYMENT_PENDING(6),
  PAYMENT_FAILED(7);

  companion object {
    fun fromString(str: String): OrderStatus = when(str) {
      "Ordered" -> ORDERED
      "InProgress" -> IN_PROGRESS
      "Ready" -> READY
      "Complete" -> COMPLETE
      "Rejected" -> REJECTED
      "PaymentPending" -> PAYMENT_PENDING
      "PaymentFailed" -> PAYMENT_FAILED
      else -> UNKNOWN
    }

    fun fromValue(value: Int): OrderStatus = when(value) {
      1 -> ORDERED
      2 -> IN_PROGRESS
      3 -> READY
      4 -> COMPLETE
      5 -> REJECTED
      6 -> PAYMENT_PENDING
      7 -> PAYMENT_FAILED
      else -> UNKNOWN
    }
  }
}

@JsonClass(generateAdapter = true)
data class HistoricalLineItem(val amount: BigDecimal, val id: Int, val productName: String, val quantity: Int)

@JsonClass(generateAdapter = true)
data class HistoricalOrder(val companyId: Int, val externalPaymentId: String?, val firstName: String?,
                           val lastName: String?, val orderId: Int, val orderNumber: Int, val orderStatus: Int,
                           val orderTimeStamp: Date, val userId: Int, val venueName: String, val lineItems: List<HistoricalLineItem>,
                           val total: BigDecimal, val serviceCharge: BigDecimal, var orderNotes: String?,
                           var servingType: Int, var table: String?) {
  fun withLineItem(lineItem: HistoricalLineItem): HistoricalOrder {
    val items = lineItems.toMutableList()
    items.add(lineItem)
    return HistoricalOrder(companyId, externalPaymentId, firstName, lastName, orderId, orderNumber,
      orderStatus, orderTimeStamp, userId, venueName, items, total, serviceCharge, orderNotes, servingType, table)
  }
}

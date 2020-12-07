package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class SendOrderResult (
  val orderNumber: Int,
  val orderId: Int,
  val status: Int
)

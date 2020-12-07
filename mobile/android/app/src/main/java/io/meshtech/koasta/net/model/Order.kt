package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class Order (
  var orderLines: List<OrderLine>,
  var paymentProcessorReference: String?,
  var paymentVerificationReference: String?,
  var nonce: String,
  var orderNotes: String?,
  var servingType: Int,
  var table: String?
)

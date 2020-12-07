package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass
import java.math.BigDecimal

@JsonClass(generateAdapter = true)
data class EstimateReceiptLine (
  val amount: BigDecimal,
  val total: BigDecimal,
  val quantity: Int,
  val title: String
)

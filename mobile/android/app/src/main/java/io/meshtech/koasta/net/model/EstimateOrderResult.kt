package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass
import java.math.BigDecimal

@JsonClass(generateAdapter = true)
data class EstimateOrderResult(
  var receiptLines: List<EstimateReceiptLine>,
  var receiptTotal: BigDecimal
)

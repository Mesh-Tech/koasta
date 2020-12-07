package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass
import java.math.BigDecimal

@JsonClass(generateAdapter = true)
data class Product (
  val productId: Int,
  val ageRestricted: Boolean,
  val productType: String,
  val productName: String,
  val productDescription: String?,
  val price: BigDecimal,
  val image: String?
)

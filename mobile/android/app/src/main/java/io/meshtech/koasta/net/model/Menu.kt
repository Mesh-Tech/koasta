package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class Menu (
  val menuId: Int,
  val menuName: String,
  val menuDescription: String?,
  val products: List<Product> = emptyList()
)

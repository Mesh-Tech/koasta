package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class OrderLine (var venueId: Int, var productId: Int, var quantity: Int)

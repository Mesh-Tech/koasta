package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class DraftOrder (var orderLines: List<OrderLine>)

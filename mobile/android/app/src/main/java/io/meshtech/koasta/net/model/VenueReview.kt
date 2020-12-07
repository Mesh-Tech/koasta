package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class VenueReview (val summary: String?,
                        val detail: String?,
                        val rating: Int?,
                        val registeredInterest: Boolean)

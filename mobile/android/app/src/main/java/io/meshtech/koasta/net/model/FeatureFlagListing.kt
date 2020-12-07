package io.meshtech.koasta.net.model

import com.squareup.moshi.Json
import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class FeatureFlagListing(
  @Json(name = "facebook-auth") val facebookAuth: Boolean?,
  @Json(name = "google-pay") val googlePay: Boolean?
)

@JsonClass(generateAdapter = true)
data class FeatureFlags(val flags: FeatureFlagListing)

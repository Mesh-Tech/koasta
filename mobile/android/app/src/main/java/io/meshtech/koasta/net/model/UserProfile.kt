package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class UserProfile(
  val registrationId: String,
  val wantAdvertising: Boolean,
  var votedVenueIds: List<Int>,
  val firstName: String?,
  val lastName: String?
)

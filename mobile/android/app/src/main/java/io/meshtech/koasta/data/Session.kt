package io.meshtech.koasta.data

import java.util.*

interface Session {
  @Deprecated(message = "Use social auth providers instead")
  var authenticationToken: String?
  @Deprecated(message = "Use social auth providers instead")
  var refreshToken: String?
  var lastVenue: String?
  var cachedVenueLocations: Map<String, String>?
  var pushToken: String?
  var hasSkippedOnboarding: Boolean?
  var phoneNumber: String?
  @Deprecated(message = "Use social auth providers instead")
  var authTokenExpiry: Date?
  @Deprecated(message = "Use social auth providers instead")
  var refreshTokenExpiry: Date?
  var source: Int?
}

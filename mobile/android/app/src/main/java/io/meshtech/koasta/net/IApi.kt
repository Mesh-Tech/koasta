package io.meshtech.koasta.net

import io.meshtech.koasta.net.model.*

enum class ApiLegalDocument(val value: String) {
  PRIVACY_POLICY("privacy-policy"),
  TERMS_AND_CONDITIONS("terms-and-conditions");

  companion object {
    fun fromValue(value: String): ApiLegalDocument = when(value) {
      "privacy-policy" -> PRIVACY_POLICY
      else -> TERMS_AND_CONDITIONS
    }
  }
}

enum class AuthTokenType(val value: Int) {
  UNKNOWN(0), FACEBOOK(1), APPLE(2)
}

interface IApi {
  suspend fun login(credentials: Credentials): ApiPlainResult
  suspend fun getVenues(lat: Double? = null, lon: Double? = null): ApiResult<List<Venue>>
  suspend fun queryVenues(str: String): ApiResult<List<Venue>>
  suspend fun getMenus(venueId: Int): ApiResult<List<Menu>>
  suspend fun getVenue(venueId: Int): ApiResult<Venue>
  suspend fun sendOrder(order: Order): ApiResult<SendOrderResult>
  suspend fun requestOrderEstimate(order: DraftOrder): ApiResult<EstimateOrderResult>
  suspend fun getOrders(): ApiResult<List<HistoricalOrder>>
  suspend fun createDevice(token: String): ApiResult<Void>
  suspend fun getOrder(id: Int): ApiResult<HistoricalOrder>
  suspend fun fetchLegalDocument(type: ApiLegalDocument): ApiResult<String>
  suspend fun submitReview(id: Int, review: VenueReview): ApiResult<Void>
  suspend fun getFlags(): ApiResult<FeatureFlags>
  suspend fun getUserProfile(): ApiResult<UserProfile>
}

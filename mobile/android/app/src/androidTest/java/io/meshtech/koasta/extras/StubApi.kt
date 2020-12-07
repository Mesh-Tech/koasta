package io.meshtech.koasta.extras

import io.meshtech.koasta.net.ApiLegalDocument
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.*

class StubApi: IApi {
  var loginResult = ApiPlainResult(null)
  var getVenuesResult = ApiResult<List<Venue>>(null, null)
  var queryVenuesResult = ApiResult<List<Venue>>(null, null)
  var getMenusResult = ApiResult<List<Menu>>(null, null)
  var getVenueResult = ApiResult<Venue>(null, null)
  var sendOrderResult = ApiResult<SendOrderResult>(null, null)
  var requestOrderEstimateResult = ApiResult<EstimateOrderResult>(null, null)
  var getOrdersResult = ApiResult<List<HistoricalOrder>>(null, null)
  var createDeviceResult = ApiResult<Void>(null, null)
  var getOrderResult = ApiResult<HistoricalOrder>(null, null)
  var fetchLegalDocumentResult = ApiResult<String>(null, null)
  var submitReviewResult = ApiResult<Void>(null, null)

  override suspend fun login(): ApiPlainResult = loginResult
  override suspend fun getVenues(lat: Double?, lon: Double?): ApiResult<List<Venue>> = getVenuesResult
  override suspend fun queryVenues(str: String): ApiResult<List<Venue>> = queryVenuesResult
  override suspend fun getMenus(venueId: Int): ApiResult<List<Menu>> = getMenusResult
  override suspend fun getVenue(venueId: Int): ApiResult<Venue> = getVenueResult
  override suspend fun sendOrder(order: Order): ApiResult<SendOrderResult> = sendOrderResult
  override suspend fun requestOrderEstimate(order: DraftOrder): ApiResult<EstimateOrderResult> = requestOrderEstimateResult
  override suspend fun getOrders(): ApiResult<List<HistoricalOrder>> = getOrdersResult
  override suspend fun createDevice(token: String): ApiResult<Void> = createDeviceResult
  override suspend fun getOrder(id: Int): ApiResult<HistoricalOrder> = getOrderResult
  override suspend fun fetchLegalDocument(type: ApiLegalDocument): ApiResult<String> = fetchLegalDocumentResult
  override suspend fun submitReview(id: Int, review: VenueReview): ApiResult<Void> = submitReviewResult
}

import Foundation
import Hydra

class WebApi: Api {
  fileprivate var venueApi: WebVenueApi!
  fileprivate var menuListApi: WebMenuListApi!
  fileprivate var authApi: WebAuthApi!
  fileprivate var orderapi: WebOrderApi!
  fileprivate var userApi: WebUserApi!
  fileprivate var legalApi: WebLegalApi!
  fileprivate var flagApi: WebFlagApi!

  init(venueApi: WebVenueApi?, menuListApi: WebMenuListApi?, authApi: WebAuthApi?, orderApi: WebOrderApi?, userApi: WebUserApi?, legalApi: WebLegalApi?, flagApi: WebFlagApi?) {
    guard let venueApi = venueApi, let menuListApi = menuListApi, let authApi = authApi, let orderApi = orderApi, let userApi = userApi, let legalApi = legalApi, let flagApi = flagApi else {
      return
    }

    self.venueApi = venueApi
    self.menuListApi = menuListApi
    self.authApi = authApi
    self.orderapi = orderApi
    self.userApi = userApi
    self.legalApi = legalApi
    self.flagApi = flagApi
  }

  func getVenue(forId id: String) -> Promise<Venue?> {
    return venueApi.getVenue(forId: id)
  }

  func getNearbyVenues(lat: Double, lon: Double, limit: Int) -> Promise<[Venue]?> {
    return venueApi.getNearbyVenues(lat: lat, lon: lon, limit: limit)
  }

  func getVenues(query: String) -> Promise<[Venue]?> {
    return venueApi.getVenues(query: query)
  }

  func submitReview(forId id: String, review: VenueReview) -> Promise<Void> {
    return venueApi.submitReview(forId: id, review: review)
  }

  func getMenuList(forId id: String) -> Promise<[Menu]?> {
    return menuListApi.getMenuList(forId: id)
  }

  func login(firstName: String?, lastName: String?) -> Promise<Void> {
    return authApi.login(firstName: firstName, lastName: lastName)
  }

  func sendOrder(_ order: Order) -> Promise<SendOrderResult> {
    return orderapi.sendOrder(order)
  }

  func requestOrderEstimate(_ order: DraftOrder) -> Promise<EstimateOrderResult> {
    return orderapi.requestOrderEstimate(order)
  }

  func getOrders() -> Promise<[HistoricalOrder]> {
    return orderapi.getOrders()
  }

  func getOrder(orderId: Int) -> Promise<HistoricalOrder> {
    return orderapi.getOrder(orderId: orderId)
  }

  func getPaymentProviderEphemeralKey() -> Promise<AuthEphemeralKeyResult> {
    return authApi.getPaymentProviderEphemeralKey()
  }

  func registerPushToken(_ registration: PushRegistration) -> Promise<Void> {
    return userApi.registerPushToken(registration)
  }

  func fetchTermsAndConditions() -> Promise<String> {
    return legalApi.fetchTermsAndConditions()
  }

  func fetchPrivacyPolicy() -> Promise<String> {
    return legalApi.fetchPrivacyPolicy()
  }

  func fetchCurrentFlags() -> Promise<FeatureFlags> {
    return flagApi.fetchCurrentFlags()
  }

  func fetchCurrentProfile() -> Promise<UserProfile> {
    return userApi.fetchCurrentProfile()
  }
}

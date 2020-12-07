import Foundation
import Hydra
@testable import pubcrawl


class StubApi: Api {
  var getVenueResult: Venue? = nil
  var getMenuListResult: [Menu]? = nil
  var getVenuesResult: [Venue]? = nil
  var getVenueShouldThrow = false
  var getEphemeralKeyResult: AuthEphemeralKeyResult? = nil
  var getNearbyVenuesResult: [Venue]? = nil
  var sendOrderResult: SendOrderResult? = nil
  var getOrdersResult: [HistoricalOrder]? = nil
  var getOrderResult: HistoricalOrder? = nil
  var getEstimateResult: EstimateOrderResult? = nil
  var getProfileResult: UserProfile = UserProfile(registrationId: "a", wantAdvertising: true, votedVenueIds: [123], firstName: nil, lastName: nil)

  var getMenuListShouldThrow = false
  var loginResultShouldThrow = false
  var getVenuesShouldThrow = false
  var getEphemeralKeyShouldThrow = false
  var getNearbyVenuesShouldThrow = false
  var sendOrderShouldThrow = false
  var getOrdersShouldThrow = false
  var getOrderShouldThrow = false
  var getEstimateShouldThrow = false
  var submitReviewShouldThrow = false
  var getProfileShouldThrow = false

  func login(firstName: String?, lastName: String?) -> Promise<Void> {
    return async({ _ in
      if self.loginResultShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }
    })
  }

  func getNearbyVenues(lat: Double, lon: Double, limit: Int) -> Promise<[Venue]?> {
    return async({ _ in
      if self.getNearbyVenuesShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getNearbyVenuesResult
    })
  }

  func getVenues(query: String) -> Promise<[Venue]?> {
    return async({ _ in
      if self.getVenuesShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getVenuesResult
    })
  }

  func getPaymentProviderEphemeralKey() -> Promise<AuthEphemeralKeyResult> {
    return async({ _ in
      if self.getEphemeralKeyShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getEphemeralKeyResult!
    })
  }

  func sendOrder(_ order: Order) -> Promise<SendOrderResult> {
    return async({ _ in
      if self.sendOrderShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.sendOrderResult!
    })
  }

  func getOrders() -> Promise<[HistoricalOrder]> {
    return async({ _ in
      if self.getOrdersShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getOrdersResult!
    })
  }

  func getOrder(orderId: Int) -> Promise<HistoricalOrder> {
    return async({ _ in
      if self.getOrderShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getOrderResult!
    })
  }

  func fetchPrivacyPolicy() -> Promise<String> {
    return Promise(resolved: "abc")
  }

  func fetchTermsAndConditions() -> Promise<String> {
    return Promise(resolved: "def")
  }

  func fetchCurrentFlags() -> Promise<FeatureFlags> {
    return Promise(resolved: FeatureFlags(flags: FeatureFlagListing(facebookAuth: true)))
  }


  func registerPushToken(_ registration: PushRegistration) -> Promise<Void> {
    return Promise(resolved: ())
  }

  func getVenue(forId id: String) -> Promise<Venue?> {
    return async({ _ in
      if self.getVenueShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getVenueResult
    })
  }

  func getMenuList(forId id: String) -> Promise<[Menu]?> {
    return async({ _ in
      if self.getMenuListShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getMenuListResult
    })
  }

  func submitReview(forId id: String, review: VenueReview) -> Promise<Void> {
    return async({ _ in
      if self.submitReviewShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }
    })
  }

  func requestOrderEstimate(_ order: DraftOrder) -> Promise<EstimateOrderResult> {
    return async({ _ in
      if self.getEstimateShouldThrow {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return self.getEstimateResult!
    })
  }

  func fetchCurrentProfile() -> Promise<UserProfile> {
    return async({ _ in
      if self.getProfileShouldThrow {
        throw ApiError(statusCode: 401, body: nil, bodyData: nil)
      }

      return self.getProfileResult
    })
  }
}

import Foundation
import Hydra

protocol VenueApi {
  func getVenue(forId id: String) -> Promise<Venue?>
  func getNearbyVenues(lat: Double, lon: Double, limit: Int) -> Promise<[Venue]?>
  func getVenues(query: String) -> Promise<[Venue]?>
  func submitReview(forId id: String, review: VenueReview) -> Promise<Void>
}

protocol MenuListApi {
  func getMenuList(forId id: String) -> Promise<[Menu]?>
}

struct AuthLoginResult {
  enum Status {
    case verify
    case done
  }

  let status: Status
}

struct AuthEphemeralKeyResult: Decodable {
  let key: StripeEphemeralKey
}

protocol AuthApi {
  func login(firstName: String?, lastName: String?) -> Promise<Void>

  func getPaymentProviderEphemeralKey() -> Promise<AuthEphemeralKeyResult>
}

struct SendOrderResult: Codable {
  let orderNumber: Int
  let orderId: Int
  let status: Int
  let total: Decimal
  let serviceCharge: Decimal
}

struct EstimateReceiptLine: Codable {
  let amount: Decimal
  let total: Decimal
  let quantity: Int
  let title: String
}

struct EstimateOrderResult: Codable {
  let receiptLines: [EstimateReceiptLine]
  let receiptTotal: Decimal
}

protocol OrderApi {
  func sendOrder(_ order: Order) -> Promise<SendOrderResult>
  func getOrders() -> Promise<[HistoricalOrder]>
  func getOrder(orderId: Int) -> Promise<HistoricalOrder>
  func requestOrderEstimate(_ order: DraftOrder) -> Promise<EstimateOrderResult>
}

protocol UserApi {
  func registerPushToken(_ registration: PushRegistration) -> Promise<Void>
  func fetchCurrentProfile() -> Promise<UserProfile>
}

protocol LegalApi {
  func fetchPrivacyPolicy() -> Promise<String>

  func fetchTermsAndConditions() -> Promise<String>
}

protocol FlagApi {
  func fetchCurrentFlags() -> Promise<FeatureFlags>
}

protocol Api: VenueApi, MenuListApi, AuthApi, OrderApi, UserApi, LegalApi, FlagApi {
}

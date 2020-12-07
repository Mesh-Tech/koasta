import Foundation

struct ConfirmPurchaseContext {
  let venueId: Int
  let order: [ProductSelection]
  let estimate: EstimateOrderResult
  let nonce: String
}

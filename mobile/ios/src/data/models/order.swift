import Foundation

struct Order: Codable {
  let orderLines: [OrderLine]
  let paymentProcessorReference: String?
  let paymentVerificationReference: String?
  let nonce: String
  let orderNotes: String?
  let servingType: VenueServingType
  let table: String?
}

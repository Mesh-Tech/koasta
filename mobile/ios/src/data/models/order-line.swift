import Foundation

struct OrderLine: Codable {
  let venueId: Int
  let productId: Int
  let quantity: Int
}

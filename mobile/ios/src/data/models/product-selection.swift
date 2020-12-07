import Foundation

struct ProductSelection: Codable, Hashable, Equatable {
  func hash(into hasher: inout Hasher) {
    hasher.combine(productId.hashValue)
    hasher.combine(quantity.hashValue)
    hasher.combine(price.hashValue)
    hasher.combine(ageRestricted.hashValue)
  }

  static func ==(lhs: ProductSelection, rhs: ProductSelection) -> Bool {
    return lhs.productId == rhs.productId && lhs.quantity == rhs.quantity && lhs.price == rhs.price && lhs.ageRestricted == rhs.ageRestricted
  }

  let productId: Int
  let quantity: Int
  let price: Decimal
  let ageRestricted: Bool
}

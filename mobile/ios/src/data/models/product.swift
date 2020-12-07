import Foundation

struct Product: Codable {

  let productId: Int
  let ageRestricted: Bool
  let productType: String
  let productName: String
  let productDescription: String?
  let price: Decimal
  let image: String?
}

extension Product {
  var imageUrl: URL? {
    if let image = image, let url = URL(string: image) {
      return url
    }

    return nil
  }
}

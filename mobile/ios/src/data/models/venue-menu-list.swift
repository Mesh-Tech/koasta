import Foundation

struct VenueProduct: Codable, Hashable, Equatable, Identifiable {
  func hash(into hasher: inout Hasher) {
    hasher.combine(productId.hashValue)
    hasher.combine(ageRestricted.hashValue)
    hasher.combine(productType.hashValue)
    hasher.combine(productName.hashValue)
    hasher.combine((productDescription ?? "").hashValue)
    hasher.combine(price.hashValue)
    hasher.combine((image?.absoluteString ?? "").hashValue)
    hasher.combine((selection?.hashValue ?? 0))
  }

  var identityId: Int { return productId }

  static func ==(lhs: VenueProduct, rhs: VenueProduct) -> Bool {
    return lhs.productId == rhs.productId
      && lhs.ageRestricted == rhs.ageRestricted
      && lhs.productType == rhs.productType
      && lhs.productName == rhs.productName
      && lhs.productDescription == rhs.productDescription
      && lhs.price == rhs.price
      && lhs.image == rhs.image
      && lhs.selection == rhs.selection
  }

  let productId: Int
  let ageRestricted: Bool
  let productType: String
  let productName: String
  let productDescription: String?
  let price: Decimal
  let image: URL?
  var selection: ProductSelection?

  init (_ productLiteral: Product) {
    productId = productLiteral.productId
    ageRestricted = productLiteral.ageRestricted
    productType = productLiteral.productType
    productName = productLiteral.productName
    productDescription = productLiteral.productDescription
    price = productLiteral.price
    image = productLiteral.imageUrl
  }
}

struct VenueMenu: Codable, Equatable, Hashable, Collection {
  func hash(into hasher: inout Hasher) {
    hasher.combine(menuId.hashValue)
    hasher.combine(menuName.hashValue)
    hasher.combine(menuDescription.hashValue)
  }

  static func ==(lhs: VenueMenu, rhs: VenueMenu) -> Bool {
    return lhs.menuId == rhs.menuId
      && lhs.menuName == rhs.menuName
      && lhs.menuDescription == rhs.menuDescription
  }

  let menuId: Int
  let menuName: String
  let menuDescription: String?
  var productList: [VenueProduct] = Array()

  init (_ menuLiteral: Menu) {
    menuId = menuLiteral.menuId
    menuName = menuLiteral.menuName
    menuDescription = menuLiteral.menuDescription
    productList = menuLiteral.products.map {
      VenueProduct($0)
    }
  }

  var startIndex: Int { return productList.startIndex }
  var endIndex: Int { return productList.endIndex }

  subscript(index: Int) -> VenueProduct {
    return productList[index]
  }

  func index(after i: Int) -> Int {
    precondition(i < endIndex, "Can't advance beyond endIndex")
    return i + 1
  }
}

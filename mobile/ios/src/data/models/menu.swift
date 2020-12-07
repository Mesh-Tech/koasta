import Foundation

struct Menu: Codable {
  let menuId: Int
  let menuName: String
  let menuDescription: String?
  var products: [Product] = Array()

  init(menuId: Int, menuName: String, menuDescription: String, products list: [Product]?) {
    self.menuId = menuId
    self.menuName = menuName
    self.menuDescription = menuDescription
    self.products = list ?? []
  }
}

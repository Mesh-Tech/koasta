import Foundation

struct MenuList: Codable {
  let menuListId: Int
  let menuListTitle: String?
  let menuListDescription: String?
  let menuImage: URL?
  let menus: [Menu]?
}

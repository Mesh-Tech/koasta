import Foundation

struct Credentials: Codable {
  let firstName: String?
  let lastName: String?

  init(firstName: String? = nil, lastName: String? = nil) {
    self.firstName = firstName
    self.lastName = lastName
  }
}

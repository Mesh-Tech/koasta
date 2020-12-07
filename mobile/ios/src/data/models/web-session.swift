import Foundation

struct WebSession: Codable {
  let authToken: String
  let refreshToken: String
  let refreshExpiry: Date
  let expiry: Date
}

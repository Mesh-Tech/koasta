import Foundation

struct RefreshWebSession: Codable {
  let authToken: String
  let refreshToken: String
  let refreshTokenExpiry: Date
  let authTokenExpiry: Date
}

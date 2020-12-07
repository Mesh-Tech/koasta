import Foundation

enum SessionSource: Int, Codable {
  case apple = 0
  case facebook = 1
}

protocol Session: Codable {
  @available(iOS, deprecated: 1.0.0)
  var authenticationToken: String? {get set}
  @available(iOS, deprecated: 1.0.0)
  var refreshToken: String? {get set}
  var lastVenue: String? {get set}
  var cachedVenueLocations: [String:String]? {get set}
  var pushToken: String? {get set}
  var hasSkippedOnboarding: Bool? {get set}
  @available(iOS, deprecated: 1.0.0)
  var phoneNumber: String? {get set}
  @available(iOS, deprecated: 1.0.0)
  var authTokenExpiry: Date? {get set}
  @available(iOS, deprecated: 1.0.0)
  var refreshTokenExpiry: Date? {get set}
  var source: SessionSource? {get set}
}

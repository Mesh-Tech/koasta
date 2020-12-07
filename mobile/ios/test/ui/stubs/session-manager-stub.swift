import Foundation
import UIKit
import Hydra

@testable import pubcrawl

class StubSession: Session {
  var authenticationToken: String?
  var refreshToken: String?
  var lastVenue: String?
  var cachedVenueLocations: [String : String]?
  var pushToken: String?
  var hasSkippedOnboarding: Bool?
  var phoneNumber: String?
  var authTokenExpiry: Date?
  var refreshTokenExpiry: Date?
  var source: SessionSource?
}

class StubSessionManager: SessionManager {
  var lastName: String? = ""
  var firstName: String? = "bobobo"
  var isAuthenticated: Bool = true
  var isExpired: Bool = false
  var authToken: String? = "abc"
  var accountDescription: String? = "def"
  var authType: String? = "facebook"
  var currentSession: Session?
  var currentProfile: UserProfile?

  func registerBespokeSession(_ bespokeSession: Any?) {}

  func restore() -> Promise<Session> {
    guard let session = currentSession else {
      return Promise(rejected: ApiError(statusCode: 500, body: nil, bodyData: nil))
    }

    return Promise(resolved: session)
  }

  func persist(session: Session) -> Promise<Void> {
    return Promise(resolved: ())
  }

  func purge() -> Promise<Void> {
    return Promise(resolved: ())
  }

  func updateProfile(_ userProfile: UserProfile) {
    currentProfile = userProfile
  }
}

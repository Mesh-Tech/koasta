import Foundation
import FBSDKLoginKit
import Hydra

class FacebookSessionProvider: SessionProvider {
  fileprivate let config: Config!

  init(config: Config!) {
    self.config = config
  }

  func initialise() -> Promise<Void> {
    return Promise(resolved: ())
  }

  var isAuthenticated: Bool {
    get {
      guard config.flags.flags.facebookAuth == true else {
        return false
      }

      guard let token = AccessToken.current else {
        return false
      }

      return !token.isExpired
    }
  }

  var isExpired: Bool {
    get {
      guard config.flags.flags.facebookAuth == true else {
        return true
      }

      guard let token = AccessToken.current else {
        return false
      }

      return token.isExpired
    }
  }

  var authToken: String? {
    get {
      guard config.flags.flags.facebookAuth == true else {
        return nil
      }

      guard let token = AccessToken.current else {
        return nil
      }

      return token.tokenString
    }
  }

  var accountDescription: String? {
    get {
      guard config.flags.flags.facebookAuth == true else {
        return nil
      }

      let ret = [Profile.current?.firstName, Profile.current?.lastName].compactMap { $0 }.joined(separator: " ")

      if ret.isEmpty {
        return nil
      }

      return ret
    }
  }

  var firstName: String? {
    get {
      guard config.flags.flags.facebookAuth == true else {
        return nil
      }

      return Profile.current?.firstName
    }
  }

  var lastName: String? {
    get {
      guard config.flags.flags.facebookAuth == true else {
        return nil
      }

      return Profile.current?.lastName
    }
  }

  func registerBespokeSession(_ session: Any?) {}
}

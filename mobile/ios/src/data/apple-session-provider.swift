import Foundation
import Hydra
import AuthenticationServices

class AppleSessionProvider: SessionProvider {
  fileprivate var token: Data?
  fileprivate var user: String?
  fileprivate var detail: String?
  fileprivate var nameF: String?
  fileprivate var nameL: String?

  func initialise() -> Promise<Void> {
    let keychain = KeychainSwift()
    user = keychain.get("io.meshtech.pubcrawl.apple.user")
    token = keychain.getData("io.meshtech.pubcrawl.apple.identity")
    detail = keychain.get("io.meshtech.pubcrawl.apple.detail")
    nameF = keychain.get("io.meshtech.pubcrawl.apple.firstname")
    nameL = keychain.get("io.meshtech.pubcrawl.apple.lastname")

    guard let u = user else {
      return Promise(resolved: ())
    }

    return Promise(in: .background, token: nil) { (resolve, _, _) in
      let appleIDProvider = ASAuthorizationAppleIDProvider()
      appleIDProvider.getCredentialState(forUserID: u) { (credentialState, _) in
        self.isAuthenticated = credentialState == .authorized
        if !self.isAuthenticated {
          self.isExpired = true
          keychain.delete("io.meshtech.pubcrawl.apple.user")
          keychain.delete("io.meshtech.pubcrawl.apple.identity")
          keychain.delete("io.meshtech.pubcrawl.apple.detail")
          keychain.delete("io.meshtech.pubcrawl.apple.firstname")
          keychain.delete("io.meshtech.pubcrawl.apple.lastname")
        }
        resolve(())
      }
    }
  }

  fileprivate(set) var isAuthenticated = false

  var authToken: String? {
    guard let token = token else {
      return nil
    }

    return String(data: token, encoding: .utf8)
  }
  var isExpired: Bool = false

  var accountDescription: String? {
    get {
      return detail
    }
  }

  var firstName: String? {
    get {
      return nameF
    }
  }

  var lastName: String? {
    get {
      return nameL
    }
  }

  func registerBespokeSession(_ bespokeSession: Any?) {
    guard let session = bespokeSession as? ASAuthorizationAppleIDCredential else {
      return
    }

    guard let token = session.identityToken else {
      return
    }

    self.user = session.user
    self.token = token

    let keychain = KeychainSwift()
    keychain.set(session.user, forKey: "io.meshtech.pubcrawl.apple.user")
    keychain.set(token, forKey: "io.meshtech.pubcrawl.apple.identity")

    if let firstName = session.fullName?.givenName {
      keychain.set(firstName, forKey: "io.meshtech.pubcrawl.apple.firstname")
      nameF = firstName
    } else {
      keychain.set("Anonymous", forKey: "io.meshtech.pubcrawl.apple.firstname")
      nameF = "Anonymous"
    }

    if let lastName = session.fullName?.familyName {
      keychain.set(lastName, forKey: "io.meshtech.pubcrawl.apple.lastname")
      nameL = lastName
    } else {
      nameL = nil
    }

    if let name = session.fullName {
      let currentName = PersonNameComponentsFormatter.localizedString(from: name, style: .default, options: [])
      keychain.set(currentName, forKey: "io.meshtech.pubcrawl.apple.detail")
      detail = currentName
    } else {
      keychain.set("Anonymous", forKey: "io.meshtech.pubcrawl.apple.detail")
      detail = "Anonymous"
    }
    isAuthenticated = true
  }
}

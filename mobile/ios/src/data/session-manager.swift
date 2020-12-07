import Foundation
import Hydra
import AuthenticationServices
import FBSDKLoginKit

private struct SessionV1: Codable {
  var token: String?
  var refreshToken: String?
  var lastVenue: String?
  var cachedVenueLocations: [String:String]?
}

private struct SessionV2: Codable {
  var pushToken: String?
  var hasSkippedOnboarding: Bool?
}

private struct SessionV3: Codable {
  var phoneNumber: String?
}

private struct SessionV4: Codable {
  var authTokenExpiry: Date?
  var refreshTokenExpiry: Date?
}

private struct SessionV5: Codable {
  var source: SessionSource?
}

private struct SessionImpl: Session {
  var version: Semver
  var v1: SessionV1?
  var v2: SessionV2?
  var v3: SessionV3?
  var v4: SessionV4?
  var v5: SessionV5?

  var lastVenue: String? {
    get { return v1?.lastVenue }
    set { v1?.lastVenue = newValue }
  }
  var cachedVenueLocations: [String : String]? {
    get { return v1?.cachedVenueLocations }
    set { v1?.cachedVenueLocations = newValue }
  }
  var authenticationToken: String? {
    get { return v1?.token }
    set { v1?.token = newValue }
  }
  var refreshToken: String? {
    get { return v1?.refreshToken }
    set { v1?.refreshToken = newValue }
  }
  var pushToken: String? {
    get { return v2?.pushToken }
    set { v2?.pushToken = newValue }
  }
  var hasSkippedOnboarding: Bool? {
    get { return v2?.hasSkippedOnboarding }
    set { v2?.hasSkippedOnboarding = newValue }
  }
  var phoneNumber: String? {
    get { return v3?.phoneNumber }
    set { v3?.phoneNumber = newValue }
  }
  var authTokenExpiry: Date? {
    get { return v4?.authTokenExpiry }
    set { v4?.authTokenExpiry = newValue }
  }
  var refreshTokenExpiry: Date? {
    get { return v4?.refreshTokenExpiry }
    set { v4?.refreshTokenExpiry = newValue }
  }
  var source: SessionSource? {
    get { return v5?.source }
    set { v5?.source = newValue }
  }
}

protocol SessionProvider {
  func initialise() -> Promise<Void>
  var isExpired: Bool {get}
  var isAuthenticated: Bool {get}
  var authToken: String? {get}
  var accountDescription: String? {get}
  var firstName: String? {get}
  var lastName: String? {get}
  func registerBespokeSession(_ session: Any?)
}

protocol SessionManager {
  var isAuthenticated: Bool { get }
  var isExpired: Bool { get }
  var authToken: String? { get }
  var accountDescription: String? { get }
  var firstName: String? { get }
  var lastName: String? { get }
  var authType: String? { get }
  func registerBespokeSession(_ bespokeSession: Any?)
  func restore () -> Promise<Session>
  func persist (session: Session) -> Promise<Void>
  func purge () -> Promise<Void>
  var currentSession: Session? { get }
  var currentProfile: UserProfile? { get }
  func updateProfile(_ userProfile: UserProfile)
}

class SessionManagerImpl: SessionManager {
  fileprivate static let LATEST_VERSION = Semver(version: "0.0.5")
  fileprivate(set) var currentSession: Session?
  fileprivate let facebookSessionProvider: SessionProvider
  fileprivate let appleSessionProvider: SessionProvider
  fileprivate(set) var currentProfile: UserProfile?

  init(facebookSessionProvider: SessionProvider, appleSessionProvider: SessionProvider) {
    self.facebookSessionProvider = facebookSessionProvider
    self.appleSessionProvider = appleSessionProvider
  }

  var isAuthenticated: Bool {
    get {
      guard let session = currentSession else {
        return false
      }

      guard let source = session.source else {
        return false
      }

      switch source {
      case .apple:
        return appleSessionProvider.isAuthenticated
      case .facebook:
        return facebookSessionProvider.isAuthenticated
      }
    }
  }

  var isExpired: Bool {
    get {
      guard let session = currentSession else {
        return false
      }

      guard let source = session.source else {
        return false
      }

      switch source {
      case .apple:
        return appleSessionProvider.isExpired
      case .facebook:
        return facebookSessionProvider.isExpired
      }
    }
  }

  var authToken: String? {
    get {
      guard let session = currentSession else {
        return nil
      }

      guard let source = session.source else {
        return nil
      }

      switch source {
      case .apple:
        return appleSessionProvider.authToken
      case .facebook:
        return facebookSessionProvider.authToken
      }
    }
  }

  var authType: String? {
    get {
      guard let session = currentSession else {
        return nil
      }

      guard let source = session.source else {
        return nil
      }

      switch source {
      case .apple:
        return "apple"
      case .facebook:
        return "facebook"
      }
    }
  }

  var accountDescription: String? {
    get {
      guard let session = currentSession else {
        return nil
      }

      guard let source = session.source else {
        return nil
      }

      switch source {
      case .apple:
        return appleSessionProvider.accountDescription
      case .facebook:
        return facebookSessionProvider.accountDescription
      }
    }
  }

  var firstName: String? {
    get {
      guard let session = currentSession else {
        return nil
      }

      guard let source = session.source else {
        return nil
      }

      switch source {
      case .apple:
        return appleSessionProvider.firstName ?? currentProfile?.firstName
      case .facebook:
        return facebookSessionProvider.firstName ?? currentProfile?.firstName
      }
    }
  }

  var lastName: String? {
    get {
      guard let session = currentSession else {
        return nil
      }

      guard let source = session.source else {
        return nil
      }

      switch source {
      case .apple:
        return appleSessionProvider.lastName ?? currentProfile?.lastName
      case .facebook:
        return facebookSessionProvider.lastName ?? currentProfile?.lastName
      }
    }
  }

  func registerBespokeSession(_ bespokeSession: Any?) {
    guard let session = currentSession else {
      return
    }

    guard let source = session.source else {
      return
    }

    switch source {
    case .apple:
      appleSessionProvider.registerBespokeSession(bespokeSession)
    case .facebook:
      facebookSessionProvider.registerBespokeSession(bespokeSession)
    }
  }

  func restore () -> Promise<Session> {
    return async({ _ -> Session in
      try await (self.facebookSessionProvider.initialise())
      try await (self.appleSessionProvider.initialise())

      let decoder = JSONDecoder.defaultDecoder()
      let documentsUrl = try FileManager.default.url(for: .documentDirectory, in: .userDomainMask, appropriateFor: nil, create: true)
      let sessionUrl = documentsUrl.appendingPathComponent("session.json")

      if !FileManager.default.fileExists(atUrl: sessionUrl) {
        let session = SessionImpl(version: SessionManagerImpl.LATEST_VERSION, v1: SessionV1(), v2: SessionV2(), v3: SessionV3(), v4: SessionV4(), v5: SessionV5())
        try await(self.persist(session: session))

        return session
      } else {
        guard let data = try? Data(contentsOf: sessionUrl) else {
          let session = SessionImpl(version: SessionManagerImpl.LATEST_VERSION, v1: SessionV1(), v2: SessionV2(), v3: SessionV3(), v4: SessionV4(), v5: SessionV5())
          try await(self.persist(session: session))

          return session
        }

        guard var session = try? decoder.decode(SessionImpl.self, from: data) else {
          let session = SessionImpl(version: SessionManagerImpl.LATEST_VERSION, v1: SessionV1(), v2: SessionV2(), v3: SessionV3(), v4: SessionV4(), v5: SessionV5())
          try await(self.persist(session: session))

          return session
        }

        if session.v1 == nil {
          session.v1 = SessionV1()
        }

        if session.v2 == nil {
          session.v2 = SessionV2()
        }

        if session.v3 == nil {
          session.v3 = SessionV3()
        }

        if session.v4 == nil {
          session.v4 = SessionV4()
        }

        if session.v5 == nil {
          session.v5 = SessionV5()
        }

        return session
      }
    }).then(in: .main, { [weak self] session -> Session in
      self?.currentSession = session
      return session
    })
  }

  func persist (session: Session) -> Promise<Void> {
    return async({ _ -> Void in
      guard let session = session as? SessionImpl else {
        throw ApplicationError(message: "Provided session was not recognised, you should not subclass the Session protocol.")
      }

      let encoder = JSONEncoder.defaultEncoder()
      let documentsUrl = try FileManager.default.url(for: .documentDirectory, in: .userDomainMask, appropriateFor: nil, create: true)
      let sessionUrl = documentsUrl.appendingPathComponent("session.json")

      try encoder.encode(self.upgrade(sessionData: session)).write(to: sessionUrl)
    }).then(in: .main, { [weak self] in
      self?.currentSession = session
    })
  }

  func purge () -> Promise<Void> {
    currentSession = SessionImpl(version: SessionManagerImpl.LATEST_VERSION, v1: SessionV1(), v2: SessionV2(), v3: SessionV3(), v4: SessionV4(), v5: SessionV5())
    currentProfile = nil
    return async({ _ -> Void in
      let documentsUrl = try FileManager.default.url(for: .documentDirectory, in: .userDomainMask, appropriateFor: nil, create: true)
      let sessionUrl = documentsUrl.appendingPathComponent("session.json")

      guard FileManager.default.fileExists(atUrl: sessionUrl) else { return }
      try FileManager.default.removeItem(at: sessionUrl)
    }).catch { _ in }.then(in: .main, {
      LoginManager().logOut()
    })
  }

  fileprivate func upgrade(sessionData session: SessionImpl) -> SessionImpl {
    // TODO: When moving to a v2, add a method in here to perform data migration.
    return session
  }

  func updateProfile(_ userProfile: UserProfile) {
    currentProfile = userProfile
  }
}

import Foundation
import Hydra
import UIKit
import FBSDKCoreKit

protocol ApplicationLifecycleManager {
  @discardableResult
  func startApplication (application: UIApplication, launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Promise<Void>

  func open(_ url: URL, context: Any) -> Bool

  func registerPushToken(_ token: String)
}

class ApplicationLifecycleManagerImpl: ApplicationLifecycleManager, SandboxManager {
  fileprivate var router: Router!
  fileprivate var sessionManager: SessionManager!
  fileprivate var authManager: AuthManager!
  fileprivate var registry: EventListenerRegistry!
  fileprivate var api: Api!
  fileprivate var loaded = false
  fileprivate var session: Session?
  fileprivate var config: Config!
  var launchOptions: [UIApplication.LaunchOptionsKey: Any]?

  init (router: Router?, sessionManager: SessionManager?, authManager: AuthManager?, registry: EventListenerRegistry?, api: Api?, config: Config?) {
    guard let router = router,
          let sessionManager = sessionManager,
          let authManager = authManager,
          let registry = registry,
          let api = api,
          let config = config else {
      fatalError()
    }

    self.router = router
    self.sessionManager = sessionManager
    self.authManager = authManager
    self.registry = registry
    self.api = api
    self.config = config

    registry <~ self.authManager.on("auth-succeeded") { [weak self] _ in
      guard let pushToken = self?.sessionManager.currentSession?.pushToken else {
        return
      }
      _ = self?.api.registerPushToken(PushRegistration(token: pushToken, platform: 1))
    }
  }

  @discardableResult
  func startApplication (application: UIApplication, launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Promise<Void> {
    return async({ [weak self]_ -> Session in
      guard let sessionManager = self?.sessionManager else { fatalError() }
      return try await(sessionManager.restore())
    }).then({ [weak self] session -> Void in
      self?.session = session
      self?.loaded = true

      if let api = self?.api {
        self?.config.flags = try await(api.fetchCurrentFlags())

        if self?.sessionManager.isAuthenticated == true {
          self?.sessionManager.updateProfile(try await(api.fetchCurrentProfile()))
        }
      }

      let currentSession = self?.session
      let sessionAvailable = self?.sessionManager.isAuthenticated == true
      let sessionExpired = self?.sessionManager.isExpired == true

      var ctx: [String:Any] = [
        "session": currentSession as Any,
        "sessionExpired": sessionExpired
      ]

      if self?.config.flags.flags.facebookAuth == true {
        Profile.enableUpdatesOnAccessTokenChange(true)
        ApplicationDelegate.shared.application(application, didFinishLaunchingWithOptions: launchOptions)
      } else if currentSession?.source == SessionSource.facebook {
        ctx["encounteredDisabledLogin"] = true
      }

      if let url = self?.launchOptions?[.url] as? URL {
        let opened = self?.router?.open(url) ?? false
        if !opened {
          self?.router?.push(url, context: ctx, from: nil, animated: false)
        }
      } else if sessionExpired {
        // Return to onboarding, even if they've skipped onboarding before, once we know they've signed in and their session has expired
        self?.router?.replace("/onboarding", context: ctx, from: nil, animated: false)
      } else if sessionAvailable || currentSession?.hasSkippedOnboarding == true {
        self?.router?.replace("/home", context: ctx, from: nil, animated: false)
      } else {
        self?.router?.replace("/onboarding", context: ctx, from: nil, animated: false)
      }
    }).catch({ [weak self] error in
      guard let s = self else { return }
      print(error)

      // Keep trying to launch the application until it succeeds.
      // We purge the sandbox here in case the issue was fs corruption.
      s.purgeSandbox()
      s.sessionManager.purge().then {
        s.startApplication(application: application, launchOptions: launchOptions)
      }
    })
  }

  func open(_ url: URL, context: Any) -> Bool {
    guard loaded else { return false }

    if router.open(url) {
      return true
    }

    let ctx: [String:Any] = [
      "session": session as Any,
      "urlContext": context
    ]

    if router.replace(url, context: ctx, from: nil, animated: false) != nil {
      return true
    }

    return false
  }

  func registerPushToken(_ token: String) {
    guard let session = sessionManager.currentSession else {
      return
    }

    var newSession = session
    newSession.pushToken = token
    _ = sessionManager.persist(session: newSession).then {}

    if sessionManager.isAuthenticated {
      _ = api.registerPushToken(PushRegistration(token: token, platform: 1)).then {}
    }
  }
}

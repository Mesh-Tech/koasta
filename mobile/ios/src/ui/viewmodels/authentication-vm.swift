import Foundation
import FBSDKLoginKit
import AuthenticationServices
import Hydra

enum AuthenticationViewModelStateIdentifier {
  case initial
  case authenticating
  case transitioningToVenueList
}

struct AuthenticationViewModelState {
  let identifier: AuthenticationViewModelStateIdentifier
  let navigationSource: String?
}

struct AuthenticationViewModelTransition {
  let oldState: AuthenticationViewModelState
  let newState: AuthenticationViewModelState
}

class AuthenticationViewModel: EventEmitter {
  fileprivate let sessionManager: SessionManager
  fileprivate let api: Api
  fileprivate let permissions: PermissionsUtil

  fileprivate var previousState: AuthenticationViewModelState?
  fileprivate var currentState: AuthenticationViewModelState! {
    willSet {
      previousState = currentState
    }

    didSet {
      emit("statechange", sticky: true, data: [
        "previous": previousState,
        "current": currentState
      ])
    }
  }

  init(sessionManager: SessionManager?, api: Api?, permissions: PermissionsUtil?) {
    guard let sessionManager = sessionManager, let api = api, let permissions = permissions else { fatalError() }
    self.sessionManager = sessionManager
    self.api = api
    self.permissions = permissions

    super.init()
    currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: nil)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear(from: String? = nil) {
    currentState = AuthenticationViewModelState(identifier: currentState.identifier, navigationSource: from)
  }

  @objc func facebookTapped() {
    currentState = AuthenticationViewModelState(identifier: .authenticating, navigationSource: currentState.navigationSource)
  }

  func appleTapped() {
    currentState = AuthenticationViewModelState(identifier: .authenticating, navigationSource: currentState.navigationSource)
  }

  func facebookLoggedOut() {
    currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
  }

  func facebookCompleted(result: LoginManagerLoginResult?, error: Error?) {
    guard let result = result else {
      currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
      return
    }

    if result.isCancelled {
      currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
      return
    }

    guard let session = sessionManager.currentSession else {
      currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
      return
    }
    var newSession = session
    newSession.source = .facebook

    Profile.loadCurrentProfile { [weak self] (_, _) in
      self?.sessionManager.persist(session: newSession).then { [weak self] _ in
        if self?.sessionManager.isAuthenticated == true {
          self?.authVerify()
        } else {
          self?.currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: self?.currentState.navigationSource)
        }
      }.catch { [weak self] _ in
        self?.currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: self?.currentState.navigationSource)
      }
    }
  }

  func appleCompleted(result: ASAuthorization?, error: Error?) {
    guard let result = result else {
      currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
      return
    }

    guard let credential = result.credential as? ASAuthorizationAppleIDCredential else {
      currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
      return
    }

    guard let session = sessionManager.currentSession else {
      currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: currentState.navigationSource)
      return
    }
    var newSession = session
    newSession.source = .apple
    sessionManager.persist(session: newSession).then { [weak self] _ in
      self?.sessionManager.registerBespokeSession(credential)
      if self?.sessionManager.isAuthenticated == true {
        self?.authVerify()
      } else {
        self?.currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: self?.currentState.navigationSource)
      }
    }.catch { [weak self] _ in
      self?.currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: self?.currentState.navigationSource)
    }
  }

  fileprivate func authVerify() {
    _ = async({ [weak self] _ in
      guard let sself = self else { return }

      try await(sself.api.login(firstName: sself.sessionManager.firstName, lastName: sself.sessionManager.lastName))
      let profile = try await(sself.api.fetchCurrentProfile())
      sself.sessionManager.updateProfile(profile)

      sself.currentState = AuthenticationViewModelState(identifier: .transitioningToVenueList, navigationSource: sself.currentState.navigationSource)
    }).catch({ [weak self] _ in
      self?.currentState = AuthenticationViewModelState(identifier: .initial, navigationSource: self?.currentState.navigationSource)
    })
  }
}

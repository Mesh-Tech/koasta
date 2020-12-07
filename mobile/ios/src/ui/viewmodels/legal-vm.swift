import Foundation

enum LegalContentType {
  case privacyPolicy
  case termsAndConditions

  static func parseString(_ str: String) -> LegalContentType? {
    switch str {
    case "/privacy-policy":
      return .privacyPolicy
    case "/terms-and-conditions":
      return .termsAndConditions
    default:
      return nil
    }
  }
}

enum LegalViewModelStateIdentifier {
  case initial
  case loaded
  case loadFailed
}

struct LegalViewModelState {
  let identifier: LegalViewModelStateIdentifier
  let content: String?
}

struct LegalViewModelTransition {
  let oldState: LegalViewModelState
  let newState: LegalViewModelState
}

class LegalViewModel: EventEmitter {
  fileprivate let api: Api
  fileprivate var previousState: LegalViewModelState?
  fileprivate var currentState: LegalViewModelState! {
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

  init(api: Api?) {
    guard let api = api else {
      fatalError()
    }
    self.api = api
    super.init()
    currentState = LegalViewModelState(identifier: .initial, content: nil)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear(contentType: LegalContentType) {
    guard currentState.identifier == .initial else {
      return
    }

    switch contentType {
    case .privacyPolicy:
      api.fetchPrivacyPolicy().then { [weak self] content in
        self?.currentState = LegalViewModelState(identifier: .loaded, content: content)
      }.catch { [weak self] _ in
        self?.currentState = LegalViewModelState(identifier: .loadFailed, content: nil)
      }
    case .termsAndConditions:
      api.fetchTermsAndConditions().then { [weak self] content in
        self?.currentState = LegalViewModelState(identifier: .loaded, content: content)
      }.catch { [weak self] _ in
        self?.currentState = LegalViewModelState(identifier: .loadFailed, content: nil)
      }
    }
  }
}

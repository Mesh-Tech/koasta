import Foundation

enum OnboardingViewModelStateIdentifier {
  case initial
  case transitioningToSignIn
  case transitioningToHome
}

struct OnboardingViewModelState {
  let identifier: OnboardingViewModelStateIdentifier
}

struct OnboardingViewModelTransition {
  let oldState: OnboardingViewModelState
  let newState: OnboardingViewModelState
}

class OnboardingViewModel: EventEmitter {
  fileprivate let sessionManager: SessionManager
  fileprivate var previousState: OnboardingViewModelState?
  fileprivate var currentState: OnboardingViewModelState! {
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

  init(sessionManager: SessionManager?) {
    guard let sessionManager = sessionManager else {
      fatalError()
    }

    self.sessionManager = sessionManager

    super.init()
    currentState = OnboardingViewModelState(identifier: .initial)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear () {
    currentState = OnboardingViewModelState(identifier: .initial)
  }

  func startButtonTapped () {
    currentState = OnboardingViewModelState(identifier: .transitioningToSignIn)
  }
}

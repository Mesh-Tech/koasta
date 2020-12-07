import Foundation

enum SettingsViewModelStateIdentifier {
  case initial
  case transitioningToNearby
  case transitioningToOrders
  case transitioningToVenueDetails
}

struct SettingsViewModelState {
  let identifier: SettingsViewModelStateIdentifier
}

struct SettingsViewModelTransition {
  let oldState: SettingsViewModelState
  let newState: SettingsViewModelState
}

class SettingsViewModel: EventEmitter {
  fileprivate var previousState: SettingsViewModelState?
  fileprivate var currentState: SettingsViewModelState! {
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

  func didSelectTab(_ tabIndex: Int) {
    switch tabIndex {
    case 0:
      currentState = SettingsViewModelState(identifier: .transitioningToNearby)
    case 1:
      currentState = SettingsViewModelState(identifier: .transitioningToOrders)
    default:
      break
    }
  }

  override init() {
    super.init()
    currentState = SettingsViewModelState(identifier: .initial)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear () {
    if currentState.identifier == .transitioningToVenueDetails {
      currentState = SettingsViewModelState(identifier: .initial)
    }
  }
}

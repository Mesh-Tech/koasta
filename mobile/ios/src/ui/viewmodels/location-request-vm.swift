import Foundation

enum LocationRequestViewModelStateIdentifier {
  case initial
  case requestingPermission
  case transitioningToVenues
  case permissionRequestFailed
}

struct LocationRequestViewModelState {
  let identifier: LocationRequestViewModelStateIdentifier
}

struct LocationRequestViewModelTransition {
  let oldState: LocationRequestViewModelState
  let newState: LocationRequestViewModelState
}

class LocationRequestViewModel: EventEmitter {
  fileprivate var previousState: LocationRequestViewModelState?
  fileprivate var currentState: LocationRequestViewModelState! {
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

  fileprivate let permissions: PermissionsUtil!

  init(permissions: PermissionsUtil?) {
    guard let permissions = permissions else {
      fatalError()
    }

    self.permissions = permissions

    super.init()
    currentState = LocationRequestViewModelState(identifier: .initial)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  @objc func skipButtonTapped () {
    currentState = LocationRequestViewModelState(identifier: .transitioningToVenues)
  }

  @objc func allowButtonTapped () {
    permissions.requestLocationPermission { [weak self] state in
      guard let strongSelf = self else { return }
      if state == .allowed {
        strongSelf.currentState = LocationRequestViewModelState(identifier: .transitioningToVenues)
      } else {
        strongSelf.currentState = LocationRequestViewModelState(identifier: .permissionRequestFailed)
      }
    }
  }
}

import Foundation
import UIKit
import Hydra

enum VenueOnboardingViewModelStateIdentifier {
  case initial
  case loading
  case loaded
  case error
  case transitioningBack
}

struct VenueOnboardingViewModelState {
  let identifier: VenueOnboardingViewModelStateIdentifier
  let venue: Venue?

  init(identifier: VenueOnboardingViewModelStateIdentifier,
       venue: Venue?) {
    self.identifier = identifier
    self.venue = venue
  }

  init(_ state: VenueOnboardingViewModelState,
       identifier: VenueOnboardingViewModelStateIdentifier? = nil,
       venue: Venue? = nil) {
    self.identifier = identifier ?? state.identifier
    self.venue = venue ?? state.venue
  }
}

struct VenueOnboardingViewModelTransition {
  let oldState: VenueOnboardingViewModelState
  let newState: VenueOnboardingViewModelState
}

class VenueOnboardingViewModel: EventEmitter {
  fileprivate var previousState: VenueOnboardingViewModelState?
  fileprivate var currentState: VenueOnboardingViewModelState! {
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

  fileprivate var api: Api!
  fileprivate var pendingState: VenueOnboardingViewModelState?
  fileprivate var sessionManager: SessionManager!
  fileprivate var registry: EventListenerRegistry!

  init(api: Api?, sessionManager: SessionManager?, registry: EventListenerRegistry?) {
    guard let api = api, let sessionManager = sessionManager, let registry = registry else {
      fatalError()
    }

    self.api = api
    self.sessionManager = sessionManager
    self.registry = registry
    super.init()
    currentState = VenueOnboardingViewModelState(identifier: .initial, venue: nil)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewDidLoad (venueId: String, venue: Venue?) {
    guard currentState.venue == nil else { return }
    currentState = VenueOnboardingViewModelState(currentState, identifier: .loading)

    if let venue = venue {
      currentState = VenueOnboardingViewModelState(currentState, identifier: .loaded, venue: venue)
      return
    }

    async({ [weak self] _ in
      guard let strongSelf = self else { return }

      // First, load the venue from the API
      let loadedVenue = try await(strongSelf.api.getVenue(forId: venueId))

      guard let venue = loadedVenue else {
        strongSelf.currentState = VenueOnboardingViewModelState(strongSelf.currentState, identifier: .error)
        return
      }

      strongSelf.currentState = VenueOnboardingViewModelState(strongSelf.currentState, identifier: .loaded, venue: venue)
    }).catch { [weak self] error in
      guard let strongSelf = self else { return }
      self?.currentState = VenueOnboardingViewModelState(strongSelf.currentState, identifier: .error)
    }
  }

  @objc func backButtonTapped () {
    currentState = VenueOnboardingViewModelState(
      currentState,
      identifier: .transitioningBack
    )
  }
}

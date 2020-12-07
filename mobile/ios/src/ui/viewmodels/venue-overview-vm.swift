import Foundation
import UIKit
import Hydra

enum VenueOverviewViewModelStateIdentifier {
  case initial
  case loading
  case loaded
  case error
  case transitioningBack
  case transitioningToVenueDetail
}

struct VenueOverviewViewModelState {
  let identifier: VenueOverviewViewModelStateIdentifier
  let venue: Venue?
  let menus: [VenueMenu]?
  let defaultTabIdx: Int?
}

struct VenueOverviewViewModelTransition {
  let oldState: VenueOverviewViewModelState
  let newState: VenueOverviewViewModelState
}

class VenueOverviewViewModel: EventEmitter {
  fileprivate var previousState: VenueOverviewViewModelState?
  fileprivate var currentState: VenueOverviewViewModelState! {
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
  fileprivate var pendingState: VenueOverviewViewModelState?
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
    currentState = VenueOverviewViewModelState(identifier: .initial, venue: nil, menus: nil, defaultTabIdx: nil)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewDidLoad (venueId: String) {
    guard currentState.venue == nil else { return }
    currentState = VenueOverviewViewModelState(identifier: .loading, venue: nil, menus: nil, defaultTabIdx: nil)

    async({ [weak self] _ in
      guard let strongSelf = self else { return }

      // First, load the venue from the API
      let loadedVenue = try await(strongSelf.api.getVenue(forId: venueId))

      guard let venue = loadedVenue else {
        strongSelf.currentState = VenueOverviewViewModelState(identifier: .error, venue: nil, menus: nil, defaultTabIdx: nil)
        return
      }

      // Update currentState immediately to allow the UI to begin updating while we load the latest menu
      strongSelf.currentState = VenueOverviewViewModelState(identifier: strongSelf.currentState.identifier, venue: venue, menus: nil, defaultTabIdx: nil)

      // Load the venue's menus from the API
      let loadedMenus = try await(strongSelf.api.getMenuList(forId: String(venue.venueId)))

      guard let menus = loadedMenus else {
        strongSelf.currentState = VenueOverviewViewModelState(identifier: .error, venue: venue, menus: nil, defaultTabIdx: nil)
        return
      }

      // Now that the menu has loaded, chances are the UI is rendering nicely, so we can finish off the state update
      strongSelf.currentState = VenueOverviewViewModelState(identifier: .loaded, venue: venue, menus: menus.map { VenueMenu($0) }, defaultTabIdx: nil)
    }).catch { [weak self] error in
      self?.currentState = VenueOverviewViewModelState(identifier: .error, venue: nil, menus: nil, defaultTabIdx: nil)
    }
  }

  func viewWillAppear () {
    if currentState.identifier == .transitioningToVenueDetail {
      self.currentState = VenueOverviewViewModelState(
        identifier: self.previousState?.identifier ?? .loaded,
        venue: self.currentState.venue,
        menus: self.currentState.menus,
        defaultTabIdx: nil
      )
    }
  }

  @objc func backButtonTapped () {
    currentState = VenueOverviewViewModelState(
      identifier: .transitioningBack,
      venue: currentState.venue,
      menus: currentState.menus,
      defaultTabIdx: nil
    )
  }

  func menuSelected(atPosition pos: Int) {
    currentState = VenueOverviewViewModelState(
      identifier: .transitioningToVenueDetail,
      venue: currentState.venue,
      menus: currentState.menus,
      defaultTabIdx: pos
    )
  }

  @objc func directionsTapped () {
    UIApplication.shared.open(URL(string: "http://maps.apple.com/?daddr=\(currentState.venue?.venuePostCode ?? "")")!, options: [:], completionHandler: nil)
  }
}

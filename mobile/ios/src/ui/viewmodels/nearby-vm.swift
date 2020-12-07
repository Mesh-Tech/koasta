import Foundation
import UIKit
import CoreLocation
import Hydra

enum NearbyViewModelStateIdentifier {
  case initial
  case transitioningToOrders
  case transitioningToSettings
  case transitioningToVenueDetails
  case transitioningToVenueSearch
  case transitioningToLocationSettings
  case transitioningToVenueOnboarding
  case loadingNearbyVenues
  case loadingNearbyVenuesFailed
  case loadedNearbyVenues
}

struct NearbyViewModelState {
  let identifier: NearbyViewModelStateIdentifier
  let locationFetching: Bool
  let locationAvailable: Bool
  let location: CLLocation?
  let venues: [Venue]?
  let selectedVenueId: String?
  let searchText: String?
  let searchState: VenueSearchViewModelState?
  let selectedVenue: Venue?

  init(identifier: NearbyViewModelStateIdentifier,
       locationFetching: Bool,
       locationAvailable: Bool,
       location: CLLocation?,
       venues: [Venue]?,
       selectedVenueId: String?,
       searchText: String?,
       searchState: VenueSearchViewModelState?,
       selectedVenue: Venue?) {
    self.identifier = identifier
    self.locationFetching = locationFetching
    self.locationAvailable = locationAvailable
    self.location = location
    self.venues = venues
    self.selectedVenueId = selectedVenueId
    self.searchText = searchText
    self.searchState = searchState
    self.selectedVenue = selectedVenue
  }

  init(_ state: NearbyViewModelState,
       identifier: NearbyViewModelStateIdentifier? = nil,
       locationFetching: Bool? = nil,
       locationAvailable: Bool? = nil,
       location: CLLocation? = nil,
       venues: [Venue]? = nil,
       selectedVenueId: String? = nil,
       searchText: String? = nil,
       searchState: VenueSearchViewModelState? = nil,
       selectedVenue: Venue? = nil) {
    self.identifier = identifier ?? state.identifier
    self.locationFetching = locationFetching ?? state.locationFetching
    self.locationAvailable = locationAvailable ?? state.locationAvailable
    self.location = location ?? state.location
    self.venues = venues ?? state.venues
    self.selectedVenueId = selectedVenueId ?? state.selectedVenueId
    self.searchText = searchText ?? state.searchText
    self.searchState = searchState ?? state.searchState
    self.selectedVenue = selectedVenue ?? state.selectedVenue
  }
}

struct NearbyViewModelTransition {
  let oldState: NearbyViewModelState
  let newState: NearbyViewModelState
}

class NearbyViewModel: EventEmitter {
  fileprivate var previousState: NearbyViewModelState?
  fileprivate var currentState: NearbyViewModelState! {
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
  fileprivate let permissions: PermissionsUtil
  fileprivate let api: Api
  fileprivate var sessionManager: SessionManager
  fileprivate var locationManagerDelegate: AsyncLocationManagerDelegate?
  fileprivate let globalEmitter: EventEmitter
  fileprivate let registry: EventListenerRegistry

  func didSelectTab(_ tabIndex: Int) {
    switch tabIndex {
    case 0:
      break
    case 1:
      currentState = NearbyViewModelState(currentState, identifier: .transitioningToOrders)
    case 2:
      currentState = NearbyViewModelState(currentState, identifier: .transitioningToSettings)
    default:
      break
    }
  }

  init(permissions: PermissionsUtil, api: Api, sessionManager: SessionManager, locationProvider: LocationManagerProvider, globalEmitter: EventEmitter, registry: EventListenerRegistry) {
    currentState = NearbyViewModelState(identifier: .initial, locationFetching: permissions.locationPermission == .notRequested, locationAvailable: permissions.locationPermission == .allowed, location: nil, venues: nil, selectedVenueId: nil, searchText: nil, searchState: nil, selectedVenue: nil)
    previousState = currentState
    self.permissions = permissions
    self.api = api
    self.sessionManager = sessionManager
    self.globalEmitter = globalEmitter
    self.registry = registry
    locationManagerDelegate = locationProvider.getDelegate()
    super.init()

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear () {
    if currentState.identifier == .transitioningToVenueDetails || currentState.identifier == .transitioningToVenueOnboarding || currentState.identifier == .transitioningToVenueSearch {
      currentState = NearbyViewModelState(currentState, identifier: previousState?.identifier ?? .initial)
    } else if currentState.identifier == .transitioningToOrders || currentState.identifier == .transitioningToSettings {
      currentState = NearbyViewModelState(currentState, identifier: previousState?.identifier ?? .initial)
    } else if currentState.identifier == .transitioningToLocationSettings {
      currentState = NearbyViewModelState(currentState, identifier: .initial)
    }

    if currentState.identifier != .initial {
      return
    }

    loadVenues()
  }

  func didSelectVenue(_ venue: Venue) {
    currentState = NearbyViewModelState(currentState, identifier: .transitioningToVenueDetails, selectedVenueId: String(venue.venueId), selectedVenue: venue)
  }

  func votedForVenue(_ venueId: Int) {
    guard let profile = sessionManager.currentProfile else {
      return
    }

    if profile.votedVenueIds.contains(venueId) {
      let venue = (currentState.venues ?? []).first {
        $0.venueId == venueId
      }
      currentState = NearbyViewModelState(currentState, identifier: .transitioningToVenueOnboarding, selectedVenueId: "\(venueId)", selectedVenue: venue)
    } else {
      let review = VenueReview(summary: nil, detail: nil, rating: nil, registeredInterest: true)
      _ = api.submitReview(forId: String(venueId), review: review).then { _ in }
      sessionManager.updateProfile(profile.withVotedVenueId(venueId))
    }
  }

  fileprivate func loadVenues () {
    currentState = NearbyViewModelState(currentState, identifier: .loadingNearbyVenues, locationFetching: false, locationAvailable: true, location: nil, venues: nil, selectedVenueId: nil, searchText: nil, searchState: nil, selectedVenue: nil)

    async({ [weak self] _ in
      guard let locationManagerDelegate = self?.locationManagerDelegate else { return }
      var location: CLLocation?

      if self?.permissions.locationPermission != .notRequested && self?.permissions.locationPermission != .denied {
        let locations = try await(locationManagerDelegate.requestLocation())
        if let currentLocation = locations.first {
          location = currentLocation
        }
      }

      guard let api = self?.api else { return }

      // Hardcoded to Bond Street, Brighton if no location available
      let actualLocation = location ?? CLLocation(latitude: 50.8236745, longitude: -0.1423743)
      let venues: [Venue]? = try await(api.getNearbyVenues(lat: Double(actualLocation.coordinate.latitude), lon: Double(actualLocation.coordinate.longitude), limit: 20))

      guard let sself = self else {
        return
      }

      guard let allVenues = venues else {
        sself.currentState = NearbyViewModelState(sself.currentState, identifier: .loadingNearbyVenuesFailed, locationFetching: false, locationAvailable: location != nil, location: location, venues: nil)
        return
      }

      sself.currentState = NearbyViewModelState(sself.currentState, identifier: .loadedNearbyVenues, locationFetching: false, locationAvailable: location != nil, location: location, venues: allVenues)
    }).catch { [weak self] _ in
      guard let sself = self else {
        return
      }

      sself.currentState = NearbyViewModelState(sself.currentState, identifier: .loadingNearbyVenuesFailed)
    }.then {

    }
  }

  @objc func allowLocationTapped() {
    if permissions.locationPermission == .denied {
      currentState = NearbyViewModelState(currentState, identifier: .transitioningToLocationSettings)
    } else {
      let previous = currentState
      currentState = NearbyViewModelState(currentState, identifier: .loadingNearbyVenues, locationFetching: true, locationAvailable: false, location: nil, venues: nil, selectedVenueId: nil, searchText: nil, searchState: nil, selectedVenue: nil)
      permissions.requestLocationPermission { result in
        if result == .denied {
          self.currentState = previous
        } else if result == .allowed {
          self.loadVenues()
        }
      }
    }
  }

  @objc func searchForLocationTapped() {
    self.currentState = NearbyViewModelState(currentState, identifier: .transitioningToVenueSearch, locationFetching: currentState.locationFetching, locationAvailable: currentState.locationAvailable, location: currentState.location, venues: currentState.venues, selectedVenueId: currentState.selectedVenueId, searchText: nil, searchState: nil)
  }

  @objc func searchForVenueTapped() {
    self.currentState = NearbyViewModelState(currentState, identifier: .transitioningToVenueSearch, locationFetching: currentState.locationFetching, locationAvailable: currentState.locationAvailable, location: currentState.location, venues: currentState.venues, selectedVenueId: currentState.selectedVenueId, searchText: "")
  }

  func refresh () {
    loadVenues()
  }
}

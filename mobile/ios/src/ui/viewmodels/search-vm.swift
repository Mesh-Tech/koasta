import Foundation
import Hydra

enum VenueSearchViewModelStateIdentifier {
  case initial
  case loaded
  case searching
  case searchResults
  case transitioningBack
  case transitioningToVenue
}

struct VenueSearchViewModelState {
  let identifier: VenueSearchViewModelStateIdentifier
  let searchString: String
  let results: [Venue]?
  let selectedVenue: Venue?
}

struct VenueSearchViewModelTransition {
  let oldState: VenueSearchViewModelState
  let newState: VenueSearchViewModelState
}

class VenueSearchViewModel: EventEmitter {
  fileprivate let api: Api!
  fileprivate let globalEmitter: EventEmitter!
  fileprivate var searchTaskTag: String?
  fileprivate var previousState: VenueSearchViewModelState?
  fileprivate var currentState: VenueSearchViewModelState! {
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

  init(api: Api?, globalEmitter: EventEmitter?) {
    guard let api = api, let globalEmitter = globalEmitter else {
      fatalError()
    }

    self.api = api
    self.globalEmitter = globalEmitter

    super.init()
    currentState = VenueSearchViewModelState(identifier: .initial, searchString: "", results: nil, selectedVenue: nil)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear(_ searchString: String? = "", oldState: VenueSearchViewModelState? = nil) {
    if let state = oldState {
      currentState = state
    } else {
      currentState = VenueSearchViewModelState(identifier: .initial, searchString: searchString ?? "", results: currentState.results, selectedVenue: nil)
    }
  }

  func viewDidAppear() {
    currentState = VenueSearchViewModelState(identifier: .loaded, searchString: currentState.searchString, results: currentState.results, selectedVenue: nil)
  }

  func didSelectVenue(_ venue: Venue) {
    currentState = VenueSearchViewModelState(identifier: .transitioningToVenue, searchString: currentState.searchString, results: currentState.results, selectedVenue: venue)
  }

  @objc func cancelTapped() {
    currentState = VenueSearchViewModelState(identifier: .transitioningBack, searchString: currentState.searchString, results: currentState.results, selectedVenue: nil)
  }

  func searchTextChanged(_ text: String) {
    currentState = VenueSearchViewModelState(identifier: currentState.identifier, searchString: text, results: currentState.results, selectedVenue: nil)
    let newTag = UUID().uuidString
    searchTaskTag = newTag

    DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + 1) {
      if self.searchTaskTag != newTag {
        return
      }

      self.currentState = VenueSearchViewModelState(identifier: .searching, searchString: self.currentState.searchString, results: self.currentState.results, selectedVenue: nil)

      _ = async({ [weak self] _ in
        guard let strongSelf = self else { return }

        var results: [Venue]?

        do {
          results = try await(strongSelf.api.getVenues(query: strongSelf.currentState.searchString))
        } catch {
          strongSelf.currentState = VenueSearchViewModelState(identifier: .searchResults, searchString: strongSelf.currentState.searchString, results: results, selectedVenue: nil)
          return
        }

        strongSelf.currentState = VenueSearchViewModelState(identifier: .searchResults, searchString: strongSelf.currentState.searchString, results: results, selectedVenue: nil)
      }).catch { error in
        print(error)
      }
    }
  }
}

import Foundation
import Hydra

enum VenueViewModelStateIdentifier {
  case initial
  case loading
  case loaded
  case error
  case productSelected
  case productRemoved
  case transitioningBack
  case loadingOrderEstimate
  case transitioningToOrderConfirm
  case transitioningToOrderComplete
}

struct VenueViewModelState {
  let identifier: VenueViewModelStateIdentifier
  let venue: Venue?
  let menus: [VenueMenu]?
  let order: Set<ProductSelection>?
  let selectedTabIdx: Int
  let orderEstimate: EstimateOrderResult?
  let orderNonce: String?

  init(identifier: VenueViewModelStateIdentifier,
       venue: Venue?,
       menus: [VenueMenu]?,
       order: Set<ProductSelection>?,
       selectedTabIdx: Int,
       orderEstimate: EstimateOrderResult?,
       orderNonce: String?) {
    self.identifier = identifier
    self.venue = venue
    self.menus = menus
    self.order = order
    self.selectedTabIdx = selectedTabIdx
    self.orderEstimate = orderEstimate
    self.orderNonce = orderNonce
  }

  init(_ state: VenueViewModelState,
       identifier: VenueViewModelStateIdentifier? = nil,
       venue: Venue? = nil,
       menus: [VenueMenu]? = nil,
       order: Set<ProductSelection>? = nil,
       selectedTabIdx: Int? = nil,
       orderEstimate: EstimateOrderResult? = nil,
       orderNonce: String? = nil) {
    self.identifier = identifier ?? state.identifier
    self.venue = venue ?? state.venue
    self.menus = menus ?? state.menus
    self.order = order ?? state.order
    self.selectedTabIdx = selectedTabIdx ?? state.selectedTabIdx
    self.orderEstimate = orderEstimate ?? state.orderEstimate
    self.orderNonce = orderNonce ?? state.orderNonce
  }
}

struct VenueViewModelTransition {
  let oldState: VenueViewModelState
  let newState: VenueViewModelState
}

class VenueViewModel: EventEmitter {
  fileprivate var previousState: VenueViewModelState?
  fileprivate var currentState: VenueViewModelState! {
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

  fileprivate var pendingState: VenueViewModelState?
  fileprivate var sessionManager: SessionManager!
  fileprivate var registry: EventListenerRegistry!
  fileprivate var api: Api!

  init(sessionManager: SessionManager?, registry: EventListenerRegistry?, globalEmitter: EventEmitter?, api: Api?) {
    guard let sessionManager = sessionManager, let registry = registry, let globalEmitter = globalEmitter, let api = api else {
      fatalError()
    }

    self.sessionManager = sessionManager
    self.registry = registry
    self.api = api
    super.init()
    currentState = VenueViewModelState(identifier: .initial, venue: nil, menus: nil, order: nil, selectedTabIdx: 0, orderEstimate: nil, orderNonce: nil)
    previousState = currentState

    self.registry <~ globalEmitter.on("willdismiss") { [weak self] event in
      guard let sourceVc = event.data["vc"] as? String, sourceVc == "ConfirmPurchaseViewController" else {
        return
      }

      self?.viewWillAppear()
    }

    self.registry <~ globalEmitter.on("orderComplete") { [weak self] _ in
      guard let strongSelf = self else { return }
      strongSelf.currentState = VenueViewModelState(strongSelf.currentState, identifier: .transitioningToOrderComplete)
    }

    self.registry <~ globalEmitter.on("orderAuthFailed") { [weak self] _ in
      guard let strongSelf = self else { return }
      strongSelf.currentState = VenueViewModelState(strongSelf.currentState, identifier: .error)
    }

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewDidLoad (venueId: String, defaultTabIndex: Int, venue: Venue?, menus: [VenueMenu]?) {
    guard currentState.venue == nil else { return }

    if let v = venue, let m = menus {
      currentState = VenueViewModelState(currentState, identifier: .loaded, venue: v, menus: m, selectedTabIdx: defaultTabIndex)
      return
    }

    currentState = VenueViewModelState(currentState, identifier: .loading)

    async({ [weak self] _ in
      guard let strongSelf = self else { return }

      // Load the venue from the API
      let loadedVenue = try await(strongSelf.api.getVenue(forId: venueId))

      guard let venue = loadedVenue else {
        strongSelf.currentState = VenueViewModelState(strongSelf.currentState, identifier: .error)
        return
      }

      // Update currentState immediately to allow the UI to begin updating while we load the latest menu
      strongSelf.currentState = VenueViewModelState(strongSelf.currentState, venue: venue)

      // Load the venue's menus either from the cache, or if we miss then from the API
      let loadedMenus: [Menu]? = try await(strongSelf.api.getMenuList(forId: String(venue.venueId)))

      guard let menus = loadedMenus else {
        strongSelf.currentState = VenueViewModelState(strongSelf.currentState, identifier: .error)
        return
      }

      // Now that the menu has loaded, chances are the UI is rendering nicely, so we can finish off the state update
      strongSelf.currentState = VenueViewModelState(strongSelf.currentState, identifier: .loaded, menus: menus.map { VenueMenu($0) }, selectedTabIdx: defaultTabIndex)
    }).catch { [weak self] error in
      guard let strongSelf = self else { return }
      self?.currentState = VenueViewModelState(strongSelf.currentState, identifier: .error, venue: nil)
    }
  }

  func viewWillAppear () {
    if currentState.identifier == .transitioningToOrderConfirm {
      currentState = VenueViewModelState(currentState, identifier: .loaded)
    } else {
      currentState = VenueViewModelState(currentState, orderNonce: UUID().uuidString)
    }
  }

  @objc func backButtonTapped () {
    currentState = VenueViewModelState(currentState, identifier: .transitioningBack)
  }

  @objc func viewReceiptTapped () {
    currentState = VenueViewModelState(currentState, identifier: .loadingOrderEstimate, orderEstimate: nil)

    async({ [weak self] _ in
      guard let strongSelf = self else { return }

      let draft = DraftOrder(orderLines: (strongSelf.currentState.order ?? []).map {
        OrderLine(venueId: strongSelf.currentState.venue?.venueId ?? 0, productId: $0.productId, quantity: $0.quantity)
      })

      let orderEstimate: EstimateOrderResult? = try await(strongSelf.api.requestOrderEstimate(draft))

      strongSelf.currentState = VenueViewModelState(strongSelf.currentState, identifier: .transitioningToOrderConfirm, orderEstimate: orderEstimate)
    }).catch { [weak self] error in
      guard let strongSelf = self else { return }
      self?.currentState = VenueViewModelState(strongSelf.currentState, identifier: .error, orderEstimate: nil)
    }
  }

  func productSelected (_ product: VenueProduct, section: Int, row: Int) {
    var menus = currentState.menus ?? []
    var menu = menus[section]
    var products = menu.productList
    var product = products[row]

    let selection = product.selection ?? ProductSelection(productId: product.productId, quantity: 0, price: product.price, ageRestricted: product.ageRestricted)
    let newSelection = ProductSelection(productId: selection.productId, quantity: selection.quantity + 1, price: product.price, ageRestricted: product.ageRestricted)

    product.selection = newSelection
    products[row] = product

    menu.productList = products
    menus[section] = menu

    var order = currentState.order ?? Set()
    order.remove(selection)
    order.insert(newSelection)

    // Two-phase transition to loaded via productSelected
    currentState = VenueViewModelState(currentState, identifier: .productSelected, menus: menus, order: order)
    currentState = VenueViewModelState(currentState, identifier: .loaded)
  }

  func productRemoved (_ product: VenueProduct, forMenu menuIdx: Int, product productIdx: Int) {
    var menus = currentState.menus ?? []
    var menu = menus[menuIdx]
    var products = menu.productList
    var product = products[productIdx]
    let selection = product.selection

    product.selection = nil
    products[productIdx] = product

    menu.productList = products
    menus[menuIdx] = menu

    var order = currentState.order ?? Set()
    if let selection = selection {
      order.remove(selection)
    }

    // Two-phase transition to loaded via productRemoved
    currentState = VenueViewModelState(currentState, identifier: .productRemoved, menus: menus, order: order)
    currentState = VenueViewModelState(currentState, identifier: .loaded)
  }

  func tappedMenu (at index: Int) {
    guard index < currentState.menus?.count ?? 0 else {
      return
    }

    currentState = VenueViewModelState(currentState, selectedTabIdx: index)
  }
}

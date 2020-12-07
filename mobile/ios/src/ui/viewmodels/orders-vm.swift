import Foundation
import Hydra

enum OrdersViewModelStateIdentifier {
  case initial
  case loading
  case loaded
  case loadFailed
  case transitioningToNearby
  case transitioningToSettings
  case transitioningToOrderDetail
}

struct OrdersViewModelState {
  let identifier: OrdersViewModelStateIdentifier
  let orders: [HistoricalOrder]?
  let selectedOrder: HistoricalOrder?
}

struct OrdersViewModelTransition {
  let oldState: OrdersViewModelState
  let newState: OrdersViewModelState
}

class OrdersViewModel: EventEmitter {
  fileprivate var previousState: OrdersViewModelState?
  fileprivate var currentState: OrdersViewModelState! {
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
  fileprivate let api: Api
  fileprivate let sessionManager: SessionManager

  func didSelectTab(_ tabIndex: Int) {
    switch tabIndex {
    case 0:
      currentState = OrdersViewModelState(identifier: .transitioningToNearby, orders: currentState.orders, selectedOrder: currentState.selectedOrder)
    case 2:
      currentState = OrdersViewModelState(identifier: .transitioningToSettings, orders: currentState.orders, selectedOrder: currentState.selectedOrder)
    default:
      break
    }
  }

  init(api: Api?, sessionManager: SessionManager?) {
    guard let api = api, let sessionManager = sessionManager else {
      fatalError()
    }

    self.api = api
    self.sessionManager = sessionManager

    super.init()
    currentState = OrdersViewModelState(identifier: .initial, orders: nil, selectedOrder: nil)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear () {
    if currentState.identifier == .transitioningToOrderDetail {
      currentState = OrdersViewModelState(identifier: .loaded, orders: currentState.orders, selectedOrder: nil)
      return
    }

    if currentState.identifier != .initial {
      return
    }

    refresh()
  }

  func orderSelected(_ order: HistoricalOrder) {
    currentState = OrdersViewModelState(identifier: .transitioningToOrderDetail, orders: currentState.orders, selectedOrder: order)
  }

  func refresh () {
    guard sessionManager.isAuthenticated else {
      currentState = OrdersViewModelState(identifier: .loaded, orders: nil, selectedOrder: nil)
      return
    }

    currentState = OrdersViewModelState(identifier: .loading, orders: currentState.orders, selectedOrder: currentState.selectedOrder)

    _ = async({ [weak self] _ in
      guard let strongSelf = self else { return }

      let results: [HistoricalOrder]? = try await(strongSelf.api.getOrders())
      strongSelf.currentState = OrdersViewModelState(identifier: .loaded, orders: results, selectedOrder: self?.currentState.selectedOrder)
    }).catch { [weak self] error in
      if let error = error as? ApiError, error.statusCode == 401 {
        self?.currentState = OrdersViewModelState(identifier: .loaded, orders: self?.currentState.orders ?? [], selectedOrder: self?.currentState.selectedOrder)
        return
      }
      self?.currentState = OrdersViewModelState(identifier: .loadFailed, orders: nil, selectedOrder: self?.currentState.selectedOrder)
    }
  }
}

import Foundation
import Hydra
import UserNotifications

enum OrderDetailViewModelStateIdentifier {
  case initial
  case loading
  case loaded
  case loadFailed
  case transitioningToExternalSettings
}

struct OrderDetailViewModelState {
  let identifier: OrderDetailViewModelStateIdentifier
  let order: HistoricalOrder?
  let notificationsEnabled: Bool
  let notificationsRequested: Bool
}

struct OrderDetailViewModelTransition {
  let oldState: OrderDetailViewModelState
  let newState: OrderDetailViewModelState
}

class OrderDetailViewModel: EventEmitter {
  fileprivate var previousState: OrderDetailViewModelState?
  fileprivate var currentState: OrderDetailViewModelState! {
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
  fileprivate let permissions: PermissionsUtil

  init(api: Api?, permissions: PermissionsUtil?) {
    guard let api = api,
      let permissions = permissions else {
      fatalError()
    }

    self.api = api
    self.permissions = permissions

    super.init()
    let permission = permissions.notificationPermission
    currentState = OrderDetailViewModelState(identifier: .initial, order: nil, notificationsEnabled: permission == .allowed, notificationsRequested: permission != .notRequested)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  func viewWillAppear (orderId: Int? = nil, order: HistoricalOrder? = nil) {
    if currentState.identifier != .initial {
      let currentPermission = permissions.notificationPermission
      currentState = OrderDetailViewModelState(identifier: currentState.identifier == .transitioningToExternalSettings ? .loaded : currentState.identifier, order: currentState.order, notificationsEnabled: currentPermission == .allowed, notificationsRequested: currentPermission != .notRequested)
      return
    }

    if orderId == nil && order == nil {
      currentState = OrderDetailViewModelState(identifier: .loadFailed, order: nil, notificationsEnabled: currentState.notificationsEnabled, notificationsRequested: currentState.notificationsRequested)
      return
    }

    if let order = order {
      let serviceChargeLineItem = HistoricalOrderItem(amount: order.serviceCharge, productName: "Service charge", quantity: 0)
      currentState = OrderDetailViewModelState(identifier: .loaded, order: order.addingLineItem(serviceChargeLineItem), notificationsEnabled: currentState.notificationsEnabled, notificationsRequested: currentState.notificationsRequested)
      return
    }

    currentState = OrderDetailViewModelState(identifier: .loading, order: currentState.order, notificationsEnabled: currentState.notificationsEnabled, notificationsRequested: currentState.notificationsRequested)

    _ = async({ [weak self] _ in
      guard let strongSelf = self else { return }

      var result: HistoricalOrder?

      do {
        result = try await(strongSelf.api.getOrder(orderId: orderId ?? -1))
      } catch {
        strongSelf.currentState = OrderDetailViewModelState(identifier: .loaded, order: nil, notificationsEnabled: strongSelf.currentState.notificationsEnabled, notificationsRequested: strongSelf.currentState.notificationsRequested)
        return
      }

      let serviceChargeLineItem = HistoricalOrderItem(amount: result?.serviceCharge ?? 0, productName: "Service charge", quantity: 0)

      strongSelf.currentState = OrderDetailViewModelState(identifier: .loaded, order: result?.addingLineItem(serviceChargeLineItem), notificationsEnabled: strongSelf.currentState.notificationsEnabled, notificationsRequested: strongSelf.currentState.notificationsRequested)
    }).catch { [weak self] _ in
      guard let strongSelf = self else { return }
      strongSelf.currentState = OrderDetailViewModelState(identifier: .loaded, order: nil, notificationsEnabled: strongSelf.currentState.notificationsEnabled, notificationsRequested: strongSelf.currentState.notificationsRequested)
    }
  }

  func refresh() {
    currentState = OrderDetailViewModelState(identifier: .loading, order: currentState.order, notificationsEnabled: currentState.notificationsEnabled, notificationsRequested: currentState.notificationsRequested)

    _ = async({ [weak self] _ in
      guard let strongSelf = self else { return }

      var result: HistoricalOrder?

      do {
        result = try await(strongSelf.api.getOrder(orderId: strongSelf.currentState.order?.orderId ?? -1))
      } catch {
        strongSelf.currentState = OrderDetailViewModelState(identifier: .loaded, order: nil, notificationsEnabled: strongSelf.currentState.notificationsEnabled, notificationsRequested: strongSelf.currentState.notificationsRequested)
        return
      }

      let serviceChargeLineItem = HistoricalOrderItem(amount: result?.serviceCharge ?? 0, productName: "Service charge", quantity: 0)

      strongSelf.currentState = OrderDetailViewModelState(identifier: .loaded, order: result?.addingLineItem(serviceChargeLineItem), notificationsEnabled: strongSelf.currentState.notificationsEnabled, notificationsRequested: strongSelf.currentState.notificationsRequested)
    }).catch { [weak self] _ in
      guard let strongSelf = self else { return }
      strongSelf.currentState = OrderDetailViewModelState(identifier: .loaded, order: strongSelf.currentState.order, notificationsEnabled: strongSelf.currentState.notificationsEnabled, notificationsRequested: strongSelf.currentState.notificationsRequested)
    }
  }

  func enableNotificationsTapped () {
    let currentPermission = permissions.notificationPermission
    switch currentPermission {
    case .notRequested:
      currentState = OrderDetailViewModelState(identifier: currentState.identifier, order: currentState.order, notificationsEnabled: currentPermission == .allowed, notificationsRequested: currentPermission != .notRequested)
      permissions.requestNotificationsPermission { [weak self] newPermission in
        guard let strongSelf = self else { return }
        strongSelf.currentState = OrderDetailViewModelState(identifier: strongSelf.currentState.identifier, order: strongSelf.currentState.order, notificationsEnabled: newPermission == .allowed, notificationsRequested: newPermission != .notRequested)
      }
    case .denied:
      currentState = OrderDetailViewModelState(identifier: .transitioningToExternalSettings, order: currentState.order, notificationsEnabled: currentPermission == .allowed, notificationsRequested: currentPermission != .notRequested)
    default:
      break
    }
  }
}

import Foundation
import Hydra

enum ConfirmPurchaseViewModelStateIdentifier {
  case initial
  case loaded
  case confirmingOrder
  case confirmingFreeOrder
  case requestingOrder
  case transitioningToOrdered
  case transitioningBack
  case transitioningToOrderFailed
  case transitioningBackFromAuthFailure
}

struct ConfirmPurchaseLineItem {
  let title: String
  let amount: Decimal
  let quantity: Int
  let total: Decimal
}

struct ConfirmPurchaseViewModelState {
  let venueId: Int
  let identifier: ConfirmPurchaseViewModelStateIdentifier
  let order: [ConfirmPurchaseLineItem]?
  let selections: Set<ProductSelection>?
  let paymentAmountInPence: Int
  let venue: Venue?
  let completedOrder: HistoricalOrder?
  let estimate: EstimateOrderResult?
  let nonce: String?
  let notes: String?
  let tableNumber: String?
  let servingType: VenueServingType

  init(venueId: Int,
       identifier: ConfirmPurchaseViewModelStateIdentifier,
       order: [ConfirmPurchaseLineItem]?,
       selections: Set<ProductSelection>?,
       paymentAmountInPence: Int,
       venue: Venue?,
       completedOrder: HistoricalOrder?,
       estimate: EstimateOrderResult?,
       nonce: String?,
       notes: String?,
       tableNumber: String?,
       servingType: VenueServingType) {
    self.identifier = identifier
    self.venueId = venueId
    self.order = order
    self.selections = selections
    self.paymentAmountInPence = paymentAmountInPence
    self.venue = venue
    self.completedOrder = completedOrder
    self.estimate = estimate
    self.nonce = nonce
    self.notes = notes
    self.tableNumber = tableNumber
    self.servingType = servingType
  }

  init(_ state: ConfirmPurchaseViewModelState,
       venueId: Int? = nil,
       identifier: ConfirmPurchaseViewModelStateIdentifier? = nil,
       order: [ConfirmPurchaseLineItem]? = nil,
       selections: Set<ProductSelection>? = nil,
       paymentAmountInPence: Int? = nil,
       venue: Venue? = nil,
       completedOrder: HistoricalOrder? = nil,
       estimate: EstimateOrderResult? = nil,
       nonce: String? = nil,
       notes: String? = nil,
       tableNumber: String? = nil,
       servingType: VenueServingType? = nil) {
    self.identifier = identifier ?? state.identifier
    self.venueId = venueId ?? state.venueId
    self.order = order ?? state.order
    self.selections = selections ?? state.selections
    self.paymentAmountInPence = paymentAmountInPence ?? state.paymentAmountInPence
    self.venue = venue ?? state.venue
    self.completedOrder = completedOrder ?? state.completedOrder
    self.estimate = estimate ?? state.estimate
    self.nonce = nonce ?? state.nonce
    self.notes = notes ?? state.notes
    self.tableNumber = tableNumber ?? state.tableNumber
    self.servingType = servingType ?? state.servingType
  }
}

struct ConfirmPurchaseViewModelTransition {
  let oldState: ConfirmPurchaseViewModelState
  let newState: ConfirmPurchaseViewModelState
}

class ConfirmPurchaseViewModel: EventEmitter {
  fileprivate let sessionManager: SessionManager
  fileprivate let api: Api
  fileprivate let globalEmitter: EventEmitter
  fileprivate let listenerRegistry: EventListenerRegistry

  fileprivate var previousState: ConfirmPurchaseViewModelState?
  fileprivate var currentState: ConfirmPurchaseViewModelState! {
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

  init(sessionManager: SessionManager?, api: Api?, globalEmitter: EventEmitter?, listenerRegistry: EventListenerRegistry?) {
    guard let sessionManager = sessionManager, let api = api, let globalEmitter = globalEmitter, let listenerRegistry = listenerRegistry else { fatalError() }
    self.sessionManager = sessionManager
    self.api = api
    self.globalEmitter = globalEmitter
    self.listenerRegistry = listenerRegistry

    super.init()
    currentState = ConfirmPurchaseViewModelState(venueId: -1, identifier: .initial, order: nil, selections: nil, paymentAmountInPence: 0, venue: nil, completedOrder: nil, estimate: nil, nonce: nil, notes: nil, tableNumber: nil, servingType: .barService)
    previousState = currentState

    emit("statechange", sticky: true, data: [
      "previous": previousState,
      "current": currentState
    ])
  }

  @objc func cancelTapped () {
    currentState = ConfirmPurchaseViewModelState(currentState, identifier: .transitioningBack)
  }

  @objc func confirmTapped () {
    if currentState.paymentAmountInPence == 0 {
      currentState = ConfirmPurchaseViewModelState(currentState, identifier: .confirmingFreeOrder)
      return purchaseReserved(paymentReference: nil, verificationReference: nil)
    }
    currentState = ConfirmPurchaseViewModelState(currentState, identifier: .confirmingOrder)
  }

  func purchaseValidated () {

  }

  func purchaseFailedValidation () {

  }

  func purchaseReserved (paymentReference: String?, verificationReference: String?) {
    let orderLines = (currentState.selections ?? []).map {
      OrderLine(venueId: currentState.venueId, productId: $0.productId, quantity: $0.quantity)
    }
    api.sendOrder(Order(orderLines: orderLines, paymentProcessorReference: paymentReference, paymentVerificationReference: verificationReference, nonce: currentState.nonce ?? "", orderNotes: currentState.notes, servingType: currentState.servingType, table: currentState.tableNumber)).then { [weak self] result in
      guard let strongSelf = self else { return }

      let lineItems = (strongSelf.currentState.order ?? []).filter { $0.quantity > 0 && $0.amount > 0 }.map {
        HistoricalOrderItem(amount: $0.amount, productName: $0.title, quantity: $0.quantity)
      }

      let completedOrder = HistoricalOrder(companyId: 0, externalPaymentId: nil, firstName: nil, lastName: nil, orderId: result.orderId, orderNumber: result.orderNumber, orderStatus: result.status, orderTimeStamp: Date(), userId: nil, venueName: strongSelf.currentState.venue?.venueName ?? "", lineItems: lineItems, total: result.total, serviceCharge: result.serviceCharge, orderNotes: strongSelf.currentState.notes, servingType: strongSelf.currentState.servingType, table: strongSelf.currentState.tableNumber)

      strongSelf.currentState = ConfirmPurchaseViewModelState(strongSelf.currentState, identifier: .transitioningToOrdered, completedOrder: completedOrder)
    }.catch { [weak self] _ in
      guard let strongSelf = self else { return }
      strongSelf.currentState = ConfirmPurchaseViewModelState(strongSelf.currentState, identifier: .transitioningToOrderFailed)
    }
  }

  func purchaseReservationFailed (error: Error) {
    if let error = error as? ApiError, error.statusCode == 401 {
      currentState = ConfirmPurchaseViewModelState(currentState, identifier: .transitioningBackFromAuthFailure)
    } else {
      currentState = ConfirmPurchaseViewModelState(currentState, identifier: .transitioningToOrderFailed)
    }
  }

  func viewDidLoad (context: Any?) {
    guard let order = context as? ConfirmPurchaseContext else { fatalError("Unrecognised context for ConfirmPurchaseViewModel:viewDidLoad") }

    if currentState.venue == nil {
      async({ [weak self] _ in
        guard let strongSelf = self else { return }

        // First, load the venue from the API
        let loadedVenue = try await(strongSelf.api.getVenue(forId: String(order.venueId)))

        guard let venue = loadedVenue else {
          return
        }

        strongSelf.currentState = ConfirmPurchaseViewModelState(strongSelf.currentState, venue: venue, estimate: order.estimate, nonce: order.nonce)
      }).catch { err in
        print(err)
      }.then { [weak self] in
        self?.loadOrder(order)
      }
    } else {
      loadOrder(order)
    }
  }

  func tableNumberUpdated(_ tableNumber: String?) {
    var servingType = currentState.venue?.servingType ?? .barService
    guard servingType != .barService else {
      return
    }

    servingType = (tableNumber ?? "").isEmpty ? .barService : .tableService
    currentState = ConfirmPurchaseViewModelState(currentState, tableNumber: tableNumber, servingType: servingType)
  }

  func orderNotesUpdated(_ orderNotes: String?) {
    currentState = ConfirmPurchaseViewModelState(currentState, notes: orderNotes)
  }

  func viewDidAppear () {

  }

  func orderConfirmationCancelled () {
    currentState = ConfirmPurchaseViewModelState(currentState, identifier: .loaded)
  }

  fileprivate func loadOrder (_ order: ConfirmPurchaseContext) {
    let lineItems: [ConfirmPurchaseLineItem] = order.estimate.receiptLines.map { selection in
      return ConfirmPurchaseLineItem(title: selection.title, amount: selection.amount, quantity: selection.quantity, total: selection.total)
    }

    let paymentAmountInPence = ((order.estimate.receiptTotal * 100) as NSDecimalNumber).intValue

    currentState = ConfirmPurchaseViewModelState(currentState, venueId: order.venueId, identifier: .loaded, order: lineItems, selections: Set(order.order), paymentAmountInPence: paymentAmountInPence)
  }
}

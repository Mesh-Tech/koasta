import Foundation
import UIKit
import Cartography

class ConfirmPurchaseViewController: UIViewController, UITableViewDelegate, UITableViewDataSource, Routable, PaymentDelegate, UITextFieldDelegate {
  fileprivate let tableHeaderUnderlay = UIView()
  fileprivate let venueName = UILabel()
  fileprivate let orderSummary = UILabel()
  fileprivate let explanationLabel = UILabel()
  fileprivate let tableNumberLabel = UILabel()
  fileprivate let tableNumber = UITextField()
  fileprivate let orderNotesLabel = UILabel()
  fileprivate let orderNotes = UITextField()
  fileprivate let formStack = UIStackView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .plain)
  fileprivate let confirmButton = BigButton(style: .solidRed)
  fileprivate var loaded = false

  var routerContext: Route?
  fileprivate var router: Router!
  fileprivate var globalEmitter: EventEmitter!
  fileprivate var viewModel: ConfirmPurchaseViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var orderItems: [ConfirmPurchaseLineItem] = Array()
  fileprivate var estimate: EstimateOrderResult?
  fileprivate var billingManager: BillingManager!
  fileprivate var isRequestingOrder = false
  fileprivate var isRequestingPayment = false

  fileprivate let formatter: NumberFormatter = {
    let formatter = NumberFormatter()
    formatter.alwaysShowsDecimalSeparator = true
    formatter.currencyCode = "GBP"
    formatter.currencySymbol = "£"
    formatter.decimalSeparator = "."
    formatter.currencyGroupingSeparator = ","
    formatter.generatesDecimalNumbers = true
    formatter.groupingSize = 3
    formatter.numberStyle = .currency

    return formatter
  }()
  fileprivate let plainFormatter: NumberFormatter = {
    let formatter = NumberFormatter()
    formatter.alwaysShowsDecimalSeparator = false
    formatter.numberStyle = .none

    return formatter
  }()

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(router: Router?,
       globalEmitter: EventEmitter?,
       viewModel: ConfirmPurchaseViewModel?,
       listenerRegistry: EventListenerRegistry?,
       billingManager: BillingManager?) {
    guard let router = router,
          let globalEmitter = globalEmitter,
          let viewModel = viewModel,
          let listenerRegistry = listenerRegistry,
          let billingManager = billingManager else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.globalEmitter = globalEmitter
    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.billingManager = billingManager
    modalPresentationStyle = .overCurrentContext
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.grey7
    view.isOpaque = true

    venueName.font = UIFont.boldBrandFont(ofSize: 19)
    venueName.numberOfLines = 1
    venueName.lineBreakMode = .byTruncatingTail
    venueName.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    venueName.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    venueName.textColor = UIColor.foregroundColour
    venueName.accessibilityIdentifier = "confirmPurchaseTitle"

    orderSummary.font = UIFont.brandFont(ofSize: 16)
    orderSummary.numberOfLines = 1
    orderSummary.lineBreakMode = .byTruncatingTail
    orderSummary.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    orderSummary.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    orderSummary.textColor = UIColor.grey14

    explanationLabel.numberOfLines = 0
    explanationLabel.lineBreakMode = .byWordWrapping
    explanationLabel.font = UIFont.brandFont(ofSize: 14, weight: .medium)
    explanationLabel.textColor = UIColor.grey5
    explanationLabel.accessibilityIdentifier = "orderCustomisationExplanationLabel"

    tableNumber.textColor = UIColor.foregroundColour
    tableNumber.font = UIFont.brandFont(ofSize: 16, weight: .regular)
    tableNumber.accessibilityIdentifier = "orderCustomisationTableNumber"
    tableNumber.placeholder = NSLocalizedString("For table service", comment: "")
    tableNumber.textAlignment = .right
    tableNumber.isHidden = true
    tableNumber.returnKeyType = .done
    tableNumber.keyboardType = .asciiCapable
    tableNumber.autocapitalizationType = .allCharacters
    tableNumber.autocorrectionType = .no
    tableNumber.delegate = self

    tableNumberLabel.numberOfLines = 0
    tableNumberLabel.lineBreakMode = .byWordWrapping
    tableNumberLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    tableNumberLabel.textColor = UIColor.grey5
    tableNumberLabel.accessibilityIdentifier = "orderCustomisationTableNumberLabel"
    tableNumberLabel.text = NSLocalizedString("Table number", comment: "")

    orderNotes.textColor = UIColor.foregroundColour
    orderNotes.font = UIFont.brandFont(ofSize: 16, weight: .regular)
    orderNotes.accessibilityIdentifier = "orderCustomisationTableNumber"
    orderNotes.placeholder = NSLocalizedString("Customise your order", comment: "")
    orderNotes.textAlignment = .right
    orderNotes.returnKeyType = .done
    orderNotes.autocapitalizationType = .sentences
    orderNotes.delegate = self

    orderNotesLabel.numberOfLines = 0
    orderNotesLabel.lineBreakMode = .byWordWrapping
    orderNotesLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    orderNotesLabel.textColor = UIColor.grey5
    orderNotesLabel.accessibilityIdentifier = "orderCustomisationTableNumberLabel"
    orderNotesLabel.text = NSLocalizedString("Order notes", comment: "")

    tableHeaderUnderlay.backgroundColor = UIColor.backgroundColour

    tableView.backgroundColor = UIColor.clear
    tableView.isOpaque = false
    tableView.layer.cornerRadius = 4
    tableView.delegate = self
    tableView.dataSource = self
    tableView.allowsSelection = false
    tableView.layoutMargins = UIEdgeInsets.zero
    tableView.separatorInset = UIEdgeInsets(top: 0, left: 60, bottom: 0, right: 0)
    tableView.tableFooterView = UIView(frame: CGRect(x: 0, y: 0, width: view.bounds.width, height: 0.01))

    tableView.register(ReceiptLineItem.self, forCellReuseIdentifier: "receipt-line-item")
    tableView.register(ReceiptFooterView.self, forCellReuseIdentifier: "receipt-footer-view")

    confirmButton.titleLabel.text = NSLocalizedString("Place Order", comment: "Screen: [Confirm Purchase] Element: [Place order button] Scenario: [Confirming purchase]")
    confirmButton.accessibilityIdentifier = "confirmButton"

    formStack.distribution = .equalSpacing
    formStack.axis = .vertical

    let rowA = UIStackView()
    rowA.distribution = .fillProportionally
    rowA.axis = .horizontal
    rowA.addArrangedSubview(tableNumberLabel)
    rowA.addArrangedSubview(tableNumber)

    formStack.addArrangedSubview(rowA)

    let rowB = UIStackView()
    rowB.distribution = .fillProportionally
    rowB.axis = .horizontal
    rowB.addArrangedSubview(orderNotesLabel)
    rowB.addArrangedSubview(orderNotes)

    formStack.addArrangedSubview(rowB)
    formStack.spacing = 10

    view.addSubview(tableHeaderUnderlay)
    view.addSubview(venueName)
    view.addSubview(orderSummary)
    [explanationLabel, formStack].forEach {
      self.view.addSubview($0)
    }
    view.addSubview(tableView)
    view.addSubview(confirmButton)
    confirmButton.addTarget(viewModel, action: #selector(ConfirmPurchaseViewModel.confirmTapped), for: .touchUpInside)

    constrain(view, venueName, orderSummary, tableNumberLabel, tableNumber, orderNotesLabel, orderNotes, explanationLabel, formStack) { container, venueName, orderSummary, tableNumberLabel, _, orderNotesLabel, _, explanationLabel, formStack in
      venueName.top == container.safeAreaLayoutGuide.top + 20
      venueName.left == container.left + 16
      venueName.right == container.right - 16
      orderSummary.top == venueName.bottom + 5
      orderSummary.left == container.left + 16
      orderSummary.right == container.right - 16

      explanationLabel.left == orderSummary.left
      explanationLabel.right == orderSummary.right
      explanationLabel.top == orderSummary.bottom + 10

      tableNumberLabel.width == container.width * 0.35
      orderNotesLabel.width == tableNumberLabel.width

      formStack.top == explanationLabel.bottom + 10
      formStack.left == explanationLabel.left
      formStack.right == explanationLabel.right
    }

    constrain(view, formStack, tableHeaderUnderlay, tableView, confirmButton) { container, formStack, tableHeaderUnderlay, tableView, confirmButton in
      tableHeaderUnderlay.top == container.top
      tableHeaderUnderlay.bottom == tableView.top
      tableHeaderUnderlay.left == container.left
      tableHeaderUnderlay.right == container.right

      tableView.top == formStack.bottom + 10
      tableView.left == container.left
      tableView.right == container.right
      tableView.bottom == confirmButton.top - 10

      confirmButton.bottom == container.safeAreaLayoutGuide.bottom - 10
      confirmButton.right == container.right - 16
      confirmButton.left == container.left + 16
      confirmButton.height == 44
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()
    navigationItem.title = NSLocalizedString("Your Order", comment: "")

    viewModel.viewDidLoad(context: routerContext?.navigationContext)

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? ConfirmPurchaseViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? ConfirmPurchaseViewModelState, to: current)
    }
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.titleTextAttributes = [NSAttributedString.Key.foregroundColor: UIColor.brandColour, NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)]
      self.navigationController?.navigationBar.tintColor = UIColor.brandColour
    }
    navigationController?.setNavigationBarHidden(false, animated: true)
  }

  override func viewDidAppear(_ animated: Bool) {
    super.viewDidAppear(animated)
    viewModel.viewDidAppear()
  }

  fileprivate func handleTransition(from: ConfirmPurchaseViewModelState?, to: ConfirmPurchaseViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if from?.identifier == .initial && to.identifier == .loaded {
      loaded = true
      venueName.text = to.venue?.venueName
      orderItems = Array(to.order ?? Array())
      estimate = to.estimate

      let quantityVal = orderItems.reduce(0) { cur, next in
        return cur + next.quantity
      }

      let priceVal = estimate?.receiptTotal ?? 0

      let quantityDescription = plainFormatter.string(from: quantityVal as NSNumber) ?? ""
      var priceDescription = ""
      if priceVal > Decimal(0) {
        priceDescription = formatter.string(from: priceVal as NSDecimalNumber) ?? ""
      } else {
        priceDescription = NSLocalizedString("Free", comment: "")
      }

      orderSummary.text = "\(quantityDescription) \(quantityVal == 1 ? "item" : "items") · \(priceDescription)"
      orderSummary.setNeedsLayout()

      if to.venue?.servingType == .tableService {
        explanationLabel.text = NSLocalizedString("This venue is exclusively table service. Please enter a table number to continue.\nIf you wish to customise your order, enter your request in the order notes.", comment: "")
        tableNumber.isHidden = false
        tableNumberLabel.isHidden = false
      } else if to.venue?.servingType == .barAndTableService {
        explanationLabel.text = NSLocalizedString("This venue offers bar service and table service. Please enter a table number if you want table service.\nIf you wish to customise your order, enter your request in the order notes.", comment: "")
        tableNumber.isHidden = false
        tableNumberLabel.isHidden = false
      } else if to.venue?.servingType == .barService {
         explanationLabel.text = NSLocalizedString("This venue is exclusively bar service.\nIf you wish to customise your order, enter your request in the order notes.", comment: "")
         tableNumber.isHidden = true
         tableNumberLabel.isHidden = true
      }

      let tableService = to.venue?.servingType == .tableService
      let tableNumberEmpty = (to.tableNumber ?? "").isEmpty

      confirmButton.isEnabled = !tableService || !tableNumberEmpty
      tableView.reloadData()
      view.layoutIfNeeded()
    } else if from?.identifier == .loaded && to.identifier == .loaded {
      let tableService = to.venue?.servingType == .tableService
      let tableNumberEmpty = (to.tableNumber ?? "").isEmpty

      confirmButton.isEnabled = !tableService || !tableNumberEmpty
    } else if to.identifier == .transitioningBack {
      self.view.layoutIfNeeded()
      self.globalEmitter.emit("willdismiss", data: ["vc": "ConfirmPurchaseViewController"])
      navigationController?.popViewController(animated: true)
      self.globalEmitter.emit("diddismiss", data: ["vc": "ConfirmPurchaseViewController"])
    } else if to.identifier == .transitioningToOrderFailed {
      confirmButton.isEnabled = true
      UIView.animate(withDuration: 0.2) {
        self.confirmButton.alpha = 1
      }
      billingManager.completePayment(status: .error) {
        let alert = UIAlertController(title: "Order failed", message: "We were unable to process this order. Please try again later.", preferredStyle: .alert)
        alert.addAction(UIAlertAction(title: "OK", style: .default, handler: nil))
        self.present(alert, animated: true)
      }
    } else if to.identifier == .transitioningToOrdered {
      billingManager.completePayment(status: .success) {
        self.router.replaceAllButOne("/active-order", context: [ "order": to.completedOrder ])
        self.globalEmitter.emit("orderComplete")
      }
    } else if to.identifier == .confirmingFreeOrder {
      confirmButton.isEnabled = false
      UIView.animate(withDuration: 0.2, animations: {
        self.confirmButton.alpha = 0
      })
      isRequestingPayment = false
      isRequestingOrder = true
    } else if to.identifier == .confirmingOrder {
      guard let venue = to.venue else {
        return
      }

      confirmButton.isEnabled = false
      UIView.animate(withDuration: 0.2, animations: {
        self.confirmButton.alpha = 0
      })
      isRequestingPayment = false
      isRequestingOrder = true

      if self.billingManager.supportsNativePayments {
        let sheet = UIAlertController(title: NSLocalizedString("Choose a payment method", comment: ""), message: nil, preferredStyle: .actionSheet)
        sheet.addAction(UIAlertAction(title: NSLocalizedString("Apple Pay", comment: ""), style: .default) { _ in
          self.billingManager.presentNativePaymentViewController(to: self, forVenue: venue, withTotalAmountInPence: to.paymentAmountInPence)
        })
        sheet.addAction(UIAlertAction(title: NSLocalizedString("Credit / debit card", comment: ""), style: .default) { _ in
          self.billingManager.presentPaymentViewController(to: self, forVenue: venue, withTotalAmountInPence: to.paymentAmountInPence)
        })
        sheet.addAction(UIAlertAction(title: NSLocalizedString("Cancel", comment: ""), style: .cancel) { _ in
          self.confirmButton.isEnabled = true
          UIView.animate(withDuration: 0.2, animations: {
            self.confirmButton.alpha = 1
          })
          self.isRequestingPayment = false
          self.isRequestingOrder = false
        })

        present(sheet, animated: true, completion: nil)
      } else {
        self.billingManager.presentPaymentViewController(to: self, forVenue: venue, withTotalAmountInPence: to.paymentAmountInPence)
      }
    } else if to.identifier == .transitioningBackFromAuthFailure {
      navigationController?.popViewController(animated: true)
      self.globalEmitter.emit("orderAuthFailed")
    } else if (from?.identifier == .requestingOrder || from?.identifier == .confirmingOrder) && to.identifier == .loaded {
      UIView.animate(withDuration: 0.2, animations: {
        self.confirmButton.alpha = 1
      }) { _ in
        self.confirmButton.isEnabled = true
      }
    }
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    guard loaded else {
      return 0
    }
    return 3
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    guard loaded else {
      return 0
    }
    if section == 0 {
      return 0
    }

    if section == 2 {
      return 1
    }

    return orderItems.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    if indexPath.section == 2 {
      let cell = tableView.dequeueReusableCell(withIdentifier: "receipt-footer-view", for: indexPath) as! ReceiptFooterView
      return cell.configure(with: estimate!)
    }

    let orderItem = orderItems[indexPath.row]
    let cell = tableView.dequeueReusableCell(withIdentifier: "receipt-line-item", for: indexPath) as! ReceiptLineItem

    return cell.configure(with: orderItem)
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    if indexPath.section == 2 {
      guard let estimate = estimate else {
        return 0
      }
      return ReceiptFooterView.calculateHeight(for: estimate)
    }

    return ReceiptLineItem.calculateHeight(for: orderItems[indexPath.row])
  }

  func paymentDelegateDidFailToInitialise(_ error: Error) {
    viewModel.purchaseReservationFailed(error: error)
  }

  func paymentDelegateDidCompleteWithStatus(_ status: PaymentStatus, withToken token: String?, withVerificationToken verificationToken: String?) {
    guard let token = token, status == .success else {
      viewModel.purchaseReservationFailed(error: ApiError(statusCode: 500, body: nil, bodyData: nil))
      return
    }

    viewModel.purchaseReserved(paymentReference: token, verificationReference: verificationToken)
  }

  func textField(_ textField: UITextField, shouldChangeCharactersIn range: NSRange, replacementString string: String) -> Bool {
    if let text = textField.text as NSString? {
      let newValue = text.replacingCharacters(in: range, with: string)

      if textField == tableNumber {
        viewModel.tableNumberUpdated(newValue)
      } else if textField == orderNotes {
        viewModel.orderNotesUpdated(newValue)
      }
    } else {
      if textField == tableNumber {
        viewModel.tableNumberUpdated(nil)
      } else if textField == orderNotes {
        viewModel.orderNotesUpdated(nil)
      }
    }
    return true
  }
}

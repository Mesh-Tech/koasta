import UIKit
import Cartography

private enum Section {
  case status
  case notification
  case receipt
}

class OrderDetailViewController: UIViewController, Routable, UITableViewDelegate, UITableViewDataSource, OrderNotificationCardItemViewDelegate {
  fileprivate var router: Router!
  fileprivate var viewModel: OrderDetailViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var globalEmitter: EventEmitter!
  fileprivate var permissions: PermissionsUtil!
  fileprivate var order: HistoricalOrder?
  fileprivate var dateFormatter: DateFormatter!
  fileprivate var detailDateFormatter: DateFormatter!
  fileprivate var hasAppearedOnce = false
  fileprivate let topInsetEnlightener = UIView()
  fileprivate let tableViewNavOverlay = UIView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .grouped)
  fileprivate var sections: [Section] = []
  fileprivate var notificationsPreviouslyDenied = false

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  var routerContext: Route?

  init(router: Router?,
       viewModel: OrderDetailViewModel?,
       listenerRegistry: EventListenerRegistry?,
       globalEmitter: EventEmitter?,
       permissions: PermissionsUtil?) {
    guard let router = router,
      let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let globalEmitter = globalEmitter,
      let permissions = permissions else { fatalError() }

    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.globalEmitter = globalEmitter
    self.permissions = permissions
    modalPresentationStyle = .overCurrentContext
    dateFormatter = DateFormatter()
    dateFormatter.locale = Locale(identifier: "en_US_POSIX")
    dateFormatter.dateFormat = "h:mm a"
    dateFormatter.amSymbol = "AM"
    dateFormatter.pmSymbol = "PM"

    detailDateFormatter = DateFormatter()
    detailDateFormatter.locale = Locale(identifier: "en_US_POSIX")
    detailDateFormatter.dateFormat = "dd/MM/yyyy '·' h:mm a"
    detailDateFormatter.amSymbol = "AM"
    detailDateFormatter.pmSymbol = "PM"
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.grey7
    tableViewNavOverlay.backgroundColor = UIColor.backgroundColour

    view.addSubview(topInsetEnlightener)
    view.addSubview(tableView)
    view.addSubview(tableViewNavOverlay)

    tableView.backgroundColor = UIColor.clear
    tableView.delegate = self
    tableView.dataSource = self
    tableView.register(OrderStatusCardItemView.self, forCellReuseIdentifier: "order-status-card")
    tableView.register(OrderNotificationCardItemView.self, forCellReuseIdentifier: "order-notification-card")
    tableView.register(OrderSummaryItem.self, forCellReuseIdentifier: "order-summary")
    tableView.tableHeaderView = UIView(frame: CGRect(x: 0, y: 0, width: 320, height: 0.01))
    tableView.sectionHeaderHeight = 6.0
    tableView.sectionFooterHeight = 6.0
    tableView.accessibilityIdentifier = "orderSummaryList"

    constrain(view, topInsetEnlightener, tableViewNavOverlay, tableView) { container, topInsetEnlightener, tableViewNavOverlay, tableView in
      topInsetEnlightener.top == container.top
      topInsetEnlightener.bottom == container.safeAreaLayoutGuide.top
      topInsetEnlightener.left == container.left
      topInsetEnlightener.right == container.right
      tableViewNavOverlay.left == container.left
      tableViewNavOverlay.top == container.top
      tableViewNavOverlay.right == container.right
      tableViewNavOverlay.height == topInsetEnlightener.height + 1
      tableView.top == topInsetEnlightener.bottom
      tableView.leading == container.leading
      tableView.trailing == container.trailing
      tableView.bottom == container.bottom
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? OrderDetailViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? OrderDetailViewModelState, to: current)
    }

    listenerRegistry <~ globalEmitter.on("orderUpdated") { [weak self] _ in
      self?.viewModel.refresh()
    }

    NotificationCenter.default.addObserver(self, selector: #selector(appWillAppear), name: UIApplication.didBecomeActiveNotification, object: nil)

    NotificationCenter.default.addObserver(self, selector: #selector(appWillDissappear), name: UIApplication.willResignActiveNotification, object: nil)
  }

  @objc fileprivate func appWillDissappear(notification: Notification) {
    if permissions.notificationPermission == .denied {
      notificationsPreviouslyDenied = true
    }
  }

  @objc fileprivate func appWillAppear(notification: Notification) {
    if permissions.notificationPermission == .allowed && notificationsPreviouslyDenied {
      UIApplication.shared.registerForRemoteNotifications()
    }

    notificationsPreviouslyDenied = false
    viewModel.viewWillAppear()
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)

    if !hasAppearedOnce {
      hasAppearedOnce = true
      if let ctx = routerContext?.navigationContext as? [String:Any], let order = ctx["order"] as? HistoricalOrder {
        viewModel.viewWillAppear(orderId: nil, order: order)
      } else if let data = routerContext?.values, let orderId = data["orderId"] as? String {
        viewModel.viewWillAppear(orderId: Int(orderId), order: nil)
      } else {
        navigationController?.popToRootViewController(animated: false)
        return
      }

      UIView.animate(withDuration: 0.2) {
        self.navigationController?.navigationBar.titleTextAttributes = [NSAttributedString.Key.foregroundColor: UIColor.brandColour, NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)]
        self.navigationController?.navigationBar.tintColor = UIColor.brandColour
        self.setNeedsStatusBarAppearanceUpdate()
      }
      navigationController?.setNavigationBarHidden(false, animated: true)
      return
    }

    viewModel.viewWillAppear()
  }

  fileprivate func handleTransition(from: OrderDetailViewModelState?, to: OrderDetailViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if to.identifier == .loaded {
      let formattedDate = dateFormatter.string(from: to.order?.orderTimeStamp ?? Date())
      navigationItem.title = NSLocalizedString("Order · \(formattedDate)", comment: "")
      order = to.order

      let oldSections = sections

      if to.notificationsRequested && to.notificationsEnabled {
        sections = [
          .status,
          .receipt
        ]
      } else {
        sections = [
          .status,
          .notification,
          .receipt
        ]
      }

      if sections.count == 2 && oldSections.count == 3 {
        tableView.beginUpdates()
        tableView.deleteSections(IndexSet(integer: 2), with: .top)
        tableView.reloadSections(IndexSet(integer: 1), with: .top)
        tableView.endUpdates()
      } else if sections.count == 3 && oldSections.count == 2 {
        tableView.beginUpdates()
        tableView.insertSections(IndexSet(integer: 2), with: .top)
        tableView.endUpdates()
      } else {
        tableView.reloadData()
      }

      if from?.identifier == .transitioningToExternalSettings {
        UIApplication.shared.registerForRemoteNotifications()
      }
    } else if to.identifier == .transitioningToExternalSettings {
      let alert = UIAlertController(title: NSLocalizedString("Enabling notifications", comment: ""), message: NSLocalizedString("You've previously denied notification permissions for Koasta. To enable notifications, please do so in the Settings app", comment: ""), preferredStyle: .alert)
      alert.addAction(UIAlertAction(title: NSLocalizedString("Done", comment: ""), style: .cancel) { [weak self] _ in
        self?.viewModel.viewWillAppear()
      })
      present(alert, animated: true)
    }
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    return order == nil ? 0 : 1
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return sections.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    guard let order = order else {
      fatalError()
    }

    let section = sections[indexPath.section]
    switch section {
    case .status:
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "order-status-card", for: indexPath) as? OrderStatusCardItemView else {
        fatalError()
      }

      let status = OrderStatus(rawValue: order.orderStatus) ?? .unknown

      return cell.configure(status: status, servingType: order.servingType)
    case .notification:
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "order-notification-card", for: indexPath) as? OrderNotificationCardItemView else {
        fatalError()
      }

      cell.delegate = self
      return cell.configure()
    case .receipt:
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "order-summary", for: indexPath) as? OrderSummaryItem else {
        fatalError()
      }

      return cell.configure(order: order)
    }
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    guard let order = order else {
      return 0
    }

    let section = sections[indexPath.section]
    switch section {
    case .status:
      return OrderStatusCardItemView.calculateHeight(status: OrderStatus(rawValue: order.orderStatus) ?? .unknown, servingType: order.servingType)
    case .notification:
      return OrderNotificationCardItemView.calculateHeight()
    case .receipt:
      return OrderSummaryItem.calculateHeight(order: order)
    }
  }

  func tableView(_ tableView: UITableView, estimatedHeightForRowAt indexPath: IndexPath) -> CGFloat {
    guard let order = order else {
      return 0
    }

    let section = sections[indexPath.section]
    switch section {
    case .status:
      return OrderStatusCardItemView.calculateHeight(status: OrderStatus(rawValue: order.orderStatus) ?? .unknown, servingType: order.servingType)
    case .notification:
      return OrderNotificationCardItemView.calculateHeight()
    case .receipt:
      return OrderSummaryItem.calculateHeight(order: order)
    }
  }

  func notificationCardItemDidRequestNotifications(_ item: OrderNotificationCardItemView) {
    viewModel.enableNotificationsTapped()
  }
}

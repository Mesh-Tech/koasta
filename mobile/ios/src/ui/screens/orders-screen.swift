import UIKit
import Cartography

class OrdersViewController: UIViewController, Routable, UITabBarDelegate, UITableViewDelegate, UITableViewDataSource {
  fileprivate var router: Router!
  fileprivate var globalEmitter: EventEmitter!
  fileprivate var viewModel: OrdersViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var toaster: Toaster!

  fileprivate let tabBar = UITabBar()
  fileprivate let topInsetEnlightener = UIView()
  fileprivate let bottomInsetEnlightener = UIView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .grouped)
  fileprivate let refreshControl = UIRefreshControl()
  fileprivate let emptyBackground = UIView()
  fileprivate let emptyTitle = UILabel()
  fileprivate let emptyBody = UILabel()
  fileprivate let middleAlignerView = UIView()

  fileprivate var orders: [String: [HistoricalOrder]] = Dictionary()
  fileprivate var sections: [String] = []
  fileprivate var loadingOrders = false

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  var routerContext: Route?

  init(router: Router?,
       globalEmitter: EventEmitter?,
       viewModel: OrdersViewModel?,
       listenerRegistry: EventListenerRegistry?,
       toaster: Toaster?) {
    guard let router = router,
      let globalEmitter = globalEmitter,
      let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let toaster = toaster else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.globalEmitter = globalEmitter
    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.toaster = toaster
    modalPresentationStyle = .overCurrentContext
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.grey7

    tabBar.backgroundColor = UIColor.backgroundColour
    tabBar.isOpaque = true
    tabBar.setItems([
      UITabBarItem(title: NSLocalizedString("Home", comment: "Tab bar item - Home"), image: UIImage(named: "icon-tabbar-home"), selectedImage: UIImage(named: "icon-tabbar-home")),
      UITabBarItem(title: NSLocalizedString("Orders", comment: "Tab bar item - Orders"), image: UIImage(systemName: "cube.box"), selectedImage: nil),
      UITabBarItem(title: NSLocalizedString("Settings", comment: "Tab bar item - Settings"), image: UIImage(systemName: "gear"), selectedImage: nil)
    ], animated: false)

    tabBar.tintColor = UIColor.brandColour
    tabBar.items?[0].tag = 0
    tabBar.items?[1].tag = 1
    tabBar.items?[2].tag = 2
    tabBar.selectedItem = tabBar.items?[1]
    tabBar.delegate = self
    tabBar.accessibilityIdentifier = "tabBar"

    bottomInsetEnlightener.backgroundColor = UIColor.clear
    bottomInsetEnlightener.isOpaque = false

    refreshControl.addTarget(self, action: #selector(refresh), for: .valueChanged)

    tableView.backgroundColor = UIColor.clear
    tableView.isOpaque = false
    tableView.delegate = self
    tableView.dataSource = self
    tableView.register(HistoricalOrderItemView.self, forCellReuseIdentifier: "order-item")
    tableView.register(HistoricalOrdersHeaderView.self, forHeaderFooterViewReuseIdentifier: "header")
    tableView.register(HistoricalOrderLoadingItemView.self, forCellReuseIdentifier: "loading-order-item")
    tableView.refreshControl = refreshControl
    tableView.accessibilityIdentifier = "ordersList"
    tableView.contentInsetAdjustmentBehavior = .never

    emptyBackground.backgroundColor = UIColor.backgroundColour
    emptyBackground.isHidden = true
    emptyBackground.alpha = 0
    emptyBackground.isUserInteractionEnabled = false
    emptyBackground.addSubview(middleAlignerView)
    emptyBackground.addSubview(emptyTitle)
    emptyBackground.addSubview(emptyBody)

    emptyTitle.numberOfLines = 0
    emptyTitle.textAlignment = .center
    emptyTitle.font = UIFont.brandFont(ofSize: 24, weight: .semibold)
    emptyTitle.alpha = 1
    emptyTitle.text = NSLocalizedString("You've not placed any orders yet", comment: "")
    emptyTitle.accessibilityIdentifier = "ordersEmptyTitle"
    emptyBody.numberOfLines = 0
    emptyBody.textAlignment = .center
    emptyBody.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    emptyBody.textColor = UIColor.grey10
    emptyBody.text = NSLocalizedString("Life's too short for queuing, start treating yourself to table-service instead!", comment: "")
    emptyBody.accessibilityIdentifier = "ordersEmptyBody"

    view.addSubview(topInsetEnlightener)
    view.addSubview(bottomInsetEnlightener)
    view.addSubview(emptyBackground)
    view.addSubview(tableView)
    view.addSubview(tabBar)

    constrain(emptyBackground, middleAlignerView) { container, middleAlignerView in
      middleAlignerView.width == 1
      middleAlignerView.height == 1
      middleAlignerView.centerX == container.centerX
      middleAlignerView.centerY == container.centerY
    }

    constrain(view, topInsetEnlightener, bottomInsetEnlightener, tabBar, tableView, emptyBackground) { container, topInsetEnlightener, bottomInsetEnlightener, tabBar, tableView, emptyBackground in
      topInsetEnlightener.top == container.top
      topInsetEnlightener.bottom == container.safeAreaLayoutGuide.top
      topInsetEnlightener.leading == container.leading
      topInsetEnlightener.trailing == container.trailing

      bottomInsetEnlightener.bottom == container.bottom
      bottomInsetEnlightener.top == container.safeAreaLayoutGuide.bottom
      bottomInsetEnlightener.left == container.left
      bottomInsetEnlightener.right == container.right

      tabBar.height == 49 + bottomInsetEnlightener.height
      tabBar.left == container.left
      tabBar.right == container.right
      tabBar.bottom == container.bottom

      tableView.bottom == tabBar.top
      tableView.top == container.top
      tableView.leading == container.leading
      tableView.trailing == container.trailing

      emptyBackground.top == tableView.top
      emptyBackground.bottom == tableView.bottom
      emptyBackground.leading == tableView.leading
      emptyBackground.trailing == tableView.trailing
    }

    constrain(emptyBackground, middleAlignerView, emptyTitle, emptyBody) { container, middleAlignerView, emptyTitle, emptyBody in
      emptyBody.top == middleAlignerView.bottom - 5
      emptyBody.leading == container.leading + 40
      emptyBody.trailing == container.trailing - 40

      emptyTitle.bottom == middleAlignerView.bottom - 15
      emptyTitle.leading == container.leading + 30
      emptyTitle.trailing == container.trailing - 30
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    view.setNeedsLayout()
    view.layoutSubviews()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? OrdersViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? OrdersViewModelState, to: current)
    }

    listenerRegistry <~ globalEmitter.on("orderComplete") { [weak self] _ in
      self?.refresh()
    }

    listenerRegistry <~ globalEmitter.on("orderUpdated") { [weak self] _ in
      self?.refresh()
    }
  }

  override func viewDidLayoutSubviews() {
    super.viewDidLayoutSubviews()
    guard tableView.contentInset.top == 0 else {
      return
    }

    tableView.contentInset.top = topInsetEnlightener.frame.height
    tableView.contentOffset = CGPoint(x: 0, y: 0-topInsetEnlightener.frame.height)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    tabBar.selectedItem = tabBar.items?[1]
    viewModel.viewWillAppear()
    navigationController?.applyDefaultStyling()
  }

  fileprivate func handleTransition(from: OrdersViewModelState?, to: OrdersViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if to.identifier == .transitioningToSettings {
      router.replace("/settings", context: nil, from: nil, animated: false)
    } else if to.identifier == .transitioningToNearby {
      router.replace("/home", context: nil, from: nil, animated: false)
    } else if to.identifier == .loading {
      loadingOrders = true
      tableView.reloadData()
      UIView.animate(withDuration: 0.3, animations: {
        self.emptyBackground.alpha = 0
      }) { _ in
        self.emptyBackground.isHidden = true
      }
    } else if to.identifier == .loadFailed || to.identifier == .loaded {
      loadingOrders = false
      refreshControl.endRefreshing()

      if to.identifier == .loadFailed {
        toaster.push(from: self, message: NSLocalizedString("Please make sure you're connected to the internet", comment: ""), caption: NSLocalizedString("Failed to fetch your order history", comment: ""), position: .bottom, style: .dark)
      }

      guard let orders = to.orders, orders.count > 0 else {
        self.orders = Dictionary()
        self.sections = []
        tableView.reloadData()
        emptyBackground.alpha = 0
        emptyBackground.isHidden = false
        UIView.animate(withDuration: 0.3, animations: {
          self.emptyBackground.alpha = 1
        }) { _ in
          self.emptyBackground.isHidden = false
        }
        return
      }

      var today: [HistoricalOrder] = []
      var yesterday: [HistoricalOrder] = []
      var older: [HistoricalOrder] = []
      var sections: [String] = []

      let dateToday = Date()
      let dateYesterday = Calendar.current.date(byAdding: .day, value: -1, to: dateToday, wrappingComponents: false)!

      orders.forEach { order in
        if Calendar.current.isDate(order.orderTimeStamp, inSameDayAs: dateToday) {
          today.append(order)
        } else if Calendar.current.isDate(order.orderTimeStamp, inSameDayAs: dateYesterday) {
          yesterday.append(order)
        } else {
          older.append(order)
        }
      }

      var newOrders: [String: [HistoricalOrder]] = Dictionary()

      if today.count > 0 {
        sections.append(NSLocalizedString("Today", comment: ""))
        newOrders[NSLocalizedString("Today", comment: "")] = today
      }

      if yesterday.count > 0 {
        sections.append(NSLocalizedString("Yesterday", comment: ""))
        newOrders[NSLocalizedString("Yesterday", comment: "")] = yesterday
      }

      if older.count > 0 {
        sections.append(NSLocalizedString("Older", comment: ""))
        newOrders[NSLocalizedString("Older", comment: "")] = older
      }

      self.orders = newOrders
      self.sections = sections
      tableView.reloadData()
    } else if to.identifier == .transitioningToOrderDetail {
      self.navigationItem.backBarButtonItem = UIBarButtonItem(title: nil, style: .plain, target: nil, action: nil)
      router.push("/active-order", context: [ "order": to.selectedOrder ])
    }
  }

  func tabBar(_ tabBar: UITabBar, didSelect item: UITabBarItem) {
    viewModel.didSelectTab(item.tag)
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    if loadingOrders {
      return 1
    }

    return sections.count
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    if loadingOrders {
      return 10
    }

    guard let orders = orders[sections[section]] else {
      return 0
    }

    return orders.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    if loadingOrders {
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "loading-order-item", for: indexPath) as? HistoricalOrderLoadingItemView else {
        fatalError()
      }

      return cell.configure()
    }

    guard let cell = tableView.dequeueReusableCell(withIdentifier: "order-item", for: indexPath) as? HistoricalOrderItemView else {
      fatalError()
    }

    guard let orders = orders[sections[indexPath.section]] else {
      fatalError()
    }

    return cell.configure(with: orders[indexPath.row])
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    if loadingOrders {
      return HistoricalOrderLoadingItemView.calculateHeight()
    }

    guard let orders = orders[sections[indexPath.section]] else {
      return 0
    }

    return HistoricalOrderItemView.calculateHeight(for: orders[indexPath.row])
  }

  func tableView(_ tableView: UITableView, estimatedHeightForRowAt indexPath: IndexPath) -> CGFloat {
    if loadingOrders {
      return HistoricalOrderLoadingItemView.calculateHeight()
    }

    guard let orders = orders[sections[indexPath.section]] else {
      return 0
    }

    return HistoricalOrderItemView.calculateHeight(for: orders[indexPath.row])
  }

  func tableView(_ tableView: UITableView, viewForHeaderInSection section: Int) -> UIView? {
    if loadingOrders {
      return nil
    }

    guard let header = tableView.dequeueReusableHeaderFooterView(withIdentifier: "header") as? HistoricalOrdersHeaderView else {
      fatalError()
    }

    let title = sections[section]
    return header.configure(title)
  }

  func tableView(_ tableView: UITableView, heightForHeaderInSection section: Int) -> CGFloat {
    if loadingOrders {
      return 0
    }

    let title = sections[section]
    return HistoricalOrdersHeaderView.calculateHeight(title: title)
  }

  func tableView(_ tableView: UITableView, estimatedHeightForHeaderInSection section: Int) -> CGFloat {
    if loadingOrders {
      return 0
    }

    let title = sections[section]
    return HistoricalOrdersHeaderView.calculateHeight(title: title)
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    if loadingOrders {
      return tableView.deselectRow(at: indexPath, animated: true)
    }

    guard let orders = orders[sections[indexPath.section]] else {
      return
    }

    viewModel.orderSelected(orders[indexPath.row])
    return tableView.deselectRow(at: indexPath, animated: true)
  }

  @objc fileprivate func refresh () {
    viewModel.refresh()
  }
}

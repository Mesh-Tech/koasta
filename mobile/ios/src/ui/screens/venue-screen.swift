import UIKit
import Cartography

class VenueViewController: UIViewController, Routable, UITableViewDelegate, UITableViewDataSource, UIScrollViewDelegate, MenuItemDelegate, MenuSwipeListDelegate {
  fileprivate let topInsetEnlightener = UIView()
  fileprivate let bottomInsetEnlightener = UIView()
  fileprivate let tableViewNavOverlay = UIView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .plain)
  fileprivate let orderBar = OrderBar()
  fileprivate let orderBarPadding = UIView()
  fileprivate let menuList = MenuSwipeList()
  fileprivate var viewModel: VenueViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var venue: Venue?
  fileprivate var menus: [VenueMenu]?
  fileprivate var orderBarVisibilityConstraint: NSLayoutConstraint!
  fileprivate var selectedTabIdx: Int = 0

  var routerContext: Route?
  fileprivate var router: Router!
  fileprivate var toaster: Toaster!

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: VenueViewModel?,
       listenerRegistry: EventListenerRegistry?,
       router: Router?,
       toaster: Toaster?) {
    guard let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let router = router,
      let toaster = toaster else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.router = router
    self.toaster = toaster
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.grey7
    view.addSubview(topInsetEnlightener)
    view.addSubview(bottomInsetEnlightener)
    view.addSubview(tableView)
    view.addSubview(tableViewNavOverlay)
    view.addSubview(orderBar)
    view.addSubview(orderBarPadding)
    view.addSubview(menuList)

    menuList.delegate = self
    menuList.accessibilityIdentifier = "menuList"

    tableViewNavOverlay.backgroundColor = UIColor.backgroundColour
    orderBarPadding.backgroundColor = UIColor.backgroundColour

    tableView.layoutMargins = UIEdgeInsets.zero
    tableView.separatorInset = UIEdgeInsets(top: 0, left: 60, bottom: 0, right: 0)
    tableView.dataSource = self
    tableView.delegate = self
    tableView.backgroundColor = UIColor.clear
    tableView.isOpaque = false
    tableView.estimatedRowHeight = UITableView.automaticDimension
    tableView.accessibilityIdentifier = "venueProductList"
    tableView.tableFooterView = UIView(frame: CGRect(x: 0, y: 0, width: UIScreen.main.bounds.width, height: 0.1))

    tableView.register(MenuItem.self, forCellReuseIdentifier: "menu-item")

    constrain(view, tableView, menuList) { container, tableView, menuList in
      tableView.left == container.left
      tableView.top == menuList.bottom
      tableView.right == container.right
      tableView.bottom == container.bottom
    }

    constrain(view, topInsetEnlightener, tableViewNavOverlay, menuList) { container, topInsetEnlightener, tableViewNavOverlay, menuList in
      topInsetEnlightener.top == container.top
      topInsetEnlightener.bottom == container.safeAreaLayoutGuide.top
      topInsetEnlightener.left == container.left
      topInsetEnlightener.right == container.right
      tableViewNavOverlay.left == container.left
      tableViewNavOverlay.top == container.top
      tableViewNavOverlay.right == container.right
      tableViewNavOverlay.height == 44 + topInsetEnlightener.height
      menuList.top == topInsetEnlightener.bottom
      menuList.left == tableViewNavOverlay.left
      menuList.right == tableViewNavOverlay.right
    }

    constrain(view, bottomInsetEnlightener, orderBar, orderBarPadding) { container, bottomInsetEnlightener, orderBar, orderBarPadding in
      bottomInsetEnlightener.bottom == container.bottom
      bottomInsetEnlightener.top == container.safeAreaLayoutGuide.bottom
      bottomInsetEnlightener.left == container.left
      bottomInsetEnlightener.right == container.right
      orderBar.left == container.left
      orderBar.right == container.right
      self.orderBarVisibilityConstraint = orderBar.bottom == bottomInsetEnlightener.top ~ LayoutPriority(100)
      orderBar.bottom == container.bottom + 72 ~ LayoutPriority(800)
      orderBarPadding.height == bottomInsetEnlightener.height
      orderBarPadding.left == container.left
      orderBarPadding.right == container.right
      orderBarPadding.top == orderBar.bottom
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    var defaultTabIdx = 0
    var venue: Venue?
    var menus: [VenueMenu]?

    if let ctx = routerContext?.navigationContext as? [String:Any] {
      if let defaultIdx = ctx["defaultTabIdx"] as? Int {
        defaultTabIdx = defaultIdx
      }

      if let cachedVenue = ctx["venue"] as? Venue {
        venue = cachedVenue
      }

      if let cachedMenus = ctx["menus"] as? [VenueMenu] {
        menus = cachedMenus
      }
    }

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? VenueViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? VenueViewModelState, to: current)
    }

    orderBar.addTarget(viewModel, action: #selector(VenueViewModel.viewReceiptTapped), for: .touchUpInside)

    guard let data = routerContext?.values,
          let venueId = data["venueId"] as? String else {
      navigationController?.popToRootViewController(animated: false)
      return
    }

    viewModel.viewDidLoad(venueId: venueId, defaultTabIndex: defaultTabIdx, venue: venue, menus: menus)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.titleTextAttributes = [NSAttributedString.Key.foregroundColor: UIColor.brandColour, NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)]
      self.navigationController?.navigationBar.tintColor = UIColor.brandColour
    }
    navigationController?.setNavigationBarHidden(false, animated: true)
    viewModel.viewWillAppear()
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return 1
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    guard let menus = menus else { return 0 }
    return menus[selectedTabIdx].productList.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    guard let cell = tableView.dequeueReusableCell(withIdentifier: "menu-item", for: indexPath) as? MenuItem else {
      fatalError()
    }
    guard let menus = menus else { return cell }
    let product = menus[selectedTabIdx].productList[indexPath.row]
    cell.delegate = self
    cell.tag = indexPath.row

    return cell.configure(with: product, selection: product.selection, isLastItem: indexPath.row + 1 == menus[selectedTabIdx].count)
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    guard let menus = menus else { return 0 }
    let product = menus[selectedTabIdx].productList[indexPath.row]

    return MenuItem.calculateHeight(for: product, selection: product.selection)
  }

  func tableView(_ tableView: UITableView, estimatedHeightForRowAt indexPath: IndexPath) -> CGFloat {
    guard let menus = menus else { return 0 }
    let product = menus[selectedTabIdx].productList[indexPath.row]

    return MenuItem.calculateHeight(for: product, selection: product.selection)
  }

  func tableView(_ tableView: UITableView, editingStyleForRowAt indexPath: IndexPath) -> UITableViewCell.EditingStyle {
    guard let menus = menus else { return .none }
    let product = menus[selectedTabIdx].productList[indexPath.row]

    return product.selection == nil ? .none : .delete
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    tableView.deselectRow(at: indexPath, animated: true)

    if venue?.isOpen == true {
      guard let menus = menus else { return }
      let product = menus[selectedTabIdx].productList[indexPath.row]

      viewModel.productSelected(product, section: selectedTabIdx, row: indexPath.row)
    } else {
      let alert = UIAlertController(title: NSLocalizedString("This venue is closed", comment: ""), message: NSLocalizedString("It'll be open later. You can browse the menu until then.", comment: ""), preferredStyle: .alert)
      alert.addAction(UIAlertAction(title: "Done", style: .cancel, handler: nil))
      present(alert, animated: true, completion: nil)
    }
  }

  func tableView(_ tableView: UITableView, commit editingStyle: UITableViewCell.EditingStyle, forRowAt indexPath: IndexPath) {
    if venue?.isOpen == true {
      guard let menus = menus else { return }
      let product = menus[selectedTabIdx].productList[indexPath.row]

      viewModel.productRemoved(product, forMenu: selectedTabIdx, product: indexPath.row)
    }
  }

  func addTapped(for key: Int) {
    if venue?.isOpen == true {
      guard let menus = menus else { return }
      let product = menus[selectedTabIdx].productList[key]

      viewModel.productSelected(product, section: selectedTabIdx, row: key)
    } else {
       let alert = UIAlertController(title: NSLocalizedString("This venue is closed", comment: ""), message: NSLocalizedString("It'll be open later. You can browse the menu until then.", comment: ""), preferredStyle: .alert)
       alert.addAction(UIAlertAction(title: "Done", style: .cancel, handler: nil))
       present(alert, animated: true, completion: nil)
     }
  }

  func didTapSwipeItem(at index: Int) {
    viewModel.tappedMenu(at: index)
  }

  fileprivate func handleTransition(from: VenueViewModelState?, to: VenueViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      // Initial is always the default UI state for the screen, so there won't always be a from state here
    } else if from?.identifier != .transitioningToOrderConfirm && from?.identifier != .transitioningToOrderComplete && to.identifier == .loaded {
      self.navigationItem.title = to.venue?.venueName
      self.venue = to.venue
      self.menus = to.menus
      self.selectedTabIdx = to.selectedTabIdx
      self.tableView.reloadData()
      self.menuList.configure(with: to.menus ?? [], defaultItem: to.selectedTabIdx)
      self.orderBar.loading = false
    } else if from?.identifier == .loaded && to.identifier == .productSelected {
      self.menus = to.menus
      self.tableView.reloadData()
      self.orderBar.configure(with: to.order)
      self.view.layoutIfNeeded()
      self.orderBarVisibilityConstraint.priority = (to.order?.count ?? 0) > 0 ? LayoutPriority(900) : LayoutPriority(100)
      UIView.animate(withDuration: 0.2) { self.view.layoutIfNeeded() }
    } else if from?.identifier == .loaded && to.identifier == .productRemoved {
      self.menus = to.menus
      self.tableView.reloadData()
      self.orderBar.configure(with: to.order, hasAddedProduct: true)
      self.view.layoutIfNeeded()
      self.orderBarVisibilityConstraint.priority = (to.order?.count ?? 0) > 0 ? LayoutPriority(900) : LayoutPriority(100)
      UIView.animate(withDuration: 0.2) { self.view.layoutIfNeeded() }
    } else if from?.identifier != .transitioningBack && to.identifier == .transitioningBack {
      navigationController?.popViewController(animated: true)
    } else if to.identifier == .loadingOrderEstimate {
      self.orderBar.loading = true
    } else if to.identifier == .transitioningToOrderConfirm {
      let context = ConfirmPurchaseContext(
        venueId: to.venue?.venueId ?? 0,
        order: Array(to.order ?? Set()),
        estimate: to.orderEstimate!,
        nonce: to.orderNonce!
      )

      if (to.order ?? Set()).contains(where: { product -> Bool in
        product.ageRestricted
      }) {
        let alert = UIAlertController(title: NSLocalizedString("Please confirm your age", comment: ""), message: NSLocalizedString("Your order contains age-restricted items. Please confirm you're 18 years of age or older. You may be asked for your ID when you pick up your order.", comment: ""), preferredStyle: .actionSheet)
        alert.addAction(UIAlertAction(title: NSLocalizedString("Confirm", comment: ""), style: .default, handler: { _ in
          self.navigationItem.backBarButtonItem = UIBarButtonItem(title: nil, style: .plain, target: nil, action: nil)
          self.router.push("/pub/\(self.routerContext?.values["venueId"] ?? "")/info/order", context: context)
          self.orderBar.alpha = 0
        }))
        alert.addAction(UIAlertAction(title: NSLocalizedString("Cancel", comment: ""), style: .cancel, handler: { _ in
          self.orderBar.loading = false
        }))
        alert.view.tintColor = .brandColour
        present(alert, animated: true, completion: nil)
      } else {
        self.navigationItem.backBarButtonItem = UIBarButtonItem(title: nil, style: .plain, target: nil, action: nil)
        self.router.push("/pub/\(self.routerContext?.values["venueId"] ?? "")/info/order", context: context)
        self.orderBar.alpha = 0
      }
    } else if from?.identifier == .transitioningToOrderConfirm {
      self.orderBar.alpha = 1
      self.orderBar.configure(with: to.order)
      self.orderBar.loading = false
      self.view.layoutIfNeeded()
      self.orderBarVisibilityConstraint.priority = (to.order?.count ?? 0) > 0 ? LayoutPriority(900) : LayoutPriority(100)
      UIView.animate(withDuration: 0.2) { self.view.layoutIfNeeded() }
    } else if to.identifier == .error {
      toaster.push(from: self, message: NSLocalizedString("Please make sure you're connected to the internet", comment: ""), caption: NSLocalizedString("Failed to fetch this information", comment: ""), position: .bottom, style: .dark)
      if to.identifier == .loadingOrderEstimate {
        self.orderBar.alpha = 1
        self.orderBar.configure(with: to.order)
        self.orderBar.loading = false
        self.view.layoutIfNeeded()
        self.orderBarVisibilityConstraint.priority = (to.order?.count ?? 0) > 0 ? LayoutPriority(900) : LayoutPriority(100)
        UIView.animate(withDuration: 0.2) { self.view.layoutIfNeeded() }
      }
    }
  }
}

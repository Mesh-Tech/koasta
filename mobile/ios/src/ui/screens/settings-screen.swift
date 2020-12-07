import UIKit
import Cartography

private enum SettingsOption {
  case share
  case faq
  case support
  case notifications
  case legal
  case beta
}

private struct SettingsMenuItem {
  let name: String
  let option: SettingsOption
  let image: String?
  let nativeImage: UIImage?

  init(name: String, option: SettingsOption, image: String?) {
    self.name = name
    self.option = option
    self.image = image
    self.nativeImage = nil
  }

  init(name: String, option: SettingsOption, image: UIImage?) {
    self.name = name
    self.option = option
    self.nativeImage = image
    self.image = nil
  }
}

class SettingsViewController: UIViewController, Routable, UITabBarDelegate, UITableViewDelegate, UITableViewDataSource {
  fileprivate var router: Router!
  fileprivate var globalEmitter: EventEmitter!
  fileprivate var viewModel: SettingsViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var sessionManager: SessionManager!

  fileprivate let tabBar = UITabBar()
  fileprivate let bottomInsetEnlightener = UIView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .grouped)

  #if ENVIRONMENT_DEV
  fileprivate let options: [SettingsMenuItem] = [
    SettingsMenuItem(name: "Invite Friends", option: .share, image: "icon-settings-share"),
//    SettingsMenuItem(name: "FAQ", option: .faq, image: ""),
    SettingsMenuItem(name: "Contact", option: .support, image: "icon-settings-contact"),
//    SettingsMenuItem(name: "Notifications", option: .notifications, image: ""),
    SettingsMenuItem(name: "Legal", option: .legal, image: "icon-settings-legal"),
    SettingsMenuItem(name: "Beta Tweaks", option: .beta, image: "icon-settings-beta")
  ]
  #elseif ENVIRONMENT_BETA
  fileprivate let options: [SettingsMenuItem] = [
    SettingsMenuItem(name: "Invite Friends", option: .share, image: "icon-settings-share"),
//    SettingsMenuItem(name: "FAQ", option: .faq, image: ""),
    SettingsMenuItem(name: "Contact", option: .support, image: "icon-settings-contact"),
//    SettingsMenuItem(name: "Notifications", option: .notifications, image: ""),
    SettingsMenuItem(name: "Legal", option: .legal, image: UIImage(systemName: "doc")),
    SettingsMenuItem(name: "Beta Tweaks", option: .beta, image: "icon-settings-beta")
  ]
  #elseif ENVIRONMENT_PROD
  fileprivate let options: [SettingsMenuItem] = [
    SettingsMenuItem(name: "Invite Friends", option: .share, image: "icon-settings-share"),
//    SettingsMenuItem(name: "FAQ", option: .faq, image: ""),
    SettingsMenuItem(name: "Contact", option: .support, image: "icon-settings-contact"),
//    SettingsMenuItem(name: "Notifications", option: .notifications, image: ""),
    SettingsMenuItem(name: "Legal", option: .legal, image: "icon-settings-legal")
  ]
  #endif

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  var routerContext: Route?

  init(router: Router?,
       globalEmitter: EventEmitter?,
       viewModel: SettingsViewModel?,
       listenerRegistry: EventListenerRegistry?,
       sessionManager: SessionManager?) {
    guard let router = router,
      let globalEmitter = globalEmitter,
      let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let sessionManager = sessionManager else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.globalEmitter = globalEmitter
    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.sessionManager = sessionManager
    modalPresentationStyle = .overCurrentContext
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.grey7

    tableView.backgroundColor = UIColor.clear
    tableView.isOpaque = false
    tableView.delegate = self
    tableView.dataSource = self
    tableView.register(SettingsItem.self, forCellReuseIdentifier: "settings-item")
    tableView.register(SettingsAccountItem.self, forCellReuseIdentifier: "settings-account-item")

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
    tabBar.selectedItem = tabBar.items?[2]
    tabBar.delegate = self
    tabBar.accessibilityIdentifier = "tabBar"

    bottomInsetEnlightener.backgroundColor = UIColor.clear
    bottomInsetEnlightener.isOpaque = false

    view.addSubview(bottomInsetEnlightener)
    view.addSubview(tabBar)
    view.addSubview(tableView)

    constrain(view, bottomInsetEnlightener, tabBar, tableView) { container, bottomInsetEnlightener, tabBar, tableView in
      bottomInsetEnlightener.bottom == container.bottom
      bottomInsetEnlightener.top == container.safeAreaLayoutGuide.bottom
      bottomInsetEnlightener.left == container.left
      bottomInsetEnlightener.right == container.right

      tabBar.height == 49 + bottomInsetEnlightener.height
      tabBar.left == container.left
      tabBar.right == container.right
      tabBar.bottom == container.bottom

      tableView.top == container.safeAreaLayoutGuide.top + 10
      tableView.bottom == tabBar.top
      tableView.left == container.left
      tableView.right == container.right
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? SettingsViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? SettingsViewModelState, to: current)
    }
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    tabBar.selectedItem = tabBar.items?[2]
    navigationController?.setNavigationBarHidden(true, animated: false)
    navigationItem.backBarButtonItem = UIBarButtonItem(title: nil, style: .plain, target: nil, action: nil)
  }

  fileprivate func handleTransition(from: SettingsViewModelState?, to: SettingsViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if to.identifier == .transitioningToOrders {
      router.replace("/orders", context: nil, from: nil, animated: false)
    } else if to.identifier == .transitioningToNearby {
      router.replace("/home", context: nil, from: nil, animated: false)
    }
  }

  func tabBar(_ tabBar: UITabBar, didSelect item: UITabBarItem) {
    viewModel.didSelectTab(item.tag)
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return 3
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    if section != 1 {
      return 1
    }

    return options.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    if indexPath.section == 0 {
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "settings-account-item", for: indexPath) as? SettingsAccountItem else {
        fatalError()
      }

      return cell.configure(withTitle: "Account Details", subtitleText: sessionManager.accountDescription ?? "Tap to sign in")
    }

    if indexPath.section == 2 {
      let cell = tableView.dequeueReusableCell(withIdentifier: "logout") ?? UITableViewCell(style: .default, reuseIdentifier: "logout")
      cell.textLabel?.text = NSLocalizedString("Sign out", comment: "Sign out")
      return cell
    }

    guard let cell = tableView.dequeueReusableCell(withIdentifier: "settings-item", for: indexPath) as? SettingsItem else {
      fatalError()
    }

    if let imageName = options[indexPath.row].image {
      return cell.configure(withTitle: options[indexPath.row].name, imageName: imageName)
    } else if let image = options[indexPath.row].nativeImage {
      return cell.configure(withTitle: options[indexPath.row].name, image: image)
    } else {
      fatalError()
    }
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    if indexPath.section == 0 {
      return tableView.deselectRow(at: indexPath, animated: true)
    }

    if indexPath.section == 2 {
      sessionManager.purge().then {
        self.router.purgeAll()
        self.router.replace("/onboarding")
      }
      return tableView.deselectRow(at: indexPath, animated: true)
    }

    tableView.deselectRow(at: indexPath, animated: true)

    switch options[indexPath.row].option {
    case .beta:
      router.push("/settings/beta")
    case .legal:
      router.push("/terms-and-conditions")
    case .support:
      showContactSheet()
    case .share:
      showShareSheet()
    default:
      break
    }
  }

  fileprivate func showShareSheet() {
    let picker = UIActivityViewController(activityItems: [
      "Make ordering easy with Koasta", URL(string: "https://www.koasta.com")!
    ], applicationActivities: nil)

    present(picker, animated: true, completion: nil)
  }

  fileprivate func showContactSheet() {
    let sheet = UIAlertController(title: NSLocalizedString("Koasta Support", comment: ""), message: NSLocalizedString("If you're having problems using Koasta, please get in touch and we'll get back to you soon", comment: ""), preferredStyle: .actionSheet)

    sheet.addAction(UIAlertAction(title: NSLocalizedString("E-mail us", comment: ""), style: .default, handler: { _ in
      guard let url = URL(string: "mailto:hello@koasta.com") else {
        return
      }

      UIApplication.shared.open(url, options: [:], completionHandler: nil)
    }))

    sheet.addAction(UIAlertAction(title: NSLocalizedString("Cancel", comment: ""), style: .cancel))

    present(sheet, animated: true, completion: nil)
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    if indexPath.section == 0 {
      return SettingsAccountItem.calculateHeight(withTitle: "Account Details", subtitleText: "07000 000 000")
    }

    return SettingsItem.calculateHeight()
  }

  func tableView(_ tableView: UITableView, estimatedHeightForRowAt indexPath: IndexPath) -> CGFloat {
    if indexPath.section == 0 {
      return SettingsAccountItem.calculateHeight(withTitle: "Account Details", subtitleText: "07000 000 000")
    }

    return SettingsItem.calculateHeight()
  }
}

import UIKit
import Cartography

class VenueOverviewViewController: UIViewController, Routable, UITableViewDelegate, UITableViewDataSource, UIScrollViewDelegate {
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .plain)
  fileprivate let pubImageView = IMG()
  fileprivate let imageOverlay = GradientContainer()
  fileprivate let statusBarUnderlay = UIVisualEffectView(effect: UIBlurEffect(style: .regular))
  fileprivate let imagestack = UIStackView()
  fileprivate let placeholderImageA = UIImageView()
  fileprivate let placeholderImageB = UIImageView()
  fileprivate let placeholderImageC = UIImageView()
  fileprivate let placeholderImageD = UIImageView()
  fileprivate var viewModel: VenueOverviewViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var venue: Venue?
  fileprivate var menuTitles: [String] = Array()
  fileprivate var orderBarVisibilityConstraint: NSLayoutConstraint!
  fileprivate var statusBlurHeightConstraint: NSLayoutConstraint!

  var routerContext: Route?
  fileprivate var router: Router!

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: VenueOverviewViewModel?,
       listenerRegistry: EventListenerRegistry?,
       router: Router?) {
    guard let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let router = router else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.router = router
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.backgroundColour
    view.addSubview(pubImageView)
    view.addSubview(imagestack)
    view.addSubview(tableView)
    view.addSubview(statusBarUnderlay)

    pubImageView.addSubview(imageOverlay)

    imageOverlay.from = UIColor(white: 0.0, alpha: 0.35)
    imageOverlay.to = UIColor(white: 0.0, alpha: 0.55)
    imageOverlay.fillPercentage = 0.8

    tableView.dataSource = self
    tableView.delegate = self
    tableView.backgroundColor = UIColor.clear
    tableView.isOpaque = false
    tableView.tableHeaderView = UIView(frame: CGRect(x: 0, y: 0, width: UIScreen.main.bounds.width, height: 0.01))
    tableView.estimatedRowHeight = UITableView.automaticDimension
    tableView.accessibilityIdentifier = "venueMenuLists"
    tableView.tableFooterView = UIView(frame: CGRect(x: 0, y: 0, width: UIScreen.main.bounds.width, height: 0.01))

    pubImageView.backgroundColor = UIColor.backgroundColour

    imagestack.distribution = .fillProportionally
    imagestack.spacing = 10
    imagestack.axis = .horizontal
    imagestack.addArrangedSubview(placeholderImageA)
    imagestack.addArrangedSubview(placeholderImageB)
    imagestack.addArrangedSubview(placeholderImageC)
    imagestack.addArrangedSubview(placeholderImageD)

    placeholderImageA.contentMode = .bottom
    placeholderImageB.contentMode = .bottom
    placeholderImageC.contentMode = .bottom
    placeholderImageD.contentMode = .bottom

    constrain(view, pubImageView, tableView, imageOverlay, statusBarUnderlay) { container, pubImageView, tableView, imageOverlay, statusBarUnderlay in
      tableView.top == pubImageView.bottom
      tableView.bottom == container.bottom
      tableView.left == container.left
      tableView.right == container.right

      pubImageView.top == container.top
      pubImageView.left == container.left
      pubImageView.right == container.right
      pubImageView.height == 280

      imageOverlay.width == pubImageView.width
      imageOverlay.height == pubImageView.height
      imageOverlay.top == pubImageView.top
      imageOverlay.left == pubImageView.left

      statusBarUnderlay.top == container.top
      statusBarUnderlay.leading == container.leading
      statusBarUnderlay.trailing == container.trailing
      statusBlurHeightConstraint = statusBarUnderlay.height == 0
    }

    constrain(pubImageView, imagestack) { thumb, imagestack in
      imagestack.right == thumb.right - 20
      imagestack.bottom == thumb.bottom - 20
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    tableView.register(VenueSummaryItemView.self, forCellReuseIdentifier: "venue-summary")
    tableView.register(VenueOverviewMenuHeaderView.self, forHeaderFooterViewReuseIdentifier: "menu-header")

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? VenueOverviewViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? VenueOverviewViewModelState, to: current)
    }

    guard let data = routerContext?.values,
          let venueId = data["venueId"] as? String else {
      navigationController?.popToRootViewController(animated: false)
      return
    }

    statusBlurHeightConstraint.constant = statusBarHeight
    view.setNeedsLayout()
    view.layoutIfNeeded()

    viewModel.viewDidLoad(venueId: venueId)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.tintColor = UIColor.chalkColour
    }
    navigationItem.title = nil
    navigationController?.setNavigationBarHidden(false, animated: true)
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return 2
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    guard section == 1 else { return 1 }
    return menuTitles.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    guard indexPath.section == 1 else {
      return (tableView.dequeueReusableCell(withIdentifier: "venue-summary", for: indexPath) as! VenueSummaryItemView).configure(with: venue)
    }

    let cell = tableView.dequeueReusableCell(withIdentifier: "menu-item") ?? UITableViewCell(style: .default, reuseIdentifier: "menu-item")
    let title = menuTitles[indexPath.row]

    cell.textLabel?.text = title
    cell.textLabel?.font = UIFont.preferredBrandFont(forTextStyle: .callout)
    cell.accessoryType = .disclosureIndicator

    return cell
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    guard indexPath.section == 1 else { return VenueSummaryItemView.calculateHeight(for: venue) }
    return 44
  }

  func tableView(_ tableView: UITableView, viewForHeaderInSection section: Int) -> UIView? {
    guard section == 1 else { return nil }

    return tableView.dequeueReusableHeaderFooterView(withIdentifier: "menu-header")
  }

  func tableView(_ tableView: UITableView, heightForHeaderInSection section: Int) -> CGFloat {
    guard section == 1 else { return 0 }
    return VenueOverviewMenuHeaderView.calculateHeight()
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    guard indexPath.section == 1 else { return }
    tableView.deselectRow(at: indexPath, animated: true)
    viewModel.menuSelected(atPosition: indexPath.row)
  }

  fileprivate func handleTransition(from: VenueOverviewViewModelState?, to: VenueOverviewViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      // Initial is always the default UI state for the screen, so there won't always be a from state here
    } else if from?.identifier == .loading && to.identifier == .loaded {
      menuTitles = (to.menus ?? []).map { $0.menuName }
      venue = to.venue
      tableView.reloadSections(IndexSet([0, 1]), with: .automatic)
      if let imageUrl = to.venue?.imageUrl {
        pubImageView.loadImage(url: imageUrl.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed))
      } else if let placeholder = to.venue?.placeholder {
        pubImageView.backgroundColor = Placeholders.backgroundColours[placeholder.backgroundIndex]
        placeholderImageA.image = Placeholders.placeholderImages[placeholder.imageAIndex]
        placeholderImageA.setNeedsLayout()
        placeholderImageB.image = Placeholders.placeholderImages[placeholder.imageBIndex]
        placeholderImageB.setNeedsLayout()
        placeholderImageC.image = Placeholders.placeholderImages[placeholder.imageCIndex]
        placeholderImageC.setNeedsLayout()
        placeholderImageD.image = Placeholders.placeholderImages[placeholder.imageDIndex]
        placeholderImageD.setNeedsLayout()

        imagestack.setNeedsLayout()
      }
    } else if to.identifier == .transitioningToVenueDetail {
      navigationItem.backBarButtonItem = UIBarButtonItem(title: "", style: .plain, target: nil, action: nil)
      let ctx: [String:Any] = [
        "defaultTabIdx": to.defaultTabIdx as Any,
        "venue": to.venue as Any,
        "menus": to.menus as Any
      ]
      router.push("/pub/\(to.venue?.venueId ?? -1)/info", context: ctx)
    }
  }
}

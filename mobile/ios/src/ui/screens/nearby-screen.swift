import UIKit
import Cartography
import CoreLocation

class NearbyViewController: UIViewController, Routable, UITabBarDelegate, UITableViewDelegate, UITableViewDataSource, SearchFieldEditingDelegate, VenueLocationCardItemViewDelegate, VenueVoteListCellDelegate, NoLaunchedVenuesItemViewDelegate {
  fileprivate var router: Router!
  fileprivate var globalEmitter: EventEmitter!
  fileprivate var viewModel: NearbyViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var toaster: Toaster!
  fileprivate var sessionManager: SessionManager!

  fileprivate let tabBar = UITabBar()
  fileprivate let topInsetEnlightener = UIView()
  fileprivate let bottomInsetEnlightener = UIView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .plain)
  fileprivate let noVenuesTitleView = UILabel()
  fileprivate let noVenuesBodyView = UILabel()
  fileprivate let middleAlignerView = UIView()
  fileprivate let navBackdropView = UIView()
  fileprivate let logo = UIImageView()
  fileprivate let searchButton = UIButton(type: .system)
  fileprivate let refreshControl = UIRefreshControl()
  fileprivate var venues: [Venue] = Array()
  fileprivate var votingVenues: [Venue] = Array()
  fileprivate var locationAvailable = true
  fileprivate var location: CLLocation?
  fileprivate var venuesLoading = false

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  var routerContext: Route?

  init(router: Router?,
       globalEmitter: EventEmitter?,
       viewModel: NearbyViewModel?,
       listenerRegistry: EventListenerRegistry?,
       toaster: Toaster?,
       sessionManager: SessionManager?) {
    guard let router = router,
      let globalEmitter = globalEmitter,
      let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let toaster = toaster,
      let sessionManager = sessionManager else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.globalEmitter = globalEmitter
    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.toaster = toaster
    self.sessionManager = sessionManager
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.backgroundColour

    tableView.delegate = self
    tableView.dataSource = self
    tableView.backgroundColor = .clear
    tableView.isOpaque = false
    tableView.register(VenueLoadingItemView.self, forCellReuseIdentifier: "venue-loading-item")
    tableView.register(VenueItemView.self, forCellReuseIdentifier: "venue-item")
    tableView.register(VenuePlaceholderItemView.self, forCellReuseIdentifier: "venue-placeholder-item")
    tableView.register(VenueVoteListCell.self, forCellReuseIdentifier: "venue-vote")
    tableView.register(VenueLocationCardItemView.self, forCellReuseIdentifier: "venue-location-card")
    tableView.register(NoLaunchedVenuesItemView.self, forCellReuseIdentifier: "no-launched-venues-item")
    tableView.layoutMargins = UIEdgeInsets.zero
    tableView.separatorInset = UIEdgeInsets.zero
    tableView.separatorStyle = .none
    tableView.accessibilityIdentifier = "venueList"

    let title = NSLocalizedString("Pull to refresh", comment: "Pull to refresh")
    refreshControl.attributedTitle = NSAttributedString(string: title)
    refreshControl.addTarget(self, action: #selector(refresh), for: .valueChanged)
    tableView.refreshControl = refreshControl

    logo.image = UIImage(named: "logo-text-light")
    logo.contentMode = .scaleAspectFit
    logo.tintColor = .brandColour

    searchButton.setImage(UIImage(systemName: "magnifyingglass"), for: .normal)
    searchButton.contentMode = .center
    searchButton.tintColor = UIColor.brandColour
    searchButton.addTarget(self, action: #selector(search), for: .touchUpInside)

    tabBar.setItems([
      UITabBarItem(title: NSLocalizedString("Home", comment: "Tab bar item - Home"), image: UIImage(named: "icon-tabbar-home"), selectedImage: UIImage(named: "icon-tabbar-home")),
      UITabBarItem(title: NSLocalizedString("Orders", comment: "Tab bar item - Orders"), image: UIImage(systemName: "cube.box"), selectedImage: nil),
      UITabBarItem(title: NSLocalizedString("Settings", comment: "Tab bar item - Settings"), image: UIImage(systemName: "gear"), selectedImage: nil)
    ], animated: false)

    tabBar.tintColor = UIColor.brandColour
    tabBar.items?[0].tag = 0
    tabBar.items?[1].tag = 1
    tabBar.items?[2].tag = 2
    tabBar.selectedItem = tabBar.items?[0]
    tabBar.delegate = self
    tabBar.accessibilityIdentifier = "tabBar"

    topInsetEnlightener.backgroundColor = UIColor.clear
    topInsetEnlightener.isOpaque = false
    bottomInsetEnlightener.backgroundColor = UIColor.clear
    bottomInsetEnlightener.isOpaque = false

    noVenuesTitleView.numberOfLines = 0
    noVenuesTitleView.textAlignment = .center
    noVenuesTitleView.font = UIFont.brandFont(ofSize: 24, weight: .semibold)
    noVenuesTitleView.alpha = 0
    noVenuesTitleView.isHidden = true
    noVenuesTitleView.text = NSLocalizedString("We're not in your area yet :(", comment: "")
    noVenuesTitleView.textColor = UIColor.foregroundColour
    noVenuesTitleView.accessibilityIdentifier = "noVenuesTitleView"

    noVenuesBodyView.numberOfLines = 0
    noVenuesBodyView.textAlignment = .center
    noVenuesBodyView.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    noVenuesBodyView.textColor = UIColor.grey10
    noVenuesBodyView.isHidden = true
    noVenuesBodyView.text = NSLocalizedString("Sorry about that, but we're expanding quickly! Check back soon to see if we've added nearby venues.", comment: "")

    navBackdropView.backgroundColor = UIColor.backgroundColour

    [topInsetEnlightener, bottomInsetEnlightener, tableView, tabBar, middleAlignerView, noVenuesTitleView, noVenuesBodyView, navBackdropView, searchButton, logo].forEach { view.addSubview($0) }

    constrain(view, topInsetEnlightener, bottomInsetEnlightener, tableView, tabBar, navBackdropView) { container, topInsetEnlightener, bottomInsetEnlightener, tableView, tabBar, navBackdropView in
      topInsetEnlightener.top == container.top
      topInsetEnlightener.bottom == container.safeAreaLayoutGuide.top
      topInsetEnlightener.left == container.left
      topInsetEnlightener.right == container.right

      bottomInsetEnlightener.bottom == container.bottom
      bottomInsetEnlightener.top == container.safeAreaLayoutGuide.bottom
      bottomInsetEnlightener.left == container.left
      bottomInsetEnlightener.right == container.right

      tableView.top == navBackdropView.bottom
      tableView.bottom == tabBar.top
      tableView.left == container.left
      tableView.right == container.right

      tabBar.height == 49 + bottomInsetEnlightener.height
      tabBar.left == container.left
      tabBar.right == container.right
      tabBar.bottom == container.bottom
    }

    constrain(view, middleAlignerView) { container, middleAlignerView in
      middleAlignerView.width == 1
      middleAlignerView.height == 1
      middleAlignerView.centerX == container.centerX
      middleAlignerView.centerY == container.centerY
    }

    constrain(view, navBackdropView, searchButton, logo) { container, navBackdropView, searchButton, logo in
      navBackdropView.top == container.top
      navBackdropView.leading == container.leading
      navBackdropView.trailing == container.trailing
      navBackdropView.bottom == container.safeAreaLayoutGuide.top + 40
      searchButton.top == navBackdropView.bottom - 52
      searchButton.trailing == container.trailing
      searchButton.width == 48
      searchButton.height == 48
      logo.top == navBackdropView.bottom - 40
      logo.centerX == container.centerX
    }

    constrain(view, middleAlignerView, noVenuesTitleView, noVenuesBodyView) { container, middleAlignerView, noVenuesTitleView, noVenuesBodyView in
      noVenuesBodyView.top == middleAlignerView.bottom - 5
      noVenuesBodyView.leading == container.leading + 40
      noVenuesBodyView.trailing == container.trailing - 40

      noVenuesTitleView.bottom == middleAlignerView.bottom - 15
      noVenuesTitleView.leading == container.leading + 40
      noVenuesTitleView.trailing == container.trailing - 40
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? NearbyViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? NearbyViewModelState, to: current)
    }

    listenerRegistry <~ globalEmitter.on("search-dismiss") { [weak self] _ in
      self?.viewModel.viewWillAppear()
    }

    NotificationCenter.default.addObserver(self, selector: #selector(appWillAppear), name: UIApplication.didBecomeActiveNotification, object: nil)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    viewModel.viewWillAppear()
    tabBar.selectedItem = tabBar.items?[0]
    navigationController?.setNavigationBarHidden(true, animated: true)
  }

  @objc fileprivate func appWillAppear(notification: Notification) {
    viewModel.viewWillAppear()
  }

  fileprivate func handleTransition(from: NearbyViewModelState?, to: NearbyViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if to.identifier == .transitioningToOrders {
      router.replace("/orders", context: nil, from: nil, animated: false)
    } else if to.identifier == .transitioningToSettings {
      router.replace("/settings", context: nil, from: nil, animated: false)
    } else if to.identifier == .transitioningToLocationSettings {
      let alert = UIAlertController(title: NSLocalizedString("Enabling location", comment: ""), message: NSLocalizedString("You've previously denied location permissions for Koasta. To enable location, please do so in the Settings app", comment: ""), preferredStyle: .alert)
      alert.addAction(UIAlertAction(title: NSLocalizedString("Done", comment: ""), style: .cancel) { [weak self] _ in
        self?.viewModel.viewWillAppear()
      })
      present(alert, animated: true)
    } else if to.identifier == .transitioningToVenueOnboarding {
      router.push("/pub-onboarding/\(to.selectedVenueId ?? "")", context: ["venue": to.selectedVenue])
    } else if to.identifier == .loadingNearbyVenues {
      locationAvailable = to.locationAvailable
      location = to.location
      venuesLoading = true
      tableView.reloadSections(IndexSet(integersIn: 0...1), with: .fade)
      UIView.animate(withDuration: 0.3) {
        self.noVenuesTitleView.alpha = 0
        self.noVenuesBodyView.alpha = 0
      }
    } else if to.identifier == .loadingNearbyVenuesFailed {
      locationAvailable = to.locationAvailable
      location = to.location
      venuesLoading = false
      refreshControl.endRefreshing()

      tableView.reloadSections(IndexSet(integersIn: 0...1), with: .fade)
      toaster.push(from: self, message: NSLocalizedString("Please make sure you're connected to the internet", comment: ""), caption: NSLocalizedString("Failed to fetch nearby venues", comment: ""), position: .bottom, style: .dark)
    } else if from?.identifier == .loadingNearbyVenues && to.identifier == .loadedNearbyVenues {
      locationAvailable = to.locationAvailable
      location = to.location
      venuesLoading = false
      refreshControl.endRefreshing()

      let newVenues = to.venues ?? []

      venues = newVenues.filter {
        $0.companyId != Constants.DataLoadCompanyId
      }

      votingVenues = []
      let tempVotingVenues = Array(newVenues.filter {
        $0.companyId == Constants.DataLoadCompanyId
      })
      for i in (0 ..< 5) where i < tempVotingVenues.count {
        votingVenues.append(tempVotingVenues[i])
      }

      tableView.reloadSections(IndexSet(integersIn: 0...1), with: .fade)

      if venues.count == 0 && votingVenues.count == 0 {
        self.noVenuesTitleView.alpha = 0
        self.noVenuesBodyView.alpha = 0
        self.noVenuesTitleView.isHidden = false
        self.noVenuesBodyView.isHidden = false

        UIView.animate(withDuration: 0.3) {
          self.noVenuesTitleView.alpha = 1
          self.noVenuesBodyView.alpha = 1
        }
      } else if venues.count > 0 {
        UIView.animate(withDuration: 0.3) {
          self.noVenuesTitleView.alpha = 0
          self.noVenuesBodyView.alpha = 0
        }
      }
    } else if to.identifier == .transitioningToVenueDetails {
      navigationItem.backBarButtonItem = UIBarButtonItem(title: "", style: .plain, target: nil, action: nil)
      router.push("/pub/\(to.selectedVenueId ?? "")")
    } else if to.identifier == .transitioningToVenueSearch {
      router.present("/pub-search", context: [ "searchString": to.searchText ?? "" ], wrap: true, from: self, animated: true, completion: nil)
    }
  }

  func tabBar(_ tabBar: UITabBar, didSelect item: UITabBarItem) {
    viewModel.didSelectTab(item.tag)
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return 2
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    if section == 0 {
      if venuesLoading {
        return 0
      }

      return locationAvailable ? 0 : 1
    }

    if venuesLoading {
      return 5
    }

    if venues.count == 0 && votingVenues.count > 0 {
      return 1
    }

    return venues.count + (votingVenues.count > 0 ? 1 : 0)
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    if venuesLoading {
      if indexPath.section == 0 {
        fatalError()
      }

      guard let cell = tableView.dequeueReusableCell(withIdentifier: "venue-loading-item", for: indexPath) as? VenueLoadingItemView else {
        fatalError()
      }

      return cell.configure()
    }

    if indexPath.section == 0 {
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "venue-location-card", for: indexPath) as? VenueLocationCardItemView else {
        fatalError()
      }

      cell.delegate = self

      return cell.configure()
    }

    if venues.isEmpty {
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "no-launched-venues-item", for: indexPath) as? NoLaunchedVenuesItemView else {
        fatalError()
      }

      cell.delegate = self
      return cell.configure(votingVenues, profile: sessionManager.currentProfile)
    }

    if indexPath.row == venues.count {
      guard let cell = tableView.dequeueReusableCell(withIdentifier: "venue-vote", for: indexPath) as? VenueVoteListCell else {
        fatalError()
      }

      cell.delegate = self
      return cell.configure(votingVenues, profile: sessionManager.currentProfile)
    } else {
      let venue = venues[indexPath.row]

      if venue.imageUrl == nil {
        guard let cell = tableView.dequeueReusableCell(withIdentifier: "venue-placeholder-item", for: indexPath) as? VenuePlaceholderItemView else {
          fatalError()
        }

        return cell.configure(with: venue, atIndexPath: indexPath, location: location)
      } else {
        guard let cell = tableView.dequeueReusableCell(withIdentifier: "venue-item", for: indexPath) as? VenueItemView else {
          fatalError()
        }

        return cell.configure(with: venue, location: location)
      }
    }
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    if venuesLoading {
      return VenueLoadingItemView.calculateHeight()
    }

    if indexPath.section == 0 {
      return VenueLocationCardItemView.calculateHeight()
    }

    if venues.isEmpty {
      return NoLaunchedVenuesItemView.calculateHeight(for: votingVenues, profile: sessionManager.currentProfile)
    }

    if indexPath.row == venues.count {
      return VenueVoteListCell.calculateHeight()
    }

    let venue = venues[indexPath.row]

    if venue.imageUrl == nil {
      return VenuePlaceholderItemView.calculateHeight(for: venue, atIndexPath: indexPath, location: location)
    }

    return VenueItemView.calculateHeight(for: venue, location: location)
  }

  func tableView(_ tableView: UITableView, estimatedHeightForRowAt indexPath: IndexPath) -> CGFloat {
    if venuesLoading {
      return VenueLoadingItemView.calculateHeight()
    }

    if indexPath.section == 0 {
      return VenueLocationCardItemView.calculateHeight()
    }

    if venues.isEmpty {
      return NoLaunchedVenuesItemView.calculateHeight(for: votingVenues, profile: sessionManager.currentProfile)
    }

    if indexPath.row == venues.count {
      return VenueVoteListCell.calculateHeight()
    }

    let venue = venues[indexPath.row]

    if venue.imageUrl == nil {
      return VenuePlaceholderItemView.calculateHeight(for: venue, atIndexPath: indexPath, location: location)
    }

    return VenueItemView.calculateHeight(for: venue, location: location)
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    if venuesLoading {
      return
    }

    if indexPath.section == 0 {
      return tableView.deselectRow(at: indexPath, animated: true)
    }

    if venues.isEmpty {
      return
    }

    if indexPath.row == venues.count {
      return
    }

    let venue = venues[indexPath.row]
    viewModel.didSelectVenue(venue)
    tableView.deselectRow(at: indexPath, animated: true)
  }

  func locationCardItemDidRequestLocation(_ item: VenueLocationCardItemView) {
    viewModel.allowLocationTapped()
  }

  @objc fileprivate func search() {
    viewModel.searchForVenueTapped()
  }

  @objc fileprivate func refresh() {
    viewModel.refresh()
  }

  func votedForVenue(_ venue: Venue?) {
    guard let v = venue else {
      return
    }

    viewModel.votedForVenue(v.venueId)
  }

  func voteRaised(for: VenueVoteListCell, venueId: Int) {
    viewModel.votedForVenue(venueId)
  }
}

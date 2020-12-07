import Foundation
import UIKit
import Cartography

class VenueSearchController: UIViewController, Routable, UITableViewDelegate, UITableViewDataSource, SearchFieldEditingDelegate {
  fileprivate var router: Router!
  fileprivate var viewModel: VenueSearchViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var globalEmitter: EventEmitter!

  fileprivate let titleLabel = UILabel()
  fileprivate let bodyLabel = UILabel()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .plain)
  fileprivate let navBackdropView = UIView()
  fileprivate let searchField = SearchField()
  fileprivate let cancelButton = UIButton(type: .system)
  fileprivate var venues: [Venue] = Array()
  fileprivate var cancelVisibleYet = false

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  var routerContext: Route?

  init(router: Router?,
       viewModel: VenueSearchViewModel?,
       listenerRegistry: EventListenerRegistry?,
       globalEmitter: EventEmitter?) {
    guard let router = router,
      let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let globalEmitter = globalEmitter else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.globalEmitter = globalEmitter
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = .backgroundColour

    searchField.placeholder = NSLocalizedString("Search for a place or location", comment: "")
    searchField.editingDelegate = self

    titleLabel.text = NSLocalizedString("Start typing to search", comment: "")
    titleLabel.font = UIFont.brandFont(ofSize: 24, weight: .semibold)
    titleLabel.textAlignment = .center
    titleLabel.textColor = .foregroundColour

    bodyLabel.text = NSLocalizedString("Find locations, bars, cafes and pubs", comment: "")
    bodyLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    bodyLabel.textAlignment = .center
    bodyLabel.textColor = .grey10

    tableView.delegate = self
    tableView.dataSource = self
    tableView.register(SearchItem.self, forCellReuseIdentifier: "search-item")
    tableView.layoutMargins = UIEdgeInsets.zero
    tableView.separatorInset = UIEdgeInsets(top: 0, left: 20, bottom: 0, right: 0)
    tableView.separatorStyle = .singleLine
    tableView.isHidden = true
    tableView.rowHeight = 150

    navBackdropView.backgroundColor = .clear

    cancelButton.setTitle(NSLocalizedString("Cancel", comment: ""), for: .normal)
    cancelButton.tintColor = .brandColour
    cancelButton.titleLabel?.font = UIFont.brandFont(ofSize: 17, weight: .medium)

    [tableView, titleLabel, bodyLabel, navBackdropView, searchField, cancelButton].forEach { view.addSubview($0) }

    constrain(view, tableView, titleLabel, bodyLabel) { container, tableView, titleLabel, bodyLabel in
      tableView.top == container.top + 70
      tableView.bottom == container.bottom
      tableView.left == container.left
      tableView.right == container.right

      titleLabel.top == container.top + 70 + 36
      titleLabel.leading == container.leading
      titleLabel.trailing == container.trailing

      bodyLabel.top == titleLabel.bottom + 10
      bodyLabel.leading == container.leading
      bodyLabel.trailing == container.trailing
    }
  }

  override func viewDidLayoutSubviews() {
    super.viewDidLayoutSubviews()
    let cancelSize = cancelButton.intrinsicContentSize
    let cancelXDelta = cancelVisibleYet ? (cancelSize.width + 20) : 0
    let searchSize = searchField.intrinsicContentSize
    let searchWidthDelta = cancelXDelta + 40

    navBackdropView.frame = CGRect(x: 0, y: 0, width: view.bounds.width, height: 70)
    searchField.frame = CGRect(x: 20, y: navBackdropView.frame.maxY - (10 + searchSize.height), width: navBackdropView.frame.width - searchWidthDelta, height: searchSize.height)
    cancelButton.frame = CGRect(x: navBackdropView.frame.maxX - cancelXDelta, y: searchField.frame.minY + ((searchField.frame.height / 2) - (cancelSize.height / 2)), width: cancelSize.width, height: cancelSize.height)
  }

  override func viewDidLoad() {
    super.viewDidLoad()
    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? VenueSearchViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? VenueSearchViewModelState, to: current)
    }

    cancelButton.addTarget(viewModel, action: #selector(VenueSearchViewModel.cancelTapped), for: .touchUpInside)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    navigationController?.setNavigationBarHidden(true, animated: true)

    if let ctx = routerContext?.navigationContext as? [String:Any] {
      viewModel.viewWillAppear(ctx["searchString"] as? String, oldState: ctx["searchState"] as? VenueSearchViewModelState)
    } else {
      viewModel.viewWillAppear()
    }
  }

  override func viewDidAppear(_ animated: Bool) {
    super.viewDidAppear(animated)
    viewModel.viewDidAppear()
  }

  fileprivate func handleTransition(from: VenueSearchViewModelState?, to: VenueSearchViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if from?.identifier == .initial && to.identifier == .initial {
      // Do nothing
    } else if to.identifier == .loaded {
      view.layoutIfNeeded()
      view.setNeedsLayout()
      cancelVisibleYet = true
      UIView.animate(withDuration: 0.2, delay: 0, options: [.curveEaseOut], animations: {
        self.view.layoutIfNeeded()
      })

      _ = self.searchField.becomeFirstResponder()
    } else if to.identifier == .transitioningBack {
      dismiss(animated: true, completion: {
        self.globalEmitter.emit("search-dismiss")
      })
    } else if from?.identifier == .loaded && to.identifier == .searching {
      tableView.isHidden = false
      UIView.animate(withDuration: 0.3) {
        self.titleLabel.alpha = 0
        self.bodyLabel.alpha = 0
      }
    } else if to.identifier == .transitioningToVenue {
      navigationItem.backBarButtonItem = UIBarButtonItem(title: nil, style: .plain, target: nil, action: nil)
      router.push("/pub/\(to.selectedVenue?.venueId ?? -1)")
    } else if to.identifier == .searchResults {
      venues = to.results ?? []
      tableView.reloadData()
    }
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return 1
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    return venues.count
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    guard let cell = tableView.dequeueReusableCell(withIdentifier: "search-item", for: indexPath) as? SearchItem else {
      fatalError()
    }

    let venue = venues[indexPath.row]

    return cell.configure(with: venue)
  }

  func tableView(_ tableView: UITableView, heightForRowAt indexPath: IndexPath) -> CGFloat {
    let venue = venues[indexPath.row]

    return SearchItem.calculateHeight(for: venue)
  }

  func tableView(_ tableView: UITableView, estimatedHeightForRowAt indexPath: IndexPath) -> CGFloat {
    let venue = venues[indexPath.row]

    return SearchItem.calculateHeight(for: venue)
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    let venue = venues[indexPath.row]
    viewModel.didSelectVenue(venue)
    tableView.deselectRow(at: indexPath, animated: true)
  }

  func textChanged(to: String?, for field: SearchField) {
    viewModel.searchTextChanged(to ?? "")
  }

  func scrollViewDidScroll(_ scrollView: UIScrollView) {
    _ = searchField.resignFirstResponder()
  }
}

import Foundation
import UIKit
import Cartography

class LocationRequestViewController: UIViewController, Routable {
  var routerContext: Route?
  fileprivate var router: Router!
  fileprivate var viewModel: LocationRequestViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate let titleView = UILabel()
  fileprivate let bodyView = UILabel()
  fileprivate let allowButton = BigButton(style: .solidRed)
  fileprivate let skipButton = BigButton(style: .solidRedInverted)
  fileprivate let middleAlignerView = UIView()

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: LocationRequestViewModel?,
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
    titleView.text = NSLocalizedString("Psst, where are you?", comment: "")
    titleView.numberOfLines = 0
    titleView.textAlignment = .center
    titleView.font = UIFont.brandFont(ofSize: 24, weight: .semibold)
    bodyView.text = NSLocalizedString("Please allow Koasta access to your location so we can show bars, pubs and cafes near you", comment: "")
    bodyView.numberOfLines = 0
    bodyView.textAlignment = .center
    bodyView.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    bodyView.textColor = UIColor.grey10
    allowButton.titleLabel.text = NSLocalizedString("Allow location access", comment: "")
    skipButton.titleLabel.text = NSLocalizedString("Not now", comment: "")

    [middleAlignerView, titleView, bodyView, allowButton, skipButton].forEach { view.addSubview($0) }

    constrain(view, middleAlignerView, titleView, bodyView, allowButton, skipButton) { container, middleAlignerView, titleView, bodyView, allowButton, skipButton in
      middleAlignerView.width == 1
      middleAlignerView.height == 1
      middleAlignerView.centerX == container.centerX
      middleAlignerView.centerY == container.centerY

      bodyView.bottom == middleAlignerView.top - 40
      bodyView.leading == container.leading + 40
      bodyView.trailing == container.trailing - 40

      titleView.bottom == bodyView.top - 10
      titleView.leading == container.leading + 40
      titleView.trailing == container.trailing - 40

      allowButton.top == middleAlignerView.bottom - 10
      allowButton.leading == container.leading + 20
      allowButton.trailing == container.trailing - 20

      skipButton.top == allowButton.bottom + 15
      skipButton.leading == container.leading + 20
      skipButton.trailing == container.trailing - 20
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? LocationRequestViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? LocationRequestViewModelState, to: current)
    }

    skipButton.addTarget(viewModel, action: #selector(LocationRequestViewModel.skipButtonTapped), for: .touchUpInside)
    allowButton.addTarget(viewModel, action: #selector(LocationRequestViewModel.allowButtonTapped), for: .touchUpInside)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    navigationController?.setNavigationBarHidden(true, animated: true)
//    navigationController?.overrideUserInterfaceStyle = .unspecified
//    navigationController?.navigationBar.barStyle = .default
  }

  fileprivate func handleTransition(from: LocationRequestViewModelState?, to: LocationRequestViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      // Initial is always the default UI state for the screen, so there won't always be a from state here
    } else if to.identifier == .requestingPermission {
      skipButton.setEnabled(false, animated: true)
      allowButton.setEnabled(false, animated: true)
    } else if to.identifier == .permissionRequestFailed {
      skipButton.setEnabled(true, animated: true)
      allowButton.setEnabled(true, animated: true)
    } else if to.identifier == .transitioningToVenues {
      router.replace("/home")
    }
  }
}

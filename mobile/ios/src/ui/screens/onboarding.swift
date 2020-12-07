import UIKit
import Cartography

class OnboardingViewController: UIViewController, Routable, UIScrollViewDelegate {
  var routerContext: Route?
  fileprivate var router: Router!
  fileprivate var viewModel: OnboardingViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var toaster: Toaster!
  fileprivate let startButton = BigButton(style: .solidRed)
  fileprivate let titleLabel = UILabel()
  fileprivate let scrollView = UIScrollView()
  fileprivate let pager = UIPageControl()
  fileprivate let page1Title = NSLocalizedString("Forget queueing for a drink", comment: "")
  fileprivate let page2Title = NSLocalizedString("Just take a seat & order with Koasta", comment: "")
  fileprivate let page3Title = NSLocalizedString("We'll let you know when it's ready", comment: "")
  fileprivate let viewA = UIImageView(frame: CGRect.zero)
  fileprivate let viewB = UIImageView(frame: CGRect.zero)
  fileprivate let viewC = UIImageView(frame: CGRect.zero)

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: OnboardingViewModel?,
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
    view.backgroundColor = UIColor.charcoalColour

    startButton.titleLabel.text = NSLocalizedString("Get Started", comment: "Screen: [Onboarding] Element: [Start button] Scenario: [Not signed in]")
    startButton.paddingHeight = 18
    startButton.addTarget(self, action: #selector(self.startButtonTapped), for: .touchUpInside)
    startButton.accessibilityIdentifier = "splashGetStartedButton"

    scrollView.contentInsetAdjustmentBehavior = .never
    scrollView.showsVerticalScrollIndicator = false
    scrollView.showsHorizontalScrollIndicator = false
    scrollView.bounces = false
    scrollView.isPagingEnabled = true
    scrollView.delegate = self

    titleLabel.font = UIFont.boldBrandFont(ofSize: 35)
    titleLabel.numberOfLines = 0
    titleLabel.textAlignment = .center
    titleLabel.text = page1Title
    titleLabel.textColor = .white
    titleLabel.accessibilityIdentifier = "splashPageTitle"

    pager.tintColor = .white
    pager.numberOfPages = 3
    pager.addTarget(self, action: #selector(self.pagerValueChanged), for: .valueChanged)

    let contentView = UIView()

    viewA.image = UIImage(named: "onboarding_1_gradient")
    viewB.image = UIImage(named: "onboarding_2_gradient")
    viewC.image = UIImage(named: "onboarding_3_gradient")

    scrollView.addSubview(contentView)

    contentView.addSubview(viewA)
    contentView.addSubview(viewB)
    contentView.addSubview(viewC)

    view.addSubview(scrollView)
    view.addSubview(pager)
    view.addSubview(titleLabel)
    view.addSubview(startButton)

    constrain(contentView, scrollView, viewA, viewB, viewC) { container, scrollView, viewA, viewB, viewC in
      viewA.leading == container.leading
      viewA.top == container.top
      viewA.bottom == container.bottom
      viewA.width == scrollView.width
      viewA.height == scrollView.height
      viewB.leading == viewA.trailing
      viewB.top == container.top
      viewB.bottom == container.bottom
      viewB.width == viewA.width
      viewB.height == viewA.height
      viewC.leading == viewB.trailing
      viewC.top == container.top
      viewC.bottom == container.bottom
      viewC.width == viewA.width
      viewC.height == viewA.height
      viewC.trailing == container.trailing
    }

    constrain(scrollView, contentView) { container, contentView in
      contentView.leading == container.leading
      contentView.trailing == container.trailing
      contentView.top == container.top
      contentView.bottom == container.bottom
    }

    constrain(view, scrollView, startButton, titleLabel, pager) { container, scrollView, startButton, titleLabel, pager in
      scrollView.top == container.top
      scrollView.bottom == container.bottom
      scrollView.leading == container.leading
      scrollView.trailing == container.trailing
      startButton.leading == container.leading + 20
      startButton.trailing == container.trailing - 20
      startButton.bottom == container.safeAreaLayoutGuide.bottom
      titleLabel.bottom == startButton.top - 75
      titleLabel.leading == startButton.leading + 26
      titleLabel.trailing == startButton.trailing - 26
      pager.bottom == startButton.top - 20
      pager.centerX == container.centerX
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? OnboardingViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? OnboardingViewModelState, to: current)
    }
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    viewModel.viewWillAppear()
    navigationController?.overrideUserInterfaceStyle = .dark
    navigationController?.navigationBar.barStyle = .black
  }

  override func viewWillDisappear(_ animated: Bool) {
    navigationController?.overrideUserInterfaceStyle = .unspecified
    navigationController?.navigationBar.barStyle = .default
  }

  fileprivate func handleTransition(from: OnboardingViewModelState?, to: OnboardingViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      // Initial is always the default UI state for the screen, so there won't always be a from state here
      if let ctx = routerContext?.navigationContext as? [String:Any], let disabledLogin = ctx["encounteredDisabledLogin"] as? Bool, disabledLogin {
        DispatchQueue.main.async {
          self.toaster.push(from: self, message: NSLocalizedString("Facebook is currently unavailable due to situations out of our control. Please try Sign in with Apple, or try again later.", comment: ""), caption: nil, position: .top, style: .light, timeout: 5)
        }
      } else if let ctx = routerContext?.navigationContext as? [String:Any], let sessionExpired = ctx["sessionExpired"] as? Bool, sessionExpired {
        DispatchQueue.main.async {
          self.toaster.push(from: self, message: NSLocalizedString("You haven't used the app for a while. Please sign in again.", comment: ""), caption: nil, position: .top, style: .light)
        }
      }
    } else if to.identifier == .transitioningToSignIn {
      self.navigationItem.backBarButtonItem = UIBarButtonItem(title: nil, style: .plain, target: nil, action: nil)
      router.push("/sign-in", context: ["from": "onboarding"])
    }
  }

  func scrollViewDidScroll(_ scrollView: UIScrollView) {
    let screenWidth = UIScreen.main.bounds.width
    let offsetX = scrollView.contentOffset.x
    var delta: CGFloat = 0

    if scrollView.contentOffset.x >= screenWidth && scrollView.contentOffset.x < screenWidth * 2 {
      delta = screenWidth
    } else if scrollView.contentOffset.x >= screenWidth * 2 {
      delta = screenWidth * 2
    }

    let scrollPercent = (offsetX - delta) / screenWidth

    if scrollPercent <= 0.5 {
      titleLabel.alpha = 1.0 - (scrollPercent * 2)
    } else {
      titleLabel.alpha = scrollPercent
    }

    if scrollView.contentOffset.x < screenWidth / 2 {
      titleLabel.text = page1Title
    } else if scrollView.contentOffset.x >= screenWidth / 2 && scrollView.contentOffset.x < screenWidth * 1.5 {
      titleLabel.text = page2Title
    } else if scrollView.contentOffset.x >= screenWidth * 1.5 {
      titleLabel.text = page3Title
    }
  }

  func scrollViewDidEndDecelerating(_ scrollView: UIScrollView) {
    let screenWidth = UIScreen.main.bounds.width

    if scrollView.contentOffset.x == 0 {
      pager.currentPage = 0
    } else if scrollView.contentOffset.x == screenWidth {
      pager.currentPage = 1
    } else {
      pager.currentPage = 2
    }
  }

  @objc private func pagerValueChanged(sender: UIPageControl) {
    scrollView.scrollRectToVisible(CGRect(x: Int(UIScreen.main.bounds.width) * sender.currentPage, y: 0, width: 1, height: 1), animated: true)
  }

  @objc private func startButtonTapped(sender: UIButton) {
    viewModel.startButtonTapped()
  }
}

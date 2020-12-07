import Foundation
import UIKit
import Cartography
import FBSDKLoginKit
import AuthenticationServices

class AuthenticationViewController: UIViewController, Routable, UITextViewDelegate, LoginButtonDelegate, ASAuthorizationControllerPresentationContextProviding, ASAuthorizationControllerDelegate {
  fileprivate var viewModel: AuthenticationViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var socialButtonProvider: SocialButtonProvider!
  fileprivate let scrollView = UIScrollView()
  fileprivate let contentView = UIView()
  fileprivate let titleAligner = UIView()

  fileprivate let titleView = UILabel()
  fileprivate let bodyView = UILabel()
  fileprivate let disclaimer = UITextView()

  fileprivate let buttonStack = UIStackView()

  fileprivate var scrollViewConstraint: NSLayoutConstraint?
  fileprivate var navigationSource: String?
  fileprivate let progress = UIActivityIndicatorView(style: UIActivityIndicatorView.Style.medium)

  var routerContext: Route?
  fileprivate var router: Router!
  fileprivate var toaster: Toaster!
  fileprivate var config: Config!

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: AuthenticationViewModel?,
       listenerRegistry: EventListenerRegistry?,
       router: Router?,
       toaster: Toaster?,
       config: Config?,
       socialButtonProvider: SocialButtonProvider?) {
    guard let viewModel = viewModel,
      let listenerRegistry = listenerRegistry,
      let router = router,
      let toaster = toaster,
      let config = config,
      let socialButtonProvider = socialButtonProvider else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.viewModel = viewModel
    self.listenerRegistry = listenerRegistry
    self.router = router
    self.toaster = toaster
    self.config = config
    self.socialButtonProvider = socialButtonProvider
  }

  override func loadView() {
    super.loadView()
    self.view.backgroundColor = UIColor.backgroundColour

    view.addSubview(scrollView)
    scrollView.addSubview(contentView)

    scrollView.contentInsetAdjustmentBehavior = .never

    constrain(view, scrollView, contentView) { container, scrollView, contentView in
      scrollView.top == container.top
      scrollViewConstraint = scrollView.bottom == container.bottom
      scrollView.leading == container.leading
      scrollView.trailing == container.trailing

      contentView.top == scrollView.top
      contentView.bottom == scrollView.bottom
      contentView.leading == scrollView.leading
      contentView.trailing == scrollView.trailing
      contentView.width == scrollView.width
      contentView.height >= scrollView.height
    }

    let appleLoginButton = socialButtonProvider.buildAppleButton()
    appleLoginButton.addTarget(self, action: #selector(appleTapped), for: .touchUpInside)
    appleLoginButton.accessibilityIdentifier = "appleLoginButton"

    buttonStack.addArrangedSubview(appleLoginButton)

    if config.flags.flags.facebookAuth == true {
      let facebookLoginButton = socialButtonProvider.buildFacebookButton(connectingDelegate: self)
      facebookLoginButton.accessibilityIdentifier = "facebookLoginButton"
      for const in facebookLoginButton.constraints {
        if const.firstAttribute == NSLayoutConstraint.Attribute.height && const.constant == 28 {
          facebookLoginButton.removeConstraint(const)
        }
      }

      buttonStack.addArrangedSubview(facebookLoginButton)
      constrain(facebookLoginButton) { facebookLoginButton in
        facebookLoginButton.height == 50
      }
    }

    buttonStack.distribution = .equalSpacing
    buttonStack.axis = .vertical
    buttonStack.spacing = 20
    buttonStack.alignment = .fill

    [titleAligner, titleView, bodyView, progress, disclaimer, buttonStack].forEach { contentView.addSubview($0) }

    constrain(contentView, titleView, bodyView, disclaimer, buttonStack, appleLoginButton) { container, titleView, bodyView, disclaimer, buttonStack, appleLoginButton in
      titleView.leading == container.leading + 35
      titleView.top == container.top + 80
      titleView.trailing == container.trailing - 35

      bodyView.top == titleView.bottom + 42
      bodyView.leading == container.leading + 20
      bodyView.trailing == container.trailing - 20

      buttonStack.leading == container.leading + 35
      buttonStack.trailing == container.trailing - 35
      buttonStack.bottom == container.safeAreaLayoutGuide.bottom - 20

      disclaimer.top >= bodyView.bottom
      disclaimer.bottom == buttonStack.top - 20
      disclaimer.leading == container.leading + 20
      disclaimer.trailing == container.trailing - 20

      appleLoginButton.height == 50
    }

    constrain(contentView, progress) { container, progress in
      progress.center == container.center
    }

    titleView.accessibilityIdentifier = "authTitle"
    bodyView.accessibilityIdentifier = "authBody"

    bodyView.font = UIFont.brandFont(ofSize: 14)
    bodyView.textColor = UIColor.grey10
    bodyView.numberOfLines = 0
    bodyView.textAlignment = .natural
    bodyView.text = NSLocalizedString("Sign in so that we can verify who you are at the bar, and show you your current and previous orders.\n\nWe only keep a record of your name. No other personal information is stored or shared with third parties.", comment: "Screen: [Sign in] Element: [Body label] Scenario: [Not signed in]")
    titleView.textColor = UIColor.foregroundColour
    titleView.font = UIFont.boldBrandFont(ofSize: 23)
    titleView.textAlignment = .center
    titleView.numberOfLines = 0
    titleView.text = NSLocalizedString("Please sign in to get started", comment: "Screen: [Sign in] Element: [Title label] Scenario: [Not signed in]")
    progress.hidesWhenStopped = false
    progress.startAnimating()
    progress.alpha = 0

    let rawDisclaimerText = NSLocalizedString("By continuing I agree to Koasta's \u{200B}Terms & Conditions\u{200B} and \u{200B}Privacy Policy\u{200B}.", comment: "Screen: [Sign in] Element: [Phone number field] Scenario: [Not signed in]")
    let disclaimerText = NSMutableAttributedString(string: rawDisclaimerText)
    disclaimerText.setAttributes([
      NSAttributedString.Key.font : UIFont.brandFont(ofSize: 14, weight: UIFont.Weight.medium),
      NSAttributedString.Key.foregroundColor : UIColor(white: 0.35, alpha: 1.0)
    ], range: NSRange(location: 0, length: (disclaimerText.string as NSString).length))

    let markerIndices = (rawDisclaimerText as NSString).indicesOf(string: "\u{200B}")
    var links: [URL] = [
      URL(string: "koasta://terms-and-conditions")!,
      URL(string: "koasta://privacy-policy")!
    ]

    markerIndices.pairs.forEach {
      let url = links.remove(at: 0)
      disclaimerText.addAttributes([
        NSAttributedString.Key.link: url,
        NSAttributedString.Key.font : UIFont.brandFont(ofSize: 14, weight: UIFont.Weight.semibold)
      ], range: NSRange(location: $0.0.location, length: $0.1.location - $0.0.location))
    }

    disclaimer.attributedText = disclaimerText
    disclaimer.isEditable = false
    disclaimer.isScrollEnabled = false
    disclaimer.tintColor = UIColor.foregroundColour
    disclaimer.textAlignment = .center
    disclaimer.delegate = self
    disclaimer.backgroundColor = .clear
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? AuthenticationViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? AuthenticationViewModelState, to: current)
    }
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)

    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.titleTextAttributes = [NSAttributedString.Key.foregroundColor: UIColor.brandColour, NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)]
      self.navigationController?.navigationBar.tintColor = UIColor.brandColour
      self.setNeedsStatusBarAppearanceUpdate()
    }

    guard let context = routerContext?.navigationContext as? [String : String], let from = context["from"] else {
      return viewModel.viewWillAppear()
    }

    viewModel.viewWillAppear(from: from)
  }

  @objc fileprivate func appleTapped() {
    viewModel.appleTapped()
    let appleIDProvider = ASAuthorizationAppleIDProvider()
    let request = appleIDProvider.createRequest()
    request.requestedScopes = [.fullName, .email]

    let authorizationController = ASAuthorizationController(authorizationRequests: [request])
    authorizationController.delegate = self
    authorizationController.presentationContextProvider = self
    authorizationController.performRequests()
  }

  fileprivate func handleTransition(from: AuthenticationViewModelState?, to: AuthenticationViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      navigationSource = to.navigationSource
    } else if from?.identifier == .initial && to.identifier == .authenticating {
      UIView.animate(withDuration: 0.3) {
        self.titleView.alpha = 0
        self.bodyView.alpha = 0
        self.disclaimer.alpha = 0
        self.buttonStack.alpha = 0
        self.progress.alpha = 1
      }
    } else if from?.identifier == .initial && to.identifier == .transitioningToVenueList {
      dismissSelf()
    } else if from?.identifier == .authenticating && to.identifier == .transitioningToVenueList {
      toaster.push(from: self, message: NSLocalizedString("Account verified! Welcome to Koasta 🎉", comment: "Screen: [Sign in] Element: [Toast] Scenario: [Verified]"))
      dismissSelf()
    } else if from?.identifier == .authenticating && to.identifier == .initial {
      UIView.animate(withDuration: 0.3) {
        self.titleView.alpha = 1
        self.bodyView.alpha = 1
        self.disclaimer.alpha = 1
        self.buttonStack.alpha = 1
        self.progress.alpha = 0
      }
    }
  }

  fileprivate func dismissSelf() {
    if navigationSource == "onboarding" {
      router.replace("/home", from: navigationController)
    } else {
      dismiss(animated: true, completion: nil)
    }
  }

  func textView(_ textView: UITextView, shouldInteractWith URL: URL, in characterRange: NSRange, interaction: UITextItemInteraction) -> Bool {
    guard URL.absoluteString.contains("koasta://") else {
      return true
    }

    let base = URL.absoluteString.replacingOccurrences(of: "koasta://", with: "")

    router.push("/\(base)", context: ["contentType": base])

    return false
  }

  func loginButtonWillLogin(_ loginButton: FBLoginButton) -> Bool {
    viewModel.facebookTapped()
    return true
  }

  func loginButtonDidLogOut(_ loginButton: FBLoginButton) {
    viewModel.facebookLoggedOut()
  }

  func loginButton(_ loginButton: FBLoginButton, didCompleteWith result: LoginManagerLoginResult?, error: Error?) {
    viewModel.facebookCompleted(result: result, error: error)
  }

  func authorizationController(controller: ASAuthorizationController, didCompleteWithError error: Error) {
    viewModel.appleCompleted(result: nil, error: error)
  }

  func authorizationController(controller: ASAuthorizationController, didCompleteWithAuthorization authorization: ASAuthorization) {
    viewModel.appleCompleted(result: authorization, error: nil)
  }

  func presentationAnchor(for controller: ASAuthorizationController) -> ASPresentationAnchor {
    return self.view.window!
  }
}

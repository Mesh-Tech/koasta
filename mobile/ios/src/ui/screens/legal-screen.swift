import Foundation
import UIKit
import Cartography
import Down

class LegalViewController: UIViewController, Routable {
  var routerContext: Route?
  fileprivate var router: Router!
  fileprivate var viewModel: LegalViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var toaster: Toaster!
  fileprivate let progressView = UIActivityIndicatorView(style: UIActivityIndicatorView.Style.medium)
  fileprivate let contentView = UITextView()
  fileprivate let topInsetEnlightener = UIView()
  fileprivate let tableViewNavOverlay = UIView()

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: LegalViewModel?,
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
    view.backgroundColor = UIColor.backgroundColour

    progressView.hidesWhenStopped = false
    progressView.startAnimating()
    progressView.alpha = 0

    contentView.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    contentView.alpha = 0
    contentView.showsHorizontalScrollIndicator = false
    contentView.alwaysBounceHorizontal = false
    contentView.isSelectable = true
    contentView.isEditable = false
    contentView.textColor = .foregroundColour
    contentView.tintColor = .brandColour
    contentView.contentInset = UIEdgeInsets(top: 20, left: 20, bottom: 20, right: 20)

    [topInsetEnlightener, tableViewNavOverlay, contentView, progressView].forEach { view.addSubview($0) }

    constrain(view, topInsetEnlightener, tableViewNavOverlay) { container, topInsetEnlightener, tableViewNavOverlay in
      topInsetEnlightener.top == container.top
      topInsetEnlightener.bottom == container.safeAreaLayoutGuide.top
      topInsetEnlightener.left == container.left
      topInsetEnlightener.right == container.right
      tableViewNavOverlay.left == container.left
      tableViewNavOverlay.top == container.top
      tableViewNavOverlay.right == container.right
      tableViewNavOverlay.height == topInsetEnlightener.height
    }

    constrain(view, tableViewNavOverlay, contentView, progressView) { container, tableViewNavOverlay, contentView, progressView in
      progressView.centerX == container.centerX
      progressView.centerY == container.centerY
      contentView.top == tableViewNavOverlay.bottom
      contentView.leading == container.leading
      contentView.trailing == container.trailing
      contentView.bottom == container.bottom
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? LegalViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? LegalViewModelState, to: current)
    }
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.titleTextAttributes = [NSAttributedString.Key.foregroundColor: UIColor.brandColour, NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)]
      self.navigationController?.navigationBar.tintColor = UIColor.brandColour
    }
    navigationController?.setNavigationBarHidden(false, animated: true)

    if let url = routerContext?.url, let contentType = LegalContentType.parseString(url.urlStringValue) {
      viewModel.viewWillAppear(contentType: contentType)
    }
  }

  fileprivate func handleTransition(from: LegalViewModelState?, to: LegalViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      // Initial is always the default UI state for the screen, so there won't always be a from state here
      UIView.animate(withDuration: 0.15) {
        self.progressView.alpha = 1
      }
    } else if to.identifier == .loaded {
      contentView.alpha = 0
      progressView.alpha = 1

      var colours = StaticColorCollection()
      colours.body = .foregroundColour
      colours.code = .foregroundColour
      colours.codeBlockBackground = .grey3
      colours.heading1 = .foregroundColour
      colours.heading2 = .foregroundColour
      colours.heading3 = .foregroundColour
      colours.heading4 = .foregroundColour
      colours.heading5 = .foregroundColour
      colours.heading6 = .foregroundColour
      colours.link = .brandColour
      colours.listItemPrefix = .foregroundColour
      colours.quote = .foregroundColour
      colours.quoteStripe = .grey3
      colours.thematicBreak = .grey3

      var fonts = StaticFontCollection()
      fonts.body = .brandFont(ofSize: 16, weight: .regular)
      fonts.code = .brandFont(ofSize: 16, weight: .regular)
      fonts.heading1 = .brandFont(ofSize: 22, weight: .semibold)
      fonts.heading2 = .brandFont(ofSize: 18, weight: .medium)
      fonts.heading3 = .brandFont(ofSize: 17, weight: .medium)
      fonts.heading4 = .brandFont(ofSize: 17, weight: .medium)
      fonts.heading5 = .brandFont(ofSize: 17, weight: .medium)
      fonts.heading6 = .brandFont(ofSize: 16, weight: .medium)
      fonts.listItemPrefix = .brandFont(ofSize: 16, weight: .regular)

      var config = DownStylerConfiguration()
      config.colors = colours
      config.fonts = fonts

      let styler = DownStyler(configuration: config)

      contentView.attributedText = try? Down(markdownString: to.content ?? "").toAttributedString(styler: styler)

      UIView.animate(withDuration: 0.15) {
        self.contentView.alpha = 1
        self.progressView.alpha = 0
      }
    } else if to.identifier == .loadFailed {
      UIView.animate(withDuration: 0.15) {
        self.progressView.alpha = 0
      }

      toaster.push(from: self, message: NSLocalizedString("We were unable to fetch this document. Please try again.", comment: ""), caption: nil, position: .bottom, style: .dark)
    }
  }
}

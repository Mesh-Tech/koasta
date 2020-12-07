import UIKit
import Cartography

class VenueOnboardingViewController: UIViewController, Routable {
  fileprivate let pubImageView = IMG()
  fileprivate let imageOverlay = GradientContainer()
  fileprivate let venueTitle = UILabel()
  fileprivate let statusText = UILabel()
  fileprivate let statusBody = UILabel()
  fileprivate let progress = Progress(progressViewStyle: .default)
  fileprivate let shareButton = BigButton(style: .solidRed)
  fileprivate var border = UIView()
  fileprivate let statusBarUnderlay = UIVisualEffectView(effect: UIBlurEffect(style: .regular))
  fileprivate let imagestack = UIStackView()
  fileprivate let placeholderImageA = UIImageView()
  fileprivate let placeholderImageB = UIImageView()
  fileprivate let placeholderImageC = UIImageView()
  fileprivate let placeholderImageD = UIImageView()
  fileprivate var viewModel: VenueOnboardingViewModel!
  fileprivate var listenerRegistry: EventListenerRegistry!
  fileprivate var venue: Venue?
  fileprivate var orderBarVisibilityConstraint: NSLayoutConstraint!
  fileprivate var statusBlurHeightConstraint: NSLayoutConstraint!

  var routerContext: Route?
  fileprivate var router: Router!

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init(viewModel: VenueOnboardingViewModel?,
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

    venueTitle.numberOfLines = 1
    venueTitle.lineBreakMode = .byClipping
    venueTitle.setContentHuggingPriority(.defaultLow, for: .horizontal)
    venueTitle.setContentHuggingPriority(.defaultHigh, for: .vertical)
    venueTitle.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    venueTitle.accessibilityIdentifier = "venueTitle"

    statusText.numberOfLines = 0
    statusText.lineBreakMode = .byWordWrapping
    statusText.font = UIFont.brandFont(ofSize: 17, weight: .medium)
    statusText.accessibilityIdentifier = "statusText"

    statusBody.numberOfLines = 0
    statusBody.lineBreakMode = .byWordWrapping
    statusBody.font = UIFont.brandFont(ofSize: 15, weight: .regular)
    statusBody.accessibilityIdentifier = "statusBody"

    border.backgroundColor = UIColor.grey15

    shareButton.titleLabel.text = NSLocalizedString("Share your interest", comment: "")
    shareButton.addTarget(self, action: #selector(share), for: .touchUpInside)

    view.backgroundColor = UIColor.backgroundColour
    view.addSubview(pubImageView)
    view.addSubview(imagestack)
    view.addSubview(border)
    view.addSubview(statusBarUnderlay)
    view.addSubview(venueTitle)
    view.addSubview(statusText)
    view.addSubview(progress)
    view.addSubview(statusBody)
    view.addSubview(shareButton)

    pubImageView.addSubview(imageOverlay)

    imageOverlay.from = UIColor(white: 0.0, alpha: 0.35)
    imageOverlay.to = UIColor(white: 0.0, alpha: 0.55)
    imageOverlay.fillPercentage = 0.8

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

    progress.tintColor = .brightBrandColour

    constrain(view, pubImageView, venueTitle, border, imageOverlay, statusBarUnderlay, statusText, progress, statusBody, shareButton) { container, pubImageView, venueTitle, border, imageOverlay, statusBarUnderlay, statusText, progress, statusBody, shareButton in
      venueTitle.top == pubImageView.bottom + 14
      venueTitle.leading == container.leading + 18
      venueTitle.trailing == container.trailing - 18

      border.top == venueTitle.bottom + 14
      border.leading == container.leading
      border.trailing == container.trailing
      border.height == 0.5

      statusText.top == border.bottom + 20
      statusText.leading == container.leading + 18
      statusText.trailing == container.trailing - 18

      progress.top == statusText.bottom + 8
      progress.leading == statusText.leading
      progress.trailing == statusText.trailing
      progress.height == 8

      statusBody.top == progress.bottom + 20
      statusBody.leading == statusText.leading
      statusBody.trailing == statusText.trailing

      pubImageView.top == container.top
      pubImageView.leading == container.leading
      pubImageView.trailing == container.trailing
      pubImageView.height == 280

      imageOverlay.width == pubImageView.width
      imageOverlay.height == pubImageView.height
      imageOverlay.top == pubImageView.top
      imageOverlay.leading == pubImageView.leading

      statusBarUnderlay.top == container.top
      statusBarUnderlay.leading == container.leading
      statusBarUnderlay.trailing == container.trailing
      statusBlurHeightConstraint = statusBarUnderlay.height == 0

      shareButton.bottom == container.safeAreaLayoutGuide.bottom - 20
      shareButton.leading == container.leading + 18
      shareButton.trailing == container.trailing - 18
    }

    constrain(pubImageView, imagestack) { thumb, imagestack in
      imagestack.trailing == thumb.trailing - 20
      imagestack.bottom == thumb.bottom - 20
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()

    listenerRegistry <~ viewModel.on("statechange") { [weak self] event in
      guard let current = event.data["current"] as? VenueOnboardingViewModelState else { return }
      self?.handleTransition(from: event.data["previous"] as? VenueOnboardingViewModelState, to: current)
    }

    guard let data = routerContext?.values,
          let venueId = data["venueId"] as? String else {
      navigationController?.popToRootViewController(animated: false)
      return
    }

    statusBlurHeightConstraint.constant = statusBarHeight
    view.setNeedsLayout()
    view.layoutIfNeeded()

    var venue: Venue?
    if let ctx = routerContext?.navigationContext as? [String:Any] {
      if let v = ctx["venue"] as? Venue {
        venue = v
      }
    }

    viewModel.viewDidLoad(venueId: venueId, venue: venue)
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.tintColor = UIColor.chalkColour
    }
    navigationItem.title = nil
    navigationController?.setNavigationBarHidden(false, animated: true)
  }

  fileprivate func handleTransition(from: VenueOnboardingViewModelState?, to: VenueOnboardingViewModelState) {
    print("Transitioning from: \(String(describing: from?.identifier)) to: \(to.identifier)")
    if (from?.identifier == .initial  || from == nil) && to.identifier == .initial {
      // Initial is always the default UI state for the screen, so there won't always be a from state here
    } else if from?.identifier == .loading && to.identifier == .loaded {
      venue = to.venue
      venueTitle.text = venue?.venueName

      if let venue = venue {
        switch venue.progress {
        case 0:
          statusText.text = NSLocalizedString("You're a trendsetter!", comment: "")
          statusBody.text = NSLocalizedString("You're one of the first few to vote for \(venue.venueName) to join Koasta. Once we have enough interest we'll start working towards getting this venue onboard.\n\nBy sharing your interest in \(venue.venueName) to your friends, you can speed this process up further.", comment: "")
          progress.progress = 0.25
        case 1:
          statusText.text = NSLocalizedString("We're gathering interest", comment: "")
          statusBody.text = NSLocalizedString("People like you are voting for \(venue.venueName) to join Koasta! Once we have enough interest we'll start working towards getting this venue onboard.\n\nBy sharing your interest in \(venue.venueName) to your friends, you can speed this process up further.", comment: "")
          progress.progress = 0.65
        default:
          statusText.text = NSLocalizedString("Nearly there!", comment: "")
          statusBody.text = NSLocalizedString("We're in talks with \(venue.venueName) and hope to bring them onboard soon. Thanks for your support!", comment: "")
          progress.progress = 0.8
        }
      }
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
    }
  }

  @objc fileprivate func share() {
    guard let venue = venue else {
      return
    }

    var items: [Any] = [
      "I voted for \(venue.venueName) to join Koasta. Add your vote too!",
      URL(string: "https://www.koasta.com")!
    ]

    if let image = pubImageView.image {
      items.append(image)
    }

    let picker = UIActivityViewController(activityItems: items, applicationActivities: nil)

    present(picker, animated: true, completion: nil)
  }
}

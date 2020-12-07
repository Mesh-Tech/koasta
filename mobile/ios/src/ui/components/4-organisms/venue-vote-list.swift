import Foundation
import UIKit
import Cartography

private class VenueVoteView: UIControl {
  fileprivate let imageView = UIView()
  fileprivate let titleLabel = UILabel()
  fileprivate var voteButton = MiniButton(style: .solidRed)
  fileprivate let voteLabel = UILabel()
  fileprivate let imagestack = UIStackView()
  fileprivate let placeholderImageA = UIImageView()
  fileprivate let placeholderImageB = UIImageView()
  fileprivate let placeholderImageC = UIImageView()
  fileprivate let placeholderImageD = UIImageView()

  var venue: Venue?

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }
  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  private func setup () {
    voteButton.isUserInteractionEnabled = false
    titleLabel.isUserInteractionEnabled = false
    voteLabel.isUserInteractionEnabled = false
    imageView.isUserInteractionEnabled = false

    imagestack.distribution = .fillProportionally
    imagestack.spacing = 10
    imagestack.axis = .horizontal
    imagestack.addArrangedSubview(placeholderImageA)
    imagestack.addArrangedSubview(placeholderImageB)
    imagestack.addArrangedSubview(placeholderImageC)
    imagestack.addArrangedSubview(placeholderImageD)
    imagestack.isUserInteractionEnabled = false

    placeholderImageA.contentMode = .bottom
    placeholderImageB.contentMode = .bottom
    placeholderImageC.contentMode = .bottom
    placeholderImageD.contentMode = .bottom

    addSubview(imageView)
    addSubview(imagestack)
    addSubview(titleLabel)
    addSubview(voteButton)
    addSubview(voteLabel)
  }

  func configure(with venue: Venue, voteCount: Int, ownVoteRegistered: Bool) {
    self.venue = venue

    titleLabel.font = UIFont.brandFont(ofSize: 16, weight: .semibold)
    titleLabel.numberOfLines = 1
    titleLabel.textAlignment = .natural
    titleLabel.textColor = UIColor.foregroundColour
    titleLabel.text = venue.venueName
    titleLabel.accessibilityIdentifier = "votingVenueTitle"
    voteLabel.font = UIFont.brandFont(ofSize: 14, weight: .medium)
    voteLabel.textColor = UIColor.grey10

    if ownVoteRegistered {
      voteLabel.text = NSLocalizedString("Tap to view progress", comment: "")
      voteButton.titleLabel.text = NSLocalizedString("Voted", comment: "")
      voteButton.isEnabled = false
    } else if voteCount == 0 {
      voteLabel.text = NSLocalizedString("Register your interest", comment: "")
      voteButton.titleLabel.text = NSLocalizedString("Vote", comment: "")
    } else if voteCount == 1 {
      voteLabel.text = NSLocalizedString("\(voteCount) vote", comment: "")
      voteButton.titleLabel.text = NSLocalizedString("Vote", comment: "")
    } else {
      voteLabel.text = NSLocalizedString("\(voteCount) votes", comment: "")
      voteButton.titleLabel.text = NSLocalizedString("Vote", comment: "")
    }

    if let placeholder = venue.placeholder {
      imageView.backgroundColor = Placeholders.backgroundColours[placeholder.backgroundIndex]

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

    invalidateIntrinsicContentSize()
    setNeedsLayout()
    setNeedsDisplay()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    backgroundColor = UIColor.clear
    isOpaque = false

    imageView.frame = CGRect(x: 0, y: 0, width: bounds.width, height: 140)
    imageView.layer.cornerRadius = 10
    imageView.layer.masksToBounds = true
    imageView.isOpaque = false

    let size = imagestack.systemLayoutSizeFitting(CGSize(width: imageView.frame.width - 40, height: imageView.frame.height - 40), withHorizontalFittingPriority: .defaultHigh, verticalFittingPriority: .defaultLow)
    imagestack.frame = CGRect(x: imageView.frame.maxX - (size.width + 20), y: imageView.frame.maxY - (size.height + 20), width: size.width, height: size.height)

    titleLabel.frame = CGRect(x: 0, y: imageView.frame.maxY + 5, width: bounds.width, height: 20)

    let voteSize = voteButton.intrinsicContentSize
    voteButton.frame = CGRect(x: 0, y: titleLabel.frame.maxY + 5, width: voteSize.width, height: voteSize.height)

    voteLabel.frame = CGRect(x: voteButton.frame.maxX + 15, y: voteButton.frame.minY, width: bounds.width - (voteButton.frame.maxX + 20), height: voteButton.frame.height)
  }
}

class VenueVoteList: UIControl {
  fileprivate let scrollView = UIScrollView()
  fileprivate var venue1View: VenueVoteView!
  fileprivate var venue2View: VenueVoteView!
  fileprivate var venue3View: VenueVoteView!
  fileprivate var venue4View: VenueVoteView!
  fileprivate var venue5View: VenueVoteView!
  fileprivate let contentView = UIView()

  fileprivate struct C {
    static let cardWidth = 250
    static let cardHeight = 200
    static let cardPadding: CGFloat = 10
    static let contentPadding: CGFloat = 20
  }

  static var height: CGFloat {
    get {
      return CGFloat(C.cardHeight)
    }
  }

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  private(set) var lastVotedVenue: Venue?

  private func setup () {
    venue1View = VenueVoteView()
    venue2View = VenueVoteView()
    venue3View = VenueVoteView()
    venue4View = VenueVoteView()
    venue5View = VenueVoteView()

    [venue1View, venue2View, venue3View, venue4View, venue5View].forEach {
      $0?.addTarget(self, action: #selector(proxyTouchUpInside), for: .touchUpInside)
    }

    scrollView.showsVerticalScrollIndicator = false
    scrollView.alwaysBounceVertical = false

    addSubview(scrollView)

    constrain(self, scrollView) { container, scrollView in
      scrollView.leading == container.leading
      scrollView.trailing == container.trailing
      scrollView.top == container.top
      scrollView.bottom == container.bottom
    }

    setNeedsLayout()
  }

  func clear() {
    scrollView.subviews.forEach { $0.removeFromSuperview() }
  }

  func display(_ venues: [Venue], profile: UserProfile?) {
    lastVotedVenue = nil
    contentView.subviews.forEach { $0.removeFromSuperview() }
    let votedVenueIds = profile?.votedVenueIds ?? []
    var availableContainers: [VenueVoteView] = [
      venue1View,
      venue2View,
      venue3View,
      venue4View,
      venue5View
    ]

    venues.forEach { venue in
      let v = availableContainers.remove(at: 0)
      v.configure(with: venue, voteCount: 0, ownVoteRegistered: votedVenueIds.contains(venue.venueId))
      contentView.addSubview(v)
    }

    scrollView.addSubview(contentView)

    invalidateIntrinsicContentSize()
    setNeedsLayout()
    setNeedsDisplay()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let height = bounds.height

    scrollView.frame = bounds
    contentView.frame = CGRect(x: 0, y: 0, width: Int(C.contentPadding) + Int(C.contentPadding - C.cardPadding) + contentView.subviews.count * (Int(C.cardPadding) + C.cardWidth), height: Int(height))
    scrollView.contentSize = contentView.frame.size

    var x: CGFloat = C.contentPadding
    contentView.subviews.forEach {
      $0.frame = CGRect(x: x, y: 0, width: CGFloat(C.cardWidth), height: height)
      x = $0.frame.maxX + C.cardPadding
    }
  }

  override var intrinsicContentSize: CGSize {
    return CGSize(width: UIView.noIntrinsicMetric, height: CGFloat(C.cardHeight))
  }

  @objc fileprivate func proxyTouchUpInside(sender: VenueVoteView) {
    lastVotedVenue = sender.venue
    sendActions(for: .touchUpInside)
    sender.configure(with: sender.venue!, voteCount: 1, ownVoteRegistered: true)
  }
}

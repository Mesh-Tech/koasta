import Foundation
import UIKit
import Cartography

protocol NoLaunchedVenuesItemViewDelegate: class {
  func votedForVenue(_ venue: Venue?)
}

class NoLaunchedVenuesItemView: UITableViewCell {
  fileprivate let noLaunchedVenuesBackgroundView = UIImageView()
  fileprivate let noLaunchedVenuesTitleView = UILabel()
  fileprivate let noLaunchedVenuesBodyView = UILabel()
  fileprivate let voteView = VenueVoteList()
  fileprivate static let sizingItem = NoLaunchedVenuesItemView()

  weak var delegate: NoLaunchedVenuesItemViewDelegate?

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
    super.init(style: style, reuseIdentifier: reuseIdentifier)
    setup()
  }

  convenience init(reuseIdentifier: String?) {
    self.init(style: .default, reuseIdentifier: reuseIdentifier)
    setup()
  }

  fileprivate convenience init() { self.init(reuseIdentifier: nil) }

  override func prepareForReuse() {
    super.prepareForReuse()
    voteView.clear()
  }

  fileprivate func setup () {
    selectionStyle = .none
    noLaunchedVenuesBackgroundView.image = UIImage(named: "angle_background")
    noLaunchedVenuesBackgroundView.contentMode = .scaleAspectFill
    noLaunchedVenuesTitleView.numberOfLines = 0
    noLaunchedVenuesTitleView.textAlignment = .left
    noLaunchedVenuesTitleView.font = UIFont.brandFont(ofSize: 24, weight: .semibold)
    noLaunchedVenuesTitleView.text = NSLocalizedString("Hi there! 👋", comment: "")
    noLaunchedVenuesTitleView.accessibilityIdentifier = "noLaunchedVenuesTitleView"
    noLaunchedVenuesTitleView.textColor = .chalkColour
    noLaunchedVenuesBodyView.numberOfLines = 0
    noLaunchedVenuesBodyView.textAlignment = .left
    noLaunchedVenuesBodyView.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    noLaunchedVenuesBodyView.textColor = .chalkColour
    noLaunchedVenuesBodyView.text = NSLocalizedString("We've just launched Koasta - an easy and safe way to order at your favourite bars and pubs.\n\nWe're working with local venues around the UK and bringing them onboard. You can help by voting for your favourite venues below.", comment: "")

    voteView.addTarget(self, action: #selector(votedForVenue), for: .touchUpInside)

    [noLaunchedVenuesBackgroundView, noLaunchedVenuesTitleView, noLaunchedVenuesBodyView, voteView].forEach { addSubview($0) }

    setNeedsLayout()
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    let containerWidth = UIScreen.main.bounds.width
    let insetWidth = containerWidth - (40 * 2)

    noLaunchedVenuesTitleView.preferredMaxLayoutWidth = insetWidth
    noLaunchedVenuesBodyView.preferredMaxLayoutWidth = insetWidth

    let titleHeight = noLaunchedVenuesTitleView.systemLayoutSizeFitting(CGSize(width: insetWidth, height: .greatestFiniteMagnitude)).height
    let bodyHeight = noLaunchedVenuesBodyView.systemLayoutSizeFitting(CGSize(width: insetWidth, height: .greatestFiniteMagnitude)).height

    noLaunchedVenuesTitleView.frame = CGRect(x: 40, y: 42, width: insetWidth, height: titleHeight)
    noLaunchedVenuesBodyView.frame = CGRect(x: 40, y: noLaunchedVenuesTitleView.frame.maxY + 10, width: insetWidth, height: bodyHeight)
    voteView.frame = CGRect(x: 0, y: noLaunchedVenuesBodyView.frame.maxY + 60, width: containerWidth, height: voteView.intrinsicContentSize.height)
    noLaunchedVenuesBackgroundView.frame = CGRect(x: 0, y: 0, width: containerWidth, height: noLaunchedVenuesBodyView.frame.maxY + 20)

    voteView.setNeedsLayout()
  }

  func configure (_ votingVenues: [Venue], profile: UserProfile?) -> NoLaunchedVenuesItemView {
    voteView.clear()
    voteView.display(votingVenues, profile: profile)
    setNeedsLayout()
    return self
  }

  static func calculateHeight (for votingVenues: [Venue], profile: UserProfile?) -> CGFloat {
    let sizingView = sizingItem.configure(votingVenues, profile: profile)

    let containerWidth = UIScreen.main.bounds.width
    let insetWidth = containerWidth - (40 * 2)

    sizingView.noLaunchedVenuesTitleView.preferredMaxLayoutWidth = insetWidth
    sizingView.noLaunchedVenuesBodyView.preferredMaxLayoutWidth = insetWidth

    let titleSize = sizingView.noLaunchedVenuesTitleView.systemLayoutSizeFitting(CGSize(width: insetWidth, height: .greatestFiniteMagnitude))
    let bodySize = sizingView.noLaunchedVenuesBodyView.systemLayoutSizeFitting(CGSize(width: insetWidth, height: .greatestFiniteMagnitude))

    return 42 + titleSize.height + 10 + bodySize.height + 60 + sizingView.voteView.intrinsicContentSize.height + 80
  }

  @objc fileprivate func votedForVenue(_ venueId: Int) {
    delegate?.votedForVenue(voteView.lastVotedVenue)
  }
}

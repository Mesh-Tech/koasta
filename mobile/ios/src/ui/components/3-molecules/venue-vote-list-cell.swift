import UIKit

@objc protocol VenueVoteListCellDelegate {
  @objc optional func voteRaised(for: VenueVoteListCell, venueId: Int)
}

class VenueVoteListCell: UITableViewCell {
  fileprivate static var sizingItem = VenueVoteListCell()
  fileprivate let venueVoteList = VenueVoteList()
  fileprivate let title = UILabel()
  fileprivate let subtitle = UILabel()
  weak var delegate: VenueVoteListCellDelegate?

  static func calculateHeight() -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)
    let sizingView = sizingItem.configure([], profile: nil)

    sizingView.title.preferredMaxLayoutWidth = containerWidth
    sizingView.subtitle.preferredMaxLayoutWidth = containerWidth

    sizingView.layoutSubviews()

    let titleHeight = sizingView.title.frame.size.height
    let subtitleHeight = sizingView.subtitle.frame.size.height

    return 16 + titleHeight + 6 + subtitleHeight + 10 + VenueVoteList.height + 10
  }

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

  fileprivate func setup () {
    contentView.addSubview(title)
    contentView.addSubview(subtitle)
    contentView.addSubview(venueVoteList)

    title.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    subtitle.font = UIFont.brandFont(ofSize: 16, weight: .semibold)
    title.numberOfLines = 0
    subtitle.numberOfLines = 0
    subtitle.textColor = UIColor.grey10

    title.text = NSLocalizedString("Can't find your favourite?", comment: "")
    subtitle.text = NSLocalizedString("Vote for any of these nearby venues and we'll do our best to bring them onboard", comment: "")

    setNeedsLayout()

    venueVoteList.addTarget(self, action: #selector(touchUpInsideProxy), for: .touchUpInside)
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    let containerWidth = UIScreen.main.bounds.width - (16 * 2)

    title.preferredMaxLayoutWidth = containerWidth
    subtitle.preferredMaxLayoutWidth = containerWidth

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize

    title.frame = CGRect(x: 16, y: 16, width: titleSize.width, height: titleSize.height)
    subtitle.frame = CGRect(x: 16, y: title.frame.origin.y + titleSize.height + 6, width: subtitleSize.width, height: subtitleSize.height)

    venueVoteList.frame = CGRect(x: 0, y: subtitle.frame.maxY + 10, width: UIScreen.main.bounds.width, height: VenueVoteList.height)
    venueVoteList.setNeedsLayout()
  }

  func configure(_ venues: [Venue], profile: UserProfile?) -> VenueVoteListCell {
    venueVoteList.display(venues, profile: profile)
    return self
  }

  @objc fileprivate func touchUpInsideProxy(sender: VenueVoteList) {
    delegate?.voteRaised?(for: self, venueId: sender.lastVotedVenue!.venueId)
  }
}

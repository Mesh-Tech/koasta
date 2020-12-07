import Foundation
import UIKit
import Kingfisher

class SearchItem: UITableViewCell {
  fileprivate static var sizingHeader = SearchItem()
  fileprivate let title = UILabel()
  fileprivate let body = UILabel()

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

  fileprivate func setup () {
    title.font = UIFont.brandFont(ofSize: 16, weight: .semibold)
    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
    title.setContentCompressionResistancePriority(.defaultHigh, for: .vertical)
    title.textColor = .foregroundColour
    body.font = UIFont.brandFont(ofSize: 16, weight: .regular)
    body.numberOfLines = 0
    body.lineBreakMode = .byWordWrapping
    body.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    body.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    body.textColor = .foregroundColour

    [title, body].forEach { contentView.addSubview($0) }
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (60 + 16)

    body.preferredMaxLayoutWidth = containerWidth
    title.preferredMaxLayoutWidth = containerWidth

    let bodySize = body.intrinsicContentSize
    let titleSize = title.intrinsicContentSize

    title.frame = CGRect(x: 60, y: 20, width: titleSize.width, height: titleSize.height)
    body.frame = CGRect(x: 60, y: title.frame.maxY + 10, width: bodySize.width, height: bodySize.height)
  }

  static func calculateHeight (for venue: Venue) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (60 + 16)
    let sizingView = sizingHeader.configure(with: venue)

    sizingView.title.preferredMaxLayoutWidth = containerWidth
    sizingView.body.preferredMaxLayoutWidth = containerWidth

    let titleHeight = sizingView.title.intrinsicContentSize.height
    let bodyHeight = sizingView.body.intrinsicContentSize.height

    return 19 + titleHeight + 10 + bodyHeight + 26
  }

  func configure (with venue: Venue) -> SearchItem {
    title.text = venue.venueName
    body.text = venue.venueAddress

    title.setNeedsLayout()
    body.setNeedsLayout()
    setNeedsLayout()

    selectionStyle = .none

    return self
  }
}

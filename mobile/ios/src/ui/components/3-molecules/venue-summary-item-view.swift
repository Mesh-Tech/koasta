import Foundation
import UIKit
import Kingfisher

class VenueSummaryItemView: UITableViewCell {
  fileprivate static var sizingItem = VenueSummaryItemView()
  fileprivate let title = UILabel()
  fileprivate let subtitle = UILabel()
  fileprivate var border = UIView()

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
    title.numberOfLines = 1
    title.lineBreakMode = .byClipping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    title.accessibilityIdentifier = "venueTitle"

    subtitle.numberOfLines = 0
    subtitle.lineBreakMode = .byWordWrapping
    subtitle.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    subtitle.setContentHuggingPriority(.defaultHigh, for: .vertical)
    subtitle.font = UIFont.brandFont(ofSize: 16, weight: .regular)
    subtitle.accessibilityIdentifier = "venueSubtitle"

    border.backgroundColor = UIColor.grey15

    addSubview(title)
    addSubview(subtitle)
    addSubview(border)

    title.setNeedsLayout()
    setNeedsLayout()

    selectionStyle = .none
  }

  func configure (with venue: Venue?) -> VenueSummaryItemView {
    title.text = venue?.venueName ?? ""
    subtitle.text = venue?.venueDescription ?? ""
    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let screenWidth = UIScreen.main.bounds.width
    let containerWidth = screenWidth - (18 * 2)

    title.preferredMaxLayoutWidth = containerWidth
    subtitle.preferredMaxLayoutWidth = containerWidth

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize

    title.frame = CGRect(x: 18, y: 14, width: titleSize.width, height: titleSize.height)
    subtitle.frame = CGRect(x: 18, y: title.frame.maxY + 4, width: subtitleSize.width, height: subtitleSize.height)

    border.frame = CGRect(x: 0, y: bounds.height - 0.5, width: bounds.width, height: 0.5)
  }

  static func calculateHeight (for venue: Venue?) -> CGFloat {
    _ = sizingItem.configure(with: venue)
    let screenWidth = UIScreen.main.bounds.width
    let containerWidth = screenWidth - (18 * 2)

    let titleSize = sizingItem.title.systemLayoutSizeFitting(CGSize(width: containerWidth, height: .greatestFiniteMagnitude))
    let subtitleSize = sizingItem.subtitle.systemLayoutSizeFitting(CGSize(width: containerWidth, height: .greatestFiniteMagnitude))

    return 14 + titleSize.height + 4 + subtitleSize.height + 14
  }
}

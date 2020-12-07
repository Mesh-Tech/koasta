import Foundation
import UIKit
import Kingfisher

class SettingsAccountItem: UITableViewCell {
  fileprivate static var sizingHeader = SettingsAccountItem()
  fileprivate let title = UILabel()
  fileprivate let subtitle = UILabel()

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
    title.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    title.lineBreakMode = .byTruncatingTail
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
    title.setContentCompressionResistancePriority(.defaultHigh, for: .vertical)
    title.textColor = UIColor.foregroundColour
    title.numberOfLines = 1
    subtitle.font = UIFont.brandFont(ofSize: 14)
    subtitle.lineBreakMode = .byTruncatingTail
    subtitle.setContentHuggingPriority(.defaultLow, for: .horizontal)
    subtitle.setContentHuggingPriority(.defaultHigh, for: .vertical)
    subtitle.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
    subtitle.setContentCompressionResistancePriority(.defaultHigh, for: .vertical)
    subtitle.textColor = UIColor.grey10
    subtitle.numberOfLines = 1

    [title, subtitle].forEach { contentView.addSubview($0) }

    accessoryType = .none
    separatorInset = UIEdgeInsets.zero
    layoutMargins = UIEdgeInsets.zero
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (15 * 2)

    title.preferredMaxLayoutWidth = containerWidth
    subtitle.preferredMaxLayoutWidth = containerWidth

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize

    title.frame = CGRect(x: 15, y: 15, width: titleSize.width, height: titleSize.height)
    subtitle.frame = CGRect(x: 15, y: title.frame.maxY + 10, width: subtitleSize.width, height: subtitleSize.height)
  }

  static func calculateHeight(withTitle titleText: String, subtitleText: String) -> CGFloat {
    _ = sizingHeader.configure(withTitle: titleText, subtitleText: subtitleText)

    let titleSize = sizingHeader.title.intrinsicContentSize
    let subtitleSize = sizingHeader.subtitle.intrinsicContentSize

    return 15 + titleSize.height + 10 + subtitleSize.height + 15
  }

  func configure (withTitle titleText: String, subtitleText: String) -> SettingsAccountItem {
    title.text = titleText
    subtitle.text = subtitleText

    title.setNeedsLayout()
    subtitle.setNeedsLayout()

    setNeedsLayout()

    return self
  }
}

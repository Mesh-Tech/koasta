import Foundation
import UIKit
import Kingfisher

class SettingsItem: UITableViewCell {
  fileprivate static var sizingHeader = SettingsItem()
  fileprivate let title = UILabel()
  fileprivate let thumb = UIImageView()

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
    title.font = UIFont.brandFont(ofSize: 16)
    title.lineBreakMode = .byTruncatingTail
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
    title.setContentCompressionResistancePriority(.defaultHigh, for: .vertical)
    title.textColor = UIColor.foregroundColour
    title.numberOfLines = 1
    thumb.contentMode = .scaleAspectFill
    thumb.layer.cornerRadius = 6
    thumb.layer.masksToBounds = true
    thumb.backgroundColor = UIColor.grey1

    [title, thumb].forEach { contentView.addSubview($0) }

    accessoryType = .disclosureIndicator
    separatorInset = UIEdgeInsets(top: 0, left: 15 + 30 + 15, bottom: 0, right: 0)
    layoutMargins = UIEdgeInsets.zero
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - ((15 + 30 + 15) * 2)

    title.preferredMaxLayoutWidth = containerWidth

    let titleSize = title.intrinsicContentSize

    title.frame = CGRect(x: 15 + 30 + 15, y: 14, width: titleSize.width, height: titleSize.height)
    thumb.frame = CGRect(x: 15, y: 8, width: 30, height: 30)
  }

  static func calculateHeight() -> CGFloat {
    return 8 + 30 + 8
  }

  func configure (withTitle titleText: String, imageName: String) -> SettingsItem {
    title.text = titleText

    title.setNeedsLayout()
    thumb.image = UIImage(named: imageName)

    setNeedsLayout()

    return self
  }

  func configure (withTitle titleText: String, image: UIImage) -> SettingsItem {
    title.text = titleText

    title.setNeedsLayout()
    thumb.image = image

    setNeedsLayout()

    return self
  }
}

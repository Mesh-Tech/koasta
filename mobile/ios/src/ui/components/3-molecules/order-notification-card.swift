import Foundation
import UIKit
import Kingfisher

@objc protocol OrderNotificationCardItemViewDelegate {
  @objc optional func notificationCardItemDidRequestNotifications(_ item: OrderNotificationCardItemView)
}

class OrderNotificationCardItemView: UITableViewCell {
  fileprivate static var sizingItem = OrderNotificationCardItemView()
  fileprivate let thumb = UIImageView()
  fileprivate let title = UILabel()
  fileprivate let locationIcon = UIImageView()
  fileprivate let subtitle = UILabel()
  fileprivate let allowButton = UIButton(type: .system)
  weak var delegate: OrderNotificationCardItemViewDelegate?

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
    thumb.image = UIImage(named: "notification")

    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.font = UIFont.preferredBrandFont(forTextStyle: UIFont.TextStyle.headline)
    title.text = NSLocalizedString("Track your order", comment: "")

    subtitle.numberOfLines = 0
    subtitle.lineBreakMode = .byWordWrapping
    subtitle.setContentHuggingPriority(.defaultLow, for: .horizontal)
    subtitle.setContentHuggingPriority(.defaultHigh, for: .vertical)
    subtitle.font = UIFont.brandFont(ofSize: 15)
    subtitle.textColor = UIColor.grey10
    subtitle.text = NSLocalizedString("Get notifications about your order's progress", comment: "")

    allowButton.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    allowButton.setContentHuggingPriority(.defaultHigh, for: .vertical)
    allowButton.tintColor = UIColor.brandColour
    allowButton.setTitle(NSLocalizedString("Keep me updated", comment: ""), for: .normal)
    allowButton.titleLabel?.font = UIFont.brandFont(ofSize: 16, weight: .bold)

    addSubview(thumb)
    addSubview(title)
    addSubview(subtitle)
    addSubview(allowButton)

    allowButton.addTarget(self, action: #selector(buttonTapped), for: .touchUpInside)
  }

  func configure () -> OrderNotificationCardItemView {
    selectionStyle = .none
    thumb.setNeedsLayout()
    title.setNeedsLayout()
    subtitle.setNeedsLayout()
    allowButton.setNeedsLayout()
    setNeedsLayout()

    return self
  }

  override func prepareForReuse() {
    super.prepareForReuse()
    delegate = nil
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    thumb.frame = CGRect(x: 16, y: 32, width: 58, height: 55)

    title.preferredMaxLayoutWidth = UIScreen.main.bounds.width - (thumb.frame.maxX + 20 + 16)
    subtitle.preferredMaxLayoutWidth = UIScreen.main.bounds.width - (thumb.frame.maxX + 20 + 16)

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize
    let commentSize = allowButton.intrinsicContentSize

    title.frame = CGRect(x: thumb.frame.maxX + 20, y: 32, width: titleSize.width, height: titleSize.height)
    subtitle.frame = CGRect(x: thumb.frame.maxX + 20, y: title.frame.origin.y + titleSize.height + 6, width: subtitleSize.width, height: subtitleSize.height)
    allowButton.frame = CGRect(x: thumb.frame.maxX + 20, y: subtitle.frame.origin.y + subtitleSize.height + 6, width: commentSize.width, height: commentSize.height)
  }

  static func calculateHeight () -> CGFloat {
    let sizingView = sizingItem.configure()

    sizingView.title.preferredMaxLayoutWidth = UIScreen.main.bounds.width - (sizingView.thumb.frame.maxX + 20 + 16)
    sizingView.subtitle.preferredMaxLayoutWidth = UIScreen.main.bounds.width -  (sizingView.thumb.frame.maxX + 20 + 16)

    sizingView.layoutSubviews()

    let titleHeight = sizingView.title.frame.size.height
    let subtitleHeight = sizingView.subtitle.frame.size.height
    let commentHeight = sizingView.allowButton.frame.size.height

    return 32 + titleHeight + 6 + subtitleHeight + 6 + commentHeight + 20
  }

  @objc fileprivate func buttonTapped () {
    delegate?.notificationCardItemDidRequestNotifications?(self)
  }
}

import Foundation
import UIKit
import Kingfisher

class OrderStatusCardItemView: UITableViewCell {
  fileprivate static var sizingItem = OrderStatusCardItemView()
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
    contentView.backgroundColor = UIColor.backgroundColour

    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.font = UIFont.preferredBrandFont(forTextStyle: UIFont.TextStyle.headline)
    title.textColor = .foregroundColour

    subtitle.numberOfLines = 0
    subtitle.lineBreakMode = .byWordWrapping
    subtitle.setContentHuggingPriority(.defaultLow, for: .horizontal)
    subtitle.setContentHuggingPriority(.defaultHigh, for: .vertical)
    subtitle.font = UIFont.brandFont(ofSize: 15)
    subtitle.textColor = UIColor.grey10
    addSubview(title)
    addSubview(subtitle)
  }

  override func prepareForReuse() {
    super.prepareForReuse()
    title.text = nil
    subtitle.text = nil
  }

  func configure (status: OrderStatus, servingType: VenueServingType) -> OrderStatusCardItemView {
    switch status {
    case .ordered:
      title.text = NSLocalizedString("Your order has been placed 👍", comment: "")
      subtitle.text = NSLocalizedString("Sit back and relax, we'll keep you updated on its progress.", comment: "")
    case .inProgress:
      title.text = NSLocalizedString("Your order has been confirmed ✅", comment: "")
      subtitle.text = NSLocalizedString("Sit back and relax, we'll keep you updated on its progress.", comment: "")
    case .ready:
      title.text = NSLocalizedString("Your order is ready 🚀", comment: "")
      switch servingType {
      case .tableService:
        subtitle.text = NSLocalizedString("Your order will be brought to your table shortly.", comment: "")
      default:
        subtitle.text = NSLocalizedString("You can now head to the bar. Just show a member of staff this screen to pick it up.", comment: "")
      }
    case .rejected:
      title.text = NSLocalizedString("Sorry, there might be a problem with your order", comment: "")
      subtitle.text = NSLocalizedString("Please see a member of staff to find out more.", comment: "")
    case .complete:
      title.text = NSLocalizedString("You received your order 🎉", comment: "")
      subtitle.text = NSLocalizedString("Nice job beating that queue! Thanks for ordering with Koasta.", comment: "")
    case .paymentPending:
      title.text = NSLocalizedString("Sorry, there might be a problem with your order", comment: "")
      subtitle.text = NSLocalizedString("We haven't received payment yet. This usually means there was a problem attempting to take payment. Please check with your bank and try again.", comment: "")
    case .paymentFailed:
      title.text = NSLocalizedString("Sorry, there might be a problem with your order", comment: "")
      subtitle.text = NSLocalizedString("We weren't able to take payment with the details you provided. Please check with your bank and try again.", comment: "")
    default:
      title.text = nil
      subtitle.text = nil
    }

    if status == .ordered || status == .inProgress {
      var addendumText = ""

      switch servingType {
      case .tableService:
        addendumText = NSLocalizedString("Your order will be brought to your table.", comment: "")
      default:
        addendumText = NSLocalizedString("You can pick up your order from the bar when it's ready.", comment: "")
      }

      subtitle.text = "\(subtitle.text ?? "")\n\n\(addendumText)"
    }

    selectionStyle = .none
    title.setNeedsLayout()
    subtitle.setNeedsLayout()
    setNeedsLayout()

    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    title.preferredMaxLayoutWidth = UIScreen.main.bounds.width - (16 * 2)
    subtitle.preferredMaxLayoutWidth = UIScreen.main.bounds.width - (16 * 2)

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize

    title.frame = CGRect(x: 16, y: 32, width: titleSize.width, height: titleSize.height)
    subtitle.frame = CGRect(x: 16, y: title.frame.origin.y + titleSize.height + 6, width: subtitleSize.width, height: subtitleSize.height)
  }

  static func calculateHeight (status: OrderStatus, servingType: VenueServingType) -> CGFloat {
    let sizingView = sizingItem.configure(status: status, servingType: servingType)

    sizingView.title.preferredMaxLayoutWidth = UIScreen.main.bounds.width - (16 * 2)
    sizingView.subtitle.preferredMaxLayoutWidth = UIScreen.main.bounds.width -  (16 * 2)

    sizingView.layoutSubviews()

    let titleHeight = sizingView.title.frame.size.height
    let subtitleHeight = sizingView.subtitle.frame.size.height

    return 32 + titleHeight + 6 + subtitleHeight + 20
  }
}

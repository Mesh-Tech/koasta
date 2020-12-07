import Foundation
import UIKit

class HistoricalOrderItemView: UITableViewCell {
  fileprivate static var sizingItem = HistoricalOrderItemView()
  fileprivate let title = UILabel()
  fileprivate let subtitle = UILabel()
  fileprivate let comment = UILabel()
  fileprivate let price = UILabel()

  fileprivate let formatter: NumberFormatter = {
    let formatter = NumberFormatter()
    formatter.alwaysShowsDecimalSeparator = true
    formatter.currencyCode = "GBP"
    formatter.currencySymbol = "£"
    formatter.decimalSeparator = "."
    formatter.currencyGroupingSeparator = ","
    formatter.generatesDecimalNumbers = true
    formatter.groupingSize = 3
    formatter.numberStyle = .currency

    return formatter
  }()

  fileprivate struct C {
    static let topMargin: CGFloat = 18
    static let margin: CGFloat = 16
    static let bottomMargin: CGFloat = 20
    static let titleBottomMargin: CGFloat = 6
    static let subtitleBottomMargin: CGFloat = 10
    static let commentRightMargin: CGFloat = 10
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

  fileprivate convenience init() { self.init(reuseIdentifier: nil) }

  fileprivate func setup () {
    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.font = UIFont.brandFont(ofSize: 18, weight: .semibold)
    title.textColor = UIColor.foregroundColour
    title.accessibilityIdentifier = "historicalOrderItemTitle"

    subtitle.numberOfLines = 0
    subtitle.lineBreakMode = .byWordWrapping
    subtitle.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    subtitle.textColor = UIColor.grey5
    subtitle.accessibilityIdentifier = "historicalOrderItemSubtitle"

    comment.numberOfLines = 1
    comment.lineBreakMode = .byClipping
    comment.font = UIFont.boldBrandFont(ofSize: 14)
    comment.accessibilityIdentifier = "historicalOrderItemComment"

    price.numberOfLines = 1
    price.lineBreakMode = .byClipping
    price.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    price.textColor = UIColor.foregroundColour
    price.accessibilityIdentifier = "historicalOrderItemPrice"

    addSubview(price)
    addSubview(title)
    addSubview(subtitle)
    addSubview(comment)
  }

  func configure (with order: HistoricalOrder) -> HistoricalOrderItemView {
    title.text = order.venueName
    let subtitleText = order.lineItems.count == 1
      ? NSLocalizedString("1 item", comment: "")
      : NSLocalizedString("\(order.lineItems.count) items", comment: "")

    let subtitleText2 = order.orderTimeStamp.timeAgoSinceDate(date: Date(), numericDates: false)

    subtitle.text = "\(subtitleText) · \(subtitleText2)"

    let status = OrderStatus(rawValue: order.orderStatus) ?? OrderStatus.unknown
      switch status {
      case .unknown:
        comment.text = NSLocalizedString("Order status unknown", comment: "")
        comment.textColor = UIColor.grey5
      case .ordered:
        comment.text = NSLocalizedString("Order Pending", comment: "")
        comment.textColor = UIColor.grey5
      case .inProgress:
        comment.text = NSLocalizedString("In Progress", comment: "")
        comment.textColor = UIColor.orangeColour
      case .ready:
        comment.text = NSLocalizedString("Ready to Collect", comment: "")
        comment.textColor = UIColor.paleGreenColour
      case .rejected:
        comment.text = NSLocalizedString("Order Cancelled", comment: "")
        comment.textColor = UIColor.brightBrandColour
      case .complete:
        comment.text = NSLocalizedString("Order Fulfilled", comment: "")
        comment.textColor = UIColor.paleGreenColour
      case .paymentPending:
        comment.text = NSLocalizedString("Incomplete", comment: "")
        comment.textColor = UIColor.grey5
      case .paymentFailed:
        comment.text = NSLocalizedString("Incomplete", comment: "")
        comment.textColor = UIColor.brightBrandColour
    }

    let totalAmount = order.total

    price.text = formatter.string(from: totalAmount as NSDecimalNumber)

    title.setNeedsLayout()
    subtitle.setNeedsLayout()
    comment.setNeedsLayout()
    price.setNeedsLayout()
    setNeedsLayout()

    accessoryType = .disclosureIndicator

    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (C.margin * 2)
    let halfContainerWidth = containerWidth / 2

    title.preferredMaxLayoutWidth = containerWidth
    subtitle.preferredMaxLayoutWidth = containerWidth
    comment.preferredMaxLayoutWidth = halfContainerWidth
    price.preferredMaxLayoutWidth = halfContainerWidth

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize
    let commentSize = comment.intrinsicContentSize
    let priceSize = price.intrinsicContentSize

    title.frame = CGRect(x: C.margin, y: C.topMargin, width: titleSize.width, height: titleSize.height)
    subtitle.frame = CGRect(x: title.frame.minX, y: title.frame.maxY + C.titleBottomMargin, width: subtitleSize.width, height: subtitleSize.height)
    comment.frame = CGRect(x: title.frame.minX, y: subtitle.frame.maxY + C.subtitleBottomMargin, width: min(halfContainerWidth, commentSize.width), height: commentSize.height)
    price.frame = CGRect(x: (UIScreen.main.bounds.width - C.margin - priceSize.width), y: comment.frame.minY, width: priceSize.width, height: priceSize.height)
  }

  static func calculateHeight (for order: HistoricalOrder) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)
    let sizingView = sizingItem.configure(with: order)

    sizingView.title.preferredMaxLayoutWidth = containerWidth
    sizingView.subtitle.preferredMaxLayoutWidth = containerWidth
    sizingView.comment.preferredMaxLayoutWidth = containerWidth - 100

    sizingView.layoutSubviews()

    let titleHeight = sizingView.title.frame.size.height
    let subtitleHeight = sizingView.subtitle.frame.size.height
    let commentHeight = sizingView.comment.frame.size.height

    return C.topMargin + titleHeight + C.titleBottomMargin + subtitleHeight + C.subtitleBottomMargin + commentHeight + C.bottomMargin
  }
}

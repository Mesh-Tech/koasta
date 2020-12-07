import Foundation
import UIKit
import Kingfisher
import Cartography

private class OrderSummaryLineItem: UIView {
  fileprivate static var sizingHeader = OrderSummaryLineItem()
  fileprivate static let amountFormatter: NumberFormatter = {
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
    static let orderItemLeftPadding: CGFloat = 15
    static let orderItemRightPadding: CGFloat = 15
    static let orderItemBottomPadding: CGFloat = 15
  }
  let leftLabel = UILabel()
  let rightLabel = UILabel()

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  fileprivate func setup () {
    leftLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    leftLabel.numberOfLines = 0

    rightLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    rightLabel.numberOfLines = 1
    rightLabel.textAlignment = .right
  }

  func configure (item: HistoricalOrderItem) -> OrderSummaryLineItem {
    if item.quantity == 0 {
      leftLabel.text = item.productName
      rightLabel.text = OrderSummaryLineItem.amountFormatter.string(from: item.amount as NSDecimalNumber)
    } else {
      leftLabel.text = "\(item.quantity) x \(item.productName)"
      rightLabel.text = OrderSummaryLineItem.amountFormatter.string(from: (Decimal(item.quantity) * item.amount) as NSDecimalNumber)
    }

    addSubview(leftLabel)
    addSubview(rightLabel)

    setNeedsLayout()
    invalidateIntrinsicContentSize()

    return self
  }

  static func calculateHeight (item: HistoricalOrderItem) -> CGFloat {
    let sizingView = sizingHeader.configure(item: item)

    return sizingView.intrinsicContentSize.height
  }

  override var intrinsicContentSize: CGSize {
    let containerWidth = UIScreen.main.bounds.width - (C.orderItemLeftPadding + C.orderItemRightPadding)

    let priceSize = rightLabel.systemLayoutSizeFitting(CGSize(width: containerWidth, height: .greatestFiniteMagnitude))
    let labelSize = leftLabel.systemLayoutSizeFitting(CGSize(width: containerWidth - (priceSize.width + 20), height: .greatestFiniteMagnitude))

    rightLabel.preferredMaxLayoutWidth = priceSize.width + 20
    leftLabel.preferredMaxLayoutWidth = labelSize.width

    return CGSize(width: UIScreen.main.bounds.width, height: max(leftLabel.intrinsicContentSize.height, rightLabel.intrinsicContentSize.height) + C.orderItemBottomPadding)
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (C.orderItemLeftPadding + C.orderItemRightPadding)

    let priceSize = rightLabel.systemLayoutSizeFitting(CGSize(width: containerWidth, height: .greatestFiniteMagnitude))
    let labelSize = leftLabel.systemLayoutSizeFitting(CGSize(width: containerWidth - (priceSize.width + 20), height: .greatestFiniteMagnitude))

    rightLabel.preferredMaxLayoutWidth = priceSize.width + 20
    leftLabel.preferredMaxLayoutWidth = labelSize.width

    leftLabel.frame = CGRect(x: C.orderItemLeftPadding, y: 0, width: leftLabel.preferredMaxLayoutWidth, height: leftLabel.intrinsicContentSize.height)
    rightLabel.frame = CGRect(x: UIScreen.main.bounds.width - rightLabel.preferredMaxLayoutWidth, y: 0, width: priceSize.width, height: rightLabel.intrinsicContentSize.height)
  }
}

class OrderSummaryItem: UITableViewCell {
  fileprivate static var sizingHeader = OrderSummaryItem()
  fileprivate let titleLabel = UILabel()
  fileprivate let orderNumberLabel = UILabel()
  fileprivate let bodyLabel = UILabel()
  fileprivate let receiptBorder = UIView()
  fileprivate let quantityLabel = UILabel()
  fileprivate let amountLabel = UILabel()
  fileprivate let contentBorder = UIView()
  fileprivate var lineItems: [UIView] = []

  fileprivate var dateFormatter: DateFormatter!
  fileprivate var detailDateFormatter: DateFormatter!
  fileprivate let amountFormatter: NumberFormatter = {
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
    static let titleLabelTopPadding: CGFloat = 20
    static let titleLabelLeftPadding: CGFloat = 15
    static let titleLabelRightPadding: CGFloat = 15
    static let orderNumberLabelTopPadding: CGFloat = 6
    static let orderNumberLabelLeftPadding: CGFloat = 15
    static let orderNumberLabelRightPadding: CGFloat = 15
    static let bodyLabelTopPadding: CGFloat = 6
    static let bodyLabelLeftPadding: CGFloat = 15
    static let bodyLabelRightPadding: CGFloat = 15
    static let orderItemStackViewTopPadding: CGFloat = 15
    static let receiptTopPadding: CGFloat = 0
    static let receiptLeftPadding: CGFloat = 15
    static let receiptRightPadding: CGFloat = 0
    static let quantityLabelTopPadding: CGFloat = 18
    static let quantityLabelLeftPadding: CGFloat = 15
    static let quantityLabelRightPadding: CGFloat = 15
    static let amountLabelTopPadding: CGFloat = 15
    static let amountLabelLeftPadding: CGFloat = 15
    static let amountLabelRightPadding: CGFloat = 15
    static let bottomPadding: CGFloat = 30
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
    dateFormatter = DateFormatter()
    dateFormatter.locale = Locale(identifier: "en_US_POSIX")
    dateFormatter.dateFormat = "h:mm a"
    dateFormatter.amSymbol = "AM"
    dateFormatter.pmSymbol = "PM"

    detailDateFormatter = DateFormatter()
    detailDateFormatter.locale = Locale(identifier: "en_US_POSIX")
    detailDateFormatter.dateFormat = "dd/MM/yyyy '·' h:mm a"
    detailDateFormatter.amSymbol = "AM"
    detailDateFormatter.pmSymbol = "PM"

    contentView.addSubview(titleLabel)
    contentView.addSubview(orderNumberLabel)
    contentView.addSubview(bodyLabel)
    contentView.addSubview(receiptBorder)
    contentView.addSubview(quantityLabel)
    contentView.addSubview(amountLabel)
    contentView.addSubview(contentBorder)

    contentView.backgroundColor = UIColor.clear
    titleLabel.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    titleLabel.numberOfLines = 0
    titleLabel.textColor = UIColor.foregroundColour
    orderNumberLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    orderNumberLabel.textColor = UIColor.grey11
    orderNumberLabel.numberOfLines = 1
    bodyLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    bodyLabel.textColor = UIColor.grey11
    bodyLabel.numberOfLines = 0
    quantityLabel.font = UIFont.brandFont(ofSize: 16, weight: .medium)
    quantityLabel.textColor = UIColor.grey11
    quantityLabel.numberOfLines = 1
    amountLabel.font = UIFont.brandFont(ofSize: 20, weight: .semibold)
    amountLabel.textAlignment = .right
    amountLabel.numberOfLines = 1
    receiptBorder.backgroundColor = UIColor.grey12
    contentBorder.backgroundColor = UIColor.grey13

    selectedBackgroundView = UIView()
    selectedBackgroundView?.backgroundColor = UIColor.clear
  }

  static func calculateHeight (order: HistoricalOrder) -> CGFloat {
    let sizingView = sizingHeader.configure(order: order, ignoringLineItems: true)

    var height: CGFloat
      = C.titleLabelTopPadding
        + sizingView.titleLabel.intrinsicContentSize.height
        + C.orderNumberLabelTopPadding
        + sizingView.orderNumberLabel.intrinsicContentSize.height
        + C.bodyLabelTopPadding
        + sizingView.bodyLabel.intrinsicContentSize.height
        + C.orderItemStackViewTopPadding

    height = height + order.lineItems.map {
      OrderSummaryLineItem.calculateHeight(item: $0)
    }.reduce(0) {
      $0 + $1
    }

    height = height
      + C.receiptTopPadding
      + max(sizingView.amountLabel.intrinsicContentSize.height, sizingView.quantityLabel.intrinsicContentSize.height)
      + C.bottomPadding

    return height
  }

  func configure (order: HistoricalOrder, ignoringLineItems: Bool = false) -> OrderSummaryItem {
    invalidateIntrinsicContentSize()
    setNeedsLayout()
    selectionStyle = .none

    let formattedDetailDate = detailDateFormatter.string(from: order.orderTimeStamp)

    titleLabel.text = order.venueName
    bodyLabel.text = NSLocalizedString("Order placed: \(formattedDetailDate)", comment: "")
    orderNumberLabel.text = "\(order.orderNumber)"

    let count = order.lineItems.filter { $0.quantity > 0 }.map { $0.quantity }.reduce(0, +)
    quantityLabel.text = count == 1 ? "1 item" : "\(count) items"
    let totalAmount = order.total
    if totalAmount == 0 {
      amountLabel.text = NSLocalizedString("Free", comment: "")
    } else {
      amountLabel.text = amountFormatter.string(from: totalAmount as NSDecimalNumber)
    }

    if ignoringLineItems {
      return self
    }

    lineItems = order.lineItems.map {
      let v = buildSubStackView($0)
      contentView.addSubview(v)
      return v
    }

    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width

    titleLabel.frame = CGRect(x: C.titleLabelLeftPadding, y: C.titleLabelTopPadding, width: containerWidth - (C.titleLabelLeftPadding + C.titleLabelRightPadding), height: titleLabel.intrinsicContentSize.height)
    orderNumberLabel.frame = CGRect(x: C.orderNumberLabelLeftPadding, y: titleLabel.frame.maxY + C.orderNumberLabelTopPadding, width: containerWidth - (C.orderNumberLabelLeftPadding + C.orderNumberLabelRightPadding), height: orderNumberLabel.intrinsicContentSize.height)
    bodyLabel.frame = CGRect(x: C.bodyLabelLeftPadding, y: orderNumberLabel.frame.maxY + C.bodyLabelTopPadding, width: containerWidth - (C.bodyLabelLeftPadding + C.bodyLabelRightPadding), height: bodyLabel.intrinsicContentSize.height)

    var lastMaxY = bodyLabel.frame.maxY + C.orderItemStackViewTopPadding
    lineItems.forEach {
      let size = $0.intrinsicContentSize
      $0.frame = CGRect(x: 0, y: lastMaxY, width: size.width, height: size.height)
      lastMaxY = $0.frame.maxY
    }

    let receiptY = (lineItems.last?.frame.maxY ?? bodyLabel.frame.maxY) + C.receiptTopPadding

    receiptBorder.frame = CGRect(x: C.receiptLeftPadding, y: receiptY, width: containerWidth - (C.receiptLeftPadding + C.receiptRightPadding), height: 0.5)
    amountLabel.frame = CGRect(x: C.amountLabelLeftPadding, y: receiptBorder.frame.maxY + C.amountLabelTopPadding, width: containerWidth - (C.amountLabelLeftPadding + C.amountLabelRightPadding), height: amountLabel.intrinsicContentSize.height)
    quantityLabel.frame = CGRect(x: C.quantityLabelLeftPadding, y: receiptBorder.frame.maxY + C.quantityLabelTopPadding, width: containerWidth - (C.quantityLabelLeftPadding + C.quantityLabelRightPadding), height: quantityLabel.intrinsicContentSize.height)
  }

  fileprivate func buildSubStackView(_ lineItem: HistoricalOrderItem) -> OrderSummaryLineItem {
    return OrderSummaryLineItem().configure(item: lineItem)
  }

  override func prepareForReuse() {
    super.prepareForReuse()
    lineItems.forEach { $0.removeFromSuperview() }
    lineItems = []
  }
}

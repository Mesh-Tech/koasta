import Foundation
import UIKit
import Kingfisher

class ReceiptFooterView: UITableViewCell {
  fileprivate static var sizingHeader = ReceiptFooterView()
  fileprivate let price = UILabel()
  fileprivate let quantity = UILabel()
  fileprivate let border = UIView()
  fileprivate static let formatter: NumberFormatter = {
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
  fileprivate static let plainFormatter: NumberFormatter = {
    let formatter = NumberFormatter()
    formatter.alwaysShowsDecimalSeparator = false
    formatter.numberStyle = .none

    return formatter
  }()

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
    quantity.font = UIFont.brandFont(ofSize: 16)
    quantity.numberOfLines = 0
    quantity.lineBreakMode = .byWordWrapping
    quantity.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    quantity.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    quantity.textColor = UIColor.grey14

    price.font = UIFont.boldBrandFont(ofSize: 20)
    price.numberOfLines = 0
    price.lineBreakMode = .byWordWrapping
    price.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    price.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    price.textColor = UIColor.foregroundColour

    border.backgroundColor = UIColor.grey13

    [border, quantity, price].forEach { contentView.addSubview($0) }

    selectedBackgroundView = UIView()
    selectedBackgroundView?.backgroundColor = UIColor.clear
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = bounds.width - (60 + 16)

    price.preferredMaxLayoutWidth = containerWidth
    quantity.preferredMaxLayoutWidth = containerWidth

    let priceSize = price.intrinsicContentSize
    let quantitySize = quantity.intrinsicContentSize

    price.frame = CGRect(x: bounds.width - (priceSize.width + 16), y: 16, width: priceSize.width, height: priceSize.height)
    quantity.frame = CGRect(x: 16, y: 20, width: quantitySize.width, height: quantitySize.height)
    border.frame = CGRect(x: 0, y: bounds.height - 0.5, width: bounds.width, height: 0.5)
  }

  static func calculateHeight (for estimate: EstimateOrderResult) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)
    let sizingView = sizingHeader.configure(with: estimate)

    sizingView.price.preferredMaxLayoutWidth = containerWidth
    sizingView.quantity.preferredMaxLayoutWidth = containerWidth

    let priceHeight = sizingView.price.intrinsicContentSize.height

    return 16 + priceHeight + 14
  }

  func configure (with estimate: EstimateOrderResult) -> ReceiptFooterView {
    let quantityVal = estimate.receiptLines.reduce(0) { cur, next in
      return cur + next.quantity
    }

    let priceVal = estimate.receiptTotal

    let quantityDescription = ReceiptFooterView.plainFormatter.string(from: quantityVal as NSNumber) ?? ""

    quantity.text = "\(quantityDescription) \(quantityVal == 1 ? "item" : "items")"

    if priceVal > Decimal(0) {
      price.text = ReceiptFooterView.formatter.string(from: priceVal as NSDecimalNumber)
    } else {
      price.text = NSLocalizedString("Free", comment: "")
    }

    price.setNeedsLayout()
    quantity.setNeedsLayout()
    setNeedsLayout()

    selectionStyle = .none

    return self
  }
}

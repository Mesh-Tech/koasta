import Foundation
import UIKit
import Kingfisher

class ReceiptLineItem: UITableViewCell {
  fileprivate static var sizingHeader = ReceiptLineItem()
  fileprivate var quantityHidden = false
  fileprivate let title = UILabel()
  fileprivate let price = UILabel()
  fileprivate var quantity = UILabel()
  fileprivate var quantityHighlight: UIView!
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
    quantityHighlight = UIView()
    quantityHighlight.backgroundColor = UIColor.grey1
    quantityHighlight.layer.masksToBounds = true
    quantityHighlight.layer.cornerRadius = 7

    quantity.font = UIFont.boldBrandFont(ofSize: 16)
    quantity.numberOfLines = 0
    quantity.lineBreakMode = .byWordWrapping
    quantity.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    quantity.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    quantity.textColor = UIColor.brandColour
    title.font = UIFont.brandFont(ofSize: 16)
    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
    title.setContentCompressionResistancePriority(.defaultHigh, for: .vertical)
    title.textColor = UIColor.foregroundColour
    price.font = UIFont.brandFont(ofSize: 16)
    price.numberOfLines = 0
    price.lineBreakMode = .byWordWrapping
    price.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    price.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    price.textColor = UIColor.foregroundColour

    [quantityHighlight, quantity, title, price].forEach { contentView.addSubview($0) }

    selectedBackgroundView = UIView()
    selectedBackgroundView?.backgroundColor = UIColor.clear
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (60 + 16)

    var priceWidth = price.systemLayoutSizeFitting(CGSize(width: containerWidth, height: .greatestFiniteMagnitude)).width
    var quantityWidth = quantityHidden ? 0 : quantity.systemLayoutSizeFitting(CGSize(width: containerWidth - priceWidth, height: .greatestFiniteMagnitude)).width
    let titleWidth = containerWidth - (priceWidth + quantityWidth + 32 + 20)

    title.preferredMaxLayoutWidth = titleWidth
    price.preferredMaxLayoutWidth = priceWidth
    quantity.preferredMaxLayoutWidth = quantityWidth

    let priceSize = price.intrinsicContentSize
    let titleSize = title.intrinsicContentSize
    let quantitySize = quantity.intrinsicContentSize

    if quantityWidth > 0 {
      quantityWidth += 16 + 20
    }
    priceWidth += 16

    title.frame = CGRect(x: 16 + quantityWidth, y: 20, width: titleSize.width, height: titleSize.height)
    price.frame = CGRect(x: UIScreen.main.bounds.width - (priceSize.width + 16), y: 20, width: priceSize.width, height: priceSize.height)
    quantity.frame = CGRect(x: 27, y: 20, width: quantitySize.width, height: quantitySize.height)
    quantityHighlight.frame = CGRect(x: 16, y: 14, width: quantitySize.width + 20, height: 30)
  }

  static func calculateHeight (for lineItem: ConfirmPurchaseLineItem) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)
    let sizingView = sizingHeader.configure(with: lineItem)
    var quantityHidden = false

    if lineItem.amount == 0 && lineItem.quantity == 0 {
      quantityHidden = true
    }

    let priceWidth = sizingView.price.systemLayoutSizeFitting(CGSize(width: containerWidth, height: .greatestFiniteMagnitude)).width
    let quantityWidth = quantityHidden ? 0 : sizingView.quantity.systemLayoutSizeFitting(CGSize(width: containerWidth - priceWidth, height: .greatestFiniteMagnitude)).width
    let titleWidth = containerWidth - (priceWidth + quantityWidth + 32 + 20)

    sizingView.title.preferredMaxLayoutWidth = titleWidth
    sizingView.price.preferredMaxLayoutWidth = priceWidth
    sizingView.quantity.preferredMaxLayoutWidth = quantityWidth

    let titleHeight = max(sizingView.title.intrinsicContentSize.height, sizingView.price.intrinsicContentSize.height, sizingView.quantity.intrinsicContentSize.height)

    return 19 + titleHeight + 26
  }

  func configure (with lineItem: ConfirmPurchaseLineItem) -> ReceiptLineItem {
    let quantityVal = lineItem.quantity
    let quantityDescription = ReceiptLineItem.plainFormatter.string(from: quantityVal as NSNumber) ?? ""

    quantity.text = quantityDescription

    title.text = lineItem.title
    if lineItem.total > 0 {
      price.text = ReceiptLineItem.formatter.string(from: lineItem.total as NSDecimalNumber)
    } else {
      price.text = NSLocalizedString("Free", comment: "")
    }

    title.setNeedsLayout()
    price.setNeedsLayout()
    quantity.setNeedsLayout()
    setNeedsLayout()

    selectionStyle = .none

    if lineItem.amount == 0 && lineItem.quantity == 0 {
      quantity.alpha = 0
      quantityHighlight.alpha = 0
      quantityHidden = true
      separatorInset = .zero
    } else {
      quantity.alpha = 1
      quantityHighlight.alpha = 1
      quantityHidden = false
      separatorInset = UIEdgeInsets(top: 0, left: 50, bottom: 0, right: 0)
    }

    return self
  }
}

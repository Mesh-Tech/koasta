import Foundation
import UIKit
import Kingfisher

@objc protocol MenuItemDelegate {
  @objc optional func addTapped(for key: Int)
}

class MenuItem: CellImmediatelyUpdatable<VenueProduct> {
  fileprivate static var sizingHeader = MenuItem()
  fileprivate let border = UIView()
  fileprivate var title: H4!
  fileprivate var price: H4!
  fileprivate var desc: UILabel!
  fileprivate var addButton: AddButton!
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

  weak var delegate: MenuItemDelegate?

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
    title = H4()
    price = H4()
    desc = UILabel()
    addButton = AddButton(style: .solid)
    quantityHighlight = UIView()

    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
    title.setContentCompressionResistancePriority(.defaultHigh, for: .vertical)
    title.textColor = UIColor.foregroundColour
    price.numberOfLines = 0
    price.lineBreakMode = .byWordWrapping
    price.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    price.setContentCompressionResistancePriority(.defaultHigh, for: .horizontal)
    price.textColor = UIColor.grey10
    desc.numberOfLines = 0
    desc.lineBreakMode = .byWordWrapping
    desc.textColor = UIColor.grey10
    desc.font = .brandFont(ofSize: 14)

    border.backgroundColor = UIColor.grey1

    [border, quantityHighlight, title, price, desc, addButton].forEach { contentView.addSubview($0) }

    selectedBackgroundView = UIView()
    selectedBackgroundView?.backgroundColor = .translucentFadedBrandColour

    quantityHighlight.backgroundColor = UIColor.brandColour

    addButton.addTarget(self, action: #selector(addTapped), for: .touchUpInside)
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (60 + 16)

    price.preferredMaxLayoutWidth = containerWidth
    title.preferredMaxLayoutWidth = containerWidth
    desc.preferredMaxLayoutWidth = containerWidth

    let priceSize = price.intrinsicContentSize
    let titleSize = title.intrinsicContentSize
    let addButtonSize = addButton.intrinsicContentSize
    let descSize = desc.intrinsicContentSize

    title.frame = CGRect(x: 60, y: 16, width: titleSize.width, height: titleSize.height)
    price.frame = CGRect(x: 60, y: title.frame.maxY + 5, width: priceSize.width, height: priceSize.height)
    addButton.frame = CGRect(x: 4, y: 4, width: addButtonSize.width, height: addButtonSize.height)

    quantityHighlight.frame = CGRect(x: 0, y: 0, width: 5, height: bounds.height)

    desc.frame = CGRect(x: 60, y: price.frame.maxY + 5, width: descSize.width, height: descSize.height)

    border.frame = CGRect(x: 0, y: bounds.height - 0.5, width: bounds.width, height: 0.5)
  }

  static func calculateHeight (for product: VenueProduct, selection: ProductSelection?) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 + 60)
    let sizingView = sizingHeader.configure(with: product, selection: selection, isLastItem: false)

    sizingView.price.preferredMaxLayoutWidth = containerWidth

    let priceSize = sizingView.price.intrinsicContentSize
    sizingView.title.preferredMaxLayoutWidth = containerWidth

    let titleHeight = max(sizingView.title.intrinsicContentSize.height, priceSize.height)

    if (product.productDescription ?? "").isEmpty {
      return 16 + titleHeight + 5 + priceSize.height + 16
    }

    sizingView.desc.preferredMaxLayoutWidth = containerWidth

    let descHeight = sizingView.desc.intrinsicContentSize.height

    return 16 + titleHeight + 5 + priceSize.height + 5 + descHeight + 16
  }

  func configure (with product: VenueProduct, selection: ProductSelection?, isLastItem: Bool) -> MenuItem {
    let quantityVal = product.selection?.quantity ?? 0
    let quantityDescription = MenuItem.plainFormatter.string(from: quantityVal as NSNumber) ?? ""

    title.text = quantityVal > 0 ? "\(quantityDescription) x \(product.productName)" : product.productName
    if product.price > 0 {
      price.text = MenuItem.formatter.string(from: product.price as NSDecimalNumber)
    } else {
      price.text = NSLocalizedString("FREE", comment: "")
    }
    desc.text = product.productDescription?.trimmingCharacters(in: .whitespacesAndNewlines)

    let targetQuantityAlpha: CGFloat = selection == nil ? 0 : 1
    let targetTitleLabelTint = selection == nil ? UIColor.foregroundColour : UIColor.brandColour

    self.quantityHighlight.alpha = targetQuantityAlpha

    title.textColor = targetTitleLabelTint

    title.setNeedsLayout()
    price.setNeedsLayout()
    desc.setNeedsLayout()
    setNeedsLayout()

    selectionStyle = .none

    border.isHidden = !isLastItem

    return self
  }

  override func update(from oldState: VenueProduct, to newState: VenueProduct) {
    let targetQuantityAlpha: CGFloat = newState.selection == nil ? 0 : 1
    let targetTitleLabelTint = newState.selection == nil ? UIColor.foregroundColour : UIColor.brandColour

    layoutSubviews()
    UIView.animate(withDuration: 0.4, delay: 0, usingSpringWithDamping: 0.4, initialSpringVelocity: 0.2, options: [.curveEaseInOut], animations: {
      self.quantityHighlight.alpha = targetQuantityAlpha
      self.title.textColor = targetTitleLabelTint
      self.layoutSubviews()
    })
  }

  override func setSelected(_ selected: Bool, animated: Bool) {
    super.setSelected(selected, animated: animated)
    quantityHighlight.backgroundColor = UIColor.brandColour
  }

  @objc fileprivate func addTapped () {
    delegate?.addTapped?(for: tag)
  }
}

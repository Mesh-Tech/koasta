import Foundation
import UIKit
import Cartography

class OrderBar: UIControl {
  fileprivate struct C {
    static let padding: CGFloat = 14
  }

  fileprivate let border = UIView()
  fileprivate let quantity = UILabel()
  fileprivate let amount = UILabel()
  fileprivate let viewButton = BigButton(style: .solidRed)
  fileprivate let progress = UIActivityIndicatorView(style: .medium)

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

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
    setupLayout()
  }

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  var loading: Bool = false {
    didSet {
      UIView.animate(withDuration: 0.2) {
        if self.loading {
          self.viewButton.isEnabled = false
          self.progress.alpha = 1
        } else {
          self.viewButton.isEnabled = true
          self.progress.alpha = 0
        }
      }
    }
  }

  private func setup () {
    backgroundColor = UIColor.backgroundColour
    border.backgroundColor = UIColor.grey1
    quantity.textColor = UIColor.grey4
    amount.textColor = UIColor.foregroundColour
    quantity.numberOfLines = 1
    amount.numberOfLines = 1
    quantity.font = UIFont.brandFont(ofSize: 16)
    amount.font = UIFont.boldBrandFont(ofSize: 16)
    quantity.setContentHuggingPriority(.required, for: .horizontal)
    amount.setContentHuggingPriority(.required, for: .horizontal)
    quantity.setContentCompressionResistancePriority(.required, for: .horizontal)
    amount.setContentCompressionResistancePriority(.required, for: .horizontal)
    progress.hidesWhenStopped = false
    progress.startAnimating()
    progress.alpha = 0
    progress.color = .chalkColour

    viewButton.titleLabel.text = NSLocalizedString("View order", comment: "Screen: [Menu list] Element: [View order button] Scenario: [One or more items added to order]")

    accessibilityIdentifier = "orderBar"
    viewButton.accessibilityIdentifier = "orderBarTitle"
    quantity.accessibilityIdentifier = "orderBarQuantity"
    amount.accessibilityIdentifier = "orderBarAmount"
  }

  fileprivate func setupLayout () {
    [border, quantity, amount, viewButton, progress].forEach { addSubview($0) }

    constrain(self, quantity, viewButton, amount, border, progress) { container, quantity, viewButton, amount, border, progress in
      quantity.top == container.top + C.padding
      quantity.left == container.left + C.padding
      viewButton.right == container.right - C.padding
      viewButton.bottom == container.bottom - C.padding
      viewButton.left >= quantity.right + 34
      viewButton.left >= amount.right + 34
      viewButton.height == 44
      viewButton.width == container.width ~ LayoutPriority(900)
      amount.left == container.left + C.padding
      amount.bottom == container.bottom - C.padding
      border.top == container.top
      border.left == container.left
      border.right == container.right
      border.height == 0.5
      progress.right == viewButton.right - 10
      progress.centerY == viewButton.centerY
    }
  }

  override var intrinsicContentSize: CGSize {
    let height = (C.padding * 2) + 44
    return CGSize(width: UIView.noIntrinsicMetric, height: height)
  }

  func configure (with order: Set<ProductSelection>?, hasAddedProduct: Bool = false) {
    let order = order ?? Set()

    if order.count == 0 {
      UIView.animate(withDuration: 0.2, animations: {
        self.quantity.alpha = 0
        self.amount.alpha = 0
      }) { _ in
        self.quantity.text = "\u{0020}"
        self.amount.text = "\u{0020}"
      }
      return
    }

    let totals: (quantity: Int, amount: Decimal) = order.reduce((quantity: 0, amount: 0)) { cur, next in
      let totalPrice: Decimal = (next.price as NSDecimalNumber).multiplying(by: NSDecimalNumber(value: next.quantity)) as Decimal
      return (quantity: cur.quantity + next.quantity, amount: cur.amount + totalPrice)
    }

    if totals.quantity == 0 {
      UIView.animate(withDuration: 0.2, animations: {
        self.quantity.alpha = 0
        self.amount.alpha = 0
      }) { _ in
        self.quantity.text = "\u{0020}"
        self.amount.text = "\u{0020}"
      }
      return
    }

    setNeedsLayout()
    setNeedsDisplay()
    invalidateIntrinsicContentSize()

    guard hasAddedProduct && order.capacity == 1 && order.first?.quantity == 1 else {
      layoutIfNeeded()
      quantity.alpha = 1
      amount.alpha = 1
      self.quantity.text = "\(OrderBar.plainFormatter.string(from: totals.quantity as NSNumber) ?? "") item\(totals.quantity == 1 ? "" : "s")"
      if totals.amount > 0 {
        self.amount.text = OrderBar.formatter.string(from: totals.amount as NSNumber)
      } else {
        self.amount.text = NSLocalizedString("FREE", comment: "")
      }
      return
    }

    UIView.animate(withDuration: 0.2, animations: {
      self.layoutIfNeeded()
      self.quantity.alpha = 1
      self.amount.alpha = 1
    }) { _ in
      self.quantity.text = "\(OrderBar.plainFormatter.string(from: totals.quantity as NSNumber) ?? "") item\(totals.quantity == 1 ? "" : "s")"
      if totals.amount > 0 {
        self.amount.text = OrderBar.formatter.string(from: totals.amount as NSNumber)
      } else {
        self.amount.text = NSLocalizedString("FREE", comment: "")
      }
    }
  }

  override func addTarget(_ target: Any?, action: Selector, for controlEvents: UIControl.Event) {
    guard controlEvents != .touchUpInside else {
      viewButton.addTarget(target, action: action, for: controlEvents)
      return
    }
    super.addTarget(target, action: action, for: controlEvents)
  }

  override func removeTarget(_ target: Any?, action: Selector?, for controlEvents: UIControl.Event) {
    guard controlEvents != .touchUpInside else {
      viewButton.removeTarget(target, action: action, for: controlEvents)
      return
    }
    super.removeTarget(target, action: action, for: controlEvents)
  }
}

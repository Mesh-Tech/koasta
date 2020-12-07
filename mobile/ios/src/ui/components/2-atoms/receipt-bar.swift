import Foundation
import UIKit
import Cartography

class ReceiptBar: UIControl {
  fileprivate struct C {
    static let padding: CGFloat = 8
  }

  fileprivate let quantityBackground = UIView()
  fileprivate let quantity = H4()
  fileprivate let title = H4()
  fileprivate let amount = H4()

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

  private func setup () {
    backgroundColor = UIColor(red: 255/255.0, green: 212/255.0, blue: 61/255.0, alpha: 1.0)
    quantityBackground.layer.cornerRadius = 4
    quantityBackground.layer.masksToBounds = true
    quantityBackground.backgroundColor = UIColor.fadedForegroundColour
    quantityBackground.alpha = 0.2
    title.textColor = UIColor.foregroundColour
    quantity.textColor = UIColor.foregroundColour
    amount.textColor = UIColor.foregroundColour

    title.text = NSLocalizedString("Your order", comment: "Screen: [Menu list] Element: [Your order header] Scenario: [Order shown]")

    layer.cornerRadius = 8
    layer.masksToBounds = true

    accessibilityIdentifier = "orderBar"
    title.accessibilityIdentifier = "orderBarTitle"
    quantity.accessibilityIdentifier = "orderBarQuantity"
    amount.accessibilityIdentifier = "orderBarAmount"
  }

  fileprivate func setupLayout () {
    [quantityBackground, quantity, title, amount].forEach { addSubview($0) }

    constrain(self, quantityBackground, quantity, title, amount) { container, quantityBackground, quantity, title, amount in
      quantity.top == container.top + (C.padding * 2)
      quantity.left == container.left + (C.padding * 2)
      quantity.bottom == container.bottom - (C.padding * 2)
      quantityBackground.top == quantity.top - C.padding
      quantityBackground.left == quantity.left - C.padding
      quantityBackground.right == quantity.right + C.padding
      quantityBackground.bottom == quantity.bottom + C.padding
      title.centerX == container.centerX
      title.centerY == container.centerY
      amount.right == container.right - C.padding
      amount.centerY == container.centerY
    }
  }

  override var intrinsicContentSize: CGSize {
    title.preferredMaxLayoutWidth = bounds.width
    let height = (C.padding * 2) + title.intrinsicContentSize.height + (C.padding * 2)
    return CGSize(width: UIView.noIntrinsicMetric, height: height)
  }

  override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
    super.touchesBegan(touches, with: event)

    guard let _ = touches.first(where: { touch in
      return self.point(inside: touch.location(in: self), with: event)
    }) else {
      return
    }

    if !isEnabled {
      return
    }

    UIView.animate(withDuration: 0.2, delay: 0, options: [.beginFromCurrentState, .curveEaseIn], animations: { [weak self] in
      self?.backgroundColor = UIColor(red: 236/255.0, green: 189/255.0, blue: 22/255.0, alpha: 1.0)
    }, completion: nil)
  }

  override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
    super.touchesEnded(touches, with: event)

    if !isEnabled {
      return
    }

    UIView.animate(withDuration: 0.2, delay: 0, options: [.beginFromCurrentState, .curveEaseOut], animations: { [weak self] in
        self?.backgroundColor = UIColor(red: 255/255.0, green: 212/255.0, blue: 61/255.0, alpha: 1.0)
    }, completion: nil)
  }

  func configure (with order: Set<ProductSelection>?) {
    let order = order ?? Set()

    if order.count == 0 {
      UIView.animate(withDuration: 0.2, animations: {
        self.quantity.alpha = 0
        self.amount.alpha = 0
        self.quantityBackground.alpha = 0
      }) { _ in
        self.quantity.text = nil
        self.amount.text = nil
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
        self.quantityBackground.alpha = 0
      }) { _ in
        self.quantity.text = nil
        self.amount.text = nil
      }
      return
    }

    UIView.animate(withDuration: 0.2, animations: {
      self.quantity.alpha = 1
      self.amount.alpha = 1
      self.quantityBackground.alpha = 1
    }) { _ in
      self.quantity.text = ReceiptBar.plainFormatter.string(from: totals.quantity as NSNumber)
      if totals.amount > 0 {
        self.amount.text = ReceiptBar.formatter.string(from: totals.amount as NSNumber)
      } else {
        self.amount.text = NSLocalizedString("FREE", comment: "")
      }
    }
  }
}

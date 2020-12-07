import Foundation
import UIKit
import Differ

private class MenuSwipeItem: UIControl {
  fileprivate static let sizingItem = MenuSwipeItem()
  fileprivate let label = UILabel()
  fileprivate let border = UIView()

  fileprivate struct C {
    static let padding: CGFloat = 15
    static let labelPadding: CGFloat = 12
  }

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  private func setup () {
    label.font = UIFont.boldBrandFont(ofSize: 14)
    label.numberOfLines =  1
    label.textAlignment = .natural
    addSubview(label)
    addSubview(border)
    setNeedsLayout()
  }

  func configure(with menu: VenueMenu, active: Bool, identifier: Int) -> MenuSwipeItem {
    tag = identifier
    label.text = menu.menuName
    setActive(active)
    return self
  }

  func setActive(_ active: Bool) {
    label.textColor = active ? UIColor.brandColour : UIColor.translucentBrandColour
    border.backgroundColor = active ? UIColor.brandColour : UIColor.clear
  }

  fileprivate func calculateTextLabelSize(boundingBox: CGRect) -> CGSize {
    let constrainedSize = CGSize(width: boundingBox.size.width - (C.padding * 2), height: CGFloat.greatestFiniteMagnitude)
    return label.systemLayoutSizeFitting(constrainedSize, withHorizontalFittingPriority: UILayoutPriority.required, verticalFittingPriority: UILayoutPriority.defaultLow)
  }

  override var intrinsicContentSize: CGSize {
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)
    let width = max(C.labelPadding + labelSize.width + C.labelPadding, 82)

    return CGSize(width: width, height: C.padding + labelSize.height + C.padding + 1)
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)
    label.frame = CGRect(x: C.labelPadding, y: C.padding, width: labelSize.width, height: labelSize.height)
    border.frame = CGRect(x: 0, y: bounds.height - 2, width: bounds.width, height: 2)
  }

  override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
    super.touchesEnded(touches, with: event)
    sendActions(for: .touchUpInside)
  }
}

@objc protocol MenuSwipeListDelegate {
  @objc optional func didTapSwipeItem(at index: Int)
}

class MenuSwipeList: UIView {
  fileprivate struct C {
    static let itemPadding: CGFloat = 17
  }
  fileprivate let scrollView = UIScrollView()
  fileprivate let border = UIView()
  fileprivate var previousMenus: [VenueMenu] = []
  fileprivate var items: [MenuSwipeItem] = []

  weak var delegate: MenuSwipeListDelegate?

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  private func setup () {
    border.backgroundColor = UIColor(red: 228 / 255.0, green: 228 / 255.0, blue: 228 / 255.0, alpha: 1.0)
    scrollView.showsHorizontalScrollIndicator = true
    scrollView.showsVerticalScrollIndicator = false
    scrollView.bounces = true
    scrollView.alwaysBounceHorizontal = true
    scrollView.alwaysBounceVertical = false
    addSubview(border)
    addSubview(scrollView)
    setNeedsLayout()
  }

  override var intrinsicContentSize: CGSize {
    return CGSize(width: UIScreen.main.bounds.width, height: 48)
  }

  func configure(with menus: [VenueMenu], defaultItem: Int) {
    backgroundColor = UIColor.backgroundColour

    let diff = previousMenus.diff(menus)

    if diff.elements.count == 0 {
      return
    }

    var idx = -1
    items = Array(menus.enumerated().map {
      idx += 1
      let ret = MenuSwipeItem().configure(with: $0.element, active: $0.offset == defaultItem, identifier: $0.offset)
      ret.tag = idx
      return ret
    })

    items.forEach {
      $0.addTarget(self, action: #selector(itemTapped), for: .touchUpInside)
    }

    scrollView.contentOffset = CGPoint(x: 0, y: 0)
    buildUIHierarchy()
  }

  fileprivate func buildUIHierarchy () {
    scrollView.subviews.forEach { $0.removeFromSuperview() }

    var lastX: CGFloat = 0
    items.forEach { item in
      item.accessibilityIdentifier = "menuItem\(item.tag)"
      scrollView.addSubview(item)
      let size = item.intrinsicContentSize
      item.frame = CGRect(x: lastX, y: 0, width: size.width, height: size.height)
      lastX += size.width
    }

    scrollView.contentSize = CGSize(width: lastX, height: 48)
  }

  override func layoutSubviews() {
    scrollView.frame = bounds
    border.frame = CGRect(x: 0, y: bounds.height - 1, width: bounds.width, height: 1)
    buildUIHierarchy()
  }

  @objc fileprivate func itemTapped(item: UIControl) {
    items.forEach { $0.setActive(false) }
    items[item.tag].setActive(true)
    delegate?.didTapSwipeItem?(at: item.tag)
  }
}

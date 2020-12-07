import UIKit

enum AddButtonStyle {
  case solid
}

class AddButton: UIControl {
  fileprivate let iconBackdrop = UIView()
  fileprivate let icon = UIImageView()

  fileprivate var buttonStyle: AddButtonStyle = .solid
  fileprivate struct C {
    static let iconSize: CGFloat = 30
  }

  fileprivate var paddingWidth: CGFloat = 12 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }
  fileprivate var paddingHeight: CGFloat = 12 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init(style: AddButtonStyle = .solid) {
    self.init(frame: CGRect.zero)
    setup(style: style)
  }

  private func setup (style: AddButtonStyle = .solid) {
    accessibilityTraits = UIAccessibilityTraits.button
    buttonStyle = style

    iconBackdrop.layer.cornerRadius = 7
    iconBackdrop.layer.masksToBounds = true

    icon.image = UIImage(named: "icon-add-product")
    icon.contentMode = .center
    icon.layer.cornerRadius = 7
    icon.layer.masksToBounds = true

    switch style {
    case .solid:
      iconBackdrop.backgroundColor = UIColor.grey1
    }

    addSubview(iconBackdrop)
    addSubview(icon)
    setNeedsLayout()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    icon.frame = CGRect(x: (bounds.width / 2) - (C.iconSize / 2), y: (bounds.height / 2) - (C.iconSize / 2), width: C.iconSize, height: C.iconSize)
    iconBackdrop.frame = icon.frame
  }

  override var intrinsicContentSize: CGSize {
    let idealWidth = paddingWidth + C.iconSize + paddingWidth
    let idealHeight = paddingHeight + C.iconSize + paddingHeight

    return CGSize(width: idealWidth, height: idealHeight)
  }

  override var isEnabled: Bool {
    didSet {
      if isEnabled {
        switch buttonStyle {
        default:
          self.icon.alpha = 1
          return
        }
      } else {
        switch buttonStyle {
        default:
          self.icon.alpha = 0.3
          return
        }
      }
    }
  }

  func setEnabled(_ enabled: Bool, animated: Bool) {
    UIView.animate(withDuration: 0.2, delay: 0, options: [.beginFromCurrentState, .curveEaseOut], animations: { [weak self] in
      self?.isEnabled = enabled
    }, completion: nil)
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
      guard let buttonStyle = self?.buttonStyle else {
        self?.icon.alpha = 0.5
        return
      }

      switch buttonStyle {
      default:
        self?.icon.alpha = 0.5
        return
      }
    }, completion: nil)
  }

  override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
    if !isEnabled {
      return
    }

    UIView.animate(withDuration: 0.2, delay: 0, options: [.beginFromCurrentState, .curveEaseOut], animations: { [weak self] in
      guard let buttonStyle = self?.buttonStyle else {
        self?.icon.alpha = 1
        return
      }

      switch buttonStyle {
      default:
        self?.icon.alpha = 1
        return
      }
    }, completion: nil)

    guard let loc = touches.first?.location(in: self) else { return }
    guard bounds.contains(loc) else { return }

    sendActions(for: .touchUpInside)
  }
}

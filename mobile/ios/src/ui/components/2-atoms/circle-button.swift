import UIKit

enum CircleButtonStyle {
  case solid
}

class CircleButton: UIControl {
  fileprivate let image = IMG()

  fileprivate var buttonStyle: CircleButtonStyle = .solid
  fileprivate struct C {
    static let imageSize: CGFloat = 40
    static let imageBottomPadding: CGFloat = 8
  }

  fileprivate var paddingWidth: CGFloat = 0 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }
  fileprivate var paddingHeight: CGFloat = 24 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }

  let titleLabel = UILabel()

  fileprivate func calculateTextLabelSize(boundingBox: CGRect) -> CGSize {
    let constrainedSize = CGSize(width: boundingBox.size.width - (paddingWidth * 2), height: CGFloat.greatestFiniteMagnitude)
    return titleLabel.systemLayoutSizeFitting(constrainedSize, withHorizontalFittingPriority: UILayoutPriority.required, verticalFittingPriority: UILayoutPriority.defaultLow)
  }

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init(style: CircleButtonStyle = .solid) {
    self.init(frame: CGRect.zero)
    setup(style: style)
  }

  private func setup (style: CircleButtonStyle = .solid) {
    accessibilityTraits = UIAccessibilityTraits.button
    buttonStyle = style

    layer.cornerRadius = 3
    layer.masksToBounds = true
    image.layer.cornerRadius = C.imageSize / 2
    image.layer.masksToBounds = true

    switch style {
    case .solid:
      titleLabel.textColor = UIColor.brandColour
      titleLabel.font = UIFont.boldBrandFont(ofSize: 13)
      titleLabel.textAlignment = .center
      image.backgroundColor = UIColor.grey1
    }

    addSubview(image)
    addSubview(titleLabel)
    setNeedsLayout()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)

    image.frame = CGRect(x: (bounds.width / 2) - (C.imageSize / 2), y: paddingHeight, width: C.imageSize, height: C.imageSize)
    titleLabel.frame = CGRect(x: 0, y: image.frame.maxY + C.imageBottomPadding, width: bounds.width, height: labelSize.height)
  }

  override var intrinsicContentSize: CGSize {
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)
    let idealWidth = paddingWidth + max(C.imageSize, labelSize.width) + paddingWidth
    let idealHeight = paddingHeight + C.imageSize + C.imageBottomPadding + labelSize.height + paddingHeight

    return CGSize(width: idealWidth, height: idealHeight)
  }

  override var isEnabled: Bool {
    didSet {
      if isEnabled {
        switch buttonStyle {
        default:
          self.image.alpha = 1
          return
        }
      } else {
        switch buttonStyle {
        default:
          self.image.alpha = 0.3
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
        self?.image.alpha = 0.5
        return
      }

      switch buttonStyle {
      default:
        self?.image.alpha = 0.5
        return
      }
    }, completion: nil)
  }

  override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
    super.touchesEnded(touches, with: event)

    if !isEnabled {
      return
    }

    UIView.animate(withDuration: 0.2, delay: 0, options: [.beginFromCurrentState, .curveEaseOut], animations: { [weak self] in
      guard let buttonStyle = self?.buttonStyle else {
        self?.image.alpha = 1
        return
      }

      switch buttonStyle {
      default:
        self?.image.alpha = 1
        return
      }
    }, completion: nil)

    guard let loc = touches.first?.location(in: self) else { return }
    guard bounds.contains(loc) else { return }

    sendActions(for: .touchUpInside)
  }
}

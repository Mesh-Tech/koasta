import UIKit

enum MiniButtonStyle {
  case solidRed
}

class MiniButton: UIControl {
  fileprivate let mass = UIView()

  fileprivate var buttonStyle: MiniButtonStyle = .solidRed

  var paddingWidth: CGFloat = 5 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }
  var paddingHeight: CGFloat = 2 {
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

  convenience init(style: MiniButtonStyle = .solidRed) {
    self.init(frame: CGRect.zero)
    setup(style: style)
  }

  private func setup (style: MiniButtonStyle = .solidRed) {
    accessibilityTraits = UIAccessibilityTraits.button
    buttonStyle = style

    switch style {
    case .solidRed:
      titleLabel.textColor = UIColor.chalkColour
      titleLabel.font = UIFont.boldBrandFont(ofSize: 14)
      titleLabel.textAlignment = .center
      mass.layer.backgroundColor = UIColor.brandColour.cgColor
      layer.cornerRadius = 3
      layer.masksToBounds = true
      mass.layer.cornerRadius = 6
      mass.layer.masksToBounds = true
    }

    addSubview(mass)
    addSubview(titleLabel)
    setNeedsLayout()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)

    mass.frame = bounds
    titleLabel.frame = CGRect(x: paddingWidth, y: paddingHeight, width: bounds.width - (paddingWidth * 2), height: labelSize.height)
  }

  override var intrinsicContentSize: CGSize {
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)
    let idealWidth = paddingWidth + labelSize.width + paddingWidth
    let idealHeight = paddingHeight + labelSize.height + paddingHeight

    return CGSize(width: idealWidth, height: idealHeight)
  }

  override var isEnabled: Bool {
    didSet {
      if isEnabled {
        switch buttonStyle {
        default:
          self.mass.alpha = 1
          return
        }
      } else {
        switch buttonStyle {
        default:
          self.mass.alpha = 0.3
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
        self?.mass.alpha = 0.5
        return
      }

      switch buttonStyle {
      default:
        self?.mass.alpha = 0.5
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
        self?.mass.alpha = 1
        return
      }

      switch buttonStyle {
      default:
        self?.mass.alpha = 1
        return
      }
      }, completion: nil)

    guard let loc = touches.first?.location(in: self) else { return }
    guard bounds.contains(loc) else { return }

    sendActions(for: .touchUpInside)
  }
}

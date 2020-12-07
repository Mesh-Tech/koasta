import UIKit

enum LineButtonStyle {
  case hollow
}

class LineButton: UIControl {
  fileprivate let border = Border()

  fileprivate var buttonStyle: LineButtonStyle = .hollow

  fileprivate var paddingWidth: CGFloat = 40 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }
  fileprivate var paddingHeight: CGFloat = 16 {
    didSet {
      setNeedsLayout()
      setNeedsDisplay()
    }
  }

  var strokeColour: UIColor? {
    get { return border.strokeColour }
    set { border.strokeColour = newValue }
  }

  let titleLabel = UILabel()

  fileprivate func calculateTextLabelSize(boundingBox: CGRect) -> CGSize {
    let constrainedSize = CGSize(width: boundingBox.size.width - (paddingWidth * 2), height: CGFloat.greatestFiniteMagnitude)
    return titleLabel.systemLayoutSizeFitting(constrainedSize, withHorizontalFittingPriority: UILayoutPriority.required, verticalFittingPriority: UILayoutPriority.defaultLow)
  }

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init(style: LineButtonStyle = .hollow) {
    self.init(frame: CGRect.zero)
    setup(style: style)
  }

  private func setup (style: LineButtonStyle = .hollow) {
    accessibilityTraits = UIAccessibilityTraits.button
    buttonStyle = style

    layer.cornerRadius = 3
    layer.masksToBounds = true
    border.layer.cornerRadius = 3
    border.layer.masksToBounds = true

    switch style {
    case .hollow:
      titleLabel.textColor = UIColor.contrastedForegroundColour
      titleLabel.font = UIFont.boldBrandFont(ofSize: 14)
      titleLabel.textAlignment = .center
      border.strokeWidth = 1
      border.strokeColour = UIColor.contrastedForegroundColour
      border.layer.backgroundColor = UIColor.clear.cgColor
      paddingHeight = 3
      paddingWidth = 22
    }

    addSubview(border)
    addSubview(titleLabel)
    setNeedsLayout()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    titleLabel.frame = CGRect(x: 0, y: paddingHeight, width: bounds.width, height: bounds.height - (paddingHeight * 2))
    border.frame = bounds
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
          self.alpha = 1
          return
        }
      } else {
        switch buttonStyle {
        default:
          self.alpha = 0.3
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
        self?.alpha = 0.5
        return
      }

      switch buttonStyle {
      default:
        self?.alpha = 0.5
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
        self?.alpha = 1
        return
      }

      switch buttonStyle {
      default:
        self?.alpha = 1
        return
      }
    }, completion: nil)

    guard let loc = touches.first?.location(in: self) else { return }
    guard bounds.contains(loc) else { return }

    sendActions(for: .touchUpInside)
  }
}

import UIKit

class Border: UIView {
  var strokeWidth: CGFloat = 4 {
    didSet {
      layer.borderWidth = strokeWidth
      setNeedsDisplay()
    }
  }

  var strokeColour: UIColor? = UIColor.charcoalColour {
    didSet {
      activeStrokeColour = strokeColour
      setNeedsDisplay()
    }
  }

  var activeStrokeColour: UIColor? = UIColor.charcoalColour {
    didSet {
      layer.borderColor = activeStrokeColour?.cgColor
    }
  }

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  convenience init() {
    self.init(frame: CGRect.zero)
  }

  private func setup () {
    isOpaque = false
    layer.backgroundColor = UIColor.clear.cgColor
    layer.borderWidth = strokeWidth
    layer.borderColor = strokeColour?.cgColor
  }

  func setStrokeColour(_ newStrokeColour: UIColor?, animated: Bool) {
    guard animated else { strokeColour = newStrokeColour; return }
    let animation = CABasicAnimation(keyPath: "borderColor")
    animation.fromValue = strokeColour?.cgColor
    animation.toValue = newStrokeColour?.cgColor
    animation.duration = 0.3
    animation.isRemovedOnCompletion = true
    layer.add(animation, forKey: UUID().uuidString)
    strokeColour = newStrokeColour
  }
}

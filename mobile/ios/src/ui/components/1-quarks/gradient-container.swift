import UIKit

class GradientContainer: UIView {
  enum Direction {
    case vertical
    case horizontal
  }

  var from: UIColor? {
    didSet {
      setNeedsDisplay()
    }
  }
  var to: UIColor? {
    didSet {
      setNeedsDisplay()
    }
  }

  var fillPercentage: CGFloat = 0.0

  var direction: Direction = .vertical

  convenience init () {
    self.init(frame: CGRect.zero)
  }

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  fileprivate func setup () {
    isOpaque = false
    backgroundColor = UIColor.clear
    setNeedsDisplay()
  }

  override func draw(_ rect: CGRect) {
    super.draw(rect)
    guard let currentContext = UIGraphicsGetCurrentContext() else { return }
    currentContext.saveGState()

    let startColor = from ?? UIColor.charcoalColour
    let endColor = to ?? UIColor.chalkColour

    let colorSpace = CGColorSpaceCreateDeviceRGB()

    let colorComponents = [
      startColor.cgColor,
      startColor.cgColor,
      endColor.cgColor
    ] as NSArray as CFArray

    let locations: [CGFloat] = [
      0.0,
      fillPercentage,
      1.0
    ]

    guard let gradient = CGGradient(colorsSpace: colorSpace, colors: colorComponents, locations: locations) else {
      return currentContext.restoreGState()
    }

    let startPoint: CGPoint
    let endPoint: CGPoint

    switch direction {
    case .horizontal:
      startPoint = CGPoint(x: 0, y: self.bounds.height)
      endPoint = CGPoint(x: self.bounds.width, y: self.bounds.height)
    case .vertical:
      startPoint = CGPoint(x: 0, y: 0)
      endPoint = CGPoint(x: 0, y: self.bounds.height)
    }

    currentContext.drawLinearGradient(gradient, start: startPoint, end: endPoint, options: [])

    currentContext.restoreGState()
  }
}

import Foundation
import UIKit

class Progress: UIProgressView {
  override func layoutSubviews() {
    super.layoutSubviews()

    let maskLayerPath = UIBezierPath(roundedRect: bounds, cornerRadius: self.frame.height / 2)
    let maskLayer = CAShapeLayer()
    maskLayer.frame = self.bounds
    maskLayer.path = maskLayerPath.cgPath
    layer.mask = maskLayer
  }
}

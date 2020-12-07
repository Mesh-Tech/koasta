import UIKit

class StretchContainer: UIView {
  override func layoutSubviews() {
    super.layoutSubviews()
    (layer.sublayers ?? []).forEach {
      $0.frame = bounds
    }
  }
}

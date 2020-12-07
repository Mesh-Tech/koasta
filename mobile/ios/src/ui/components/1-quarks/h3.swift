import Foundation
import UIKit

class H3: UILabel {
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
    setup()
  }

  private func setup () {
    numberOfLines = 0
  }

  override var accessibilityValue: String? {
    get {
      return text
    }
    set {}
  }
}

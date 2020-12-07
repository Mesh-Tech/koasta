import Foundation
import UIKit

extension UINavigationController {
  func applyDefaultStyling () {
    navigationBar.setBackgroundImage(UIImage(), for: .default)
    navigationBar.shadowImage = UIImage()
    navigationBar.isTranslucent = true
  }
}

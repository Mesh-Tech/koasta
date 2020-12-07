import Foundation
import UIKit

extension UIFont {
  static func fontName(forWeight weight: UIFont.Weight) -> String {
    switch weight {
    case .ultraLight:
      return "Manrope-ExtraLight"
    case .light:
      return "Manrope-Light"
    case .medium:
      return "Manrope-Medium"
    case .semibold:
      return "Manrope-SemiBold"
    case .bold:
      return "Manrope-Bold"
    case .heavy:
      return "Manrope-ExtraBold"
    case .black:
      return "Manrope-ExtraBold"
    default:
      return "Manrope-Regular"
    }
  }

  static func brandFont(ofSize fontSize: CGFloat, weight: UIFont.Weight) -> UIFont {
    return UIFont(name: fontName(forWeight: weight), size: fontSize)!
  }

  static func brandFont(ofSize fontSize: CGFloat) -> UIFont {
    return UIFont(name: fontName(forWeight: .regular), size: fontSize)!
  }

  static func boldBrandFont(ofSize fontSize: CGFloat) -> UIFont {
    return UIFont(name: fontName(forWeight: .bold), size: fontSize)!
  }

  static func preferredBrandFont(forTextStyle: UIFont.TextStyle) -> UIFont {
    let font = UIFont(name: fontName(forWeight: .regular), size: UIFont.labelFontSize)
    let fontMetrics = UIFontMetrics(forTextStyle: .body)
    return fontMetrics.scaledFont(for: font!)
  }
}

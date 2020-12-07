import Foundation
import UIKit

protocol Toast {
  func show(completion: @escaping () -> Void)
  func adjustForKeyboard(keyboardFrame: CGRect, duration: TimeInterval, curve: UIView.AnimationCurve)
}

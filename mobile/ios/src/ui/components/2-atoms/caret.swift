import Foundation
import UIKit

class Caret: UIView {
  fileprivate var blinkTimer: Timer?
  fileprivate var caretVisible: Bool = false

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
    backgroundColor = UIColor.brandColour
  }

  override var isHidden: Bool {
    didSet {
      setupBlinking()
    }
  }

  fileprivate func setupBlinking() {
    guard !isHidden else {
      return invalidateCaret()
    }

    blinkTimer = Timer(fireAt: Date().addingTimeInterval(0.5), interval: 0.5, target: self, selector: #selector(doBlink), userInfo: nil, repeats: true)

    RunLoop.current.add(blinkTimer!, forMode: RunLoop.Mode.default)
  }

  @objc fileprivate func doBlink() {
    guard let _ = superview, let _ = window else {
      return invalidateCaret()
    }

    if !caretVisible {
      showCaret()
    } else {
      hideCaret()
    }
  }

  func hideCaret(animated: Bool = true) {
    UIView.animate(withDuration: 0.25) {
      self.alpha = 0
    }

    caretVisible = false
  }

  func showCaret(animated: Bool = true) {
    UIView.animate(withDuration: 0.25) {
      self.alpha = 1
    }

    caretVisible = true
  }

  fileprivate func invalidateCaret() {
    blinkTimer?.invalidate()
    blinkTimer = nil
  }
}

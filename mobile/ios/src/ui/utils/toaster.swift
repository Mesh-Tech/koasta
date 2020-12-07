import Foundation
import UIKit

enum ToastPosition {
  case top
  case bottom
}

enum ToastStyle {
  case light
  case dark
}

class Toaster {
  fileprivate var toastStack: [Toast] = []
  fileprivate var isShowingToasts = false
  fileprivate var keyboardFrame = CGRect.zero

  func beginListening () {
    NotificationCenter.default.addObserver(self, selector: #selector(keyBoardWillShow(notification:)), name: UIResponder.keyboardWillShowNotification, object: nil)
    NotificationCenter.default.addObserver(self, selector: #selector(keyBoardWillHide(notification:)), name: UIResponder.keyboardWillHideNotification, object: nil)
  }

  func endListening () {

  }

  func push(from: UIViewController, message: String, caption: String? = nil, position: ToastPosition = .top, style: ToastStyle = .light, timeout: TimeInterval = 3) {
    toastStack.append(style == .light
      ? PaleToast(message: message, presenter: from, position: position, keyboardFrame: keyboardFrame, timeout: timeout)
      : BurntToast(message: message, caption: caption ?? "", presenter: from, position: position, keyboardFrame: keyboardFrame, timeout: timeout)
    )

    if !isShowingToasts {
      doNext()
    }
  }

  fileprivate func doNext() {
    if toastStack.count == 0 {
      return
    }

    toastStack.remove(at: 0).show { self.doNext() }
  }

  @objc fileprivate func keyBoardWillShow(notification: Notification) {
    guard let keyboardFrame = notification.userInfo?[UIResponder.keyboardFrameEndUserInfoKey] as? CGRect,
      let duration = notification.userInfo?[UIResponder.keyboardAnimationDurationUserInfoKey] as? TimeInterval else {
        return
    }

    var curve = UIView.AnimationCurve.easeInOut
    if let nCurve = notification.userInfo?[UIResponder.keyboardAnimationCurveUserInfoKey] as? NSNumber,
      let realCurve = UIView.AnimationCurve(rawValue: nCurve.intValue) {
      curve = realCurve
    }

    self.keyboardFrame = keyboardFrame
    toastStack.forEach {
      $0.adjustForKeyboard(keyboardFrame: keyboardFrame, duration: duration, curve: curve)
    }
  }

  @objc fileprivate func keyBoardWillHide(notification: Notification) {
    guard let duration = notification.userInfo?[UIResponder.keyboardAnimationDurationUserInfoKey] as? TimeInterval else {
      return
    }

    var curve = UIView.AnimationCurve.easeInOut
    if let nCurve = notification.userInfo?[UIResponder.keyboardAnimationCurveUserInfoKey] as? NSNumber,
      let realCurve = UIView.AnimationCurve(rawValue: nCurve.intValue) {
      curve = realCurve
    }

    self.keyboardFrame = CGRect.zero
    toastStack.forEach {
      $0.adjustForKeyboard(keyboardFrame: keyboardFrame, duration: duration, curve: curve)
    }
  }
}

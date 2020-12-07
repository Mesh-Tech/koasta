import XCTest

extension XCTestCase {
  func scenario(_ name: String) {
    _ = XCTContext.runActivity(named: name) { _ in }
  }

  func given(_ name: String, block: () -> Void) {
    _ = XCTContext.runActivity(named: "Given \(name)") { _ in
      block()
    }
  }

  func then(_ name: String, block: () -> Void) {
    _ = XCTContext.runActivity(named: "Then \(name)") { _ in
      block()
    }
  }

  @discardableResult func wait(forIdentifier identifier: String, timeout: TimeInterval = 10) -> UIView {
    if let view = getView(identifier) {
      return view
    }

    guard timeout > 0 else {
      fatalError()
    }

    sleep(1)
    return wait(forIdentifier: identifier, timeout: timeout - 1)
  }

  fileprivate func getView(_ accessibilityIdentifier: String, parentView: UIView? = nil) -> UIView? {
    let parentView: UIView = parentView ?? UIApplication.shared.keyWindow!
    guard parentView.accessibilityIdentifier != accessibilityIdentifier else {
      return parentView
    }

    for subview in parentView.subviews {
      if let matchingView = getView(accessibilityIdentifier, parentView: subview) {
        return matchingView
      }
    }

    return nil
  }
}

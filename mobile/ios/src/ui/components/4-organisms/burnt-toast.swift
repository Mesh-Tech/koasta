import Foundation
import UIKit

private class GhostView: UIView {}

private class GhostWindow: UIWindow {
  override func hitTest(_ point: CGPoint, with event: UIEvent?) -> UIView? {
    return nil
  }
}

private class GhostViewController: UIViewController {
  private let style: UIBarStyle
  private let interface: UIUserInterfaceStyle

  init(barStyle: UIBarStyle, styleOverride: UIUserInterfaceStyle) {
    self.style = barStyle
    self.interface = styleOverride
    super.init(nibName: nil, bundle: nil)
  }

  required init?(coder aDecoder: NSCoder) {
    fatalError()
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    navigationController?.navigationBar.barStyle = style
    overrideUserInterfaceStyle = interface
  }
}

class BurntToast: UIControl, Toast {
  fileprivate let titleLabel = UILabel()
  fileprivate let bodyLabel = UILabel()
  fileprivate struct C {
    static let paddingWidth: CGFloat = 20
    static let paddingHeight: CGFloat = 20
    static let labelPaddingHeight: CGFloat = 6
    static let edgePadding: CGFloat = 0
  }
  fileprivate var animatingPhase2 = false
  fileprivate var keyboardFrame = CGRect.zero
  fileprivate var secondPhaseAnimationDelay: TimeInterval = 0
  fileprivate var position: ToastPosition = .top
  fileprivate var barStyle: UIBarStyle = .default
  fileprivate var styleOverride: UIUserInterfaceStyle = .unspecified
  fileprivate var timeout: TimeInterval = 3

  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}

  convenience init(message: String, caption: String, presenter: UIViewController, position: ToastPosition = .top, keyboardFrame: CGRect = CGRect.zero, timeout: TimeInterval = 3) {
    self.init(frame: CGRect.zero)
    self.position = position
    self.keyboardFrame = keyboardFrame
    self.barStyle = presenter.navigationController?.navigationBar.barStyle ?? .default
    self.styleOverride = presenter.navigationController?.overrideUserInterfaceStyle ?? .unspecified
    self.timeout = timeout
    setup(message: message, caption: caption)
  }

  fileprivate var bottomScreenY: CGFloat {
    get {
      return keyboardFrame.height == 0
        ? UIScreen.main.bounds.height
      : keyboardFrame.minY
    }
  }

  fileprivate func setup(message: String, caption: String) {
    backgroundColor = UIColor.deepBlueColour

    addSubview(titleLabel)
    titleLabel.text = caption
    titleLabel.numberOfLines = 0
    titleLabel.font = UIFont.brandFont(ofSize: 16, weight: .semibold)
    titleLabel.textColor = UIColor.chalkColour
    titleLabel.accessibilityIdentifier = "toastTitle"

    addSubview(bodyLabel)
    bodyLabel.text = message
    bodyLabel.numberOfLines = 0
    bodyLabel.font = UIFont.brandFont(ofSize: 16, weight: .regular)
    bodyLabel.textColor = UIColor.chalkColour
  }

  func show(completion: @escaping () -> Void) {
    let screenSize = UIScreen.main.bounds.size
    let win = UIApplication.shared.windows.filter { $0.isKeyWindow && !$0.isHidden }.first
    let statusBarHeight = win?.windowScene?.statusBarManager?.statusBarFrame.height ?? 0
    let intrinsicHeight = intrinsicContentSize.height

    let startY: CGFloat = self.position == .top
      ? 0-(intrinsicHeight + statusBarHeight)
      : self.bottomScreenY

    frame = CGRect(x: C.edgePadding, y: startY, width: screenSize.width - (C.edgePadding * 2), height: intrinsicHeight)

    setNeedsLayout()
    setNeedsDisplay()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()

    let ghostView = GhostView(frame: CGRect(x: 0, y: 0, width: UIScreen.main.bounds.width, height: UIScreen.main.bounds.height))
    ghostView.backgroundColor = UIColor.clear

    let window = GhostWindow()
    window.backgroundColor = UIColor.clear
    window.isOpaque = false
    window.rootViewController = GhostViewController(barStyle: barStyle, styleOverride: styleOverride)
    window.addSubview(ghostView)
    window.addSubview(self)

    window.windowLevel = UIWindow.Level.statusBar
    window.isHidden = false

    let y: CGFloat = position == .top
      ? statusBarHeight + C.edgePadding
      : bottomScreenY - (C.edgePadding + self.frame.size.height)

    UIView.animate(withDuration: 0.3, delay: 0, options: [.curveEaseOut], animations: {
      self.frame = CGRect(x: self.frame.origin.x, y: y, width: self.frame.size.width, height: self.frame.size.height)
    }) { _ in
      self.animatingPhase2 = true
      DispatchQueue.main.asyncAfter(deadline: DispatchTime.now() + self.timeout) {
        UIView.animate(withDuration: 0.3, delay: self.secondPhaseAnimationDelay, options: [.curveEaseIn], animations: {
          self.alpha = 0
          let endY: CGFloat = self.position == .top
            ? 0-intrinsicHeight
            : self.bottomScreenY

          self.frame = CGRect(x: self.frame.origin.x, y: endY, width: self.frame.size.width, height: self.frame.size.height)
        }) { _ in
          window.isHidden = true
          completion()
        }
      }
    }
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)
    let bodyLabelSize = calculateBodyLabelSize(boundingBox: UIScreen.main.bounds)
    titleLabel.frame = CGRect(x: C.paddingWidth, y: C.paddingHeight, width: bounds.width - (C.paddingWidth * 2), height: labelSize.height)
    bodyLabel.frame = CGRect(x: C.paddingWidth, y: titleLabel.frame.maxY + C.labelPaddingHeight, width: bounds.width - (C.paddingWidth * 2), height: bodyLabelSize.height)
  }

  fileprivate func calculateTextLabelSize(boundingBox: CGRect) -> CGSize {
    let constrainedSize = CGSize(width: boundingBox.size.width - (C.paddingWidth * 2), height: CGFloat.greatestFiniteMagnitude)
    return titleLabel.systemLayoutSizeFitting(constrainedSize, withHorizontalFittingPriority: UILayoutPriority.required, verticalFittingPriority: UILayoutPriority.defaultLow)
  }

  fileprivate func calculateBodyLabelSize(boundingBox: CGRect) -> CGSize {
    let constrainedSize = CGSize(width: boundingBox.size.width - (C.paddingWidth * 2), height: CGFloat.greatestFiniteMagnitude)
    return bodyLabel.systemLayoutSizeFitting(constrainedSize, withHorizontalFittingPriority: UILayoutPriority.required, verticalFittingPriority: UILayoutPriority.defaultLow)
  }

  override var intrinsicContentSize: CGSize {
    let labelSize = calculateTextLabelSize(boundingBox: UIScreen.main.bounds)
    let bodyLabelSize = calculateBodyLabelSize(boundingBox: UIScreen.main.bounds)

    let idealWidth = C.paddingWidth + labelSize.width + C.paddingWidth
    let idealHeight = C.paddingHeight + labelSize.height + C.labelPaddingHeight + bodyLabelSize.height + C.paddingHeight

    return CGSize(width: idealWidth, height: idealHeight)
  }

  func adjustForKeyboard(keyboardFrame: CGRect, duration: TimeInterval, curve: UIView.AnimationCurve) {
    self.keyboardFrame = keyboardFrame
    secondPhaseAnimationDelay = duration
  }
}

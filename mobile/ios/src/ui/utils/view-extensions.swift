import UIKit

extension UIView {
  func roundCorners(_ corners: UIRectCorner, radius: CGFloat) {
    let path = UIBezierPath(roundedRect: self.bounds, byRoundingCorners: corners, cornerRadii: CGSize(width: radius, height: radius))
    let mask = CAShapeLayer()
    mask.path = path.cgPath
    self.layer.mask = mask
  }
}

extension UIViewController: UIAdaptivePresentationControllerDelegate {
  public func presentationControllerDidDismiss( _ presentationController: UIPresentationController) {
    viewWillAppear(true)
  }
}

extension UIViewController {

    /**
     *  Calculates the height of the status bar, if possible
     */
    var statusBarHeight: CGFloat {
      if UIApplication.shared.windows.count == 0 {
        return 0
      }

      // HACK: We do this to exclusively blur status bars and NOT the navigation bars.
      //       If there is ever a better way of achieving this, we should do that.
        return UIApplication.shared.windows[0].windowScene?.statusBarManager?.statusBarFrame.height ?? 0.0
    }
}

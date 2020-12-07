import Foundation
import Hydra
import UIKit

@objc enum PaymentStatus: Int {
  case success
  case error
  case cancellation
}

@objc protocol PaymentDelegate {
  @objc optional func paymentDelegateDidFailToInitialise(_ error: Error)
  @objc optional func paymentDelegateDidCompleteWithStatus(_ status: PaymentStatus, withToken token: String?, withVerificationToken verificationToken: String?)
}

protocol BillingManager {
  func initialise()

  func presentPaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int)

  func presentNativePaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int)

  func completePayment(status: PaymentStatus, done: @escaping () -> Void)

  var supportsNativePayments: Bool { get }
}

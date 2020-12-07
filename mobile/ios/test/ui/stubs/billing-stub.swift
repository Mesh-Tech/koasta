import Foundation
import Hydra
import UIKit
@testable import pubcrawl

class StubBillingManager: BillingManager {
  func presentNativePaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int) {
    let vc = viewController as? PaymentDelegate

    if success {
      vc?.paymentDelegateDidCompleteWithStatus?(.success, withToken: "abc", withVerificationToken: "def")
    } else {
      vc?.paymentDelegateDidCompleteWithStatus?(.error, withToken: "abc", withVerificationToken: "def")
    }
  }

  var supportsNativePayments: Bool = false

  var success = true

  func initialise() {}

  func presentPaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int) {
    let vc = viewController as? PaymentDelegate

    if success {
      vc?.paymentDelegateDidCompleteWithStatus?(.success, withToken: "abc", withVerificationToken: "def")
    } else {
      vc?.paymentDelegateDidCompleteWithStatus?(.error, withToken: "abc", withVerificationToken: "def")
    }
  }

  func completePayment(status: PaymentStatus, done: @escaping () -> Void) {
    done()
  }
}

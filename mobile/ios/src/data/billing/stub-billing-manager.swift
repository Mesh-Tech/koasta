import Foundation
import UIKit
import Hydra

class StubBillingManager: BillingManager {
  func completePayment(status: PaymentStatus, done: @escaping () -> Void) {
    done()
  }

  func initialise() {
  }

  var supportsNativePayments: Bool {
    get {
      return false
    }
  }

  func presentPaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int) {
    guard let vc = viewController as? PaymentDelegate else {
      return
    }

    vc.paymentDelegateDidCompleteWithStatus?(.success, withToken: "abc", withVerificationToken: "def")
  }

  func presentNativePaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int) {
    guard let vc = viewController as? PaymentDelegate else {
      return
    }

    vc.paymentDelegateDidCompleteWithStatus?(.success, withToken: "abc", withVerificationToken: "def")
  }
}

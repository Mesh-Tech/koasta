import Foundation
import UIKit
import SquareInAppPaymentsSDK
import SquareBuyerVerificationSDK
import Hydra
import PassKit

class SquareBillingManager: NSObject, BillingManager, SQIPCardEntryViewControllerDelegate, PKPaymentAuthorizationViewControllerDelegate {
  fileprivate weak var delegate: PaymentDelegate?
  fileprivate var completion: ((Error?) -> Void)?
  fileprivate var completeCompletion: (() -> Void)?
  fileprivate let config: Config
  fileprivate let session: SessionManager
  fileprivate var venue: Venue!
  fileprivate var amount: Int = 0
  fileprivate var hasDismissed = false
  fileprivate var cardVc: SQIPCardEntryViewController?
  fileprivate var hasAuthorisedPayment = false
  fileprivate var hasFailedPayment = false
  fileprivate var applePayCardDetails: SQIPCardDetails?

  public init(config: Config?, session: SessionManager?) {
    guard let config = config, let session = session else {
      fatalError()
    }

    self.config = config
    self.session = session
  }

  var supportsNativePayments: Bool {
    get {
      return SQIPInAppPaymentsSDK.canUseApplePay
    }
  }

  func presentPaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int) {
    guard let vc = viewController as? PaymentDelegate else {
      return
    }

    self.venue = venue
    self.amount = amount
    hasAuthorisedPayment = false
    hasFailedPayment = false
    delegate = vc
    cardVc = SQIPCardEntryViewController(theme: SQIPTheme())
    cardVc!.delegate = self
    viewController.present(cardVc!, animated: true, completion: nil)
  }

  func presentNativePaymentViewController(to viewController: UIViewController, forVenue venue: Venue, withTotalAmountInPence amount: Int) {
    if !SQIPInAppPaymentsSDK.canUseApplePay {
      return
    }

    guard let dvc = viewController as? PaymentDelegate else {
      return
    }

    let req = PKPaymentRequest.squarePaymentRequest(merchantIdentifier: config.merchantId, countryCode: "GB", currencyCode: "GBP")
    let paymentAmount = NSDecimalNumber(value: amount).dividing(by: NSDecimalNumber(100))
    req.paymentSummaryItems = [
      PKPaymentSummaryItem(label: "Koasta", amount: paymentAmount)
    ]

    guard let vc = PKPaymentAuthorizationViewController(paymentRequest: req) else {
      return
    }

    self.venue = venue
    self.amount = amount
    hasAuthorisedPayment = false
    hasFailedPayment = false
    delegate = dvc

    vc.delegate = self

    viewController.present(vc, animated: true, completion: nil)
  }

  func initialise() {
    SQIPInAppPaymentsSDK.squareApplicationID = config.squareApplicationId
  }

  func completePayment(status: PaymentStatus, done: @escaping () -> Void) {
    guard let completion = completion else {
      done()
      return
    }

    completeCompletion = done

    if status == .error || status == .cancellation {
      completion(UserFriendlyError(message: NSLocalizedString("We weren't able to complete this transaction. Please check your payment details and try again", comment: "")))
    } else {
      completion(nil)
    }
  }

  func cardEntryViewController(_ cardEntryViewController: SQIPCardEntryViewController, didObtain cardDetails: SQIPCardDetails, completionHandler: @escaping (Error?) -> Void) {
    completion = completionHandler

    let contact = SQIPContact()
    contact.givenName = session.firstName ?? ""

    let params = SQIPVerificationParameters(
      paymentSourceID: cardDetails.nonce,
      buyerAction: SQIPBuyerAction.charge(SQIPMoney(amount: amount, currency: .GBP)),
      locationID: venue.externalLocationId ?? "",
      contact: contact
    )

    SQIPBuyerVerificationSDK.shared.verify(with: params, theme: SQIPTheme(), viewController: cardEntryViewController, success: { [weak self] (details) in
      self?.delegate?.paymentDelegateDidCompleteWithStatus?(.success, withToken: cardDetails.nonce, withVerificationToken: details.verificationToken)
    }) { [weak self] (error) in
      self?.delegate?.paymentDelegateDidFailToInitialise?(error)
    }
  }

  func cardEntryViewController(_ cardEntryViewController: SQIPCardEntryViewController, didCompleteWith status: SQIPCardEntryCompletionStatus) {
    guard !hasDismissed else {
      return
    }

    cardVc = nil
    hasDismissed = true
    cardEntryViewController.dismiss(animated: true) {
      self.completeCompletion?()
      self.completeCompletion = nil
    }

    if status == .canceled {
      delegate?.paymentDelegateDidCompleteWithStatus?(.cancellation, withToken: nil, withVerificationToken: nil)
    }
  }

  func paymentAuthorizationViewController(_ controller: PKPaymentAuthorizationViewController, didAuthorizePayment payment: PKPayment, handler completion: @escaping (PKPaymentAuthorizationResult) -> Void) {
    let nonceRequest = SQIPApplePayNonceRequest(payment: payment)
    nonceRequest.perform { [weak self] cardDetails, error in
      if let error = error {
        self?.hasFailedPayment = true
        let result = PKPaymentAuthorizationResult(status: .failure, errors: [error])
        completion(result)
      } else if let cardDetails = cardDetails {
        self?.applePayCardDetails = cardDetails
        self?.hasAuthorisedPayment = true
        let result = PKPaymentAuthorizationResult(status: .success, errors: nil)
        completion(result)
      } else {
        let result = PKPaymentAuthorizationResult(status: .failure, errors: [])
        completion(result)
      }
    }
  }

  func paymentAuthorizationViewControllerDidFinish(_ controller: PKPaymentAuthorizationViewController) {
    controller.dismiss(animated: true) {
      if self.hasFailedPayment {
        self.delegate?.paymentDelegateDidCompleteWithStatus?(.error, withToken: nil, withVerificationToken: nil)
        return
      }

      if !self.hasAuthorisedPayment {
        self.delegate?.paymentDelegateDidCompleteWithStatus?(.cancellation, withToken: nil, withVerificationToken: nil)
      }

      guard let cardDetails = self.applePayCardDetails else {
        self.delegate?.paymentDelegateDidCompleteWithStatus?(.cancellation, withToken: nil, withVerificationToken: nil)
        return
      }

      self.delegate?.paymentDelegateDidCompleteWithStatus?(.success, withToken: cardDetails.nonce, withVerificationToken: nil)
    }
  }
}

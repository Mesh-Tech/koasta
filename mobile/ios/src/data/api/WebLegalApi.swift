import Foundation
import Hydra

class WebLegalApi: LegalApi {
  func fetchPrivacyPolicy() -> Promise<String> {
    return async(in: .custom(queue: DispatchQueue.global()), { _ in
      let content = try String(contentsOf: Constants.PrivacyPolicyUrl)
      return content
    })
  }

  func fetchTermsAndConditions() -> Promise<String> {
    return async(in: .custom(queue: DispatchQueue.global()), { _ in
      let content = try String(contentsOf: Constants.TermsAndConditionsUrl)
      return content
    })
  }
}

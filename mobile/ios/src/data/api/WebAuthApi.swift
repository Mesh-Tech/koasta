import Foundation
import Hydra

class WebAuthApi: AuthApi {
  fileprivate let request: Request!
  fileprivate let config: Config!

  init(request: Request?,
       config: Config?) {
    guard let request = request,
          let config = config else {
      fatalError()
    }

    self.request = request
    self.config = config
  }

  func login(firstName: String?, lastName: String?) -> Promise<Void> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("auth").appendingPathComponent("auth").appendingPathComponent("authorise")
      let body = Credentials(firstName: firstName, lastName: lastName)
      try await(self.request.post(url: url, body: body))
    })
  }

  func getPaymentProviderEphemeralKey() -> Promise<AuthEphemeralKeyResult> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("auth").appendingPathComponent("auth").appendingPathComponent("payment-key")

      do {
        guard let key: AuthEphemeralKeyResult = try await(self.request.post(url: url)) else {
          throw ApiError(statusCode: 500, body: nil, bodyData: nil)
        }
        return key
      } catch let error as ApiError {
        throw error
      }
    })
  }
}

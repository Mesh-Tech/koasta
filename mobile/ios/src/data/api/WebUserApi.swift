import Foundation
import Hydra

class WebUserApi: UserApi {
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

  func registerPushToken(_ registration: PushRegistration) -> Promise<Void> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("user").appendingPathComponent("/users/me/devices")
      return try await(self.request.postExecute(url: url, body: registration))
    })
  }

  func fetchCurrentProfile() -> Promise<UserProfile> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("user").appendingPathComponent("/users/me")
      let ret: UserProfile? = try await(self.request.get(url: url))

      if let ret = ret {
        return ret
      }

      throw ApiError(statusCode: 401, body: "Unauthorized", bodyData: nil)
    })
  }
}

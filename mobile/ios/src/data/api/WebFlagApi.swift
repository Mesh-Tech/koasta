import Foundation
import Hydra

class WebFlagApi: FlagApi {
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

  func fetchCurrentFlags() -> Promise<FeatureFlags> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("flags").appendingPathComponent("flags").appendingPathComponent("current")
      do {
        let flags: FeatureFlags? = try await(self.request.get(url: url))
        return flags ?? FeatureFlags(flags: FeatureFlagListing(facebookAuth: true))
      } catch {
        return FeatureFlags(flags: FeatureFlagListing(facebookAuth: true))
      }
    })
  }
}

import Foundation

class Config {
  var flags = FeatureFlags(flags: FeatureFlagListing(
    facebookAuth: true
  ))

#if ENVIRONMENT_DEV
  func apiUrl(_ service: String) -> URL {
    let override = ProcessInfo.processInfo.environment["API_URL"] ?? UserDefaults.standard.string(forKey: "PC_OVERRIDE_API_URL") ?? ""
    if override.starts(with: "http://") {
      return URL(string: override)!
    } else {
      return URL(string: "https://test.api.koasta.com")!
    }
  }
  let squareApplicationId = "CHANGEME"
  let merchantId = "CHANGEME"
#elseif ENVIRONMENT_BETA
  func apiUrl(_ service: String) -> URL {
    return URL(string: UserDefaults.standard.string(forKey: "PC_OVERRIDE_API_URL") ?? "https://test.api.koasta.com")!
  }
  let squareApplicationId = "CHANGEME"
  let merchantId = "CHANGEME"
#elseif ENVIRONMENT_PROD
  func apiUrl(_ service: String) -> URL {
    return URL(string: "https://api.koasta.com")!
  }
  let squareApplicationId = "CHANGEME"
  let merchantId = "CHANGEME"
#endif
}

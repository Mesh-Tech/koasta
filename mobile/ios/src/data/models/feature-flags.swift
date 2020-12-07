import Foundation

struct FeatureFlagListing: Codable {
  let facebookAuth: Bool?

  enum CodingKeys: String, CodingKey {
    case facebookAuth = "facebook-auth"
  }
}

struct FeatureFlags: Codable {
  let flags: FeatureFlagListing
}

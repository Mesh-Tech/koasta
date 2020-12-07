import Foundation

class Constants {
  #if ENVIRONMENT_DEV
  public static let DataLoadCompanyId = 2
  #elseif ENVIRONMENT_BETA
  public static let DataLoadCompanyId = 2
  #elseif ENVIRONMENT_PROD
  public static let DataLoadCompanyId = 2
  #endif
  public static let TermsAndConditionsUrl = URL(string: "https://s3-eu-west-1.amazonaws.com/koasta-shared-pub/legal/terms-and-conditions.md")!
  public static let PrivacyPolicyUrl = URL(string: "https://s3-eu-west-1.amazonaws.com/koasta-shared-pub/legal/privacy-policy.md")!
}

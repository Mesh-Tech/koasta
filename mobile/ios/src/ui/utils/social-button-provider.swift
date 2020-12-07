import Foundation
import UIKit
import FBSDKLoginKit
import AuthenticationServices

protocol SocialButtonProvider {
  func buildFacebookButton(connectingDelegate: LoginButtonDelegate) -> UIControl
  func buildAppleButton() -> UIControl
}

class DefaultSocialButtonProvider: SocialButtonProvider {
  func buildFacebookButton(connectingDelegate: LoginButtonDelegate) -> UIControl {
    let facebookLoginButton = FBLoginButton()
    facebookLoginButton.delegate = connectingDelegate
    return facebookLoginButton
  }

  func buildAppleButton() -> UIControl {
    return ASAuthorizationAppleIDButton()
  }
}

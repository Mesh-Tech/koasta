import Foundation
import UIKit
import AuthenticationServices
import FBSDKLoginKit

@testable import pubcrawl

class SocialButtonStub: SocialButtonProvider {
  fileprivate var delegate: LoginButtonDelegate?
  fileprivate let randomButton = FBLoginButton()

  func buildFacebookButton(connectingDelegate: LoginButtonDelegate) -> UIControl {
    let ret = UIButton(type: .custom)
    ret.backgroundColor = .grey3
    ret.setTitleColor(.foregroundColour, for: .normal)
    ret.titleLabel?.text = "Continue with Facebook"
    ret.addTarget(self, action: #selector(handleFacebook), for: .touchUpInside)
    delegate = connectingDelegate
    return ret
  }

  func buildAppleButton() -> UIControl {
    let ret = UIButton(type: .custom)
    ret.backgroundColor = .grey3
    ret.setTitleColor(.foregroundColour, for: .normal)
    ret.titleLabel?.text = "Sign in with Apple"
    return ret
  }

  @objc fileprivate func handleFacebook() {
    delegate?.loginButton(randomButton, didCompleteWith: .some(.init(token: .init(tokenString: "abc", permissions: [], declinedPermissions: [], expiredPermissions: [], appID: "bobobo", userID: "bobobo", expirationDate: Date(), refreshDate: Date(), dataAccessExpirationDate: Date()), isCancelled: false, grantedPermissions: [], declinedPermissions: [])), error: nil)
  }
}

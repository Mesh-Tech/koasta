import UIKit

#if DEBUG && (UI_TESTS || ENVIRONMENT_DEV)
import FBSDKLoginKit
import AuthenticationServices

class TestHost: UIViewController, SandboxManager {
  static var shared: TestHost?
  fileprivate var router: Router!

  fileprivate var hasParent = false

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.magenta
  }

  override func viewDidLoad() {
    super.viewDidLoad()
    TestHost.shared = self
    resetEnvironment(purgeData: false)
  }

  override func didMove(toParent parent: UIViewController?) {
    super.didMove(toParent: parent)
    hasParent = parent != nil
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
  }

  func resetEnvironment (smooth: Bool = false, purgeData: Bool = true, generateFakeSession: Bool = false) {
    let navigationController = UIApplication.shared.windows.first?.rootViewController as? UINavigationController
    navigationController?.setViewControllers([self], animated: smooth)
    if let bundle = Bundle.main.bundleIdentifier {
      UserDefaults.standard.removePersistentDomain(forName: bundle)
    }
    if purgeData {
      purgeSandbox()
      LoginManager().logOut()

      if generateFakeSession {
        injectSandboxSession()
      }
    }
    KoastaContainer.reset()
    router = KoastaContainer.router
  }

  var isClean: Bool {
    get {
      return navigationController?.viewControllers.count == 1 && hasParent && view.window != nil
    }
  }

  func launch (_ context: [String:Any]? = nil, smooth: Bool = false) {
    router.replace("/onboarding", context: context, from: navigationController, animated: smooth)
  }

  func launchSignedIn (_ context: [String:Any]? = nil, smooth: Bool = false) {
    router.replace("/home", context: context, from: navigationController, animated: smooth)
  }
}
#endif

import UIKit
import FBSDKCoreKit

@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {
  var window: UIWindow?
  let navController = UINavigationController()
  var config: Config?
  var lifecycleManager: ApplicationLifecycleManager?
  var toaster: Toaster?
  var billingManager: BillingManager?
  var eventEmitter: EventEmitter?

  func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
    UINavigationBar.appearance().titleTextAttributes = [
      NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)
    ]
    UIBarButtonItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.buttonFontSize)], for: .normal)
    UIBarButtonItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.buttonFontSize)], for: .selected)
    UIBarButtonItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.buttonFontSize)], for: .highlighted)
    UIBarButtonItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.buttonFontSize)], for: .disabled)
    UITabBarItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.smallSystemFontSize)], for: .normal)
    UITabBarItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.smallSystemFontSize)], for: .selected)
    UITabBarItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.smallSystemFontSize)], for: .highlighted)
    UITabBarItem.appearance().setTitleTextAttributes([NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.smallSystemFontSize)], for: .disabled)

    window = UIWindow(frame: UIScreen.main.bounds)
    navController.applyDefaultStyling()
    navController.setNavigationBarHidden(true, animated: false)
    window?.rootViewController = navController
    lifecycleManager = KoastaContainer.applicationLifecycleManager
    config = KoastaContainer.config
    toaster = KoastaContainer.toaster
    billingManager = KoastaContainer.billingManager
    eventEmitter = KoastaContainer.globalEmitter

    toaster?.beginListening()
    billingManager?.initialise()

    #if DEBUG && (UI_TESTS || ENVIRONMENT_DEV)
      if let _ = ProcessInfo.processInfo.environment["PC_IS_IN_TEST"] {
        navController.setViewControllers([TestHost()], animated: false)
      } else {
        navController.setViewControllers([KoastaContainer.viewController("LoadingViewController")!], animated: false)
        _ = lifecycleManager?.startApplication(application: application, launchOptions: launchOptions)
      }
    #else
      navController.setViewControllers([KoastaContainer.viewController("LoadingViewController")!], animated: false)
      _ = lifecycleManager?.startApplication(application: application, launchOptions: launchOptions)
    #endif

    window?.makeKeyAndVisible()

    return true
  }

  func application(_ application: UIApplication, open url: URL, sourceApplication: String?, annotation: Any) -> Bool {
    if config?.flags.flags.facebookAuth == true {
      ApplicationDelegate.shared.application(
        application,
        open: url,
        sourceApplication: sourceApplication,
        annotation: annotation
      )
    }

    return lifecycleManager?.open(url, context: annotation) ?? false
  }

  func application(_ application: UIApplication, didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
    let token = deviceToken.hexEncodedString()
    lifecycleManager?.registerPushToken(token)
  }

  func applicationWillTerminate(_ application: UIApplication) {
    toaster?.endListening()
  }

  func application(_ application: UIApplication, didFailToRegisterForRemoteNotificationsWithError error: Error) {
    print(error)
  }

  func application(_ application: UIApplication, didReceiveRemoteNotification userInfo: [AnyHashable : Any], fetchCompletionHandler completionHandler: @escaping (UIBackgroundFetchResult) -> Void) {
    print(userInfo)
    eventEmitter?.emit("orderUpdated")
  }
}

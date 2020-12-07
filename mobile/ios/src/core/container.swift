import Foundation
import UIKit

class KoastaContainer {
  // Singletons
  static var locationManagerProvider = { () -> LocationManagerProvider in
    guard ProcessInfo.processInfo.environment.keys.contains("PC_OVERRIDE_LAT")
      || ProcessInfo.processInfo.environment.keys.contains("PC_OVERRIDE_LON")
      || UserDefaults.standard.double(forKey: "PC_OVERRIDE_LAT") > 0
      || UserDefaults.standard.double(forKey: "PC_OVERRIDE_LON") > 0 else {
        return PhysicalLocationManagerProvider()
    }

    return HardcodedLocationManagerProvider()
  }
  static var config = Config()
  static var permissionsUtil = PermissionsUtil(locationProvider: locationManagerProvider())
  static var facebookSessionProvider: SessionProvider = FacebookSessionProvider(config: config)
  static var appleSessionProvider: SessionProvider = AppleSessionProvider()
  static var sessionManager: SessionManager = SessionManagerImpl(facebookSessionProvider: facebookSessionProvider, appleSessionProvider: appleSessionProvider)
  static var globalEmitter = EventEmitter()
  static var authManager = AuthManager(sessionManager: sessionManager, config: config, globalEmitter: globalEmitter)
  static var eventListenerRegistry = EventListenerRegistry()
  static var request = Request(authManager: authManager, registry: eventListenerRegistry, sessionManager: sessionManager)
  static var venueApi = WebVenueApi(request: request, config: config)
  static var menuListApi = WebMenuListApi(request: request, config: config)
  static var authApi = WebAuthApi(request: request, config: config)
  static var orderApi = WebOrderApi(request: request, config: config)
  static var userApi = WebUserApi(request: request, config: config)
  static var legalApi = WebLegalApi()
  static var flagApi = WebFlagApi(request: request, config: config)
  static var api: Api = WebApi(venueApi: venueApi, menuListApi: menuListApi, authApi: authApi, orderApi: orderApi, userApi: userApi, legalApi: legalApi, flagApi: flagApi)
  static var applicationLifecycleManager: ApplicationLifecycleManager = ApplicationLifecycleManagerImpl(router: router, sessionManager: sessionManager, authManager: authManager, registry: eventListenerRegistry, api: api, config: config)
  static var toaster = Toaster()
  static var socialButtonProvider: SocialButtonProvider = DefaultSocialButtonProvider()
  static var billingManager: BillingManager = SquareBillingManager(config: config, session: sessionManager)
  static var router: Router = {
    let router = Router()
    router.defaultScheme = "pubcrawl"

    router.map("/onboarding", "OnboardingViewController", cached: true)
    router.map("/home", "NearbyViewController", cached: true)
    router.map("/orders", "OrdersViewController", cached: true)
    router.map("/settings", "SettingsViewController", cached: true)
    router.map("/pub/<string:venueId>", "VenueOverviewViewController")
    router.map("/pub-onboarding/<string:venueId>", "VenueOnboardingViewController")
    router.map("/pub/<string:venueId>/info", "VenueViewController")
    router.map("/pub/<string:venueId>/info/order", "ConfirmPurchaseViewController")
    router.map("/pub-search", "VenueSearchViewController")
    router.map("/orders/<string:orderId>", "OrderDetailViewController")
    router.map("/active-order", "OrderDetailViewController")
    router.map("/sign-in", "AuthenticationViewController")
    router.map("/settings/beta", "BetaSettingsViewController")
    router.map("/location-request", "LocationRequestViewController")
    router.map("/privacy-policy", "LegalViewController")
    router.map("/terms-and-conditions", "LegalViewController")

    return router
  }()

  // Viewmodels
  static var nearbyViewModel = {
    NearbyViewModel(permissions: permissionsUtil, api: api, sessionManager: sessionManager, locationProvider: locationManagerProvider(), globalEmitter: globalEmitter, registry: eventListenerRegistry)
  }
  static var onboardingViewModel = {
    OnboardingViewModel(sessionManager: sessionManager)
  }
  static var ordersViewModel = {
    OrdersViewModel(api: api, sessionManager: sessionManager)
  }
  static var settingsViewModel = {
    SettingsViewModel()
  }
  static var venueViewModel = {
    VenueViewModel(sessionManager: sessionManager, registry: eventListenerRegistry, globalEmitter: globalEmitter, api: api)
  }
  static var authenticationViewModel = {
    AuthenticationViewModel(sessionManager: sessionManager, api: api, permissions: permissionsUtil)
  }
  static var confirmPurchaseViewModel = {
    ConfirmPurchaseViewModel(sessionManager: sessionManager, api: api, globalEmitter: globalEmitter, listenerRegistry: eventListenerRegistry)
  }
  static var venueOverviewViewModel = {
    VenueOverviewViewModel(api: api, sessionManager: sessionManager, registry: eventListenerRegistry)
  }
  static var locationRequestViewModel = {
    LocationRequestViewModel(permissions: permissionsUtil)
  }
  static var venueSearchViewModel = {
    VenueSearchViewModel(api: api, globalEmitter: globalEmitter)
  }
  static var orderDetailViewModel = {
    OrderDetailViewModel(api: api, permissions: permissionsUtil)
  }
  static var legalViewModel = {
    LegalViewModel(api: api)
  }
  static var venueOnboardingViewModel = {
    VenueOnboardingViewModel(api: api, sessionManager: sessionManager, registry: eventListenerRegistry)
  }

  // View controllers
  static var viewController: (String) -> UIViewController? = { name in
    switch name {
    case "LoadingViewController":
      return LoadingViewController()
    case "NearbyViewController":
      return NearbyViewController(router: router, globalEmitter: globalEmitter, viewModel: nearbyViewModel(), listenerRegistry: eventListenerRegistry, toaster: toaster, sessionManager: sessionManager)
    case "OrdersViewController":
      return OrdersViewController(router: router, globalEmitter: globalEmitter, viewModel: ordersViewModel(), listenerRegistry: eventListenerRegistry, toaster: toaster)
    case "SettingsViewController":
      return SettingsViewController(router: router, globalEmitter: globalEmitter, viewModel: settingsViewModel(), listenerRegistry: eventListenerRegistry, sessionManager: sessionManager)
    case "VenueViewController":
      return VenueViewController(viewModel: venueViewModel(), listenerRegistry: eventListenerRegistry, router: router, toaster: toaster)
    case "AuthenticationViewController":
      return AuthenticationViewController(viewModel: authenticationViewModel(), listenerRegistry: eventListenerRegistry, router: router, toaster: toaster, config: config, socialButtonProvider: socialButtonProvider)
    case "ConfirmPurchaseViewController":
      return ConfirmPurchaseViewController(router: router, globalEmitter: globalEmitter, viewModel: confirmPurchaseViewModel(), listenerRegistry: eventListenerRegistry, billingManager: billingManager)
    case "VenueOverviewViewController":
      return VenueOverviewViewController(viewModel: venueOverviewViewModel(), listenerRegistry: eventListenerRegistry, router: router)
    case "VenueOnboardingViewController":
      return VenueOnboardingViewController(viewModel: venueOnboardingViewModel(), listenerRegistry: eventListenerRegistry, router: router)
    case "BetaSettingsViewController":
      return BetaSettingsViewController(router: router, globalEmitter: globalEmitter)
    case "OnboardingViewController":
      return OnboardingViewController(viewModel: onboardingViewModel(), listenerRegistry: eventListenerRegistry, router: router, toaster: toaster)
    case "LocationRequestViewController":
      return LocationRequestViewController(viewModel: locationRequestViewModel(), listenerRegistry: eventListenerRegistry, router: router)
    case "VenueSearchViewController":
      return VenueSearchController(router: router, viewModel: venueSearchViewModel(), listenerRegistry: eventListenerRegistry, globalEmitter: globalEmitter)
    case "OrderDetailViewController":
      return OrderDetailViewController(router: router, viewModel: orderDetailViewModel(), listenerRegistry: eventListenerRegistry, globalEmitter: globalEmitter, permissions: permissionsUtil)
    case "LegalViewController":
      return LegalViewController(viewModel: legalViewModel(), listenerRegistry: eventListenerRegistry, router: router, toaster: toaster)
    default:
      return nil
    }
  }

  #if UI_TESTS || ENVIRONMENT_DEV

  static func reset() {
    locationManagerProvider = { () -> LocationManagerProvider in
      guard ProcessInfo.processInfo.environment.keys.contains("PC_OVERRIDE_LAT")
        || ProcessInfo.processInfo.environment.keys.contains("PC_OVERRIDE_LON")
        || UserDefaults.standard.double(forKey: "PC_OVERRIDE_LAT") > 0
        || UserDefaults.standard.double(forKey: "PC_OVERRIDE_LON") > 0 else {
          return PhysicalLocationManagerProvider()
      }

      return HardcodedLocationManagerProvider()
    }
    config = Config()
    permissionsUtil = PermissionsUtil(locationProvider: locationManagerProvider())
    facebookSessionProvider = FacebookSessionProvider(config: config)
    appleSessionProvider = AppleSessionProvider()
    sessionManager = SessionManagerImpl(facebookSessionProvider: facebookSessionProvider, appleSessionProvider: appleSessionProvider)
    globalEmitter = EventEmitter()
    authManager = AuthManager(sessionManager: sessionManager, config: config, globalEmitter: globalEmitter)
    eventListenerRegistry = EventListenerRegistry()
    request = Request(authManager: authManager, registry: eventListenerRegistry, sessionManager: sessionManager)
    venueApi = WebVenueApi(request: request, config: config)
    menuListApi = WebMenuListApi(request: request, config: config)
    authApi = WebAuthApi(request: request, config: config)
    orderApi = WebOrderApi(request: request, config: config)
    userApi = WebUserApi(request: request, config: config)
    legalApi = WebLegalApi()
    flagApi = WebFlagApi(request: request, config: config)
    api = WebApi(venueApi: venueApi, menuListApi: menuListApi, authApi: authApi, orderApi: orderApi, userApi: userApi, legalApi: legalApi, flagApi: flagApi)
    applicationLifecycleManager = ApplicationLifecycleManagerImpl(router: router, sessionManager: sessionManager, authManager: authManager, registry: eventListenerRegistry, api: api, config: config)
    toaster = Toaster()
    socialButtonProvider = DefaultSocialButtonProvider()
    billingManager = SquareBillingManager(config: config, session: sessionManager)
    router = {
       let router = Router()
       router.defaultScheme = "pubcrawl"

       router.map("/onboarding", "OnboardingViewController", cached: true)
       router.map("/home", "NearbyViewController", cached: true)
       router.map("/orders", "OrdersViewController", cached: true)
       router.map("/settings", "SettingsViewController", cached: true)
       router.map("/pub/<string:venueId>", "VenueOverviewViewController")
       router.map("/pub/<string:venueId>/info", "VenueViewController")
       router.map("/pub/<string:venueId>/info/order", "ConfirmPurchaseViewController")
       router.map("/pub-search", "VenueSearchViewController")
       router.map("/orders/<string:orderId>", "OrderDetailViewController")
       router.map("/active-order", "OrderDetailViewController")
       router.map("/sign-in", "AuthenticationViewController")
       router.map("/settings/beta", "BetaSettingsViewController")
       router.map("/location-request", "LocationRequestViewController")
       router.map("/privacy-policy", "LegalViewController")
       router.map("/terms-and-conditions", "LegalViewController")

       return router
     }()

    // Viewmodels
    nearbyViewModel = {
      NearbyViewModel(permissions: permissionsUtil, api: api, sessionManager: sessionManager, locationProvider: locationManagerProvider(), globalEmitter: globalEmitter, registry: eventListenerRegistry)
    }
    onboardingViewModel = {
      OnboardingViewModel(sessionManager: sessionManager)
    }
    ordersViewModel = {
      OrdersViewModel(api: api, sessionManager: sessionManager)
    }
    settingsViewModel = {
      SettingsViewModel()
    }
    venueViewModel = {
      VenueViewModel(sessionManager: sessionManager, registry: eventListenerRegistry, globalEmitter: globalEmitter, api: api)
    }
    authenticationViewModel = {
      AuthenticationViewModel(sessionManager: sessionManager, api: api, permissions: permissionsUtil)
    }
    confirmPurchaseViewModel = {
      ConfirmPurchaseViewModel(sessionManager: sessionManager, api: api, globalEmitter: globalEmitter, listenerRegistry: eventListenerRegistry)
    }
    venueOverviewViewModel = {
      VenueOverviewViewModel(api: api, sessionManager: sessionManager, registry: eventListenerRegistry)
    }
    locationRequestViewModel = {
      LocationRequestViewModel(permissions: permissionsUtil)
    }
    venueSearchViewModel = {
      VenueSearchViewModel(api: api, globalEmitter: globalEmitter)
    }
    orderDetailViewModel = {
      OrderDetailViewModel(api: api, permissions: permissionsUtil)
    }
    legalViewModel = {
      LegalViewModel(api: api)
    }
  }

  #endif
}

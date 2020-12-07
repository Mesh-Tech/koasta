import XCTest
import KIF

@testable import pubcrawl

class UI_0_SplashScreenTestCase: XCTestCase {
  override func setUp () {
    super.setUp()
    continueAfterFailure = false
    TestHost.shared?.resetEnvironment()
    tester.wait(forTimeInterval: 1, relativeToAnimationSpeed: true)
  }

  override func tearDown () {
    super.tearDown()
    TestHost.shared?.resetEnvironment()
  }

  func test_Scenario_1 () {
    scenario("1: Splash Screen: Splash screen shows on first launch.")
    given("application is launched") {
      TestHost.shared?.launch()
    }

    _ = XCTContext.runActivity(named: "I should eventually see the splash screen") { _ in
      assertFinishedDisplayingSplashScreen()
    }
  }

  func test_Scenario_2 () {
    scenario("2: Splash Screen: Get started button navigates away from the current screen.")
    given("application is launched") {
      TestHost.shared?.launch()
    }

    then("I should see the splash screen") {
      assertFinishedDisplayingSplashScreen()
    }

    then("I tap the 'Get started' button") {
      tester.tapView(withAccessibilityIdentifier: "splashGetStartedButton")
    }

    then("I should not see the splash screen") {
      assertNotDisplayingSplashScreen()
    }
  }

  func test_Scenario_3 () {
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()
    let buttonStub = SocialButtonStub()

    permissionsStub.initialPermissionState = .allowed
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = []
    sessionStub.isAuthenticated = false

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }
    KoastaContainer.socialButtonProvider = buttonStub

    scenario("3: Splash Screen: Can sign in with a social provider")
    given("application is launched") {
      TestHost.shared?.launch()
    }

    then("I should see the splash screen") {
      assertFinishedDisplayingSplashScreen()
    }

    then("I tap the 'Get started' button") {
      tester.tapView(withAccessibilityIdentifier: "splashGetStartedButton")
    }

    then("I should eventually see the authentication screen") {
      assertDisplayingAuthenticationScreen()
    }

    then("when I tap on Sign in with Facebook") {
      sessionStub.isAuthenticated = true
      sessionStub.currentSession = StubSession()
      tester.tapView(withAccessibilityIdentifier: "facebookLoginButton")
    }

    then("I should eventually see the nearby venue screen") {
      assertNotDisplayingAuthenticationScreen()
    }
  }
}

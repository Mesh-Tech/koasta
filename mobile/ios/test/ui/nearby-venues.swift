import XCTest
import KIF

@testable import pubcrawl

class UI_2_NearbyVenuesScreenTestCase: XCTestCase {
  override func setUp () {
    super.setUp()
    continueAfterFailure = false
    TestHost.shared?.resetEnvironment(generateFakeSession: true)
    tester.wait(forTimeInterval: 1, relativeToAnimationSpeed: true)
  }

  override func tearDown () {
    super.tearDown()
    TestHost.shared?.resetEnvironment()
  }

  func test_Scenario_3 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("3: Nearby Venues: Venue information shows after loading a venue")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I tap on a venue") {
      tester.tapRow(at: IndexPath(row: 0, section: 1), inTableViewWithAccessibilityIdentifier: "venueList")
    }

    then("after a little while I should see the venue screen") {
      assertDisplayingVenueScreen()
    }
  }

  func test_Scenario_4 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: StubLocationManagerProvider())
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "A", venueAddress: "B", venuePostCode: "C", venuePhoneNumber: "D", venueContact: "E", venueDescription: "F", venueNotes: "G", imageUrl: nil, tags: nil, venueLatitude: "H", venueLongitude: "I", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getMenuListResult = [
      Menu(menuId: 0, menuName: "A", menuDescription: "B", products: [
        Product(productId: 0, ageRestricted: false, productType: "B", productName: "C", productDescription: "D", price: 10.0, image: nil)
    ])]
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("4: Nearby Venues: Menus can be viewed from the overview")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I tap on a venue") {
      tester.tapRow(at: IndexPath(row: 0, section: 1), inTableViewWithAccessibilityIdentifier: "venueList")
    }

    then("after a little while I should see the venue screen") {
      assertDisplayingVenueScreen()
    }

    then("I tap on the first menu in the list") {
      tester.tapRow(at: IndexPath(row: 0, section: 1), inTableViewWithAccessibilityIdentifier: "venueMenuLists")
    }

    then("I should navigate to the next screen") {
      assertNotDisplayingVenueScreen()
    }
  }

  func test_Scenario_5 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getNearbyVenuesShouldThrow = true

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("5: Nearby Venues: Error shown when fetching venues fails")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I should see an error") {
      assertDisplayingToast()
    }
  }

  func test_Scenario_6 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = [
      Venue(venueId: 1, venueCode: "", companyId: 2, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("6: Nearby Venues: Voting venues are shown")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I should see a voting venue and summary text") {
      assertDisplayingVotingVenue()
    }
  }

  func test_Scenario_7 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!,
      Venue(venueId: 1, venueCode: "", companyId: 2, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("7: Nearby Venues: Voting venues are shown")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I should see a voting venue") {
      assertDisplayingVotingVenueAndNormalVenue()
    }
  }

  func test_Scenario_9 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = []

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("9: Nearby Venues: Summary text shown when no venues returned")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I should see an empty state") {
      assertDisplayingVenueListEmptyState()
    }
  }
}


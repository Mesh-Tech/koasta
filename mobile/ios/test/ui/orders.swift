import XCTest
import KIF

@testable import pubcrawl

class UI_3_OrdersScreenTestCase: XCTestCase {
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

  func test_Scenario_1 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]
    apiStub.getOrdersResult = []

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("1: Orders: No orders displayed if you haven't made any")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I tap on Orders") {
      selectTab(1)
    }

    then("after a little while I should see the orders screen") {
      assertDisplayingOrdersScreen()
    }

    then("I should see an empty state") {
      assertDisplayingOrdersEmptyState()
    }
  }

  func test_Scenario_2 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]
    apiStub.getOrdersResult = [
      HistoricalOrder(companyId: 0, externalPaymentId: "a", firstName: "b", lastName: "c", orderId: 1, orderNumber: 2, orderStatus: 3, orderTimeStamp: Date(), userId: 4, venueName: "d", lineItems: [
        HistoricalOrderItem(amount: 10.49, productName: "e", quantity: 5)
      ], total: 10, serviceCharge: 0.2, orderNotes: nil, servingType: .barService, table: nil)
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("2: Orders: Historical orders are displayed")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I tap on Orders") {
      selectTab(1)
    }

    then("after a little while I should see the orders screen") {
      assertDisplayingOrdersScreen()
    }

    then("I should see an order item") {
      assertDisplayingOrderItems()
    }
  }

  func test_Scenario_3 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: locationStub)
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()

    permissionsStub.initialPermissionState = .allowed
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "a", venueAddress: "b", venuePostCode: "c", venuePhoneNumber: "d", venueContact: "e", venueDescription: "f", venueNotes: "g", imageUrl: "h", tags: nil, venueLatitude: "i", venueLongitude: "j", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]
    apiStub.getOrdersResult = [
      HistoricalOrder(companyId: 0, externalPaymentId: "a", firstName: "b", lastName: "c", orderId: 1, orderNumber: 2, orderStatus: 3, orderTimeStamp: Date(), userId: 4, venueName: "d", lineItems: [
        HistoricalOrderItem(amount: 10.49, productName: "e", quantity: 5)
      ], total: 10, serviceCharge: 0.2, orderNotes: nil, servingType: .barService, table: nil)
    ]
    apiStub.getOrderResult = apiStub.getOrdersResult?[0]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("3: Orders: Tapping a historical order navigates to order summary")
    given("application is launched") {
      TestHost.shared?.launchSignedIn()
    }

    then("after a little while I should see the venue list") {
      assertDisplayingNearbyVenuesScreen()
    }

    then("I tap on Orders") {
      selectTab(1)
    }

    then("after a little while I should see the orders screen") {
      assertDisplayingOrdersScreen()
    }

    then("I should see an order item") {
      assertDisplayingOrderItems()
    }

    then("when tapping on an order item") {
      tester.tapRow(at: IndexPath(row: 0, section: 0), inTableViewWithAccessibilityIdentifier: "ordersList")
    }

    then("I should no longer see the orders screen") {
      assertNotDisplayingOrdersScreen()
    }
  }
}

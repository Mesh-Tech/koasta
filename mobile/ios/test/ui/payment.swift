import XCTest
import KIF

@testable import pubcrawl

class UI_5_PaymentScreenTestCase: XCTestCase {
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
      ])
    ]
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("2: Payments: Can build an order with multiple items")
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

    then("after a little while I see the product list") {
      assertDisplayingProducts()
    }

    then("when I tap on a product") {
      tester.tapRow(at: IndexPath(row: 0, section: 0), inTableViewWithAccessibilityIdentifier: "venueProductList")
    }

    then("after a little while I see the order bar") {
      assertDisplayingOrderBar()
    }

    then("the order bar is updated with the summary of my order") {
      let quantity = tester.waitForView(withAccessibilityIdentifier: "orderBarQuantity") as! UILabel
      let amount = tester.waitForView(withAccessibilityIdentifier: "orderBarAmount") as! UILabel

      XCTAssertEqual(quantity.text, "1 item")
      XCTAssertEqual(amount.text, "£10.00")
    }
  }

  func test_Scenario_2 () {
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
      ]),
      Menu(menuId: 0, menuName: "B", menuDescription: "C", products: [
        Product(productId: 0, ageRestricted: false, productType: "B", productName: "C", productDescription: "D", price: 5.0, image: nil)
      ])
    ]
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("1: Payments: Can build an order with a single item")
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

    then("after a little while I see the product list") {
      assertDisplayingProducts()
    }

    then("when I tap on a product") {
      tester.tapRow(at: IndexPath(row: 0, section: 0), inTableViewWithAccessibilityIdentifier: "venueProductList")
    }

    then("after a little while I see the order bar") {
      assertDisplayingOrderBar()
    }

    then("the order bar is updated with the summary of my order") {
      let quantity = tester.waitForView(withAccessibilityIdentifier: "orderBarQuantity") as! UILabel
      let amount = tester.waitForView(withAccessibilityIdentifier: "orderBarAmount") as! UILabel

      XCTAssertEqual(quantity.text, "1 item")
      XCTAssertEqual(amount.text, "£10.00")
    }

    then("then when I tap on the other menu") {
      tester.tapView(withAccessibilityIdentifier: "menuItem1")
    }

    then("when I tap on a product") {
      tester.tapRow(at: IndexPath(row: 0, section: 0), inTableViewWithAccessibilityIdentifier: "venueProductList")
    }

    then("the order bar is updated with the summary of my order") {
      let quantity = tester.waitForView(withAccessibilityIdentifier: "orderBarQuantity") as! UILabel
      let amount = tester.waitForView(withAccessibilityIdentifier: "orderBarAmount") as! UILabel

      XCTAssertEqual(quantity.text, "2 items")
      XCTAssertEqual(amount.text, "£15.00")
    }
  }

  func test_Scenario_3 () {
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
      ])
    ]
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]
    apiStub.getEstimateResult = EstimateOrderResult(receiptLines: [
      EstimateReceiptLine(amount: 10.0, total: 10.0, quantity: 1, title: "C"),
      EstimateReceiptLine(amount: 0, total: 0, quantity: 0, title: "Service charge")
    ], receiptTotal: 10.0)

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }

    scenario("3: Payments: Can view a summary of my order")
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

    then("after a little while I see the product list") {
      assertDisplayingProducts()
    }

    then("when I tap on a product") {
      tester.tapRow(at: IndexPath(row: 0, section: 0), inTableViewWithAccessibilityIdentifier: "venueProductList")
    }

    then("after a little while I see the order bar") {
      assertDisplayingOrderBar()
    }

    then("the order bar is updated with the summary of my order") {
      let quantity = tester.waitForView(withAccessibilityIdentifier: "orderBarQuantity") as! UILabel
      let amount = tester.waitForView(withAccessibilityIdentifier: "orderBarAmount") as! UILabel

      XCTAssertEqual(quantity.text, "1 item")
      XCTAssertEqual(amount.text, "£10.00")
    }

    then("when I tap on show order") {
      tester.tapView(withAccessibilityIdentifier: "orderBarTitle")
    }

    then("after a little while I should see the confirm order screen") {
      assertDisplayingConfirmOrderScreen()
    }
  }

  func test_Scenario_4 () {
    // Setup mocks to stub out real functionality
    let locationStub = StubLocationManagerProvider()
    let permissionsStub = StubPermissionsUtil(locationProvider: StubLocationManagerProvider())
    let qrCaptureStub = StubQRCaptureUtil()
    let apiStub = StubApi()
    let sessionStub = StubSessionManager()
    let billingStub = StubBillingManager()

    permissionsStub.initialPermissionState = .allowed
    qrCaptureStub.scheduledCaptures.append(StubQRCaptureUtil.ScheduledCapture(qrCode: "ABC", sleepMs: 2))
    apiStub.getVenueResult = Venue(venueId: 0, venueCode: "", companyId: 0, venueName: "A", venueAddress: "B", venuePostCode: "C", venuePhoneNumber: "D", venueContact: "E", venueDescription: "F", venueNotes: "G", imageUrl: nil, tags: nil, venueLatitude: "H", venueLongitude: "I", externalLocationId: "k", progress: 1, isOpen: true, servingType: .barService)
    apiStub.getMenuListResult = [
      Menu(menuId: 0, menuName: "A", menuDescription: "B", products: [
        Product(productId: 0, ageRestricted: false, productType: "B", productName: "C", productDescription: "D", price: 10.0, image: nil)
      ])
    ]
    apiStub.getNearbyVenuesResult = [
      apiStub.getVenueResult!
    ]
    apiStub.getEstimateResult = EstimateOrderResult(receiptLines: [
      EstimateReceiptLine(amount: 10.0, total: 10.0, quantity: 1, title: "C"),
      EstimateReceiptLine(amount: 0, total: 0, quantity: 0, title: "Service charge")
    ], receiptTotal: 10.0)
    apiStub.sendOrderResult = SendOrderResult(orderNumber: 1, orderId: 2, status: 1, total: 10.0, serviceCharge: 0.0)

    KoastaContainer.permissionsUtil = permissionsStub
    KoastaContainer.api = apiStub
    KoastaContainer.sessionManager = sessionStub
    KoastaContainer.locationManagerProvider = {
      return locationStub
    }
    KoastaContainer.billingManager = billingStub

    scenario("4: Payments: Can confirm an order and see its status")
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

    then("after a little while I see the product list") {
      assertDisplayingProducts()
    }

    then("when I tap on a product") {
      tester.tapRow(at: IndexPath(row: 0, section: 0), inTableViewWithAccessibilityIdentifier: "venueProductList")
    }

    then("after a little while I see the order bar") {
      assertDisplayingOrderBar()
    }

    then("the order bar is updated with the summary of my order") {
      let quantity = tester.waitForView(withAccessibilityIdentifier: "orderBarQuantity") as! UILabel
      let amount = tester.waitForView(withAccessibilityIdentifier: "orderBarAmount") as! UILabel

      XCTAssertEqual(quantity.text, "1 item")
      XCTAssertEqual(amount.text, "£10.00")
    }

    then("when I tap on show order") {
      tester.tapView(withAccessibilityIdentifier: "orderBarTitle")
    }

    then("after a little while I should see the confirm order screen") {
      assertDisplayingConfirmOrderScreen()
    }

    then("when I tap on confirm payment") {
      tester.tapView(withAccessibilityIdentifier: "confirmButton")
    }

    then("after a little while I should see the order status screen") {
      assertDisplayingOrderSummaryScreen()
    }
  }
}

import XCTest
import Foundation

extension XCTestCase {
  func assertFinishedDisplayingSplashScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "splashPageTitle"))
    XCTAssertNoThrow(tester.usingTimeout(0).waitForView(withAccessibilityIdentifier: "splashGetStartedButton"))
  }

  func assertStartedDisplayingSplashScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "splashPageTitle"))
    XCTAssertNoThrow(tester.usingTimeout(0).waitForView(withAccessibilityIdentifier: "splashGetStartedButton"))
  }

  func assertNotDisplayingSplashScreen () {
    tester.waitForAbsenceOfView(withAccessibilityIdentifier: "splashPageTitle")
    tester.waitForAbsenceOfView(withAccessibilityIdentifier: "splashGetStartedButton")
  }

  func assertDisplayingNearbyVenuesScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "venueList"))
  }

  func assertDisplayingVenueScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "venueTitle"))
  }

  func assertNotDisplayingVenueScreen () {
    XCTAssertNoThrow(tester.waitForAbsenceOfView(withAccessibilityIdentifier: "venueTitle"))
  }

  func assertDisplayingAuthenticationScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "authTitle"))
  }

  func assertNotDisplayingAuthenticationScreen () {
    XCTAssertNoThrow(tester.waitForAbsenceOfView(withAccessibilityIdentifier: "authTitle"))
  }

  func assertDisplayingToast () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "toastTitle"))
  }

  func assertDisplayingVotingVenue () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "noLaunchedVenuesTitleView"))
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "votingVenueTitle"))
  }

  func assertDisplayingVotingVenueAndNormalVenue () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "venueTitle"))
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "votingVenueTitle"))
  }

  func assertDisplayingVenueListEmptyState () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "noVenuesTitleView"))
  }

  func assertDisplayingOrdersScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "ordersList"))
  }

  func assertNotDisplayingOrdersScreen () {
    XCTAssertNoThrow(tester.waitForAbsenceOfView(withAccessibilityIdentifier: "ordersList"))
  }

  func assertDisplayingOrdersEmptyState() {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "ordersEmptyTitle"))
  }

  func assertDisplayingOrderItems() {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "historicalOrderItemTitle"))
  }

  func assertDisplayingOrderSummaryScreen() {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "orderSummaryList"))
  }

  func assertDisplayingProducts() {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "venueProductList"))
  }

  func assertDisplayingOrderBar() {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "orderBar"))
  }

  func assertDisplayingConfirmOrderScreen () {
    XCTAssertNoThrow(tester.waitForView(withAccessibilityIdentifier: "confirmPurchaseTitle"))
  }

  func assertNotDisplayingConfirmOrderScreen () {
    XCTAssertNoThrow(tester.waitForAbsenceOfView(withAccessibilityIdentifier: "confirmPurchaseTitle"))
  }

  func selectTab(_ index: Int) {
    let tabBar = wait(forIdentifier: "tabBar") as! UITabBar
    tabBar.selectedItem = tabBar.items![index]
    tabBar.delegate?.tabBar?(tabBar, didSelect: tabBar.items![index])
  }
}

import Foundation
import UIKit
import Hydra
import CoreLocation

@testable import pubcrawl

class StubAsyncLocationManagerDelegate: AsyncLocationManagerDelegate {
  public var location = CLLocation(latitude: 51.0, longitude: -1.10)

  override init() {}

  override func requestWhenInUseAuthorisation() -> Promise<CLAuthorizationStatus> {
    return Promise(resolved: CLAuthorizationStatus.authorizedAlways)
  }

  override func requestLocation() -> Promise<[CLLocation]> {
    return Promise(resolved: [location])
  }

  override func locationManager(_ manager: CLLocationManager, didChangeAuthorization status: CLAuthorizationStatus) {
  }

  override func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {
  }
}

class StubLocationManagerProvider: LocationManagerProvider {
  public var stubDelegate = StubAsyncLocationManagerDelegate()

  func getDelegate() -> AsyncLocationManagerDelegate {
    return stubDelegate
  }
}

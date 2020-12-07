import Foundation
import CoreLocation
import Hydra

private class HardcodedLocationManagerDelegate: AsyncLocationManagerDelegate {
  fileprivate let hardcodedLat: Double
  fileprivate let hardcodedLon: Double

  override init () {
    if let envLat = ProcessInfo.processInfo.environment["PC_OVERRIDE_LAT"], let envLatD = Double(envLat) {
      hardcodedLat = envLatD
    } else {
      hardcodedLat = UserDefaults.standard.double(forKey: "PC_OVERRIDE_LAT")
    }

    if let envLon = ProcessInfo.processInfo.environment["PC_OVERRIDE_LON"], let envLonD = Double(envLon) {
      hardcodedLon = envLonD
    } else {
      hardcodedLon = UserDefaults.standard.double(forKey: "PC_OVERRIDE_LON")
    }

    super.init()
  }

  override func requestWhenInUseAuthorisation() -> Promise<CLAuthorizationStatus> {
    return Promise(resolved: CLAuthorizationStatus.authorizedWhenInUse)
  }

  override func requestLocation() -> Promise<[CLLocation]> {
    return Promise(resolved: [CLLocation(latitude: hardcodedLat, longitude: hardcodedLon)])
  }

  override func locationManager(_ manager: CLLocationManager, didChangeAuthorization status: CLAuthorizationStatus) {}

  override func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {}
}

class HardcodedLocationManagerProvider: LocationManagerProvider {
  func getDelegate() -> AsyncLocationManagerDelegate {
    return HardcodedLocationManagerDelegate()
  }
}

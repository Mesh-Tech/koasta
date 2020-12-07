import Foundation
import CoreLocation
import Hydra

class AsyncLocationManagerDelegate: NSObject, CLLocationManagerDelegate {
  fileprivate let manager = CLLocationManager()
  fileprivate var requestWhenInUseAuthorisationCb: ((CLAuthorizationStatus) -> Void)?
  fileprivate var requestLocationCb: (([CLLocation]) -> Void)?
  fileprivate var requestLocationTimeoutToken: String?
  fileprivate var requestLocationTimeoutAttempts: Int = 0

  override init () {
    super.init()
    manager.delegate = self
  }

  open func requestWhenInUseAuthorisation() -> Promise<CLAuthorizationStatus> {
    return Promise(in: .main, token: nil, { [weak self] (resolve, _, _) in
      self?.requestWhenInUseAuthorisationCb = { [weak self] status in
        self?.requestWhenInUseAuthorisationCb = nil
        resolve(status)
      }
      self?.manager.requestWhenInUseAuthorization()
    })
  }

  open func requestLocation() -> Promise<[CLLocation]> {
    requestLocationTimeoutToken = UUID().uuidString
    requestLocationTimeoutAttempts = 0
    return Promise(in: .main, token: nil, { [weak self] (resolve, _, _) in
      self?.requestLocationCb = { [weak self] locations in
        self?.manager.stopUpdatingLocation()
        self?.requestLocationCb = nil
        self?.requestLocationTimeoutAttempts = 0
        resolve(locations)
      }
      self?.manager.startUpdatingLocation()
    })
  }

  open func locationManager(_ manager: CLLocationManager, didChangeAuthorization status: CLAuthorizationStatus) {
    requestWhenInUseAuthorisationCb?(status)
  }

  open func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {
    if requestLocationCb == nil {
      return
    }

    if requestLocationTimeoutAttempts >= 5 {
      requestLocationTimeoutToken = nil
      requestLocationCb?(locations)
      return
    }

    let nextToken = UUID().uuidString
    requestLocationTimeoutToken = nextToken
    requestLocationTimeoutAttempts += 1
    DispatchQueue.main.asyncAfter(deadline: .now() + 0.5) { [weak self] in
      guard let strongSelf = self else { return }
      guard strongSelf.requestLocationTimeoutToken == nextToken else {
        if strongSelf.requestLocationCb != nil {
          strongSelf.requestLocationTimeoutAttempts += 1
        }
        return
      }

      strongSelf.requestLocationTimeoutToken = nil
      strongSelf.requestLocationCb?(locations)
    }
  }
}

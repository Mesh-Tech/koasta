import Foundation
import CoreLocation

extension Venue {
  func evaluateDistanceMetersFromLocation(_ location: CLLocation) -> Double {
    let point = CLLocation(latitude: venueLatitude.doubleValue(), longitude: venueLongitude.doubleValue())

    return floor(point.distance(from: location))
  }
}

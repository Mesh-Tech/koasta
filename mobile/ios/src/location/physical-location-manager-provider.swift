import Foundation

class PhysicalLocationManagerProvider: LocationManagerProvider {
  func getDelegate() -> AsyncLocationManagerDelegate {
    return AsyncLocationManagerDelegate()
  }
}

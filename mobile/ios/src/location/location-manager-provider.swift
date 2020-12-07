import Foundation

protocol LocationManagerProvider {
  func getDelegate () -> AsyncLocationManagerDelegate
}

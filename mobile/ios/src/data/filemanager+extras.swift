import Foundation
import Hydra

extension FileManager {
  func fileExists(atUrl url: URL) -> Bool {
    return fileExists(atPath: url.path)
  }
}

import Foundation
import UIKit

class CellImmediatelyUpdatable<T>: UITableViewCell {
  func update(from oldState: T, to newState: T) {}
}

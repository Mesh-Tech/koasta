import Foundation

extension Array {
  func pick(_ n: Int) -> [Element] {
    guard count >= n else {
      fatalError("The count has to be at least \(n)")
    }
    guard n >= 0 else {
      fatalError("The number of elements to be picked must be positive")
    }

    let shuffledIndices = indices.shuffled().prefix(upTo: n)
    return shuffledIndices.map {self[$0]}
  }

  func pickInidices(_ n: Int) -> [Int] {
    guard count >= n else {
      fatalError("The count has to be at least \(n)")
    }
    guard n >= 0 else {
      fatalError("The number of elements to be picked must be positive")
    }

    let shuffledIndices = indices.shuffled().prefix(upTo: n)
    return shuffledIndices.map { $0 }
  }
}

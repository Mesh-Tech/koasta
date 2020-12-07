import Foundation

struct PairIterator<C: IteratorProtocol>: IteratorProtocol {
  private var baseIterator: C
  init(_ iterator: C) {
    baseIterator = iterator
  }

  mutating func next() -> (C.Element, C.Element)? {
    if let left = baseIterator.next(), let right = baseIterator.next() {
      return (left, right)
    }
    return nil
  }
}
extension Sequence {
  var pairs: AnySequence<(Self.Iterator.Element, Self.Iterator.Element)> {
    return AnySequence({PairIterator(self.makeIterator())})
  }
}

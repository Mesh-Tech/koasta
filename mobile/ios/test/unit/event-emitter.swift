import XCTest
@testable import pubcrawl

class EventEmitterTestCase: XCTestCase {
  func testEventIsEquatable () {
    let eventA = Event(name: "a", data: [:])
    let eventB = Event(name: "a", data: [:])

    XCTAssert(eventA == eventB)
  }

  func testEventIsHashable () {
    let eventA = Event(name: "a", data: [:])
    let eventB = Event(name: "a", data: [:])

    var set = Set<Event>()
    set.insert(eventA)
    set.insert(eventB)

    XCTAssert(set.count == 1)
  }

  func testEventHandleIsEquatable () {
    let handleA = EventHandle(eventName: "A", subscriptionId: "B")
    let handleB = EventHandle(eventName: "A", subscriptionId: "B")

    XCTAssert(handleA == handleB)
  }

  func testEventHandleIsHashable () {
    let handleA = EventHandle(eventName: "A", subscriptionId: "B")
    let handleB = EventHandle(eventName: "A", subscriptionId: "B")

    var set = Set<EventHandle>()
    set.insert(handleA)
    set.insert(handleB)

    XCTAssert(set.count == 1)
  }

  func testEventSubscriptionWorks () {
    let emitter = EventEmitter()
    var works = false

    _ = emitter.on("a") { _ in works = true }
    emitter.emit("a")

    XCTAssert(works)
  }

  func testStickyEventSubscriptionWorks () {
    let emitter = EventEmitter()
    var works = false

    emitter.emit("a", sticky: true)
    _ = emitter.on("a") { _ in works = true }

    XCTAssert(works)
  }

  func testMultiEventSubscriptionWorks () {
    let emitter = EventEmitter()
    var hits = 0

    _ = emitter.on("a") { _ in hits += 1 }
    _ = emitter.on("a") { _ in hits += 1 }
    emitter.emit("a")

    XCTAssert(hits == 2)
  }

  func testOnceEventSubscriptionWorks () {
    let emitter = EventEmitter()
    var hits = 0

    _ = emitter.once("a") { _ in hits += 1 }
    emitter.emit("a")
    emitter.emit("a")

    XCTAssert(hits == 1)
  }

  func testStickyOnceEventSubscriptionWorks () {
    let emitter = EventEmitter()
    var hits = 0

    emitter.emit("a", sticky: true)
    _ = emitter.once("a") { _ in hits += 1 }
    emitter.emit("a", sticky: true)

    XCTAssert(hits == 1)
  }

  func testBackgroundEventSubscriptionWorks () {
    let ex = self.expectation(description: "it works")
    let emitter = EventEmitter()
    var works = false

    _ = emitter.on("a") { _ in works = true }
    _ = emitter.on("a") { _ in ex.fulfill() }
    DispatchQueue.global().async {
      emitter.emit("a")
    }

    wait(for: [ex], timeout: 5.0)
    XCTAssert(works)
  }

  func testMultiBackgroundEventSubscriptionWorks () {
    let ex = self.expectation(description: "it works")
    let emitter = EventEmitter()
    var hits = 0

    _ = emitter.on("a") { _ in hits += 1 }
    _ = emitter.on("a") { _ in ex.fulfill() }
    DispatchQueue.global().async {
      _ = emitter.on("a") { _ in hits += 1 }
      emitter.emit("a")
    }

    wait(for: [ex], timeout: 5.0)
    XCTAssert(hits == 2)
  }

  func testBackgroundOnceEventSubscriptionWorks () {
    let ex = self.expectation(description: "it works")
    let emitter = EventEmitter()
    var hits = 0

    DispatchQueue.global().async {
      _ = emitter.once("a") { _ in
        hits += 1
        ex.fulfill()
      }
      DispatchQueue.main.async {
        emitter.emit("a")
        emitter.emit("a")
      }
    }

    wait(for: [ex], timeout: 5.0)
    XCTAssert(hits == 1)
  }

  func testRemoveAllListenersWorks () {
    let emitter = EventEmitter()
    var failed = false

    _ = emitter.on("a") { _ in failed = true }
    _ = emitter.on("a") { _ in failed = true }

    emitter.removeAllListeners("a")

    XCTAssert(!failed)
  }

  func testRemoveListenerWorks () {
    let emitter = EventEmitter()
    var failed = false

    _ = emitter.on("a") { _ in }
    let b = emitter.on("a") { _ in failed = true }

    emitter.removeListener(b)

    XCTAssert(!failed)
  }

  func testRemoveNonExistantListenerWorks () {
    let emitter = EventEmitter()
    let emitter2 = EventEmitter()
    var failed = true

    let a = emitter.on("a") { _ in failed = false }
    emitter2.removeListener(a)

    emitter.emit("a")
    XCTAssert(!failed)
  }

  func testBackgroundRemoveAllListenersWorks () {
    let ex = self.expectation(description: "it works")
    let emitter = EventEmitter()
    var failed = false

    _ = emitter.on("a") { _ in failed = true }
    _ = emitter.on("a") { _ in failed = true }

    DispatchQueue.global().async {
      emitter.removeAllListeners("a")

      DispatchQueue.global().async {
        ex.fulfill()
      }
    }

    wait(for: [ex], timeout: 5.0)
    XCTAssert(!failed)
  }

  func testBackgroundRemoveListenerWorks () {
    let ex = self.expectation(description: "it works")
    let emitter = EventEmitter()
    var failed = false

    _ = emitter.on("a") { _ in }
    let b = emitter.on("a") { _ in failed = true }

    DispatchQueue.global().async {
      emitter.removeListener(b)

      DispatchQueue.global().async {
        ex.fulfill()
      }
    }

    wait(for: [ex], timeout: 5.0)
    XCTAssert(!failed)
  }
}

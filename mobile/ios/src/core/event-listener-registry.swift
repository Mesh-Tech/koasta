import Foundation

infix operator <~

func <~ (value1: EventListenerRegistry, value2: EventHandle) {
  value1.add(value2)
}

class EventListenerRegistry {
  fileprivate var handles = Set<EventHandle>()
  fileprivate var emitter: EventEmitter?

  func attached(to emitter: EventEmitter?) -> EventListenerRegistry {
    self.emitter = emitter
    return self
  }

  func add(_ handle: EventHandle) {
    handles.insert(handle)
  }

  deinit {
    handles.forEach { emitter?.removeListener($0) }
    handles = Set()
    emitter = nil
  }
}

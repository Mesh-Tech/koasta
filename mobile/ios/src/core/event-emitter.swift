import Foundation

struct Event: Hashable, Equatable {
  static func ==(lhs: Event, rhs: Event) -> Bool { return lhs.name == rhs.name }
  func hash(into hasher: inout Hasher) {
    hasher.combine(name.hashValue)
  }

  let name: String
  let data: EventData
}

struct EventHandle: Hashable, Equatable {
  static func ==(lhs: EventHandle, rhs: EventHandle) -> Bool { return lhs.subscriptionId == rhs.subscriptionId }
  func hash(into hasher: inout Hasher) {
    hasher.combine(subscriptionId.hashValue)
  }

  let eventName: String
  let subscriptionId: String
}

typealias EventHandler = (Event) -> Void
typealias EventData = [String:Any?]

private struct EventListener: Hashable, Equatable {
  static func ==(lhs: EventListener, rhs: EventListener) -> Bool { return lhs.id == rhs.id }
  func hash(into hasher: inout Hasher) {
    hasher.combine(id.hashValue)
  }

  let id = UUID().uuidString
  let shouldRemoveImmediately: Bool
  let handler: EventHandler
}

class EventEmitter {
  fileprivate var listeners: [String:Set<EventListener>] = Dictionary()
  fileprivate var stickyEvents: [String:EventData] = Dictionary()

  func on(_ eventName: String, callback: @escaping EventHandler) -> EventHandle {
    let listener = EventListener(shouldRemoveImmediately: false, handler: callback)

    guard Thread.isMainThread else {
      DispatchQueue.main.async { [weak self] in self?.addListener(listener, for: eventName) }
      return EventHandle(eventName: eventName, subscriptionId: listener.id)
    }

    self.addListener(listener, for: eventName)

    if let stickyEventData = stickyEvents[eventName] {
      callback(Event(name: eventName, data: stickyEventData))
    }

    return EventHandle(eventName: eventName, subscriptionId: listener.id)
  }

  func once(_ eventName: String, callback: @escaping EventHandler) -> EventHandle {
    let listener = EventListener(shouldRemoveImmediately: true, handler: callback)

    guard Thread.isMainThread else {
      DispatchQueue.main.async { [weak self] in self?.addListener(listener, for: eventName) }
      return EventHandle(eventName: eventName, subscriptionId: listener.id)
    }

    if let stickyEventData = stickyEvents[eventName] {
      callback(Event(name: eventName, data: stickyEventData))
      return EventHandle(eventName: eventName, subscriptionId: listener.id)
    }

    self.addListener(listener, for: eventName)
    return EventHandle(eventName: eventName, subscriptionId: listener.id)
  }

  fileprivate func addListener(_ listener: EventListener, for eventName: String) {
    var handlers: Set<EventListener>!

    if let existingHandlers = listeners[eventName] {
      handlers = existingHandlers
      handlers.insert(listener)
    } else {
      handlers = Set([listener])
    }

    listeners[eventName] = handlers
  }

  func emit(_ eventName: String, sticky: Bool = false) {
    return emit(eventName, sticky: sticky, data: Dictionary())
  }

  func emit(_ eventName: String, sticky: Bool = false, data: EventData) {
    guard Thread.isMainThread else {
      return DispatchQueue.main.async { [weak self] in self?.emit(eventName, data: data) }
    }

    let event = Event(name: eventName, data: data)
    if sticky {
      stickyEvents[event.name] = event.data
    }

    guard let eventListeners = listeners[eventName] else { return }

    var pendingRemoval = Set<EventListener>()

    eventListeners.forEach { listener in
      listener.handler(event)

      if listener.shouldRemoveImmediately {
        pendingRemoval.insert(listener)
      }
    }

    guard pendingRemoval.count > 0 else { return }
    listeners[eventName] = eventListeners.subtracting(pendingRemoval)
  }

  func removeAllListeners(_ eventName: String) {
    guard Thread.isMainThread else { return DispatchQueue.main.async { [weak self] in self?.removeAllListeners(eventName) } }
    listeners.removeValue(forKey: eventName)
  }

  func removeListener(_ handle: EventHandle) {
    guard Thread.isMainThread else { return DispatchQueue.main.async { [weak self] in self?.removeListener(handle) } }
    guard let existingHandlers = listeners[handle.eventName] else { return }

    listeners[handle.eventName] = existingHandlers.subtracting([])
  }

  deinit {
    listeners = Dictionary()
  }
}

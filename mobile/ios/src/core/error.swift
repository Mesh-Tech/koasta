import Foundation

class ApplicationError: NSObject, Error, LocalizedError {
  let message: String

  init(message: String) {
    self.message = "An internal error occurred. \(message)"
  }

  override var description: String {
    return message
  }

  var localizedDescription: String {
    return message
  }

  var errorDescription: String? {
    return message
  }
}

class UserFriendlyError: NSObject, Error, LocalizedError {
  let message: String

  init(message: String) {
    self.message = message
  }

  override var description: String {
    return message
  }

  var localizedDescription: String {
    return message
  }

  var errorDescription: String? {
    return message
  }
}

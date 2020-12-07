import Foundation

enum OrderStatus: Int {
  case unknown = 0
  case ordered = 1
  case inProgress = 2
  case ready = 3
  case complete = 4
  case rejected = 5
  case paymentPending = 6
  case paymentFailed = 7

  static func fromString(_ str: String) -> OrderStatus {
    switch str {
    case "Ordered":
      return .ordered
    case "InProgress":
      return .inProgress
    case "Ready":
      return .ready
    case "Complete":
      return .complete
    case "Rejected":
      return .rejected
    case "PaymentPending":
      return .paymentPending
    case "PaymentFailed":
      return .paymentFailed
    default:
      return .unknown
    }
  }
}

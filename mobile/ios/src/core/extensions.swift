import Foundation

extension Data {
  private static let hexAlphabet = "0123456789abcdef".unicodeScalars.map { $0 }

  public func hexEncodedString() -> String {
    return String(self.reduce(into: "".unicodeScalars, { (result, value) in
      result.append(Data.hexAlphabet[Int(value/16)])
      result.append(Data.hexAlphabet[Int(value%16)])
    }))
  }
}

extension Date {
  func timeAgoSinceDate(date: Date, numericDates:Bool) -> String {
    let calendar = Calendar.current
    let now = Date()
    let earliest = now < self ? now : self
    let latest = (earliest == now) ? self : now
    let components: DateComponents = calendar.dateComponents([Calendar.Component.minute, Calendar.Component.hour, Calendar.Component.day, Calendar.Component.weekOfYear, Calendar.Component.month, Calendar.Component.year, Calendar.Component.second], from: earliest, to: latest)

    guard let year = components.year, let month = components.month, let weekOfYear = components.weekOfYear, let day = components.day, let hour = components.hour, let minute = components.minute, let second = components.second else {
      return "Some time ago"
    }

    if year >= 2 {
      return "\(year) years ago"
    } else if year >= 1 {
      if numericDates {
        return "1 year ago"
      } else {
        return "Last year"
      }
    } else if month >= 2 {
      return "\(month) months ago"
    } else if month >= 1 {
      if numericDates {
        return "1 month ago"
      } else {
        return "Last month"
      }
    } else if weekOfYear >= 2 {
      return "\(weekOfYear) weeks ago"
    } else if weekOfYear >= 1 {
      if numericDates {
        return "1 week ago"
      } else {
        return "Last week"
      }
    } else if day >= 2 {
      return "\(day) days ago"
    } else if day >= 1 {
      if numericDates {
        return "1 day ago"
      } else {
        return "Yesterday"
      }
    } else if hour >= 2 {
      return "\(hour) hours ago"
    } else if hour >= 1 {
      if numericDates {
        return "1 hour ago"
      } else {
        return "An hour ago"
      }
    } else if minute >= 2 {
      return "\(minute) minutes ago"
    } else if minute >= 1 {
      if numericDates {
        return "1 minute ago"
      } else {
        return "A minute ago"
      }
    } else if second >= 3 {
      return "\(second) seconds ago"
    } else {
      return "Just now"
    }
  }
}

extension JSONEncoder {
  static func defaultEncoder() -> JSONEncoder {
    let d = JSONEncoder()

    d.dateEncodingStrategy = .formatted(NotShitDateFormatter())

    return d
  }
}

extension JSONDecoder {
  static func defaultDecoder() -> JSONDecoder {
    let d = JSONDecoder()

    d.dateDecodingStrategy = .formatted(NotShitDateFormatter())

    return d
  }
}

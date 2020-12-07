import Foundation

class NotShitDateFormatter: DateFormatter {
  fileprivate let formatter: ISO8601DateFormatter = {
    let df = ISO8601DateFormatter()
    df.formatOptions = [.withInternetDateTime, .withDashSeparatorInDate, .withColonSeparatorInTime, .withTimeZone, .withFractionalSeconds]
    return df
  }()

  fileprivate let formatter2: ISO8601DateFormatter = {
    let df = ISO8601DateFormatter()
    df.formatOptions = [.withInternetDateTime, .withDashSeparatorInDate, .withColonSeparatorInTime, .withTimeZone]
    return df
  }()

  override func string(from date: Date) -> String {
    return formatter.string(from: date)
  }

  override func string(for obj: Any?) -> String? {
    return formatter.string(for: obj) ?? formatter2.string(for: obj)
  }

  override func date(from string: String) -> Date? {
    return formatter.date(from: string) ?? formatter2.date(from: string)
  }

  override class func localizedString(from date: Date, dateStyle dstyle: DateFormatter.Style, timeStyle tstyle: DateFormatter.Style) -> String {
    let f = NotShitDateFormatter()
    return f.string(from: date)
  }
}

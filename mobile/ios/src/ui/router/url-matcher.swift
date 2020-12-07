import Foundation

struct URLMatchComponents {
  let pattern: String
  let values: [String: Any]
}

class URLMatcher {
  typealias URLValueMatcherHandler = (String) -> Any?

  private var customURLValueMatcherHandlers = [String: URLValueMatcherHandler]()

  static let `default` = URLMatcher()

  init() {}

  func match(_ url: URLConvertible, scheme: String? = nil, from urlPatterns: [String]) -> URLMatchComponents? {
    let normalizedURLString = self.normalized(url, scheme: scheme).urlStringValue
    let urlPathComponents = normalizedURLString.components(separatedBy: "/")

    outer: for urlPattern in urlPatterns {
      let urlPatternPathComponents = urlPattern.components(separatedBy: "/")
      let containsPathPlaceholder = urlPatternPathComponents.contains { $0.hasPrefix("<path:") }
      guard containsPathPlaceholder || urlPatternPathComponents.count == urlPathComponents.count else {
        continue
      }

      var values = [String: Any]()

      for (i, component) in urlPatternPathComponents.enumerated() {
        guard i < urlPathComponents.count else {
          continue outer
        }
        let info = self.placeholderKeyValueFrom(urlPatternPathComponent: component,
                                                urlPathComponents: urlPathComponents,
                                                atIndex: i)
        if let (key, value) = info {
          values[key] = value
          if component.hasPrefix("<path:") {
            break
          }
        } else if component != urlPathComponents[i] {
          continue outer
        }
      }

      return URLMatchComponents(pattern: urlPattern, values: values)
    }
    return nil
  }

  func addURLValueMatcherHandler(for valueType: String, handler: @escaping URLValueMatcherHandler) {
    self.customURLValueMatcherHandlers[valueType] = handler
  }

  func url(withScheme scheme: String?, _ url: URLConvertible) -> URLConvertible {
    let urlString = url.urlStringValue
    if let scheme = scheme, !urlString.contains("://") {
      #if DEBUG
        if !urlString.hasPrefix("/") {
          NSLog("[Warning] URL pattern doesn't have leading slash(/): '\(url)'")
        }
      #endif
      return scheme + ":/" + urlString
    } else if scheme == nil && !urlString.contains("://") {
      assertionFailure("Either matcher or URL should have scheme: '\(url)'") // assert only in debug build
    }
    return urlString
  }

  func normalized(_ dirtyURL: URLConvertible, scheme: String? = nil) -> URLConvertible {
    guard dirtyURL.urlValue != nil else {
      return dirtyURL
    }
    var urlString = self.url(withScheme: scheme, dirtyURL).urlStringValue
    urlString = urlString.components(separatedBy: "?")[0].components(separatedBy: "#")[0]
    urlString = self.replaceRegex(":/{3,}", "://", urlString)
    urlString = self.replaceRegex("(?<!:)/{2,}", "/", urlString)
    urlString = self.replaceRegex("(?<!:|:/)/+$", "", urlString)
    return urlString
  }

  func placeholderKeyValueFrom(
    urlPatternPathComponent component: String,
    urlPathComponents: [String],
    atIndex index: Int
  ) -> (String, Any)? {
    guard component.hasPrefix("<") && component.hasSuffix(">") else { return nil }

    let start = component.index(after: component.startIndex)
    let end = component.index(before: component.endIndex)
    let placeholder = component[start..<end] // e.g. "<int:id>" -> "int:id"

    let typeAndKey = placeholder.components(separatedBy: ":") // e.g. ["int", "id"]
    if typeAndKey.count == 0 { // e.g. component is "<>"
      return nil
    }
    if typeAndKey.count == 1 { // untyped placeholder
      return (String(placeholder), urlPathComponents[index])
    }

    let (type, key) = (typeAndKey[0], typeAndKey[1]) // e.g. ("int", "id")
    let value: Any?
    switch type {
    case "UUID": value = UUID(uuidString: urlPathComponents[index]) // e.g. 123e4567-e89b-12d3-a456-426655440000
    case "string": value = String(urlPathComponents[index]) // e.g. "123"
    case "int": value = Int(urlPathComponents[index]) // e.g. 123
    case "float": value = Float(urlPathComponents[index]) // e.g. 123.0
    case "path": value = urlPathComponents[index..<urlPathComponents.count].joined(separator: "/")
    default:
      if let customURLValueTypeHandler = customURLValueMatcherHandlers[type] {
        value = customURLValueTypeHandler(urlPathComponents[index])
      } else {
        value = urlPathComponents[index]
      }
    }

    if let value = value {
      return (key, value)
    }
    return nil
  }

  func replaceRegex(_ pattern: String, _ repl: String, _ string: String) -> String {
    guard let regex = try? NSRegularExpression(pattern: pattern, options: []) else { return string }
    let range = NSRange(location: 0, length: string.count)
    return regex.stringByReplacingMatches(in: string, options: [], range: range, withTemplate: repl)
  }
}

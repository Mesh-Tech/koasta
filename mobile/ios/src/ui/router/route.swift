import Foundation

typealias MappingContext = Any
typealias NavigationContext = Any

struct Route {
  let url: URLConvertible
  let values: [String: Any]
  let mappingContext: MappingContext?
  let navigationContext: NavigationContext?
}

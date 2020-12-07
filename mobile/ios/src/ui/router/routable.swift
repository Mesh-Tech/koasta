import Foundation

protocol Routable: class {
  var routerContext: Route? {get set}
}

extension Routable {
  var activeSession: Session? {
    guard let routerContext = routerContext else {
      return nil
    }

    guard let navContext = routerContext.navigationContext as? [String:Any] else {
      return nil
    }

    return navContext["session"] as? Session
  }
}

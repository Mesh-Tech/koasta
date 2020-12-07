import UIKit

public typealias _URLConvertible = URLConvertible

class Router {
  struct URLMapItem {
    let routableName: String
    let mappingContext: MappingContext?
    let cached: Bool
  }

  public typealias URLConvertible = _URLConvertible
  public typealias URLOpenHandler = (_ url: URLConvertible, _ values: [String: Any]) -> Bool

  fileprivate(set) var urlMap = [String: URLMapItem]()
  fileprivate(set) var urlOpenHandlers = [String: URLOpenHandler]()
  fileprivate var viewCache = [String: UIViewController]()

  var defaultScheme: String? {
    didSet {
      if let scheme = self.defaultScheme, scheme.contains("://") {
        self.defaultScheme = scheme.components(separatedBy: "://")[0]
      }
    }
  }

  func map(_ urlPattern: URLConvertible, _ routableName: String, context: MappingContext? = nil, cached: Bool = false) {
    let URLString = URLMatcher.default.normalized(urlPattern, scheme: self.defaultScheme).urlStringValue
    self.urlMap[URLString] = URLMapItem(routableName: routableName, mappingContext: context, cached: cached)
  }

  func map(_ urlPattern: URLConvertible, _ handler: @escaping URLOpenHandler) {
    let URLString = URLMatcher.default.normalized(urlPattern, scheme: self.defaultScheme).urlStringValue
    self.urlOpenHandlers[URLString] = handler
  }

  func viewController(for url: URLConvertible, context: NavigationContext? = nil) -> UIViewController? {
    if let urlMatchComponents = URLMatcher.default.match(url, scheme: self.defaultScheme, from: Array(self.urlMap.keys)) {
      guard let item = self.urlMap[urlMatchComponents.pattern] else { return nil }
      let route = Route(
        url: url,
        values: urlMatchComponents.values,
        mappingContext: item.mappingContext,
        navigationContext: context
      )

      if item.cached && viewCache.keys.contains(item.routableName) {
        let vc = viewCache[item.routableName]
        let a = vc as! Routable
        a.routerContext = route
        return vc
      }

      let vc = KoastaContainer.viewController(item.routableName)
      guard let a = vc as? Routable else {
        return nil
      }

      a.routerContext = route

      guard item.cached else { return vc }

      viewCache[item.routableName] = vc
      return vc
    }
    return nil
  }

  @discardableResult
  func push(
    _ url: URLConvertible,
    context: NavigationContext? = nil,
    from: UINavigationController? = nil,
    animated: Bool = true
  ) -> UIViewController? {
    guard let viewController = self.viewController(for: url, context: context) else {
      return nil
    }
    return self.push(viewController, from: from, animated: animated)
  }

  @discardableResult
  fileprivate func push(
    _ viewController: UIViewController,
    from: UINavigationController? = nil,
    animated: Bool = true
    ) -> UIViewController? {
    guard let navigationController = from ?? UIViewController.topMost?.navigationController else {
      return nil
    }
    guard (viewController is UINavigationController) == false else { return nil }
    navigationController.pushViewController(viewController, animated: animated)
    return viewController
  }

  @discardableResult
  func replace(
    _ url: URLConvertible,
    context: NavigationContext? = nil,
    from: UINavigationController? = nil,
    animated: Bool = true
    ) -> UIViewController? {
    guard let viewController = self.viewController(for: url, context: context) else {
      return nil
    }
    return self.replace(viewController, from: from, animated: animated)
  }

  @discardableResult
  fileprivate func replace(
    _ viewController: UIViewController,
    from: UINavigationController? = nil,
    animated: Bool = true
    ) -> UIViewController? {
    guard let navigationController = from ?? UIViewController.topMost?.navigationController else {
      return nil
    }
    guard (viewController is UINavigationController) == false else { return nil }

    navigationController.setViewControllers([viewController], animated: animated)
    return viewController
  }

  @discardableResult
  func replaceAllButOne(
    _ url: URLConvertible,
    context: NavigationContext? = nil,
    from: UINavigationController? = nil,
    animated: Bool = true
    ) -> UIViewController? {
    guard let viewController = self.viewController(for: url, context: context) else {
      return nil
    }
    return self.replaceAllButOne(viewController, from: from, animated: animated)
  }

  @discardableResult
  fileprivate func replaceAllButOne(
    _ viewController: UIViewController,
    from: UINavigationController? = nil,
    animated: Bool = true
    ) -> UIViewController? {
    guard let navigationController = from ?? UIViewController.topMost?.navigationController else {
      return nil
    }
    guard (viewController is UINavigationController) == false else { return nil }
    var vcs = navigationController.viewControllers
    let remainingVc = vcs.first
    vcs.removeAll()

    if let vc = remainingVc {
      vcs.append(vc)
    }

    vcs.append(viewController)

    navigationController.setViewControllers(vcs, animated: animated)
    return viewController
  }

  @discardableResult
  func present(
    _ url: URLConvertible,
    context: NavigationContext? = nil,
    wrap: Bool = false,
    from: UIViewController? = nil,
    animated: Bool = true,
    completion: (() -> Void)? = nil
  ) -> UIViewController? {
    guard let viewController = self.viewController(for: url, context: context) else { return nil }
//    viewController.modalPresentationCapturesStatusBarAppearance = true
    return self.present(viewController, wrap: wrap, from: from, animated: animated, completion: completion)
  }

  @discardableResult
  fileprivate func present(
    _ viewController: UIViewController,
    wrap: Bool = false,
    from: UIViewController? = nil,
    animated: Bool = true,
    completion: (() -> Void)? = nil
  ) -> UIViewController? {
    guard let from2 = from ?? UIViewController.topMost else { return nil }
    let fromViewController = from2.navigationController ?? from2

    let wrap = wrap && (viewController is UINavigationController) == false
    if wrap {
      let navigationController = UINavigationController(rootViewController: viewController)
      navigationController.applyDefaultStyling()
//      navigationController.modalPresentationCapturesStatusBarAppearance = true
      if let _ = viewController as? CurrentContextViewController {
        navigationController.modalPresentationStyle = .overCurrentContext
        fromViewController.definesPresentationContext = true
      }
//      viewController.modalPresentationCapturesStatusBarAppearance = true
      navigationController.presentationController?.delegate = from
      fromViewController.present(navigationController, animated: animated, completion: nil)
    } else {
      if let _ = viewController as? CurrentContextViewController {
        viewController.modalPresentationStyle = .overCurrentContext
        fromViewController.definesPresentationContext = true
      }
//      viewController.modalPresentationCapturesStatusBarAppearance = true
      viewController.presentationController?.delegate = from
      fromViewController.present(viewController, animated: animated, completion: nil)
    }
    return viewController
  }

  @discardableResult
  func open(_ url: URLConvertible) -> Bool {
    let urlOpenHandlersKeys = Array(self.urlOpenHandlers.keys)
    if let urlMatchComponents = URLMatcher.default.match(url, scheme: self.defaultScheme, from: urlOpenHandlersKeys) {
      let handler = self.urlOpenHandlers[urlMatchComponents.pattern]
      if handler?(url, urlMatchComponents.values) == true {
        return true
      }
    }
    return false
  }

  func purgeAll() {
    viewCache = Dictionary()
  }

  @discardableResult
  func purge(_ url: URLConvertible) -> Bool {
    if let urlMatchComponents = URLMatcher.default.match(url, scheme: self.defaultScheme, from: Array(self.urlMap.keys)) {
      guard let item = self.urlMap[urlMatchComponents.pattern] else { return false }
      if viewCache.keys.contains(item.routableName) {
        viewCache.removeValue(forKey: item.routableName)
        return true
      }
    }

    return false
  }
}

import Foundation
import Hydra

class WebMenuListApi: MenuListApi {
  fileprivate let request: Request!
  fileprivate let config: Config!

  init(request: Request?,
       config: Config?) {
    guard let request = request,
          let config = config else {
      fatalError()
    }

    self.request = request
    self.config = config
  }

  func getMenuList(forId id: String) -> Promise<[Menu]?> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("menu").appendingPathComponent("menu").appendingPathComponent(id)
      do {
        let menus: [Menu]? = try await(self.request.get(url: url))
        return menus
      } catch {
        if let err = error as? ApiError, err.statusCode == 404 {
          return []
        }

        throw error
      }
    })
  }
}

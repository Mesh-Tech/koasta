import Foundation
import Hydra

class AuthManager: EventEmitter {
  fileprivate let queue = DispatchQueue(label: "io.meshtech.pubcrawl.request.authqueue", qos: .default, attributes: [.concurrent], autoreleaseFrequency: .inherit, target: nil)
  fileprivate let session: URLSession
  fileprivate let sessionManager: SessionManager
  fileprivate let config: Config
  fileprivate let globalEmitter: EventEmitter
  fileprivate var isReauthenticating = false
  fileprivate var refreshTimer: Timer?
  fileprivate let encoder = JSONEncoder.defaultEncoder()
  fileprivate let decoder = JSONDecoder.defaultDecoder()

  init (sessionManager: SessionManager?, config: Config?, globalEmitter: EventEmitter?) {
    guard let sessionManager = sessionManager, let config = config, let globalEmitter = globalEmitter else { fatalError() }
    self.sessionManager = sessionManager
    self.config = config
    self.globalEmitter = globalEmitter

    let configuration = URLSessionConfiguration.ephemeral
    configuration.requestCachePolicy = .reloadIgnoringLocalAndRemoteCacheData
    configuration.timeoutIntervalForResource = 30
    configuration.timeoutIntervalForRequest = 30

    session = URLSession(configuration: configuration)

    super.init()
    DispatchQueue.main.asyncAfter(deadline: .now() + 10) { [weak self] in
      self?.refreshTimer?.fire()
    }
  }
}

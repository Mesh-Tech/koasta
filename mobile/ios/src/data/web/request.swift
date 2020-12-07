import Foundation
import Hydra

extension URLSession {
  struct Response {
    let data: Data?
    let response: HTTPURLResponse
  }

  func dataTaskAsync(with request: URLRequest, queue: DispatchQueue? = nil) -> Promise<Response> {
    return Promise<Response>({ resolve, reject, _  in
      self.dataTask(with: request, completionHandler: { (data, response, error) in
        (queue ?? DispatchQueue.global()).async {
          if let error = error { return reject(error) }
          guard let response = response as? HTTPURLResponse else { return reject(ApplicationError(message: "Invalid TCP response")) }

          resolve(Response(data: data, response: response))
        }
      }).resume()
    })
  }

  func dataTaskAsync(with url: URL, queue: DispatchQueue? = nil) -> Promise<Response> {
    return Promise<Response>({ resolve, reject, _  in
      self.dataTask(with: url, completionHandler: { (data, response, error) in
        (queue ?? DispatchQueue.global()).async {
          if let error = error { return reject(error) }
          guard let response = response as? HTTPURLResponse else { return reject(ApplicationError(message: "Invalid TCP response")) }

          resolve(Response(data: data, response: response))
        }
      }).resume()
    })
  }
}

struct ApiError: Error {
  let statusCode: Int
  let body: String?
  let bodyData: Data?
}

class Request {
  fileprivate let session: URLSession
  fileprivate let authManager: AuthManager
  fileprivate let registry: EventListenerRegistry
  fileprivate let sessionManager: SessionManager
  fileprivate let retryQueue = DispatchQueue(label: "io.meshtech.pubcrawl.request.retryqueue", qos: .default, attributes: [.concurrent], autoreleaseFrequency: .inherit, target: nil)
  fileprivate var shouldResume = false
  fileprivate var reauthSucceeded = false

  init (authManager: AuthManager?, registry: EventListenerRegistry?, sessionManager: SessionManager?) {
    guard let authManager = authManager, let registry = registry, let sessionManager = sessionManager else { fatalError() }
    self.authManager = authManager
    self.registry = registry
    self.sessionManager = sessionManager
    let configuration = URLSessionConfiguration.ephemeral
    configuration.requestCachePolicy = .reloadIgnoringLocalAndRemoteCacheData
    configuration.timeoutIntervalForResource = 30
    configuration.timeoutIntervalForRequest = 30

    session = URLSession(configuration: configuration)

    registry <~ self.authManager.on("auth-failed") { [weak self] _ in
      self?.reauthSucceeded = false
      if self?.shouldResume == true {
        self?.retryQueue.resume()
        self?.shouldResume = false
      }
    }

    registry <~ self.authManager.on("auth-succeeded") { [weak self] _ in
      self?.reauthSucceeded = true
      if self?.shouldResume == true {
        self?.retryQueue.resume()
        self?.shouldResume = false
      }
    }
  }

  fileprivate func get<T: Decodable>(url: URL, headers: [String:String]? = nil, queue: DispatchQueue?, timeout: Int?) -> Promise<T?> {
    let queue = queue ?? DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: TimeInterval(timeout ?? 18))
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }

      guard let data = response.data else {
        return nil
      }

      do {
        let decoder = JSONDecoder.defaultDecoder()
        return try decoder.decode(T.self, from: data)
      } catch let error {
        print(error)
        return nil
      }
    })
  }

  func get<T: Decodable>(url: URL, headers: [String:String]? = nil) -> Promise<T?> {
    return get(url: url, headers: headers, queue: nil, timeout: nil)
  }

  func get<T: Decodable>(url: URL, headers: [String:String]? = nil, timeout: Int? = nil) -> Promise<T?> {
    return get(url: url, headers: headers, queue: nil, timeout: timeout)
  }

  fileprivate func post<T: Decodable, RT: Codable>(url: URL, body: RT?, headers: [String:String]? = nil, queue: DispatchQueue?) -> Promise<T?> {
    let queue = queue ?? DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 18)
      request.httpMethod = "POST"
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let body = body {
        request.httpBody = try JSONEncoder().encode(body)
      }
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }

      guard let data = response.data else {
        return nil
      }

      do {
        let decoder = JSONDecoder.defaultDecoder()
        return try decoder.decode(T.self, from: data)
      } catch let error {
        print(error)
        return nil
      }
    })
  }

  fileprivate func post<T: Decodable>(url: URL, headers: [String:String]? = nil, queue: DispatchQueue?) -> Promise<T?> {
    let queue = queue ?? DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 18)
      request.httpMethod = "POST"
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }

      guard let data = response.data else {
        return nil
      }

      do {
        let decoder = JSONDecoder.defaultDecoder()
        return try decoder.decode(T.self, from: data)
      } catch let error {
        print(error)
        return nil
      }
    })
  }

  func post<T: Decodable, RT: Codable>(url: URL, body: RT?, headers: [String:String]? = nil) -> Promise<T?> {
    return post(url: url, body: body, headers: headers, queue: nil)
  }

  func post<T: Decodable>(url: URL, headers: [String:String]? = nil) -> Promise<T?> {
    return post(url: url, headers: headers, queue: nil)
  }

  func post(url: URL, headers: [String:String]? = nil) -> Promise<Void> {
    let queue = DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 18)
      request.httpMethod = "POST"
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }
      request.httpBody = "{}".data(using: .utf8)

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }
    })
  }

  func post<RT: Codable>(url: URL, body: RT?, headers: [String:String]? = nil) -> Promise<Void> {
    let queue = DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 18)
      request.httpMethod = "POST"
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }
      if let body = body {
        request.httpBody = try JSONEncoder().encode(body)
      }

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }
    })
  }

  fileprivate func postExecute<RT: Codable>(url: URL, body: RT?, headers: [String:String]? = nil, queue: DispatchQueue?) -> Promise<Void> {
    let queue = queue ?? DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 18)
      request.httpMethod = "POST"
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let body = body {
        request.httpBody = try JSONEncoder().encode(body)
      }
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }
    })
  }

  fileprivate func postExecute(url: URL, headers: [String:String]? = nil, queue: DispatchQueue?) -> Promise<Void> {
    let queue = queue ?? DispatchQueue.global()
    return async(in: .custom(queue: queue), { _ in
      var request = URLRequest(url: url, cachePolicy: .reloadIgnoringLocalAndRemoteCacheData, timeoutInterval: 18)
      request.httpMethod = "POST"
      request.allHTTPHeaderFields = headers ?? [:]
      request.setValue("application/json", forHTTPHeaderField: "Content-Type")
      request.setValue("application/json", forHTTPHeaderField: "Accept")
      request.setValue("pubcrawl/ios", forHTTPHeaderField: "User-Agent")
      if let authToken = self.sessionManager.authToken {
        request.setValue("Bearer \(authToken)", forHTTPHeaderField: "Authorization")
      }
      if let authType = self.sessionManager.authType {
        request.setValue(authType, forHTTPHeaderField: "x-koasta-authtype")
      }

      let response = try await(self.session.dataTaskAsync(with: request))
      guard response.response.statusCode < 300 else {
        if let data = response.data {
          throw ApiError(statusCode: response.response.statusCode, body: String(data: data, encoding: .utf8), bodyData: data)
        } else {
          throw ApiError(statusCode: response.response.statusCode, body: nil, bodyData: nil)
        }
      }
    })
  }

  func postExecute<RT: Codable>(url: URL, body: RT?, headers: [String:String]? = nil) -> Promise<Void> {
    return postExecute(url: url, body: body, headers: headers, queue: nil)
  }

  func postExecute(url: URL, headers: [String:String]? = nil) -> Promise<Void> {
    return postExecute(url: url, headers: headers, queue: nil)
  }

  fileprivate func handleUnauthorized () -> Promise<Void> {
    return Promise(rejected: ApiError(statusCode: 401, body: nil, bodyData: nil))
  }
}

import Foundation
import Hydra

class WebOrderApi: OrderApi {
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

  func sendOrder(_ order: Order) -> Promise<SendOrderResult> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("order").appendingPathComponent("order")

      guard let confirmation: SendOrderResult = try await(self.request.post(url: url, body: order)) else {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return confirmation
    })
  }

  func requestOrderEstimate(_ order: DraftOrder) -> Promise<EstimateOrderResult> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("order").appendingPathComponent("order").appendingPathComponent("estimate")
      print(url)

      guard let confirmation: EstimateOrderResult = try await(self.request.post(url: url, body: order)) else {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return confirmation
    })
  }

  func getOrders() -> Promise<[HistoricalOrder]> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("order").appendingPathComponent("order")

      guard let orders: [HistoricalOrder] = try await(self.request.get(url: url)) else {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return orders
    })
  }

  func getOrder(orderId: Int) -> Promise<HistoricalOrder> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("order").appendingPathComponent("order").appendingPathComponent(String(orderId))

      guard let order: HistoricalOrder = try await(self.request.get(url: url)) else {
        throw ApiError(statusCode: 500, body: nil, bodyData: nil)
      }

      return order
    })
  }
}

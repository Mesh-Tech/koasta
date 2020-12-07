import Foundation

struct HistoricalOrderItem: Codable {
  let amount: Decimal
  let productName: String
  let quantity: Int
}

struct HistoricalOrder: Codable {
  let companyId: Int
  let externalPaymentId: String?
  let firstName: String?
  let lastName: String?
  let orderId: Int
  let orderNumber: Int
  let orderStatus: Int
  let orderTimeStamp: Date
  let userId: Int?
  let venueName: String
  let lineItems: [HistoricalOrderItem]
  let total: Decimal
  let serviceCharge: Decimal
  let orderNotes: String?
  let servingType: VenueServingType
  let table: String?
}

extension HistoricalOrder {
  func addingLineItem(_ item: HistoricalOrderItem) -> HistoricalOrder {
    var items = lineItems
    items.append(item)
    return HistoricalOrder(companyId: companyId, externalPaymentId: externalPaymentId, firstName: firstName, lastName: lastName, orderId: orderId, orderNumber: orderNumber, orderStatus: orderStatus, orderTimeStamp: orderTimeStamp, userId: userId, venueName: venueName, lineItems: items, total: total, serviceCharge: serviceCharge, orderNotes: orderNotes, servingType: servingType, table: table)
  }
}

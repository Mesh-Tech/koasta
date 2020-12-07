import Foundation

struct StripeEphemeralKeyAssociatedObject: Decodable {
  let type: String
  let id: String?

  func toDict() -> [AnyHashable : Any] {
    var data: [AnyHashable : Any] = [
      "type": type
    ]

    if let id = id {
      data["id"] = id
    }

    return data
  }
}

struct StripeEphemeralKey: Decodable {
  let id: String
  let object: String
  let associated_objects: [StripeEphemeralKeyAssociatedObject]?
  let created: Int
  let expires: Int
  let livemode: Bool
  let secret: String

  func toDict() -> [AnyHashable : Any] {
    return [
      "id": id,
      "object": object,
      "associated_objects": (associated_objects ?? []).map({ $0.toDict() }),
      "created": created,
      "expires": expires,
      "livemode": livemode,
      "secret": secret
    ]
  }
}

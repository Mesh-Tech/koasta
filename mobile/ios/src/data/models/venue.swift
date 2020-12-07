import Foundation

struct VenuePlaceholder: Codable {
  let backgroundIndex: Int
  let imageAIndex: Int
  let imageBIndex: Int
  let imageCIndex: Int
  let imageDIndex: Int
}

enum VenueServingType: Int, Codable {
  case barService = 0
  case tableService = 1
  case barAndTableService = 2
}

struct Venue: Codable {
  let venueId: Int
  let venueCode: String
  let companyId: Int
  let venueName: String
  let venueAddress: String
  let venuePostCode: String
  let venuePhoneNumber: String?
  let venueContact: String?
  let venueDescription: String?
  let venueNotes: String?
  let imageUrl: String?
  let tags: [String]?
  let venueLatitude: String
  let venueLongitude: String
  let externalLocationId: String?
  let progress: Int
  let isOpen: Bool
  let servingType: VenueServingType
  var placeholder: VenuePlaceholder?

  func withPlaceholder(_ placeholder: VenuePlaceholder) -> Venue {
    var ret = self
    ret.placeholder = placeholder
    return ret
  }
}

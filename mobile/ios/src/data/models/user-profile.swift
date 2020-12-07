import Foundation

struct UserProfile: Codable {
  let registrationId: String
  let wantAdvertising: Bool
  let votedVenueIds: [Int]
  let firstName: String?
  let lastName: String?

  func withVotedVenueId(_ venueId: Int) -> UserProfile {
    var ids = votedVenueIds
    ids.append(venueId)
    return UserProfile(registrationId: registrationId, wantAdvertising: wantAdvertising, votedVenueIds: ids, firstName: firstName, lastName: lastName)
  }
}

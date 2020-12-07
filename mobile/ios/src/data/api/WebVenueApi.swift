import Foundation
import Hydra

class WebVenueApi: VenueApi {
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

  func getVenue(forId id: String) -> Promise<Venue?> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("venue").appendingPathComponent("venue").appendingPathComponent(id)
      let ret: Venue? = try await(self.request.get(url: url))

      guard let r = ret else {
        return ret
      }

      let backgroundIndex = Placeholders.backgroundColours.pickInidices(1)
      let imageIndices = Placeholders.placeholderImages.pickInidices(4)
      return r.withPlaceholder(VenuePlaceholder(backgroundIndex: backgroundIndex[0], imageAIndex: imageIndices[0], imageBIndex: imageIndices[1], imageCIndex: imageIndices[2], imageDIndex: imageIndices[3]))
    })
  }

  func getNearbyVenues(lat: Double, lon: Double, limit: Int) -> Promise<[Venue]?> {
    return async(in: .background, { _ in
      var components = URLComponents(url: self.config.apiUrl("venue"), resolvingAgainstBaseURL: false)!
      components.path = "/venue/nearby-venues/\(String(lat))/\(String(lon))"
      components.queryItems = [
        URLQueryItem(name: "count", value: String(limit))
      ]

      let url = components.url!

      let ret: [Venue]? = try await(self.request.get(url: url))

      guard let r = ret else {
        return ret
      }

      return r.map { venue in
        let backgroundIndex = Placeholders.backgroundColours.pickInidices(1)
        let imageIndices = Placeholders.placeholderImages.pickInidices(4)
        return venue.withPlaceholder(VenuePlaceholder(backgroundIndex: backgroundIndex[0], imageAIndex: imageIndices[0], imageBIndex: imageIndices[1], imageCIndex: imageIndices[2], imageDIndex: imageIndices[3]))
      }
    })
  }

  func getVenues(query: String) -> Promise<[Venue]?> {
    return async(in: .background, { _ in
      var components = URLComponents(url: self.config.apiUrl("venue"), resolvingAgainstBaseURL: false)!
      let queryEncoded = query.addingPercentEncoding(withAllowedCharacters: CharacterSet.urlPathAllowed) ?? ""

      components.path = "/venue/named/\(queryEncoded)"
      components.queryItems = [
        URLQueryItem(name: "count", value: String(10))
      ]

      let url = components.url!

      let ret: [Venue]? = try await(self.request.get(url: url))

      guard let r = ret else {
        return ret
      }

      return r.map { venue in
        let backgroundIndex = Placeholders.backgroundColours.pickInidices(1)
        let imageIndices = Placeholders.placeholderImages.pickInidices(4)
        return venue.withPlaceholder(VenuePlaceholder(backgroundIndex: backgroundIndex[0], imageAIndex: imageIndices[0], imageBIndex: imageIndices[1], imageCIndex: imageIndices[2], imageDIndex: imageIndices[3]))
      }
    })
  }

  func submitReview(forId id: String, review: VenueReview) -> Promise<Void> {
    return async(in: .background, { _ in
      let url = self.config.apiUrl("venue").appendingPathComponent("venue").appendingPathComponent(id).appendingPathComponent("review")
      return try await(self.request.postExecute(url: url, body: review))
    })
  }

}

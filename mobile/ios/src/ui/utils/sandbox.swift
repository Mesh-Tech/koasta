import Foundation

protocol SandboxManager {}

private struct SessionV1: Codable {
  var token: String?
  var refreshToken: String?
  var lastVenue: String?
  var cachedVenueLocations: [String:String]?
}

private struct SessionV2: Codable {
  var pushToken: String?
  var hasSkippedOnboarding: Bool?
}

private struct SessionV3: Codable {
  var phoneNumber: String?
}

private struct SessionV4: Codable {
  var authTokenExpiry: Date?
  var refreshTokenExpiry: Date?
}

private struct SessionV5: Codable {
  var source: SessionSource?
}

private struct SessionImpl: Session {
  var version: Semver
  var v1: SessionV1?
  var v2: SessionV2?
  var v3: SessionV3?
  var v4: SessionV4?
  var v5: SessionV5?

  var lastVenue: String? {
    get { return v1?.lastVenue }
    set { v1?.lastVenue = newValue }
  }
  var cachedVenueLocations: [String : String]? {
    get { return v1?.cachedVenueLocations }
    set { v1?.cachedVenueLocations = newValue }
  }
  var authenticationToken: String? {
    get { return v1?.token }
    set { v1?.token = newValue }
  }
  var refreshToken: String? {
    get { return v1?.refreshToken }
    set { v1?.refreshToken = newValue }
  }
  var pushToken: String? {
    get { return v2?.pushToken }
    set { v2?.pushToken = newValue }
  }
  var hasSkippedOnboarding: Bool? {
    get { return v2?.hasSkippedOnboarding }
    set { v2?.hasSkippedOnboarding = newValue }
  }
  var phoneNumber: String? {
    get { return v3?.phoneNumber }
    set { v3?.phoneNumber = newValue }
  }
  var authTokenExpiry: Date? {
    get { return v4?.authTokenExpiry }
    set { v4?.authTokenExpiry = newValue }
  }
  var refreshTokenExpiry: Date? {
    get { return v4?.refreshTokenExpiry }
    set { v4?.refreshTokenExpiry = newValue }
  }
  var source: SessionSource? {
    get { return v5?.source }
    set { v5?.source = newValue }
  }
}

extension SandboxManager {
  func purgeSandbox () {
    do {
      var paths = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true)
      paths.append(contentsOf: NSSearchPathForDirectoriesInDomains(.cachesDirectory, .userDomainMask, true))

      var fileCountRemoved: Double = 0
      var fileCount = 0

      try paths.forEach { documentsDirectory in
        let documentsDirectoryURL = NSURL(fileURLWithPath: paths[0])

        let directoryContents = try FileManager.default.contentsOfDirectory(atPath: documentsDirectory)

        try? directoryContents.forEach { filename in
          guard let file = documentsDirectoryURL.appendingPathComponent(filename) else { return }

          let fileAttributes = try FileManager.default.attributesOfItem(atPath: file.path)
          let fileSize = fileAttributes[FileAttributeKey.size] as! Double
          fileCountRemoved += fileSize/1024/1024

          do {
            try FileManager.default.removeItem(at: file)
            fileCount += 1
          } catch let error as NSError {
            print("> Emptying sandbox: could not delete file", filename, error)
          }
        }
      }

      print("> Emptying sandbox: Removed \(fileCount) files with a total of \(round(fileCountRemoved))MB")
    } catch let error as NSError {
      print("> Emptying sandbox: Error emptying sandbox", error)
    }
  }

  func injectSandboxSession () {
    do {
      let session = SessionImpl(version: Semver(version: "0.0.5"), v1: SessionV1(token: "a", refreshToken: "b", lastVenue: nil, cachedVenueLocations: nil), v2: SessionV2(pushToken: "a", hasSkippedOnboarding: false), v3: SessionV3(phoneNumber: "c"), v4: SessionV4(authTokenExpiry: Date(timeIntervalSince1970: 999999999999), refreshTokenExpiry: Date(timeIntervalSince1970: 999999999999)), v5: SessionV5(source: SessionSource.facebook))
      var paths = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true)
      paths.append(contentsOf: NSSearchPathForDirectoriesInDomains(.cachesDirectory, .userDomainMask, true))

      let encoder = JSONEncoder.defaultEncoder()
      let documentsUrl = try FileManager.default.url(for: .documentDirectory, in: .userDomainMask, appropriateFor: nil, create: true)
      let sessionUrl = documentsUrl.appendingPathComponent("session.json")

      try encoder.encode(session).write(to: sessionUrl)
    } catch let error as NSError {
      print("> Emptying sandbox: Error injecting sandbox", error)
    }
  }
}

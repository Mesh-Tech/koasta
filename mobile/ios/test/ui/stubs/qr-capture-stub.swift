import Foundation
import UIKit

@testable import pubcrawl

class StubQRCaptureUtil: QRCaptureUtil {
  struct ScheduledCapture {
    var qrCode: String
    var sleepMs: TimeInterval
  }

  weak var delegate: QRCaptureUtilDelegate?

  var startCapturingShouldSucceed = true
  var scheduledCaptures: [ScheduledCapture] = Array()
  fileprivate var isCancelled = false

  func startCapturing(in hostView: UIView) -> Bool {
    for cap in scheduledCaptures {
      guard !isCancelled else { return startCapturingShouldSucceed }
      DispatchQueue.main.asyncAfter(deadline: .now() + cap.sleepMs) {
        guard !self.isCancelled else { return }
        self.delegate?.qrUtil?(self, didCapture: cap.qrCode)
      }
    }

    return startCapturingShouldSucceed
  }

  func stopCapturing(in hostView: UIView?) {
    isCancelled = true
  }
}

import Foundation
import UIKit

@testable import pubcrawl

class StubPermissionsUtil: PermissionsUtil {
  var initialPermissionState: PermissionState = .notRequested
  var permissionStateAfterRequest: PermissionState = .allowed

  override var cameraPermission: PermissionsUtil.PermissionState {
    return initialPermissionState
  }

  override func requestCameraPermission(callback: @escaping (PermissionsUtil.PermissionState) -> Void) {
    callback(permissionStateAfterRequest)
  }
}

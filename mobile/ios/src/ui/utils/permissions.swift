import Foundation
import AVFoundation
import CoreLocation
import UserNotifications
import UIKit

class PermissionsUtil {
  enum PermissionState {
    case allowed
    case notRequested
    case denied
  }

  fileprivate var locationManagerDelegate: AsyncLocationManagerDelegate?

  init(locationProvider: LocationManagerProvider) {
    locationManagerDelegate = locationProvider.getDelegate()
  }

  var cameraPermission: PermissionState {
    get {
      let authStatus = AVCaptureDevice.authorizationStatus(for: AVMediaType.video)
      switch authStatus {
      case .authorized: return .allowed
      case .denied: return .notRequested
      case .notDetermined: return .denied
      default: return .notRequested
      }
    }
  }

  var locationPermission: PermissionState {
    get {
      let enabled = CLLocationManager.locationServicesEnabled()
      let status = CLLocationManager.authorizationStatus()

      if !enabled {
        return .denied
      }

      switch status {
      case .denied: return .denied
      case .authorizedAlways: return .allowed
      case .authorizedWhenInUse: return .allowed
      case .restricted: return .denied
      case .notDetermined: return .notRequested
      @unknown default:
        return .denied
      }
    }
  }

  var notificationPermission: PermissionState {
    get {
      let current = UNUserNotificationCenter.current()
      var permissionState: PermissionState!
      let semaphore = DispatchSemaphore(value: 0)
      current.getNotificationSettings(completionHandler: { settings in
        switch settings.authorizationStatus {
        case .authorized:
          permissionState = .allowed
        case .denied:
          permissionState = .denied
        case .notDetermined:
          permissionState = .notRequested
        case .provisional:
          permissionState = .allowed
        @unknown default:
          permissionState = .denied
        }
        semaphore.signal()
      })
      _ = semaphore.wait(timeout: .now() + 5)
      return permissionState
    }
  }

  func requestCameraPermission (callback: @escaping (PermissionState) -> Void) {
    let session: AVCaptureDevice.DiscoverySession
    if #available(iOS 10.2, *) {
      session = AVCaptureDevice.DiscoverySession(deviceTypes: [AVCaptureDevice.DeviceType.builtInDualCamera, AVCaptureDevice.DeviceType.builtInTelephotoCamera, AVCaptureDevice.DeviceType.builtInWideAngleCamera], mediaType: AVMediaType.video, position: .back)
    } else {
      session = AVCaptureDevice.DiscoverySession(deviceTypes: [AVCaptureDevice.DeviceType.builtInDuoCamera, AVCaptureDevice.DeviceType.builtInTelephotoCamera, AVCaptureDevice.DeviceType.builtInWideAngleCamera], mediaType: AVMediaType.video, position: .back)
    }

    // ASSUMPTION: No valid retail iOS device ships without a camera, so in this scenario we're clearly in the simulator, so we default to .denied here.
    guard session.devices.count > 0 else {
      return callback(.denied)
    }

    AVCaptureDevice.requestAccess(for: AVMediaType.video) { granted in
      DispatchQueue.main.async {
        if granted {
          callback(.allowed)
        } else {
          callback(.denied)
        }
      }
    }
  }

  func requestLocationPermission (callback: @escaping (PermissionState) -> Void) {
    locationManagerDelegate?.requestWhenInUseAuthorisation().then { status in
      switch status {
      case .denied: callback(.denied)
      case .authorizedAlways: callback(.allowed)
      case .authorizedWhenInUse: callback(.allowed)
      case .restricted: callback(.denied)
      case .notDetermined: callback(.notRequested)
      @unknown default:
        callback(.denied)
      }
    }
  }

  func requestNotificationsPermission (callback: @escaping (PermissionState) -> Void) {
    UNUserNotificationCenter.current().requestAuthorization(options: [.alert, .badge, .sound]) { _, _ in
      UNUserNotificationCenter.current().getNotificationSettings { settings in
        DispatchQueue.main.async {
          switch settings.authorizationStatus {
          case .authorized:
            UIApplication.shared.registerForRemoteNotifications()
            callback(.allowed)
          case .denied:
            callback(.denied)
          case .notDetermined:
            callback(.notRequested)
          case .provisional:
            UIApplication.shared.registerForRemoteNotifications()
            callback(.allowed)
          @unknown default:
            callback(.denied)
          }
        }
      }
    }
  }
}

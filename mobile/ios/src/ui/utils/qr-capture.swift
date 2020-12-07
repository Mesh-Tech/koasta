import Foundation
import AVFoundation
import UIKit

@objc protocol QRCaptureUtilDelegate {
  @objc optional func qrUtil(_ qrUtil: QRCaptureUtil, didCapture qrCode: String)
}

@objc protocol QRCaptureUtil {
  weak var delegate: QRCaptureUtilDelegate? {get set}

  func startCapturing(in hostView: UIView) -> Bool

  func stopCapturing(in hostView: UIView?)
}

@objc class NativeQRCaptureUtil: NSObject, QRCaptureUtil, AVCaptureMetadataOutputObjectsDelegate {
  weak var delegate: QRCaptureUtilDelegate?
  fileprivate var captureMetadataOutput: AVCaptureMetadataOutput?
  fileprivate var captureSession: AVCaptureSession?
  fileprivate var videoPreviewLayer: AVCaptureVideoPreviewLayer?

  func startCapturing(in hostView: UIView) -> Bool {
    let session: AVCaptureDevice.DiscoverySession
    if #available(iOS 10.2, *) {
      session = AVCaptureDevice.DiscoverySession(deviceTypes: [AVCaptureDevice.DeviceType.builtInDualCamera, AVCaptureDevice.DeviceType.builtInTelephotoCamera, AVCaptureDevice.DeviceType.builtInWideAngleCamera], mediaType: AVMediaType.video, position: .back)
    } else {
      session = AVCaptureDevice.DiscoverySession(deviceTypes: [AVCaptureDevice.DeviceType.builtInDuoCamera, AVCaptureDevice.DeviceType.builtInTelephotoCamera, AVCaptureDevice.DeviceType.builtInWideAngleCamera], mediaType: AVMediaType.video, position: .back)
    }

    guard session.devices.count > 0 else { return false }

    do {
      let input = try AVCaptureDeviceInput(device: session.devices[0])
      captureSession = AVCaptureSession()
      captureSession?.addInput(input)

      captureMetadataOutput = AVCaptureMetadataOutput()
      captureSession?.addOutput(captureMetadataOutput!)

      captureMetadataOutput?.setMetadataObjectsDelegate(self, queue: DispatchQueue.main)
      captureMetadataOutput?.metadataObjectTypes = [AVMetadataObject.ObjectType.qr]

      videoPreviewLayer = AVCaptureVideoPreviewLayer(session: captureSession!)
      videoPreviewLayer?.videoGravity = AVLayerVideoGravity.resizeAspectFill
      videoPreviewLayer?.frame = hostView.layer.bounds
      hostView.layer.addSublayer(videoPreviewLayer!)

      captureSession?.startRunning()
      return true
    } catch {
      print(error)
      return false
    }
  }

  func stopCapturing(in hostView: UIView?) {
    if let output = captureMetadataOutput {
      captureSession?.removeOutput(output)
      captureMetadataOutput = nil
    }

    guard let _ = hostView else { return }

    captureSession?.stopRunning()
    videoPreviewLayer?.removeFromSuperlayer()
    videoPreviewLayer?.session = nil
    videoPreviewLayer = nil
    captureSession = nil
  }

  func metadataOutput(_ output: AVCaptureMetadataOutput, didOutput metadataObjects: [AVMetadataObject], from connection: AVCaptureConnection) {
    let qrObjects = metadataObjects.filter { $0.type == .qr }
    guard qrObjects.count > 0 else { return }

    guard let qr = qrObjects.first as? AVMetadataMachineReadableCodeObject else { return }
    guard let qrValue = qr.stringValue else { return }
    delegate?.qrUtil?(self, didCapture: qrValue)
  }
}

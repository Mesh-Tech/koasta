package io.meshtech.koasta.location

import android.Manifest.permission.ACCESS_FINE_LOCATION
import android.app.Activity
import android.location.Location
import android.os.Looper
import androidx.annotation.RequiresPermission
import com.google.android.gms.location.*
import java.lang.ref.WeakReference

interface ILocationListener {
  fun receivedLocation(loc: Location)
}

interface ILocationProvider {
  fun init(activity: Activity)
  @RequiresPermission(ACCESS_FINE_LOCATION)
  fun start(l: ILocationListener)
  fun stop()
}

class LocationProvider: ILocationProvider {
  private lateinit var mLocationCallback: LocationCallback
  private var listener: WeakReference<ILocationListener> = WeakReference<ILocationListener>(null)
  private lateinit var mLocationClient: FusedLocationProviderClient

  override fun init(activity: Activity) {
    mLocationClient = LocationServices.getFusedLocationProviderClient(activity)
  }

  @RequiresPermission(ACCESS_FINE_LOCATION)
  override fun start(l: ILocationListener) {
    this.listener = WeakReference(l)

    val request = LocationRequest()
    request.maxWaitTime = 28000

    mLocationCallback = object : LocationCallback() {
      override fun onLocationResult(p0: LocationResult?) {
        super.onLocationResult(p0)

        if (p0 == null) {
          return
        }

        listener.get()?.receivedLocation(p0.lastLocation)
      }
    }

    mLocationClient.requestLocationUpdates(request, mLocationCallback, Looper.getMainLooper())
  }

  override fun stop() {
    this.listener.clear()
    if (this::mLocationClient.isInitialized && this::mLocationCallback.isInitialized) {
      mLocationClient.removeLocationUpdates(mLocationCallback)
    }
  }
}

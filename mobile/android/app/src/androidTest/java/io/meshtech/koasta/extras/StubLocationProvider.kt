package io.meshtech.koasta.extras

import android.app.Activity
import android.location.Location
import android.location.LocationManager
import io.meshtech.koasta.location.ILocationListener
import io.meshtech.koasta.location.ILocationProvider

class StubLocationProvider: ILocationProvider {
  override fun init(activity: Activity) {}

  override fun start(l: ILocationListener) {
    val loc = Location(LocationManager.GPS_PROVIDER)
    loc.latitude = 50.8236745
    loc.longitude = -0.1423743
    l.receivedLocation(loc)
  }

  override fun stop() {}
}

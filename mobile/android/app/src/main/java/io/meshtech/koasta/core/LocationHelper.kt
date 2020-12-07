package io.meshtech.koasta.core

import android.location.Location

class LocationHelper private constructor() {
    companion object {
        fun fromLatLon(lat: Double?, lon: Double?): Location? {
            if (lat == null || lon == null) {
                return null
            }

            val loc = Location("")
            loc.latitude = lat
            loc.longitude = lon
            return loc
        }

        fun fromLatLon(lat: String?, lon: String?): Location? {
            if (lat == null || lon == null) {
                return null
            }

            val latd = lat.toDouble()
            val lond = lon.toDouble()

            val loc = Location("")
            loc.latitude = latd
            loc.longitude = lond
            return loc
        }
    }
}

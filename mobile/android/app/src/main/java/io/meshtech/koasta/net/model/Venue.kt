package io.meshtech.koasta.net.model

import android.location.Location
import com.squareup.moshi.JsonClass
import io.meshtech.koasta.core.Placeholders
import java.io.IOException

@JsonClass(generateAdapter = true)
data class VenuePlaceholder (val backgroundIndex: Int, val imageAIndex: Int, val imageBIndex: Int, val imageCIndex: Int, val imageDIndex: Int) {
  companion object {
    fun randomPlaceholder(): VenuePlaceholder {
      val backgroundIndex = Placeholders.backgroundColours.indices.shuffled().first()
      val imageIndices = Placeholders.placeholderImages.indices.shuffled().subList(0, 4)

      return VenuePlaceholder(
        Placeholders.backgroundColours[backgroundIndex],
        Placeholders.placeholderImages[imageIndices[0]],
        Placeholders.placeholderImages[imageIndices[1]],
        Placeholders.placeholderImages[imageIndices[2]],
        Placeholders.placeholderImages[imageIndices[3]]
      )
    }
  }
}

enum class VenueServingType(val value: Int) {
  BAR_SERVICE(0),
  TABLE_SERVICE(1),
  BAR_AND_TABLE_SERVICE(2)
}

@JsonClass(generateAdapter = true)
data class Venue (
  val venueId: Int,
  val venueCode: String,
  val companyId: Int,
  val venueName: String,
  val venueAddress: String,
  val venuePostCode: String,
  val venuePhoneNumber: String?,
  val venueContact: String?,
  val venueDescription: String?,
  val venueNotes: String?,
  val imageUrl: String?,
  val tags: List<String>?,
  val venueLatitude: String?,
  val venueLongitude: String?,
  val androidDistanceDescription: String?,
  val externalLocationId: String?,
  val progress: Int,
  val isOpen: Boolean,
  val servingType: Int,
  var placeholder: VenuePlaceholder? = null
) {
  fun realServingType(): VenueServingType {
    return when(servingType) {
      1 -> VenueServingType.TABLE_SERVICE
      2 -> VenueServingType.BAR_AND_TABLE_SERVICE
      else -> VenueServingType.BAR_SERVICE
    }
  }

  companion object {
    fun determineDistanceDescription(venue: Venue, location: Location?): String {
      if (location == null || venue.venueLatitude == null || venue.venueLongitude == null) {
        return venue.venuePostCode
      }

      return try {
        val latitude = venue.venueLatitude.toDoubleOrNull()
        val longitude = venue.venueLongitude.toDoubleOrNull()

        if (latitude == null || longitude == null) {
          return venue.venuePostCode
        }

        val result = FloatArray(1)
        Location.distanceBetween(latitude, longitude, location.latitude, location.longitude, result)

        "%.0fm".format(result[0])
      } catch (ex: IOException) {
        venue.venuePostCode
      }
    }
  }

  fun withPlaceholder(): Venue {
    if (imageUrl != null) {
      return this
    }

    this.placeholder = VenuePlaceholder.randomPlaceholder()
    return this
  }
}

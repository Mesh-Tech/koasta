package io.meshtech.koasta.net.internal

import com.github.kittinunf.fuel.Fuel
import com.github.kittinunf.fuel.coroutines.awaitStringResponseResult
import com.squareup.moshi.*
import com.squareup.moshi.adapters.Rfc3339DateJsonAdapter
import io.meshtech.koasta.BuildConfig
import io.meshtech.koasta.core.LocationHelper
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.net.ApiLegalDocument
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.*
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.math.BigDecimal
import java.math.RoundingMode
import java.net.URLEncoder
import java.util.*

class BigDecimalAdapter: JsonAdapter<BigDecimal>() {
  override fun fromJson(reader: JsonReader): BigDecimal? {
    return BigDecimal(reader.nextDouble()).setScale(2, RoundingMode.HALF_EVEN)
  }

  override fun toJson(writer: JsonWriter, value: BigDecimal?) {
    writer.value(value?.toDouble())
  }
}

class Api constructor (private val sessionManager: ISessionManager): IApi {
  private val moshi = Moshi.Builder()
    .add(Date::class.java, Rfc3339DateJsonAdapter().nullSafe())
    .add(BigDecimal::class.java, BigDecimalAdapter())
    .build()

  override suspend fun login(credentials: Credentials): ApiPlainResult {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/auth/authorise"
      var req = Fuel
        .post(url)
        .body(moshi.adapter(Credentials::class.java).toJson(credentials))
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val webResult = req
        .awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        lateinit var result: ApiPlainResult

        if (webResult.second.statusCode == -1) {
          result = ApiPlainResult("Network error")
        } else if (webResult.second.statusCode == 401) {
          result = ApiPlainResult("Unauthorised")
        } else if (webResult.second.statusCode < 200 || webResult.second.statusCode >= 300) {
          result = ApiPlainResult("Internal server error")
        } else {
          result = ApiPlainResult(null)
        }

        result
      }
    }
  }

  override suspend fun getVenues(lat: Double?, lon: Double?): ApiResult<List<Venue>> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url =
        if (lat != null && lon != null) "${BuildConfig.API_URL}/venue/nearby-venues/${lat}/${lon}"
        else "${BuildConfig.API_URL}/venue"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<List<Venue>>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          var results = moshi.adapter<List<Venue>>(
            Types.newParameterizedType(
              MutableList::class.java,
              Venue::class.java
            )).fromJson(wr.third.get())

          results = (results ?: emptyList()).map {
            Venue(
              venueId = it.venueId,
              venueCode = it.venueCode,
              companyId = it.companyId,
              venueName = it.venueName,
              venueAddress = it.venueAddress,
              venuePostCode = it.venuePostCode,
              venuePhoneNumber = it.venuePhoneNumber,
              venueContact = it.venueContact,
              venueDescription = it.venueDescription,
              venueNotes = it.venueNotes,
              imageUrl = it.imageUrl,
              tags = it.tags,
              venueLatitude = it.venueLatitude,
              venueLongitude = it.venueLongitude,
              androidDistanceDescription = Venue.determineDistanceDescription(it, LocationHelper.fromLatLon(lat, lon)),
              externalLocationId = it.externalLocationId,
              progress = it.progress,
              isOpen = it.isOpen,
              servingType = it.servingType
            ).withPlaceholder()
          }

          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun queryVenues(query: String): ApiResult<List<Venue>> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/venue/named/"+ URLEncoder.encode(query, "UTF-8")

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<List<Venue>>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          var results = moshi.adapter<List<Venue>>(
            Types.newParameterizedType(
              MutableList::class.java,
              Venue::class.java
            )).fromJson(wr.third.get())

          results = (results ?: emptyList()).map {
            Venue(
              venueId = it.venueId,
              venueCode = it.venueCode,
              companyId = it.companyId,
              venueName = it.venueName,
              venueAddress = it.venueAddress,
              venuePostCode = it.venuePostCode,
              venuePhoneNumber = it.venuePhoneNumber,
              venueContact = it.venueContact,
              venueDescription = it.venueDescription,
              venueNotes = it.venueNotes,
              imageUrl = it.imageUrl,
              tags = it.tags,
              venueLatitude = it.venueLatitude,
              venueLongitude = it.venueLongitude,
              androidDistanceDescription = Venue.determineDistanceDescription(it, null),
              externalLocationId = it.externalLocationId,
              progress = it.progress,
              isOpen = it.isOpen,
              servingType = it.servingType
            ).withPlaceholder()
          }

          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun getMenus(venueId: Int): ApiResult<List<Menu>> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/menu/$venueId"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<List<Menu>>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          val results = moshi.adapter<List<Menu>>(
            Types.newParameterizedType(
              MutableList::class.java,
              Menu::class.java
            )).fromJson(wr.third.get())

          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun getVenue(venueId: Int): ApiResult<Venue> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/venue/${venueId}"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<Venue>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          val results = moshi.adapter<Venue>(Venue::class.java).fromJson(wr.third.get())?.withPlaceholder()
          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun sendOrder(order: Order): ApiResult<SendOrderResult> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/order"

      var req = Fuel
        .post(url)
        .body(moshi.adapter(Order::class.java).toJson(order))
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<SendOrderResult>

        result = if (wr.second.statusCode != 200) {
          ApiResult(null, null)
        } else {
          val results = moshi.adapter<SendOrderResult>(SendOrderResult::class.java).fromJson(wr.third.get())
          ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun requestOrderEstimate(order: DraftOrder): ApiResult<EstimateOrderResult> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/order/estimate"

      var req = Fuel
        .post(url)
        .body(moshi.adapter(DraftOrder::class.java).toJson(order))
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<EstimateOrderResult>

        result = if (wr.second.statusCode != 200) {
          ApiResult(null, null)
        } else {
          val results = moshi.adapter<EstimateOrderResult>(EstimateOrderResult::class.java).fromJson(wr.third.get())
          ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun getOrders(): ApiResult<List<HistoricalOrder>> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/order"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<List<HistoricalOrder>>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          val results = moshi.adapter<List<HistoricalOrder>>(
            Types.newParameterizedType(
              MutableList::class.java,
              HistoricalOrder::class.java
            )).fromJson(wr.third.get())

          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun createDevice(token: String): ApiResult<Void> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/users/me/devices"

      var req = Fuel
        .post(url)
        .body(moshi.adapter(DeviceRegistration::class.java).toJson(DeviceRegistration(token)))
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<Void> = if (wr.second.statusCode != 201) {
          ApiResult(null, "Failed")
        } else {
          ApiResult(null, null)
        }

        result
      }
    }
  }

  override suspend fun getOrder(id: Int): ApiResult<HistoricalOrder> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/order/$id"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<HistoricalOrder>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          val results = moshi.adapter<HistoricalOrder>(HistoricalOrder::class.java).fromJson(wr.third.get())

          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun fetchLegalDocument(type: ApiLegalDocument): ApiResult<String> {
    val document = when(type) {
      ApiLegalDocument.PRIVACY_POLICY -> "privacy-policy.md"
      ApiLegalDocument.TERMS_AND_CONDITIONS -> "terms-and-conditions.md"
    }

    return withContext(Dispatchers.IO) {
      val url = "https://s3-eu-west-1.amazonaws.com/koasta-shared-pub/legal/$document"

      val req = Fuel.get(url)
      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        if (wr.second.statusCode == 200) {
          ApiResult(wr.third.get(), null)
        } else {
          ApiResult<String>(null, "Fetch error")
        }
      }
    }
  }

  override suspend fun submitReview(id: Int, review: VenueReview): ApiResult<Void> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/venue/${id}/review"

      var req = Fuel
        .post(url)
        .body(moshi.adapter(VenueReview::class.java).toJson(review))
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<Void> = if (wr.second.statusCode != 200) {
          ApiResult(null, "Failed")
        } else {
          ApiResult(null, null)
        }

        result
      }
    }
  }

  override suspend fun getFlags(): ApiResult<FeatureFlags> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/flags/current"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<FeatureFlags>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          val results = moshi.adapter<FeatureFlags>(FeatureFlags::class.java).fromJson(wr.third.get())
          result = ApiResult(results, null)
        }

        result
      }
    }
  }

  override suspend fun getUserProfile(): ApiResult<UserProfile> {
    val accessKey = sessionManager.authToken
    val authType = sessionManager.authType

    return withContext(Dispatchers.IO) {
      val url = "${BuildConfig.API_URL}/users/me"

      var req = Fuel
        .get(url)
        .header("Content-Type", "application/json")
        .header("User-Agent", "pubcrawl/android")
        .timeout(1800)

      if (accessKey != null) {
        req = req.header("Authorization", "Bearer $accessKey")
      }
      if (authType != null) {
        req = req.header("x-koasta-authtype", authType)
      }

      val wr = req.awaitStringResponseResult()

      return@withContext withContext(Dispatchers.Main) {
        val result: ApiResult<UserProfile>

        if (wr.second.statusCode != 200) {
          result = ApiResult(null, null)
        } else {
          val results = moshi.adapter<UserProfile>(UserProfile::class.java).fromJson(wr.third.get())
          result = ApiResult(results, null)
        }

        result
      }
    }
  }
}

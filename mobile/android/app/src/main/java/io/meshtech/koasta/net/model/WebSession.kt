package io.meshtech.koasta.net.model

import com.squareup.moshi.Json
import com.squareup.moshi.JsonClass
import java.util.*

@JsonClass(generateAdapter = true)
data class WebSession(val authToken: String,
                      val refreshToken: String,
                      @Json(name = "refreshTokenExpiry")
                      val refreshExpiry: Date,
                      @Json(name = "authTokenExpiry")
                      val expiry: Date)

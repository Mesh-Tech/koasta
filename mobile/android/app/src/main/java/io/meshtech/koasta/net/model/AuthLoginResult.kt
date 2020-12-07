package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass
import java.util.*

enum class AuthLoginStatus {
  verify, done
}

@JsonClass(generateAdapter = true)
data class WebAuthLoginResult(val authToken: String?,
                              val refreshToken: String?,
                              val refreshExpiry: Date?,
                              val expiry: Date?)

data class AuthLoginResult(val authToken: String?,
                           val refreshToken: String?,
                           val refreshExpiry: Date?,
                           val expiry: Date?,
                           val status: AuthLoginStatus?)

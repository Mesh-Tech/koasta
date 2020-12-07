package io.meshtech.koasta.net.model

import com.squareup.moshi.JsonClass

@JsonClass(generateAdapter = true)
data class DeviceRegistration(val token: String, val platform: Int = 2)

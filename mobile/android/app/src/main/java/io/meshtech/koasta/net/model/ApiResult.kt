package io.meshtech.koasta.net.model

data class ApiPlainResult(val error: String?)

data class ApiResult<T>(val data: T?, val error: String?)

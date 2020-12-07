package io.meshtech.koasta.data.internal

import com.facebook.login.LoginManager
import com.google.android.gms.auth.api.signin.GoogleSignIn
import com.google.android.gms.auth.api.signin.GoogleSignInOptions
import com.squareup.moshi.FromJson
import com.squareup.moshi.JsonClass
import com.squareup.moshi.Moshi
import com.squareup.moshi.ToJson
import com.squareup.moshi.adapters.Rfc3339DateJsonAdapter
import io.meshtech.koasta.BuildConfig
import io.meshtech.koasta.GlobalApplication
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.data.Session
import io.meshtech.koasta.net.model.UserProfile
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import net.swiftzer.semver.SemVer
import java.io.File
import java.util.*

internal class SemVerAdapter {
  @ToJson
  fun toJson(semver: SemVer): String {
    return semver.toString()
  }

  @FromJson
  fun fromJson(semver: String): SemVer {
    return SemVer.parse(semver)
  }
}

@JsonClass(generateAdapter = true)
internal data class SessionV1 constructor(
  var token: String? = null,
  var refreshToken: String? = null,
  var lastVenue: String? = null,
  var cachedVenueLocations: Map<String, String>? = null,
  var pushToken: String? = null,
  var hasSkippedOnboarding: Boolean? = null,
  var phoneNumber: String? = null,
  var authTokenExpiry: Date? = null,
  var refreshTokenExpiry: Date? = null
)

@JsonClass(generateAdapter = true)
internal data class SessionV2 constructor(
  var source: Int? = null
)

@JsonClass(generateAdapter = true)
internal data class SessionHolder (
  var version: SemVer? = null,
  var v1: SessionV1? = null,
  var v2: SessionV2? = null
)

internal class SessionImpl(holder: SessionHolder? = null) : Session {
  var version: SemVer? = holder?.version
  var v1: SessionV1? = holder?.v1
  var v2: SessionV2? = holder?.v2

  fun toHolder(): SessionHolder {
    return SessionHolder(version, v1, v2)
  }

  override var authenticationToken: String?
    get() = v1?.token
    set(value) {
      v1?.token = value
    }

  override var refreshToken: String?
    get() = v1?.refreshToken
    set(value) {
      v1?.refreshToken = value
    }

  override var lastVenue: String?
    get() = v1?.lastVenue
    set(value) {
      v1?.lastVenue = value
    }

  override var cachedVenueLocations: Map<String, String>?
    get() = v1?.cachedVenueLocations
    set(value) {
      v1?.cachedVenueLocations = value
    }

  override var pushToken: String?
    get() = v1?.pushToken
    set(value) {
      v1?.pushToken = value
    }

  override var hasSkippedOnboarding: Boolean?
    get() = v1?.hasSkippedOnboarding
    set(value) {
      v1?.hasSkippedOnboarding = value
    }

  override var phoneNumber: String?
    get() = v1?.phoneNumber
    set(value) {
      v1?.phoneNumber = value
    }

  override var authTokenExpiry: Date?
    get() = v1?.authTokenExpiry
    set(value) {
      v1?.authTokenExpiry = value
    }

  override var refreshTokenExpiry: Date?
    get() = v1?.refreshTokenExpiry
    set(value) {
      v1?.refreshTokenExpiry = value
    }

  override var source: Int?
    get() = v2?.source
    set(value) {
      v2?.source = value
    }

  fun copy(): SessionImpl {
    val ret = SessionImpl()

    ret.version = version?.copy()
    ret.v1 = v1?.copy()
    ret.v2 = v2?.copy()

    return ret
  }

  init {
    version = version ?: SessionManager.LATEST_VERSION
    v1 = v1 ?: SessionV1(null, null, null,
      null, null, null,
      null, null, null)
    v2 = v2 ?: SessionV2(
      null
    )
  }
}

class SessionManager(private val facebookProvider: IFacebookSessionProvider, private val googleProvider: IGoogleSessionProvider): ISessionManager {
  companion object {
    val LATEST_VERSION = SemVer(0, 0, 2)
  }

  private var session: SessionImpl = SessionImpl()
  private val moshi = Moshi.Builder()
    .add(Date::class.java, Rfc3339DateJsonAdapter().nullSafe())
    .add(SemVerAdapter())
    .build()
  override var currentProfile: UserProfile? = null

  override val currentSession: Session
    get() = session.copy()

  override val isAuthenticated: Boolean
    get() {
      if (session.source == null) {
        return false
      }

      return when (session.source) {
        1 -> {
          facebookProvider.isAuthenticated
        }
        3 -> {
          googleProvider.isAuthenticated
        }
        else -> {
          false
        }
      }
    }
  override val isExpired: Boolean
    get() {
      if (session.source == null) {
        return false
      }

      return when (session.source) {
        1 -> {
          facebookProvider.isExpired
        }
        3 -> {
          googleProvider.isExpired
        }
        else -> {
          false
        }
      }
    }
  override val authToken: String?
    get() {
      if (session.source == null) {
        return null
      }

      return when (session.source) {
        1 -> {
          facebookProvider.authToken
        }
        3 -> {
          googleProvider.authToken
        }
        else -> {
          null
        }
      }
    }
  override val authType: String?
    get() {
      if (session.source == null) {
        return null
      }

      return when (session.source) {
        1 -> {
          "facebook"
        }
        3 -> {
          "google"
        }
        else -> {
          null
        }
      }
    }

  override val accountDescription: String?
    get() {
      if (session.source == null) {
        return null
      }

      return when (session.source) {
        1 -> {
          facebookProvider.accountDescription
        }
        3 -> {
          googleProvider.accountDescription
        }
        else -> {
          null
        }
      }
    }

  override val firstName: String?
    get() {
      if (session.source == null) {
        return null
      }

      return when (session.source) {
        1 -> {
          facebookProvider.firstName ?: currentProfile?.firstName
        }
        3 -> {
          googleProvider.firstName ?: currentProfile?.firstName
        }
        else -> {
          null
        }
      }
    }

  override val lastName: String?
    get() {
      if (session.source == null) {
        return null
      }

      return when (session.source) {
        1 -> {
          facebookProvider.lastName ?: currentProfile?.lastName
        }
        3 -> {
          googleProvider.lastName ?: currentProfile?.lastName
        }
        else -> {
          null
        }
      }
    }

  fun registerBespokeSession(newSession: Any?) {
    if (session.source == null) {
      return
    }

    when (session.source) {
      1 -> {
        facebookProvider.registerBespokeSession(newSession)
      }
      3 -> {
        googleProvider.registerBespokeSession(newSession)
      }
    }
  }

  override suspend fun restore(): Session {
    return withContext(Dispatchers.IO) {
      val file = File(GlobalApplication.shared.filesDir, "session.json")
      if (file.exists()) {
        session = SessionImpl(moshi.adapter(SessionHolder::class.java).fromJson(file.readText()) ?: SessionHolder())
      }

      withContext(Dispatchers.Main) {
        this@SessionManager.session
      }
    }
  }

  override suspend fun persist(session: Session) {
    return withContext(Dispatchers.IO) {
      val file = File(GlobalApplication.shared.filesDir, "session.json")
      file.writeText(moshi.adapter(SessionHolder::class.java).toJson((session as SessionImpl).toHolder()))

      this@SessionManager.session = session
    }
  }

  override fun purge() {
    val file = File(GlobalApplication.shared.filesDir, "session.json")
    if (file.exists()) {
      file.delete()
    }
    session = SessionImpl(SessionHolder())
    LoginManager.getInstance().logOut()
    val signInClient = GoogleSignIn.getClient(GlobalApplication.shared, GoogleSignInOptions.Builder(
      GoogleSignInOptions.DEFAULT_SIGN_IN)
      .requestIdToken(BuildConfig.GOOGLE_API_KEY)
      .requestEmail()
      .build())
    signInClient.signOut()
    currentProfile = null
  }
}

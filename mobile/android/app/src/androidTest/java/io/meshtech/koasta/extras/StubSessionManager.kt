package io.meshtech.koasta.extras

import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.data.Session
import io.meshtech.koasta.net.model.UserProfile
import java.util.*

data class StubSession(
  override var authenticationToken: String? = null,
  override var refreshToken: String? = null,
  override var lastVenue: String? = null,
  override var cachedVenueLocations: Map<String, String>? = null,
  override var pushToken: String? = null,
  override var hasSkippedOnboarding: Boolean? = null,
  override var phoneNumber: String? = null,
  override var authTokenExpiry: Date? = null,
  override var refreshTokenExpiry: Date? = null,
  override var source: Int? = null
) : Session

class StubSessionManager(
  private var session: StubSession = StubSession(),
  override var isAuthenticated: Boolean = false,
  override var isExpired: Boolean = false,
  override var authToken: String? = null,
  override var authType: String? = null,
  override var accountDescription: String? = null,
  override var firstName: String? = null,
  override var lastName: String? = null,
  override var currentProfile: UserProfile? = null
) : ISessionManager {
  override suspend fun restore(): Session = session
  override suspend fun persist(session: Session) {}
  override fun purge() {
    session = StubSession()
    currentProfile = null
  }

  override val currentSession: Session = session

  fun signIn() {
    isAuthenticated = true
    isExpired = false
    authToken = "a"
    authType = "google"
    accountDescription = "b"
    firstName = "c"
    lastName = "d"
    currentProfile = UserProfile("a", true, listOf(123))
  }
}

package io.meshtech.koasta.data

import io.meshtech.koasta.net.model.UserProfile

interface ISessionManager {
  val currentSession: Session
  val isAuthenticated: Boolean
  val isExpired: Boolean
  val authToken: String?
  val authType: String?
  val accountDescription: String?
  val firstName: String?
  val lastName: String?
  var currentProfile: UserProfile?

  suspend fun restore(): Session
  suspend fun persist(session: Session)
  fun purge()
}

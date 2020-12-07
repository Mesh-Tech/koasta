package io.meshtech.koasta.data

interface ISessionProvider {
  fun initialise()
  fun registerBespokeSession(session: Any?)

  val isAuthenticated: Boolean
  val isExpired: Boolean
  val authToken: String?
  val accountDescription: String?
  val firstName: String?
  val lastName: String?
}

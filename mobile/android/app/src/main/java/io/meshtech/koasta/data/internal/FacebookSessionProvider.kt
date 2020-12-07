package io.meshtech.koasta.data.internal

import com.facebook.AccessToken
import com.facebook.Profile

class FacebookSessionProvider: IFacebookSessionProvider {
  override fun initialise() {}

  override fun registerBespokeSession(session: Any?) {}

  override val isAuthenticated: Boolean
    get() {
      val token = AccessToken.getCurrentAccessToken() ?: return false
      return !token.isExpired
    }

  override val isExpired: Boolean
    get() {
      val token = AccessToken.getCurrentAccessToken() ?: return false
      return token.isExpired
    }

  override val authToken: String?
    get() {
      val token = AccessToken.getCurrentAccessToken() ?: return null
      return token.token
    }

  override val accountDescription: String?
    get() {
      val profile = Profile.getCurrentProfile() ?: return null

      val firstName = profile.firstName ?: ""
      val lastName = profile.lastName ?: ""

      val ret = listOf(firstName, lastName).joinToString(" ").trim()

      return if (ret.isEmpty()) { null } else ret
    }

  override val firstName: String?
    get() {
      val profile = Profile.getCurrentProfile() ?: return null
      return profile.firstName ?: "Anonymous"
    }

  override val lastName: String?
    get() {
      val profile = Profile.getCurrentProfile() ?: return null
      return profile.lastName ?: ""
    }
}

package io.meshtech.koasta.data.internal

import com.google.android.gms.auth.api.signin.GoogleSignIn
import io.meshtech.koasta.GlobalApplication

class GoogleSessionProvider: IGoogleSessionProvider {
  override fun initialise() {
  }

  override fun registerBespokeSession(session: Any?) {}

  override val isAuthenticated: Boolean
    get() = GoogleSignIn.getLastSignedInAccount(GlobalApplication.shared) != null

  override val isExpired: Boolean
    get() = false

  override val authToken: String?
    get() = GoogleSignIn.getLastSignedInAccount(GlobalApplication.shared)?.idToken

  override val accountDescription: String?
    get() = GoogleSignIn.getLastSignedInAccount(GlobalApplication.shared)?.displayName

  override val firstName: String?
    get() = GoogleSignIn.getLastSignedInAccount(GlobalApplication.shared)?.givenName

  override val lastName: String?
    get() = GoogleSignIn.getLastSignedInAccount(GlobalApplication.shared)?.familyName
}

package io.meshtech.koasta.view

import android.app.Activity
import android.content.Intent
import com.google.android.gms.auth.api.signin.GoogleSignIn
import com.google.android.gms.auth.api.signin.GoogleSignInClient
import com.google.android.gms.auth.api.signin.GoogleSignInOptions
import com.google.android.gms.common.SignInButton
import com.google.android.gms.common.api.ApiException
import io.meshtech.koasta.BuildConfig
import io.meshtech.koasta.activity.AuthenticationActivity

interface IAuthActivityAdapter {
  fun init(activity: Activity)

  fun connect(button: SignInButton, activity: Activity)

  fun handleGoogleActivityResult(activity: Activity, requestCode: Int, resultCode: Int, data: Intent?)
}

/**
 * This class is soley to allow the AuthenticationActivity to be easily tested.
 * The Google sign in button doesn't do anything special, (unlike Facebook's) so
 * the usual behaviour you'd attach to it is instead put here so we can swap
 * it out with test-specific behaviour.
 */
class AuthActivityAdapter: IAuthActivityAdapter {
  private lateinit var signInClient: GoogleSignInClient

  override fun init(activity: Activity) {
    signInClient = GoogleSignIn.getClient(activity, GoogleSignInOptions.Builder(
      GoogleSignInOptions.DEFAULT_SIGN_IN)
      .requestIdToken(BuildConfig.GOOGLE_API_KEY)
      .requestEmail()
      .build())
  }

  override fun connect(button: SignInButton, activity: Activity) {
    val realActivity = if (activity is AuthenticationActivity) {
      activity
    } else {
      return
    }

    button.setOnClickListener {
      realActivity.disableUI()

      val signInIntent: Intent = signInClient.signInIntent
      realActivity.startActivityForResult(signInIntent, AuthenticationActivity.RESULT_ID_GOOGLE_AUTH)
    }
  }

  override fun handleGoogleActivityResult(
    activity: Activity,
    requestCode: Int,
    resultCode: Int,
    data: Intent?
  ) {
    val realActivity = if (activity is AuthenticationActivity) {
      activity
    } else {
      return
    }

    try {
      val completedTask = GoogleSignIn.getSignedInAccountFromIntent(data)
      if (completedTask.getResult(ApiException::class.java) == null) {
        realActivity.revertUI()
      }

      realActivity.continueGoogleAuth()
    } catch (ex: Exception) {
      realActivity.revertUI()
      return
    }
  }
}

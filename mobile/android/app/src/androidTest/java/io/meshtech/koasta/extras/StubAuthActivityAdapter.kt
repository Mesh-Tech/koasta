package io.meshtech.koasta.extras

import android.app.Activity
import android.content.Intent
import com.google.android.gms.common.SignInButton
import io.meshtech.koasta.activity.AuthenticationActivity
import io.meshtech.koasta.view.IAuthActivityAdapter

class StubAuthActivityAdapter: IAuthActivityAdapter {
  override fun init(activity: Activity) {}

  override fun connect(button: SignInButton, activity: Activity) {
    val realActivity = if (activity is AuthenticationActivity) {
      activity
    } else {
      return
    }

    button.setOnClickListener {
      realActivity.continueGoogleAuth()
    }
  }

  override fun handleGoogleActivityResult(
    activity: Activity,
    requestCode: Int,
    resultCode: Int,
    data: Intent?
  ) {}
}

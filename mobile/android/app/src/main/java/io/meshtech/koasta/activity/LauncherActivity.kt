package io.meshtech.koasta.activity

import android.content.Intent
import android.os.Bundle
import android.view.WindowManager
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import com.google.android.gms.common.ConnectionResult.*
import com.google.android.gms.common.GoogleApiAvailability
import io.meshtech.koasta.R
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.net.IApi
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject

class LauncherActivity : AppCompatActivity(), CoroutineScope by MainScope() {
  private val sessions: ISessionManager by inject()
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private var job: Job? = null

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_launcher)
    window.setFlags(WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS, WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS)
    verifyLibraryAvailability()
  }

  private fun verifyLibraryAvailability() {
    val result = GoogleApiAvailability.getInstance().isGooglePlayServicesAvailable(this)
    if (result == SUCCESS) {
      job = loadData()
      return
    }

    if (result == SERVICE_MISSING || result == SERVICE_VERSION_UPDATE_REQUIRED || result == SERVICE_DISABLED) {
      val d = GoogleApiAvailability.getInstance().getErrorDialog(this, result, 0 /* No fucking clue what it wants here. Docs are useless as usual. */)
      d.show()
      return
    }

    Toast.makeText(this, getString(R.string.google_play_missing), Toast.LENGTH_LONG).show()
  }

  private fun loadData() = launch {
    sessions.restore()
    val flags = api.getFlags()
    var currentFacebookSessionDisabled = false

    if (flags.data != null) {
      caches.flags = flags.data
      if (sessions.authType == "facebook" && flags.data.flags.facebookAuth != true) {
        currentFacebookSessionDisabled = true
      }
    }

    var profileMismatch = false

    if (sessions.isAuthenticated) {
      val profile = api.getUserProfile()
      if (profile.data == null) {
        profileMismatch = true
      } else {
        sessions.currentProfile = profile.data
      }
    }

    if (sessions.isAuthenticated && !currentFacebookSessionDisabled && !profileMismatch) {
      launchHomeActivity()
    } else {
      sessions.purge()
      if (currentFacebookSessionDisabled) {
        Toast.makeText(this@LauncherActivity, getString(R.string.error_facebook_broken), Toast.LENGTH_LONG).show()
      }
      launchOnboardingActivity()
    }
  }

  private fun launchOnboardingActivity() {
    job = null
    val intent = Intent(this, OnboardingActivity::class.java)
    intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_TASK_ON_HOME or Intent.FLAG_ACTIVITY_NO_ANIMATION
    startActivity(intent)
    finish()
  }

  private fun launchHomeActivity() {
    job = null
    val intent = Intent(this, MainActivity::class.java)
    intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_TASK_ON_HOME or Intent.FLAG_ACTIVITY_NO_ANIMATION
    startActivity(intent)
    finish()
  }

  override fun onDestroy() {
    job?.cancel()
    job = null
    super.onDestroy()
  }
}

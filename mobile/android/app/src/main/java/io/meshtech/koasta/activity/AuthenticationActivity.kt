package io.meshtech.koasta.activity

import android.animation.ObjectAnimator
import android.content.Intent
import android.net.Uri
import android.os.Bundle
import android.text.Html
import android.text.SpannableStringBuilder
import android.text.method.LinkMovementMethod
import android.text.style.ClickableSpan
import android.text.style.URLSpan
import android.view.MenuItem
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.facebook.*
import com.facebook.internal.Utility
import com.facebook.internal.Utility.GraphMeRequestWithCacheCallback
import com.facebook.login.LoginManager
import com.facebook.login.LoginResult
import com.facebook.login.widget.LoginButton
import com.google.android.gms.common.SignInButton
import io.meshtech.koasta.R
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.data.Session
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.Credentials
import io.meshtech.koasta.view.IAuthActivityAdapter
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.json.JSONObject
import org.koin.android.ext.android.inject
import java.lang.ref.WeakReference
import kotlin.math.min


class AuthenticationActivity : AppCompatActivity(), FacebookCallback<LoginResult>, CoroutineScope by MainScope() {
  companion object {
    const val INTENT_EXTRA_SHOULD_SPAWN_HOME = "should-spawn-home"
    const val RESULT_ID_GOOGLE_AUTH = 1337
  }

  private val api: IApi by inject()
  private val caches: Caches by inject()
  private val sessionManager: ISessionManager by inject()
  private val authAdapter: IAuthActivityAdapter by inject()
  private var shouldSpawnHome = true
  private val callbackManager = CallbackManager.Factory.create()
  private lateinit var googleLoginButton: SignInButton
  private lateinit var facebookLoginButton: LoginButton
  private lateinit var contentLayout: ViewGroup
  private lateinit var progressLayout: ViewGroup
  private lateinit var legalText: TextView
  private var job: Job? = null

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    authAdapter.init(this)
    setContentView(R.layout.activity_authentication)
    setSupportActionBar(findViewById(R.id.toolbar))

    googleLoginButton = findViewById(R.id.authentication_google_button)
    authAdapter.connect(googleLoginButton, this)
    googleLoginButton.getChildAt(0)?.let {
      val smaller = min(it.paddingLeft, it.paddingRight)
      it.setPadding(smaller, it.paddingTop, smaller, it.paddingBottom)
    }

    facebookLoginButton = findViewById(R.id.authentication_facebook_button)
    if (caches.flags.flags.facebookAuth == true) {
      facebookLoginButton.setPermissions("public_profile")
      facebookLoginButton.setOnClickListener {
        onStartFacebookLogin()
      }
      LoginManager.getInstance().registerCallback(callbackManager, this)
    } else {
      facebookLoginButton.visibility = View.GONE
    }

    contentLayout = findViewById(R.id.authentication_content_layout)
    progressLayout = findViewById(R.id.authentication_progress_layout)
    legalText = findViewById(R.id.verify_phone_input_verification_wizard_legal)
    setTextViewHTML(legalText, resources.getString(R.string.authentication_input_verification_wizard_legal))

    supportActionBar?.title = "";
    supportActionBar?.setDisplayHomeAsUpEnabled(false)

    shouldSpawnHome = intent.getBooleanExtra(INTENT_EXTRA_SHOULD_SPAWN_HOME, true)
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id = item.itemId

    if (id == android.R.id.home) {
      onBackPressed()
      return true
    }

    return super.onOptionsItemSelected(item)
  }

  fun disableUI() {
    googleLoginButton.isEnabled = false
    facebookLoginButton.isEnabled = false

    run {
      val from = 0.0f
      val to = 1.0f
      val slider = ObjectAnimator.ofFloat(progressLayout, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }

    run {
      val from = 1.0f
      val to = 0.0f
      val slider = ObjectAnimator.ofFloat(contentLayout, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }
  }

  private fun onStartFacebookLogin() {
    if (AccessToken.getCurrentAccessToken() != null && !AccessToken.getCurrentAccessToken().isExpired) {
      return
    }

    disableUI()
  }

  override fun onSuccess(result: LoginResult?) {
    val session = sessionManager.currentSession
    session.source = 1

    val accessToken = AccessToken.getCurrentAccessToken()
    if (AccessToken.isCurrentAccessTokenActive()) {
      Utility.getGraphMeRequestWithCacheAsync(
        accessToken.token,
        object : GraphMeRequestWithCacheCallback {
          @Suppress("SENSELESS_COMPARISON") // Throws NPE if null provided on Uri.parse. Kotlin is wrong here.
          override fun onSuccess(userInfo: JSONObject) {
            val id = userInfo.optString("id") ?: return
            val link = userInfo.optString("link")
            val profile = Profile(
              id,
              userInfo.optString("first_name"),
              userInfo.optString("middle_name"),
              userInfo.optString("last_name"),
              userInfo.optString("name"),
              if (link != null) Uri.parse(link) else null
            )
            Profile.setCurrentProfile(profile)
            job = doAuthenticate(session)
          }

          override fun onFailure(error: FacebookException) {
            job = doAuthenticate(session)
          }
        })
    } else {
      job = doAuthenticate(session)
    }
  }

  override fun onCancel() {
    revertUI()
  }

  override fun onError(error: FacebookException?) {
    revertUI()
  }

  private fun doAuthenticate(session: Session) = launch {
    try {
      sessionManager.persist(session)
      if (sessionManager.isAuthenticated) {
        val result = api.login(Credentials(sessionManager.firstName, sessionManager.lastName))
        if (result.error != null) {
          revertUI()
        } else {
          completeAuth()
        }
        job = null
      } else {
        revertUI()
      }
    } catch(ex: Exception) {
      job = null
      revertUI()
    }
  }

  fun revertUI() {
    googleLoginButton.isEnabled = true
    facebookLoginButton.isEnabled = true

    run {
      val from = 1.0f
      val to = 0.0f
      val slider = ObjectAnimator.ofFloat(progressLayout, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }

    run {
      val from = 0.0f
      val to = 1.0f
      val slider = ObjectAnimator.ofFloat(contentLayout, "alpha", from, to);
      slider.duration = 300
      slider.start()
    }
  }

  fun continueGoogleAuth() {
    try {
      val session = sessionManager.currentSession
      session.source = 3
      job = doAuthenticate(session)
    } catch (ex: Exception) {
      revertUI()
      return
    }
  }

  private fun completeAuth() {
    if (shouldSpawnHome) {
      val intent = Intent(this, MainActivity::class.java)
      intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_TASK_ON_HOME or Intent.FLAG_ACTIVITY_NO_ANIMATION
      startActivity(intent)
    }
    finish()
  }

  override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
    if (requestCode == RESULT_ID_GOOGLE_AUTH) {
      authAdapter.handleGoogleActivityResult(this, requestCode, resultCode, data)
      return
    }

    callbackManager.onActivityResult(requestCode, resultCode, data)
    super.onActivityResult(requestCode, resultCode, data)
  }

  override fun onDestroy() {
    super.onDestroy()
    job?.cancel()
    LoginManager.getInstance().unregisterCallback(callbackManager)
  }

  private fun makeLinkClickable(
    strBuilder: SpannableStringBuilder,
    span: URLSpan?
  ) {
    val start = strBuilder.getSpanStart(span)
    val end = strBuilder.getSpanEnd(span)
    val flags = strBuilder.getSpanFlags(span)
    val that = WeakReference(this)
    val clickable: ClickableSpan = object : ClickableSpan() {
      override fun onClick(widget: View) {
        val there = that.get()
        if (there != null) {
          val intent = Intent(there, LegalActivity::class.java)
          intent.putExtra(LegalActivity.INTENT_EXTRA_LEGAL_DOCUMENT_TYPE, span?.url ?: "")
          there.startActivity(intent)
        }
      }
    }
    strBuilder.setSpan(clickable, start, end, flags)
    strBuilder.removeSpan(span)
  }

  private fun setTextViewHTML(text: TextView, html: String?) {
    val sequence: CharSequence = Html.fromHtml(html)
    val strBuilder = SpannableStringBuilder(sequence)
    val urls =
      strBuilder.getSpans(0, sequence.length, URLSpan::class.java)
    for (span in urls) {
      makeLinkClickable(strBuilder, span)
    }
    text.text = strBuilder
    text.movementMethod = LinkMovementMethod.getInstance()
  }
}

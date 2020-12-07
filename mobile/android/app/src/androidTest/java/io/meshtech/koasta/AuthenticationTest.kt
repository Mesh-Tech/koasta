package io.meshtech.koasta

import android.content.Intent
import androidx.test.espresso.Espresso
import androidx.test.espresso.action.ViewActions
import androidx.test.espresso.assertion.ViewAssertions
import androidx.test.espresso.matcher.ViewMatchers.*
import androidx.test.rule.ActivityTestRule
import io.meshtech.koasta.activity.LauncherActivity
import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.extras.*
import io.meshtech.koasta.location.ILocationProvider
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.ApiPlainResult
import io.meshtech.koasta.view.IAuthActivityAdapter
import org.junit.Rule
import org.junit.Test
import org.koin.core.module.Module
import org.koin.dsl.module

class AuthenticationTest: BaseTest() {
  private val sessions = StubSessionManager()
  private val api = StubApi()

  @get:Rule
  val activityRule = ActivityTestRule(LauncherActivity::class.java, true, false)

  override fun configureDi(): Module {
    return module {
      single<ISessionManager> { sessions }
      single<IApi> { api }
      single { Caches() }
      single<IBillingManager> { StubBilling() }
      single<IAuthActivityAdapter> { StubAuthActivityAdapter() }
      factory<ILocationProvider> { StubLocationProvider() }
    }
  }

  @Test
  fun handlesAuthFailure() {
    activityRule.launchActivity(Intent())
    api.loginResult = ApiPlainResult("failed")
    Espresso.onView(isRoot()).perform(waitId(R.id.onboarding_start_button, 10000))
    Espresso.onView(withId(R.id.onboarding_start_button)).perform(ViewActions.click())
    Espresso.onView(withId(R.id.authentication_google_button)).perform(ViewActions.click())
    Espresso.onView(withId(R.id.authentication_google_button)).check(ViewAssertions.matches(isEnabled()))
  }

  @Test
  fun handlesAuthSuccess() {
    activityRule.launchActivity(Intent())
    api.loginResult = ApiPlainResult(null)
    Espresso.onView(isRoot()).perform(waitId(R.id.onboarding_start_button, 10000))
    Espresso.onView(withId(R.id.onboarding_start_button)).perform(ViewActions.click())
    sessions.signIn()
    Espresso.onView(withId(R.id.authentication_google_button)).perform(ViewActions.click())
    Espresso.onView(withId(R.id.venues_loading_scrollview)).check(ViewAssertions.matches(isDisplayed()))
  }
}

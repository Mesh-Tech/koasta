package io.meshtech.koasta

import android.content.Intent
import androidx.test.espresso.Espresso
import androidx.test.espresso.action.ViewActions
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.matcher.ViewMatchers
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.filters.LargeTest
import androidx.test.rule.ActivityTestRule
import io.meshtech.koasta.activity.LauncherActivity
import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.extras.*
import io.meshtech.koasta.location.ILocationProvider
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.view.AuthActivityAdapter
import io.meshtech.koasta.view.IAuthActivityAdapter
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith
import org.koin.core.module.Module
import org.koin.dsl.module

@RunWith(AndroidJUnit4::class)
@LargeTest
class OnboardingTest: BaseTest() {
  private val api = StubApi()

  @get:Rule
  val activityRule = ActivityTestRule(LauncherActivity::class.java, true, false)

  override fun configureDi(): Module {
    return module {
      single<ISessionManager> { StubSessionManager() }
      single<IApi> { api }
      single { Caches() }
      single<IBillingManager> { StubBilling() }
      single<IAuthActivityAdapter> { AuthActivityAdapter() }
      factory<ILocationProvider> { StubLocationProvider() }
    }
  }

  @Test
  fun onboardingAppears() {
    activityRule.launchActivity(Intent())
    Espresso.onView(ViewMatchers.withId(R.id.onboarding_start_button)).check(matches(isDisplayed()))
  }

  @Test
  fun canNavigateToAuthentication() {
    activityRule.launchActivity(Intent())
    Espresso.onView(ViewMatchers.withId(R.id.onboarding_start_button)).perform(ViewActions.click())
    Espresso.onView(ViewMatchers.withId(R.id.authentication_input_number_wizard_title)).check(matches(isDisplayed()))
  }
}

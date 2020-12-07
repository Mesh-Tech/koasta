package io.meshtech.koasta

import android.content.Intent
import androidx.test.espresso.Espresso
import androidx.test.espresso.assertion.ViewAssertions.matches
import androidx.test.espresso.matcher.ViewMatchers.isDisplayed
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.rule.ActivityTestRule
import io.meshtech.koasta.activity.LauncherActivity
import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.extras.*
import io.meshtech.koasta.location.ILocationProvider
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.ApiResult
import io.meshtech.koasta.net.model.Venue
import io.meshtech.koasta.view.IAuthActivityAdapter
import org.junit.Before
import org.junit.Rule
import org.junit.Test
import org.koin.core.module.Module
import org.koin.dsl.module

class VenuesTest: BaseTest() {
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

  @Before
  override fun beforeTest() {
    sessions.signIn()
    super.beforeTest()
  }

  @Test
  fun showsLoadingState() {
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.venues_loading_scrollview)).check(matches(isDisplayed()))
  }

  @Test
  fun showsEmptyState() {
    api.getVenuesResult = ApiResult(emptyList(), null)
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.venues_empty_title)).check(matches(isDisplayed()))
  }

  @Test
  fun showsVotingState() {
    api.getVenuesResult = ApiResult(listOf(
      Venue(0, "a", 1, "a", "a" ,"a", "a", "a", "a", "a", null, null, null, null, "a", "a", 0, true),
      Venue(0, "a", 2, "a", "a" ,"a", "a", "a", "a", "a", null, null, null, null, "a", "a", 0, true)
    ), null)
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.venue_list_recycler)).check(matches(isDisplayed()))
    Espresso.onView(withId(R.id.vote_item_a)).check(matches(isDisplayed()))
  }

  @Test
  fun showsLoadedState() {
    api.getVenuesResult = ApiResult(listOf(
      Venue(0, "a", 2, "a", "a" ,"a", "a", "a", "a", "a", null, null, null, null, "a", "a", 0, true)
    ), null)
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.venue_list_recycler)).check(matches(isDisplayed()))
  }
}

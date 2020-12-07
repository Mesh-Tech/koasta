package io.meshtech.koasta

import android.content.Intent
import androidx.test.espresso.Espresso
import androidx.test.espresso.action.ViewActions
import androidx.test.espresso.assertion.ViewAssertions
import androidx.test.espresso.matcher.ViewMatchers
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
import io.meshtech.koasta.net.model.HistoricalOrder
import io.meshtech.koasta.net.model.Venue
import io.meshtech.koasta.view.IAuthActivityAdapter
import org.junit.Rule
import org.junit.Test
import org.koin.core.module.Module
import org.koin.dsl.module
import java.math.BigDecimal
import java.util.*

class OrdersTest: BaseTest() {
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

  override fun beforeTest() {
    sessions.signIn()
    api.getVenuesResult = ApiResult(listOf(
      Venue(0, "a", 2, "a", "a" ,"a", "a", "a", "a", "a", null, null, null, null, "a", "a", 0, true)
    ), null)
    super.beforeTest()
  }

  @Test
  fun showsLoadingState() {
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.orders)).perform(ViewActions.click())
    Espresso.onView(withId(R.id.loading)).check(ViewAssertions.matches(ViewMatchers.isDisplayed()))
  }

  @Test
  fun showsEmptyState() {
    api.getOrdersResult = ApiResult(emptyList(), null)
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.orders)).perform(ViewActions.click())
    Espresso.onView(withId(R.id.orders_empty_title)).check(ViewAssertions.matches(ViewMatchers.isDisplayed()))
  }

  @Test
  fun showsLoadedState() {
    api.getOrdersResult = ApiResult(listOf(
      HistoricalOrder(0, "a", "b", "c", 1, 2, 1, Date(), 1, "a", emptyList(), BigDecimal(10), BigDecimal(1))
    ), null)
    activityRule.launchActivity(Intent())
    Espresso.onView(withId(R.id.orders)).perform(ViewActions.click())
    Espresso.onView(withId(R.id.recycler)).check(ViewAssertions.matches(ViewMatchers.isDisplayed()))
  }
}

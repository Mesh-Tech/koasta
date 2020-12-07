package io.meshtech.koasta

import android.content.Intent
import androidx.test.espresso.Espresso
import androidx.test.espresso.assertion.ViewAssertions
import androidx.test.espresso.matcher.ViewMatchers
import androidx.test.rule.ActivityTestRule
import io.meshtech.koasta.activity.PaymentFlowActivity
import io.meshtech.koasta.activity.PaymentFlowActivity.Companion.VENUE_ID
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

class PaymentTest: BaseTest() {
  private val sessions = StubSessionManager()
  private val api = StubApi()
  private val billing = StubBilling()

  @get:Rule
  val activityRule = ActivityTestRule(PaymentFlowActivity::class.java, true, false)

  override fun configureDi(): Module {
    return module {
      single<ISessionManager> { sessions }
      single<IApi> { api }
      single { Caches() }
      single<IBillingManager> { billing }
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
  fun showsOverview() {
    api.getVenueResult = ApiResult(
      Venue(0, "a", 2, "a", "a" ,"a", "a", "a", "a", "a", null, null, null, null, "a", "a", 0, true)
      , null
    )
    api.getMenusResult = ApiResult(
      emptyList(),
      null
    )

    val intent = Intent()
    intent.putExtra(VENUE_ID, 1)
    activityRule.launchActivity(intent)
    Espresso.onView(ViewMatchers.withId(R.id.venue_detail_banner_image)).check(ViewAssertions.matches(ViewMatchers.isDisplayed()))
  }
}

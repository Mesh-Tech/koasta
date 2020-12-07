package io.meshtech.koasta

import android.content.Intent
import androidx.test.espresso.Espresso
import androidx.test.espresso.assertion.ViewAssertions
import androidx.test.espresso.matcher.ViewMatchers
import androidx.test.rule.ActivityTestRule
import io.meshtech.koasta.activity.OrderStatusActivity
import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.billing.SquareBillingManager
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.extras.*
import io.meshtech.koasta.location.ILocationProvider
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.ApiResult
import io.meshtech.koasta.net.model.HistoricalOrder
import io.meshtech.koasta.view.IAuthActivityAdapter
import org.junit.Rule
import org.junit.Test
import org.koin.core.module.Module
import org.koin.dsl.module
import java.math.BigDecimal
import java.util.*

class OrderTest: BaseTest() {
  private val sessions = StubSessionManager()
  private val api = StubApi()

  @get:Rule
  val activityRule = ActivityTestRule(OrderStatusActivity::class.java, true, false)

  override fun configureDi(): Module {
    return module {
      single<ISessionManager> { sessions }
      single<IApi> { api }
      single { Caches() }
      single<IBillingManager> { SquareBillingManager( get() ) }
      single<IAuthActivityAdapter> { StubAuthActivityAdapter() }
      factory<ILocationProvider> { StubLocationProvider() }
    }
  }

  override fun beforeTest() {
    sessions.signIn()
    api.getOrderResult = ApiResult(
      HistoricalOrder(0, "a", "b", "c", 1, 2, 1, Date(), 1, "a", emptyList(), BigDecimal(10), BigDecimal(1)),
      null
    )
    super.beforeTest()
  }

  @Test
  fun showsOrder() {
    val intent = Intent()
    intent.putExtra(OrderStatusActivity.BUNDLE_KEY_ORDER_ID, 2)
    activityRule.launchActivity(intent)
    Espresso.onView(ViewMatchers.withId(R.id.recycler)).check(ViewAssertions.matches(ViewMatchers.isDisplayed()))
  }
}

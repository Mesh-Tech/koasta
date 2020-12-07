package io.meshtech.koasta.extras

import android.app.Activity
import android.view.View
import androidx.test.espresso.PerformException
import androidx.test.espresso.UiController
import androidx.test.espresso.ViewAction
import androidx.test.espresso.matcher.ViewMatchers.isRoot
import androidx.test.espresso.matcher.ViewMatchers.withId
import androidx.test.espresso.util.HumanReadables
import androidx.test.espresso.util.TreeIterables
import androidx.test.ext.junit.runners.AndroidJUnit4
import io.meshtech.koasta.activity.TestableActivity
import org.hamcrest.Matcher
import org.junit.After
import org.junit.Before
import org.junit.runner.RunWith
import org.koin.core.context.KoinContextHandler
import org.koin.core.context.startKoin
import org.koin.core.module.Module
import org.koin.test.KoinTest
import java.lang.reflect.Field
import java.util.concurrent.TimeoutException


@RunWith(AndroidJUnit4::class)
abstract class BaseTest: KoinTest {
  @Before
  open fun beforeTest() {
    val modules = this.configureDi()
    startKoin {
      modules(modules)
    }
  }

  @After
  open fun tearDown() {
    val activity = getActivity()
    if (activity is TestableActivity) {
      activity.onBeforeDestroy()
    }
    KoinContextHandler.stop()
  }

  // CONFIGURATION
  protected abstract fun configureDi(): Module

  /**
   * Perform action of waiting for a specific view id.
   * @param viewId The id of the view to wait for.
   * @param millis The timeout of until when to wait for.
   */
  open fun waitId(viewId: Int, millis: Long): ViewAction? {
    return object : ViewAction {
      override fun getConstraints(): Matcher<View> {
        return isRoot()
      }

      override fun getDescription(): String {
        return "wait for a specific view with id <$viewId> during $millis millis."
      }

      override fun perform(uiController: UiController, view: View?) {
        uiController.loopMainThreadUntilIdle()
        val startTime = System.currentTimeMillis()
        val endTime = startTime + millis
        val viewMatcher: Matcher<View> = withId(viewId)
        do {
          for (child in TreeIterables.breadthFirstViewTraversal(view)) {
            // found view with required ID
            if (viewMatcher.matches(child)) {
              return
            }
          }
          uiController.loopMainThreadForAtLeast(50)
        } while (System.currentTimeMillis() < endTime)
        throw PerformException.Builder()
          .withActionDescription(this.description)
          .withViewDescription(HumanReadables.describe(view))
          .withCause(TimeoutException())
          .build()
      }
    }
  }

  @Suppress("UNCHECKED_CAST")
  fun getActivity(): Activity? {
    val activityThreadClass =
      Class.forName("android.app.ActivityThread")
    val activityThread =
      activityThreadClass.getMethod("currentActivityThread").invoke(null)
    val activitiesField: Field = activityThreadClass.getDeclaredField("mActivities")
    activitiesField.isAccessible = true
    val activities =
      activitiesField.get(activityThread) as Map<Any, Any>
        ?: return null
    for (activityRecord in activities.values) {
      val activityRecordClass: Class<*> = activityRecord.javaClass
      val pausedField: Field = activityRecordClass.getDeclaredField("paused")
      pausedField.isAccessible = true
      if (!pausedField.getBoolean(activityRecord)) {
        val activityField: Field = activityRecordClass.getDeclaredField("activity")
        activityField.isAccessible = true
        return activityField.get(activityRecord) as Activity
      }
    }
    return null
  }
}

package io.meshtech.koasta.net

import android.content.Context
import android.content.Intent
import android.util.Log
import androidx.localbroadcastmanager.content.LocalBroadcastManager
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import io.meshtech.koasta.GlobalApplication
import io.meshtech.koasta.activity.OrderStatusActivity
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.fragment.OrdersFragment
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject


class PushService : FirebaseMessagingService(), CoroutineScope by MainScope() {
  private val sessions: ISessionManager by inject()
  private val api: IApi by inject()

  companion object {
    lateinit var shared: PushService
    const val PREFERENCES_KEY = "push_prefs"
    const val PREFERENCES_ENTRY_KEY = "pushtoken"
    const val PREFERENCES_ENTRY_SUBMITTED_KEY = "pushtokensubmitted"
  }

  override fun onCreate() {
    super.onCreate()
    shared = this

    val prefs = GlobalApplication.shared.getSharedPreferences(PREFERENCES_KEY, Context.MODE_PRIVATE)
    currentPushToken = prefs.getString(PREFERENCES_ENTRY_KEY, "") ?: ""

    if (currentPushToken.isEmpty()) {
      return
    }

    // If we haven't submitted the push token yet and we have a session, try to do it
    if (!prefs.getBoolean(PREFERENCES_ENTRY_SUBMITTED_KEY, false) && !sessions.isAuthenticated) {
      submitToken()
    }
  }

  private var currentPushToken = ""

  override fun onNewToken(p0: String) {
    currentPushToken = p0
    val prefs = GlobalApplication.shared.getSharedPreferences(PREFERENCES_KEY, Context.MODE_PRIVATE)
    prefs.edit().putString(PREFERENCES_ENTRY_KEY, p0).apply()

    if (!sessions.isAuthenticated) {
      return
    }

    submitToken()
  }

  override fun onMessageReceived(p0: RemoteMessage) {
    super.onMessageReceived(p0)
    var intent = Intent(OrdersFragment.BROADCAST_EVENT_ORDERS_UPDATED)
    LocalBroadcastManager.getInstance(this).sendBroadcast(intent)

    intent = Intent(OrderStatusActivity.BROADCAST_EVENT_ORDER_UPDATED)
    LocalBroadcastManager.getInstance(this).sendBroadcast(intent)
  }

  private fun submitToken() = launch {
    val result = api.createDevice(currentPushToken)

    if (result.error != null) {
      Log.e("KOASTA", "Failed to submit push token")
    }

    val prefs = GlobalApplication.shared.getSharedPreferences(PREFERENCES_KEY, Context.MODE_PRIVATE)
    prefs.edit().putBoolean(PREFERENCES_ENTRY_SUBMITTED_KEY, true).apply()
  }
}

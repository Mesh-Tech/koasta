package io.meshtech.koasta

import android.app.Application
import android.app.NotificationChannel
import android.app.NotificationManager
import android.os.Build
import io.meshtech.koasta.di.appModule
import org.koin.android.ext.koin.androidContext
import org.koin.android.ext.koin.androidLogger
import org.koin.core.context.startKoin

class KoastaApplication : Application() {
  override fun onCreate() {
    super.onCreate()
    GlobalApplication.shared = this

    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
      // Create the NotificationChannel
      val name = getString(R.string.generic_notification_channel_name)
      val descriptionText = getString(R.string.generic_notification_channel_description)
      val importance = NotificationManager.IMPORTANCE_DEFAULT
      val mChannel = NotificationChannel(getString(R.string.generic_notification_channel_id), name, importance)
      mChannel.description = descriptionText
      val notificationManager = getSystemService(NOTIFICATION_SERVICE) as NotificationManager
      notificationManager.createNotificationChannel(mChannel)
    }

    startKoin{
      androidLogger()
      androidContext(this@KoastaApplication)
      modules(appModule)
    }
  }
}

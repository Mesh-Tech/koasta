package io.meshtech.koasta.extras

import android.app.Application
import io.meshtech.koasta.GlobalApplication

class TestApplication : Application() {
  override fun onCreate() {
    GlobalApplication.shared = this
    super.onCreate()
  }
}

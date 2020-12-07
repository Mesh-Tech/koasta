package io.meshtech.koasta

import android.app.Application

class GlobalApplication {
  companion object {
    lateinit var shared: Application
  }
}

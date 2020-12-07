package io.meshtech.koasta.activity

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.view.WindowManager
import android.widget.Button
import android.widget.ImageView
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import androidx.viewpager.widget.PagerAdapter
import androidx.viewpager.widget.ViewPager
import com.facebook.login.LoginManager
import com.google.android.gms.auth.api.signin.GoogleSignIn
import com.google.android.gms.auth.api.signin.GoogleSignInOptions
import io.meshtech.koasta.BuildConfig
import io.meshtech.koasta.R

private class OnboardingPagerAdapter(private val context: Context): PagerAdapter() {
  override fun isViewFromObject(view: View, `object`: Any): Boolean = view == `object`

  override fun getCount(): Int = 3

  override fun instantiateItem(container: ViewGroup, position: Int): Any {
    val inflater = LayoutInflater.from(context)
    val group = inflater.inflate(R.layout.onboarding_pager_cell, container, false)
    val image = group.findViewById<ImageView>(R.id.onboarding_pager_cell_image)
    val text = group.findViewById<TextView>(R.id.onboarding_pager_cell_text)
    when (position) {
      0 -> {
        image.setImageResource(R.drawable.onboarding_1_gradient)
        text.setText(R.string.onboarding_caption_page_1)
      }
      1 -> {
        image.setImageResource(R.drawable.onboarding_2_gradient)
        text.setText(R.string.onboarding_caption_page_2)
      }
      else -> {
        image.setImageResource(R.drawable.onboarding_3_gradient)
        text.setText(R.string.onboarding_caption_page_3)
      }
    }
    container.addView(group)
    return group
  }

  override fun destroyItem(container: ViewGroup, position: Int, view: Any) {
    container.removeView(view as View?);
  }
}

class OnboardingActivity : AppCompatActivity(), ViewPager.OnPageChangeListener {
  private lateinit var pager: ViewPager
  private lateinit var dotA: ImageView
  private lateinit var dotB: ImageView
  private lateinit var dotC: ImageView
  private lateinit var startButton: Button

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_onboarding)
    window.setFlags(WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS, WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS)

    pager = findViewById(R.id.onboarding_pager)
    dotA = findViewById(R.id.onboarding_page_dot_a)
    dotB = findViewById(R.id.onboarding_page_dot_b)
    dotC = findViewById(R.id.onboarding_page_dot_c)
    pager.adapter = OnboardingPagerAdapter(this)
    startButton = findViewById(R.id.onboarding_start_button)

    pager.addOnPageChangeListener(this)

    startButton.setOnClickListener {
      start()
    }

    LoginManager.getInstance().logOut()

    val signInClient = GoogleSignIn.getClient(this, GoogleSignInOptions.Builder(
      GoogleSignInOptions.DEFAULT_SIGN_IN)
      .requestIdToken(BuildConfig.GOOGLE_API_KEY)
      .requestEmail()
      .build())
    signInClient.signOut()
  }

  override fun onPageScrollStateChanged(state: Int) {}

  override fun onPageScrolled(position: Int, positionOffset: Float, positionOffsetPixels: Int) {}

  override fun onPageSelected(position: Int) {
    when (position) {
      0 -> {
        dotA.alpha = 1.0f
        dotB.alpha = 0.3f
        dotC.alpha = 0.3f
      }
      1 -> {
        dotB.alpha = 1.0f
        dotA.alpha = 0.3f
        dotC.alpha = 0.3f
      }
      else -> {
        dotC.alpha = 1.0f
        dotB.alpha = 0.3f
        dotA.alpha = 0.3f
      }
    }
  }

  private fun start() {
    val intent = Intent(this, AuthenticationActivity::class.java)
    startActivity(intent)
  }
}

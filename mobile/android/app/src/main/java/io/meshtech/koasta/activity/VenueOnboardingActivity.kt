package io.meshtech.koasta.activity

import android.content.Intent
import android.content.res.ColorStateList
import android.os.Bundle
import android.view.MenuItem
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import androidx.core.graphics.drawable.toBitmap
import coil.api.load
import io.meshtech.koasta.R
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.Venue
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject

class VenueOnboardingActivity : AppCompatActivity(), CoroutineScope by MainScope()  {
  companion object {
    const val VENUE_ID: String = "venueId"
  }

  private var currentVenue: Venue? = null
  private var job: Job? = null
  private val caches: Caches by inject()
  private val api: IApi by inject()


  private lateinit var banner: ImageView
  private lateinit var imagePlaceholderA: ImageView
  private lateinit var imagePlaceholderB: ImageView
  private lateinit var imagePlaceholderC: ImageView
  private lateinit var imagePlaceholderD: ImageView
  private lateinit var venueName: TextView
  private lateinit var title: TextView
  private lateinit var subtitle: TextView
  private lateinit var progress: ProgressBar
  private lateinit var share: Button

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_venue_onboarding)
    setSupportActionBar(findViewById(R.id.toolbar))
    supportActionBar?.setDisplayHomeAsUpEnabled(true)

    banner = findViewById(R.id.venue_detail_banner_image)
    imagePlaceholderA = findViewById(R.id.venue_item_placeholder_a)
    imagePlaceholderB = findViewById(R.id.venue_item_placeholder_b)
    imagePlaceholderC = findViewById(R.id.venue_item_placeholder_c)
    imagePlaceholderD = findViewById(R.id.venue_item_placeholder_d)
    venueName = findViewById(R.id.venue_detail_info_title)
    title = findViewById(R.id.venue_onboarding_title)
    subtitle = findViewById(R.id.venue_onboarding_subtitle)
    progress = findViewById(R.id.venue_onboarding_progress)
    share = findViewById(R.id.venue_onboarding_share_button)

    share.setOnClickListener {
      doShare()
    }

    if (!intent.hasExtra(VENUE_ID)) {
      return
    }

    val venueId = intent.getIntExtra(VENUE_ID, -1)
    job = fetchData(venueId)
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id = item.itemId

    if (id == android.R.id.home) {
      finish()
      return true
    }

    return super.onOptionsItemSelected(item)
  }

  private fun fetchData(venueId: Int) = launch {
    var venue: Venue? = null

    if (caches.venue != null) {
      venue = caches.venue
    } else {
      val result = api.getVenue(venueId)
      if (result.data != null) {
        venue = result.data
      }
    }

    if (venue == null) {
      showError()
    } else {
      bind(venue)
    }
  }

  private fun showError() {
    Toast.makeText(this, "We weren't able to fetch the details for this venue. Please try again in a moment.", Toast.LENGTH_LONG).show()
  }

  private fun bind(venue: Venue) {
    this.currentVenue = venue
    supportActionBar?.title = venue.venueName
    venueName.text = venue.venueName

    if (venue.imageUrl != null) {
      banner.load(venue.imageUrl)
    } else if (venue.placeholder != null) {
      banner.backgroundTintList = ColorStateList.valueOf(resources.getColor(venue.placeholder!!.backgroundIndex, null))
      imagePlaceholderA.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageAIndex, null).toBitmap())
      imagePlaceholderB.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageBIndex, null).toBitmap())
      imagePlaceholderC.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageCIndex, null).toBitmap())
      imagePlaceholderD.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageDIndex, null).toBitmap())
    }

    when (venue.progress) {
      0 -> {
        title.text = getString(R.string.venue_onboarding_progress_0_title)
        subtitle.text = getString(R.string.venue_onboarding_progress_0_subtitle, venue.venueName, venue.venueName)
        progress.progress = 25
      }
      1 -> {
        title.text = getString(R.string.venue_onboarding_progress_1_title)
        subtitle.text = getString(R.string.venue_onboarding_progress_1_subtitle, venue.venueName, venue.venueName)
        progress.progress = 65
      }
      else -> {
        title.text = getString(R.string.venue_onboarding_progress_2_title)
        subtitle.text = getString(R.string.venue_onboarding_progress_2_subtitle, venue.venueName)
        progress.progress = 80
      }
    }
  }

  override fun onDestroy() {
    job?.cancel()
    job = null
    caches.venue = null
    super.onDestroy()
  }

  private fun doShare() {
    if (currentVenue == null) {
      return
    }

    val sendIntent: Intent = Intent().apply {
      action = Intent.ACTION_SEND
      putExtra(Intent.EXTRA_TEXT, "I voted for ${currentVenue?.venueName ?: ""} to join Koasta. Add your vote too! https://www.koasta.com")
      type = "text/plain"
    }

    val shareIntent = Intent.createChooser(sendIntent, null)
    startActivity(shareIntent)
  }
}

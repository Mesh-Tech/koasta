package io.meshtech.koasta.activity

import android.Manifest.permission.ACCESS_FINE_LOCATION
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.content.pm.PackageManager.PERMISSION_DENIED
import android.content.pm.PackageManager.PERMISSION_GRANTED
import android.location.Location
import android.location.LocationManager
import android.os.Bundle
import android.os.Handler
import android.util.Log
import android.view.Gravity
import android.view.Menu
import android.view.MenuItem
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.content.ContextCompat
import androidx.fragment.app.FragmentContainerView
import com.google.android.material.bottomnavigation.BottomNavigationView
import com.google.firebase.iid.FirebaseInstanceId
import io.meshtech.koasta.GlobalApplication
import io.meshtech.koasta.R
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.fragment.*
import io.meshtech.koasta.location.ILocationListener
import io.meshtech.koasta.location.ILocationProvider
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.PushService
import io.meshtech.koasta.net.model.Venue
import io.meshtech.koasta.net.model.VenueReview
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import java.lang.ref.WeakReference


interface MainActivityListener {
  fun requestLocation()
  fun getVenues(): List<Venue>
  fun getLocation(): Location?
}

enum class VenuesState {
  empty, loading, data
}

class MainActivity : AppCompatActivity(), MainActivityListener, ILocationListener, TestableActivity, CoroutineScope by MainScope() {
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private val locationProvider: ILocationProvider by inject()
  val sessions: ISessionManager by inject()
  private var locationProviderStopped = false
  private var job: Job? = null
  private lateinit var container: FragmentContainerView
  private lateinit var currentFragment: WeakReference<IVenuesFragment>
  private lateinit var navbar: BottomNavigationView
  private var venues: List<Venue> = emptyList()
  private var currentLocation: Location? = null
  private var locationTimeoutHandler: Handler? = null
  private var venuesState = VenuesState.empty

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_main)
    setSupportActionBar(findViewById(R.id.main_toolbar))
    navbar = findViewById(R.id.bottom_navigation)
    supportActionBar?.setDisplayShowTitleEnabled(false);
    container = findViewById(R.id.main_fragments)

    navbar.setOnNavigationItemSelectedListener {
      var result = true
      currentFragment.clear()
      when (it.itemId) {
        R.id.home -> {
          when (venuesState) {
            VenuesState.empty -> {
              showEmptyState()
            }
            VenuesState.data -> {
              showLoadedState()
            }
            VenuesState.loading -> {
              showLoadingState()
            }
          }
        }
        R.id.orders -> {
          supportFragmentManager.beginTransaction()
            .replace(R.id.main_fragments, OrdersFragment.newInstance())
            .runOnCommit {
              supportActionBar?.hide()
            }
            .commit()
        }
        R.id.settings -> {
          supportFragmentManager.beginTransaction()
            .replace(R.id.main_fragments, SettingsFragment.newInstance())
            .runOnCommit {
              supportActionBar?.hide()
            }
            .commit()
        }
        else -> {
          result = false
        }
      }
      result
    }

    caches.clearPaymentFlowCaches()
    showLoadingState()

    val prefs = GlobalApplication.shared.getSharedPreferences(PushService.PREFERENCES_KEY, Context.MODE_PRIVATE)
    val currentPushToken = prefs.getString(PushService.PREFERENCES_ENTRY_KEY, "") ?: ""

    if (currentPushToken.isEmpty()) {
      FirebaseInstanceId.getInstance().instanceId.addOnCompleteListener { task ->
        if (task.isSuccessful) {
          val token = task.result?.token
          if (token != null) {
            prefs.edit().putString(PushService.PREFERENCES_ENTRY_KEY, token).apply()
            submitToken(token)
          }
        }
      }
    }

    // If we haven't submitted the push token yet and we have a session, try to do it
    if (!prefs.getBoolean(PushService.PREFERENCES_ENTRY_SUBMITTED_KEY, false)) {
      submitToken(currentPushToken)
    }
  }

  override fun onResume() {
    super.onResume()
    caches.clearPaymentFlowCaches()
  }

  override fun onStart() {
    super.onStart()
    caches.clearPaymentFlowCaches()
  }

  private fun startListeningForLocation() {
    if (ContextCompat.checkSelfPermission( this, ACCESS_FINE_LOCATION ) == PERMISSION_GRANTED) {
      locationProvider.init(this)

      locationTimeoutHandler = Handler()
      locationTimeoutHandler?.postDelayed({
        locationTimeoutHandler = null
        if (currentLocation == null) {
          // Hardcoded to Bond Street, Brighton if no location available
          val loc = Location(LocationManager.GPS_PROVIDER)
          loc.latitude = 50.8236745
          loc.longitude = -0.1423743
          updateUIFromLocationChange(loc)
          dataRefresh()
        }
      }, 30000)

      locationProvider.start(this)
    }
  }

  override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
    super.onActivityResult(requestCode, resultCode, data)

    if (requestCode != SearchActivity.SEARCH_REQUEST || resultCode == -1) {
      return
    }

    val intent = Intent(this, PaymentFlowActivity::class.java)
    intent.putExtra(PaymentFlowActivity.VENUE_ID, resultCode)
    startActivity(intent)
  }

  override fun onCreateOptionsMenu(menu: Menu?): Boolean {
    menuInflater.inflate(R.menu.main, menu)
    return true
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id: Int = item.itemId
    if (id == R.id.main_search) {
      val intent = Intent(this, SearchActivity::class.java)
      startActivityForResult(intent, SearchActivity.SEARCH_REQUEST)
      return true
    }

    return super.onOptionsItemSelected(item)
  }

  private fun showLoadingState() {
    venuesState = VenuesState.loading

    supportFragmentManager.beginTransaction()
      .replace(R.id.main_fragments, VenuesLoadingFragment.newInstance())
      .runOnCommit {
        supportActionBar?.show()
      }
      .commit()
  }

  private fun showEmptyState() {
    if (currentFragment.get() is VenuesEmptyFragment) {
      return
    }

    venuesState = VenuesState.empty

    supportFragmentManager.beginTransaction()
      .replace(R.id.main_fragments, VenuesEmptyFragment.newInstance())
      .runOnCommit {
        supportActionBar?.show()
      }
      .commit()
  }

  private fun showLoadedState() {
    if (currentFragment.get() is VenueListFragment) {
      return
    }

    venuesState = VenuesState.data

    supportFragmentManager.beginTransaction()
      .replace(R.id.main_fragments, VenueListFragment.newInstance())
      .runOnCommit {
        supportActionBar?.show()
      }
      .commit()
  }

  override fun requestLocation() {
    when {
      ContextCompat.checkSelfPermission(this, ACCESS_FINE_LOCATION) == PERMISSION_GRANTED -> {
        refreshCards()
      }
      shouldShowRequestPermissionRationale(ACCESS_FINE_LOCATION) -> {
    }
      else -> {
        requestPermissions(arrayOf(ACCESS_FINE_LOCATION), 1337)
      }
    }
  }

  override fun getVenues(): List<Venue> = venues
  override fun getLocation(): Location? = currentLocation

  override fun onRequestPermissionsResult(
    requestCode: Int,
    permissions: Array<out String>,
    grantResults: IntArray
  ) {
    if (requestCode != 1337) {
      return
    }

    if (permissions.isEmpty() || grantResults.isEmpty()) {
      return
    }

    if (grantResults[0] != PackageManager::PERMISSION_GRANTED.get()) {
      return
    }

    startListeningForLocation()
    refreshCards()
  }

  fun dataRefresh(ignoreLocation: Boolean = false) {
    if (currentLocation == null && !ignoreLocation) {
      return
    }

    if (currentLocation == null && ignoreLocation) {
      // Hardcoded to Bond Street, Brighton if no location available
      val loc = Location(LocationManager.GPS_PROVIDER)
      loc.latitude = 50.8236745
      loc.longitude = -0.1423743
      updateUIFromLocationChange(loc)
    }

    job = doRefresh()
  }

  private fun doRefresh() = launch {
    val result = api.getVenues(currentLocation?.latitude, currentLocation?.longitude)
    when {
      result.data == null -> {
        showFetchError()
      }
      result.data.isEmpty() -> {
        showEmptyState()
      }
      else -> {
        venues = result.data
        if (venuesState == VenuesState.data) {
          currentFragment.get()?.invalidateData()
        } else {
          showLoadedState()
        }
      }
    }
  }

  private fun showFetchError() {
    val toast = Toast.makeText(this, R.string.venues_fetch_failed, Toast.LENGTH_LONG)
    toast.setGravity(Gravity.TOP or Gravity.CENTER_HORIZONTAL, 0, 0)
    toast.show()
  }

  private fun refreshCards() {
    val permission = ContextCompat.checkSelfPermission(this, ACCESS_FINE_LOCATION)
    if (permission != PERMISSION_GRANTED && permission != PERMISSION_DENIED) {
      currentFragment.get()?.showCard(VenuesCard.location)
    } else {
      currentFragment.get()?.hideCard(VenuesCard.location)
    }
  }

  fun registerFragment(frag: IVenuesFragment) {
    currentFragment = WeakReference(frag)

    if (frag !is VenueListFragment) {
      if (ContextCompat.checkSelfPermission( this, ACCESS_FINE_LOCATION ) == PERMISSION_GRANTED) {
        startListeningForLocation()
      } else {
        dataRefresh(true)
      }
    }

    refreshCards()
  }

  fun selectVenue(venue: Venue) {
    caches.venueCache[venue.venueId] = venue
    val intent = Intent(this, PaymentFlowActivity::class.java)
    intent.putExtra(PaymentFlowActivity.VENUE_ID, venue.venueId)
    startActivity(intent)
  }

  override fun onBeforeDestroy() {
    locationProvider.stop()
    locationProviderStopped = true
  }

  override fun onDestroy() {
    job?.cancel()
    job = null
    if (!locationProviderStopped) {
      onBeforeDestroy()
    }
    super.onDestroy()
  }

  private fun updateUIFromLocationChange(p0: Location?) {
    currentLocation = p0
    currentFragment.get()?.invalidateData()
  }

  fun voteForVenue(votingVenue: Venue) {
    val profile = sessions.currentProfile ?: return

    if (profile.votedVenueIds.contains(votingVenue.venueId)) {
      val intent = Intent(this, VenueOnboardingActivity::class.java)
      intent.putExtra(VenueOnboardingActivity.VENUE_ID, votingVenue.venueId)
      caches.venue = votingVenue
      startActivity(intent)
    } else {
      profile.votedVenueIds += votingVenue.venueId
      sessions.currentProfile = profile
      submitRegisteredInterest(votingVenue.venueId)
    }
  }

  private fun submitRegisteredInterest(venueId: Int) = launch {
    try {
      api.submitReview(venueId, VenueReview(null, null, null, true))
    } catch (_: Exception) {}
  }

  override fun receivedLocation(loc: Location) {
    updateUIFromLocationChange(loc)
    if (venues.isEmpty()) {
      dataRefresh()
    }
  }

  private fun submitToken(currentPushToken: String) = launch {
    val result = api.createDevice(currentPushToken)

    if (result.error != null) {
      Log.e("KOASTA", "Failed to submit push token")
    } else {
      val prefs = GlobalApplication.shared.getSharedPreferences(PushService.PREFERENCES_KEY, Context.MODE_PRIVATE)
      prefs.edit().putBoolean(PushService.PREFERENCES_ENTRY_SUBMITTED_KEY, true).apply()
    }
  }
}

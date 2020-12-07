package io.meshtech.koasta.activity

import android.content.Context
import android.os.Bundle
import android.os.Handler
import android.view.LayoutInflater
import android.view.MenuItem
import android.view.View
import android.view.ViewGroup
import android.view.inputmethod.InputMethodManager
import android.widget.EditText
import android.widget.ImageView
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.widget.addTextChangedListener
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import coil.Coil
import coil.api.load
import coil.transform.RoundedCornersTransformation
import io.meshtech.koasta.R
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.Venue
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject
import java.lang.ref.WeakReference
import java.util.*

interface OnSearchVenueItemClickListener {
  fun onVenueItemClick(venue: Venue)
}

class SearchVenueViewHolder(val view: View) : RecyclerView.ViewHolder(view) {
  private val thumb = view.findViewById<ImageView>(R.id.venue_item_thumb)
  private val title = view.findViewById<TextView>(R.id.venue_item_title)
  private val subtitle = view.findViewById<TextView>(R.id.venue_item_subtitle)
  private val comment = view.findViewById<TextView>(R.id.venue_item_comment)
  fun bind(venue: Venue) {
    title.text = venue.venueName
    subtitle.text = venue.androidDistanceDescription ?: venue.venuePostCode
    comment.text = if (venue.isOpen) "" else view.resources.getString(R.string.venue_opening_later)
    comment.visibility = if (venue.isOpen) View.GONE else View.VISIBLE
    if (venue.imageUrl != null) {
      thumb.load(venue.imageUrl, Coil.imageLoader(view.context)) {
        transformations(RoundedCornersTransformation(20.0f))
      }
    } else {
      thumb.setImageBitmap(null)
    }
  }
}

class SearchListAdapter(context: Context,
                       private val values: List<Venue>,
                       private val listener: WeakReference<OnSearchVenueItemClickListener>
) : RecyclerView.Adapter<SearchVenueViewHolder>() {
  private val inflater = LayoutInflater.from(context)

  override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): SearchVenueViewHolder {
    val view = inflater.inflate(R.layout.layout_venue_item, parent, false)
    return SearchVenueViewHolder(view)
  }

  override fun getItemCount(): Int {
    return values.size
  }

  override fun getItemViewType(position: Int): Int {
    if (values.isEmpty()) {
      if (position == 0) {
        return -2
      }

      return -1
    }

    if (position >= values.count()) {
      return -1
    }

    return 0
  }

  override fun getItemId(position: Int): Long {
    if (getItemViewType(position) == -1) {
      return -1
    }

    if (getItemViewType(position) == -2) {
      return -2
    }

    return values[position].venueId.toLong()
  }

  override fun onBindViewHolder(holder: SearchVenueViewHolder, position: Int) {
    val value = values[position]
    holder.view.setOnClickListener {
      listener.get()?.onVenueItemClick(value)
    }
    holder.bind(value)
  }
}

class SearchActivity : AppCompatActivity(), CoroutineScope by MainScope(), OnSearchVenueItemClickListener {
  private var searchTimeoutHandler: Handler? = null
  private lateinit var recyclerView: RecyclerView
  private lateinit var searchField: EditText
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private var job: Job? = null
  private var searchToken = UUID.randomUUID()
  private var hasSearchedOnce = false
  private var destroyed = false

  companion object {
    const val SEARCH_REQUEST = 1337
  }

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    setContentView(R.layout.activity_search)
    setSupportActionBar(findViewById(R.id.toolbar))

    supportActionBar?.title = "";
    supportActionBar?.setDisplayHomeAsUpEnabled(true)

    recyclerView = findViewById(R.id.recycler)
    recyclerView.layoutManager = LinearLayoutManager(this)
    searchField = findViewById(R.id.search_field)

    searchField.addTextChangedListener {
      handleTextChanged(it.toString())
    }

    searchField.requestFocus()
    val imm: InputMethodManager = getSystemService(Context.INPUT_METHOD_SERVICE) as InputMethodManager
    imm.toggleSoftInput(InputMethodManager.SHOW_FORCED, InputMethodManager.HIDE_IMPLICIT_ONLY)
  }

  override fun onOptionsItemSelected(item: MenuItem): Boolean {
    val id = item.itemId

    if (id == android.R.id.home) {
      setResult(-1)
      val imm: InputMethodManager = getSystemService(Context.INPUT_METHOD_SERVICE) as InputMethodManager
      imm.toggleSoftInputFromWindow(recyclerView.windowToken, 0,0);
      onBackPressed()
      return true
    }

    return super.onOptionsItemSelected(item)
  }

  private fun doFetchData(query: String) = launch {
    try {
      val result = api.queryVenues(query)
      if (result.error != null || result.data == null) {
        handleError()
      } else {
        handleResults(result.data)
      }
    } catch(ex: Exception) {
      handleError()
    }
  }

  private fun handleError() {
    Toast.makeText(this, getString(R.string.error_search_failed), Toast.LENGTH_LONG).show()
  }

  private fun handleResults(results: List<Venue>) {
    recyclerView.adapter = SearchListAdapter(this, results, WeakReference(this))
  }
  
  private fun handleTextChanged(text: String) {
    if (!hasSearchedOnce && searchField.text.length < 3) {
      return
    }

    val token = UUID.randomUUID()
    searchToken = token

    searchTimeoutHandler = Handler()
    searchTimeoutHandler?.postDelayed({
      if (destroyed || searchToken != token) {
        return@postDelayed
      }

      searchTimeoutHandler = null

      doFetchData(text)
    }, 1000)
  }

  override fun onDestroy() {
    super.onDestroy()
    destroyed = true
    searchTimeoutHandler = null
    job?.cancel()
  }

  override fun onVenueItemClick(venue: Venue) {
    caches.venueCache[venue.venueId] = venue
    setResult(venue.venueId)
    val imm: InputMethodManager = getSystemService(Context.INPUT_METHOD_SERVICE) as InputMethodManager
    imm.toggleSoftInputFromWindow(recyclerView.windowToken, 0,0);
    finish()
  }
}

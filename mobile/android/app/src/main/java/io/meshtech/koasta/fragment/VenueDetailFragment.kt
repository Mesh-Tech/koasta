package io.meshtech.koasta.fragment

import android.annotation.SuppressLint
import android.content.Context
import android.content.res.ColorStateList
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.*
import androidx.core.graphics.drawable.toBitmap
import androidx.fragment.app.Fragment
import androidx.navigation.fragment.findNavController
import coil.api.load
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.PaymentFlowActivity
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.model.Menu
import io.meshtech.koasta.net.model.Venue
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.MainScope
import kotlinx.coroutines.launch
import org.koin.android.ext.android.inject

data class VenueDetailContentItem(val isTitleCard: Boolean, val isSection: Boolean, val venue: Venue? = null, val menu: Menu? = null)

class VenueDetailContentListAdapter(context: Context, private val items: List<VenueDetailContentItem>) :
  ArrayAdapter<VenueDetailContentItem>(context, android.R.layout.simple_list_item_1) {

  override fun getViewTypeCount(): Int = 3
  override fun getItemViewType(position: Int): Int = when(position) {
    0 -> 0
    1 -> 1
    else -> 1
  }

  override fun getCount(): Int = items.count()

  override fun getItem(position: Int): VenueDetailContentItem? = items[position]

  override fun getItemId(position: Int): Long {
    return when(position) {
      0 -> -1
      1 -> -2
      else -> (getItem(position)?.menu?.menuId ?: 0).toLong()
    }
  }

  @SuppressLint("InflateParams")
  override fun getView(position: Int, convertView: View?, parent: ViewGroup): View =
    when(position) {
      0 -> {
        val view = convertView ?: LayoutInflater.from(context).inflate(R.layout.venue_detail_info_cell, null)
        val title = view.findViewById<TextView>(R.id.venue_detail_info_title)
        val subtitle = view.findViewById<TextView>(R.id.venue_detail_info_subtitle)
        val item = getItem(position)

        title.text = item?.venue?.venueName ?: ""
        subtitle.text = item?.venue?.venueDescription ?: ""

        subtitle.visibility = if (subtitle.text == null || subtitle.text == "") View.GONE else View.VISIBLE

        view
      }
      1 -> {
        val view = convertView ?: LayoutInflater.from(context).inflate((R.layout.section_header), null)
        val title = view.findViewById<TextView>(android.R.id.text1)
        title.text = context.getString(R.string.section_title_menu)

        view
      }
      else -> {
        val view = convertView ?: LayoutInflater.from(context).inflate(R.layout.list_item, null)
        val item = getItem(position)
        val text = view.findViewById<TextView>(android.R.id.text1)
        text.text = item?.menu?.menuName ?: ""

        view
      }
    }
  }


class VenueDetailFragment : Fragment(), CoroutineScope by MainScope() {
  private val api: IApi by inject()
  private val caches: Caches by inject()
  private var job: Job? = null
  private var venueId: Int = -1
  private lateinit var banner: ImageView
  private lateinit var list: ListView
  private lateinit var imagePlaceholderA: ImageView
  private lateinit var imagePlaceholderB: ImageView
  private lateinit var imagePlaceholderC: ImageView
  private lateinit var imagePlaceholderD: ImageView
  private var items: List<VenueDetailContentItem> = emptyList()

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    arguments?.let {
      venueId = it.getInt(PaymentFlowActivity.VENUE_ID)
    }
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_venue_detail, container, false)
  }

  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    banner = view.findViewById(R.id.venue_detail_banner_image)
    list = view.findViewById(R.id.venue_detail_content_list)
    imagePlaceholderA = view.findViewById(R.id.venue_item_placeholder_a)
    imagePlaceholderB = view.findViewById(R.id.venue_item_placeholder_b)
    imagePlaceholderC = view.findViewById(R.id.venue_item_placeholder_c)
    imagePlaceholderD = view.findViewById(R.id.venue_item_placeholder_d)
    job = fetchData()

    list.setOnItemClickListener { _, _, pos, _ ->
      val id = items[pos].menu?.menuId
      if (id != null) {
        val args = Bundle()
        args.putInt(PaymentFlowActivity.VENUE_ID, venueId)
        args.putInt(PaymentFlowActivity.INITIAL_MENU_ID, id)
        findNavController().navigate(R.id.nav_venue_details_to_venue_menu, args)
      }
    }
  }

  private fun fetchData() = launch {
    var venue: Venue? = null

    if (caches.venueCache.containsKey(venueId)) {
      venue = caches.venueCache[venueId]!!
    } else {
      val result = api.getVenue(venueId)
      if (result.data != null) {
        venue = result.data
      }
    }

    if (venue == null) {
      showError()
    } else {
      val menus: List<Menu>
      if (caches.venueMenuCache.containsKey(venueId)) {
        menus = caches.venueMenuCache[venueId] ?: emptyList()
        bind(venue, menus)
      } else {
        val menuResult = api.getMenus(venueId)
        if (menuResult.data == null) {
          showError()
        } else {
          menus = menuResult.data
          bind(venue, menus)
        }
      }
    }
  }

  private fun showError() {
    val ctx = activity
    if (ctx == null) {
      return
    }

    Toast.makeText(ctx, "We weren't able to fetch the details for this venue. Please try again in a moment.", Toast.LENGTH_LONG).show()
  }

  private fun bind(venue: Venue, menus: List<Menu>) {
    caches.venueCache[venueId] = venue
    caches.venueMenuCache[venueId] = menus

    if (venue.imageUrl != null) {
      banner.load(venue.imageUrl)
    } else if (venue.placeholder != null) {
      banner.backgroundTintList = ColorStateList.valueOf(resources.getColor(venue.placeholder!!.backgroundIndex, null))
      imagePlaceholderA.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageAIndex, null).toBitmap())
      imagePlaceholderB.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageBIndex, null).toBitmap())
      imagePlaceholderC.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageCIndex, null).toBitmap())
      imagePlaceholderD.setImageBitmap(resources.getDrawable(venue.placeholder!!.imageDIndex, null).toBitmap())
    }

    (activity as PaymentFlowActivity).supportActionBar?.title = venue.venueName

    items = emptyList()

    items = items + VenueDetailContentItem(
      isTitleCard = true,
      isSection = false,
      venue = venue
    )

    items = items + VenueDetailContentItem(
      isTitleCard = false,
      isSection = true
    )

    menus.forEach {
      items = items + VenueDetailContentItem(
        isTitleCard = false,
        isSection = false,
        menu = it
      )
    }

    val ctx = context
    if (ctx == null) {
      return
    }

    list.adapter = VenueDetailContentListAdapter(ctx, items)
  }

  override fun onDestroyView() {
    job?.cancel()
    job = null
    super.onDestroyView()
  }
}

package io.meshtech.koasta.fragment

import android.annotation.SuppressLint
import android.content.Context
import android.content.res.ColorStateList
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.ImageView
import android.widget.TextView
import androidx.core.graphics.drawable.toBitmap
import androidx.core.view.isGone
import androidx.fragment.app.Fragment
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import androidx.swiperefreshlayout.widget.SwipeRefreshLayout
import coil.Coil
import coil.api.load
import coil.transform.RoundedCornersTransformation
import io.meshtech.koasta.R
import io.meshtech.koasta.activity.MainActivity
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.net.model.Venue
import io.meshtech.koasta.view.VenueVoteItemView
import java.lang.ref.WeakReference

interface OnVenueItemClickListener {
  fun onVenueItemClick(venue: Venue)

  fun onVotingVenueItemClick(votingVenue: Venue)
}

abstract class VenueViewHolder(val view: View) : RecyclerView.ViewHolder(view) {
  abstract fun bind()
  abstract fun bind(venue: Venue)
  abstract fun bind(votingValues: List<Venue>, sessions: ISessionManager, listener: WeakReference<OnVenueItemClickListener>)
}

class NoVenuesVenueViewHolder(view: View) : VenueViewHolder(view) {
  override fun bind() {}

  override fun bind(venue: Venue) {}

  override fun bind(votingValues: List<Venue>, sessions: ISessionManager, listener: WeakReference<OnVenueItemClickListener>) {}
}

class NormalVenueViewHolder(view: View) : VenueViewHolder(view) {
  private val thumb = view.findViewById<ImageView>(R.id.venue_item_thumb)
  private val title = view.findViewById<TextView>(R.id.venue_item_title)
  private val subtitle = view.findViewById<TextView>(R.id.venue_item_subtitle)
  private val comment = view.findViewById<TextView>(R.id.venue_item_comment)
  private val imagePlaceholderA = view.findViewById<ImageView>(R.id.venue_item_placeholder_a)
  private val imagePlaceholderB = view.findViewById<ImageView>(R.id.venue_item_placeholder_b)
  private val imagePlaceholderC = view.findViewById<ImageView>(R.id.venue_item_placeholder_c)
  private val imagePlaceholderD = view.findViewById<ImageView>(R.id.venue_item_placeholder_d)

  override fun bind() {}

  override fun bind(venue: Venue) {
    title.text = venue.venueName
    subtitle.text = venue.androidDistanceDescription ?: venue.venuePostCode
    comment.text = if (venue.isOpen) "" else view.resources.getString(R.string.venue_opening_later)
    comment.visibility = if (venue.isOpen) View.GONE else View.VISIBLE
    if (venue.imageUrl != null) {
      thumb.load(venue.imageUrl, Coil.imageLoader(view.context)) {
        transformations(RoundedCornersTransformation(20.0f))
      }
      thumb.backgroundTintList = null
    } else if (venue.placeholder != null) {
      thumb.setImageBitmap(null)
      thumb.backgroundTintList = ColorStateList.valueOf(view.resources.getColor(venue.placeholder!!.backgroundIndex, null))
      imagePlaceholderA.setImageBitmap(view.resources.getDrawable(venue.placeholder!!.imageAIndex, null).toBitmap())
      imagePlaceholderB.setImageBitmap(view.resources.getDrawable(venue.placeholder!!.imageBIndex, null).toBitmap())
      imagePlaceholderC.setImageBitmap(view.resources.getDrawable(venue.placeholder!!.imageCIndex, null).toBitmap())
      imagePlaceholderD.setImageBitmap(view.resources.getDrawable(venue.placeholder!!.imageDIndex, null).toBitmap())
    }
  }

  override fun bind(votingValues: List<Venue>, sessions: ISessionManager, listener: WeakReference<OnVenueItemClickListener>) {}
}

class VotingVenueViewHolder(view: View) : VenueViewHolder(view) {
  private val itemA = view.findViewById<VenueVoteItemView>(R.id.vote_item_a)
  private val itemB = view.findViewById<VenueVoteItemView>(R.id.vote_item_b)
  private val itemC = view.findViewById<VenueVoteItemView>(R.id.vote_item_c)
  private val itemD = view.findViewById<VenueVoteItemView>(R.id.vote_item_d)
  private val itemE = view.findViewById<VenueVoteItemView>(R.id.vote_item_e)

  override fun bind() {}

  override fun bind(venue: Venue) {}

  override fun bind(votingValues: List<Venue>, sessions: ISessionManager, listener: WeakReference<OnVenueItemClickListener>) {
    itemA.isGone = true
    itemB.isGone = true
    itemC.isGone = true
    itemD.isGone = true
    itemE.isGone = true

    val votedVenueIds = sessions.currentProfile?.votedVenueIds ?: emptyList()

    if (votingValues.isNotEmpty()) {
      itemA.setTitle(votingValues[0].venueName)
      itemA.setVoteButtonTitle(view.context.getString(R.string.voting_item_vote))
      itemA.setVoteDescription(view.context.getString(R.string.voting_item_description))
      itemA.isGone = false
      if (votedVenueIds.contains(votingValues[0].venueId)) {
        itemA.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemA.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemA.setVoteButtonAlpha(0.5f)
      }
      itemA.setOnClickListener {
        listener.get()?.onVotingVenueItemClick(votingValues[0])
        itemA.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemA.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemA.setVoteButtonAlpha(0.5f)
      }
      itemA.setThumbTint(ColorStateList.valueOf(view.resources.getColor(votingValues[0].placeholder!!.backgroundIndex, null)))
      itemA.setImagePlaceholderA(view.resources.getDrawable(votingValues[0].placeholder!!.imageAIndex, null).toBitmap())
      itemA.setImagePlaceholderB(view.resources.getDrawable(votingValues[0].placeholder!!.imageBIndex, null).toBitmap())
      itemA.setImagePlaceholderC(view.resources.getDrawable(votingValues[0].placeholder!!.imageCIndex, null).toBitmap())
      itemA.setImagePlaceholderD(view.resources.getDrawable(votingValues[0].placeholder!!.imageDIndex, null).toBitmap())
    }

    if (votingValues.size >= 2) {
      itemB.setTitle(votingValues[1].venueName)
      itemB.setVoteButtonTitle(view.context.getString(R.string.voting_item_vote))
      itemB.setVoteDescription(view.context.getString(R.string.voting_item_description))
      itemB.isGone = false
      if (votedVenueIds.contains(votingValues[1].venueId)) {
        itemB.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemB.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemB.setVoteButtonAlpha(0.5f)
      }
      itemB.setOnClickListener {
        listener.get()?.onVotingVenueItemClick(votingValues[1])
        itemB.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemB.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemB.setVoteButtonAlpha(0.5f)
      }
      itemB.setThumbTint(ColorStateList.valueOf(view.resources.getColor(votingValues[1].placeholder!!.backgroundIndex, null)))
      itemB.setImagePlaceholderA(view.resources.getDrawable(votingValues[1].placeholder!!.imageAIndex, null).toBitmap())
      itemB.setImagePlaceholderB(view.resources.getDrawable(votingValues[1].placeholder!!.imageBIndex, null).toBitmap())
      itemB.setImagePlaceholderC(view.resources.getDrawable(votingValues[1].placeholder!!.imageCIndex, null).toBitmap())
      itemB.setImagePlaceholderD(view.resources.getDrawable(votingValues[1].placeholder!!.imageDIndex, null).toBitmap())
    }

    if (votingValues.size >= 3) {
      itemC.setTitle(votingValues[2].venueName)
      itemC.setVoteButtonTitle(view.context.getString(R.string.voting_item_vote))
      itemC.setVoteDescription(view.context.getString(R.string.voting_item_description))
      itemC.isGone = false
      if (votedVenueIds.contains(votingValues[2].venueId)) {
        itemC.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemC.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemC.setVoteButtonAlpha(0.5f)
      }
      itemC.setOnClickListener {
        listener.get()?.onVotingVenueItemClick(votingValues[2])
        itemC.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemC.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemC.setVoteButtonAlpha(0.5f)
      }
      itemC.setThumbTint(ColorStateList.valueOf(view.resources.getColor(votingValues[2].placeholder!!.backgroundIndex, null)))
      itemC.setImagePlaceholderA(view.resources.getDrawable(votingValues[2].placeholder!!.imageAIndex, null).toBitmap())
      itemC.setImagePlaceholderB(view.resources.getDrawable(votingValues[2].placeholder!!.imageBIndex, null).toBitmap())
      itemC.setImagePlaceholderC(view.resources.getDrawable(votingValues[2].placeholder!!.imageCIndex, null).toBitmap())
      itemC.setImagePlaceholderD(view.resources.getDrawable(votingValues[2].placeholder!!.imageDIndex, null).toBitmap())
    }

    if (votingValues.size >= 4) {
      itemD.setTitle(votingValues[3].venueName)
      itemD.setVoteButtonTitle(view.context.getString(R.string.voting_item_vote))
      itemD.setVoteDescription(view.context.getString(R.string.voting_item_description))
      itemD.isGone = false
      if (votedVenueIds.contains(votingValues[3].venueId)) {
        itemD.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemD.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemD.setVoteButtonAlpha(0.5f)
      }
      itemD.setOnClickListener {
        listener.get()?.onVotingVenueItemClick(votingValues[3])
        itemD.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemD.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemD.setVoteButtonAlpha(0.5f)
      }
      itemD.setThumbTint(ColorStateList.valueOf(view.resources.getColor(votingValues[3].placeholder!!.backgroundIndex, null)))
      itemD.setImagePlaceholderA(view.resources.getDrawable(votingValues[3].placeholder!!.imageAIndex, null).toBitmap())
      itemD.setImagePlaceholderB(view.resources.getDrawable(votingValues[3].placeholder!!.imageBIndex, null).toBitmap())
      itemD.setImagePlaceholderC(view.resources.getDrawable(votingValues[3].placeholder!!.imageCIndex, null).toBitmap())
      itemD.setImagePlaceholderD(view.resources.getDrawable(votingValues[3].placeholder!!.imageDIndex, null).toBitmap())
    }

    if (votingValues.size >= 5) {
      itemE.setTitle(votingValues[4].venueName)
      itemE.setVoteButtonTitle(view.context.getString(R.string.voting_item_vote))
      itemE.setVoteDescription(view.context.getString(R.string.voting_item_description))
      itemE.isGone = false
      if (votedVenueIds.contains(votingValues[4].venueId)) {
        itemE.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemE.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemE.setVoteButtonAlpha(0.5f)
      }
      itemE.setOnClickListener {
        listener.get()?.onVotingVenueItemClick(votingValues[4])
        itemE.setVoteButtonTitle(view.context.getString(R.string.voting_item_voted))
        itemE.setVoteDescription(view.context.getString(R.string.voting_item_voted_description))
        itemE.setVoteButtonAlpha(0.5f)
      }
      itemE.setThumbTint(ColorStateList.valueOf(view.resources.getColor(votingValues[4].placeholder!!.backgroundIndex, null)))
      itemE.setImagePlaceholderA(view.resources.getDrawable(votingValues[4].placeholder!!.imageAIndex, null).toBitmap())
      itemE.setImagePlaceholderB(view.resources.getDrawable(votingValues[4].placeholder!!.imageBIndex, null).toBitmap())
      itemE.setImagePlaceholderC(view.resources.getDrawable(votingValues[4].placeholder!!.imageCIndex, null).toBitmap())
      itemE.setImagePlaceholderD(view.resources.getDrawable(votingValues[4].placeholder!!.imageDIndex, null).toBitmap())
    }
  }
}

class VenueListAdapter(context: Context,
                       private val sessions: ISessionManager,
                       private val values: List<Venue>,
                       private val votingValues: List<Venue>,
                       private val listener: WeakReference<OnVenueItemClickListener>) : RecyclerView.Adapter<VenueViewHolder>() {
  private val inflater = LayoutInflater.from(context)

  override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): VenueViewHolder {
    if (viewType == -1) {
      val view = inflater.inflate(R.layout.layout_venue_vote_list_item, parent, false)
      return VotingVenueViewHolder(view)
    }

    if (viewType == -2) {
      val view = inflater.inflate(R.layout.layout_venue_vote_list_only_voting_item, parent, false)
      return NoVenuesVenueViewHolder(view)
    }

    val view = inflater.inflate(R.layout.layout_venue_item, parent, false)
    return NormalVenueViewHolder(view)
  }

  override fun getItemCount(): Int {
    var count = values.size

    if (votingValues.isNotEmpty()) {
      if (count == 0) {
        count = 2
      } else {
        count += 1
      }
    }

    return count
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

  override fun onBindViewHolder(holder: VenueViewHolder, position: Int) {
    if (getItemViewType(position) == -1) {
      holder.bind(votingValues, sessions, listener)
    } else if (getItemViewType(position) == -2) {
      holder.bind()
    } else {
      val value = values[position]
      holder.view.setOnClickListener {
        listener.get()?.onVenueItemClick(value)
      }
      holder.bind(value)
    }
  }
}

class VenueListFragment : Fragment(), IVenuesFragment, OnVenueItemClickListener {
  private lateinit var locationCard: ViewGroup
  private var visibleCards: Array<VenuesCard> = emptyArray()
  private var viewCreated = false
  private lateinit var locationCardEnableLocationButton: Button
  private lateinit var recyclerView: RecyclerView
  private lateinit var refresh: SwipeRefreshLayout

  companion object {
    private const val DATA_LOAD_COMPANY_ID = 2

    @JvmStatic
    fun newInstance() =
      VenueListFragment().apply { arguments = Bundle().apply {} }
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_venue_list, container, false)
  }

  @SuppressLint("ClickableViewAccessibility")
  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    viewCreated = true
    locationCard = view.findViewById(R.id.venues_empty_location_card)
    locationCardEnableLocationButton = view.findViewById(R.id.enable_location_card_button)
    recyclerView = view.findViewById(R.id.venue_list_recycler)
    recyclerView.layoutManager = LinearLayoutManager(context)
    refresh = view.findViewById(R.id.refresh)

    refresh.setOnRefreshListener {
      val parent = activity
      if (parent !is MainActivity) {
        return@setOnRefreshListener
      }

      parent.dataRefresh()
    }

    visibleCards.forEach {
      when (it) {
        VenuesCard.location -> locationCard.visibility = View.VISIBLE
      }
    }

    visibleCards = emptyArray()

    registerAgainstActivity()

    locationCardEnableLocationButton.setOnClickListener {
      val parent = activity
      if (parent is MainActivity) {
        parent.requestLocation()
      }
    }
  }

  private fun registerAgainstActivity() {
    val parent = activity
    if (parent !is MainActivity) {
      return
    }

    parent.registerFragment(this)
    invalidateData()
  }

  override fun showCard(card: VenuesCard) {
    if (!viewCreated) {
      visibleCards += card
    } else {
      locationCard.visibility = View.VISIBLE
    }
  }

  override fun hideCard(card: VenuesCard) {
    if (!viewCreated) {
      visibleCards = visibleCards.filterNot { it != card }.toTypedArray()
    } else {
      locationCard.visibility = View.GONE
    }
  }

  override fun invalidateData() {
    val parent = activity
    if (parent !is MainActivity) {
      return
    }

    val venues = parent.getVenues()
    val normalVenues = venues.filter { it.companyId != DATA_LOAD_COMPANY_ID }
    val votingVenues = venues.filter { it.companyId == DATA_LOAD_COMPANY_ID }

    recyclerView.adapter = VenueListAdapter(parent, parent.sessions, normalVenues, votingVenues, WeakReference(this))
    recyclerView.adapter?.notifyDataSetChanged()
    refresh.isRefreshing = false
  }

  override fun onVenueItemClick(venue: Venue) {
    val parent = activity
    if (parent !is MainActivity) {
      return
    }

    parent.selectVenue(venue)
  }

  override fun onVotingVenueItemClick(votingVenue: Venue) {
    val parent = activity
    if (parent !is MainActivity) {
      return
    }

    parent.voteForVenue(votingVenue)
  }
}

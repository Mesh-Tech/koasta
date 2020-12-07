package io.meshtech.koasta.fragment

import android.content.Context
import android.os.Bundle
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import android.widget.Toast
import androidx.fragment.app.Fragment
import androidx.recyclerview.widget.LinearLayoutManager
import androidx.recyclerview.widget.RecyclerView
import io.meshtech.koasta.R
import io.meshtech.koasta.net.model.Menu
import io.meshtech.koasta.net.model.Product
import io.meshtech.koasta.net.model.Venue
import java.lang.ref.WeakReference
import java.text.NumberFormat
import java.util.*

private const val ARG_MENU_IDX = "menuIdx"

interface OnVenueProductItemClickListener {
  fun onVenueProductItemClick(value: ProductSelection)
  fun onVenueProductItemLongClick(value: ProductSelection)
}

class VenueProductViewHolder(val view: View) : RecyclerView.ViewHolder(view) {
  private val price = view.findViewById<TextView>(R.id.product_list_item_price)
  private val title = view.findViewById<TextView>(R.id.product_list_item_title)
  private val quantity = view.findViewById<TextView>(R.id.product_list_item_quantity)
  private val desc = view.findViewById<TextView>(R.id.product_list_item_description)

  fun bind(value: ProductSelection, listener: OnVenueProductItemClickListener) {
    price.text = NumberFormat.getCurrencyInstance(Locale.UK).format(value.product.price)
    title.text = value.product.productName
    view.setOnClickListener {
      listener.onVenueProductItemClick(value)
    }
    view.setOnLongClickListener {
      listener.onVenueProductItemLongClick(value)
      true
    }
    if (value.quantity > 0) {
      quantity.visibility = View.VISIBLE
      quantity.text = value.quantity.toString()
    } else {
      quantity.visibility = View.GONE
    }
    if (value.product.productDescription == null) {
      desc.text = null
      desc.visibility = View.GONE
    } else {
      desc.text = value.product.productDescription
      desc.visibility = View.VISIBLE
    }
  }
}

data class ProductSelection(val product: Product, var quantity: Int = 0)

class VenueProductListAdapter(context: Context,
                       private val values: List<ProductSelection>,
                       private val listener: OnVenueProductItemClickListener) : RecyclerView.Adapter<VenueProductViewHolder>() {
  private val inflater = LayoutInflater.from(context)

  override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): VenueProductViewHolder {
    val view = inflater.inflate(R.layout.layout_product_list_item, parent, false)
    return VenueProductViewHolder(view)
  }

  override fun getItemId(position: Int): Long {
    return values[position].product.productId.toLong()
  }

  override fun getItemCount(): Int {
    return values.size
  }

  override fun onBindViewHolder(holder: VenueProductViewHolder, position: Int) {
    val value = values[position]
    holder.bind(value, listener)
  }
}

class VenueProductListFragment : Fragment(), OnVenueProductItemClickListener {
  private lateinit var venue: Venue
  private lateinit var menu: Menu
  private lateinit var listener: WeakReference<VenueMenuFragmentListener>
  private lateinit var list: RecyclerView
  private var menuIdx: Int = -1
  private var selections: List<ProductSelection> = emptyList()

  override fun onCreate(savedInstanceState: Bundle?) {
    super.onCreate(savedInstanceState)
    arguments?.let {
      menuIdx = it.getInt(ARG_MENU_IDX)
    }
  }

  override fun onCreateView(
    inflater: LayoutInflater, container: ViewGroup?,
    savedInstanceState: Bundle?
  ): View? {
    return inflater.inflate(R.layout.fragment_venue_product_list, container, false)
  }

  override fun onViewCreated(view: View, savedInstanceState: Bundle?) {
    super.onViewCreated(view, savedInstanceState)
    list = view as RecyclerView
    list.layoutManager = LinearLayoutManager(context)

    if (menuIdx == -1) {
      return
    }

    val l = listener.get() ?: return

    this.venue = l.getVenue()
    this.menu = l.getMenu(menuIdx)
    list.adapter = VenueProductListAdapter(requireContext(), menu.products.map { ProductSelection(it, l.getSelectionQuantity(it.productId)) }, this)
    list.invalidate()
  }

  fun setListener(listener: WeakReference<VenueMenuFragmentListener>) {
    this.listener = listener
  }

  companion object {
    @JvmStatic
    fun newInstance(menuIdx: Int) =
      VenueProductListFragment().apply {
        arguments = Bundle().apply {
          putInt(ARG_MENU_IDX, menuIdx)
        }
      }
  }

  override fun onVenueProductItemClick(value: ProductSelection) {
    if (!venue.isOpen) {
      Toast.makeText(requireActivity(), getString(R.string.venue_closed_error), Toast.LENGTH_LONG).show()
      return
    }
    listener.get()?.addProductToCart(value.product)
    value.quantity++
    list.adapter?.notifyDataSetChanged()
  }

  override fun onVenueProductItemLongClick(value: ProductSelection) {
    if (!venue.isOpen) {
      Toast.makeText(requireActivity(), getString(R.string.venue_closed_error), Toast.LENGTH_LONG).show()
      return
    }
    listener.get()?.removeAllOfProductFromCart(value.product)
    value.quantity = 0
    list.adapter?.notifyDataSetChanged()
  }
}

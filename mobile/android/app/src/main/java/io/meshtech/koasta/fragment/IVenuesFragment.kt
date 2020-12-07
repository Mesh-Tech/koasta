package io.meshtech.koasta.fragment

enum class VenuesCard {
  location
}

interface IVenuesFragment {
  fun showCard(card: VenuesCard)
  fun hideCard(card: VenuesCard)
  fun invalidateData()
}

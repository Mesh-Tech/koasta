package io.meshtech.koasta.core

import io.meshtech.koasta.net.model.*

class Caches {
  fun clearPaymentFlowCaches() {
    venueCache.clear()
    venueMenuCache.clear()
    cachedCart = emptyMap()
    cachedOrder = null
    cachedEstimate = null
  }

  fun clearAll() {
    clearPaymentFlowCaches()
    orderCache = emptyList()
    venue = null
  }

  var flags: FeatureFlags = FeatureFlags(FeatureFlagListing(facebookAuth = true, googlePay = true))
  var venueCache = HashMap<Int, Venue>()
  var venueMenuCache = HashMap<Int, List<Menu>>()
  var cachedCart = emptyMap<Int, Int>()
  var cachedOrder: Order? = null
  var cachedEstimate: EstimateOrderResult? = null
  var orderCache = emptyList<HistoricalOrder>()
  var venue: Venue? = null
}

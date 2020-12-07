package io.meshtech.koasta.di

import io.meshtech.koasta.billing.IBillingManager
import io.meshtech.koasta.billing.SquareBillingManager
import io.meshtech.koasta.core.Caches
import io.meshtech.koasta.data.ISessionManager
import io.meshtech.koasta.data.internal.*
import io.meshtech.koasta.location.ILocationProvider
import io.meshtech.koasta.location.LocationProvider
import io.meshtech.koasta.net.IApi
import io.meshtech.koasta.net.internal.Api
import io.meshtech.koasta.view.AuthActivityAdapter
import io.meshtech.koasta.view.IAuthActivityAdapter
import org.koin.dsl.module

val appModule = module {
  single<IFacebookSessionProvider> { FacebookSessionProvider() }
  single<IGoogleSessionProvider> { GoogleSessionProvider() }
  single<ISessionManager> { SessionManager(get(), get()) }
  single<IApi> { Api(get()) }
  single { Caches() }
  single<IBillingManager> { SquareBillingManager( get() ) }
  single<IAuthActivityAdapter> { AuthActivityAdapter() }
  factory<ILocationProvider> { LocationProvider() }
}

import ApiService from './api' // eslint-disable-line no-unused-vars
import StorageService from './storage' // eslint-disable-line no-unused-vars
import moment from 'moment'
import { doAction } from 'fluxify'

export default class AuthService {
  /**
   * @param {ApiService} apiService
   * @param {StorageService} storageService
   */
  constructor(apiService, storageService) {
    this.apiService = apiService
    this.storageService = storageService
    this.refreshTicker = undefined

    if (this.isAuthenticated) {
      this._startTicking()
    }
  }

  get isAuthenticated() {
    const credentials = this.storageService.get('pcsession')
    if (!credentials) {
      return false
    }

    const expiry = moment(credentials.expiry)
    const now = moment()

    return now.isBefore(expiry)
  }

  get credentials() {
    return this.storageService.get('pcsession')
  }

  get userDetails() {
    return this.storageService.get('pcinfo')
  }

  invalidate() {
    this.storageService.invalidate()
    this._stopTicking()
  }

  async authenticate(username, password) {
    const session = await this.apiService.authenticate(username, password)
    if (!session) {
      throw new Error(
        `An unknown error occurred when validating your credentials. Please try again later or contact support.`
      )
    }

    this.storageService.invalidate()
    this.storageService.set('pcsession', session)

    const details = await this.apiService.fetchUserDetails()
    if (!details) {
      this.storageService.invalidate()
      throw new Error(
        `An unknown error occurred when validating your credentials. Please try again later or contact support.`
      )
    }

    const venueDetails = await this.apiService.fetchVenue(details.venueId)

    this.storageService.set('pcinfo', {
      ...details,
      venueName: (venueDetails || {}).venueName,
    })
    this._startTicking()
  }

  _startTicking() {
    if (!this.isAuthenticated) {
      return
    }

    this.refreshTicker = setInterval(() => {
      const credentials = this.storageService.get('pcsession')
      if (!credentials) {
        return this._stopTicking()
      }

      const expiry = moment(credentials.expiry).add(-1, 'minutes')
      const now = moment()

      if (now.isBefore(expiry)) {
        console.debug('Session still valid for more than 1 minute, continuing')
        if (!this.isAuthenticated) {
          this._stopTicking()
        }
        return
      }

      console.debug('Session will be invalid soon, refreshing now')
      this._stopTicking()
      this.apiService
        .refresh()
        .then(() => {
          console.debug('Session refreshed')
          this._startTicking()
        })
        .catch(() => {
          console.debug('Session failed to refresh, invalidating now')
          this._stopTicking()
          doAction('global:invalidateSession')
        })
    }, 5000)
  }

  _stopTicking() {
    if (!this.refreshTicker) {
      return
    }

    clearInterval(this.refreshTicker)
    this.refreshTicker = undefined
  }
}

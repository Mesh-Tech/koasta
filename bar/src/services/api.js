import { post, get, put } from './request'
import ConfigService from './config' // eslint-disable-line no-unused-vars
import StorageService from './storage' // eslint-disable-line no-unused-vars

const mapStatusName = (status) => {
  switch (status) {
    case 'Ordered':
      return 'ready'
    case 'In progress':
      return 'in-progress'
    case 'Ready':
      return 'ready-to-collect'
    case 'Complete':
      return 'finished'
    case 'Rejected':
      return 'cancelled'
    default:
      return status
  }
}

const mapStatusIndicator = (status) => {
  switch (status) {
    case 'ready':
      return 'Ordered'
    case 'in-progress':
      return 'InProgress'
    case 'ready-to-collect':
      return 'Ready'
    case 'finished':
      return 'Complete'
    case 'cancelled':
      return 'Rejected'
    default:
      return status
  }
}

class ApiService {
  /**
   * @param {ConfigService} configService
   * @param {StorageService} storageService
   */
  constructor(configService, storageService) {
    this.configService = configService
    this.storageService = storageService
  }

  /**
   * Authenticates against the Pubcrawl Admin API with the provided credentials.
   * @param {string} username A username
   * @param {string} password A password
   * @return {Promise<object>} An Admin API session
   */
  async authenticate(username, password) {
    const result = await post(`${this.configService.apiUrl}/auth/authorise`)
      .withCredentials()
      .send({ username, password })
      .set('x-api-key', this.configService.apiKey)
      .set('accept', 'json')

    if (result.status !== 200) {
      const err = new Error(
        `We were unable to verify your credentials. Please try again or contact support.`
      )
      err.response = result
      throw err
    }

    return result.body
  }

  async refresh() {
    const session = this.storageService.get('pcsession')

    try {
      // Attempt the refresh flow
      const refreshResult = await requestAuthenticated(this)(
        post(`${this.configService.apiUrl}/auth/refresh`)
          .withCredentials()
          .send({ refreshToken: (session || {}).refreshToken })
          .set('accept', 'json')
      )

      if (
        refreshResult.status !== 200 ||
        !refreshResult.body ||
        !refreshResult.body.authToken ||
        !refreshResult.body.refreshToken
      ) {
        return
      }

      this.storageService.set('pcsession', refreshResult.body)
      return
    } catch (err) {
      console.error(err)
      throw err
    }
  }

  async fetchUserDetails() {
    const result = await requestAuthenticated(this)(
      get(`${this.configService.apiUrl}/employee/me`)
        .withCredentials()
        .set('accept', 'json')
    )

    if (result.status >= 300 && result.status !== 404) {
      const err = new Error(
        `We were unable to verify your credentials. Please try again or contact support.`
      )
      err.response = result
      throw err
    }

    return result.body
  }

  async fetchVenue(id) {
    const result = await requestAuthenticated(this)(
      get(`${this.configService.apiUrl}/venue/${id}`)
        .withCredentials()
        .set('accept', 'json')
    )

    if (result.status >= 300 && result.status !== 404) {
      const err = new Error(
        `We were unable to verify your credentials. Please try again or contact support.`
      )
      err.response = result
      throw err
    }

    return result.body
  }

  async fetchActiveOrders() {
    const result = await requestAuthenticated(this)(
      get(
        `${this.configService.apiUrl}/order/incomplete/${
          this.storageService.get('pcinfo').venueId
        }`
      )
        .withCredentials()
        .set('accept', 'json')
    )

    if (result.status !== 200) {
      const err = new Error(
        `We were unable to verify your credentials. Please try again or contact support.`
      )
      err.response = result
      throw err
    }

    if (!result.body) {
      return result.body
    }

    return result.body
      .map((i) =>
        Object.assign(i, {
          statusName: mapStatusName(i.status),
        })
      )
      .sort((orderA, orderB) => orderA.orderTimeStamp - orderB.orderTimeStamp)
  }

  async fetchOrder(orderId) {
    const result = await requestAuthenticated(this)(
      get(`${this.configService.apiUrl}/order/${orderId}`)
        .withCredentials()
        .set('accept', 'json')
    )

    if (result.status !== 200) {
      const err = new Error(
        `We were unable to verify your credentials. Please try again or contact support.`
      )
      err.response = result
      throw err
    }

    return result.body
  }

  async updateOrderStatus(orderId, status) {
    const realStatus = mapStatusIndicator(status)
    const result = await requestAuthenticated(this)(
      put(`${this.configService.apiUrl}/order/${orderId}`)
        .withCredentials()
        .send({ statusName: realStatus })
        .set('accept', 'json')
    )

    if (result.status !== 200) {
      const err = new Error(
        `We were unable to verify your credentials. Please try again or contact support.`
      )
      err.response = result
      throw err
    }

    return realStatus
  }
}

/**
 * Ensures a request can handle authentication properly.
 * @param {ApiService} apiService
 */
const requestAuthenticated = (apiService) => async (req) => {
  const session = apiService.storageService.get('pcsession')
  if (session) {
    req.header.Authorization = `Bearer ${session.authToken}`
  }

  req.header['x-api-key'] = apiService.configService.apiKey

  let res
  try {
    res = await req
  } catch (err) {
    res = err.response
  }

  if (res.status !== 401 && res.status !== 403) {
    return res
  }

  if (!session) {
    return res
  }

  try {
    // Attempt the refresh flow
    const refreshResult = await post(
      `${apiService.configService.apiUrl}/auth/refresh`
    )
      .withCredentials()
      .send({ refreshToken: session.refreshToken })
      .set('accept', 'json')
      .set('authorization', `Bearer ${session.authToken}`)
      .set('x-api-key', apiService.configService.apiKey)

    if (
      refreshResult.status !== 200 ||
      !refreshResult.body ||
      !refreshResult.body.authToken ||
      !refreshResult.body.refreshToken
    ) {
      return res
    }

    apiService.storageService.set('pcsession', refreshResult.body)

    // Now that we've refreshed, try the request again
    req.set('Authorization', `Bearer ${refreshResult.body.authToken}`)
    res = await req
    return res
  } catch (err) {
    console.error(err)
    return res
  }
}

export default ApiService

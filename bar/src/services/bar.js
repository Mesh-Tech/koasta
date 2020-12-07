import ConfigService from './config' // eslint-disable-line no-unused-vars
import AuthService from './auth' // eslint-disable-line no-unused-vars
import { doAction } from 'fluxify'

const mapStatusName = (status) => {
  switch (status) {
    case 'Ordered':
      return 'ready'
    case 'InProgress':
      return 'in-progress'
    case 'Ready':
      return 'finished'
    case 'Rejected':
      return 'cancelled'
    default:
      return status
  }
}

const mapStatusNumber = (status) => {
  switch (status) {
    case 'Ordered':
      return 1
    case 'InProgress':
      return 2
    case 'Ready':
      return 3
    case 'Complete':
      return 4
    case 'Rejected':
      return 5
    default:
      return status
  }
}

export default class BarService {
  /**
   * @param {ConfigService} configService
   * @param {AuthService} authService
   * @param {object} store
   */
  constructor(configService, authService, store) {
    this.configService = configService
    this.authService = authService
    this.store = store
    this.intervalHandle = undefined
  }

  async connect(newOrderCb, updatedOrderCb, cancelledOrderCb) {
    this.newOrderCb = newOrderCb
    this.updatedOrderCb = updatedOrderCb
    this.cancelledOrderCb = cancelledOrderCb
    this._connect()
  }

  _connect() {
    if (!this.authService.isAuthenticated) {
      return
    }

    this.connection = new window.WebSocket(
      `${this.configService.queueUrl}?tk=${this.authService.credentials.authToken}`
    )

    this.connection.onopen = this._handleOpen.bind(this)
    this.connection.onclose = this._handleClose.bind(this)
    this.connection.onerror = (e) => {
      console.error(e)
      console.error('Connection attempt failed, retrying in 1 second')
      setTimeout(() => {
        this._connect()
      }, 1000)
    }

    this.connection.onmessage = (event) => {
      const message = JSON.parse(event.data)

      if (message.type === 'orderUpdated') {
        this._handleUpdatedMessage(message.data)
      }

      if (message.type === 'orderCancelled') {
        this._handleCancelledMessage(message.data)
      }

      if (message.type === 'orderCreated') {
        this._handleMessage(message.data)
      }
    }

    this.intervalHandle = setInterval(() => {
      if (!this.connection) {
        clearInterval(this.intervalHandle)
        return
      }

      console.debug('Keeping connection alive')
      this.connection.send(
        JSON.stringify({ authToken: this.authService.credentials.authToken })
      )
    }, 5000)
  }

  disconnect() {
    this.connected = false
    this.connection.close()
    this.connection = undefined
    clearInterval(this.intervalHandle)
    this.intervalHandle = undefined
  }

  _handleOpen() {
    console.log(`Connection to live event stream opened`)
    this.connected = true
  }

  _handleClose() {
    if (this.connected) {
      console.log(`Connection to live event stream lost, reconnecting`)
      clearInterval(this.intervalHandle)
      this.intervalHandle = undefined
      this._connect()
    } else {
      console.log(`Connection to live event stream closed`)
    }
  }

  _handleMessage(data) {
    if (!this.newOrderCb || !this.cancelledOrderCb) {
      return
    }

    if (!data) {
      return
    }

    data.statusName = mapStatusName(data.statusName)
    if (data.statusName === 'cancelled') {
      return this.cancelledOrderCb(data)
    }

    this.newOrderCb(data)
  }

  _handleUpdatedMessage(data) {
    if (!this.updatedOrderCb) {
      return
    }

    if (!data) {
      return
    }

    const newData = {
      orderId: data.orderId,
      orderStatus: mapStatusNumber(data.orderStatus),
    }

    this.updatedOrderCb(newData)
  }

  _handleCancelledMessage(data) {
    if (!this.cancelledOrderCb) {
      return
    }

    if (!data) {
      return
    }

    const newData = {
      orderId: data.orderId,
      orderStatus: mapStatusNumber(data.orderStatus),
    }

    this.cancelledOrderCb(newData)
  }
}

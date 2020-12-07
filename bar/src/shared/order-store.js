import { createStore, doAction } from 'fluxify'
import ApiService from '../services/api' // eslint-disable-line no-unused-vars
import BarService from '../services/bar' // eslint-disable-line no-unused-vars
import moment from 'moment'

/**
 * @param {Object} initialState
 * @param {ApiService} apiService
 * @param {BarService} barService
 */
const createOrderStore = (initialState, apiService, barService) => {
  return createStore({
    id: 'orderStore',
    initialState,
    actionCallbacks: {
      'order:startListeningForOrders': async (updater) => {
        updater.set({
          listening: true,
        })
        await barService.connect(
          (newOrder) => doAction('order:newOrder', newOrder),
          (updatedOrder) => doAction('order:updatedOrder', updatedOrder),
          (cancelledOrder) => doAction('order:cancelledOrder', cancelledOrder)
        )
      },
      'order:fetchOrders': async (updater) => {
        try {
          const orders = await apiService.fetchActiveOrders()
          updater.set({
            orders: (orders || []).sort(
              (a, b) => moment(a.orderTimeStamp) - moment(b.orderTimeStamp)
            ),
          })
        } catch (err) {
          if (err.response.status === 401 || err.response.status === 403) {
            doAction('global:invalidateSession')
          }
        }
      },
      'order:selectOrder': async (updater, orderIdx) => {
        if (orderIdx === updater.props.selectedOrder) {
          updater.set({
            selectedOrder: undefined,
            orderLoading: false,
            orderLoadFailed: false,
            order: undefined,
          })
          return
        }

        const order = updater.props.orders.find((o) => o.orderId === orderIdx)
        if (!order) {
          return
        }

        updater.set({
          selectedOrder: orderIdx,
          orderLoading: true,
          orderLoadFailed: false,
          order: undefined,
        })

        try {
          const orderDetails = await apiService.fetchOrder(order.orderId)
          updater.set({
            order: orderDetails,
            orderLoading: false,
            orderLoadFailed: false,
          })
        } catch (err) {
          if (err.response.status === 401 || err.response.status === 403) {
            doAction('global:invalidateSession')
          } else {
            updater.set({
              orderLoading: false,
              orderLoadFailed: true,
              order: undefined,
            })
          }
        }
      },
      'order:updateOrderStatus': async (updater, status) => {
        if (!updater.props.order) {
          return
        }

        let newStatus

        try {
          newStatus = await apiService.updateOrderStatus(
            updater.props.order.orderId,
            status
          )
        } catch (err) {
          if (err.response.status === 401 || err.response.status === 403) {
            doAction('global:invalidateSession')
          }

          return
        }

        const order = updater.props.order
        order.status = newStatus

        updater.set({ order })
      },
      'order:newOrder': (updater, newOrder) => {
        const orders = (updater.props.orders || []).slice(0)
        orders.push(newOrder)
        updater.set({
          orders: (orders || []).sort(
            (a, b) => moment(a.orderTimeStamp) - moment(b.orderTimeStamp)
          ),
        })
      },
      'order:updatedOrder': (updater, newOrder) => {
        const idx = updater.props.orders.findIndex(
          (order) => order.orderId === newOrder.orderId
        )
        if (idx === -1) {
          return
        }

        let orders = (updater.props.orders || []).slice(0)
        orders[idx] = Object.assign({}, orders[idx], newOrder)
        orders = orders.filter((o) => o.orderStatus < 4)

        let order = updater.props.order
        let selectedOrder = updater.props.selectedOrder
        if (order && order.orderId === newOrder.orderId) {
          if (newOrder.orderStatus >= 4) {
            selectedOrder = undefined
            order = undefined
          } else {
            order = Object.assign({}, order, newOrder)
          }
        }

        updater.set({
          orders,
          order,
          selectedOrder,
        })
      },
      'order:cancelledOrder': (updater, cancelledOrder) => {
        const cancelledIdx = updater.props.orders.findIndex(
          (o) => o.orderId === cancelledOrder.orderId
        )
        let selectedOrder = updater.props.selectedOrder
        if (cancelledIdx > -1 && selectedOrder === o.orderId) {
          selectedOrder = undefined
        }

        const orders = (updater.props.orders || [])
          .slice(0)
          .filter(
            (o) => o.orderStatus < 4 && o.orderId !== cancelledOrder.orderId
          )

        updater.set({
          orders: (orders || []).sort(
            (a, b) => moment(a.orderTimeStamp) - moment(b.orderTimeStamp)
          ),
          selectedOrder,
        })
      },
    },
  })
}

export default (initialState, apiService, barService) => {
  const state = Object.assign(
    { order: { orders: [], listening: false } },
    initialState
  )

  return {
    order: createOrderStore(state.order, apiService, barService),
  }
}

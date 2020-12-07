import { createElement as h, div, section } from 'preact-hyperscript'
import OrderCard from '../1-atoms/order-card'
import { doAction } from 'fluxify'

const OrderList = (props) => {
  return section({ className: 'section--order-list' }, [
    div(
      { className: 'orders' },
      (props.orders || []).map((order, idx) =>
        h(OrderCard, {
          order,
          key: idx,
          selected: props.selectedOrder === order.orderId,
          onClick: (e) => {
            e.preventDefault()
            doAction('order:selectOrder', order.orderId)
          },
        })
      )
    ),
  ])
}

export default OrderList

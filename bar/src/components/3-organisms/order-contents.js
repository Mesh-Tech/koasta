import { Component } from 'preact'
import { div, createElement as h } from 'preact-hyperscript'
import OrderList from '../2-molecules/order-list'
import ColumnCard from '../2-molecules/column-card'
import moment from 'moment'

const { duration } = moment

const mapStatusToName = (status) => {
  if (!status) {
    return 'Unknown'
  }

  switch (status) {
    case 1:
      return 'Not started'
    case 2:
      return 'In progress'
    case 3:
      return 'Ready to collect'
    case 4:
      return 'Collected'
    case 5:
      return 'Rejected'
    default:
      return 'Unknown'
  }
}

class OrderContents extends Component {
  render() {
    const { order } = this.props

    const summaryCard = order
      ? h(ColumnCard, {
          key: 1,
          columns: [
            { title: 'Order number', description: order.orderNumber },
            {
              title: 'Ordered at',
              description: moment(order.orderTimeStamp).format(
                'Do MMM YYYY, h:mma'
              ),
            },
            {
              title: 'Customer',
              description:
                order.customerFirstName && order.customerLastName
                  ? `${order.customerFirstName} ${order.customerLastName}`
                  : 'Anonymous',
            },
            {
              title: 'Service requested',
              description: (() => {
                switch (order.servingType) {
                  case 0:
                    return 'Bar Service'
                  case 1:
                    return 'Table Service'
                  default:
                    return 'Bar Service'
                }
              })()
            },
          ],
        })
      : undefined

    const waitTimeDuration = order
      ? duration(moment(new Date()).diff(moment(order.orderTimeStamp)))
      : undefined

    const orderCard = order
      ? h(ColumnCard, {
          key: 2,
          columns: [
            {
              title: 'Status',
              description: mapStatusToName(order.orderStatus),
            },
            { title: 'Wait time', description: waitTimeDuration.humanize() },
            {
              title: 'Notes',
              description: order.orderNotes || '-'
            }
          ],
        })
      : undefined

    const orderList = order
      ? h(OrderList, {
          key: 3,
          items: order.lineItems.map((li) => ({
            title: `${li.productName} x ${li.quantity}`,
            price: `£${li.amount.toFixed(2)}`,
          })),
        })
      : undefined

    return div({ className: 'section--order-contents' }, [
      summaryCard,
      orderCard,
      orderList,
      div({ className: 'bottom-spacer' }, String.fromCharCode(160)),
    ])
  }
}

export default OrderContents

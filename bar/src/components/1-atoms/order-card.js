import { a, div } from 'preact-hyperscript'
import { Component } from 'preact'

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
      return 'Ready'
    case 4:
      return 'Collected'
    case 5:
      return 'Rejected'
    default:
      return 'Unknown'
  }
}

class OrderCard extends Component {
  render() {
    const { onClick, order, selected } = this.props
    const el = onClick ? a : div

    const title = order ? `Order ${order.orderNumber}` : ''
    const description = order
      ? `${mapStatusToName(order.orderStatus)} · ${
          order.lineItems.length
        } item${order.lineItems.length === 1 ? '' : 's'}`
      : ''

    return el(
      {
        className: `data-card ${onClick ? '' : 'static'} ${
          selected ? 'selected' : ''
        }`,
        href: '#',
        onClick: onClick,
        ref: (el) => {
          this.$el = el
        },
      },
      [
        title || '',
        description
          ? div({ className: 'data-card-detail' }, description)
          : undefined,
      ]
    )
  }
}

export default OrderCard

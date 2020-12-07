import { li, span } from 'preact-hyperscript'
import OrderItemBadge from '../1-atoms/order-item-badge'
import { doAction } from 'fluxify'

const OrderItem = (props) => {
  const clicked = (e) => {
    doAction('order:selectOrder', props.id)
  }

  const selectedClass = props.selected ? ' active' : ''

  return li(
    {
      className: `orders--order-item${selectedClass}`,
      role: 'link',
      onClick: clicked,
    },
    [span(props.title), h(OrderItemBadge, { type: props.status })]
  )
}

export default OrderItem

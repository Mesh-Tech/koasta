import { div } from 'preact-hyperscript'

const OrderItemBadge = (props) => {
  let typeClass = ''
  let content = 'Unknown'
  if (props.type === 'ready') {
    typeClass = 'ready'
    content = 'Ordered'
  } else if (props.type === 'in-progress') {
    typeClass = 'in-progress'
    content = 'In Progress'
  } else if (props.type === 'ready-to-collect') {
    typeClass = 'in-progress'
    content = 'Ready'
  } else if (props.type === 'finished') {
    typeClass = 'finished'
    content = 'Finished'
  }

  return div({ className: `orders--order-item-badge ${typeClass}` }, content)
}

export default OrderItemBadge

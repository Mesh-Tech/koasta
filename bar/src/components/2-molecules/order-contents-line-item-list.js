import { ul, createElement as h } from 'preact-hyperscript'
import OrderContentsLineItem from './order-contents-line-item'

const OrderContentsLineItemList = (props) => {
  return ul(
    { className: 'order-contents--line-items' },
    props.lineItems.map((line) => h(OrderContentsLineItem, line))
  )
}

export default OrderContentsLineItemList

import { li, div } from 'preact-hyperscript'

const OrderContentsPlaceholder = (props) => {
  const amount = props.amount ? props.amount.toFixed(2) : 'FREE'

  return li({ className: 'order-contents--line-item' }, [
    div(
      { className: 'order-contents--line-item-title' },
      `${props.productName} x${props.quantity}`
    ),
    div({ className: 'order-contents--line-item-price' }, amount),
  ])
}

export default OrderContentsPlaceholder

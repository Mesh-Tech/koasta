import { div } from 'preact-hyperscript'

const OrderContentsPlaceholder = (props) => {
  return [
    div({ className: `order-detail--left`, key: 0 }, [
      div({ className: 'placeholder' }),
    ]),
    div({ className: `order-detail--right`, key: 1 }, [
      div(
        {
          className: `order-detail--top-right order-detail--top-right-loading`,
          key: 2,
        },
        [div({ className: 'placeholder' })]
      ),
      div({ className: `order-detail--bottom-right`, key: 3 }, [
        div({ className: 'placeholder' }),
      ]),
    ]),
  ]
}

export default OrderContentsPlaceholder

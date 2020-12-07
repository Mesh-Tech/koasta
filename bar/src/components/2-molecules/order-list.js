import { ul, li, div } from 'preact-hyperscript'
import { Component } from 'preact'

class OrderList extends Component {
  render() {
    const props = this.props

    return ul(
      { className: `order-receipt`, href: '#' },
      (props.items || []).map((i) =>
        li([
          div({ className: 'item-title' }, i.title),
          div({ className: 'item-price' }, i.price),
        ])
      )
    )
  }
}

export default OrderList

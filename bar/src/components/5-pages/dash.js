import { Component } from 'preact'
import { createElement as h, a, div } from 'preact-hyperscript'
import OrderList from '../3-organisms/order-list'
import OrderContents from '../3-organisms/order-contents'
import UserMenu from '../2-molecules/user-menu'
import { doAction } from 'fluxify'

const acceptOrder = () => {
  doAction('order:updateOrderStatus', 'in-progress')
}

const abortOrder = () => {
  doAction('order:updateOrderStatus', 'ready')
}

const collectOrder = () => {
  doAction('order:updateOrderStatus', 'ready-to-collect')
}

const completeOrder = () => {
  doAction('order:updateOrderStatus', 'finished')
}

const cancelOrder = () => {
  doAction('order:updateOrderStatus', 'cancelled')
}

class DashPage extends Component {
  componentDidMount() {
    doAction('order:fetchOrders')
    doAction('order:startListeningForOrders')
  }

  render() {
    let actionButtons
    if (this.props.order.order) {
      switch (this.props.order.order.orderStatus) {
        case 1:
          actionButtons = [
            a(
              { href: '#', className: 'data-link', onClick: cancelOrder },
              'Reject order'
            ),
            a(
              { href: '#', className: 'data-link', onClick: acceptOrder },
              'Accept order'
            ),
          ]
          break
        case 2:
          actionButtons = [
            a(
              { href: '#', className: 'data-link', onClick: abortOrder },
              'Abort order'
            ),
            a(
              { href: '#', className: 'data-link', onClick: collectOrder },
              (() => {
                switch (this.props.order.order.servingType) {
                  case 0:
                    return 'Ready to Collect'
                  case 1:
                    return 'Ready to Deliver'
                  default:
                    return 'Ready to Collect'
                }
              })()
            ),
          ]
          break
        case 3:
          actionButtons = [
            a(
              { href: '#', className: 'data-link', onClick: completeOrder },
              'Complete order'
            ),
          ]
          break
      }
    }

    return div({ className: 'page--dash' }, [
      div({ className: 'title-bar' }, [
        div({ className: 'title' }, [
          this.props.global.userDetails.venueName || '',
        ]),
        ...(actionButtons || []),
      ]),
      h(UserMenu, { global: this.props.global }),
      div({ className: 'section--orders' }, [
        h(OrderList, {
          orders: this.props.order.orders,
          selectedOrder: this.props.order.selectedOrder,
        }),
        h(OrderContents, {
          selectedOrder: this.props.order.selectedOrder,
          orderLoading: this.props.order.orderLoading,
          order: this.props.order.order,
          acceptOrder,
          abortOrder,
          completeOrder,
          cancelOrder,
          collectOrder,
        }),
      ]),
    ])
  }
}

export default DashPage

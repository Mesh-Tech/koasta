import { Component } from 'preact'
import { a, div } from 'preact-hyperscript'
import { doAction } from 'fluxify'

function translateRoleIdToName(roleId) {
  switch (roleId) {
    case 2:
      return 'Bar Staff'
    case 3:
      return 'Bar Manager'
    case 4:
      return 'Company Staff'
    case 5:
      return 'Company Manager'
    case 6:
      return 'Sysadmin'
    default:
      return 'Unknown'
  }
}

class UserMenu extends Component {
  componentWillUnmount() {
    this._removeClickListener()
  }

  render() {
    const { global } = this.props
    const { visible } = this.state || {}
    if (!global.userDetails || !global.userDetails.employeeName) {
      return div()
    }

    const nameComponents = (
      global.userDetails.employeeName || 'Anonymous'
    ).split(' ')
    const letters =
      nameComponents.length === 1
        ? nameComponents[0][0]
        : nameComponents[0][0] + nameComponents[nameComponents.length - 1][0]

    const popover = visible
      ? div(
          {
            className: 'user-menu-popover',
            key: 2,
            ref: (el) => {
              this.$menuel = el
            },
          },
          [
            div(
              { className: 'user-menu-popover__title' },
              global.userDetails.employeeName
            ),
            div(
              { className: 'user-menu-popover__subtitle' },
              translateRoleIdToName(global.userDetails.roleId)
            ),
            a(
              {
                className: 'user-menu-popover__link logout',
                href: '#',
                onClick: this.logOut.bind(this),
              },
              'Sign out'
            ),
          ]
        )
      : undefined

    return [
      a(
        {
          key: 0,
          className: 'user-menu',
          href: '#',
          onClick: this.revealMenu.bind(this),
          ref: (el) => {
            this.$el = el
          },
        },
        [letters.toUpperCase()]
      ),
      popover,
    ]
  }

  _addClickListener() {
    this.lostFocusHandler = (e) => {
      if (!(this.state || {}).visible) {
        return
      }
      if (
        e.target === this.$el ||
        e.target === this.$menuel ||
        e.target.parentNode === this.$menuel
      ) {
        return
      }
      this.setState({ visible: false })
    }
    document.body.addEventListener('click', this.lostFocusHandler)
  }

  _removeClickListener() {
    if (!this.lostFocusHandler) {
      return
    }
    document.body.removeEventListener('click', this.lostFocusHandler)
    this.lostFocusHandler = undefined
  }

  revealMenu(e) {
    e.preventDefault()
    e.stopPropagation()

    const visible = !(this.state || {}).visible

    if (visible) {
      this._addClickListener()
    } else {
      this._removeClickListener()
    }

    this.setState({ visible })
    return false
  }

  goToProfile(e) {
    e.preventDefault()
    const { global } = this.props
    if (!global.userDetails || !global.userDetails.employeeId) {
      return
    }

    doAction(
      'global:navigateToPage',
      `/employees/${global.userDetails.employeeId}`
    )
  }

  logOut(e) {
    e.preventDefault()
    doAction('global:invalidateSession')
  }
}

export default UserMenu

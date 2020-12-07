import { createStore } from 'fluxify'
import AuthService from '../services/auth' // eslint-disable-line no-unused-vars
import BarService from '../services/bar' // eslint-disable-line no-unused-vars

/**
 * @param {Object} initialState
 * @param {AuthService} authService
 * @param {BarService} barService
 * @param {History} history
 */
const createGlobalStore = (initialState, authService, barService, history) => {
  return createStore({
    id: 'globalStore',
    initialState,
    actionCallbacks: {
      navigatePage: (updater, requestPath) => {
        updater.set({
          requestPath,
        })
      },
      'global:refreshAuthState': (updater) => {
        updater.set({
          isLoggedIn: authService.isAuthenticated,
          userDetails: authService.userDetails,
        })
      },
      'global:invalidateSession': (updater) => {
        authService.invalidate()
        barService.disconnect()
        updater.set({
          isLoggedIn: false,
          userDetails: undefined,
        })
        history.push('/login')
      },
    },
  })
}

/**
 * @param {Object} initialState
 * @param {AuthService} authService
 */
export default (initialState, authService, barService, history) => {
  const state = Object.assign(
    {
      global: {
        isLoggedIn: authService.isAuthenticated,
        userDetails: authService.userDetails,
      },
    },
    initialState
  )

  return {
    global: createGlobalStore(state.global, authService, barService, history),
  }
}

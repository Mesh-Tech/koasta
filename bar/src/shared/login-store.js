import { createStore, doAction } from 'fluxify'
import AuthService from '../services/auth' // eslint-disable-line no-unused-vars

const evaluateFormValidation = (username, password) => {
  return username && password && username.length && password.length
}

/**
 * @param {Object} initialState
 * @param {AuthService} authService
 */
const createLoginStore = (initialState, authService, history) => {
  return createStore({
    id: 'loginStore',
    initialState,
    actionCallbacks: {
      'login:doLogin': async (updater) => {
        try {
          await authService.authenticate(
            updater.props.username,
            updater.props.password
          )
          if (authService.isAuthenticated) {
            doAction('global:refreshAuthState')
            history.replace('/')
          }
        } catch (err) {
          console.error(err)
        }
      },
      'login:loginForm:usernameChanged': (updater, username) => {
        updater.set({
          username,
          usernameValidation:
            username && username.length
              ? undefined
              : 'Please enter your username',
          formValid: evaluateFormValidation(username, updater.props.password),
        })
      },
      'login:loginForm:passwordChanged': (updater, password) => {
        updater.set({
          password,
          passwordValidation:
            password && password.length
              ? undefined
              : 'Please enter your password',
          formValid: evaluateFormValidation(updater.props.username, password),
        })
      },
    },
  })
}

export default (initialState, authService, history) => {
  const state = Object.assign({ login: { formValid: false } }, initialState)

  return {
    login: createLoginStore(state.login, authService, history),
  }
}

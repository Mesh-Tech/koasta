import Bottle from 'bottlejs'
import ConfigService from '../services/config'
import StorageService from '../services/storage'
import ApiService from '../services/api'
import AuthService from '../services/auth'
import BarService from '../services/bar'
import history from './history'

const container = new Bottle()

import createGlobalStore from './store'
import createLoginStore from './login-store'
import createOrderStore from './order-store'

container.service('ConfigService', ConfigService)
container.service('StorageService', StorageService)
container.service('ApiService', ApiService, 'ConfigService', 'StorageService')
container.service('AuthService', AuthService, 'ApiService', 'StorageService')
container.service('BarService', BarService, 'ConfigService', 'AuthService')
container.provider(
  'History',
  class HistoryProvider {
    $get() {
      return history
    }
  }
)

container.provider(
  'GlobalStore',
  class GlobalStoreProvider {
    $get(container) {
      return createGlobalStore(
        {},
        container.AuthService,
        container.BarService,
        container.History
      )
    }
  }
)

container.provider(
  'LoginStore',
  class LoginStoreProvider {
    $get(container) {
      return createLoginStore({}, container.AuthService, container.History)
    }
  }
)
container.provider(
  'OrderStore',
  class OrderStoreProvider {
    $get(container) {
      return createOrderStore({}, container.ApiService, container.BarService)
    }
  }
)

window.container = container.container
export default container.container

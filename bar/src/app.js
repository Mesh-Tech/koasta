import { render } from 'preact'
import { createElement as h } from "preact-hyperscript"
import { doAction } from 'fluxify'
import UniversalRouter from 'universal-router'
import container from './shared/container'
import LoginPage from './components/5-pages/login'
import DashPage from './components/5-pages/dash'
import './styles/styles.scss'

const { GlobalStore, LoginStore, OrderStore, History, AuthService } = container

const store = Object.assign(
  {},
  GlobalStore,
  LoginStore,
  OrderStore
)
const history = History

const authenticatedRoute = (elem) => {
  return AuthService.isAuthenticated ? elem : { redirect: '/login' }
}

const routes = [
  { path: '', action: () => authenticatedRoute(DashPage) },
  { path: '/', action: () => authenticatedRoute(DashPage) },
  { path: '/index.html', action: () => authenticatedRoute(DashPage) },
  { path: '/login', action: () => LoginPage },
  { path: '(.*)', action: () => authenticatedRoute(DashPage) }
]

const router = new UniversalRouter(routes)

const rerender = async (pathname) => {
  const page = await router.resolve(pathname || '/')
  if (page.redirect) {
    window.location = page.redirect
    return
  }

  render(
    h(page, {router, global: store.global, login: store.login, order: store.order}),
    document.getElementById('approot')
  )
}

(async () => {
  store.global.on('change', () => rerender(history.location.pathname))
  store.login.on('change', () => rerender(history.location.pathname))
  store.order.on('change', () => rerender(history.location.pathname))
  history.listen(async (context) => {
    if (!AuthService.isAuthenticated) {
      await doAction('navigatePage', '/login')
      await rerender('/login')
    } else {
      await doAction("navigatePage", context.location.pathname);
      await rerender(context.location.pathname);
    }
  })

  await rerender(history.location.pathname)
})()

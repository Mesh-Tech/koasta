import {
  createElement as h,
  section,
  fieldset,
  form,
  button,
  input,
  div,
  a,
  br,
} from 'preact-hyperscript'
import { doAction } from 'fluxify'

const usernameChanged = (e) => {
  e.preventDefault()
  doAction('login:loginForm:usernameChanged', e.target.value)
}

const passwordChanged = (e) => {
  e.preventDefault()
  doAction('login:loginForm:passwordChanged', e.target.value)
}

const doLogin = (e) => {
  e.preventDefault()
  doAction('login:doLogin')
}

const LoginPage = (props) => {
  const {
    formValid,
    usernameValidation,
    passwordValidation,
    loggingIn,
    loginFailed,
  } = props.login
  const usernameValidationClass = usernameValidation ? 'error' : ''
  const passwordValidationClass = passwordValidation ? 'error' : ''
  const disabled = loggingIn

  const loginError = div(
    { className: `login-error ${loginFailed && !loggingIn ? 'visible' : ''}` },
    div({ className: 'login-error__content' }, [
      'We were unable to verify your credentials.',
      br(),
      'Please try again later, or ',
      a({ href: 'mailto://support@koasta.com' }, 'contact support'),
      '.',
    ])
  )

  return section({ className: 'page--login' }, [
    div({ className: 'login-brand' }),
    loginError,
    form({ className: 'login-panel', onSubmit: doLogin }, [
      fieldset({ className: 'aligned' }, [
        input({
          id: 'login_username',
          name: 'username',
          placeholder: 'Username',
          type: 'text',
          className: `inset ${usernameValidationClass}`,
          onChange: usernameChanged,
          onKeyUp: usernameChanged,
          noValidate: true,
          disabled,
        }),
      ]),
      fieldset({ className: 'aligned' }, [
        input({
          id: 'login_password',
          name: 'password',
          placeholder: 'Password',
          type: 'password',
          className: `inset ${passwordValidationClass}`,
          onChange: passwordChanged,
          onKeyUp: passwordChanged,
          noValidate: true,
          disabled,
        }),
      ]),
      button(
        {
          type: 'submit',
          className: `button secondary ${loggingIn ? 'loading' : ''}`,
          disabled: !formValid || disabled,
        },
        'Log in'
      ),
    ]),
  ])
}

export default LoginPage

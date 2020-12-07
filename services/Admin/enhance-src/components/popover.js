import { h } from "maquette"

export default function Popover(props, children) {
  const { hidden, onclick, loading } = props

  return h('div', { class: `tag-popover ${hidden ? 'hidden' : 'visible'} ${loading ? 'loading' : ''}`, onclick, tabIndex: -1 }, [...children])
}

import { createProjector, h } from "maquette"
import Popover from './popover'

function load(initialState, setState) {
  let state = initialState
  setState({disabled: true})

  fetch("/tags/suggestions").then(res => res.json()).then(tags => setState({ availableTags: tags || [], loadingTags: false }))
}

function TagOption(props) {
  const { tag, onSelect, selected } = props

  const onclick = function(e) {
    e.stopPropagation();
    e.preventDefault()
    onSelect()
  }

  const onkeyup = function(e) {
    if (event.which == 13 || event.keyCode == 13) {
      onclick(e)
      return false
    }

    return true
  }

  return h("li", { class: `tag-option ${selected ? 'selected' : ''}`, key: tag, onclick, onkeyup, tabIndex: 0 }, [tag])
}

function TagPill(props) {
  const {key, text, onDelete} = props
  const onClick = function(e) {
    e.preventDefault()
    onDelete()
  }

  return h("a", { key, class: "tag-pill", href: "#", onclick: onClick }, [
    h("span", {}, [text || ""]),
    h("div", { class: "delete-button" }),
  ]);
}

function TagInput(state) {
  const {id, className, value, placeholder, name, type, disabled, tags, availableTags, setState, popoverHidden, loadingTags} = state

  const pills = tags
    .map(tag => TagPill({
      key: tag,
      text: tag,
      onDelete: () => {
        const newTags = tags.filter((t) => t !== tag);
        setState({
          tags: newTags,
          value: newTags.join(',')
        })
      }
    }))

  const toggleActive = (e) => {
    e.preventDefault()
    setState({ popoverHidden: !popoverHidden })
  }

  const onAddKeyup = function (e) {
    if (event.which == 13 || event.keyCode == 13) {
      toggleActive(e);
      return false;
    }

    return true;
  };

  const toggleTag = (tag) => () => {
    const newTags = tags.includes(tag) ? tags.filter(t => t !== tag) : [...tags, tag]
    setState({
      tags: newTags,
      value: newTags.join(","),
    });
  }

  const doNothing = (e) => {
    e.stopPropagation()
    e.preventDefault()
  }

  const popoverContent = loadingTags || popoverHidden
    ? [h("div", { class: "loading-spinner" })]
    : [h("ul", { class: "tag-options", tabIndex: -1 }, (availableTags || []).map((tag) => TagOption({ tag, onSelect: toggleTag(tag), selected: tags.includes(tag) })))]

  return h("div", { class: `tag-input ${disabled ? "disabled" : ""}` }, [
    h("input", { id, [`class`]: className, value, placeholder, name, type, hidden: true }),
    h("div", { class: `add-tag-button ${popoverHidden ? '' : 'active'}`, onclick: toggleActive, onkeyup: onAddKeyup, tabIndex: 0 }, [
      Popover({ hidden: popoverHidden, onclick: doNothing, loading: loadingTags }, popoverContent)
    ]),
    ...pills
  ]);
}

export default function($el) {
  var projector = createProjector()
  var state = {
    id: $el.id,
    className: $el.className,
    value: $el.value,
    placeholder: $el.placeholder,
    name: $el.name,
    type: $el.type,
    tags: ($el.value || "")
      .split(",")
      .map((tag) => tag.trim())
      .filter((tag) => tag),
    popoverHidden: true,
    loadingTags: true
  };
  var setState

  function render() {
    return TagInput({
      ...state,
      setState
    })
  }

  setState = function(change) {
    state = {...state, ...change}
    projector.scheduleRender()
  }

  projector.replace($el, render)
  load(state, setState)
}

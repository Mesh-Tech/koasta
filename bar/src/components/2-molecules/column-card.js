import { div } from 'preact-hyperscript'

const ColumnCard = (props) => {
  const columns = props.columns || []
  if (!columns.length) {
    return div()
  }

  let titleBar
  if (props.title) {
    titleBar = div(
      { className: 'column-card__title-bar', key: -1 },
      props.title
    )
  }

  const items = columns
    .filter((c) => c)
    .map((column, key) =>
      div({ className: `column-card__column`, key }, [
        div({ className: `column-card__column-title` }, column.title),
        div(
          { className: `column-card__column-description` },
          column.description
        ),
      ])
    )

  return div(
    { className: `column-card ${titleBar ? 'with-title' : ''}` },
    [].concat([titleBar], items)
  )
}

export default ColumnCard

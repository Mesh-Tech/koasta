import { h4, p, div } from 'preact-hyperscript'

const InfoCard = (props) => {
  let extraClass = props.status || ''

  return div({ className: `info-card ${extraClass}` }, [
    h4({ className: 'title' }, props.title),
    p({ className: 'description' }, props.description),
  ])
}

export default InfoCard

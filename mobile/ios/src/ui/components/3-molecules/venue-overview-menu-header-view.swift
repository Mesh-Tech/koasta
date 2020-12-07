import Foundation
import UIKit
import Kingfisher

class VenueOverviewMenuHeaderView: UITableViewHeaderFooterView {
  fileprivate static var sizingItem = VenueOverviewMenuHeaderView()
  fileprivate let title = UILabel()

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  override init(reuseIdentifier: String?) {
    super.init(reuseIdentifier: reuseIdentifier)
    setup()
  }

  fileprivate convenience init() { self.init(reuseIdentifier: nil) }

  fileprivate func setup () {
    contentView.backgroundColor = UIColor.grey7

    title.numberOfLines = 1
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.font = UIFont.boldBrandFont(ofSize: 14)
    title.textColor = UIColor.grey10
    title.text = NSLocalizedString("Menu", comment: "Screen: [Venue Overview] Element: [Menu list header] Scenario: [Venue overview loaded]")

    addSubview(title)

    title.setNeedsLayout()
    setNeedsLayout()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)

    title.preferredMaxLayoutWidth = containerWidth

    let titleSize = title.intrinsicContentSize

    title.frame = CGRect(x: 16, y: 20, width: titleSize.width, height: titleSize.height)
  }

  static func calculateHeight () -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)
    let sizingView = sizingItem

    sizingView.title.preferredMaxLayoutWidth = containerWidth

    sizingView.layoutSubviews()

    let titleHeight = sizingView.title.frame.size.height

    return 20 + titleHeight + 12
  }
}

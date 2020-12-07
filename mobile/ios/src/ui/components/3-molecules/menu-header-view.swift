import Foundation
import UIKit
import Cartography

class MenuHeader: UITableViewHeaderFooterView {
  fileprivate static var sizingHeader = MenuHeader()
  fileprivate var title: H3!
  fileprivate var body: P!

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
    contentView.backgroundColor = UIColor.grey9
    title = H3()
    body = P()

    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    body.numberOfLines = 0
    body.lineBreakMode = .byWordWrapping

    [title, body].forEach { contentView.addSubview($0) }

    constrain(contentView, title, body) { container, title, body in
      title.top == container.top + 16
      title.left == container.left + 16
      title.right == container.right - 16

      body.top == title.bottom + 10
      body.left == container.left + 16
      body.right == container.right - 16
      body.bottom <= container.bottom - 16
    }
  }

  static func calculateHeight (for menu: VenueMenu) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - 32
    let sizingView = sizingHeader.configure(with: menu)
    sizingView.title.preferredMaxLayoutWidth = containerWidth
    sizingView.body.preferredMaxLayoutWidth = containerWidth

    let titleHeight = sizingView.title.intrinsicContentSize.height
    let bodyHeight = sizingView.body.intrinsicContentSize.height

    return 16 + titleHeight + 10 + bodyHeight + 16
  }

  func configure (with menu: VenueMenu) -> MenuHeader {
    title.text = menu.menuName
    body.text = menu.menuDescription

    return self
  }
}

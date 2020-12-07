import Foundation
import UIKit
import Kingfisher

class HistoricalOrderLoadingItemView: UITableViewCell {
  fileprivate static var sizingItem = HistoricalOrderLoadingItemView()
  fileprivate let thumb = UIView()
  fileprivate let title = UIView()
  fileprivate let subtitle = UIView()
  fileprivate let comment = UIView()

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
    super.init(style: style, reuseIdentifier: reuseIdentifier)
    setup()
  }

  convenience init(reuseIdentifier: String?) {
    self.init(style: .default, reuseIdentifier: reuseIdentifier)
    setup()
  }

  fileprivate convenience init() { self.init(reuseIdentifier: nil) }

  fileprivate func setup () {
    contentView.backgroundColor = UIColor.backgroundColour

    thumb.backgroundColor = UIColor.grey6
    thumb.layer.cornerRadius = 10
    thumb.contentMode = .scaleAspectFill
    thumb.clipsToBounds = true

    title.backgroundColor = UIColor.grey6
    title.layer.cornerRadius = 3
    title.contentMode = .scaleAspectFill
    title.clipsToBounds = true

    subtitle.backgroundColor = UIColor.grey6
    subtitle.layer.cornerRadius = 3
    subtitle.contentMode = .scaleAspectFill
    subtitle.clipsToBounds = true

    comment.backgroundColor = UIColor.grey6
    comment.layer.cornerRadius = 3
    comment.contentMode = .scaleAspectFill
    comment.clipsToBounds = true

    addSubview(thumb)
    addSubview(title)
    addSubview(subtitle)
    addSubview(comment)
  }

  func configure () -> HistoricalOrderLoadingItemView {
    setNeedsLayout()
    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)

    thumb.frame = CGRect(x: 16, y: 16, width: UIScreen.main.bounds.width - (16 * 2), height: 168)

    title.frame = CGRect(x: 16, y: thumb.frame.origin.y + thumb.frame.size.height + 10, width: containerWidth * 0.15, height: 20)
    subtitle.frame = CGRect(x: 16, y: title.frame.origin.y + title.frame.height + 6, width: containerWidth * 0.50, height: 15)
    comment.frame = CGRect(x: 16, y: subtitle.frame.origin.y + subtitle.frame.height + 6, width: containerWidth * 0.30, height: 14)
  }

  static func calculateHeight () -> CGFloat {
    let sizingView = sizingItem.configure()

    sizingView.layoutSubviews()

    let thumbHeight = sizingView.thumb.frame.size.height
    let titleHeight = sizingView.title.frame.size.height
    let subtitleHeight = sizingView.subtitle.frame.size.height
    let commentHeight = sizingView.comment.frame.size.height

    return 16 + thumbHeight +  10 + titleHeight + 6 + subtitleHeight + 6 + commentHeight + 16
  }
}

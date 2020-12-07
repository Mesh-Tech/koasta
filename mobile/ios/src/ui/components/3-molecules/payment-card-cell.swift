import Foundation
import UIKit

class PaymentCardCell: UITableViewCell {
  fileprivate let label = UILabel()
  fileprivate let thumb = UIImageView()

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
    contentView.addSubview(label)
    contentView.addSubview(thumb)

    label.font = UIFont.brandFont(ofSize: 16, weight: .regular)
    thumb.contentMode = .scaleAspectFit

    setNeedsLayout()
  }

  func configure(text: String, image: UIImage?) -> PaymentCardCell {
    label.text = text
    thumb.image = image
    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)

    label.preferredMaxLayoutWidth = containerWidth - 30

    let labelSize = label.intrinsicContentSize
    label.frame = CGRect(x: 46, y: 12, width: labelSize.width, height: labelSize.height)

    thumb.frame = CGRect(x: 16, y: 12, width: 20, height: 20)
  }
}

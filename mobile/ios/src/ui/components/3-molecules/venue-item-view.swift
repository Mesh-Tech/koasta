import Foundation
import UIKit
import Kingfisher
import CoreLocation

class VenueItemView: UITableViewCell {
  fileprivate static var sizingItem = VenueItemView()
  fileprivate let thumb = UIImageView()
  fileprivate let thumbOverlay = UIView()
  fileprivate let title = UILabel()
  fileprivate let locationIcon = UIImageView()
  fileprivate let subtitle = UILabel()
  fileprivate let comment = UILabel()

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
    thumb.backgroundColor = UIColor.grey6
    thumb.layer.cornerRadius = 10
    thumb.contentMode = .scaleAspectFill
    thumb.clipsToBounds = true

    title.numberOfLines = 0
    title.lineBreakMode = .byWordWrapping
    title.setContentHuggingPriority(.defaultLow, for: .horizontal)
    title.setContentHuggingPriority(.defaultHigh, for: .vertical)
    title.font = UIFont.preferredBrandFont(forTextStyle: UIFont.TextStyle.headline)
    title.accessibilityIdentifier = "venueTitle"

    subtitle.numberOfLines = 0
    subtitle.lineBreakMode = .byWordWrapping
    subtitle.setContentHuggingPriority(.defaultLow, for: .horizontal)
    subtitle.setContentHuggingPriority(.defaultHigh, for: .vertical)
    subtitle.font = UIFont.boldBrandFont(ofSize: 15)

    comment.numberOfLines = 1
    comment.lineBreakMode = .byClipping
    comment.setContentHuggingPriority(.defaultHigh, for: .horizontal)
    comment.setContentHuggingPriority(.defaultHigh, for: .vertical)
    comment.font = UIFont.boldBrandFont(ofSize: 14)
    comment.textColor = UIColor.grey8

    thumbOverlay.backgroundColor = .charcoalColour
    thumbOverlay.alpha = 0.6
    thumbOverlay.layer.cornerRadius = 10

    locationIcon.image = UIImage(named: "nearby-pin")
    locationIcon.tintColor = UIColor.grey8

    addSubview(thumb)
    addSubview(thumbOverlay)
    addSubview(title)
    addSubview(locationIcon)
    addSubview(subtitle)
    addSubview(comment)
  }

  func configure (with venue: Venue, location: CLLocation?) -> VenueItemView {
    return configure(with: venue, location: location, shouldLoadImageNow: true)
  }

  fileprivate func configure (with venue: Venue, location: CLLocation?, shouldLoadImageNow: Bool) -> VenueItemView {
    title.text = venue.venueName
    comment.text = venue.isOpen ? "" : NSLocalizedString("Opening later", comment: "")

    if let location = location {
      let distance = "\(venue.evaluateDistanceMetersFromLocation(location))".split(separator: ".")[0]
      subtitle.text = NSLocalizedString("\(distance)m", comment: "")
    } else {
      subtitle.text = venue.venuePostCode
    }

    if shouldLoadImageNow, let imageAddress = venue.imageUrl?.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed), imageAddress.count > 0, let imageUrl = URL(string: imageAddress) {
      let resize = ResizingImageProcessor(referenceSize: CGSize(width: UIScreen.main.bounds.width - 66, height: 200), mode: .aspectFill)

      thumb.kf.setImage(with: imageUrl, placeholder: nil, options: [ .processor(resize) ])
    } else {
      thumb.image = nil
    }

    title.setNeedsLayout()
    subtitle.setNeedsLayout()
    comment.setNeedsLayout()
    thumbOverlay.isHidden = venue.isOpen
    setNeedsLayout()

    return self
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)

    title.preferredMaxLayoutWidth = containerWidth
    subtitle.preferredMaxLayoutWidth = containerWidth
    comment.preferredMaxLayoutWidth = containerWidth - 100

    let titleSize = title.intrinsicContentSize
    let subtitleSize = subtitle.intrinsicContentSize
    let commentSize = comment.intrinsicContentSize

    thumb.frame = CGRect(x: 16, y: 16, width: UIScreen.main.bounds.width - (16 * 2), height: 168)
    thumbOverlay.frame = thumb.frame

    title.frame = CGRect(x: 16, y: thumb.frame.origin.y + thumb.frame.size.height + 10, width: titleSize.width, height: titleSize.height)
    locationIcon.frame = CGRect(x: 16, y: title.frame.origin.y + titleSize.height + 6, width: 7, height: 9)
    subtitle.frame = CGRect(x: locationIcon.frame.maxX + 10, y: locationIcon.frame.minY - 5, width: subtitleSize.width, height: subtitleSize.height)
    comment.frame = CGRect(x: 16, y: subtitle.frame.origin.y + subtitleSize.height + 6, width: commentSize.width, height: commentSize.height)
  }

  static func calculateHeight (for venue: Venue, location: CLLocation?) -> CGFloat {
    let containerWidth = UIScreen.main.bounds.width - (16 * 2)
    let sizingView = sizingItem.configure(with: venue, location: location, shouldLoadImageNow: false)

    sizingView.title.preferredMaxLayoutWidth = containerWidth
    sizingView.subtitle.preferredMaxLayoutWidth = containerWidth
    sizingView.comment.preferredMaxLayoutWidth = containerWidth - 100

    sizingView.layoutSubviews()

    let thumbHeight = sizingView.thumb.frame.size.height
    let titleHeight = sizingView.title.frame.size.height
    let subtitleHeight = sizingView.subtitle.frame.size.height
    let commentHeight = sizingView.comment.frame.size.height

    return 16 + thumbHeight +  10 + titleHeight + 6 + subtitleHeight + 6 + commentHeight + 16
  }
}

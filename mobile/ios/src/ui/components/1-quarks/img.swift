import UIKit
import Kingfisher

class IMG: UIView {
  fileprivate let imageView = UIImageView()

  convenience init () {
    self.init(frame: CGRect.zero)
  }

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  fileprivate func setup() {
    imageView.contentMode = .scaleAspectFill
    imageView.backgroundColor = UIColor.clear
    imageView.isOpaque = false
    imageView.clipsToBounds = true
    imageView.alpha = 0

    addSubview(imageView)
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    imageView.frame = bounds
  }

  var image: UIImage? {
    get {
      return imageView.image
    }
  }

  func loadImage(url: String?) {
    guard let urlString = url, let url = URL(string: urlString) else { return }

    imageView.kf.cancelDownloadTask()
    imageView.kf.setImage(
      with: url,
      options: [KingfisherOptionsInfoItem.transition(.fade(0.2))]
    ) { [weak self] result in
      switch result {
      case .success:
        UIView.animate(withDuration: 0.2) { [weak self] in
          self?.imageView.alpha = 1
        }
      case .failure:
        UIView.animate(withDuration: 0.2) { [weak self] in
          self?.imageView.alpha = 0
        }
      }
    }
  }
}

import Foundation
import UIKit

class H4: UILabel {
  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  private func setup () {
    numberOfLines = 0
  }

  var lineHeight: CGFloat = 1.2 {
    didSet { attributedText = buildAttributedText(text: text) }
  }

  override var accessibilityValue: String? {
    get {
      return text
    }
    set {}
  }

  override var text: String? {
    get { return attributedText?.string }
    set { attributedText = buildAttributedText(text: newValue) }
  }

  fileprivate func buildAttributedText (text: String?) -> NSAttributedString? {
    guard let text = text else { return nil }

    let paragraphStyle = NSMutableParagraphStyle()
    paragraphStyle.lineSpacing = font.pointSize * max((lineHeight - 1), 0)

    let string = NSMutableAttributedString(string: text)
    string.setAttributes([
      NSAttributedString.Key.paragraphStyle: paragraphStyle
    ], range: NSRange(location: 0, length: text.count))

    return string
  }
}

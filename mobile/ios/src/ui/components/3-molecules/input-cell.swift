import Foundation
import UIKit

@objc protocol InputCellDelegate {
  @objc optional func textChanged(to text: String?, for inputCell: InputCell)
}

class InputCell: UITableViewCell, UITextFieldDelegate {
  weak var delegate: InputCellDelegate?
  fileprivate let field = UITextField()

  override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
    super.init(style: style, reuseIdentifier: reuseIdentifier)
    setup()
  }

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  override func prepareForReuse() {
    super.prepareForReuse()
    textLabel?.text = nil
    delegate = nil
    field.text = nil
    setNeedsLayout()
  }

  func setup () {
    textLabel?.font = UIFont.boldBrandFont(ofSize: 14)
    field.font = UIFont.brandFont(ofSize: 17)
    field.borderStyle = .none
    field.delegate = self
    field.isOpaque = false
    field.backgroundColor = .clear

    layoutMargins = UIEdgeInsets.zero
    separatorInset = UIEdgeInsets(top: 0, left: 120, bottom: 0, right: 0)

    contentView.addSubview(field)
    setNeedsLayout()
    contentView.setNeedsLayout()

    selectionStyle = .none
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    textLabel?.frame = CGRect(x: 10, y: 10, width: 100, height: bounds.height - 20)
    field.frame = CGRect(x: 120, y: 10, width: bounds.width - 130, height: bounds.height - 20)
  }

  func focus() {
    field.becomeFirstResponder()
  }

  func textField(_ textField: UITextField, shouldChangeCharactersIn range: NSRange, replacementString string: String) -> Bool {
    if let text = textField.text as NSString? {
      let newValue = text.replacingCharacters(in: range, with: string)

      delegate?.textChanged?(to: newValue, for: self)
    } else {
      delegate?.textChanged?(to: nil, for: self)
    }
    return true
  }

  var value: String? {
    get {
      return field.text
    }
    set {
      field.text = newValue
    }
  }

  var keyboardType: UIKeyboardType {
    get {
      return field.keyboardType
    }
    set {
      field.keyboardType = newValue
    }
  }
}

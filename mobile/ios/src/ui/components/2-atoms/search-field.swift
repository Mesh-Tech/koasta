import UIKit

@objc protocol SearchFieldEditingDelegate {
  @objc optional func textChanged(to: String?, for field: SearchField)
  @objc optional func gotFocus(for field: SearchField)
}

class SearchField: UIControl, UITextFieldDelegate {
  fileprivate struct C {
    static let paddingWidth: CGFloat = 20
    static let paddingHeight: CGFloat = 10
  }

  var autocorrectionType: UITextAutocorrectionType {
    get { return field.autocorrectionType }
    set { field.autocorrectionType = newValue }
  }

  var autocapitalizationType: UITextAutocapitalizationType {
    get { return field.autocapitalizationType }
    set { field.autocapitalizationType = newValue }
  }

  var textColor: UIColor? {
    get { return field.textColor }
    set { field.textColor = newValue }
  }

  override var accessibilityIdentifier: String? {
    get { return super.accessibilityIdentifier }
    set { field.accessibilityIdentifier = newValue }
  }

  var text: String? {
    get { return field.text }
    set {
      field.text = newValue
      setNeedsLayout()
      invalidateIntrinsicContentSize()
    }
  }

  var placeholder: String? {
    get { return field.placeholder }
    set {
      field.attributedPlaceholder = NSAttributedString(string: newValue ?? "", attributes: [
        NSAttributedString.Key.foregroundColor: UIColor.grey3
      ])
      setNeedsLayout()
      invalidateIntrinsicContentSize()
    }
  }

  var keyboardType: UIKeyboardType {
    get { return field.keyboardType }
    set { field.keyboardType = newValue }
  }

  var keyboardAppearance: UIKeyboardAppearance {
    get { return field.keyboardAppearance }
    set { field.keyboardAppearance = newValue }
  }

  var returnKeyType: UIReturnKeyType {
    get { return field.returnKeyType }
    set { field.returnKeyType = newValue }
  }

  var maximumLength: Int = -1

  weak var editingDelegate: SearchFieldEditingDelegate?

  fileprivate let field = UITextField()

  fileprivate let dot = UIImageView()

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

    backgroundColor = .backgroundColour
    layer.cornerRadius = 5
    layer.masksToBounds = true

    returnKeyType = .done

    textColor = .foregroundColour

    field.textAlignment = .natural
    field.isUserInteractionEnabled = true
    field.isEnabled = true
    field.delegate = self
    field.font = UIFont.brandFont(ofSize: 14)
    field.tintColor = .brandColour
    field.autocorrectionType = .no
    field.autocapitalizationType = .words
    isUserInteractionEnabled = true
    isEnabled = true

    dot.image = UIImage(named: "searchbar")

    addSubview(field)
    addSubview(dot)
    setNeedsLayout()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    dot.frame = CGRect(x: 10, y: 10, width: 12, height: 12)
    field.frame = CGRect(x: 20 + C.paddingWidth / 2, y: 2, width: bounds.width - C.paddingWidth, height: bounds.height - 2)
  }

  fileprivate func calculateFieldSize(boundingBox: CGRect) -> CGSize {
    let constrainedSize = CGSize(width: boundingBox.size.width - (C.paddingWidth * 2), height: CGFloat.greatestFiniteMagnitude)
    return field.systemLayoutSizeFitting(constrainedSize, withHorizontalFittingPriority: UILayoutPriority.required, verticalFittingPriority: UILayoutPriority.defaultLow)
  }

  override var intrinsicContentSize: CGSize {
    let labelSize = calculateFieldSize(boundingBox: UIScreen.main.bounds)
    let idealWidth = C.paddingWidth + labelSize.width + C.paddingWidth
    let idealHeight = C.paddingHeight + C.paddingHeight + C.paddingHeight

    return CGSize(width: max(idealWidth, 240), height: idealHeight)
  }

  override var isEnabled: Bool {
    didSet {
      self.alpha = isEnabled ? 1 : 0.3
      field.isEnabled = isEnabled

      if !isEnabled {
        field.resignFirstResponder()
      }
    }
  }

  override func becomeFirstResponder() -> Bool {
    return field.becomeFirstResponder()
  }

  override func resignFirstResponder() -> Bool {
    return field.resignFirstResponder()
  }

  func textFieldDidBeginEditing(_ textField: UITextField) {
    editingDelegate?.gotFocus?(for: self)
  }

  override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
    super.touchesEnded(touches, with: event)

    guard let _ = touches.first(where: { touch in
      return self.point(inside: touch.location(in: self), with: event)
    }) else {
      return
    }

    if !isEnabled {
      return
    }

    guard let loc = touches.first?.location(in: self) else { return }
    guard bounds.contains(loc) else { return }

    _ = becomeFirstResponder()
  }

  func setEnabled(_ enabled: Bool, animated: Bool) {
    UIView.animate(withDuration: 0.2, delay: 0, options: [.beginFromCurrentState, .curveEaseOut], animations: { [weak self] in
      self?.isEnabled = enabled
      }, completion: nil)
  }

  func textField(_ textField: UITextField, shouldChangeCharactersIn range: NSRange, replacementString string: String) -> Bool {
    if let text = textField.text as NSString? {
      let newValue = text.replacingCharacters(in: range, with: string)

      if maximumLength > -1 && newValue.count > maximumLength {
        return false
      }

      editingDelegate?.textChanged?(to: newValue, for: self)
    } else {
      editingDelegate?.textChanged?(to: nil, for: self)
    }
    return true
  }

  func textFieldShouldReturn(_ textField: UITextField) -> Bool {
    _ = resignFirstResponder()
    return true
  }
}

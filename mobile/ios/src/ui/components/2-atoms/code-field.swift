import Foundation
import UIKit
import QuartzCore

class CodeField: UIView {
  override init(frame: CGRect) {super.init(frame: frame)}
  required init?(coder aDecoder: NSCoder) {super.init(coder: aDecoder)}
  fileprivate let slot1 = UIView()
  fileprivate let slot2 = UIView()
  fileprivate let slot3 = UIView()
  fileprivate let slot4 = UIView()
  fileprivate let slot5 = UIView()
  fileprivate let slot6 = UIView()
  fileprivate let slot1Label = UILabel()
  fileprivate let slot2Label = UILabel()
  fileprivate let slot3Label = UILabel()
  fileprivate let slot4Label = UILabel()
  fileprivate let slot5Label = UILabel()
  fileprivate let slot6Label = UILabel()
  fileprivate let caret1 = Caret()
  fileprivate let caret2 = Caret()
  fileprivate let caret3 = Caret()
  fileprivate let caret4 = Caret()
  fileprivate let caret5 = Caret()
  fileprivate let caret6 = Caret()

  var caretIndex = 0 {
    didSet {
      updateCaretPosition()
    }
  }

  fileprivate struct C {
    static let slotSize: CGFloat = 44
    static let slotBackgroundColour = UIColor.grey1
    static let slotCornerRadius: CGFloat = 5
    static let labelFont = UIFont.brandFont(ofSize: 20, weight: UIFont.Weight.medium)
    static let labelForegroundColour = UIColor.foregroundColour
    static let labelTextAlignment = NSTextAlignment.center
  }

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  var slot1Text: String? {
    get {
      return slot1Label.text
    }
    set {
      slot1Label.text = newValue
      if caretIndex < 0 && (newValue ?? "").replacingOccurrences(of: " ", with: "").count == 0 {
        caretIndex = 0
      }
    }
  }

  var slot2Text: String? {
    get {
      return slot2Label.text
    }
    set {
      slot2Label.text = newValue
      if caretIndex < 0 && (newValue ?? "").replacingOccurrences(of: " ", with: "").count == 0 {
        caretIndex = 1
      }
    }
  }

  var slot3Text: String? {
    get {
      return slot3Label.text
    }
    set {
      slot3Label.text = newValue
      if caretIndex < 0 && (newValue ?? "").replacingOccurrences(of: " ", with: "").count == 0 {
        caretIndex = 2
      }
    }
  }

  var slot4Text: String? {
    get {
      return slot4Label.text
    }
    set {
      slot4Label.text = newValue
      if caretIndex < 0 && (newValue ?? "").replacingOccurrences(of: " ", with: "").count == 0 {
        caretIndex = 3
      }
    }
  }

  var slot5Text: String? {
    get {
      return slot5Label.text
    }
    set {
      slot5Label.text = newValue
      if caretIndex < 0 && (newValue ?? "").replacingOccurrences(of: " ", with: "").count == 0 {
        caretIndex = 4
      }
    }
  }

  var slot6Text: String? {
    get {
      return slot6Label.text
    }
    set {
      slot6Label.text = newValue
      if caretIndex < 0 && (newValue ?? "").replacingOccurrences(of: " ", with: "").count == 0 {
        caretIndex = 5
      }
    }
  }

  func setSlotText(text: String) {
    caretIndex = -1

    for (index, character) in text.padding(toLength: 6, withPad: " ", startingAt: 0).enumerated() {
      switch index {
      case 0:
        slot1Text = "\(character)"
      case 1:
        slot2Text = "\(character)"
      case 2:
        slot3Text = "\(character)"
      case 3:
        slot4Text = "\(character)"
      case 4:
        slot5Text = "\(character)"
      case 5:
        slot6Text = "\(character)"
      default:
        break
      }
    }
  }

  func clearText() {
    slot1Text = " "
    slot2Text = " "
    slot3Text = " "
    slot4Text = " "
    slot5Text = " "
    slot6Text = " "
  }

  private func setup () {
    accessibilityTraits = UIAccessibilityTraits.staticText

    [slot1, slot2, slot3, slot4, slot5, slot6].forEach {
      addSubview($0)
      $0.backgroundColor = C.slotBackgroundColour
      $0.layer.cornerRadius = C.slotCornerRadius
    }

    [slot1Label, slot2Label, slot3Label, slot4Label, slot5Label, slot6Label].forEach {
      addSubview($0)
      $0.font = C.labelFont
      $0.textAlignment = C.labelTextAlignment
      $0.textColor = C.labelForegroundColour
    }

    [caret1, caret2, caret3, caret4, caret5, caret6].forEach {
      addSubview($0)
    }

    setNeedsLayout()
    invalidateIntrinsicContentSize()
    layoutIfNeeded()
    updateCaretPosition()
  }

  override func layoutSubviews() {
    super.layoutSubviews()
    let width = bounds.width
    let itemInset: CGFloat = (width - (C.slotSize * 6)) / 5
    var x: CGFloat = 0

    [slot1, slot2, slot3, slot4, slot5, slot6].forEach { slot in
      slot.frame = CGRect(x: x, y: 0, width: C.slotSize, height: C.slotSize)
      x += C.slotSize + itemInset
    }

    x = 0

    [slot1Label, slot2Label, slot3Label, slot4Label, slot5Label, slot6Label].forEach { label in
      label.frame = CGRect(x: x, y: 0, width: C.slotSize, height: C.slotSize)
      x += C.slotSize + itemInset
    }

    caret1.frame = CGRect(x: slot1Label.frame.minX + 8, y: slot1Label.frame.minY + 10, width: 2, height: slot1Label.frame.height - 20)
    caret2.frame = CGRect(x: slot2Label.frame.minX + 8, y: slot2Label.frame.minY + 10, width: 2, height: slot2Label.frame.height - 20)
    caret3.frame = CGRect(x: slot3Label.frame.minX + 8, y: slot3Label.frame.minY + 10, width: 2, height: slot3Label.frame.height - 20)
    caret4.frame = CGRect(x: slot4Label.frame.minX + 8, y: slot4Label.frame.minY + 10, width: 2, height: slot4Label.frame.height - 20)
    caret5.frame = CGRect(x: slot5Label.frame.minX + 8, y: slot5Label.frame.minY + 10, width: 2, height: slot5Label.frame.height - 20)
    caret6.frame = CGRect(x: slot6Label.frame.minX + 8, y: slot6Label.frame.minY + 10, width: 2, height: slot6Label.frame.height - 20)
  }

  override var intrinsicContentSize: CGSize {
    return CGSize(width: UIView.noIntrinsicMetric, height: C.slotSize)
  }

  fileprivate func updateCaretPosition () {
    [caret1, caret2, caret3, caret4, caret5, caret6].forEach {
      $0.isHidden = true
    }

    switch caretIndex {
    case 0:
      caret1.isHidden = false
    case 1:
      caret2.isHidden = false
    case 2:
      caret3.isHidden = false
    case 3:
      caret4.isHidden = false
    case 4:
      caret5.isHidden = false
    case 5:
      caret6.isHidden = false
    default:
      break
    }
  }
}

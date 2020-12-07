import UIKit

extension UIColor {
    func rgb() -> Int? {
        var fRed : CGFloat = 0
        var fGreen : CGFloat = 0
        var fBlue : CGFloat = 0
        var fAlpha: CGFloat = 0
        if self.getRed(&fRed, green: &fGreen, blue: &fBlue, alpha: &fAlpha) {
            let iRed = Int(fRed * 255.0)
            let iGreen = Int(fGreen * 255.0)
            let iBlue = Int(fBlue * 255.0)
            let iAlpha = Int(fAlpha * 255.0)

            //  (Bits 24-31 are alpha, 16-23 are red, 8-15 are green, 0-7 are blue).
            let rgb = (iAlpha << 24) + (iRed << 16) + (iGreen << 8) + iBlue
            return rgb
        } else {
            // Could not extract RGBA components:
            return nil
        }
    }

  static var brandColour: UIColor {
    get {
      return UIColor(named: "BrandColour")!
    }
  }

  static var translucentBrandColour: UIColor {
    get {
      return UIColor(named: "BrandColour")!.withAlphaComponent(0.5)
    }
  }

  static var translucentFadedBrandColour: UIColor {
    get {
      return UIColor(named: "BrandColour")!.withAlphaComponent(0.2)
    }
  }

  static var accentColour: UIColor {
    get {
      return UIColor(named: "AccentColour")!
    }
  }

  static var backgroundColour: UIColor {
    get {
      return UIColor(named: "BackgroundColour")!
    }
  }

  static var brightBrandColour: UIColor {
    get {
      return UIColor(named: "BrightBrandColour")!
    }
  }

  static var contrastedBackgroundColour: UIColor {
    get {
      return UIColor(named: "ContrastedBackgroundColour")!
    }
  }

  static var contrastedForegroundColour: UIColor {
    get {
      return UIColor(named: "ContrastedForegroundColour")!
    }
  }

  static var fadedForegroundColour: UIColor {
    get {
      return UIColor(named: "FadedForegroundColour")!
    }
  }

  static var foregroundColour: UIColor {
    get {
      return UIColor(named: "ForegroundColour")!
    }
  }

  static var grey1: UIColor {
    get {
      return UIColor(named: "Grey1")!
    }
  }

  static var grey2: UIColor {
    get {
      return UIColor(named: "Grey2")!
    }
  }

  static var grey3: UIColor {
    get {
      return UIColor(named: "Grey3")!
    }
  }

  static var grey4: UIColor {
    get {
      return UIColor(named: "Grey4")!
    }
  }

  static var grey5: UIColor {
    get {
      return UIColor(named: "Grey5")!
    }
  }

  static var grey6: UIColor {
    get {
      return UIColor(named: "Grey6")!
    }
  }

  static var grey7: UIColor {
    get {
      return UIColor(named: "Grey7")!
    }
  }

  static var grey8: UIColor {
    get {
      return UIColor(named: "Grey8")!
    }
  }

  static var grey9: UIColor {
    get {
      return UIColor(named: "Grey9")!
    }
  }

  static var grey10: UIColor {
    get {
      return UIColor(named: "Grey10")!
    }
  }

  static var grey11: UIColor {
    get {
      return UIColor(named: "Grey11")!
    }
  }

  static var grey12: UIColor {
    get {
      return UIColor(named: "Grey12")!
    }
  }

  static var grey13: UIColor {
    get {
      return UIColor(named: "Grey13")!
    }
  }

  static var grey14: UIColor {
    get {
      return UIColor(named: "Grey14")!
    }
  }

  static var grey15: UIColor {
    get {
      return UIColor(named: "Grey15")!
    }
  }

  static var grey16: UIColor {
    get {
      return UIColor(named: "Grey16")!
    }
  }

  static var orangeColour: UIColor {
    get {
      return UIColor(named: "OrangeColour")!
    }
  }

  static var paleGreenColour: UIColor {
    get {
      return UIColor(named: "PaleGreenColour")!
    }
  }

  static var deepBlueColour: UIColor {
    get {
      return UIColor(named: "DeepBlueColour")!
    }
  }

  static var chalkColour: UIColor {
    get {
      return UIColor(named: "ChalkColour")!
    }
  }

  static var charcoalColour: UIColor {
    get {
      return UIColor(named: "CharcoalColour")!
    }
  }

  static var redPlaceholderColour: UIColor {
    get {
      return UIColor(named: "RedPlaceholderColour")!
    }
  }

  static var greenPlaceholderColour: UIColor {
    get {
      return UIColor(named: "GreenPlaceholderColour")!
    }
  }

  static var bluePlaceholderColour: UIColor {
    get {
      return UIColor(named: "BluePlaceholderColour")!
    }
  }

  static var yellowPlaceholderColour: UIColor {
    get {
      return UIColor(named: "YellowPlaceholderColour")!
    }
  }
}

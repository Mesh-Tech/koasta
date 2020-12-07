import UIKit

private enum IconType: Int {
  case cocktail = 0
  case beer = 1
  case pint = 2
  case wine = 3
  case hipsterCocktail = 4

  var acronym: String {
    switch self {
    case .cocktail:
      return "cocktail"
    case .beer:
      return "beer"
    case .pint:
      return "pint"
    case .wine:
      return "wine"
    case .hipsterCocktail:
      return "hipster-cocktail"
    }
  }
}

private enum IconSize: Int {
  case small = 0
  case medium = 1
  case large = 2

  var acronym: String {
    switch self {
    case .small:
      return "s"
    case .medium:
      return "m"
    case .large:
      return "l"
    }
  }
}

private struct Icon: Hashable, Equatable {
  static func ==(lhs: Icon, rhs: Icon) -> Bool {
    return lhs.id == rhs.id
  }

  let id: String = UUID().uuidString
  var iconType: IconType
  var iconSize: IconSize
  var startDelay: TimeInterval
  var yOffsetPercent: CGFloat
  var xOffset: Int
  var movementSpeed: TimeInterval

  func hash(into hasher: inout Hasher) {
    hasher.combine(id.hashValue)
  }

  var size: CGSize {
    if iconType == .beer {
      if iconSize == .small {
        return CGSize(width: 40, height: 40)
      } else if iconSize == .medium {
        return CGSize(width: 60, height: 60)
      } else if iconSize == .large {
        return CGSize(width: 70, height: 70)
      }
    } else if iconType == .cocktail {
      if iconSize == .small {
        return CGSize(width: 35, height: 35)
      } else if iconSize == .medium {
        return CGSize(width: 50, height: 50)
      } else if iconSize == .large {
        return CGSize(width: 64, height: 64)
      }
    } else if iconType == .hipsterCocktail {
      if iconSize == .small {
        return CGSize(width: 42, height: 42)
      } else if iconSize == .medium {
        return CGSize(width: 65, height: 65)
      } else if iconSize == .large {
        return CGSize(width: 80, height: 80)
      }
    } else if iconType == .pint {
      if iconSize == .small {
        return CGSize(width: 26, height: 51)
      } else if iconSize == .medium {
        return CGSize(width: 34, height: 68)
      } else if iconSize == .large {
        return CGSize(width: 40, height: 80)
      }
    } else if iconType == .wine {
      if iconSize == .small {
        return CGSize(width: 26, height: 46)
      } else if iconSize == .medium {
        return CGSize(width: 40, height: 70)
      } else if iconSize == .large {
        return CGSize(width: 48, height: 84)
      }
    }

    return CGSize.zero
  }
}

class DrinksParallaxView: UIView {
  fileprivate var icons: [Icon] = []
  fileprivate var viewportWidth: CGFloat = 0
  fileprivate var viewportHeight: CGFloat = 0
  fileprivate let iconCount = 10

  fileprivate var imageCache: [IconSize: [IconType: UIImage]] = Dictionary()

  convenience init() {
    self.init(frame: CGRect.zero)
    setup()
  }

  override init(frame: CGRect) {
    super.init(frame: frame)
    setup()
  }

  required init?(coder aDecoder: NSCoder) {
    super.init(coder: aDecoder)
    setup()
  }

  func setup () {
    buildImageCache()

    var allIcons = Set<Icon>()

    for _ in 0...iconCount {
      let iconTypeInt = Int(arc4random_uniform(5))
      let iconSizeInt = Int(arc4random_uniform(3))
      let startDelay = TimeInterval(Int(arc4random_uniform(8)))
      let yOffsetPercent = CGFloat(Int(arc4random_uniform(101))) / 100.0
      let movementSpeed = TimeInterval(Int(arc4random_uniform(20)))

      allIcons.insert(Icon(
        iconType: IconType(rawValue: iconTypeInt)!,
        iconSize: IconSize(rawValue: iconSizeInt)!,
        startDelay: startDelay,
        yOffsetPercent: yOffsetPercent,
        xOffset: -1,
        movementSpeed: movementSpeed
      ))
    }

    icons = allIcons.sorted { iconA, iconB in
      return iconA.iconSize.rawValue < iconB.iconSize.rawValue
    }
  }

  override func layoutSubviews() {
    super.layoutSubviews()

    layer.removeAllAnimations()
    layer.sublayers?.forEach { $0.removeFromSuperlayer() }

    guard viewportHeight != UIScreen.main.bounds.width || viewportWidth != bounds.height else { return }

    viewportWidth = UIScreen.main.bounds.width
    viewportHeight = bounds.height - 84 // Ensure height accounts for height of largest icon

    for icon in icons {
      let iconLayer = CALayer()
      let size = icon.size
      let image = imageCache[icon.iconSize]![icon.iconType]!
      iconLayer.frame = CGRect(x: viewportWidth + size.width, y: CGFloat(Int(viewportHeight * icon.yOffsetPercent)), width: size.width, height: size.height)
      iconLayer.contents = image.cgImage!
      iconLayer.contentsScale = image.scale

      let animation = CABasicAnimation(keyPath: "position.x")
      animation.beginTime = CACurrentMediaTime() + icon.startDelay
      animation.repeatCount = Float.greatestFiniteMagnitude
      animation.duration = 10 + icon.movementSpeed
      animation.fromValue = viewportWidth + size.width
      animation.toValue = 0-size.width
      animation.isRemovedOnCompletion = true

      iconLayer.add(animation, forKey: "x")

      layer.addSublayer(iconLayer)
    }
  }

  fileprivate func buildImageCache () {
    for size in [IconSize.small, IconSize.medium, IconSize.large] {
      var sizeCache: [IconType: UIImage] = Dictionary()
      for type in [IconType.cocktail, IconType.beer, IconType.pint, IconType.wine, IconType.hipsterCocktail] {
        sizeCache[type] = UIImage(named: "\(type.acronym)-\(size.acronym)")!
      }
      imageCache[size] = sizeCache
    }
  }
}

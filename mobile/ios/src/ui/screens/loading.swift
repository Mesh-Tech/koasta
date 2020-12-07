import Foundation
import UIKit
import Cartography

class LoadingViewController: UIViewController {
  fileprivate let logo = UIImageView()
  fileprivate let tabBar = UITabBar()
  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  init () {
    super.init(nibName: nil, bundle: nil)
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.backgroundColour

    logo.image = UIImage(named: "logo-light")
    logo.contentMode = .scaleAspectFit

    view.addSubview(logo)
    view.addSubview(tabBar)

    constrain(view, logo, tabBar) { container, logo, tabBar in
      logo.center == container.center
      tabBar.bottom == container.bottom
      tabBar.leading == container.leading
      tabBar.trailing == container.trailing
    }
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
  }
}

import UIKit
import Cartography

class BetaSettingsViewController: UIViewController, Routable, UITableViewDelegate, UITableViewDataSource, InputCellDelegate {
  fileprivate var router: Router!
  fileprivate var globalEmitter: EventEmitter!

  fileprivate let bottomInsetEnlightener = UIView()
  fileprivate let tableView = UITableView(frame: CGRect.zero, style: .grouped)

  required init?(coder aDecoder: NSCoder) { super.init(coder: aDecoder) }

  var routerContext: Route?

  init(router: Router?,
       globalEmitter: EventEmitter?) {
    guard let router = router,
      let globalEmitter = globalEmitter else { fatalError() }
    super.init(nibName: nil, bundle: nil)

    self.router = router
    self.globalEmitter = globalEmitter
    modalPresentationStyle = .overCurrentContext
  }

  override func loadView() {
    super.loadView()
    view.backgroundColor = UIColor.grey7

    let contentView = UIView()
    let header = UILabel()
    header.font = UIFont.brandFont(ofSize: 14, weight: .regular)
    header.numberOfLines = 0
    header.lineBreakMode = .byWordWrapping
    header.preferredMaxLayoutWidth = view.bounds.width
    header.text = "Please restart the application after making any changes to these settings. They do not apply immediately."
    header.textColor = UIColor.grey10

    contentView.addSubview(header)
    header.preferredMaxLayoutWidth = UIScreen.main.bounds.width - 20
    header.frame = CGRect(x: 10, y: 10, width: header.preferredMaxLayoutWidth, height: header.intrinsicContentSize.height)
    contentView.frame = CGRect(x: 0, y: 0, width: UIScreen.main.bounds.width, height: header.intrinsicContentSize.height + 20)

    tableView.tableHeaderView = contentView
    tableView.backgroundColor = UIColor.clear
    tableView.isOpaque = false
    tableView.delegate = self
    tableView.dataSource = self
    tableView.register(InputCell.self, forCellReuseIdentifier: "input-cell")

    bottomInsetEnlightener.backgroundColor = UIColor.clear
    bottomInsetEnlightener.isOpaque = false

    view.addSubview(bottomInsetEnlightener)
    view.addSubview(tableView)

    constrain(view, bottomInsetEnlightener, tableView) { container, bottomInsetEnlightener, tableView in
      bottomInsetEnlightener.bottom == container.bottom
      bottomInsetEnlightener.top == container.safeAreaLayoutGuide.bottom
      bottomInsetEnlightener.left == container.left
      bottomInsetEnlightener.right == container.right

      tableView.top == container.safeAreaLayoutGuide.top
      tableView.bottom == container.safeAreaLayoutGuide.bottom
      tableView.left == container.left
      tableView.right == container.right
    }
  }

  override func viewDidLoad() {
    super.viewDidLoad()
    navigationItem.title = "Beta Tweaks"
  }

  override func viewWillAppear(_ animated: Bool) {
    super.viewWillAppear(animated)
    UIView.animate(withDuration: 0.2) {
      self.navigationController?.navigationBar.titleTextAttributes = [NSAttributedString.Key.foregroundColor: UIColor.brandColour, NSAttributedString.Key.font: UIFont.brandFont(ofSize: UIFont.labelFontSize, weight: .medium)]
      self.navigationController?.navigationBar.tintColor = UIColor.brandColour
    }
    navigationController?.setNavigationBarHidden(false, animated: true)
  }

  func numberOfSections(in tableView: UITableView) -> Int {
    return 1
  }

  func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
    return 3
  }

  func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
    let cell = tableView.dequeueReusableCell(withIdentifier: "input-cell", for: indexPath) as! InputCell

    cell.tag = indexPath.row
    cell.delegate = self

    if indexPath.row == 0 {
      cell.textLabel?.text = "Latitude"
      if UserDefaults.standard.value(forKey: "PC_OVERRIDE_LAT") != nil {
        cell.value = String(UserDefaults.standard.double(forKey: "PC_OVERRIDE_LAT"))
      }
      cell.keyboardType = .numbersAndPunctuation
    } else if indexPath.row == 1 {
      cell.textLabel?.text = "Longitude"
      if UserDefaults.standard.value(forKey: "PC_OVERRIDE_LON") != nil {
        cell.value = String(UserDefaults.standard.double(forKey: "PC_OVERRIDE_LON"))
      }
      cell.keyboardType = .numbersAndPunctuation
    } else if indexPath.row == 2 {
      cell.textLabel?.text = "API URL"
      cell.value = UserDefaults.standard.string(forKey: "PC_OVERRIDE_API_URL")
      cell.keyboardType = .URL
    }

    return cell
  }

  func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
    tableView.deselectRow(at: indexPath, animated: true)
    (tableView.visibleCells.first {
      $0.tag == indexPath.row
    } as? InputCell)?.focus()
  }

  func textChanged(to text: String?, for inputCell: InputCell) {
    if let text = text, text.count > 0 {
      if inputCell.tag == 0 {
        UserDefaults.standard.set(Double(text), forKey: "PC_OVERRIDE_LAT")
      } else if inputCell.tag == 1 {
        UserDefaults.standard.set(Double(text), forKey: "PC_OVERRIDE_LON")
      } else if inputCell.tag == 2 {
        UserDefaults.standard.set(text, forKey: "PC_OVERRIDE_API_URL")
      }
    } else {
      if inputCell.tag == 0 {
        UserDefaults.standard.removeObject(forKey: "PC_OVERRIDE_LAT")
      } else if inputCell.tag == 1 {
        UserDefaults.standard.removeObject(forKey: "PC_OVERRIDE_LON")
      } else if inputCell.tag == 2 {
        UserDefaults.standard.removeObject(forKey: "PC_OVERRIDE_API_URL")
      }
    }
  }
}

import XCTest
import KIF

extension XCTestCase {
  fileprivate func tester(_ file : String = #file, _ line : Int = #line) -> KIFUITestActor {
    return KIFUITestActor(inFile: file, atLine: line, delegate: self)
  }

  fileprivate func system(_ file : String = #file, _ line : Int = #line) -> KIFSystemTestActor {
    return KIFSystemTestActor(inFile: file, atLine: line, delegate: self)
  }

  var tester: KIFUITestActor { return tester() }
  var system: KIFSystemTestActor { return system() }
}

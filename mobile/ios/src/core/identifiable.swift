import Foundation

infix operator ~=
func ~=(lhs: Identifiable, rhs: Identifiable) -> Bool {
  return lhs.identityId == rhs.identityId
}

protocol Identifiable {
  var identityId: Int {get}
}

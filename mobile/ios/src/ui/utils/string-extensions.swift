import Foundation

extension String {
  func indicesOf(string: String) -> [Int] {
    // Converting to an array of utf8 characters makes indicing and comparing a lot easier
    let search = self.utf8.map { $0 }
    let word = string.utf8.map { $0 }

    var indices = [Int]()

    // m - the beginning of the current match in the search string
    // i - the position of the current character in the string we're trying to match
    var m = 0, i = 0
    while m + i < search.count {
      if word[i] == search[m+i] {
        if i == word.count - 1 {
          indices.append(m)
          m += i + 1
          i = 0
        } else {
          i += 1
        }
      } else {
        m += 1
        i = 0
      }
    }

    return indices
  }
}

extension NSString {
  func indicesOf(string: NSString) -> [NSRange] {
    var indices: [NSRange] = []

    var searchRange = NSRange(location: 0, length: length)
    var foundRange = NSRange()
    while searchRange.location < length {
      searchRange.length = length - searchRange.location
      foundRange = range(of: string as String, options: [], range: searchRange)
      if Int(foundRange.location) != NSNotFound {
        searchRange.location = foundRange.location + foundRange.length
        indices.append(foundRange)
        foundRange = NSRange()
      } else {
        break
      }
    }

    return indices
  }
}

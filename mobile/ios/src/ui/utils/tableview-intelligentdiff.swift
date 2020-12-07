import Foundation
import UIKit
import Differ

extension UITableView {
  func apply<T: Collection>(diff: NestedExtendedDiff, from oldData: [T], to newData: [T]) where T.Index == Int, T.Iterator.Element: Identifiable {
    var deletedSections: [Int] = []
    var addedSections: [Int] = []
    var deletedIndexPaths: Set<IndexPath> = Set()
    var addedIndexPaths: Set<IndexPath> = Set()

    var mutatedIndexPaths: Set<IndexPath> = Set()

    diff.forEach { change in
      switch change {
      case .insertSection(let section):
        addedSections.append(section)
      case .deleteSection(let section):
        deletedSections.append(section)
      case .deleteElement(let row, section: let section):
        if let index = addedIndexPaths.firstIndex(where: {$0.row == row && $0.section == section }), oldData[section][row] ~= newData[section][row] {
          mutatedIndexPaths.insert(addedIndexPaths[index])
          addedIndexPaths.remove(at: index)
        } else {
          deletedIndexPaths.insert(IndexPath(row: row, section: section))
        }
      case .insertElement(let row, section: let section):
        if let index = deletedIndexPaths.firstIndex(where: {$0.row == row && $0.section == section }), oldData[section][row] ~= newData[section][row] {
          mutatedIndexPaths.insert(deletedIndexPaths[index])
          deletedIndexPaths.remove(at: index)
        } else {
          addedIndexPaths.insert(IndexPath(row: row, section: section))
        }
      case .moveElement(from: let oldIndex, to: let newIndex):
        deletedIndexPaths.insert(IndexPath(row: oldIndex.item, section: oldIndex.section))
        addedIndexPaths.insert(IndexPath(row: newIndex.item, section: newIndex.section))
      default:
        break
      }
    }

    let manuallyMutatedIndexPaths: Set<IndexPath> = Set(self.indexPathsForVisibleRows ?? []).intersection(mutatedIndexPaths).filter { indexPath in
      if let _ = cellForRow(at: indexPath) as? CellImmediatelyUpdatable<T.Element> { return true }
      return false
    }
    mutatedIndexPaths = mutatedIndexPaths.subtracting(manuallyMutatedIndexPaths)

    beginUpdates()

    reloadRows(at: Array(mutatedIndexPaths), with: .fade)
    deleteRows(at: Array(deletedIndexPaths), with: .automatic)
    insertRows(at: Array(addedIndexPaths), with: .automatic)
    deleteSections(IndexSet(deletedSections), with: .automatic)
    insertSections(IndexSet(addedSections), with: .automatic)

    endUpdates()

    manuallyMutatedIndexPaths.forEach { indexPath in
      guard let cell = cellForRow(at: indexPath) as? CellImmediatelyUpdatable<T.Element> else { return }
      let old = oldData[indexPath.section][indexPath.row]
      let new = newData[indexPath.section][indexPath.row]
      cell.update(from: old, to: new)
    }
  }

  func apply<T: Collection>(diff: ExtendedDiff, from oldData: T, to newData: T, inSection section: Int)  where T.Index == Int, T.Iterator.Element: Identifiable {
    var deletedIndexPaths: Set<IndexPath> = Set()
    var addedIndexPaths: Set<IndexPath> = Set()

    var mutatedIndexPaths: Set<IndexPath> = Set()

    diff.forEach { change in
      switch change {
      case .insert(let row):
        if let index = deletedIndexPaths.firstIndex(where: {$0.row == row }), oldData[row] ~= newData[row] {
          mutatedIndexPaths.insert(deletedIndexPaths[index])
          deletedIndexPaths.remove(at: index)
        } else {
          addedIndexPaths.insert(IndexPath(row: row, section: section))
        }
      case .delete(let row):
        if let index = addedIndexPaths.firstIndex(where: {$0.row == row && $0.section == section }), oldData[row] ~= newData[row] {
          mutatedIndexPaths.insert(addedIndexPaths[index])
          addedIndexPaths.remove(at: index)
        } else {
          deletedIndexPaths.insert(IndexPath(row: row, section: section))
        }
      case .move(from: let oldIndex, to: let newIndex):
        deletedIndexPaths.insert(IndexPath(row: oldIndex, section: section))
        addedIndexPaths.insert(IndexPath(row: newIndex, section: section))
      }
    }

    let manuallyMutatedIndexPaths: Set<IndexPath> = Set(self.indexPathsForVisibleRows ?? []).intersection(mutatedIndexPaths).filter { indexPath in
      if let _ = cellForRow(at: indexPath) as? CellImmediatelyUpdatable<T.Element> { return true }
      return false
    }
    mutatedIndexPaths = mutatedIndexPaths.subtracting(manuallyMutatedIndexPaths)

    beginUpdates()

    reloadRows(at: Array(mutatedIndexPaths), with: .fade)
    deleteRows(at: Array(deletedIndexPaths), with: .automatic)
    insertRows(at: Array(addedIndexPaths), with: .automatic)

    endUpdates()

    manuallyMutatedIndexPaths.forEach { indexPath in
      guard let cell = cellForRow(at: indexPath) as? CellImmediatelyUpdatable<T.Element> else { return }
      let old = oldData[indexPath.row]
      let new = newData[indexPath.row]
      cell.update(from: old, to: new)
    }
  }
}

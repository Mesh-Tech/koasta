package io.meshtech.koasta.activity

/**
 * This interface enables tests to work around an issue where the Koin container is torn down
 * before the activity actually stops.
 *
 * The BaseTest will call onBeforeDestroy on any activity implementing this interface. Any
 * Activity that performs teardown operations on injected objects should do so within
 * onBeforeDestroy to prevent the tests failing.
 */
interface TestableActivity {
  fun onBeforeDestroy()
}

# Changelog

## 0.3.0 (branch `feature/0.3.0`, not released)

### Editor

- The editor is UI Toolkit. Every inspector and window is a UXML document next to its script in `Editor/`, assigned to the script as a default reference, and all of them share `Editor/Ripple.uss`.
- Odin Inspector is no longer required. The runtime has no Odin attributes and the assemblies no longer wait for the `ODIN_INSPECTOR` define. With Odin installed, the asset inspectors are still Ripple's own, and the reference and Signal drawers show inside Odin-drawn components.
- Variable inspector: notes, the value, `Clamp` with `Min` and `Max`, `Persist`, the Signal, then a Debug foldout. In play mode the value field shows and edits the live value and a `Reset` button appears.
- Event inspector: a `Raise` button with a field for the value, enabled in play mode.
- List inspector: the initial values in edit mode, the live values in play mode.
- Debug foldout on every asset: `Raised By` with the time of the last call, the call stack of the selected row with lines that open the file, and `Listeners`, which fills in when the inspector opens. `Include Prefabs` replaces the `Scan prefabs too` toggle.
- A reference field is one line with a menu at the end to switch between constant and variable. Dragging the label of a number still scrubs it.
- `Tools > Ripple > Debugger` lists every Ripple asset beside the session log. The log is a table: time, asset, caller, value. Double-clicking a row opens its asset.
- `Tools > Ripple > Type Generator` finds the type with a search field.

### Features

- String and Color are built in: variable, reference, event, list and both listeners.
- `LocalVariables` component and `variable.For(this)` give each object its own copy of a variable. Variable listeners below the component use the copy without any code.
- `OnItemAdded` and `OnItemRemoved` on lists are `Signal<T>`: a list change can call a `UnityEvent` and raise events from the inspector. `+=` and `-=` keep working.
- Dropping an event or variable asset on a GameObject, in the Hierarchy or on its inspector, adds the matching listener with the asset assigned.
- `BaseVariable.ClearSavedValue()` forgets the value a persisted variable saved.
- `VariableListSO` is the non-generic base of every list, the way `GameEvent` is for events.
- A change made through a UI Toolkit binding is attributed to the binding, not to Unity's property plumbing.

### Breaking

- `OnItemAdded` and `OnItemRemoved` are no longer `Action<T>`. Assigning them or calling `?.Invoke` no longer compiles; subscribe with `+=`.
- `EventLoggerWindow` and `RippleWizard` are removed, with their menu items.
- Removed with Odin: the `AssetDropdown` attribute and the dropdown it put on every event and variable field, the inline editor that unfolded a variable below a field that references it, and the `Required` marker on listener fields.
- `Logger.Log` takes the asset, caller, value and time. `Logger.Logs` replaces `GetLogs()` and `onLogAdded`. `LogEntry` has `caller`, `value` and `time` instead of `message`.
- `RippleConnections` moved to the editor assembly and is internal.

## 0.2.0

### Events

- `Signal` and `Signal<T>` replace UltEvents across the package. A Signal is a `UnityEvent`, a list of Ripple events to raise, and code listeners. UltEvents is no longer a dependency.
- `GameEvent<T>.Raise(T)` and `VoidEventSO.Raise()` can be wired from a `UnityEvent`.
- `GameObjectEvent` and `EventListenerGameObject` added.
- `Vector2Event` and `Vector3Event` moved into the `Ripple` namespace.

### Variables

- The current value is no longer serialized. Playing does not change the asset, so runtime values stop showing up in version control.
- `ResetValue()` is public and notifies listeners. `BaseVariable.ResetAll()` resets every loaded variable and list.
- Clamped numeric variables show a slider between `min` and `max`.
- `Persist` saves a variable to PlayerPrefs.
- Variables are UI Toolkit data sources (`CurrentValue` is bindable and settable).
- `SetCurrentValue(value, sender)` and `Invoke(value, sender)` record who made the change.

### Lists

- A list keeps its initial values in the asset and its current values in memory, like a variable. Lists are `BaseVariable`s.
- `AddRange` and `Resize` added. `Insert`, `RemoveAt` and `Clear` now raise `OnItemAdded` and `OnItemRemoved` like `Add` and `Remove` do.
- The `Runtime Set` toggle is gone. A list with no initial values starts every session empty.
- `RuntimeSetMember` moved here from JamKit.

### Debugging

- `Invoked by` keeps the full call stack for each caller. `Trace` logs it with clickable file links.
- Event Logger lists newest first and has a Clear button.
- `Scan Listeners` reads serialized listeners from `UnityEvent` data.

### Editor

- `Tools > Ripple > Type Generator` generates variable, reference, event, list and listener classes for a type.
- Menus moved under `Tools > Ripple`.
- The wizard uses Unity's `TypeCache`. `RippleTypeCache` and `ReflectionUtils` are removed.
- `Tests/` added as an edit mode test assembly.

### Breaking

- Requires Unity 6000.0 or newer (0.1.4 required 2022.1).
- UltEvent responses on existing event and listener assets do not convert. Re-wire them on the `UnityEvent`.
- `VariableListSO` serializes `_initialValues` (migrated from `_currentValue`). The `_runtimeSet` field is removed.

## 0.1.4

Last release on UltEvents. Tagged `v0.1.4`.

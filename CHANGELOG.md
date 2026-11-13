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
- `Tools > Ripple > Debugger` lists every Ripple asset, with its icon, beside the session log. The log is a table: time, asset, caller, value. It keeps the newest 10,000 entries, and its search matches the asset or the caller. Double-clicking a row opens its asset, or selects a local copy. The selected asset survives a project change and a domain reload.
- A Signal's `Also Raise` list warns about any event it cannot raise.
- `Tools > Ripple > Type Generator` finds the type with a search field.

### Features

- A field typed `GameEvent` takes any event. `AddListener(Action)` and `RemoveListener(Action)` hear every raise, and `Raise(value)` passes the value to an event of that type and raises a void event without it. `Event Listener Void` and a Signal's `Also Raise` list take any event the same way.
- String and Color are built in: variable, reference, event, list and both listeners. `Vector2VariableSO` and `Vector2Reference` complete Vector2.
- Lists record who changed them: `Raised By` and the Debugger log show `+ item` and `- item`, and `Listeners` shows the responses wired on both list Signals.
- `LocalVariables` component and `variable.For(this)` give each object its own copy of a variable. Variable listeners below the component use the copy without any code.
- `OnItemAdded` and `OnItemRemoved` on lists are `Signal<T>`: a list change can call a `UnityEvent` and raise events from the inspector. `+=` and `-=` keep working.
- Dropping an event or variable asset on a GameObject, in the Hierarchy or on its inspector, adds the matching listener with the asset assigned.
- `BaseVariable.ClearSavedValue()` forgets the value a persisted variable saved.
- `VariableListSO` is the non-generic base of every list, the way `GameEvent` is for events.
- A change made through UI Toolkit, such as a bound field, is attributed to `UI Toolkit` instead of Unity's internal types.
- A local copy starts at the variable's initial value and never persists. Ten enemies no longer write ten PlayerPrefs keys.

### Fixes

- Clamping no longer throws when `Min` is greater than `Max`.
- `PreviousValue` is the start value until the first change, for value types too.

### Breaking

- `OnItemAdded` and `OnItemRemoved` are no longer `Action<T>`. Assigning them or calling `?.Invoke` no longer compiles; subscribe with `+=`.
- `EventLoggerWindow` and `RippleWizard` are removed, with their menu items.
- Removed with Odin: the `AssetDropdown` attribute and the dropdown it put on every event and variable field, the inline editor that unfolded a variable below a field that references it, and the `Required` marker on listener fields.
- `Logger`, `LogEntry`, `RippleConnectionRegistry` and `SubscriberInfo` are internal, and `RippleConnections` moved to the editor assembly. The Debugger window is the way to read the log.
- `IVariable<T>` is removed. Nothing implemented it.
- `RippleData` is obsolete. Nothing reads it; remove it from generated types to clear the warning.
- Setting a list item by index raises `OnItemRemoved` for the old item and `OnItemAdded` for the new one.

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

# Changelog

## 0.3.0 (branch `feature/0.3.0`, not released)

- String and Color are built in: variable, reference, event, list and both listeners.
- `LocalVariables` component and `variable.For(this)` give each object its own copy of a variable. Variable listeners below the component use the copy without any code.
- `OnItemAdded` and `OnItemRemoved` on lists are `Signal<T>`: a list change can call a `UnityEvent` and raise events from the inspector. `+=` and `-=` keep working.
- Dropping an event or variable asset on a GameObject, in the Hierarchy or on its inspector, adds the matching listener with the asset assigned.
- `Tools > Ripple > Debugger` replaces the Event Logger and the Wizard: the asset tree and the session log in one window, one search field for both.
- The value is the first field of a variable in edit and play mode.

### Breaking

- `OnItemAdded` and `OnItemRemoved` are no longer `Action<T>`. Assigning them or calling `?.Invoke` no longer compiles; subscribe with `+=`.
- `EventLoggerWindow` and `RippleWizard` are removed, with their menu items.

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

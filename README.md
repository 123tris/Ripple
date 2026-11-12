# Ripple

ScriptableObject variables and events for Unity, after Ryan Hipple's "Game Architecture with Scriptable Objects". State and events are assets, so systems meet in the inspector instead of referencing each other.

Requires Odin Inspector. See [ROADMAP.md](ROADMAP.md) for where the package is going and [CHANGELOG.md](CHANGELOG.md) for what changed.

## Install

Add to `Packages/manifest.json`:

```json
"com.metz.ripple": "https://github.com/123tris/Ripple.git#feature/jamkit"
```

| Ref | Version | Notes |
| --- | --- | --- |
| `feature/jamkit` | 0.2.0 | Current work. Signal instead of UltEvents. |
| `v0.1.4` (`main`) | 0.1.4 | Last UltEvents release. Unity 2022.1 or newer. |

## The pieces

**Variables** hold a value. `Assets > Create > Variables`.

```csharp
[SerializeField] private FloatVariableSO health;

health.CurrentValue -= 10f;
health.AddListener(OnHealthChanged);
health.ResetValue();
```

- The asset stores the initial value. The current value lives only in memory, so playing never changes the asset.
- `Persist` saves the value to PlayerPrefs and restores it next session.
- `Is Clamped` on numeric variables keeps the value between `min` and `max` and turns the inspector field into a slider.
- `BaseVariable.ResetAll()` puts every loaded variable and list back to its starting value, for restarting a session without reloading.
- A variable is a UI Toolkit data source: bind to `CurrentValue` from UXML.

**References** let a field be either a constant or a variable asset, chosen in the inspector.

```csharp
[SerializeField] private FloatReference speed = new(5f);

transform.position += Vector3.right * speed;
```

**Events** are assets you raise. `Assets > Create > Events`.

```csharp
[SerializeField] private VoidEventSO gameOver;
[SerializeField] private FloatEvent damaged;

gameOver.Raise();
damaged.Invoke(12f, this);
```

`EventListener` and `VariableListener` components respond to them in a scene.

**Lists** are variables that hold a list, with `OnItemAdded` and `OnItemRemoved`. A list with no initial values is a runtime set: objects join it with a `RuntimeSetMember` component and leave when disabled.

**Signal** is the event type used everywhere above, and the one to use for events on your own components. In the inspector it is a `UnityEvent` plus a list of Ripple events to raise. In code it takes listeners.

```csharp
public Signal<float> OnDamaged = new();

OnDamaged.Invoke(amount);
health.OnDamaged += amount => Debug.Log(amount);
```

## Your own types

`Tools > Ripple > Type Generator` writes the variable, reference, event, list and listener classes for any type, one folder per type.

## Debugging

Every variable and event asset has a Debug box in its inspector:

- **Invoked by** lists who changed or raised it, how often, and the last value. `Ping` selects the sender, `Trace` logs the full call stack with clickable file links.
- **Scan Listeners** lists everything listening, in scenes, prefabs and code.
- Pass yourself as the sender (`variable.SetCurrentValue(value, this)`, `gameEvent.Invoke(value, this)`) to have the object named instead of the calling method.

`Tools > Ripple > Event Logger` shows every change and raise of the session, newest first. `Tools > Ripple > Wizard` browses all Ripple assets in the project.

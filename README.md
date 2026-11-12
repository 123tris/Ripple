# Ripple

ScriptableObject variables and events for Unity, after Ryan Hipple's "Game Architecture with Scriptable Objects". State and events are assets, so systems meet in the inspector instead of referencing each other.

No dependencies: the inspectors are UI Toolkit, and the package works the same with or without Odin Inspector. See [ROADMAP.md](ROADMAP.md) for where the package is going and [CHANGELOG.md](CHANGELOG.md) for what changed.

## Install

Add to `Packages/manifest.json`:

```json
"com.metz.ripple": "https://github.com/123tris/Ripple.git#v0.2.0"
```

| Ref | Version | Notes |
| --- | --- | --- |
| `feature/0.3.0` | 0.3.0 | Not released yet. UI Toolkit editor, no Odin requirement, local variables, String and Color, drag to listen. |
| `v0.2.0` | 0.2.0 | Current release. Signal instead of UltEvents. Unity 6000.0 or newer. |
| `v0.1.4` | 0.1.4 | Last UltEvents release. Unity 2022.1 or newer. |

Installing without a ref follows `main`, which moves with every release. Pin a tag.

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
- `Clamp` on numeric variables keeps the value between `Min` and `Max` and turns the inspector field into a slider.
- In play mode the inspector shows the live value. Editing it there changes the variable and notifies its listeners.
- `BaseVariable.ResetAll()` puts every loaded variable and list back to its starting value, for restarting a session without reloading.
- A variable is a UI Toolkit data source: bind to `CurrentValue` from UXML.
- Bool, Int, Float, String, Color, Vector3 and GameObject are built in.

**References** let a field be either a constant or a variable asset, chosen from the menu at the end of the field.

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

`EventListener` and `VariableListener` components respond to them in a scene. Drop an event or variable asset on a GameObject in the Hierarchy, or on its inspector, to add the matching listener with the asset assigned.

**Lists** are variables that hold a list, with `OnItemAdded` and `OnItemRemoved` Signals. A list with no initial values is a runtime set: objects join it with a `RuntimeSetMember` component and leave when disabled.

**Signal** is the event type used everywhere above, and the one to use for events on your own components. In the inspector it is a `UnityEvent` plus a list of Ripple events to raise. In code it takes listeners.

```csharp
public Signal<float> OnDamaged = new();

OnDamaged.Invoke(amount);
health.OnDamaged += amount => Debug.Log(amount);
```

## One value per object

Ten enemies cannot share one `Health` asset. Add a **Local Variables** component to the root of the prefab and list the variable: that object and its children get their own copy at runtime. Every `VariableListener` below it listens to the copy. Code asks for it once:

```csharp
[SerializeField] private FloatVariableSO health;

private void Awake() => health = health.For(this);
```

A variable that is not listed, or an object without the component, keeps using the shared asset.

## Your own types

`Tools > Ripple > Type Generator` writes the variable, reference, event, list and listener classes for any type, one folder per type. Search for the type by name, pick it from the list, and generate.

## Debugging

Every variable, list and event asset has a Debug foldout in its inspector:

- **Raised By** lists who changed or raised it, how often, when, and the last value. Select a row to see the call stack captured for that caller. Click a line of the stack to open the file at that line.
- **Listeners** lists what is listening: responses wired on the asset, code that subscribed, and listener components in the open scenes. `Include Prefabs` also searches every prefab.
- Pass yourself as the sender (`variable.SetCurrentValue(value, this)`, `gameEvent.Invoke(value, this)`) to have the object named instead of the calling method. Selecting its row pings it.
- An event has a `Raise` button, with a field for the value when it takes one. It is enabled in play mode.

`Tools > Ripple > Debugger` lists every Ripple asset in the project. Select **Log** at the top for every change and raise of the session, newest first, or select an asset for its inspector. The search field filters both by asset name, and double-clicking a log row opens its asset.

## Editing the editor

Each inspector and window is a UXML document next to its script in `Editor/`, assigned to the script as a default reference, and all of them share `Editor/Ripple.uss`. Open one in UI Builder to rearrange it. Fields are placed by `binding-path`, so moving one needs no code; the elements the scripts look up are found by `name`, so keep those.

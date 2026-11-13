# Ripple

ScriptableObject variables and events for Unity, after Ryan Hipple's "Game Architecture with Scriptable Objects". State and events are assets, so systems meet in the inspector instead of referencing each other.

No third-party dependencies: the inspectors are UI Toolkit, and the package works the same with or without Odin Inspector. See [ROADMAP.md](ROADMAP.md) for where the package is going and [CHANGELOG.md](CHANGELOG.md) for what changed.

## Install

Add to `Packages/manifest.json`:

```json
"com.metz.ripple": "https://github.com/123tris/Ripple.git#v0.2.0"
```

| Ref | Version | Notes |
| --- | --- | --- |
| `feature/0.3.0` | 0.3.0 | Not released yet. UI Toolkit editor, no Odin requirement, local variables, fields that take any event, String, Color and Vector2, drag to listen. |
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
- Bool, Int, Float, String, Color, Vector2, Vector3 and GameObject are built in.

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

A field typed `GameEvent` takes any event, with or without a value:

```csharp
[SerializeField] private GameEvent onHit;

onHit.AddListener(Flash);
onHit.Raise(damage);
```

`AddListener(Action)` hears every raise. `Raise(value)` passes the value to an event of that type and raises a void event without it. An event of a third type, say an `IntEvent` raised with a float, throws an exception that names both types. `Event Listener Void` takes any event the same way.

**Lists** are variables that hold a list, with `OnItemAdded` and `OnItemRemoved` Signals. Adding, removing or replacing an item raises them. A list with no initial values is a runtime set: objects join it with a `RuntimeSetMember` component and leave when disabled.

**Signal** is the event type used everywhere above, and the one to use for events on your own components. In the inspector it is a `UnityEvent` plus a list of Ripple events to raise: an event of the Signal's type gets the value, a void event is raised without it, and the inspector warns about any other. In code it takes listeners.

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

A copy starts at the variable's initial value and never persists. Each copy is a ScriptableObject made the first time something asks for it: right for tens of objects, too heavy for hundreds of bullets.

## Your own types

`Tools > Ripple > Type Generator` writes the variable, reference, event, list and listener classes for any type, one folder per type. Search for the type by name, pick it from the list, and generate.

## Debugging

Every variable, list and event asset has a Debug foldout in its inspector. A list records `+ item` and `- item` like a variable records its values.

- **Raised By** lists who changed or raised it, how often, when, and the last value. Select a row to see the call stack captured for that caller. Click a line of the stack to open the file at that line.
- **Listeners** lists what is listening: responses wired on the asset, code that subscribed with `AddListener`, and listener components in the open scenes. `Include Prefabs` also searches every prefab.
- Pass yourself as the sender (`variable.SetCurrentValue(value, this)`, `gameEvent.Invoke(value, this)`) to have the object named instead of the calling method. Selecting its row pings it.
- An event has a `Raise` button, with a field for the value when it takes one. It is enabled in play mode.

`Tools > Ripple > Debugger` lists every Ripple asset in the project. Select **Log** at the top for every change and raise of the session, newest first, or select an asset for its inspector. The search field filters the assets by name and the log by asset or caller. Double-clicking a log row opens its asset. The log keeps the newest 10,000 entries; a variable that changes every frame fills it fast, so tick `Disable Logging` on it.

## Editing the editor

Each inspector and window is a UXML document next to its script in `Editor/`, assigned to the script as a default reference. Open one in UI Builder and pick **Active Editor Theme** in the canvas theme menu; the Builder starts in the runtime theme. The document then looks almost the same as the inspector it draws.

- **Fields** are concrete controls placed by `binding-path`, with their tooltips in the UXML. Moving one needs no code. The elements the scripts look up are found by `name`, so keep those.
- **Ripple controls.** `ReferenceField` and `SignalField` are in the Builder's Library under Project > Custom Controls > Ripple. In the Builder they show a stand-in of the field; the inspector replaces it with the real one.
- **The value slot** of a variable shows a stand-in slider. The script swaps in the field for the variable's type.
- **States are classes on the document root**: `ripple-editor--playing`, `ripple-variable--clamped`, `ripple-variable--persisted`, `ripple-debug--history` and so on. The stylesheet decides what each state shows, and the scripts only switch the classes. Add or remove one on the root to see that state in the Builder.
- **Tables** take their cells from the documents in `Editor/Cells/`, and the Debugger's tree and the Type Generator's list take their rows from `Editor/Rows/`. Each cell is bound to one field of its row with data binding. A document that uses a cell or row document must also declare it as a `ui:Template`, so it imports first; a test checks every column.
- **Previews.** `Editor/Preview/` shows the variable inspector with its Debug foldout open and the Debugger window, both filled with sample rows from `RipplePreview.asset`.
- **Style.** `Editor/Ripple.uss` holds the sizes as `--ripple-*` variables on `.ripple-editor` and names classes block, element and modifier: `ripple-variable__range`, `ripple-variable--clamped`. Colors come from Unity's editor variables, so both editor themes work.

A test fails on any `PropertyField` in a Ripple document, because UI Builder cannot draw one.

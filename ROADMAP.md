# Ripple roadmap

## What Ripple is

State and events as ScriptableObject assets, wired in the inspector, with enough debugging to answer "who changed this?" without a debugger. Nothing else.

Rules for any change:

- **One way to do a thing.** One event type (`Signal`), one variable base, one listener per kind. A second mechanism for the same job is removed, not kept for compatibility.
- **Use Unity before writing anything.** `UnityEvent`, `TypeCache`, UI Toolkit and its bindings, `PropertyField` for anything serialized. No third-party dependency.
- **Layout in UXML, behaviour in C#.** An inspector or window is a UXML document, assigned to its script as a default reference. Fields are placed by `binding-path`. Code is only for what a document cannot do: live values, tables, buttons.
- **Small enough to read in one sitting.** The package is about 2,400 lines of C# and 200 of UXML and USS. A feature that needs its own subsystem (dispatcher, sink, registry of registries) is the wrong shape.
- **A feature lands when a real project needed it**, and in the smallest form that solves that project's problem.
- **No comments.** Names carry the meaning. Reasons go in the changelog.

## Branches and tags

| Ref | What it is |
| --- | --- |
| `main` | The current line. Every release is a tag on it: `v0.2.0` is the latest. |
| `feature/0.3.0` | 0.3.0, built and tested, waiting for the release steps below. |
| `v0.1.4` | The last release on UltEvents. Older projects pin it, or the exact commit they were built on. |
| `feature/abstraction+runtime-registry` | Fully contained in `main`. Kept only because JamKit `main` and the GMTK 2026 project still pin it. Delete once neither does. |
| `archive/*` tags | The retired experiments below. Restore one with `git switch -c <name> archive/<name>`. |

Work happens on short-lived branches off `main` and merges back. Projects pin a tag, never `main`. Experiments that are not going to merge get an `archive/` tag and the branch is deleted.

## What the old branches tried

Every branch below was an attempt at a real need. The need is kept where it holds up. The implementation is not.

| Branch | Idea | Verdict |
| --- | --- | --- |
| `random-variables` | A reference that returns a random value in a range | Dropped from Ripple. A reference is "constant or variable"; a third mode makes every reference field wider. If a jam needs it, it is a small struct in JamKit. |
| `feature/variable-declaration` | Per-object variables: a declaration asset plus an instancer component holding a value per GameObject | **Need kept** as `LocalVariables` in 0.3.0. The implementation stored untyped `object` values in a dictionary. |
| `feature/command-system` | `[SerializeReference]` command lists with conditions, run from an asset | Dropped. A `Signal` already runs a list of actions from the inspector, and branching belongs in code. |
| `feature/refactor-v1` | Color, String and Quaternion types; event groups; settings provider; factory; `Unit`; reference-usage search; richer logger; tests | Types: covered by the Type Generator, with String and Color built in since 0.3.0. Event groups: covered by a Signal's raise list. Usage search: covered by `Listeners` in the Debug foldout. Tests: kept as an idea, `Tests/` now exists. The rest dropped. |
| `v2-redesign` (and the unpushed "v3 overhaul" commit) | Ground-up rewrite: channels and a dispatcher, generated code for every built-in type, diagnostics sinks, traits and validators, snapshots, per-frame coalescing, instancers, a debugger window, a project validator | Dropped as a whole. It is 6,000 lines against 2,000 to deliver the same variables and events. Three ideas survive in smaller form, all in 0.3.0: instancers as `LocalVariables`, one debugger window, and drag-an-asset-onto-an-object to create its listener. |
| `feature/abstraction+runtime-registry` | Listener scan, runtime subscriber registry, PlayerPrefs persistence, draggable reference label | Merged. All of it is in 0.2.0. |

## What AVR Japan needed

Pull request 3 (`XRJapan:main` into `main`, "Sync hotfixes") is 22 commits of fixes made under project pressure. It is not merged. Each problem it solved is listed here with how 0.2.0 solves it.

| Problem at AVR | In the pull request | In 0.2.0 |
| --- | --- | --- |
| Runtime values were written into variable assets and showed up in version control | Stopped serializing the current value, marked assets dirty on reset | The current value is `[NonSerialized]` for variables and lists. Nothing to mark dirty. |
| No way to restart a session without reloading | `BaseVariable.ResetAllVariables()` over a static instance list | `BaseVariable.ResetAll()` and a public `ResetValue()`. Lists are included. Listeners are notified, so UI follows. |
| "Who changed this?" showed one method name, often a helper | A 30-frame stack trace string per invoke, a `context` argument, a `[ThreadStatic] CurrentInvoker` | `sender` argument on `SetCurrentValue` and `Invoke`. The full stack is captured once per distinct caller and logged with clickable links by the `Trace` button. No depth limit, no thread-static state. |
| Every custom type needed five hand-written classes | Type Generator window | `Tools > Ripple > Type Generator`, with a search field over the types in the project (the slow hand-built dropdown was the pull request's first attempt). Also generates the list type. |
| IL2CPP builds failed on `dynamic` in `Add` | Per-type `Add` overrides | Already on `main`: `Add` is abstract and each numeric type implements it. |
| A build failed on editor-only code | Hotfix | The runtime assembly is compiled without `UNITY_EDITOR` as part of the check before each commit. |
| Lists could not be resized or filled in bulk | `AddRange`, `ResizeRange` | `AddRange`, `Resize`. Every mutation raises the item events. |
| Clamped numbers were typed, not dragged | Duplicate slider properties and an `isClamped` check in the base class | The value field is a slider whenever `Clamp` is on, in edit and play mode. |
| The logger filled bottom-up and could not be cleared | Reverse order, Clear button | The Debugger's log is a table, newest first, with a Clear button. |
| Project code could not reuse the menu paths | `Config` made public | `Config` is public, in its own file. |
| Tools were scattered at the top of the Tools menu | `Tools/Ripple/` | Same. |
| `GameObjectVariableSO` missing from the wizard | `[RippleData]` added | The Debugger finds every Ripple asset by its base class, so no type can be left out. `[RippleData]` is obsolete since 0.3.0. |

Ripple's Unity floor is 6000.0, below the AVR projects (6000.1 and 6000.3). JamKit requires 6000.6 on its own; Ripple does not follow it. Keep Ripple free of APIs newer than its floor, and compile it against a 6000.0 editor before raising it.

The pull request also left TODOs to move the Odin attributes out of the runtime. Done in 0.3.0: the editor is UI Toolkit and nothing in the package references Odin.

Moving an AVR project to 0.2.0 also means re-wiring UltEvent responses on its event and listener assets, because UltEvent data does not convert to `UnityEvent`. That is the real cost of the upgrade and should be scheduled, not slipped into a hotfix.

## Direction

Reviewed on 2026-11-14, after fields that take any event were built.

**Where it is going.** Ripple started as variables and events wired in the inspector. With local variables and fields that take any event it is becoming the data layer for logic that designers build: a graph node holds a variable or event asset and resolves it against the object that runs it. That is the road Hipple took with declarations and an instancer. Ripple keeps one difference worth keeping: the asset is both the global and the declaration, so there is no second asset type to learn.

**Scope.** 0.3.0 was planned as five small candidates. It now also holds the editor rewrite, the removal of Odin and fields that take any event, roughly three times the change. All of it is tested by script in Unity, none of it by hand, and JamKit still pins 0.2.0 while its host project compiles this branch. **0.3.0 is frozen.** Anything not in the table below waits for 0.4.0.

**Type safety.** A field that takes any event moves one check from compile time to run time. A typed reference still refuses a value of another type at compile time, and a Signal's raise list warns in the inspector. A `GameEvent` field on a project's own script cannot be checked before it runs; the exception names both types.

**Limits to state, not to fix yet.**

- A local copy is a full ScriptableObject: right for tens of objects, too heavy for hundreds of bullets. A project with that many copies needs a lighter backing for them, not a second component.
- Debugging cost grows with how often a value changes. JamKit's `GameTimer` writes its variable every frame, which means a stack walk per change and 60 log entries a second. The log keeps the newest 10,000; `Disable Logging` on such a variable is the answer.

**Odin.** Ripple no longer needs it, so JamKit is the only reason a project does. Moving JamKit's own inspectors to UI Toolkit is a JamKit decision and is not scheduled.

## Next

### Left over from the 0.2.0 release

0.2.0 is on `main`, tagged `v0.2.0`. Its tests ran in Unity 6000.0.26f1 and its inspectors were checked in edit and play mode.

- Delete the remote branches `feature/jamkit`, `random-variables`, `feature/variable-declaration`, `feature/command-system`, `feature/refactor-v1` and `v2-redesign`. `feature/jamkit` is merged; the rest are kept as `archive/` tags.
- Close pull request 3 with a link to the table above.
- Delete `feature/abstraction+runtime-registry` once JamKit `main` and the GMTK 2026 project pin a tag instead.
- An AVR project moves to 0.2.0 when the UltEvent re-wiring is scheduled, not before.

### 0.3.0 release

Everything below is on `feature/0.3.0`.

| Candidate | What shipped |
| --- | --- |
| UI Toolkit editor | Every inspector, drawer and window. Layout in UXML documents the scripts reference, one stylesheet. Each document opens in UI Builder looking almost like the real inspector: editor theme, bound fields, `ReferenceField` and `SignalField` controls with stand-ins, states as root classes, and sample rows in the tables. |
| No Odin requirement | No Odin attributes in the runtime and no define constraint on the assemblies. |
| String and Color built in | Variable, reference, event, list and listeners for both. |
| Per-instance variables | `LocalVariables` component. It copies the variables it lists the first time something asks, so script order does not matter. `variable.For(this)` returns the copy, or the shared asset when there is none. `VariableListener` calls it in `Awake`. |
| Signals on lists | `OnItemAdded` and `OnItemRemoved` are `Signal<T>`. |
| Drag to listen | A drop handler for the Hierarchy and the inspector. Uses the `EntityId` handlers from Unity 6000.3 on, the older ones below. |
| One debug window | `Tools > Ripple > Debugger`. The Event Logger and the Wizard are removed. |
| Fields that take any event | `GameEvent` has `AddListener(Action)`, `RemoveListener(Action)` and `Raise(value)`. `Event Listener Void` and a Signal's raise list take any event. The inspector warns about an event a Signal cannot raise. |
| Vector2 complete | `Vector2VariableSO` and `Vector2Reference`. |
| Lists in the debug tools | Lists fill `Raised By` and the log with `+ item` and `- item`. `Listeners` shows the responses on both list Signals. |
| Fixes from the review | Clamping no longer throws on an inverted range. Setting a list item raises its events. `PreviousValue` starts at the start value. Local copies never persist. The log keeps the newest 10,000 entries. `Logger`, the connection registry and `IVariable` left the public API. |

Checked on 2026-11-14:

- 87 tests pass in Unity 6000.0.26f1 without Odin and in 6000.3.17f1 with Odin. They include one test per asset type that builds its inspector, one per document that fails on a `PropertyField`, and one per document that checks every table column has its cell template.
- The same 87 pass in a new 6000.0 project, which imports Ripple in the order a first install does. Its manifest leaves out the audio module, and Ripple's own dependency brings it in.
- A scripted run in 6000.0.26f1, with and without Odin, captured the inspectors and both windows in edit and play mode. It checked:
  - the live value of every built-in type in both directions;
  - the state classes as clamping, persisting and play mode change;
  - a reference switching between constant and variable;
  - the bound cells of every table;
  - the raise-list warning appearing and clearing;
  - the Debugger keeping its selection through a project change, a search and a click on a group;
  - fields that take any event in play mode, and both Raise buttons.
- A scripted run opened every inspector, window and preview document in UI Builder 6000.0.26f1 with the editor theme. None has a `PropertyField`, the controls show their stand-ins, and the two previews fill their tables from the sample asset.
- The package compiles against 6000.5.7f1 in the JamKit host project with JamKit on top, and the runtime compiles without the editor defines.

To release:

1. Try what only a mouse can check:
   - Drag an event asset onto a GameObject.
   - Open the menu at the end of a reference field.
   - Click a line of a call stack in the Debug foldout.
   - Pick an `IntEvent` into the `Also Raise` list of a float Signal by hand, and see the warning.
   - Double-click a row of the Debugger's log.
   - Edit a document in UI Builder and save it. Check that the Builder keeps the `ui:Template` lines that declare cell and row documents.
2. Merge `feature/0.3.0` into `main`, tag `v0.3.0`, delete the branch.
3. In JamKit:
   - Pin `v0.3.0`.
   - Let `Health` resolve its variable with `CurrentVariable.For(this)`, so the "leave empty when several objects share this prefab" caveat goes away.
   - Make a Game Flow transition's `On` a `GameEvent`, so any event can drive a scene change.
   - Have the wizard tick `Disable Logging` on the timer's variable, which changes every frame.

### Proposed for 0.4.0, waiting for a yes

These come from comparing Ripple with Ryan Hipple's own system (variable declarations, an instancer, and a value that is a literal, a global variable or an instanced variable) and from asking where interfaces with `[SerializeReference]` would help. A fourth proposal, fields that take any event, was built in 0.3.0. None of these three is built.

**Local variables with their own starting value.** Hipple's instancer lets each object override the initial value (100 health on one enemy, 300 on the boss). `LocalVariables` copies the asset as it is.

- Each entry of `LocalVariables` becomes the variable plus an optional starting value, edited inline next to it.
- This is where `[SerializeReference]` earns its place: the value's type differs per entry, so the entry holds a box the variable creates (`Override<T>`). Unity 6000.0 serializes generic managed references, object references inside them included; that was checked in the test project.
- The variable asset stays the key. There is no separate declaration type: an asset is global until an object lists it.

**References that follow local variables.** Hipple's value type has three modes. Ripple needs no third one, because the same asset is local wherever a `LocalVariables` component lists it. Smallest form: `reference.For(this)` in `Awake`, like `variable.For(this)`.

**Graph nodes on local variables.** With the two above and fields that take any event, a Graph Toolkit node can hold a variable or event asset as an option and resolve it against the object that runs the graph: "if this enemy's `Health` is below 50". The asset is the key and the runner's `LocalVariables` is the instancer. The nodes belong in JamKit, next to Game Flow. Ripple only has to keep `For` cheap.

### After 0.3.0

Nothing here is scheduled. Each item is a gap seen while building 0.3.0 and waits for a project that hits it.

- **Live values of custom types.** In play mode the value field edits the built-in types. A type from the Type Generator shows its `ToString()` and cannot be edited there.
- **An empty reference in variable mode reads "None (Base Variable`1)".** That is how Unity names the generic field type.
- **List items edited in the inspector.** In play mode they change without raising `OnItemAdded` or `OnItemRemoved`. Code that sets an item does raise them.
- **Code subscribed through a Signal.** `Listeners` shows code that subscribed with `AddListener`. Code that used `+=` on a Signal, such as `variable.OnValueChanged += …`, is not listed.
- **Variables keep code listeners across play sessions** when domain reload is off. Events have `Clear Listeners On Play`; variables do not.
- **The Debugger's asset tree is empty in UI Builder.** Code fills it, so the preview shows the log only. `Editor/Rows/AssetRow.uxml` shows one row on its own.

### Not doing

Channels and dispatchers, command assets, generated code for built-in types, diagnostics sinks, validators and traits, snapshot managers, per-frame coalescing, a settings provider, random references.

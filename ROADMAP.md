# Ripple roadmap

## What Ripple is

State and events as ScriptableObject assets, wired in the inspector, with enough debugging to answer "who changed this?" without a debugger. Nothing else.

Rules for any change:

- **One way to do a thing.** One event type (`Signal`), one variable base, one listener per kind. A second mechanism for the same job is removed, not kept for compatibility.
- **Use Unity before writing anything.** `UnityEvent`, `TypeCache`, UI Toolkit bindings, the console's stack trace links.
- **Small enough to read in one sitting.** The package is about 2,100 lines. A feature that needs its own subsystem (dispatcher, sink, registry of registries) is the wrong shape.
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
| `feature/refactor-v1` | Color, String and Quaternion types; event groups; settings provider; factory; `Unit`; reference-usage search; richer logger; tests | Types: covered by the Type Generator, with String and Color built in since 0.3.0. Event groups: covered by a Signal's raise list. Usage search: covered by `Scan Listeners`. Tests: kept as an idea, `Tests/` now exists. The rest dropped. |
| `v2-redesign` (and the unpushed "v3 overhaul" commit) | Ground-up rewrite: channels and a dispatcher, generated code for every built-in type, diagnostics sinks, traits and validators, snapshots, per-frame coalescing, instancers, a debugger window, a project validator | Dropped as a whole. It is 6,000 lines against 2,000 to deliver the same variables and events. Three ideas survive in smaller form, all in 0.3.0: instancers as `LocalVariables`, one debugger window, and drag-an-asset-onto-an-object to create its listener. |
| `feature/abstraction+runtime-registry` | Listener scan, runtime subscriber registry, PlayerPrefs persistence, draggable reference label | Merged. All of it is in 0.2.0. |

## What AVR Japan needed

Pull request 3 (`XRJapan:main` into `main`, "Sync hotfixes") is 22 commits of fixes made under project pressure. It is not merged. Each problem it solved is listed here with how 0.2.0 solves it.

| Problem at AVR | In the pull request | In 0.2.0 |
| --- | --- | --- |
| Runtime values were written into variable assets and showed up in version control | Stopped serializing the current value, marked assets dirty on reset | The current value is `[NonSerialized]` for variables and lists. Nothing to mark dirty. |
| No way to restart a session without reloading | `BaseVariable.ResetAllVariables()` over a static instance list | `BaseVariable.ResetAll()` and a public `ResetValue()`. Lists are included. Listeners are notified, so UI follows. |
| "Who changed this?" showed one method name, often a helper | A 30-frame stack trace string per invoke, a `context` argument, a `[ThreadStatic] CurrentInvoker` | `sender` argument on `SetCurrentValue` and `Invoke`. The full stack is captured once per distinct caller and logged with clickable links by the `Trace` button. No depth limit, no thread-static state. |
| Every custom type needed five hand-written classes | Type Generator window | `Tools > Ripple > Type Generator`, using Odin's own type picker (the slow hand-built dropdown was the pull request's first attempt). Also generates the list type. |
| IL2CPP builds failed on `dynamic` in `Add` | Per-type `Add` overrides | Already on `main`: `Add` is abstract and each numeric type implements it. |
| A build failed on editor-only code | Hotfix | The runtime assembly is compiled without `UNITY_EDITOR` as part of the check before each commit. |
| Lists could not be resized or filled in bulk | `AddRange`, `ResizeRange` | `AddRange`, `Resize`. Every mutation raises the item events. |
| Clamped numbers were typed, not dragged | Duplicate slider properties and an `isClamped` check in the base class | One slider property on `NumericalVariable`. |
| The logger filled bottom-up and could not be cleared | Reverse order, Clear button | Same, and the window no longer edits Unity's shared label style or leaks its subscription. |
| Project code could not reuse the menu paths | `Config` made public | `Config` is public, in its own file. |
| Tools were scattered at the top of the Tools menu | `Tools/Ripple/` | Same. |
| `GameObjectVariableSO` missing from the wizard | `[RippleData]` added | Added, and on the four event types that also lacked it. |

Ripple's Unity floor is 6000.0, below the AVR projects (6000.1 and 6000.3). JamKit requires 6000.6 on its own; Ripple does not follow it. Keep Ripple free of APIs newer than its floor, and compile it against a 6000.0 editor before raising it.

One thing the pull request points at is **not** solved:

- **Odin as a hard dependency.** The pull request leaves TODOs to move Odin attributes out of the runtime. Not planned. It becomes worth doing only if a project cannot have Odin.

Moving an AVR project to 0.2.0 also means re-wiring UltEvent responses on its event and listener assets, because UltEvent data does not convert to `UnityEvent`. That is the real cost of the upgrade and should be scheduled, not slipped into a hotfix.

## Next

### Left over from the 0.2.0 release

0.2.0 is on `main`, tagged `v0.2.0`. Its tests ran in Unity 6000.0.26f1 and its inspectors were checked in edit and play mode.

- Close pull request 3 with a link to the table above.
- Delete `feature/abstraction+runtime-registry` once JamKit `main` and the GMTK 2026 project pin a tag instead.
- An AVR project moves to 0.2.0 when the UltEvent re-wiring is scheduled, not before.

### 0.3.0 release

Everything below is on `feature/0.3.0`. Its tests pass in Unity 6000.0.26f1 and 6000.3.17f1.

| Candidate | What shipped |
| --- | --- |
| String and Color built in | Variable, reference, event, list and listeners for both. |
| Per-instance variables | `LocalVariables` component. It copies the variables it lists the first time something asks, so script order does not matter. `variable.For(this)` returns the copy, or the shared asset when there is none. `VariableListener` calls it in `Awake`. |
| Signals on lists | `OnItemAdded` and `OnItemRemoved` are `Signal<T>`. |
| Drag to listen | A drop handler for the Hierarchy and the inspector. Uses the `EntityId` handlers from Unity 6000.3 on, the older ones below. |
| One debug window | `Tools > Ripple > Debugger`. The Event Logger and the Wizard are removed. |

To release:

1. Drag an event asset onto a GameObject in the editor once. The handler was run from a script with every asset type, but not with a real mouse drag.
2. Merge `feature/0.3.0` into `main`, tag `v0.3.0`, delete the branch.
3. In JamKit, pin `v0.3.0` and let `Health` resolve its variable with `CurrentVariable.For(this)`, so the "leave empty when several objects share this prefab" caveat goes away.

### After 0.3.0

Nothing here is scheduled. Each item is a gap seen while building 0.3.0 and waits for a project that hits it.

- **References and local variables.** A `FloatReference` that points at a variable reads the shared asset, also below a `LocalVariables` component. Smallest form: `reference.For(this)`.
- **Vector2 has an event and a variable listener but no variable.** Add `Vector2VariableSO` or remove `VariableListenerVector2`.
- **Lists in the debug tools.** A list does not fill `Invoked by`, and `Scan Listeners` does not read the responses wired on its two Signals.
- **Odin visibility attributes.** Odin applies the last visibility attribute on a member, so two of them on one member silently cancel. Keep one condition per member.

### Not doing

Channels and dispatchers, command assets, generated code for built-in types, diagnostics sinks, validators and traits, snapshot managers, per-frame coalescing, a settings provider, random references.

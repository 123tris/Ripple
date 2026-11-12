# Ripple roadmap

## What Ripple is

State and events as ScriptableObject assets, wired in the inspector, with enough debugging to answer "who changed this?" without a debugger. Nothing else.

Rules for any change:

- **One way to do a thing.** One event type (`Signal`), one variable base, one listener per kind. A second mechanism for the same job is removed, not kept for compatibility.
- **Use Unity before writing anything.** `UnityEvent`, `TypeCache`, UI Toolkit bindings, the console's stack trace links.
- **Small enough to read in one sitting.** The package is about 2,000 lines. A feature that needs its own subsystem (dispatcher, sink, registry of registries) is the wrong shape.
- **A feature lands when a real project needed it**, and in the smallest form that solves that project's problem.
- **No comments.** Names carry the meaning. Reasons go in the changelog.

## Branches and tags

| Ref | What it is |
| --- | --- |
| `main` | 0.1.4, the UltEvents line. Projects that install Ripple without a branch get this. Tagged `v0.1.4`. |
| `feature/jamkit` | 0.2.0, the current line. JamKit pins it. Becomes `main` at the 0.2.0 release. |
| `feature/abstraction+runtime-registry` | Fully contained in `feature/jamkit`. Kept only because JamKit `main` and the GMTK 2026 project still pin it. Delete once neither does. |
| `archive/*` tags | The retired experiments below. Restore one with `git switch -c <name> archive/<name>`. |

Work happens on short-lived branches off the current line and merges back. Experiments that are not going to merge get an `archive/` tag and the branch is deleted.

## What the old branches tried

Every branch below was an attempt at a real need. The need is kept where it holds up. The implementation is not.

| Branch | Idea | Verdict |
| --- | --- | --- |
| `random-variables` | A reference that returns a random value in a range | Dropped from Ripple. A reference is "constant or variable"; a third mode makes every reference field wider. If a jam needs it, it is a small struct in JamKit. |
| `feature/variable-declaration` | Per-object variables: a declaration asset plus an instancer component holding a value per GameObject | **Need kept**, see "Per-instance variables" below. The implementation stored untyped `object` values in a dictionary. |
| `feature/command-system` | `[SerializeReference]` command lists with conditions, run from an asset | Dropped. A `Signal` already runs a list of actions from the inspector, and branching belongs in code. |
| `feature/refactor-v1` | Color, String and Quaternion types; event groups; settings provider; factory; `Unit`; reference-usage search; richer logger; tests | Types: covered by the Type Generator, with String and Color to ship built in. Event groups: covered by a Signal's raise list. Usage search: covered by `Scan Listeners`. Tests: kept as an idea, `Tests/` now exists. The rest dropped. |
| `v2-redesign` (and the unpushed "v3 overhaul" commit) | Ground-up rewrite: channels and a dispatcher, generated code for every built-in type, diagnostics sinks, traits and validators, snapshots, per-frame coalescing, instancers, a debugger window, a project validator | Dropped as a whole. It is 6,000 lines against 2,000 to deliver the same variables and events. Three ideas survive in smaller form: instancers (below), one debug window (below), and drag-an-asset-onto-an-object to create its listener (below). |
| `feature/abstraction+runtime-registry` | Listener scan, runtime subscriber registry, PlayerPrefs persistence, draggable reference label | Merged. All of it is in `feature/jamkit`. |

## What AVR Japan needed

Pull request 3 (`XRJapan:main` into `main`, "Sync hotfixes") is 22 commits of fixes made under project pressure. It is not merged. Each problem it solved is listed here with how the current line solves it.

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

Two things the pull request points at that are **not** solved:

- **Unity version.** 0.2.0 declares Unity 6000.6. The AVR projects are on 6000.1 and 6000.3. Ripple's code needs nothing newer than 6000.0, so the floor can be lowered in `package.json` without touching code.
- **Odin as a hard dependency.** The pull request leaves TODOs to move Odin attributes out of the runtime. Not planned. It becomes worth doing only if a project cannot have Odin.

Moving an AVR project to 0.2.0 also means re-wiring UltEvent responses on its event and listener assets, because UltEvent data does not convert to `UnityEvent`. That is the real cost of the upgrade and should be scheduled, not slipped into a hotfix.

## Next

### 0.2.0 release

1. Open the host project in an editor that satisfies the package floor and run the `Metz.Ripple.Tests` assembly. Nothing in 0.2.0 has run inside Unity yet; it is compile-checked only.
2. Look at three inspectors by eye: a `Signal` field, a clamped `FloatVariableSO` (slider), and the `Invoked by` table with its `Trace` button.
3. Merge `feature/jamkit` into `main`, tag `v0.2.0`, point JamKit at the tag.
4. Projects that install Ripple with no branch follow `main` and will pick up 0.2.0 on their next package update. Pin them to `#v0.1.4` first if they should stay on UltEvents.
5. Delete `feature/jamkit` and `feature/abstraction+runtime-registry`.
6. Close pull request 3 with a link to the table above.

### 0.3.0 candidates, in order

- **String and Color built in.** Generate them with the Type Generator and commit the result. They are the two types every project adds first.
- **Per-instance variables.** The need behind `variable-declaration` and the v2 instancers: ten enemies cannot share one `Health` asset. Smallest form: a component that clones the variable assets it lists at `Awake` (`Instantiate(asset)`), and sibling components read the clone through it. No declaration assets, no untyped storage. Design it against JamKit's `Health` before writing it.
- **Signals on lists.** `OnItemAdded` and `OnItemRemoved` as `Signal<T>` so a list change can raise an event from the inspector, as a variable change already can.
- **Drag to listen.** Dropping an event or variable asset on a GameObject adds the matching listener component with the asset assigned.
- **One debug window.** Fold the Event Logger and the wizard into a single window with a filter field, instead of growing either.

### Not doing

Channels and dispatchers, command assets, generated code for built-in types, diagnostics sinks, validators and traits, snapshot managers, per-frame coalescing, a settings provider, random references.

# Player-Triggered Power-Test Preflight — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#123](https://github.com/cixanla/PC-Shop-Empire-3D/issues/123)<br>
**Draft PR:** [#124](https://github.com/cixanla/PC-Shop-Empire-3D/pull/124)<br>
**Parent branch head:** `48e1c8d6278c9c255cc3c49e5e88d05db85da9a1` — Issue #121 docs checkpoint<br>
**Technical head:** `3c26ce0d6de80c975b064f2dff68d96fbd4378bc`<br>
**Technical tree:** `58dd983e314ecb78d94b3871dc672641e0a87b5d`<br>
**Technical branch:** `codex/issue123-power-test-preflight-attempt`<br>
**Current state:** Exact source, authority/context/receipt contracts, committed scene/input integration, targeted/full Mac tests, universal native build, Apple M1/Metal player smoke, independent review fixes, repository guard, codesign and user-setting preservation pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because those devices are unavailable. Issue #123 and its Roadmap card remain `OPEN / In Progress`; PR #124 remains draft and no merge or closure is claimed.

## Delivered result

`PowerTestAttemptAuthority` owns only the historical fact that the player requested and passed one preflight for an exact electrical configuration. Creation is reference-bound to the same power-budget and Assembly authorities; null, foreign and mismatched configuration fails closed.

The captured immutable context includes:

- exact build and chassis identity;
- seven installed component item identities and their seven retain/secure operation identities;
- ATX24, EPS12V and PCIe/GPU cable item and route-operation identities;
- Assembly plus all three cable revisions;
- all seven installed product IDs, exact electrical-policy ID and individual load values;
- `380 W` draw, `500 W` minimum recommendation, installed `550 W` PSU and `+50 W` margin.

The first accepted command requires the stable prototype operation ID, expected attempt revision and current exact context. The authority recomputes current electrical readiness and power budget before accepting; no UI boolean or caller-provided success can create a receipt.

Exactly one immutable `PreflightReady` receipt is stored. Exact replay returns the same object instance. Different lineage under the same operation ID conflicts, and a second distinct completion is rejected. Historical exact replay intentionally remains valid after a cable reroute, while `EvaluateCurrentReceipt()` separately returns `ContextStale`. The old receipt is never rewritten to match the new state.

Inventory, BuildKit, reservation, custody, Assembly, all three cable authorities, quote, work order, Economy and benchmark state remain unchanged. Session invariants validate receipt history only when the optional authority already exists, avoiding query-time state creation.

## Player interaction and presentation

The committed GarageGraybox r60 scene reuses the existing Workbench status/focus surface. No new gameplay collider, renderer, light, camera, NavMesh obstacle, physical item or second Assembly authority exists.

Keyboard/mouse `E` and virtual-gamepad South use the normal Interact action and one command path. The station rejects:

- paused and pause+Interact co-edge frames;
- out-of-range, off-focus and occluded attempts;
- carried item, driven cart or Assembly prompt ownership;
- a competing world Interact owner;
- same-frame replay and a completed/stale receipt.

Interact is consumed only after every gate succeeds. Line of sight uses a fixed `16`-entry `RaycastNonAlloc` buffer, ignores only the player hierarchy, treats every other hit as obstruction and fails closed on capacity saturation. Repeated prompt reads in one frame reuse one cached string/context result; actual command execution always reads fresh authority state.

Visible states are exact:

```text
E / A: GÜÇ TESTİ ÖN KONTROLÜNÜ ÇALIŞTIR
ÖN KONTROL GEÇTİ • POWER-ON BEKLİYOR
ÖN KONTROL GEÇERSİZ • assembly.power-test-attempt.context-stale
```

No state in this issue energizes the PC or claims connector electrical correctness, fault detection, POST, BIOS/UEFI, OS, driver, benchmark, packaging, delivery or settlement.

## Independent review and corrections

A bounded, read-only post-implementation review identified three actionable findings before the final verification chain:

1. Historical exact replay was incorrectly revalidating current context and could return `ContextStale`. Replay lookup now returns the stored same-instance receipt before current-context validation; changed-command reuse still conflicts. A dedicated domain test and native smoke assertion cover replay after stale.
2. The new authority was absent from the session invariant chain. `ValidateInvariants()` now validates receipt history only when the authority already exists and does not instantiate it as a side effect.
3. Repeated HUD reads in the same frame rebuilt context, snapshots and prompt strings. The projection now caches the prompt per frame and invalidates on configuration, input processing and remembered result. The PlayMode test requires same-reference reuse.

The follow-up review found no remaining P0, P1 or P2 issue in receipt identity/replay/current-stale separation, authority mutation boundaries, input ownership, line of sight, allocation behavior, scene wiring or test coverage.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Domain authority plus committed scene contracts | `6/6` | `9.5041649 s` | `9,129` | `311dafa0eeabf5c81c44b4023edbf43f5bc53691a5d2bbbf08d9672219904375` |
| Keyboard/mouse, virtual-gamepad and presentation contracts | `3/3` | `9.5907737 s` | `13,007` | `3cb9af8fa75e7a75317b864236a2bf3f03dfef65f5f74de0dd02c3edccd11eb9` |
| Full EditMode | `773/773` | `58.7978954 s` | `641,001` | `93f6f4fd8ff420c67dcb4f9410dd4798fc94bea2ae3cd2c0d51b5e289e7ed955` |
| Full PlayMode | `161/161` | `703.9726667 s` | `542,922` | `8f67eef79062acbf827b97f4b13c2fdb35984ac7b7474eebc14b1a8bfbb4223b` |

Every XML reports failed, skipped and inconclusive `0`.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Full EditMode | `38,384` | `89b44f3db6d4a89f4d55c5d0e17e9edc1e93fa014e36f268da49463d67d196c2` |
| Full PlayMode | `650,497` | `74ac397a23b65cd2b5e1bda1aa7a8306a86ed04739d5bfa71135a6a06c920bc3` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `602,525` | `2cdb96cddc3f081b23e6eecd5c2a8911e797d642c015ab39cf36201f7436cad2` |
| Universal app executable | `117,179` | `c39ab49b5177b05935a18cc93e7e05d3327ba91c59405b10a421f6c13f558c1f` |
| Apple M1/Metal power-test-preflight runtime log | `9,425` | `24cdc6a92d06309605fa49ae1e0039059990c15143fe72e1795072a6474cfffb` |

The Unity marker reports `330,507,808` build bytes. The app contains `302` files; its executable is universal Mach-O `x86_64 + arm64`, and `codesign --verify --deep --strict` passes.

The native player runs graphically on Apple M1/Metal and emits exactly one success marker:

```text
GARAGE_POWER_TEST_PREFLIGHT_RUNTIME_SMOKE prerequisite-setup=assisted electrical-readiness=ready power-budget=380/500/550 station=existing-focus-surface input=keyboard+gamepad single-consumer=ok range=ok focus=ok los=ok pause=ok co-edge=ok receipt=immutable replay=ok stale=detected attempt-mutation=zero benchmark=untouched presentation=ok power-on=not-started invariants=ok
```

The player exits `0`, reaches graceful Input System shutdown and leaves player/Unity/shader/IL2CPP residue `0`.

## Settings, repository and raw evidence

The Issue #123 source commit contains exactly `25` related source, scene and test paths. No ProjectSettings or Packages file is staged or committed. The only local path differing under those roots is the pre-existing user/editor-owned ProBuilder setting, still SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it remains unstaged and unreverted.

Repository Guard run [33357285973](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33357285973) passed on technical source `3c26ce0`. `git diff --check` and the explicit staged manifest passed. Canonical raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue123-power-test-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, screenshot, process/task/firewall cleanup or evidence readback claim is made for Issue #123. UTM was deliberately not used as replacement evidence. Closure requires a clean checkout of the exact accepted commit/tree, full EditMode/PlayMode, native preflight smoke, fatal-token audit, readback and zero final residue.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity and health verification, exact accepted source/evidence and complete readback. Absence is not treated as pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the current claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #123 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Dedicated authority is reference-bound to exact canonical power-budget and Assembly authorities. | PASS — domain and invariant contracts |
| 2 | Command binds stable operation ID, expected attempt revision and current exact Assembly/cable/budget lineage. | PASS — domain contracts |
| 3 | Success publishes one immutable receipt with exact identity, operations, revisions, products, policy and wattage. | PASS — domain plus native smoke |
| 4 | Exact replay is same-instance and immutable; changed reuse conflicts; second completion is blocked. | PASS — post-review regression and native smoke |
| 5 | Blocked/stale/insufficient states create no receipt or gameplay-authority mutation. | PASS — domain, input and native invariants |
| 6 | Existing Workbench surface is reused with no new gameplay geometry, item or second authority. | PASS — committed scene and hero regression contracts |
| 7 | Keyboard/mouse and virtual-gamepad use the same gated, single-consumer Interact command. | PASS — PlayMode and native dynamic-frame smoke |
| 8 | Presentation observes current validity while historical replay remains immutable after lineage drift. | PASS — post-review domain/input/native regressions |
| 9 | Targeted/full suites, universal Mac native and clean physical Windows IL2CPP/D3D11 runtime pass. | PARTIAL — Mac PASS; physical Windows DEFERRED |

Issue #123 remains open and its Roadmap card In Progress until physical Windows validation and later integration/closure steps complete. PR #124 is draft and intentionally does not auto-close the issue while acceptance #9 is partial.

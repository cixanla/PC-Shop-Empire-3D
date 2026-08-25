# Canonical DDR5 Memory-Module Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#73](https://github.com/cixanla/PC-Shop-Empire-3D/issues/73)<br>
**Technical head:** `a2df663d6fa0e9d2004697bfb038a65a5e6c3d81`<br>
**Technical tree:** `e32a8e143049c4059e402bafbfcd39b9760cd025`<br>
**Closure status:** source/domain/scene/input/full-regression and macOS native gates passed; exact-head Windows, final docs/CI, immutable package, physical USB double-readback and Issue/Project lifecycle gates pending

## Delivered playable result

GarageGraybox r37 adds the third physical component handoff for the accepted custom-PC work order. The domain resolves the canonical DDR5 module by `ComponentKind.MemoryModule` and the exact work-order/ticket/allocation line, product, serialized item and reservation tuple. The player takes that exact object using real `E / Gamepad South`, carries it, toggles the keyed 180° orientation and places it into a memory-specific capacity-one managed BuildKit slot.

The authoritative custody chain is source → `ActorHands` → memory-module BuildKit. Both motherboard and processor prerequisites remain staged in their own slots, reservation/allocation identity stays live, and progress becomes exact `3/10`. World mutation occurs only after domain success. Placement recovery, exact replay and failure paths keep the same Unity component and stable `ItemId`; generic transfer/drop/stack/cart and A2/dual-latch Assembly paths cannot bypass active BuildKit custody.

The exact runtime success marker is:

```text
GARAGE_MEMORY_MODULE_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor-staged memory-pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=180 placement=ok progress=3/10 reservation=alive custody=memory-module-build-kit receipts=ok revisions=ok assembly=untouched dimm-a2=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

## Exact source and full regression

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 680/680 | `editmode.xml` | 567,569 | `c0b30ade908ac40cdf538d947ecdacee239d2f2b145b150bdbe61e316d8a0746` |
| PlayMode | 86/86 | `playmode.xml` | 231,398 | `0abbb8abf177f8dbe7f130c47869b8a390614c891c3cb4ddd171c18789bf5981` |

Both suites report failed, skipped and inconclusive `0`. The final runs created one ProBuilder preference entry and one untracked scene-template settings file; both were generated editor state, were removed without touching game source, and the committed technical tree is clean. `git diff --check` passes.

Targeted evidence before the full suites additionally passed:

- BuildKit domain/replay/forgery class: `18/18`.
- Memory-module gamepad pause/co-edge release-repress gate: `1/1`.
- GarageGraybox r37 scene contract: `9/9`.
- Runtime-smoke compile gate: `1/1`.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development/StrictMode build | Success | `macos-build.log` | 599,941 | `13daa3827b33c45ad5bed90492b9d3b26a9e53d0d38f58bbd47e84a931aeaba6` |
| Apple M1/Metal native runtime | Success | `macos-runtime.log` | 8,338 | `288ba169623c35b68262a21f1e82b1cfc630bc594a2e605c7cba24dc3ffcc740` |
| App executable | Universal + valid ad-hoc signature | `PC Shop Empire 3D` | 117,179 | `fe7e568f8417f5d7082296e0d8efd63b9ed4fe3ac999de621c7768c62c1a04c1` |

The build report is `329,681,642` bytes. `file` confirms `x86_64` and `arm64`; deep/strict `codesign --verify` passes. The native player forced Apple M1/Metal, emitted exact r37 readiness and exact memory-module BuildKit success once, emitted no BuildKit failure/assertion/unhandled-exception marker and left no player process. The runtime-reported window was `640×480`; no unsupported 1280×720 claim is made for this batchmode run.

## Windows exact-source IL2CPP and Direct3D11 gate

The exact technical branch was packaged as one complete Git bundle: `6,839,312` bytes, SHA-256 `262ef6817456538230a37ae72d308d337fec854c47d0129afb97623929d40eac`. The bundle exposes only `feature/issue73-ddr5-build-kit-handoff` at the technical head and passes `git bundle verify`.

The Windows procedure source is based on the accepted Issue #71 hardened-v2 pattern, retargeted to Issue #73 without altering older evidence. It requires a collision-free detached clean clone, byte-exact `ProjectSettings.asset` restoration, x64 IL2CPP with only Direct3D11, a broadened Burst/native-link fatal policy, three native-binary hashes, an interactive logged-on-user runtime, one exact host/readiness/success marker, zero forbidden tokens, graceful shutdown, task deletion and zero process residue.

Current state: the fresh Windows validation root and build are in progress. No Windows success claim is made until the produced evidence returns to the Mac and passes full hash/procedure/source readback.

## Canonical Issue #73 acceptance matrix — current technical state

`TECHNICAL PASS` means source/tests/Mac evidence currently proves the item. `WINDOWS PENDING` or `LIFECYCLE PENDING` means the item is deliberately not closed yet.

| # | Acceptance contract | Current gate | Evidence |
|---:|---|---|---|
| 1 | Exact MemoryModule role and full line/product/item/reservation tuple. | TECHNICAL PASS | `CustomPcBuildKitAuthority`, allocation cross-check and exact-line tests. |
| 2 | No ordinal/display-name/product-value/regenerated identity authority. | TECHNICAL PASS | Wrong-line, value-equal and regenerated-identity no-mutation tests. |
| 3 | Separate stable operation and capacity-one DIMM BuildKit slot. | TECHNICAL PASS | Append-only IDs, third managed container and capacity tests. |
| 4 | Motherboard/CPU slots, receipts, replay, revisions and staged state unchanged. | TECHNICAL PASS | Three-component aggregate preservation assertions. |
| 5 | Immediate/delayed replay returns one canonical receipt. | TECHNICAL PASS | Pickup/place history/replay matrix. |
| 6 | Foreign/value-equal/wrong-kind/line/product/item/reservation/order/operation fails closed. | TECHNICAL PASS | Expanded exact forgery matrix. |
| 7 | Full hands, occupied slot, source drift and stale revisions are no-mutation. | TECHNICAL PASS | Conflict/capacity/stale tests. |
| 8 | Custody is only source → hands → DIMM BuildKit; generic bypasses fail. | TECHNICAL PASS | Narrow Inventory bridge and bypass tests. |
| 9 | Reservation and allocation remain exact/live. | TECHNICAL PASS | Receipt identity assertions and native marker. |
| 10 | Motherboard+CPU prerequisites prevent skipping `2/10`. | TECHNICAL PASS | Prerequisite no-mutation tests and smoke setup. |
| 11 | Real `E / Gamepad South` pickup preserves range/focus/LOS/empty hands. | TECHNICAL PASS | Keyboard/mouse and gamepad PlayMode paths. |
| 12 | Separate DIMM target, one support collider, exact anchor, gates and 180° preview. | TECHNICAL PASS | Scene contract and projection tests. |
| 13 | BuildKit contextual input is single-consumer; A2 cannot steal same frame. | TECHNICAL PASS | Receipt-owned arbiter and A2 isolation tests. |
| 14 | Co-edge, held, pause and release-repress are deterministic. | TECHNICAL PASS | Keyboard/gamepad pause/co-edge matrix. |
| 15 | Domain failure leaves the same DIMM in hands before world mutation. | TECHNICAL PASS | Preflight/domain-failure snapshots. |
| 16 | Physical failure recovers the same instance at the exact kit pose. | TECHNICAL PASS | Forced `PlaceAt` recovery test and no-clone invariant. |
| 17 | Unity instance, ItemId, collider/layer/ownership and container stay exact. | TECHNICAL PASS | Binding/scene/runtime identity assertions. |
| 18 | Visible progress derives from staged receipts as `2/10 → 3/10`. | TECHNICAL PASS | Aggregate authority, projection and runtime marker. |
| 19 | A2 Assembly, motherboard/CPU Assembly, electrical, price and seven reservations untouched. | TECHNICAL PASS | Revision/receipt/item snapshots and native isolation marker. |
| 20 | WASD/mouse-look and keyboard/gamepad carry flow work in human-shaped scenarios. | TECHNICAL PASS | Existing locomotion regression plus real Input System BuildKit matrix and native input route. |
| 21 | Domain/scene/input matrices and full regressions pass. | TECHNICAL PASS | EditMode `680/680`, PlayMode `86/86`. |
| 22 | Diff/Guard/Mac/Windows native gates pass. | WINDOWS PENDING | Diff and Mac pass; exact-head Windows and final Guard remain open. |
| 23 | Docs, private push/CI and physical USB lifecycle complete. | LIFECYCLE PENDING | Technical branch pushed; final CI/package/USB/Issue metadata remains open. |

## Remaining closure sequence

1. Finish exact detached-clean Windows build and interactive Intel Iris Xe/Direct3D11 runtime; promote procedure-bound evidence back to the Mac.
2. Bind exact Windows hashes and runtime/task receipts into this document and the project handoff/checkpoint records.
3. Commit/push final source/docs metadata and require Repository Guard/CI success.
4. Rediscover the correct external physical USB identity and previous milestone chain. Copy only to a collision-free `.incoming-*` target, remove AppleDouble only inside that new target, perform full path/size/SHA readback, atomically rename, and repeat the full readback.
5. Only after every gate passes, mark all `23/23` acceptance boxes, close Issue #73 and set its Roadmap item to `Done`.

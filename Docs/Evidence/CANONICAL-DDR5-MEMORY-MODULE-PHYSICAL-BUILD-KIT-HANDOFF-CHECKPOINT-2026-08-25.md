# Canonical DDR5 Memory-Module Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#73](https://github.com/cixanla/PC-Shop-Empire-3D/issues/73)<br>
**Technical head:** `a2df663d6fa0e9d2004697bfb038a65a5e6c3d81`<br>
**Technical tree:** `e32a8e143049c4059e402bafbfcd39b9760cd025`<br>
**Closure status:** source/domain/scene/input/full-regression plus exact-head macOS and Windows native gates passed; final procedure-bound provenance/CI, immutable package, physical USB double-readback and Issue/Project lifecycle gates pending

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

The collision-free validation root was `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue73-a2df663d6fa0-hardened-v2`. It remained detached, clean and byte-exact to technical commit/tree before and after build/runtime.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/Direct3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,064 | `bc48cdaaa0db73fe5d849647c43cddc7c5663cf4d7617e4e50b8ef9bd1cef2aa` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `dbed88819472b67dcd358bed8188b4124ffff1eb73cfdf6f69c832d544ce442f` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,393 | `f84ffd67c8941828518135fa172d2a9e87ca06655f78c1582fb83866e9bb7f81` |
| Runtime summary | Exit/task/residue contract passed | `runtime-summary.json` | 3,294 | `41d4796e7a763c27d48c93e7df0657a8e09f2a3cbbce05d707e43da651e5b19b` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,702 | `41935fe07ecb2c2d1e67a43fe2daecdd171f14b9d28eac9520a062f3e1ce3a26` |

The Windows build report is `1,330,930,513` bytes. The expanded hardened-v2 Burst/native-link fatal-token count is `0`, and `ProjectSettings.asset` restoration is byte-exact. The binary manifest locks `PC Shop Empire 3D.exe` (`667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`), `GameAssembly.dll` (`45,072,384` bytes, `7a66081460ae43af8c0d852105ebbe609976579b4672015e02c4cdbe12eefa8d`) and `UnityPlayer.dll` (`84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`).

The logged-on-user runtime reported Intel Iris Xe, Direct3D 11.0 feature level 11.1 and `1280×720`. Exact host, r37 readiness and memory-module BuildKit success markers each appeared once; forbidden markers were `0`. The player exited `0`, graceful shutdown was true, the scheduled task was deleted, cleanup was unnecessary, process residue was `0`, and the detached source remained clean.

Procedure source is itself hash-bound:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,296 | `1ba847cf7579a8456ac8bd8252a762e7b639698fa75fbd9e53a17cd10d4b5401` |
| `launch-procedure.ps1` | 7,963 | `69d9ffcdd0928f03fbfe215466a03f58545d2f99ce84821c229f2ce2c53f81a7` |
| `runtime-procedure.ps1` | 13,965 | `59dc9e8bdbddea50d668a0ffe1c2b56b05f00f9c754b3aeba75ccacff803b03d` |
| `procedure-manifest.json` | 670 | `fb322e4d8da56008e5cee4e026867dd203fe12b92e46fd1d72ef10d7c5ea851f` |

The thirteen immutable test/build/runtime/procedure artifacts have returned to the Mac under an incoming evidence root with exact size/hash readback. The fourteenth `source-receipt.json` is deliberately generated only after the final source/docs commit is clean and its Repository Guard succeeds; therefore canonical promotion and full lifecycle closure remain pending at this document state.

## Canonical Issue #73 acceptance matrix — current technical state

`TECHNICAL PASS` means source/tests/Mac/Windows evidence currently proves the item. `LIFECYCLE PENDING` means the item is deliberately not closed yet.

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
| 22 | Diff/Guard/Mac/Windows native gates pass. | TECHNICAL PASS | Diff, initial Guard, exact-head Mac and detached-clean Windows IL2CPP/D3D11 gates pass; final lifecycle Guard remains acceptance item 23. |
| 23 | Docs, private push/CI and physical USB lifecycle complete. | LIFECYCLE PENDING | Technical branch pushed; final CI/package/USB/Issue metadata remains open. |

## Remaining closure sequence

1. Commit/push the Windows-bound source/docs and hardened Issue #73 package verifier, then require Repository Guard/CI success.
2. Generate the fourteenth source receipt from the clean source/docs head and successful Guard; promote the exact `14/14` evidence set atomically.
3. Build and fully verify the immutable local checkpoint package against exact Git source, procedure/source semantics and the 14-name evidence contract.
4. Rediscover the correct external physical USB identity and previous milestone chain. Copy only to a collision-free `.incoming-*` target, remove AppleDouble only inside that new target, perform full path/size/SHA readback, atomically rename, and repeat the full readback.
5. Commit/push final physical metadata and require the final Guard. Only then mark all `23/23` acceptance boxes, close Issue #73 and set its Roadmap item to `Done`.

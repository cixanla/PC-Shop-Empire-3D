# Canonical Processor Cooler Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#77](https://github.com/cixanla/PC-Shop-Empire-3D/issues/77)<br>
**Technical head:** `197233688c4fe587097dbfc1cbee843cfc78603e`<br>
**Technical tree:** `58458f400a7efaa68e452a0e85e35d6d7eb5a3ab`<br>
**Closure status:** technical source/domain/scene/input/full-regression and exact-head macOS/Windows native gates passed; source/docs Guard, final provenance, immutable local/physical-USB double-readback and Issue/Project lifecycle gates remain pending

## Delivered playable result

GarageGraybox r39 adds the fifth physical component handoff for the accepted custom-PC work order. The domain resolves the canonical processor cooler by `ComponentKind.ProcessorCooler` and the exact work-order/ticket/allocation line, product, serialized item and reservation tuple. The player takes that exact object using real `E / Gamepad South`, carries it, advances its keyed 90° quarter-turn preview and places it into a processor-cooler-specific capacity-one managed BuildKit slot.

The authoritative custody chain is source → `ActorHands` → processor-cooler BuildKit. Motherboard, processor, DDR5 memory and M.2 storage prerequisites remain staged in their own slots, reservation/allocation identity stays live, and progress becomes exact `5/10`. World mutation occurs only after domain success. Placement recovery, exact replay and failure paths keep the same Unity component and stable `ItemId`; generic transfer/drop/box/stack/cart and processor-cooler Assembly/TIM paths cannot bypass active BuildKit custody.

The exact runtime success marker is:

```text
GARAGE_PROCESSOR_COOLER_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory+storage-staged processor-cooler-pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=90 placement=ok progress=5/10 reservation=alive custody=processor-cooler-build-kit receipts=ok revisions=ok assembly=untouched processor-cooler-slot=untouched tim=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

## Exact source and full regression

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 686/686 | `editmode.xml` | 572,393 | `f2b43ae1240a54c5572d495e7c27b1c74c19e9832c3bbc12bd1373aeefcf8d50` |
| PlayMode | 96/96 | `playmode.xml` | 263,349 | `dca0761cb57cdf41fc27987d7228d958d488169f7205e3cf46088ec6bd82c3e7` |

Both suites report failed, skipped and inconclusive `0`. The committed technical tree is clean and `git diff --check` passes.

Focused evidence before the full suites additionally passed:

- Processor-cooler BuildKit domain/replay/forgery/prerequisite matrix.
- Keyboard/gamepad pause, co-edge and release-repress gates.
- Four-pose preview visibility/position/rotation cycle and re-entry paths.
- GarageGraybox r39 scene contract and targeted processor-cooler flow.
- Nested Storage smoke fail-closed propagation and runtime-smoke compile gate.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development/StrictMode build | Success | `macos-build.log` | 600,193 | `97a444ea6aae9ff6869a3bdf5a507f946011b9f0df05ab56d69e5ba5faa90012` |
| Apple M1/Metal native runtime | Success | `macos-runtime.log` | 8,593 | `f33bc809ae31b31e683abe2c7113c361189102d207f3fb30baaa967705b75d1d` |
| App executable | Universal + valid ad-hoc signature | `PC Shop Empire 3D` | 117,179 | `586e327e34bdb12a71a86c2f2ad9ba5d96b1f8ac14b1ceab3392f993af27c55d` |

The build report is `329,787,583` bytes. `file` confirms `x86_64` and `arm64`; deep/strict `codesign --verify` passes. The native player forced Apple M1/Metal, emitted exact r39 readiness and exact processor-cooler BuildKit success once, emitted no BuildKit failure/assertion/unhandled-exception marker, shut the Input System down cleanly and left no player process.

## Windows exact-source IL2CPP and Direct3D11 gate

The exact technical branch was packaged as one complete Git bundle: `7,032,180` bytes, SHA-256 `743e0b24d7240c01d7e8addf84ed9f2dc2ed8288504741b8f31b0d9768b260fe`. The bundle exposes only `feature/issue77-processor-cooler-build-kit-handoff` at the technical head and passes `git bundle verify`.

The Windows procedure source uses a collision-free detached clean clone, byte-exact `ProjectSettings.asset` restoration, x64 IL2CPP with only Direct3D11, the expanded Burst/native-link fatal policy, three native-binary hashes, an interactive logged-on-user runtime, one exact host/readiness/success marker, zero forbidden tokens, graceful shutdown, task deletion and zero process residue.

The validation root was `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue77-197233688c4f-hardened-v2`. It remained detached, clean and byte-exact to technical commit/tree before and after build/runtime.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/Direct3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,251 | `3d4d3e425f240044d3cf2a7791d80df7a03b7060d2c50fd946e9205f6e0a1e7d` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `6bd5fb5a33470634e0cfbb935a256d1d43fa434cd720f6361e01a3bd37c69dee` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,513 | `25f762c7b23411ff415b39bbc989ee61d51c1f3a3dd750acdccbe6b10ff79669` |
| Runtime summary | Exit/task/residue contract passed | `runtime-summary.json` | 3,294 | `b0381f730523b8a51b7d3cd80d0440cd59990500aabf844a0963cc08c5725b23` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,701 | `b5cefabc9e0ae07a3c60fa4cd2515cfec38b21ed7d943513d2eeb2243e566896` |

The Windows build report is `1,333,221,634` bytes. The `issue77-hardened-v2` Burst/native-link fatal-token count is `0`, and `ProjectSettings.asset` before/after SHA-256 is the same: `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`. The binary manifest locks `PC Shop Empire 3D.exe` (`667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`), `GameAssembly.dll` (`45,169,664` bytes, `d969420573bd7aa7bcb72901fd868e93cf7ce26fd6159cae44db20d5619b334c`) and `UnityPlayer.dll` (`84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`).

The logged-on-user runtime reported Intel Iris Xe, Direct3D 11.0 feature level 11.1 and `1280×720`. Exact host, r39 readiness and processor-cooler BuildKit success markers each appeared once; forbidden markers were `0`. The player exited `0`, graceful shutdown was true, scheduled task `PSE-Issue77-197233688c4f-H2` was deleted, cleanup was unnecessary, process residue was `0`, and detached source remained clean.

Procedure source is itself hash-bound:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,424 | `02c06cd022bf4357c37ff197985a7cf064889e26c62ed3e01fea5111179e5b19` |
| `launch-procedure.ps1` | 7,963 | `1d1e311e2773642124e6c30a2d3faef52844218b331f27ed6bdbe40f68c37bfc` |
| `runtime-procedure.ps1` | 14,279 | `76b7fd2e630d30b25559b531a4a5b9a7a8143c700fda85935c3e33a1d2f81270` |
| `procedure-manifest.json` | 1,036 | `ace16f7da1dff686606ded0891d2c1f0cbb8248c2d6def62877027c7aed6cb7b` |

## Canonical evidence provenance state

The thirteen immutable test/build/runtime/procedure artifacts returned to the Mac with exact size/hash readback and are staged at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue77-197233688c4f/canonical-evidence`. The temporary Windows build receipt currently named `source-receipt.json` is not accepted as final checkpoint provenance. It will be atomically replaced only after the exact nine-file source/docs commit and Repository Guard succeed.

The final receipt must bind:

- technical commit/tree and clean local gates;
- exact source/docs commit/tree and successful Guard URL/run ID;
- EditMode `686/686`, PlayMode `96/96`;
- Universal Mac and detached-clean Windows native proof;
- `issue77-hardened-v2`;
- all other 13 promoted artifacts by exact path/size/SHA-256.

No immutable local package or physical USB completion is claimed in this document revision.

## Canonical Issue #77 acceptance matrix — current technical state

`TECHNICAL PASS` marks source/tests/native proof. `PARTIAL` means the implemented portion passed while a named lifecycle gate remains. `PENDING` is not a success claim.

| # | Acceptance contract | Current gate | Evidence |
|---:|---|---|---|
| 1 | Exact ProcessorCooler role and full line/product/item/reservation tuple. | TECHNICAL PASS | `CustomPcBuildKitAuthority`, allocation cross-check and exact-line tests. |
| 2 | No ordinal/display-name/product-value/regenerated identity authority. | TECHNICAL PASS | Wrong-line, value-equal and regenerated-identity no-mutation tests. |
| 3 | Separate stable operation and capacity-one processor-cooler BuildKit slot. | TECHNICAL PASS | Append-only IDs, fifth managed container and capacity tests. |
| 4 | All five BuildKit containers are claimed atomically through Quintuple access; partial/ghost topology fails with zero mutation. | TECHNICAL PASS | Quintuple ownership, duplicate/foreign container and partial-topology tests. |
| 5 | Motherboard/CPU/memory/storage slots, receipts, replay, revisions and staged state unchanged. | TECHNICAL PASS | Five-component aggregate preservation assertions. |
| 6 | Immediate/delayed replay returns one canonical receipt. | TECHNICAL PASS | Pickup/place history and replay matrix. |
| 7 | Foreign/value-equal/wrong-kind/line/product/item/reservation/order/operation fails closed. | TECHNICAL PASS | Expanded exact forgery matrix. |
| 8 | Full hands, occupied slot, source drift and stale revisions are no-mutation. | TECHNICAL PASS | Conflict/capacity/stale tests. |
| 9 | Custody is only source → hands → processor-cooler BuildKit; generic bypasses fail. | TECHNICAL PASS | Narrow Inventory bridge and bypass tests. |
| 10 | Reservation and allocation remain exact/live. | TECHNICAL PASS | Receipt identity assertions and native marker. |
| 11 | Motherboard+CPU+memory+storage prerequisites prevent skipping `4/10`. | TECHNICAL PASS | Prerequisite no-mutation tests and smoke setup. |
| 12 | Real `E / Gamepad South` pickup preserves range/focus/LOS/empty hands. | TECHNICAL PASS | Keyboard/mouse and gamepad PlayMode paths. |
| 13 | Separate cooler target, one support collider, exact anchor, gates and keyed 90° preview. | TECHNICAL PASS | Scene contract and four-pose projection tests. |
| 14 | BuildKit contextual input is single-consumer; cooler mounting cannot steal the same frame. | TECHNICAL PASS | Receipt-owned arbiter and Issue #58 isolation tests. |
| 15 | Co-edge, held, pause and release-repress are deterministic. | TECHNICAL PASS | Keyboard/gamepad pause/co-edge matrix. |
| 16 | Domain failure leaves the same cooler in hands before world mutation. | TECHNICAL PASS | Preflight/domain-failure snapshots. |
| 17 | Physical failure recovers the same instance at the exact kit pose. | TECHNICAL PASS | Forced placement recovery and no-clone invariant. |
| 18 | Unity instance, ItemId, collider/layer/ownership and container stay exact. | TECHNICAL PASS | Binding/scene/runtime identity assertions. |
| 19 | Visible progress derives from staged receipts as `4/10 → 5/10`. | TECHNICAL PASS | Aggregate authority, projection and runtime marker. |
| 20 | Cooler Assembly/TIM, earlier Assembly, electrical, price and five reservations remain untouched. | TECHNICAL PASS | Revision/receipt/item snapshots and native isolation marker. |
| 21 | WASD/mouse-look and keyboard/gamepad carry flow work in human-shaped scenarios. | TECHNICAL PASS | Existing locomotion regression plus real Input System BuildKit matrix and native route. |
| 22 | Domain/scene/input matrices and full regressions pass. | TECHNICAL PASS | EditMode `686/686`, PlayMode `96/96`. |
| 23 | Diff/Guard/Mac/Windows native gates pass. | PARTIAL | Diff and exact-head Mac/Windows native gates pass; current source/docs Repository Guard is pending. |
| 24 | Docs, private push/CI and physical USB lifecycle complete. | PENDING | Current source/docs commit/Guard, final receipt, current local package, physical double-readback and administrative closure remain. |

## Pending lifecycle sequence

1. Commit and push the exact nine-file source/docs closure delta.
2. Require successful Repository Guard and bind its exact run ID/URL in the final source receipt.
3. Replace only the temporary source receipt and prove exact canonical evidence `14/14`.
4. Create a collision-free immutable local package and pass `Tools/verify-checkpoint-package.sh ... issue77` on incoming and atomically named final paths.
5. Rediscover the external physical device and verify the correct volume, `90_BACKUPS/PCShopEmpire3D`, and the previous Issue #75 milestone chain before any write.
6. Copy only to a collision-free `.incoming-*`, remove AppleDouble only inside that new target, perform full hash/size/path and exact Git-source readback, atomically rename, and perform the same full readback again.
7. Commit physical lifecycle metadata, pass final Guard, check all `24/24`, close Issue #77 and set the Roadmap item `Done`.

Parent Epic #10 remains open for the remaining five component handoffs and later assembly/electrical/POST/OS/QA stages.

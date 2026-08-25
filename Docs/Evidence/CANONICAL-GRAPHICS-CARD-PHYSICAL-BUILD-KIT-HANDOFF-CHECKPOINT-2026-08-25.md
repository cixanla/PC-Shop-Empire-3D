# Canonical Graphics Card Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#79](https://github.com/cixanla/PC-Shop-Empire-3D/issues/79)<br>
**Technical head:** `f40ef21058caf1a2aca3054218abfc1dd7305c01`<br>
**Technical tree:** `c7500e7300f75f5d9b089bf23657750dccc5ffed`<br>
**Closure status:** technical source/domain/scene/input/full-regression and exact-head macOS/Windows native gates passed; source/docs Guard, final provenance, immutable local/physical-USB double-readback and Issue/Project lifecycle gates remain pending

## Delivered playable result

GarageGraybox r40 adds the sixth physical component handoff for the accepted custom-PC work order. The domain resolves the canonical graphics card by `ComponentKind.GraphicsCard` and the exact work-order/ticket/allocation line, product, serialized item and reservation tuple. The player takes that exact object using real `E / Gamepad South`, carries it, toggles its keyed 180° preview and places it into a graphics-card-specific capacity-one managed BuildKit slot.

The authoritative custody chain is source → `ActorHands` → graphics-card BuildKit. Motherboard, processor, DDR5 memory, M.2 storage and processor-cooler prerequisites remain staged in their own slots, reservation/allocation identity stays live, and progress becomes exact `6/10`. World mutation occurs only after domain success. Placement recovery, exact replay and failure paths keep the same Unity component and stable `ItemId`; generic transfer/drop/box/stack/cart, Issue #59 GPU Assembly and Issue #63 PCIe cable paths cannot bypass active BuildKit custody.

The exact runtime success marker is:

```text
GARAGE_GRAPHICS_CARD_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory+storage+processor-cooler-staged graphics-card-pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=180 placement=ok progress=6/10 reservation=alive custody=graphics-card-build-kit receipts=ok revisions=ok assembly=untouched graphics-card-slot=untouched pcie-route=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

## Exact source and full regression

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 690/690 | `editmode.xml` | 575,579 | `c94c3e1af3fe137fc069e6d3d6a7ff6347274dc55cda3e96c50ff4299b9f2c8f` |
| PlayMode | 100/100 | `playmode.xml` | 276,780 | `30078e18871a802d2d8480dccd69ce92371c22cd0410bdbf770b9a4a48eabddd` |

Both suites report failed, skipped and inconclusive `0`. The committed technical tree is clean and `git diff --check` passes.

Focused evidence before the full suites additionally passed:

- Graphics-card BuildKit domain/replay/forgery/prerequisite matrix.
- Six-container all-or-none claim and partial/duplicate/foreign topology matrix.
- Keyboard/gamepad pause, co-edge and release-repress gates.
- Two-pose 180° preview, exact scene contract and physical same-instance recovery.
- Nested processor-cooler smoke fail-closed propagation and runtime-smoke compile gate.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development/StrictMode build | Success | `macos-build.log` | 601,457 | `f04fe02542e255eb780eced9c61c797a3cfc8bd373864b54fb63f5c2a4fbc9d0` |
| Apple Silicon/Metal native runtime | Success | `macos-runtime.log` | 8,459 | `38c558d8ccbcf35c61c1c9d37b2607d4641e2b2a7e064e1c2a77632e3455278b` |
| App executable | Universal + valid ad-hoc signature | `PC Shop Empire 3D` | 117,179 | `a04f38a94a9b5b3b93251f9be10112cfd004317e26a743573038c36df4826e9b` |

The build report is `329,839,788` bytes. `file` confirms `x86_64` and `arm64`; deep/strict `codesign --verify` passes. The native player forced Metal, emitted exact r40 readiness and exact graphics-card BuildKit success once, emitted no BuildKit failure/assertion/unhandled-exception marker and left no player process.

## Windows exact-source IL2CPP and Direct3D11 gate

The exact technical branch was packaged as one complete Git bundle: `7,096,172` bytes, SHA-256 `7c9ca43567dfd09fbf01c9f22142942e62bd4b087e71d88a9f3beda4a1fd9636`. The bundle exposes only `feature/issue79-graphics-card-build-kit-handoff` at the technical head and passes `git bundle verify`.

The final procedure uses a collision-free detached clean clone, byte-exact `ProjectSettings.asset` restoration, x64 IL2CPP with only Direct3D11, the expanded Burst/native-link fatal policy, three native-binary hashes, an interactive logged-on-user runtime, one exact host/readiness/success marker, zero forbidden tokens, graceful shutdown, task deletion and zero process residue.

The accepted validation root was `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue79-f40ef21058ca-hardened-v3`. It remained detached, clean and byte-exact to technical commit/tree before and after build/runtime. An earlier v2 build emitted a successful Unity marker but was not promoted because `Start-Process -Wait` remained blocked after Unity exited and therefore did not produce the required receipts. The v3 polling procedure ran again from a different fresh root and is the only accepted Windows evidence.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/Direct3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,383 | `e42d3fe5cb488ca7dc033c36dbce7e5c01623d4eead985aed8ecd5f5f4166030` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `d3eec52375de2643e77d31a1067977705a07fb77ffa30233f1c50c8f547a16ab` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,544 | `af18ae42464f47e0996975457681699d137917fd780bf61d8e473fef22413a5c` |
| Runtime summary | Exit/task/residue contract passed | `runtime-summary.json` | 3,294 | `24df266309de49f566abc29f65f505e0be88a8f778181f6aea269075b7bbabdd` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,702 | `603a7b54b48c19324e9825e120c4dc37723df8f7d9d1bfc59e5fa0af9e8cb1ba` |

The Windows build report is `1,334,256,694` bytes. The `issue79-hardened-v3` Burst/native-link fatal-token count is `0`, and `ProjectSettings.asset` before/after SHA-256 is the same: `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`. The binary manifest locks `PC Shop Empire 3D.exe` (`667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`), `GameAssembly.dll` (`45,218,816` bytes, `908730282291b3c93dfad39822e39222e7d8e87f2c0b5d9c878f0435a6951883`) and `UnityPlayer.dll` (`84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`).

The logged-on-user runtime reported Intel Iris Xe, Direct3D 11.0 feature level 11.1 and `1280×720`. Exact host, r40 readiness and graphics-card BuildKit success markers each appeared once; forbidden markers were `0`. The player exited `0`, graceful shutdown was true, scheduled task `PSE-Issue79-f40ef21058ca-H3` was deleted, cleanup was unnecessary, process residue was `0`, and detached source remained clean.

Procedure source is itself hash-bound:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,582 | `ca8f582b602e3d1f75576758f6d7014c4926265c8504bb97f3c6bf2d79a0110b` |
| `launch-procedure.ps1` | 7,963 | `934367916149db86e4b8f9e3b842f74392154ea51482a853bce9a5eb5cd9747b` |
| `runtime-procedure.ps1` | 14,378 | `f6f6298bcebe3c6f1b0131778990161fe28f7348bdcd19b48f1670dada85ef34` |
| `procedure-manifest.json` | 670 | `603dbc82e56ee616df125920e595bc42c814bb8aa295fe558cf7ca72a995a11a` |

## Canonical evidence provenance state

The thirteen immutable test/build/runtime/procedure artifacts returned to the Mac with exact size/hash readback and are staged at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue79-f40ef21058ca/canonical-evidence`. The temporary Windows build receipt currently named `source-receipt.json` is not accepted as final checkpoint provenance. It will be atomically replaced only after the exact nine-file source/docs commit and Repository Guard succeed.

The final receipt must bind:

- technical commit/tree and clean local gates;
- exact source/docs commit/tree and successful Guard URL/run ID;
- EditMode `690/690`, PlayMode `100/100`;
- Universal Mac and detached-clean Windows native proof;
- `issue79-hardened-v3`;
- all other 13 promoted artifacts by exact path/size/SHA-256.

No immutable local package or physical USB completion is claimed in this document revision.

## Canonical Issue #79 acceptance matrix — current technical state

`TECHNICAL PASS` marks source/tests/native proof. `PARTIAL` means the implemented portion passed while a named lifecycle gate remains. `PENDING` is not a success claim.

| # | Acceptance contract | Current gate | Evidence |
|---:|---|---|---|
| 1 | Exact GraphicsCard role and full line/product/item/reservation tuple. | TECHNICAL PASS | `CustomPcBuildKitAuthority`, allocation cross-check and exact-line tests. |
| 2 | No ordinal/display-name/product-value/regenerated identity authority. | TECHNICAL PASS | Wrong-line, value-equal and regenerated-identity no-mutation tests. |
| 3 | Separate stable operation and capacity-one graphics-card BuildKit slot. | TECHNICAL PASS | Append-only IDs, sixth managed container and capacity tests. |
| 4 | All six BuildKit containers are claimed atomically through Sextuple access; partial/ghost topology fails with zero mutation. | TECHNICAL PASS | Sextuple ownership, duplicate/foreign container and partial-topology tests. |
| 5 | First five BuildKit slots, receipts, replay, revisions and staged state unchanged. | TECHNICAL PASS | Six-component aggregate preservation assertions. |
| 6 | Immediate/delayed replay returns one canonical receipt. | TECHNICAL PASS | Pickup/place history and replay matrix. |
| 7 | Foreign/value-equal/wrong-kind/line/product/item/reservation/order/operation fails closed. | TECHNICAL PASS | Expanded exact forgery matrix. |
| 8 | Full hands, occupied slot, source drift and stale revisions are no-mutation. | TECHNICAL PASS | Conflict/capacity/stale tests. |
| 9 | Custody is only source → hands → graphics-card BuildKit; generic bypasses fail. | TECHNICAL PASS | Narrow Inventory bridge and bypass tests. |
| 10 | Reservation and allocation remain exact/live. | TECHNICAL PASS | Receipt identity assertions and native marker. |
| 11 | First five staged prerequisites prevent skipping `5/10`. | TECHNICAL PASS | Prerequisite no-mutation tests and smoke setup. |
| 12 | Real `E / Gamepad South` pickup preserves range/focus/LOS/empty hands. | TECHNICAL PASS | Keyboard/mouse and gamepad PlayMode paths. |
| 13 | Separate GPU target, one support collider, exact anchor, gates and keyed 180° preview. | TECHNICAL PASS | Scene contract and two-pose projection tests. |
| 14 | BuildKit contextual input is single-consumer; GPU seating and PCIe routing cannot steal the same frame. | TECHNICAL PASS | Receipt-owned arbiter and legacy-mode isolation tests. |
| 15 | Co-edge, held, pause and release-repress are deterministic. | TECHNICAL PASS | Keyboard/gamepad pause/co-edge matrix. |
| 16 | Domain failure leaves the same GPU in hands before world mutation. | TECHNICAL PASS | Preflight/domain-failure snapshots. |
| 17 | Physical failure recovers the same instance at the exact kit pose. | TECHNICAL PASS | Forced placement recovery and no-clone invariant. |
| 18 | Unity instance, ItemId, collider/layer/ownership and container stay exact. | TECHNICAL PASS | Binding/scene/runtime identity assertions. |
| 19 | Visible progress derives from staged receipts as `5/10 → 6/10`. | TECHNICAL PASS | Aggregate authority, projection and runtime marker. |
| 20 | #59 GPU Assembly, #63 PCIe route, other Assembly/electrical/price/reservations remain untouched. | TECHNICAL PASS | Revision/receipt/item snapshots and native isolation marker. |
| 21 | WASD/mouse-look and keyboard/gamepad carry flow work in human-shaped scenarios. | TECHNICAL PASS | Existing locomotion regression plus real Input System BuildKit matrix and native route. |
| 22 | Domain/scene/input matrices and full regressions pass. | TECHNICAL PASS | EditMode `690/690`, PlayMode `100/100`. |
| 23 | Diff/Guard/Mac/Windows native gates pass. | PARTIAL | Diff and exact-head Mac/Windows native gates pass; current source/docs Repository Guard is pending. |
| 24 | Docs, private push/CI and physical USB lifecycle complete. | PENDING | Current source/docs commit/Guard, final receipt, current local package, physical double-readback and administrative closure remain. |

## Pending lifecycle sequence

1. Commit and push the exact nine-file source/docs closure delta.
2. Open the integration pull request, require successful Repository Guard and bind its exact run ID/URL in the final source receipt.
3. Replace only the temporary source receipt and prove exact canonical evidence `14/14`.
4. Create a collision-free immutable local package and pass `Tools/verify-checkpoint-package.sh ... issue79` on incoming and atomically named final paths.
5. Rediscover the external physical device and verify the correct volume, `90_BACKUPS/PCShopEmpire3D`, and the previous milestone chain before any write.
6. Copy only to a collision-free `.incoming-*`, remove AppleDouble only inside that new target, perform full hash/size/path and exact Git-source readback, atomically rename, and perform the same full readback again.
7. Commit physical lifecycle metadata, pass final Guard, check all `24/24`, close Issue #79 and set the Roadmap item `Done`.

Parent Epic #10 remains open for the remaining four component handoffs and later assembly/electrical/POST/OS/QA stages. Issue #77 remains independently open/In Progress until its own physical lifecycle closes.

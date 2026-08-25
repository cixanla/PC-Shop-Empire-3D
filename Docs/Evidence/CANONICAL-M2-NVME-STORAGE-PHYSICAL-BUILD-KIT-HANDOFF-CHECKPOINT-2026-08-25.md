# Canonical M.2 NVMe Storage Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#75](https://github.com/cixanla/PC-Shop-Empire-3D/issues/75)<br>
**Technical head:** `646e66cfa269a217ecb1f6942f9accb77f9e463c`<br>
**Technical tree:** `ee9b0b2c0bb5e1fb07de397da222d00a7480b23c`<br>
**Closure status:** source/domain/scene/input/full-regression, exact-head macOS/Windows native, procedure-bound provenance, source/docs CI and immutable local/physical-USB double-readback gates passed; final physical-metadata CI and Issue/Project administrative closure pending

## Delivered playable result

GarageGraybox r38 adds the fourth physical component handoff for the accepted custom-PC work order. The domain resolves the canonical M.2 2280 NVMe SSD by `ComponentKind.StorageDevice` and the exact work-order/ticket/allocation line, product, serialized item and reservation tuple. The player takes that exact object using real `E / Gamepad South`, carries it, toggles the keyed 180° orientation and places it into a storage-specific capacity-one managed BuildKit slot.

The authoritative custody chain is source → `ActorHands` → storage BuildKit. Motherboard, processor and DDR5 memory prerequisites remain staged in their own slots, reservation/allocation identity stays live, and progress becomes exact `4/10`. World mutation occurs only after domain success. Placement recovery, exact replay and failure paths keep the same Unity component and stable `ItemId`; generic transfer/drop/box/stack/cart and motherboard M.2 Assembly paths cannot bypass active BuildKit custody.

The exact runtime success marker is:

```text
GARAGE_STORAGE_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory-staged storage-pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=180 placement=ok progress=4/10 reservation=alive custody=storage-build-kit receipts=ok revisions=ok assembly=untouched m2-slot=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

## Exact source and full regression

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 683/683 | `editmode.xml` | 569,918 | `2ed8dcea48890f00bda529ec14645bf98757fbdcdf17e9cb45c726065eab860c` |
| PlayMode | 90/90 | `playmode.xml` | 243,482 | `3ade7cd4b42df2aa90dcf4cd5af194b1e66e9994abb8ec2bdbc792e43c7df228` |

Both suites report failed, skipped and inconclusive `0`. The committed technical tree is clean and `git diff --check` passes. All five newly added Unity GUIDs are unique.

Focused evidence before the full suites additionally passed:

- Storage BuildKit domain/replay/forgery/prerequisite matrix.
- Keyboard/gamepad pause, co-edge and release-repress gates.
- GarageGraybox r38 scene contract `9/9`.
- Targeted new PlayMode storage flow `4/4`.
- Runtime-smoke compile gate.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development/StrictMode build | Success | `macos-build.log` | 601,343 | `177b9e938d19180e08e116fed510a4213ea6f7a8ecd8fe38adbf4a7304ef5ebd` |
| Apple M1/Metal native runtime | Success | `macos-runtime.log` | 8,289 | `f3cdb97adda5965410acd538840d0fc976e953fc70406224d55ecf3d41a5c4a6` |
| App executable | Universal + valid ad-hoc signature | `PC Shop Empire 3D` | 117,179 | `ad9c7bd33a0ad8dee9a3ccbd6260b533aba40413b19b1d826293ba20c66dc8bb` |

The build report is `329,735,698` bytes. `file` confirms `x86_64` and `arm64`; deep/strict `codesign --verify` passes. The native player forced Apple M1/Metal, emitted exact r38 readiness and exact storage BuildKit success once, emitted no BuildKit failure/assertion/unhandled-exception marker and left no player process.

## Windows exact-source IL2CPP and Direct3D11 gate

The exact technical branch was packaged as one complete Git bundle: `6,944,340` bytes, SHA-256 `1f6692dfde079d13747a1beddfdb69ea8000f7f5ffd3925c7639213fa8c5d35e`. The bundle exposes only `feature/issue75-m2-build-kit-handoff` at the technical head and passes `git bundle verify`.

The Windows procedure source uses a collision-free detached clean clone, byte-exact `ProjectSettings.asset` restoration, x64 IL2CPP with only Direct3D11, the expanded Burst/native-link fatal policy, three native-binary hashes, an interactive logged-on-user runtime, one exact host/readiness/success marker, zero forbidden tokens, graceful shutdown, task deletion and zero process residue.

The validation root was `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue75-646e66cfa269-hardened-v2`. It remained detached, clean and byte-exact to technical commit/tree before and after build/runtime.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/Direct3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,178 | `9074cd3227fe4dafa998be700474c6b59f67abb48b84cc71db487f0df06daa72` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `36d869b33cd10e43f264d3fd03594eda0c8a9a1f18d44afb4093482afb53aecc` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,405 | `e8f787616dd716e4c1def3eb74aa5b870f6f2d5c5cf78b8f97b349d74354ef21` |
| Runtime summary | Exit/task/residue contract passed | `runtime-summary.json` | 3,294 | `72783f4806d0eb36c78a561663dae1cee17713b9560953fc91980ab84a1195ab` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,702 | `05ca7482fc4d4d8eab32f3c31abe0f4d7c69b8ef67fbbec24c3cf07f41579174` |

The Windows build report is `1,332,182,927` bytes. The expanded hardened-v2 Burst/native-link fatal-token count is `0`, and `ProjectSettings.asset` before/after SHA-256 is the same: `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`. The binary manifest locks `PC Shop Empire 3D.exe` (`667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`), `GameAssembly.dll` (`45,122,560` bytes, `e98981ad78240626fba7d05201e7b4260aa5b538cdfbd5b9cd2d5f82f2c2edd9`) and `UnityPlayer.dll` (`84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`).

The logged-on-user runtime reported Intel Iris Xe, Direct3D 11.0 feature level 11.1 and `1280×720`. Exact host, r38 readiness and storage BuildKit success markers each appeared once; forbidden markers were `0`. The player exited `0`, graceful shutdown was true, scheduled task `PSE-Issue75-646e66cfa269-H2` was deleted, cleanup was unnecessary, process residue was `0`, and detached source remained clean.

Procedure source is itself hash-bound:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,294 | `1d9d6920aeac5aac65d5ed1ed85a3e30464aac973bf5bcc88cc53a4657eb12bc` |
| `launch-procedure.ps1` | 7,963 | `471dca8cd503fb86deca1e9689285946d33381a382236289a8bcdf0d702e573c` |
| `runtime-procedure.ps1` | 13,980 | `7539fda7a52fef07532e9394f29b1f41f3cf0aad6631bfbabecfbcff2a6de6f1` |
| `procedure-manifest.json` | 670 | `d8f5527117d0cc73b000006f903d4dcccdacfd5933c4bae9d7b09bf3666d9ccf` |

## Canonical evidence and immutable physical package

The thirteen immutable test/build/runtime/procedure artifacts returned to the Mac with exact size/hash readback. Clean source/docs commit `af6578aa224b931fdcfdd6293dccfcfd77a29eac`, tree `39ec1c0573223899d2982f72fb877dbea58306ba` and [Repository Guard 32849988087](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32849988087) then authorized final `source-receipt.json` (`4,778` bytes, SHA-256 `f6dfd8f0aca43eb9d5301879070bb1663790bb899993984f9cd0056ce33cf1d6`). Promoted-artifact path/size/hash equality is `13/13`; canonical evidence is exact `14/14` at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue75-646e66cfa269/canonical-evidence`.

The Issue #75 mode in `Tools/verify-checkpoint-package.sh` fixes the 14-name evidence contract, technical commit/tree, test counts, exact nine-file technical→source/docs allowlist, procedure and promoted-artifact receipts, `issue75-hardened-v2` build policy, expanded native-link fatal-token count `0`, exact r38 Storage BuildKit marker count `1`, task cleanup and residue `0`. Generic canonical mode is not accepted for this closure.

- Local final: `/Users/cixanla/Developer/PCShopEmpire3D/CheckpointStaging/2026-08-25_STAGE_B_CANONICAL_M2_NVME_STORAGE_BUILDKIT_HARDENED_V2`.
- Physical final: `D:\CIXANLA\90_BACKUPS\PCShopEmpire3D\2026-08-25_STAGE_B_CANONICAL_M2_NVME_STORAGE_BUILDKIT_HARDENED_V2` on healthy `Intenso Alu Line`, USB disk 1, exFAT volume label `cixanla`.
- Previous-chain proof: the existing Issue #73 milestone was present and its `954`-line manifest recorded and recalculated as `912e35ff4a2d81c4a010a84185c5ea88f5c2782d2caf4cfe3aadd40ed2ee9cc8` before any new write.
- Local incoming and local atomically named final both returned `CHECKPOINT_PACKAGE_OK`; an additional independent final verification also passed.
- The collision-free physical `.incoming-issue75-af6578a` copy was streamed back over SSH to an independent Mac temporary root and returned `CHECKPOINT_PACKAGE_OK`; after same-filesystem atomic rename, a second independent physical readback returned the same result.
- Every full readback is identical: `966/966` payload, `951/951` exact Git source, `14/14` evidence, source/docs commit `af6578aa224b931fdcfdd6293dccfcfd77a29eac`, tree `39ec1c0573223899d2982f72fb877dbea58306ba`, `19,598,907` payload bytes and manifest SHA-256 `958ba6bcb0f4891a168f73da46b20552b8a059467c7e0e6d55a5d7a51f0f9d2b`.
- Physical incoming directory/sidecar residue, internal AppleDouble and exact final sidecar counts are all `0`; older milestones and unrelated user data were not modified.

## Canonical Issue #75 acceptance matrix — current technical state

`TECHNICAL PASS` marks source/tests/native proof. `FINAL METADATA PENDING` means the immutable package and physical readback passed while this metadata commit/Guard and administrative transition remain.

| # | Acceptance contract | Current gate | Evidence |
|---:|---|---|---|
| 1 | Exact StorageDevice role and full line/product/item/reservation tuple. | TECHNICAL PASS | `CustomPcBuildKitAuthority`, allocation cross-check and exact-line tests. |
| 2 | No ordinal/display-name/product-value/regenerated identity authority. | TECHNICAL PASS | Wrong-line, value-equal and regenerated-identity no-mutation tests. |
| 3 | Separate stable operation and capacity-one storage BuildKit slot. | TECHNICAL PASS | Append-only IDs, fourth managed container and capacity tests. |
| 4 | Motherboard/CPU/memory slots, receipts, replay, revisions and staged state unchanged. | TECHNICAL PASS | Four-component aggregate preservation assertions. |
| 5 | Immediate/delayed replay returns one canonical receipt. | TECHNICAL PASS | Pickup/place history and replay matrix. |
| 6 | Foreign/value-equal/wrong-kind/line/product/item/reservation/order/operation fails closed. | TECHNICAL PASS | Expanded exact forgery matrix. |
| 7 | Full hands, occupied slot, source drift and stale revisions are no-mutation. | TECHNICAL PASS | Conflict/capacity/stale tests. |
| 8 | Custody is only source → hands → storage BuildKit; generic bypasses fail. | TECHNICAL PASS | Narrow Inventory bridge and bypass tests. |
| 9 | Reservation and allocation remain exact/live. | TECHNICAL PASS | Receipt identity assertions and native marker. |
| 10 | Motherboard+CPU+memory prerequisites prevent skipping `3/10`. | TECHNICAL PASS | Prerequisite no-mutation tests and smoke setup. |
| 11 | Real `E / Gamepad South` pickup preserves range/focus/LOS/empty hands. | TECHNICAL PASS | Keyboard/mouse and gamepad PlayMode paths. |
| 12 | Separate storage target, one support collider, exact anchor, gates and 180° preview. | TECHNICAL PASS | Scene contract and projection tests. |
| 13 | BuildKit contextual input is single-consumer; M.2 seating cannot steal the same frame. | TECHNICAL PASS | Receipt-owned arbiter and M.2 isolation tests. |
| 14 | Co-edge, held, pause and release-repress are deterministic. | TECHNICAL PASS | Keyboard/gamepad pause/co-edge matrix. |
| 15 | Domain failure leaves the same NVMe in hands before world mutation. | TECHNICAL PASS | Preflight/domain-failure snapshots. |
| 16 | Physical failure recovers the same instance at the exact kit pose. | TECHNICAL PASS | Forced placement recovery and no-clone invariant. |
| 17 | Unity instance, ItemId, collider/layer/ownership and container stay exact. | TECHNICAL PASS | Binding/scene/runtime identity assertions. |
| 18 | Visible progress derives from staged receipts as `3/10 → 4/10`. | TECHNICAL PASS | Aggregate authority, projection and runtime marker. |
| 19 | M.2 Assembly, earlier Assembly, electrical, price and six reservations remain untouched. | TECHNICAL PASS | Revision/receipt/item snapshots and native isolation marker. |
| 20 | WASD/mouse-look and keyboard/gamepad carry flow work in human-shaped scenarios. | TECHNICAL PASS | Existing locomotion regression plus real Input System BuildKit matrix and native input route. |
| 21 | Domain/scene/input matrices and full regressions pass. | TECHNICAL PASS | EditMode `683/683`, PlayMode `90/90`. |
| 22 | Diff/Guard/Mac/Windows native gates pass. | TECHNICAL PASS | Diff, source/docs Guard `32849988087`, exact-head Mac and detached-clean Windows IL2CPP/D3D11 gates pass. |
| 23 | Docs, private push/CI and physical USB lifecycle complete. | FINAL METADATA PENDING | Source/docs Guard, final receipt, local package and two physical USB readbacks pass; this metadata commit/Guard and administrative closure remain. |

## Pending lifecycle sequence

1. Commit and push this physical lifecycle metadata and require Repository Guard/CI success.
2. Mark all `23/23` acceptance boxes, close Issue #75 and set its Roadmap item to `Done`; make PR #76 ready for integration.
3. Record the exact physical-metadata commit/Guard and closed lifecycle state in the final project handoff without starting the next gameplay slice.

Parent Epic #10 remains open for the remaining six component handoffs and later assembly/electrical/POST/OS/QA stages.

# Canonical M.2 BuildKit-to-Primary-Slot Captive-Screw Assembly Handoff — Checkpoint Evidence

**Date:** 26 August 2026
**Issue:** [#95](https://github.com/cixanla/PC-Shop-Empire-3D/issues/95)
**Draft PR:** [#96](https://github.com/cixanla/PC-Shop-Empire-3D/pull/96)
**Technical head:** `42c1ae4dff2421b38879c0bfc82b4bf52522be1e`
**Technical tree:** `16304340da0ae7e42d8e7dd1ea6aef66ffe27efc`
**Current closure:** source/domain/scene/input/full-regression, exact-head macOS/Windows native and technical-CI gates passed; source/docs, final canonical/local package, healthy physical USB, real-human session and administrative gates pending

## Delivered playable result

GarageGraybox r48 connects the canonical reserved M.2 2280 NVMe in the completed `10/10` custom-PC BuildKit to the existing primary M-key M.2 slot and motherboard-owned captive screw after the exact Issue #89 motherboard is live and `SeatedSecured`, Issue #91 CPU is live and `ProcessorRetained`, and Issue #93 DDR5 is live in A2 and `MemoryModuleRetained`. The domain resolves storage only from the exact accepted work-order/ticket/allocation line, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity SSD instance with `E / Gamepad South`, carries it from BuildKit custody into exact hands, opens the existing keyed M.2 seat mode with `Mouse Left / Gamepad RT`, commits the M-key-aligned 18-degree insertion and flat seat with `G / Gamepad East`, tightens the captive screw, proves remove is blocked while secured, loosens, detaches and reseats that same instance. Live reservation/allocation remains exact. The original ten staging receipts and visible `10/10` preparation history remain immutable; the secured motherboard, retained CPU, retained DDR5 and other six uninstalled BuildKit items do not move.

This is a custody bridge, not a second Inventory, M.2 slot or fastener authority. Existing Issue #57 family/topology/orientation/seat/press/remove/fastener authority remains the only storage Assembly truth. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal the handoff item.

Issue #95 uses a tested atomicity refinement at pickup: the physical projection is staged reversibly first, but no usable held-input state is published; the exact authority commit follows. If authority rejects, the same instance is recovered to the exact BuildKit safe pose. Only after both steps succeed is `HeldItem` exposed. Fault-boundary tests cover physical-stage failure and authority-rejection rollback. This replaces the issue draft's older “domain success before physical mutation” wording and preserves the observable no-duplicate/no-loss/no-ghost contract.

The exact runtime success marker is:

```text
GARAGE_STORAGE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained memory=retained pickup=exact custody=build-kit-to-hands-to-primary-m2 reservation=alive physical-identity=stable input=keyboard+mouse m-key=aligned guided-angle=18 seat=ok captive-screw=tightened secured-remove-blocked=ok loosen=ok detach=ok reseat=ok history=10/10-preserved other-six=untouched receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok
```

Automated native smoke proves the production handoff and invariants; it is not a real-human keyboard/mouse or gamepad acceptance session.

The exact technical head also includes the human-playability follow-up found by the bounded input audits: pause/focus resume suppresses held keyboard/gamepad Move and held gamepad Look until every resolved continuous control is neutral. Opposing `W+S` / `A+D` pairs cannot release the latch through aggregate-vector cancellation, while fresh mouse delta after resume remains responsive. The regression is exercised inside the existing full PlayMode suite.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Full EditMode | 722/722 | `editmode.xml` | 600,642 | `038561270629ab626df5441c96b036aa5e30463f5fd148557bb2a087f75e02b2` |
| Full PlayMode | 130/130 | `playmode.xml` | 393,262 | `4b91a2d6b98e27ba8041e9a4f98811916cfb0d8df8278038273821b766ab7115` |

Both suites have failed, skipped and inconclusive `0`. EditMode duration is `13.4635925` seconds; PlayMode duration is `367.4320988` seconds. The technical tree passes `git diff --check`. Exact-head GitHub Repository Guard [32962078481](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32962078481) passed at `42c1ae4dff2421b38879c0bfc82b4bf52522be1e`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 590,146 | `179f16696b2b4da6e74ebc629ca4b42c9166df7be93ca6e8a0185764fad6c1f4` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 8,893 | `05e20e7f943fc5b8644b1bd642214557c73899d5d2c53afd80e6133e4a7aa657` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `4c557beecb847916c7e30f00e0ce82d3050c9d2de7a75d7f9917d4275757a9de` |

The build report is `330,195,891` bytes and the application contains `302` files with the same total file bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits one exact r48 readiness marker including `storage-assembly-handoff=ready`, one exact success marker, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete technical bundle is `7,626,490` bytes with SHA-256 `f11c846bd025342d6b64743a1d1b872664a7f21853754e492d1baad0350b3748`. Its exact technical head was restored into the collision-free detached-clean checkout:

`C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue95-42c1ae4-opposing-neutral-v3`

Unity 6000.3.21f1 built a strict x64 IL2CPP development player with only Direct3D11. The build report is `1,343,654,204` bytes; build log `build-il2cpp-d3d11.log` is `1,583,576` bytes with SHA-256 `798c69316d0d24718efe825c079da60cad355ce672d86741ec2f634d3318afaa`. The expanded native-link/build fatal-token count is `0`, build marker count is `1`, ProjectSettings before/after SHA-256 is byte-exact `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`, the detached checkout remains at the exact technical head/tree and `git status` remains empty.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | 667,136 | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | 45,604,864 | `3cb40b4b01a82449d32623fc3891b4eda91e65d3053911f622772c442aa6620c` |
| `UnityPlayer.dll` | 84,237,744 | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The interactive scheduled-task player used `Intel(R) Iris(R) Xe Graphics`, forced Direct3D 11.0 feature level 11.1 and emitted exact host/readiness/success counts `1/1/1`, including `storage-assembly-handoff=ready`; forbidden count is `0`. It exited `0` after Input System shutdown, graceful close succeeded, scheduled task `PCShop-Issue95-r48-42c1ae4` was deleted, cleanup was not required, and player/Unity/task residue is `0`. `runtime-d3d11.log` is `5,805` bytes with SHA-256 `2b9907be58c3d4f732cf000814c80c598f0d6d30eeeafb426e25f1daa091914d`. The checkout remains detached-clean at the exact technical head/tree.

The three exact procedures are hash-bound by `procedure-manifest.json`:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 10,104 | `e22c67b8e99d150d5d5d78d3ce8cc525b552d7edecd94223d0ef1ee2d7a7ebde` |
| `launch-procedure.ps1` | 8,079 | `13cdf7013e16125f65ac9eff3035f6dd18b102d3fd861566cb8e13cef60a7301` |
| `runtime-procedure.ps1` | 15,378 | `55588b0c98d18ec2142c59fdb05205af71098a987931e67bdc364d208184f144` |

The first thirteen immutable canonical artifacts now exist at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue95-42c1ae4dff24/canonical-evidence`; only the final source/docs provenance receipt remains for `14/14`.

## Issue #95 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical Storage line/product/item/reservation/allocation tuple. | TECHNICAL PASS |
| 2 | Historical `10/10` receipts and owned Issue #89 + #91 + #93 Assembly chain required. | TECHNICAL PASS |
| 3 | Exact motherboard Workbench custody, `SeatedSecured`, attach/secure receipts. | TECHNICAL PASS |
| 4 | Exact CPU ProcessorSocket custody, `ProcessorRetained`, seat/retain receipts. | TECHNICAL PASS |
| 5 | Exact DDR5 A2 custody, `MemoryModuleRetained`, seat/latch receipts. | TECHNICAL PASS |
| 6 | Exact primary M.2 2280 capacity-one managed empty/open slot and captive screw. | TECHNICAL PASS |
| 7 | Stable storage handoff operation is distinct and bound to exact staging receipt. | TECHNICAL PASS |
| 8 | Immediate/delayed replay is exactly once. | TECHNICAL PASS |
| 9 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | TECHNICAL PASS |
| 10 | Stale revisions, full hands, occupied slot, wrong tool and overflow are no-mutation. | TECHNICAL PASS |
| 11 | Inventory permits only registered Storage BuildKit→hands and M.2 slot↔hands transfer. | TECHNICAL PASS |
| 12 | Reservation/allocation remains live and exact through the reversible flow. | TECHNICAL PASS |
| 13 | Ten receipts/history, other six items, motherboard, CPU and DDR5 remain untouched. | TECHNICAL PASS |
| 14 | Existing Issue #57 topology, seat/remove and captive-screw semantics remain authoritative. | TECHNICAL PASS |
| 15 | Same Unity instance and stable ItemId survive pickup→reseat. | TECHNICAL PASS |
| 16 | Pickup and slot physical/input/tool gates fail closed. | TECHNICAL PASS |
| 17 | Reversible projection staging, authority commit and same-instance rollback are atomic to observable play. | TECHNICAL PASS |
| 18 | Generic transfer/drop/box/stack/cart and receipt-free bypasses are blocked. | TECHNICAL PASS |
| 19 | `M.2 MONTAJDA`, immutable ticket and `10/10` history remain readable. | TECHNICAL PASS |
| 20 | Keyboard/mouse and real Input System gamepad full handoff flow. | TECHNICAL PASS |
| 21 | WASD, mouse-look, pause/no-lurch and single-consumer input regressions. | TECHNICAL PASS |
| 22 | Motherboard/storage/CPU/DIMM interlocks remain fail closed. | TECHNICAL PASS |
| 23 | Retail/Economy/customer/other components/cable routes remain untouched. | TECHNICAL PASS |
| 24 | Targeted and full EditMode/PlayMode regressions have zero fail/skip/inconclusive. | TECHNICAL PASS |
| 25 | Diff, Repository Guard and universal Mac native gates. | PASS — GUARD 32962078481 |
| 26 | Exact-head clean Windows IL2CPP/only-D3D11 runtime and zero residue. | PASS |
| 27 | Bible/ADR/Evidence/CHANGELOG, canonical/local package, real-human and healthy USB lifecycle. | PENDING |

The current strict count is `26/27` fully passed. Automated human-shaped tests and native smoke are never substituted for the real-human/healthy-USB portions of item 27.

## Pending bounded closure

1. Commit and push the exact nine-file source/docs/verifier closure; update draft PR #96 and require Repository Guard at that exact source/docs head.
2. Create the final source receipt, validate canonical exact `14/14` evidence and create a collision-free immutable local checkpoint.
3. Run one exact-r48 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` for keyboard/mouse and gamepad.
4. Use physical USB only when live disk/volume identity and health are clean. The currently observed Windows D: volume reports `Warning / Full Repair Needed`; no package write, repair or rename is permitted.
5. Only after item 27 is complete may Issue #95 close, Roadmap move to `Done` and draft PR #96 become ready. Parent Epic #10 remains open for the remaining component installation, cable, electrical/POST/OS/QA and later product work.

# Canonical Processor-Cooler BuildKit-to-Four-Point-Retention Assembly Handoff — Checkpoint Evidence

**Date:** 26 August 2026  
**Issue:** [#97](https://github.com/cixanla/PC-Shop-Empire-3D/issues/97)  
**Draft PR:** [#98](https://github.com/cixanla/PC-Shop-Empire-3D/pull/98)  
**Technical head:** `b45806f5a584d219de74be33ed97a580af59fd68`  
**Technical tree:** `6f62c8653ad2c8505e2927ecc80ac6987399e232`  
**Current closure:** source/domain/scene/input/full-regression, exact-head macOS/Windows native and technical-CI gates passed; source/docs, final canonical/local package, healthy physical USB, real-human session and administrative gates pending

## Delivered playable result

GarageGraybox r49 connects the canonical reserved processor cooler in the completed `10/10` custom-PC BuildKit to the existing LGA1700 ProcessorCoolerSlot and four-point retention authority. Handoff starts only after the exact Issue #89 motherboard is live and `SeatedSecured`, Issue #91 CPU is `ProcessorRetained`, Issue #93 DDR5 is `MemoryModuleRetained` in A2 and Issue #95 M.2 is secured in the primary slot. The domain resolves only the accepted work-order/ticket/allocation line, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity cooler instance with `E / Gamepad South`, carries it from BuildKit custody into exact hands, enters the existing guided seat with `Mouse Left / Gamepad RT`, selects either supported 180-degree orientation, commits the seat with `G / Gamepad East`, consumes pre-applied TIM once, retains the four points in `1→3→2→4`, proves retained removal is blocked, unretains in `4→2→3→1`, and detaches that same instance. A fresh-TIM-free reseat is then atomically rejected as `ProcessorCoolerTimConsumed`; the cooler stays in hands with no duplicate, ghost or loss.

This is a custody bridge, not a second Inventory, cooler slot, TIM or retention authority. Existing Issue #58 topology, compatibility, support/RAM/obstruction clearance, preview-equals-commit, consume-once TIM, retain/unretain and replay rules remain the only Assembly truth. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal the item. Reservation/allocation remains live; the original ten staging receipts and visible `10/10` history remain immutable; installed motherboard/CPU/DDR5/M.2 and the other five BuildKit components do not move.

A bounded physical audit found that the staged primary M.2 volume intersected the cooler volume. The source fix raises the M.2 slot to the verified motherboard plane and removes the temporary collision exemption. The final scene contract proves actual collider disjointness for both valid cooler orientations; no solver exception hides the geometry error.

The exact runtime success marker is:

```text
GARAGE_PROCESSOR_COOLER_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained memory=retained storage=secured pickup=exact custody=build-kit-to-hands-to-cooler-slot reservation=alive physical-identity=stable input=keyboard+mouse orientation=180 tim=consumed-once four-point=1-3-2-4 reverse=4-2-3-1 retained-remove-blocked=ok detach=ok consumed-tim-reseat-blocked=ok history=10/10-preserved other-five=untouched receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok
```

Automated native smoke proves the production handoff and invariants; it is not a real-human keyboard/mouse or gamepad acceptance session.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Full EditMode | 726/726 | `editmode.xml` | 603,699 | `e93cdd85d80261e27286ccd2230c0924ccfef132ffb151de59ee7742084227df` |
| Full PlayMode | 133/133 | `playmode.xml` | 408,634 | `afde3a34a9c1196adcfc743aac87bc61e607a8ad3337b16d85e06423f93d6156` |

Both suites have failed, skipped and inconclusive `0`. EditMode duration is `17.8379228` seconds; PlayMode duration is `402.0102964` seconds. The technical tree passes `git diff --check`. Exact-head GitHub Repository Guard [32973861692](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32973861692) passed at `b45806f5a584d219de74be33ed97a580af59fd68`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 615,534 | `9c128786a74367f28f33acf4c010c68f9a0e87e554b4225c90862356a591e8ed` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 9,027 | `16d59b682fbd5fbe8fc0c74bc0a8bc7c1c46d902e4672a93465fe8cc38210f56` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `8654ade61927c9969703c636cef84c1ee01c129f72eae5dfc68d43d3fc69a278` |

The build report is `330,220,810` bytes and the application contains `302` files with the same total file bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits one exact r49 readiness marker including `processor-cooler-assembly-handoff=ready`, one exact success marker, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete technical bundle is `7,630,681` bytes with SHA-256 `2751e62e02d1c8cf5458d282755103bd7f160829f4f3719384d5f4bbd9fe3537`. Its exact technical head was restored into the collision-free detached-clean checkout:

`C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue97-b45806f5a584-r1`

Unity 6000.3.21f1 built a strict x64 IL2CPP development player with only Direct3D11. The build report is `1,344,385,080` bytes; output contains `663` files and `1,344,554,204` bytes. Build log `build-il2cpp-d3d11.log` is `1,582,901` bytes with SHA-256 `a3084b8f172545ea55d93bcab31e1e8ee8a95748aa1880a5c2382e822256e14b`. The expanded native-link/build fatal-token count is `0`, build marker count is `1`, ProjectSettings before/after SHA-256 is byte-exact `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`, and the checkout remains detached-clean at the exact technical head/tree.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | 667,136 | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | 45,635,584 | `54c168cb3d25fd06a671fe03cd1e036a7a67cc1196970f0b54f767e155b555ff` |
| `UnityPlayer.dll` | 84,237,744 | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The interactive scheduled-task player used `Intel(R) Iris(R) Xe Graphics`, forced Direct3D11 and emitted exact host/readiness/success counts `1/1/1`; forbidden count is `0`. It exited `0` after Input System shutdown, graceful close succeeded, scheduled task `PCShop-Issue97-r49-b45806f` was deleted, cleanup was not required, and exact player/Unity/Bee/IL2CPP/task residue is `0`. `runtime-d3d11.log` is `5,906` bytes with SHA-256 `cab136c0844702b04f6c8e58a4fc05b4eaf119f078ec2f4f6f00d23fe29c1112`. The source remains detached-clean at the exact head/tree.

Unity left one Roslyn `VBCSCompiler.dll` descendant after the successful build, causing only the outer `Start-Process -Wait` wrapper to remain open. The process identity, Unity path and build-time creation were checked; only that exact PID was terminated. The build wrapper then completed normally with `exit 0`, wrote a passing receipt, and the final exact residue probe returned `0`. No source, build binary or evidence file was altered by this cleanup.

The three exact procedures are hash-bound by `procedure-manifest.json`:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 10,096 | `4fd098d47149755f1b4490a3f4f17ed1e4e1eb2571e0e1d0b545b510cc9b0364` |
| `launch-procedure.ps1` | 8,055 | `e8c4b5fa586dabcca42a7c205c12c4bacd48e8f470a6df4b73384d081cfe3fc3` |
| `runtime-procedure.ps1` | 15,539 | `ee85d5752c1fb761f7074144db98bdb7c4bc7a9a340d9c586dc82e7ccf85706e` |

The first thirteen immutable canonical artifacts exist at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue97-b45806f5a584-r1/canonical-evidence`; the preliminary technical source receipt will be replaced by the final source/docs provenance receipt for exact `14/14`.

## Issue #97 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical cooler line/product/item/reservation/allocation tuple. | TECHNICAL PASS |
| 2 | Historical `10/10` receipts and owned Issue #89/#91/#93/#95 chain. | TECHNICAL PASS |
| 3 | Exact motherboard Workbench custody and `SeatedSecured` receipts. | TECHNICAL PASS |
| 4 | Exact CPU ProcessorSocket custody and `ProcessorRetained` receipts. | TECHNICAL PASS |
| 5 | Exact DDR5 A2 custody and `MemoryModuleRetained` receipts. | TECHNICAL PASS |
| 6 | Exact primary M.2 custody and captive-screw-secured receipts. | TECHNICAL PASS |
| 7 | Exact compatible capacity-one managed empty/open cooler slot. | TECHNICAL PASS |
| 8 | Stable handoff operation is distinct and bound to the exact staging receipt. | TECHNICAL PASS |
| 9 | Immediate/delayed replay is exactly once. | TECHNICAL PASS |
| 10 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | TECHNICAL PASS |
| 11 | Stale revisions, full hands/slot and overflow are no-mutation. | TECHNICAL PASS |
| 12 | Only registered BuildKit→hands and exact cooler-slot↔hands transfer is accepted. | TECHNICAL PASS |
| 13 | Reservation/allocation remains live through the reversible flow. | TECHNICAL PASS |
| 14 | Ten receipts/history, other five items and installed prerequisites remain untouched. | TECHNICAL PASS |
| 15 | Existing Issue #58 compatibility/TIM/retention/replay authority remains exact. | TECHNICAL PASS |
| 16 | Same Unity instance and stable ItemId survive pickup→detach. | TECHNICAL PASS |
| 17 | Physical/input/orientation/support/RAM/obstruction gates fail closed. | TECHNICAL PASS |
| 18 | Authority-first projection and same-instance recovery are atomic. | TECHNICAL PASS |
| 19 | Generic drop/box/stack/cart/raw-transfer/receipt-free bypasses are blocked. | TECHNICAL PASS |
| 20 | `SOĞUTUCU MONTAJDA`, immutable ticket and `10/10` history remain readable. | TECHNICAL PASS |
| 21 | Keyboard/mouse and Input System gamepad full handoff flow. | TECHNICAL PASS |
| 22 | WASD, mouse-look, pause/focus no-lurch and single-consumer regressions. | TECHNICAL PASS |
| 23 | Retained remove is blocked; reverse unretain and detach preserve the instance. | TECHNICAL PASS |
| 24 | Consumed-TIM reseat rejects atomically with cooler still in hands. | TECHNICAL PASS |
| 25 | Motherboard/CPU/DDR5/M.2 interlocks remain fail closed. | TECHNICAL PASS |
| 26 | Retail/economy/customer/remaining components/cable routes remain untouched. | TECHNICAL PASS |
| 27 | Targeted and full EditMode/PlayMode regressions have zero fail/skip/inconclusive. | TECHNICAL PASS |
| 28 | Diff, Repository Guard and universal Mac native gates. | PASS — GUARD 32973861692 |
| 29 | Exact-head clean Windows IL2CPP/only-D3D11 runtime and zero residue. | PASS |
| 30 | Bible/ADR/Evidence/CHANGELOG, canonical/local package, real-human and healthy USB lifecycle. | PENDING |

The current strict count is `29/30` fully passed. Automated human-shaped tests and native smoke are never substituted for the real-human/healthy-USB portions of item 30.

## Pending bounded closure

1. Commit and push the exact nine-file source/docs/verifier closure; update draft PR #98 and require Repository Guard at that exact source/docs head.
2. Create the final source receipt, validate canonical exact `14/14` evidence and create a collision-free immutable local checkpoint.
3. Run one exact-r49 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` for keyboard/mouse and gamepad.
4. Use physical USB only after live disk/volume identity and health are clean. A mounted path alone is not write authorization.
5. Only after item 30 is complete may Issue #97 close, Roadmap move to `Done` and draft PR #98 become ready. Parent Epic #10 remains open for remaining component installation, cables, electrical/POST/OS/QA and later product work.

# Canonical DDR5 BuildKit-to-A2 Dual-Latch Assembly Handoff — Checkpoint Evidence

**Date:** 26 August 2026
**Issue:** [#93](https://github.com/cixanla/PC-Shop-Empire-3D/issues/93)
**Draft PR:** [#94](https://github.com/cixanla/PC-Shop-Empire-3D/pull/94)
**Technical head:** `0caca090d2859dfb78219abb089274fe599eaca2`
**Technical tree:** `e52c75872a8ec59a98b63c0c46d5e3f6f9c5e084`
**Current closure:** source/domain/scene/input/full-regression, exact-head macOS/Windows native and technical CI gates passed; source/docs, final canonical/local package, healthy physical USB, human and administrative gates pending

## Delivered playable result

GarageGraybox r47 connects the canonical DDR5 UDIMM in the completed `10/10` custom-PC BuildKit to the existing A2/Channel A/Bank 2 MemorySlot and dual-latch flow after the exact Issue #89 motherboard is live and `SeatedSecured` and the exact Issue #91 CPU is live and `ProcessorRetained`. The domain resolves the DIMM only from the exact accepted work-order/ticket/allocation line, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity DIMM instance with `E / Gamepad South`, carries it from BuildKit custody into exact hands, opens the existing keyed DIMM-seat mode with `Mouse Left / Gamepad RT`, commits the notch-aligned A2 seat with `G / Gamepad East`, closes and reopens both latches, detaches and reseats that same instance. Live reservation/allocation remains exact. The original ten staging receipts and visible `10/10` preparation history remain immutable; the secured motherboard, retained CPU and other seven uninstalled BuildKit items do not move.

This is a custody bridge, not a second Inventory, MemorySlot or latch authority. Existing Issue #56 family/topology/orientation/seat/remove/retention authority remains the only DDR5 Assembly truth. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal the handoff item. Domain success precedes physical mutation and projection failure recovers the same object at the authoritative hands or exact A2 pose.

The exact runtime success marker is:

```text
GARAGE_MEMORY_MODULE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained pickup=exact custody=build-kit-to-hands-to-a2 reservation=alive physical-identity=stable input=keyboard+mouse notch=aligned seat=ok dual-latch=closed retained-block=ok open=ok detach=ok reseat=ok history=10/10-preserved other-seven=untouched receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok
```

Automated native smoke proves the production handoff and invariants; it is not a real-human keyboard/mouse or gamepad acceptance session.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Full EditMode | 718/718 | `editmode.xml` | 597,582 | `701b60a909a489974cbbfd5bae6a876c68e4d5b2dd23d51f7d0c2d69d55485f7` |
| Full PlayMode | 125/125 | `playmode.xml` | 375,388 | `0d0c15c20cb8622798b1ca3e5855d6a341a2152f9706f556a08b700d43782f8d` |

Both suites have failed, skipped and inconclusive `0`. The technical tree passes `git diff --check`, the local Repository Guard and exact-head GitHub Repository Guard [32946849858](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32946849858) at head `0caca090d2859dfb78219abb089274fe599eaca2`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 603,443 | `82a16fb55bd845af697470b42ae16ee33f93f78b6c14b8bd019951bc024a46cc` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 8,589 | `7d812d2806cd26d4eb2691bba77a59fe2287a25f9fc7379125d0567894797b73` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `36627528d2bad158581afdcc8d8a92822b108707527d47c028c3251f162f6d6f` |

The build report is `330,173,019` bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits one exact r47 readiness marker including `memory-module-assembly-handoff=ready`, one exact success marker, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete technical bundle is `7,594,847` bytes with SHA-256 `039ec06b79bb807a5b85792149af8c31b51ef8c5de8e878d192be71527d31572`. Its exact technical head was restored into the collision-free detached clean checkout:

`C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue93-0caca09-hardened-v1`

Unity 6000.3.21f1 built a strict x64 IL2CPP development player with only Direct3D11. The build report is `1,342,974,093` bytes; build log `build-il2cpp-d3d11.log` is `1,583,221` bytes with SHA-256 `7124bba7143010ed851ba11d15185c44ae96a486dce971159b3b76b6dae2ee9a`. The expanded native-link/build fatal-token count is `0`, the detached checkout remains at the exact technical head/tree and `git status` remains empty.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | 667,136 | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | 45,578,240 | `3932baee43fbe49b5693f64006527ccb536079b7738c74805e2c91e7f4f25bc0` |
| `UnityPlayer.dll` | 84,237,744 | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The interactive scheduled-task player used Intel Iris Xe, forced Direct3D 11.0 feature level 11.1 and emitted exact host/readiness/success counts `1/1/1`, including `memory-module-assembly-handoff=ready`; forbidden count is `0`. It exited `0` after Input System shutdown, graceful close succeeded, the scheduled task was deleted, cleanup was not required, and player/Unity/task residue is `0`. `runtime-d3d11.log` is `5,732` bytes with SHA-256 `b727412f54471a24fcb9170623cb9405a914352892c51ce6698428c7d6483731`. The first thirteen immutable canonical artifacts now exist locally; only final source/docs provenance receipt remains for `14/14`.

## Issue #93 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical DDR5 line/product/item/reservation/allocation tuple. | TECHNICAL PASS |
| 2 | Historical `10/10` receipts and owned Issue #89 + #91 Assembly chain required. | TECHNICAL PASS |
| 3 | Exact motherboard Workbench custody, `SeatedSecured`, attach/secure receipts. | TECHNICAL PASS |
| 4 | Exact CPU ProcessorSocket custody, `ProcessorRetained`, seat/retain receipts. | TECHNICAL PASS |
| 5 | Exact A2/Channel A/Bank 2/priority-1 capacity-one managed slot is `EmptyOpen`. | TECHNICAL PASS |
| 6 | Stable DIMM handoff operation is distinct and bound to exact staging receipt. | TECHNICAL PASS |
| 7 | Immediate/delayed replay is exactly once. | TECHNICAL PASS |
| 8 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | TECHNICAL PASS |
| 9 | Stale revisions, full hands, occupied slot and overflow are no-mutation. | TECHNICAL PASS |
| 10 | Inventory permits only registered BuildKit→hands and A2↔hands transfer. | TECHNICAL PASS |
| 11 | Reservation/allocation remains live and exact through the reversible flow. | TECHNICAL PASS |
| 12 | Ten receipts/history, other seven items, motherboard and CPU remain untouched. | TECHNICAL PASS |
| 13 | Existing Issue #56 topology, seat/remove and latch semantics remain authoritative. | TECHNICAL PASS |
| 14 | Same Unity instance and stable ItemId survive pickup→reseat. | TECHNICAL PASS |
| 15 | Pickup and slot physical/input gates fail closed. | TECHNICAL PASS |
| 16 | Domain commit precedes physical mutation; same-instance recovery is authoritative. | TECHNICAL PASS |
| 17 | Generic transfer/drop/box/stack/cart and receipt-free bypasses are blocked. | TECHNICAL PASS |
| 18 | `DDR5 MONTAJDA`, immutable ticket and `10/10` history remain readable. | TECHNICAL PASS |
| 19 | Keyboard/mouse and real Input System gamepad full handoff flow. | TECHNICAL PASS |
| 20 | WASD, mouse-look, pause/no-lurch and single-consumer input regressions. | TECHNICAL PASS |
| 21 | Motherboard/DIMM/CPU interlocks remain fail closed. | TECHNICAL PASS |
| 22 | Retail/Economy/customer/other components/cable routes remain untouched. | TECHNICAL PASS |
| 23 | Targeted and full EditMode/PlayMode regressions have zero fail/skip/inconclusive. | TECHNICAL PASS |
| 24 | Diff, Repository Guard and Universal Mac native gates. | PASS — GUARD 32946849858 |
| 25 | Exact-head clean Windows IL2CPP/only-D3D11 runtime and zero residue. | PASS |
| 26 | Bible/ADR/Evidence/CHANGELOG, canonical/local package, human and healthy USB lifecycle. | PENDING |

The current strict count is `25/26` fully passed. Automated human-shaped tests and native smoke are never substituted for the human/USB portions of item 26.

## Pending bounded closure

1. Commit and push the exact nine-file source/docs/verifier closure; update draft PR #94 and require Repository Guard at that exact source/docs head.
2. Create the final source receipt, validate canonical `14/14` evidence and create a collision-free immutable local checkpoint.
3. Use physical USB only when live disk/volume identity and health are clean. The currently observed Windows D: volume is Dirty and `Full Repair Needed`; therefore no package write is allowed.
4. Run one exact-r47 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md`. Until human and healthy USB pass, keep Issue #93 open/In Progress and PR #94 draft/not-ready.
5. Only after `26/26`, close Issue #93 and set its Roadmap item `Done`. Parent Epic #10 remains open for the remaining installation, cable, electrical/POST/OS/QA and later product work.

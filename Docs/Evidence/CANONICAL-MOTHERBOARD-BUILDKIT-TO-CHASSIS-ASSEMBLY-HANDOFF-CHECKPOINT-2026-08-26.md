# Canonical Motherboard BuildKit-to-Chassis Assembly Handoff — Checkpoint Evidence

**Date:** 26 August 2026
**Issue:** [#89](https://github.com/cixanla/PC-Shop-Empire-3D/issues/89)
**Technical head:** `2fdf371206bc58c32e1c20d471f4abe7c0bfba01`
**Technical tree:** `c5e6de5942993a98735984caca4a04fd396105f6`
**Current closure:** source/domain/scene/input/full-regression, technical Repository Guard and exact-head macOS/Windows native gates passed; source/docs CI, final source receipt, immutable package, healthy physical USB, human-player and Issue/Project administrative gates pending

## Delivered playable result

GarageGraybox r45 connects the completed canonical `10/10` custom-PC BuildKit to the existing motherboard Assembly flow. The domain resolves the motherboard only from the exact accepted work-order/ticket/allocation line, product, serialized item and reservation tuple and verifies the authoritative ten-receipt staged aggregate before handoff.

The player takes the same Unity motherboard instance with `E / Gamepad South`, carries it from BuildKit custody into exact hands, opens the existing guided motherboard-seat mode, commits the keyed supported/clear pose, secures the canonical fastener, then unsecures, detaches and reseats that same instance. Live reservation/allocation remains exact. The original ten staging receipts and visible `10/10` preparation history remain immutable; the other nine items, slots, receipts and revisions do not move.

This is a custody bridge, not a second Inventory or Assembly. Existing Issue #53 seat/detach and Issue #54 secure/unsecure authority remains the only chassis truth. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal the handoff item. Domain success precedes physical mutation and a projection failure recovers the same object at the authoritative hands or seat pose.

The exact runtime success marker is:

```text
GARAGE_MOTHERBOARD_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 pickup=exact custody=build-kit-to-hands-to-workbench reservation=alive physical-identity=stable input=keyboard+mouse guided-seat=ok secure=ok unsecure=ok detach=ok reseat=ok history=10/10-preserved other-nine=untouched receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok
```

Automated native smoke proves the production handoff and invariants; it is not a real-human keyboard/mouse or gamepad acceptance session.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Full EditMode | 712/712 | `editmode.xml` | 592,850 | `266a6b6d503f696a1415e8354ad91fcc05b32f6ea77a19ba909d2a22178d6ac7` |
| Full PlayMode | 119/119 | `playmode.xml` | 349,306 | `2a757c2540d121ddd2bb75fe993cdc048c52dd4f1bb21b8410b5531a0405c212` |

Both suites have failed, skipped and inconclusive `0`. The committed technical tree passes `git diff --check`. Technical Repository Guard [32930403290](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32930403290) passed at the exact technical head.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 589,426 | `f0f046deee2441572b4fd3eb1c82319b01d02b6aa7ee7c4e9f9d48b954457c8f` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 8,385 | `4f6b80172ef6cb64dde45d77f015cb1bb4387d8d72b3d86ffeb787cf39476f83` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `9e381270df3f9a5d1b1a3849179d1137566a148a4f66ddd0b1e61b061ed8384b` |

The build report is `330,104,684` bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits one exact r45 readiness marker including `motherboard-assembly-handoff=ready`, one exact success marker, zero `assembly-handoff-flow=failed`, reaches Input System `Shutdown`, exits gracefully and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,534,603` bytes with SHA-256 `04e6c66df4f0c08d442f4fbe7449f9686033d3d352fa46d32a777ffbc5a13e01`. It exposes one branch head and produced a detached, clean Windows checkout at the exact technical commit/tree in:

`C:\Users\mertk\Developer\PCShopEmpire3D\WindowsValidation\issue89-2fdf371-cold-v1`

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/D3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,609 | `03637ac37b83a68e5b3f048f5e114847305b8edd5df8d2a59e9a67425ec4f7e2` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `d0307ae6a2f528a23ea4f55c1707a29b3af60846b11e988481f86de6aca36438` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,619 | `8314fc25f7ec4f79c0ba898f311672c22c8365e88cf8163471ac44eecd6999c2` |
| Runtime summary | Accepted, graceful, residue-free | `runtime-summary.json` | 3,293 | `6a8d20f79da1d21e333173e76a10eaa8db6d8f9a57512f11be0fa45fdf893aa6` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,696 | `a3178801ac127dfe0be766ba7c9745c9ab91ca5a27d9b91cb708742e38ec99bc` |

The strict `issue89-hardened-v1` build reports `1,340,592,635` bytes and expanded fatal-token count `0`. ProjectSettings is byte-exact before/after. Exact binaries are:

- `PC Shop Empire 3D.exe`: `667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: `45,479,936` bytes, `43115f7efac1f853500104b5b4db65fee8eba2ecb07e085699c51eb91063445a`.
- `UnityPlayer.dll`: `84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

Intel Iris Xe reports Direct3D 11.0 feature level 11.1 at 1280×720. Host, r45 readiness and exact motherboard Assembly success occur once; forbidden count is `0`. Player exit is `0`, graceful shutdown is true, scheduled task `PSE-Issue89-2fdf371-R1` is deleted, cleanup is not required and player/Unity/task residue is `0`. The checkout remains detached-clean at the exact commit/tree after runtime.

Procedure source and Windows readback are exact:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,625 | `c05fadc4dfe7cbecbaa908feb3c83b5dc161232d216667548a4c45948eeb05b8` |
| `launch-procedure.ps1` | 7,956 | `9089d742281795b0cb32b3c1fdfa7ec00e1450b1e665d8db6fc0d7d2b8889cb0` |
| `runtime-procedure.ps1` | 14,811 | `1f8bba0978da335d1586f61b65c9aa455ed03e9995b22dc547564ab26b62c33f` |
| `procedure-manifest.json` | 679 | `fd93a8cc2da783b725b0575de255e18c918618250ed09125bef726f5c6862a82` |

## Issue #89 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact work order and full motherboard line/product/item/reservation tuple. | TECHNICAL PASS |
| 2 | Authoritative historical `10/10` staged receipt aggregate required. | TECHNICAL PASS |
| 3 | Stable handoff operation distinct from staging, attach and fastener operations. | TECHNICAL PASS |
| 4 | Immediate/delayed replay is exactly-once. | TECHNICAL PASS |
| 5 | Foreign/value-equal identity and receipt forgeries fail closed. | TECHNICAL PASS |
| 6 | Drift, stale revision, full hands, workbench and overflow failures are no-mutation. | TECHNICAL PASS |
| 7 | Inventory permits only registered BuildKit→hands and exact workbench↔hands transfer. | TECHNICAL PASS |
| 8 | Reservation/allocation remains live and exact throughout reversible custody. | TECHNICAL PASS |
| 9 | Ten original receipts/history and the other nine components remain untouched. | TECHNICAL PASS |
| 10 | Existing Assembly seat/detach and fastener semantics are reused unchanged. | TECHNICAL PASS |
| 11 | Same Unity instance and stable ItemId survive pickup→reseat. | TECHNICAL PASS |
| 12 | Pickup and seat physical/input gates fail closed. | TECHNICAL PASS |
| 13 | Domain commit precedes physical mutation; same-instance recovery is authoritative. | TECHNICAL PASS |
| 14 | Generic transfer/drop/box/stack/cart and receipt-free Assembly bypasses are blocked. | TECHNICAL PASS |
| 15 | `ANAKART MONTAJDA`, immutable work ticket and `10/10` history remain readable. | TECHNICAL PASS |
| 16 | Keyboard/mouse and real Input System gamepad full handoff flow. | TECHNICAL PASS |
| 17 | WASD, mouse-look, pause/no-lurch and single-consumer input regressions. | TECHNICAL PASS |
| 18 | Dependent component installation remains blocked until motherboard secure. | TECHNICAL PASS |
| 19 | Quote/customer/Retail/Economy/other nine/cable-route state untouched. | TECHNICAL PASS |
| 20 | Full EditMode/PlayMode regressions with zero fail/skip/inconclusive. | TECHNICAL PASS |
| 21 | Diff, technical Repository Guard and Universal Mac native gates. | TECHNICAL PASS |
| 22 | Exact-head clean Windows IL2CPP/only-D3D11 runtime and zero residue. | TECHNICAL PASS |
| 23 | Project Bible, ADR, Evidence, CHANGELOG and procedure-bound canonical evidence. | SOURCE/DOCS PENDING |
| 24 | Collision-free immutable local checkpoint with two full readbacks. | LOCAL PACKAGE PENDING |
| 25 | Exact-r45 real-human keyboard/mouse + gamepad and healthy physical USB lifecycle. | HUMAN + HEALTHY USB PENDING |

The current strict count is `22/25` fully passed. Automated human-shaped tests and native smoke are never substituted for item 25.

## Canonical evidence state and pending bounded closure

The thirteen promoted technical artifacts are byte/SHA-exact in `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue89-2fdf371206bc/canonical-evidence`. `source-receipt.json` is deliberately absent, so the final canonical contract remains `13/14` until a clean source/docs commit and Repository Guard result can be bound without circular provenance.

1. Commit and push the exact nine-file source/docs/verifier closure; update draft PR #90 and require Repository Guard at the exact source/docs head.
2. Create the final source receipt, validate the canonical `14/14` evidence and create a collision-free immutable local checkpoint.
3. Use physical USB only when live disk/volume identity and health are clean. Then perform collision-free `.incoming-*` copy, full path/size/hash readback, atomic rename and second full readback; never overwrite an older milestone.
4. Run one exact-r45 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md`. Until human and healthy USB pass, keep Issue #89 open/In Progress and PR #90 draft/not-ready.
5. Only after `25/25`, close Issue #89 and set its Roadmap item `Done`. Parent Epic #10 remains open for the remaining component installation, cable routing, electrical/POST/OS/QA and later product work.

# Canonical Graphics-Card BuildKit-to-PCIe x16 Retention Assembly Handoff — Checkpoint Evidence

**Date:** 26 August 2026<br>
**Issue:** [#99](https://github.com/cixanla/PC-Shop-Empire-3D/issues/99)<br>
**PR:** [#100](https://github.com/cixanla/PC-Shop-Empire-3D/pull/100)<br>
**Technical head:** `d5532bb921b94715dbb1ed2006092a9542b139a4`<br>
**Technical tree:** `6f00d7fb23a305e1e9eb4241dd3cba57a5e076dd`<br>
**Current closure:** source/domain/scene/input/full-regression, exact-head macOS/Windows native, technical/source-docs CI, final canonical evidence and large local/healthy physical-USB lifecycle passed; real-human session and administrative gates pending

## Delivered playable result

GarageGraybox r50 connects the canonical reserved graphics card in the completed `10/10` custom-PC BuildKit to the existing Assembly-owned PCIe x16 GraphicsCardSlot, slot latch and rear-bracket fastener authority. Handoff starts only after the exact Issue #89 motherboard is live and `SeatedSecured`, Issue #91 CPU is `ProcessorRetained`, Issue #93 DDR5 is `MemoryModuleRetained` in A2, Issue #95 M.2 is secured in the primary slot and Issue #97 processor cooler is `CoolerRetained`. The domain resolves only the accepted work-order/ticket/allocation line, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity GPU instance with `E / Gamepad South`, carries it from BuildKit custody into exact hands, enters the existing guided PCIe x16 seat with `Mouse Left / Gamepad RT`, proves invalid orientation is blocked, commits the seat with `G / Gamepad East`, retains the slot latch and rear bracket, proves retained removal is blocked, unretains, detaches and reseats that same instance. Keyboard/mouse and a real Input System gamepad each complete the reversible cycle. A current obstruction blocks recovery without duplication or loss; clearing the obstruction lets the same instance recover exactly once.

This is a custody bridge, not a second Inventory, GPU, slot, latch, rear bracket or cable route authority. Existing Issue #59 compatibility/orientation/seat/retention/remove/replay rules remain the only GraphicsCard Assembly truth. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal the item. Reservation/allocation remains live; the original ten staging receipts and visible `10/10` history remain immutable; installed motherboard/CPU/DDR5/M.2/cooler and the other four BuildKit components do not move.

Issue #63 PCIe GPU power-cable authority remains independent. The final domain tests and r50 native smoke snapshot its exact item, product, container, state, revision, receipt and operation identities across pickup, seat, retain, unretain and detach. The handoff neither routes nor mutates the cable; an already-routed cable continues to block unretain/remove fail closed.

The exact runtime success marker is:

```text
GARAGE_GRAPHICS_CARD_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained memory=retained storage=secured cooler=retained prerequisite-setup=assisted pickup=exact custody=build-kit-to-hands-to-pcie-x16 reservation=alive physical-identity=stable gpu-input=keyboard+mouse orientation-invalid=blocked seat=ok slot-latch=retained rear-bracket=secured retained-remove-blocked=ok unretain=ok detach=ok reseat=ok history=10/10-preserved other-four=untouched pcie-power-cable=untouched receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok
```

Automated native smoke proves the production handoff and invariants. Its `prerequisite-setup=assisted` token is an explicit boundary: it is not a real-human keyboard/mouse or gamepad acceptance session.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 | Duration |
|---|---:|---|---:|---|---:|
| Full Mac EditMode | 733/733 | `editmode.xml` | 609,373 | `ade914da857ee17e0caf03b8c6aec33976f17cc962b58a1db2abaa85d0c62efa` | 15.6504669 s |
| Full Mac PlayMode | 137/137 | `playmode.xml` | 425,419 | `216df19cb10eea6a34c4c05e6791f8ca10db0372cf633d6148dd28c09aa6685a` | 711.1051021 s |

Both full suites have failed, skipped and inconclusive `0`. The technical tree passes `git diff --check`. Exact-head GitHub Repository Guard [32990791761](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32990791761) and [32990807874](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32990807874) passed at `d5532bb921b94715dbb1ed2006092a9542b139a4`.

The bounded P1 and physical-input sets were independently rerun on both platforms:

| Platform | Suite | Result | Bytes | SHA-256 | Duration |
|---|---|---:|---:|---|---:|
| macOS | Targeted EditMode | 5/5 | 8,505 | `be440867b1b572c25afb1c81c8c69b044cd267179c5758983a6bd6eadcee477e` | 3.0973616 s |
| macOS | Targeted PlayMode | 4/4 | 15,380 | `0a4f45e95224bf28b8e996e0bb9c16ed3d40ec694883f647690f51316491edc3` | 28.2358164 s |
| Windows | Targeted EditMode | 5/5 | 8,562 | `561ab7323b09fcb205d62d9797a94d3ff49b85142d6fa7df81317199151e2a72` | 2.6990114 s |
| Windows | Targeted PlayMode | 4/4 | 15,433 | `28797fc2ccd0671288a7e328a25bf6eeaaf2b0eeefeee75164336285c7552522` | 26.4091001 s |

The five-test EditMode set proves stale BuildKit, Inventory and Assembly revision no-mutation, the full BuildKit→hands→PCIe retention cycle's independent PCIe power-cable authority, and the authored GarageGraybox rig contract. The four-test PlayMode set proves keyboard/mouse, gamepad, same-instance recovery exactly once and obstruction fail-closed recovery. Windows PlayMode did not use `-runSynchronously`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 588,907 | `7b05cccc7712bd92cec55be830361bab294cd26ed509f71c1920cff7b93873ee` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 9,112 | `d89b39b95b625de4d999bd2cb2370189dc19b4359f3c1890b1df7043b78e91a3` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `b87aa19baca1f1567f657a51ebfbd3a113ee2c386ed743f3b546e1bfbebb7c47` |

The build report is `330,251,472` bytes and the application contains `302` files with the same total file bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits one exact r50 readiness marker including `graphics-card-assembly-handoff=ready`, one exact success marker, zero handoff-failure/diagnostic/exception markers, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,657,509` bytes with SHA-256 `4afa893177e307853a7bbdef56caa879eb45aa6554f51f75d0ca182bae7adc74`. Windows readback matched that hash before the bundle restored exact technical head `d5532bb921b94715dbb1ed2006092a9542b139a4` into the canonical clean clone at `C:\Users\mertk\Developer\PCShopEmpire3D\Game`. Final head/tree matched the technical contract and `git status --porcelain` was empty.

Unity 6000.3.21f1 built a strict x64 IL2CPP development player with only Direct3D11. The build report is `1,350,250,674` bytes; output contains `663` files and `1,350,419,408` bytes. Build log `build-il2cpp-d3d11.log` is `493,211` bytes with SHA-256 `324feb029a2dce6d928f8d4ceb0cbd4fa29eaafcbdb1249507339df02dfe3489`. Expanded native-link/build fatal-token count is `0`, the exact build marker count is `1`, and the marker proves `scripting-backend=IL2CPP graphics-api=Direct3D11 settings-restored=ok project-settings=byte-exact`.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | 667,136 | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | 45,675,520 | `a3c6f6eb2587889c2ebd4d9cb70d096196e12ba89da6d518a22480ed066831e0` |
| `UnityPlayer.dll` | 84,237,744 | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted interactive task ran as logged-on `cixanla\mertk` with `LogonType=Interactive` and `RunLevel=Limited`. It used `-force-d3d11 -pse-require-d3d11 -pse-graphics-card-assembly-handoff-smoke -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. Intel Iris Xe Direct3D 11 runtime has exact host/readiness/success counts `1/1/1`; force-D3D11/version/renderer/shutdown counts are `1/1/1/1`; failure, diagnostic, exception and forbidden counts are `0`. Player exit and task result are `0`; the task was deleted; cleanup was not required; final player/Unity/build/validation-PowerShell/task residue is `0`; Git remains detached-clean.

`runtime-d3d11.log` is `5,987` bytes with SHA-256 `cc8ab4f5696a41bef9ec50077da5b70e3821f216573ca347c905009ff92b1787`. The first direct SSH launch was deliberately rejected as evidence after a bounded `356.4` second non-interactive harness stall with no smoke marker; only its owned PID and crash-handler child were terminated. Three subsequent malformed task-action attempts never started a wrapper or player and were deleted. Inspection found their transferred wrapper had lost Windows backslashes. The accepted run used a separately named, Mac→Windows SHA-readback-bound fixed wrapper and canonical `Register-ScheduledTask` API. Failed harness attempts are preserved in raw evidence and are not counted as product failures or PASS runs.

The three exact procedures are hash-bound by `procedure-manifest.json`:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 10,070 | `25c4bc73cf3015cac2189639ca396698fadfa4fa2197f31c5afdfd1e0b9e1c61` |
| `launch-procedure.ps1` | 3,520 | `a7a5e99c87a92cb39761aa98b017c0323e6cba070ff92af32843d54ed0c4ad6a` |
| `runtime-procedure.ps1` | 2,431 | `6d2ec577ca8246fd24c633310f44f0bfb7cbd2ff3463c53161a2c3dde3487a3d` |

The first thirteen immutable canonical artifacts returned to the Mac with exact size/hash readback. Exact nine-file source/docs commit `0f259605da017e874863da2646eef6a90898816f`, tree `c0f5a07ac65baf959d4279d7a782a9827b369336` and [Repository Guard 32995195634](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32995195634) then authorized final `source-receipt.json` (`5,120` bytes, SHA-256 `e1d3e205b79892abcb02d78ed4fc96fdc5452737d27a66fc2ea91a6312cacc6d`). Promoted-artifact path/size/hash equality is `13/13`; canonical evidence is exact `14/14` at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue99-d5532bb921b-r1/canonical-evidence`.

The user-requested large immutable local final is `/Users/cixanla/Developer/PCShopEmpire3D/CheckpointStaging/2026-08-26_STAGE_B_CANONICAL_GRAPHICS_CARD_BUILDKIT_TO_PCIE_X16_RETENTION_ASSEMBLY_HANDOFF_HARDENED_V1_LARGE`. Its unique local `.incoming-*` target passed the exact Issue #99 verifier before same-filesystem atomic rename; the final path passed a second full readback. Neither target overwrote a previous checkpoint.

Physical final is `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-26_STAGE_B_CANONICAL_GRAPHICS_CARD_BUILDKIT_TO_PCIE_X16_RETENTION_ASSEMBLY_HANDOFF_HARDENED_V1_LARGE` on external physical USB `Alu Line`, `/dev/disk4s1`, ExFAT label `cixanla`, UUID `CB0AC8C9-4E97-3BE4-94AD-6406A057C00B`. SMART is unsupported by the bridge; both pre-write and post-write read-only `fsck_exfat -n` checks report the volume appears OK with exit code `0`. The unique physical `.incoming-*` target passed a complete readback, was atomically renamed without collision and the final path passed the same complete second readback.

All four local/physical incoming/final results are identical: manifest `1090/1090`, exact Git source `1075/1075`, evidence `14/14`, source/docs commit/tree exact, `20,899,041` payload bytes and manifest SHA-256 `0c2a26b5ef33cdcb79a2e06f5c97eea323be3520adac5886013dc7d7c0b15223`. Package file count is `1,092` including both manifest files. Local/physical incoming residue, internal AppleDouble, incoming sidecar and exact final sidecar counts are `0`; older milestones and unrelated USB data were not modified.

## Issue #99 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical GPU line/product/item/reservation/allocation tuple. | TECHNICAL PASS |
| 2 | Historical `10/10` receipts and owned Issue #89/#91/#93/#95/#97 chain. | TECHNICAL PASS |
| 3 | Exact motherboard Workbench custody and `SeatedSecured` receipts. | TECHNICAL PASS |
| 4 | Exact CPU ProcessorSocket custody and `ProcessorRetained` receipts. | TECHNICAL PASS |
| 5 | Exact DDR5 A2 custody and `MemoryModuleRetained` receipts. | TECHNICAL PASS |
| 6 | Exact primary M.2 custody and captive-screw-secured receipts. | TECHNICAL PASS |
| 7 | Exact cooler custody and `CoolerRetained` four-point receipts. | TECHNICAL PASS |
| 8 | Exact capacity-one managed empty/open PCIe x16 slot and matching retention topology. | TECHNICAL PASS |
| 9 | Stable GPU handoff operation is distinct and bound to the exact staging receipt. | TECHNICAL PASS |
| 10 | Immediate/delayed replay is exactly once. | TECHNICAL PASS |
| 11 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | TECHNICAL PASS |
| 12 | Stale BuildKit/Inventory/Assembly revisions, full hands/slot and overflow are no-mutation. | TECHNICAL PASS |
| 13 | Only registered BuildKit→hands and exact GPU-slot↔hands transfer is accepted. | TECHNICAL PASS |
| 14 | Reservation/allocation remains live through the reversible flow. | TECHNICAL PASS |
| 15 | Ten receipts/history, other four items and installed prerequisites remain untouched. | TECHNICAL PASS |
| 16 | Existing Issue #59 GPU authority and Issue #63 cable route remain exact/independent. | TECHNICAL PASS |
| 17 | Same Unity instance and stable ItemId survive pickup→detach. | TECHNICAL PASS |
| 18 | Physical/input/orientation/support/chassis/cooler/obstruction gates fail closed. | TECHNICAL PASS |
| 19 | Authority-first projection and same-instance recovery are atomic. | TECHNICAL PASS |
| 20 | Generic drop/box/stack/cart/raw-transfer/receipt-free bypasses are blocked. | TECHNICAL PASS |
| 21 | `GPU MONTAJDA`, immutable ticket and `10/10` history remain readable. | TECHNICAL PASS |
| 22 | Keyboard/mouse and Input System gamepad full handoff flow. | TECHNICAL PASS |
| 23 | WASD, mouse-look, pause/focus no-lurch and single-consumer regressions. | TECHNICAL PASS |
| 24 | Retained remove is blocked; reverse unretain and detach preserve the instance. | TECHNICAL PASS |
| 25 | Motherboard/CPU/DDR5/M.2/cooler interlocks remain fail closed. | TECHNICAL PASS |
| 26 | Routed PCIe cable blocks removal while this issue never creates/mutates a route. | TECHNICAL PASS |
| 27 | Retail/economy/customer/remaining components/cable routes remain untouched. | TECHNICAL PASS |
| 28 | Targeted and full EditMode/PlayMode regressions have zero fail/skip/inconclusive. | TECHNICAL PASS |
| 29 | Diff, Repository Guard and universal Mac native gates. | PASS — GUARD 32990791761/32990807874 |
| 30 | Exact-head clean Windows IL2CPP/only-D3D11 runtime and zero residue. | PASS |
| 31 | Bible/ADR/Evidence/CHANGELOG, canonical/large local checkpoint, real-human and healthy USB lifecycle. | PARTIAL — DOCS/CANONICAL/LOCAL/USB PASS; HUMAN PENDING |

The current strict count remains `30/31` fully passed because item 31 is composite and the real-human portion is still open. Automated human-shaped tests and native smoke are never substituted for that session.

## Pending bounded closure

1. Commit and push this physical-lifecycle metadata and require Repository Guard at that exact metadata head.
2. Run one exact-r50 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` for keyboard/mouse and gamepad, including movement/camera/window, physical edge cases and the 15-minute endurance pass.
3. Only after item 31 is complete may Issue #99 close, Roadmap move to `Done` and PR #100 become merge-ready. Parent Epic #10 remains open for PSU/cable installation, electrical/POST/OS/QA and later product work.

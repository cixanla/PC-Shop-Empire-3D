# Canonical Power-Supply BuildKit-to-Bay Four-Fastener Assembly Handoff — Checkpoint Evidence

**Date:** 27 August 2026<br>
**Issue:** [#102](https://github.com/cixanla/PC-Shop-Empire-3D/issues/102)<br>
**PR:** [#103](https://github.com/cixanla/PC-Shop-Empire-3D/pull/103)<br>
**Technical head:** `740a8869e2efc1f525b9560d4d5638343c957eb5`<br>
**Technical tree:** `d64e70bb6bd2d7f0d8583555146050f7060db0f2`<br>
**Branch:** `feature/issue102-power-supply-buildkit-to-bay-retention`<br>
**Current state:** source/domain/scene/input/full-regression, exact-head macOS/Windows native, foreground Windows OS-input and source/docs Guard passed; PR #103 merge/main integration is the remaining administrative gate at this document revision

## Delivered playable result

GarageGraybox r51 connects the canonical reserved ATX PS/2 power supply in the completed `10/10` custom-PC BuildKit to the existing Assembly-owned `PowerSupplyBay`, rear mount and four-fastener authority. Handoff starts only after the exact motherboard is secured, CPU retained, DDR5 retained in A2, M.2 secured in the primary slot, processor cooler retained and graphics card retained in PCIe x16. The domain resolves only the accepted work-order/ticket/allocation line, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity PSU instance with `E / Gamepad South`, carries it from exact BuildKit custody into exact hands, enters the existing guided PSU-bay seat with `Mouse Left / Gamepad RT`, rotates only between two keyed fan orientations, proves invalid orientation is blocked, commits the seat with `G / Gamepad East`, completes the stable four-fastener retention, proves retained removal is blocked, reverses retention, detaches and reseats that same instance. Keyboard/mouse and Input System gamepad automation complete the reversible cycle. Current obstruction blocks recovery without duplication or loss; clearing it lets the same instance recover exactly once.

This is a custody bridge, not a second Inventory, PSU, bay, rear mount, fastener or cable-route authority. Existing Issue #60 compatibility/orientation/seat/retention/remove/replay rules remain the only PSU Assembly truth. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal the item. Reservation/allocation remains live; the original ten staging receipts and visible `10/10` history remain immutable; installed motherboard/CPU/DDR5/M.2/cooler/GPU do not move.

Issue #61/#62/#63 power-cable authorities remain independent. Final domain tests and r51 native smoke preserve all three exact item/product/container/state/revision/receipt/operation lineages across pickup, seat, retain, unretain and detach. The handoff neither routes nor mutates a cable; any existing route continues to block unretain/remove fail closed.

The exact runtime success marker is:

```text
GARAGE_POWER_SUPPLY_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained memory=retained storage=secured cooler=retained graphics-card=retained prerequisite-setup=assisted pickup=exact custody=build-kit-to-hands-to-psu-bay reservation=alive physical-identity=stable input=keyboard+mouse orientation-invalid=blocked seat=ok four-screw=retained retained-remove-blocked=ok unretain=ok detach=ok reseat=ok history=10/10-preserved cables=3/3-untouched receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok
```

`prerequisite-setup=assisted` is an explicit automation boundary. It is not a real-human or physical-device claim.

## Exact source and tests

| Platform | Gate | Result | Duration | Bytes | SHA-256 |
|---|---|---:|---:|---:|---|
| macOS | Targeted EditMode | `6/6` | `6.4901228 s` | `9,657` | `6f98fb96fa77ceb06af4f9fd775597bb1b72f7ed62214211c40020506c3ba218` |
| macOS | Targeted PlayMode | `5/5` | `71.4146464 s` | `18,386` | `69344b622e5ee64ae860fd49048a232c22841e640c399955d5369ef4109fa2d9` |
| macOS | Scene contract | `1/1` | `1.0262815 s` | `4,294` | `713e70dcaf7564a9a5fd298bebf7e486c08243f02f0f8fe77a50c1d8a761542c` |
| macOS | Runtime readiness | `1/1` | `13.3801433 s` | `6,224` | `fa2c4d0853dd22dcf402215767775305ee2612e039c0296f4cff6bc113cf36b7` |
| macOS | Recovery diagnostic | `1/1` | `12.1499671 s` | `6,189` | `ec3152853ddd9662db795c950de5f9d2837416f87f0f757cd6b041b5754af952` |
| macOS | Full EditMode | `739/739` | `37.5614243 s` | `614,125` | `23b9d191e10a30312d16f0f32e8e199a48917ecc603bb2e64ea6998461fdf7ab` |
| macOS | Full PlayMode | `144/144` | `704.8890219 s` | `451,774` | `901943c2e4d77e52189ca70ab42b67442361910c8c064c23c1aeb6f9caf5cdfe` |
| Windows | Full EditMode | `739/739` | `17.7648565 s` | `618,831` | `f216d359d72b1e24883b82810eb9068deeb564db4b0a067ad3f5134913bceb1b` |
| Windows | Full PlayMode | `144/144` | `416.5919464 s` | `453,371` | `53be627d4426df83f1e8e8da69a38c7c787a607232db1c2b58074a63e3066db2` |

Every accepted XML has failed, skipped and inconclusive `0`. The first Mac full EditMode run correctly exposed one old scene-contract expectation after the production tray geometry opened the PSU chamber; only that authored test expectation was updated. The final full run is `739/739`. An earlier command with `-quit` produced no XML and is not counted. Windows Unity allocator lifetime warnings are retained in logs; the exact XMLs pass and no compiler/fatal product error is present.

`git diff --check` and local Repository Guard pass. Exact-head technical Repository Guard [33027397901](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33027397901) passed at `740a8869e2efc1f525b9560d4d5638343c957eb5`.

Source/docs commit `988591c18dd5fbbdcb2f16146cc1330daec87657`, tree `bc9c189807abf1c0aa5ac2c8b65b8760e10d9797` and Repository Guard [33029851072](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33029851072) passed.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | `603,483` | `1438cc23a7bf358aa9003d4c1e6e4cb0855f1433a122b22b468eba0da871eaa6` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | `9,090` | `c61700e23cf5e853c2b42b5142b76d105cc202302b8cc77fd952a2ce6babc667` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | `86,863` | `bcf5b95f27419683dae6572219f822ddaac03b45771ac3c53fd939d5bcfc1464` |

The build report is `330,279,904` bytes and the application contains `302` files with the same total logical bytes. `file` confirms Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits one exact r51 readiness marker including `power-supply-assembly-handoff=ready`, one exact success marker, zero handoff failure/exception markers, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,632,290` bytes with SHA-256 `3936b661bd1dc711099838949613f745729f30719320632a0ee2c5de535bb53f`. Windows readback matched that hash before restoring a collision-free detached-clean clone at `C:\Users\mertk\AppData\Local\Temp\PCShopIssue102-740a8869-r1\Game`. Final head/tree remain exact technical source and `git status --porcelain` is empty.

Unity 6000.3.21f1 built an x64 IL2CPP Development player with Direct3D11 only. The build report is `1,346,115,186` bytes. `build-il2cpp-d3d11.log` is `566,005` bytes with SHA-256 `c57cc92489b01d42e18f54680de7ec8c7b934426daa50f971dffe06c2081c959`; expanded compiler/Burst/AOT/native-link fatal count is `0`; exact success marker count is `1`; `ProjectSettings.asset` returns byte-exact to SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | `667,136` | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | `45,711,872` | `a7d5a7d57f59f35cb3f973a6fa60e25391319c5de27e8d0ca43f466bdd4f5f4b` |
| `UnityPlayer.dll` | `84,237,744` | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted runtime task ran as logged-on `cixanla\mertk`, `LogonType=Interactive`, `RunLevel=Limited`, using `-force-d3d11 -pse-require-d3d11 -pse-power-supply-assembly-handoff-smoke -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. Intel Iris Xe reports Direct3D 11.0 feature level 11.1. Host/readiness/success counts are `1/1/1`; forbidden count is `0`; player and task results are `0`; graceful Input System shutdown occurs; the task is deleted; player residue is `0`.

`runtime-issue102-r51-interactive.log` is `6,012` bytes / SHA-256 `fea28753237929a9951c408edb3c9b9a61b443bf217740caa1cb669bd952775a`; its receipt is `863` bytes / `8b2303e533c7eba70cfe5d80a00103b6c7fb6eafa957ca214e78a57b52d4513c`. Unity's child `VBCSCompiler` retained the build wrapper handle after Unity had already emitted the exact success marker and exited `0`; its command line was verified as Unity-bundled Roslyn and only that validation-owned PID was terminated. Final audit finds no Unity, player, compiler, validation PowerShell or scheduled-task residue.

Windows `ProjectVersion.txt` has `i/lf w/crlf attr/text=auto` under the clean checkout. Its raw Windows byte hash therefore differs from the Mac LF hash while Git-normalized worktree blob and HEAD blob both equal `bed7306bacdbabb221b9f7c55acbc410fb0e7644`; content remains exact Unity `6000.3.21f1 (c02631ffc030)`. This is recorded explicitly, not mislabeled as source drift.

## Foreground Windows OS-input acceptance

The accepted `os-input-r2` harness ran against the real foreground r51 player window in interactive Session `2`. Report SHA-256 is `adbcda987387b40df37839301b381b1f06cfda99cd4b640b99050d12159715e1`; runtime log SHA-256 is `b3a77dd877fb7500efead0e74fd1df0d37a85364e8f0595950b047229d924c6f`; harness SHA-256 is `c9807d5bb8475664bcbfc756de4d7b331fc0c922d2dfcd76035289e5c726d776`.

The receipt proves:

- foreground handle equals the player handle;
- W/A/S/D scan-code down/up calls each return `1/1`;
- relative mouse-only calls return `18/18`;
- one combined call delivers W + D + mouse as `3/3`;
- while W+D remain held, a further `30/30` relative mouse deltas are accepted;
- W/D release returns `1/1`; player residue is `0`;
- final claim is `HARNESS_RESULT=PASS human=false input=Win32-SendInput keyboard=W+A+S+D mouse=relative simultaneous=true`.

All eight screenshots are nonempty and have unique SHA-256 values. The first `os-input-r1` run is retained as negative harness evidence: the player reached exact foreground r51 readiness, then PowerShell 5 rejected the script-only `[ushort]` alias. It left player/task residue `0`; `[uint16]` fixed the harness in a separately named r2 run. This is not a product failure and is not counted as a PASS.

The screenshots and OS delivery are combined with exact PlayMode same-frame movement/look assertions. They do not claim a real-human session, physical keyboard, physical gamepad or endurance test. Input System gamepad automation is likewise not a physical-gamepad claim.

## Final audit and readback

`windows-final-audit.json` uses schema `pcshop-issue102-windows-final-audit-v2`; all `27` checks pass. It is `6,184` bytes / SHA-256 `35d1d2e1807a209df09b7164093500e9bb53e8415098dbf272b87663151db995`. Its 30-entry evidence manifest is `7,756` bytes / `c18f32e61188c174791a6d60fbdb47ff5000e2fa2f32e880e48d7e702a528f97`.

Windows evidence returned to the Mac as `issue102-windows-evidence-r1.tar`, `6,587,392` bytes / SHA-256 `c15e21ff64ab08328072470120df62b69d8f0c52c364301a079a56dbd90c44a0`. The Mac readback matches the Windows hash. Canonical raw evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue102-740a8869-r1`.

No removable Windows volume or USB disk was present at final audit. No USB write, physical checkpoint or USB acceptance claim is made for Issue #102.

## Issue #102 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical PSU line/product/item/reservation/allocation tuple. | PASS |
| 2 | Historical `10/10` receipts and owned #89/#91/#93/#95/#97/#99 live chain. | PASS |
| 3 | Exact motherboard/CPU/DDR5/M.2/cooler/GPU custody/state/receipt chain. | PASS |
| 4 | Exact managed capacity-one empty/open ATX PS/2 bay and matching rear-mount/four-fastener topology. | PASS |
| 5 | Stable distinct PSU handoff operation bound to exact staging receipt. | PASS |
| 6 | Immediate/delayed replay exactly once with no second mutation. | PASS |
| 7 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | PASS |
| 8 | Stale revisions, full hands, occupied bay and overflow are no-mutation. | PASS |
| 9 | Only registered BuildKit→hands and exact PSU-bay↔hands transfer is accepted. | PASS |
| 10 | Reservation/allocation remains live through the reversible flow. | PASS |
| 11 | Ten receipts/history, installed six components and all three cable records remain untouched. | PASS |
| 12 | Existing Issue #60 PSU and Issue #61/#62/#63 cable authorities remain exact. | PASS |
| 13 | Same Unity instance and stable ItemId survive pickup→detach. | PASS |
| 14 | Physical/input/orientation/support/rear-plane/clearance/obstruction gates fail closed. | PASS |
| 15 | Authority-first projection and same-instance recovery are atomic. | PASS |
| 16 | Generic drop/box/stack/cart/raw-transfer/receipt-free bypasses are blocked. | PASS |
| 17 | `PSU MONTAJDA`, immutable ticket and `10/10` history remain readable. | PASS |
| 18 | Keyboard/mouse and Input System gamepad full handoff flow. | PASS — physical gamepad not claimed |
| 19 | WASD, simultaneous movement+mouse-look, pause/focus and single-consumer regressions. | PASS |
| 20 | Retained/cable-routed removal and predecessor detach/unretain interlocks stay fail closed. | PASS |
| 21 | Retail/economy/customer/electrical readiness/POST/OS/benchmark remain untouched. | PASS |
| 22 | Targeted and full EditMode/PlayMode have zero fail/skip/inconclusive. | PASS |
| 23 | Diff, Repository Guard and universal Mac native gates. | PASS — Guard 33027397901 |
| 24 | Exact-head clean Windows IL2CPP/only-D3D11 runtime, foreground OS input and zero residue. | PASS |
| 25 | Binary/procedure/evidence hashes and cross-machine archive readback. | PASS |
| 26 | Bible/ADR/Evidence/CHANGELOG and private PR/CI chain. | PARTIAL — docs and source/docs Guard PASS; PR #103 merge/main integration pending |
| 27 | Claim explicitly preserves physical human/HID/gamepad/endurance certification for Steam 1.0. | PASS |

The bounded product/platform acceptance count is `26/27` at this document revision. Only PR #103 merge/main integration remains. Physical human/HID/gamepad and endurance are deliberately outside Issue #102 closure and remain mandatory before Steam 1.0 release certification.

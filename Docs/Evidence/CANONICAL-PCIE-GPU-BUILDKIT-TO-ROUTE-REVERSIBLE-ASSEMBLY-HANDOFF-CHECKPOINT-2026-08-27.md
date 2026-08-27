# Canonical PCIe/GPU 6+2 BuildKit-to-Route Reversible Assembly Handoff — Checkpoint Evidence

**Date:** 27 August 2026<br>
**Issue:** [#109](https://github.com/cixanla/PC-Shop-Empire-3D/issues/109)<br>
**PR:** [#110](https://github.com/cixanla/PC-Shop-Empire-3D/pull/110)<br>
**Technical head:** `1acba166855efffa906112e2df24b9b5cef550a7`<br>
**Technical tree:** `eb40a392169e5288e29bc59ae75367029cc00f57`<br>
**Branch:** `codex/issue109-pcie-gpu-buildkit-route-handoff`<br>
**Current state:** bounded technical acceptance `27/27`; source/domain/scene/input/full regression, universal macOS native, clean Windows full tests/IL2CPP/D3D11/runtime/foreground input, cross-machine readback and technical Repository Guard passed; PR #110 is the integration record and GitHub remains authoritative for the subsequent administrative Issue/Roadmap state

## Delivered playable result

GarageGraybox r54 connects the canonical reserved PCIe/GPU 6+2 power cable in the completed `10/10` custom-PC BuildKit to the existing Assembly-owned PCIe/GPU endpoint/waypoint route. Handoff starts only after the exact motherboard is secured, CPU retained, DDR5 retained in A2, M.2 secured in the primary slot, processor cooler retained, graphics card retained in PCIe x16, ATX PS/2 power supply four-fastener retained and both exact canonical ATX24 and EPS12V cables are routed. The domain resolves only the accepted work-order/ticket/allocation line, exact `ModularPcie8PinPsuToGraphicsCard` family, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity cable instance with `E / Gamepad South`, carries it from exact PCIe/GPU BuildKit custody into exact hands, enters the existing guided Issue #63 route with `Mouse Left / Gamepad RT`, rotates between only canonical keyed orientations with `R / Right Shoulder`, commits with `G / Gamepad East`, keeps the cable visibly routed, proves generic drop and dependent PSU/GPU removal are blocked, then focuses the routed cable with empty hands and unroutes the exact same instance back to `ActorHands`. Keyboard/mouse and Input System virtual-gamepad automation complete the reversible cycle. Authority-first projection and recovery preserve the same instance after physical failure.

This is a custody bridge, not a second Inventory, BuildKit, cable, connector, endpoint, waypoint or route authority. Issue #63 remains the only PCIe/GPU route/unroute truth. The authored route-collider allowlist is narrow; foreign geometry remains a blocker. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal or route the item.

Reservation/allocation remains live; the original ten staging receipts and visible `10/10` history remain immutable; installed motherboard/CPU/DDR5/M.2/cooler/GPU/PSU do not move. ATX24 and EPS12V remain exact-routed. Direct physical-cable focus wins over a fixed ATX24/EPS12V/PCIe priority, preventing another routed cable from stealing the unroute action while retaining a single input consumer.

Routing PCIe/GPU power completes the current three-cable physical route chain but does not manufacture power-on readiness. The bounded assembly remains `BuildIncomplete`; wattage/headroom, electricity, short-circuit, POST, BIOS/UEFI, OS, drivers and benchmark are subsequent systems.

The exact runtime success marker is:

```text
GARAGE_PCIE_GPU_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 assembly-chain=7/7 atx24+eps12v=routed pickup=exact custody=build-kit-to-hands-to-route-to-hands reservation=alive physical-identity=stable input=keyboard+mouse generic-drop=blocked route=ok psu-unretain=blocked unroute=ok history=10/10-preserved cables=2/2-protected replay=immediate+delayed receipts=ok revisions=ok electrical-readiness=blocked no-duplicate-loss=ok invariants=ok
```

The marker is automation evidence, not a real-human or physical-device claim.

## Exact source and tests

| Platform | Gate | Result | Duration | Bytes | SHA-256 |
|---|---|---:|---:|---:|---|
| macOS | Targeted BuildKit/domain EditMode | `87/87` | `28.7031532 s` | `71,738` | `b06266dc7557117b80cf8292c3f21f974e4b37993d355486695e97ff350f8c2c` |
| macOS | Scene contract EditMode | `9/9` | `1.1906804 s` | `10,356` | `11dae86e06ab149022b09a0f7cb8a593abd19785b2f63d712bc2d48a891fe7c0` |
| macOS | Targeted P1 PlayMode | `4/4` | `39.8878984 s` | `15,787` | `f114556b7cccbef773f9185a394074adf33a647c6d902e60a20e3611193410a9` |
| macOS | Full EditMode | `752/752` | `48.2324686 s` | `624,263` | `94b80eaff72b4ad0d9112d1cdccc8053a69b1962b0a9dd3eabaab8eb1d3c5fef` |
| macOS | Full PlayMode | `156/156` | `768.1683333 s` | `507,940` | `b2a70719536738dba58f5d9534ea31c6b403da30a1d305ab6ea80cfdb5a4a451` |
| Windows | Full EditMode | `752/752` | `24.6630064 s` | `629,091` | `09d585ec7d8cd596651e5db6a8330b744db69a46687543fadfe21d49d7b97306` |
| Windows | Full PlayMode | `156/156` | `511.3468741 s` | `509,361` | `5e263ab5cbb238ad4c36c689208cb3c7d1ad316b7d254c10a2ed43279852cc34` |

Every accepted XML has failed, skipped and inconclusive `0`. The targeted P1 PlayMode set covers exact keyboard/mouse pickup→route→unroute, Input System virtual-gamepad cycle, foreign obstruction fail-closed followed by clear recovery, and projection failure with same-instance recovery. Full suites include exact replay, forgery, stale revision, overflow, route solver, scene/runtime, focused-cable arbitration and simultaneous movement/look regressions.

`git diff --check` and local Repository Guard pass. Exact-head technical Repository Guard [33054757532](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33054757532) passed at `1acba166855efffa906112e2df24b9b5cef550a7`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build log | Success | `mac-development-build-r1.log` | `603,517` | `0d282411fc08a7da03241ca711f142a371b4b841ca2a26de9c1fa2e03857fd97` |
| Apple M1/Metal runtime | Success | `mac-r54-runtime-smoke-r1.log` | `8,901` | `5987f35d45925465a508f90cab59c27557adfedbb446022e970c9d207b78693d` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | `117,179` | `80556318de7d2aa5e1f1f0abc8315cc0a0453c67a7a804fe0e9c4df467879dd0` |

The build report is `330,366,591` bytes and the application contains `302` files. `file` confirms Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The final player emits one exact r54 readiness marker including `pcie-gpu-power-cable-assembly-handoff=ready`, one exact success marker, zero handoff failure/exception markers, reaches Input System `Shutdown`, exits `0` and leaves no player process.

Canonical raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue109-working-r1`.

## Windows clean IL2CPP and D3D11 gate

Windows canonical checkout `C:\Users\mertk\Developer\PCShopEmpire3D\Game` was first proven exact at the technical head/tree with empty status. A collision-free local validation clone was then created at `C:\Users\mertk\AppData\Local\Temp\PCShopIssue109-1acba16-r1\Game`; source and clone remained exact and clean throughout. No GitHub credential or second Windows source-write lane was required.

Unity 6000.3.21f1 built an x64 IL2CPP Development player with Direct3D11 only. The build report is `1,349,053,878` bytes; output contains `666` files and `1,349,222,872` bytes. `windows-build-il2cpp-d3d11.log` is `565,977` bytes with SHA-256 `77051c76bf8710a42f164843396027a08d25cc322df19a9c13c0c02fcfc28b55`; expanded compiler/AOT/native-link fatal count is `0`; exact success marker count is `1`; `ProjectSettings.asset` returns byte-exact to SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` and the clone remains clean.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | `667,136` | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | `45,821,952` | `f327f7036c483fa6edcfcfcc1a6cfd261bd6472e23d337a2f59c01e8fd7522a7` |
| `UnityPlayer.dll` | `84,237,744` | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted runtime task ran as logged-on `cixanla\mertk`, `LogonType=Interactive`, `RunLevel=Limited`, using `-force-d3d11 -pse-require-d3d11 -pse-pcie-gpu-power-cable-assembly-handoff-smoke -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. Intel Iris Xe reports Direct3D 11.0 feature level 11.1. Host/readiness/success counts are `1/1/1`; forbidden count is `0`; player and task results are `0`; graceful Input System shutdown occurs; the task is deleted; player residue is `0`.

`runtime-issue109-r54-interactive.log` is `6,058` bytes / SHA-256 `97d56230a3b3684b05a1bf21541e1fc19eb1d53b9ee89648def4d1e0814a4dc9`; its receipt is `869` bytes / `de3d7235367443d94acc1ee1223ed1b3925f02bd66c70f95f69908c19284405a`.

The build wrapper's first three-second post-Unity sample observed one exact scoped child process. It failed closed instead of declaring immediate cleanup. The child exited naturally before exact final readback; no forced termination was required, and accepted final process residue is `0`.

## Windows foreground OS-input gate

The accepted harness runs in interactive Session 2, verifies the exact player SHA before launch, requires exact r54 readiness and rechecks the exact player foreground window around every input stage. It uses `-force-d3d11` and verifies Unity's Direct3D 11.0 / Intel Iris Xe engine lines directly. It records:

- W/A/S/D scan-code down/up as `1/1` for each direction;
- relative mouse-only calls as `18/18`;
- one combined call delivering W + D + mouse as `3/3`;
- a further `30/30` relative mouse deltas while W+D remain held;
- W/D release as `1/1`, player residue `0` and scheduled-task residue `0`;
- final claim `HARNESS_RESULT=PASS human=false input=Win32-SendInput keyboard=W+A+S+D mouse=relative simultaneous=true`.

The accepted report is `1,796` bytes / SHA-256 `d5c5211e63f1a7c7a553a9d678c1d27a5c8c0f882f08ec0568ae3c93a6ab209b`; its runtime log is `4,174` bytes / `b5671ca89c75054e0a9808a6bf7fff355a6be870af416616fac1823991f36427`. Runtime forbidden count is `0`. All eight screenshots are nonempty and have unique SHA-256 values; their exact hashes are retained in `windows-final-audit.json`.

The screenshots and OS delivery are combined with exact PlayMode same-frame movement/look assertions. They do not claim a real-human session, physical keyboard, physical gamepad or endurance test. Input System virtual-gamepad automation is likewise not a physical-gamepad claim.

## Final audit, readback and cleanup

`windows-final-audit.json` uses schema `pcshop-issue109-windows-final-audit-v1`; all `28` checks pass. It is `8,228` bytes / SHA-256 `123b7fdfaf636f76dccf82c5564a749d38016f75e565c0f5fa73fed8dd013e32`. Its 33-entry evidence/native-binary manifest is `8,537` bytes / `dc4d0e6cfbdf542d738831f44b3bdab76a15dc779e57c90da62c2065a11cd2bf`.

The first final audit correctly failed only because Unity had created two exact disposable-player TCP/UDP Query User firewall rules. The failed pre-cleanup audit and manifest remain preserved under `evidence/diagnostics`. Guarded cleanup selected only rules whose live `Program` matched the exact Issue #109 temporary executable, removed those two exact names and read back the executable-path rule count as `0`. The final audit then passed with process/task/firewall residue `0/0/0`.

Windows evidence returned to the Mac as `issue109-windows-evidence.tar.gz`, `4,599,837` bytes / SHA-256 `924792e2c4dd239e8b5209b9f8eaed8b8d248a9ca93cfe597d39450785db74e4`. Mac readback matches the Windows hash. Every `30/30` evidence file listed by manifest matches bytes and SHA-256; the three native-binary records match; the two self-referential extras, `windows-file-manifest.json` and `windows-final-audit.json`, independently match their Windows bytes and hashes. Readback report schema `pcshop-issue109-mac-windows-evidence-readback-v1` is `pass=true` with mismatches `[]`.

Canonical raw Windows evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue109-1acba16-r1/windows`.

After Mac readback passed, the exact temporary Windows validation root and launch scripts were removed. The first PowerShell cleanup encountered a Unity PackageCache long-path condition after removing most data; the same previously verified exact root was then removed with Windows `\\?\` long-path semantics. Final readback reports root absent, process/task/firewall residue `0/0/0`, approximately `10.29 GB` free RAM and `199.62 GB` free on C:.

During the accepted Windows run no removable volume or USB disk was present, so no USB acceptance or checkpoint is claimed. After acceptance, the user's USB was moved from the safely ejected Mac to Windows and read-only identity/health discovery found `D:`, label `cixanla`, exFAT, Intenso Alu Line, serial `900B00076010`, `Healthy/OK`, size `125,820,993,536` bytes. Issue #109 did not write to it.

## Issue #109 acceptance matrix — technical state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical PCIe/GPU line/kind/family/product/item/reservation/allocation tuple. | PASS |
| 2 | Historical `10/10` receipts, owned #89/#91/#93/#95/#97/#99/#102 live chain and routed #105/#107 cables. | PASS |
| 3 | Exact motherboard/CPU/DDR5/M.2/cooler/GPU/PSU/ATX24/EPS12V custody/state/receipt chain. | PASS |
| 4 | Exact capacity-one route container, canonical cable, typed 8-pin and 6+2 endpoints, three waypoints and `Loose` state. | PASS |
| 5 | Stable PCIe/GPU handoff operation bound to exact staging receipt. | PASS |
| 6 | Immediate/delayed replay exactly once with no second mutation. | PASS |
| 7 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | PASS |
| 8 | Stale revisions, full hands, occupied route and overflow are no-mutation. | PASS |
| 9 | Only registered BuildKit→hands and exact hands↔PCIe/GPU route transfer is accepted. | PASS |
| 10 | Reservation/allocation remains live through pickup, route and unroute. | PASS |
| 11 | Ten receipts/history, installed seven components and routed ATX24/EPS12V remain protected. | PASS |
| 12 | Existing Issue #63 route and #61/#62 cable authorities remain exact. | PASS |
| 13 | Same Unity instance and stable ItemId survive BuildKit→Hands→Routed→Hands. | PASS |
| 14 | Range/focus/LOS/pause/orientation/host/topology/clearance/obstruction/preview gates fail closed. | PASS |
| 15 | Authority-first projection and same-instance recovery are atomic. | PASS |
| 16 | Generic drop/box/stack/cart/raw-transfer/receipt-free bypasses are blocked. | PASS |
| 17 | BuildKit assembly state, immutable ticket and `10/10` history remain readable. | PASS |
| 18 | Keyboard/mouse and Input System virtual-gamepad pickup→route→unroute flow. | PASS — physical gamepad not claimed |
| 19 | WASD, simultaneous movement+mouse-look, pause/focus and single-consumer regressions. | PASS |
| 20 | Routed ATX24+EPS12V prerequisites and PCIe-dependent PSU/GPU detach blocks. | PASS |
| 21 | Electrical/power-on readiness remains outside this bounded handoff. | PASS |
| 22 | Retail/economy/customer/price, Save/Guardian and unrelated systems remain untouched. | PASS |
| 23 | Targeted and full EditMode/PlayMode have zero fail/skip/inconclusive. | PASS |
| 24 | Diff, Repository Guard and universal Mac native gates. | PASS — Guard 33054757532 |
| 25 | Exact-head clean Windows IL2CPP/only-D3D11 runtime, foreground OS input and zero residue. | PASS |
| 26 | Bible/ADR/Evidence/CHANGELOG and private PR/CI chain. | TECHNICAL PASS — PR #110 is the integration record |
| 27 | Claim explicitly preserves physical human/HID/gamepad/endurance certification for Steam 1.0. | PASS |

The bounded technical acceptance count is `27/27`. Administrative Issue/Roadmap closure is intentionally separate and occurs only after PR #110 source/docs integration and final required Guard checks. Parent Epic #10 and the full Steam 1.0 Goal remain open for electrical, product, world, visual and release work. Physical human/HID/gamepad and endurance remain mandatory before Steam 1.0 release certification.

# Canonical EPS12V BuildKit-to-Route Reversible Assembly Handoff — Checkpoint Evidence

**Date:** 27 August 2026<br>
**Issue:** [#107](https://github.com/cixanla/PC-Shop-Empire-3D/issues/107)<br>
**PR:** [#108](https://github.com/cixanla/PC-Shop-Empire-3D/pull/108)<br>
**Technical head:** `9cd3276d60c03cec1b5b15049027523dddbee8b6`<br>
**Technical tree:** `01f3edc99dd94aeeb125323048bf8532891c028a`<br>
**Branch:** `codex/issue107-eps12v-buildkit-route-handoff`<br>
**Current state:** bounded technical acceptance `27/27`; source/domain/scene/input/full regression, universal macOS native, detached-clean Windows full tests/IL2CPP/D3D11/runtime/foreground input, cross-machine readback and technical Repository Guard passed; PR #108 integration is the remaining administrative chain at this document snapshot

## Delivered playable result

GarageGraybox r53 connects the canonical reserved EPS12V CPU power cable in the completed `10/10` custom-PC BuildKit to the existing Assembly-owned EPS12V endpoint/waypoint route. Handoff starts only after the exact motherboard is secured, CPU retained, DDR5 retained in A2, M.2 secured in the primary slot, processor cooler retained, graphics card retained in PCIe x16, ATX PS/2 power supply four-fastener retained and the exact canonical ATX24 cable routed. The domain resolves only the accepted work-order/ticket/allocation line, exact `ModularEps12v8PinPsuToMotherboard` family, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity cable instance with `E / Gamepad South`, carries it from exact EPS12V BuildKit custody into exact hands, enters the existing guided Issue #62 route with `Mouse Left / Gamepad RT`, rotates between only the canonical keyed orientations with `R / Right Shoulder`, commits with `G / Gamepad East`, keeps the cable visibly routed, proves generic drop and dependent PSU/motherboard/CPU removal are blocked, then focuses the routed cable with empty hands and unroutes the exact same instance back to `ActorHands`. Keyboard/mouse and Input System gamepad automation complete the reversible cycle. Authority-first projection and recovery preserve the same instance after physical failure.

This is a custody bridge, not a second Inventory, BuildKit, cable, connector, endpoint, waypoint or route authority. Issue #62 remains the only EPS12V route/unroute truth. Exact chassis-right-rail, installed GPU and PCIe power connector colliders are the only narrowly authored route-obstruction exclusions; foreign objects remain blockers. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal or route the item.

Reservation/allocation remains live; the original ten staging receipts and visible `10/10` history remain immutable; installed motherboard/CPU/DDR5/M.2/cooler/GPU/PSU do not move. ATX24 remains exact-routed and PCIe/GPU remains staged/untouched. Routing EPS12V does not manufacture power-on readiness: the assembly remains `BuildIncomplete` until PCIe/GPU power is routed; after EPS12V unroute the exact failure becomes `PowerCableMissing`.

The exact runtime success marker is:

```text
GARAGE_EPS12V_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 assembly-chain=7/7 atx24=routed pickup=exact custody=build-kit-to-hands-to-route-to-hands reservation=alive physical-identity=stable input=keyboard+mouse generic-drop=blocked route=ok psu-unretain=blocked unroute=ok history=10/10-preserved cables=2/2-protected replay=immediate+delayed receipts=ok revisions=ok electrical-readiness=blocked no-duplicate-loss=ok invariants=ok
```

The marker is automation evidence, not a real-human or physical-device claim.

## Exact source and tests

| Platform | Gate | Result | Duration | Bytes | SHA-256 |
|---|---|---:|---:|---:|---|
| macOS | Targeted BuildKit/domain EditMode | `83/83` | `51.302192 s` | `68,608` | `3d37e25df0ac12f59be0581b90462ef0e85ca165bc3d73aaf3d76de2b954c635` |
| macOS | Scene contract EditMode | `9/9` | `1.9464486 s` | `10,353` | `07b1859327d88f92240b686041120f89edbbe2d54f557044413b1eece871651d` |
| macOS | Targeted P1 PlayMode | `4/4` | `86.5014904 s` | `15,607` | `d3e3dd0de3a90b79b97f328aa27c518bdd77896e09b49b96d59846b826729270` |
| macOS | Full EditMode | `748/748` | `48.924889 s` | `621,140` | `f8857573b92a7df25eb0b055227ecaaaee4e2b8eb889efa0a12611f841a9baf2` |
| macOS | Full PlayMode | `152/152` | `895.1181722 s` | `488,120` | `4cb88cea1c6dbc436c120aeb8277f905ebeaa8a596aac12d4bad457ca53e56ed` |
| Windows | Full EditMode | `748/748` | `21.376105 s` | `625,950` | `faf35ac2b7ba10e91e585388638ed8b463ff02b0c23ee4cd33c47542155ef30e` |
| Windows | Full PlayMode | `152/152` | `485.6175228 s` | `489,793` | `aeefc654312e412374c4a69cc7c6fc0e898f6d2a43bc34f0ad193f984016f7a7` |

Every accepted XML has failed, skipped and inconclusive `0`. The targeted P1 PlayMode set covers exact keyboard/mouse pickup→route→unroute, Input System virtual-gamepad cycle, foreign obstruction fail-closed followed by clear recovery, and projection failure with same-instance recovery. Full suites include exact replay, forgery, stale revision, overflow, route solver, scene/runtime and simultaneous movement/look regressions.

`git diff --check` and local Repository Guard pass. Exact-head technical Repository Guard [33044086315](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33044086315) passed at `9cd3276d60c03cec1b5b15049027523dddbee8b6`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build log | Success | `mac-development-build-r1.log` | `604,908` | `fc56e4052d728cf31b3d481409cae93d9c78afb6b7090bbd149a26e077d585aa` |
| Apple M1/Metal runtime | Success | `mac-r53-runtime-smoke-r1.log` | `9,084` | `b8994182ee92e0391adb80f5d0a09a94ac8f268d8218968b8c9fa738ccbe5376` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | `117,179` | `24c0fdadc339653722b8a5c44ba66857b3237a03d4f0c3585402267bef492582` |

The build report is `330,340,220` bytes and the application contains `302` files. `file` confirms Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The final player emits one exact r53 readiness marker including `eps12v-power-cable-assembly-handoff=ready`, one exact success marker, zero handoff failure/exception markers, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,708,889` bytes with SHA-256 `ffd2d43a3c0182c8c0e21565b21fb85b16fc72416dd2623a4a237c1875bebe55`. Windows readback matched that hash before restoring a collision-free clean clone at `C:\Users\mertk\AppData\Local\Temp\PCShopIssue107-9cd3276-r1\Game`. Final head/tree remain exact technical source and `git status --porcelain` is empty. A first noninteractive private-HTTPS clone attempt failed at Git Credential Manager before Unity launched; its exact partial temporary root was inspected and removed, and it contributes no product acceptance evidence.

Unity 6000.3.21f1 built an x64 IL2CPP Development player with Direct3D11 only. The build report is `1,348,030,823` bytes; output contains `666` files and `1,348,199,817` bytes. `windows-build-il2cpp-d3d11.log` is `565,974` bytes with SHA-256 `cd0b07e6c6c8283b573462d16fbd601bfa2afb0fc99215977e3f75af5b57ed13`; expanded compiler/AOT/native-link fatal count is `0`; exact success marker count is `1`; `ProjectSettings.asset` returns byte-exact to SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` and the clone remains clean.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | `667,136` | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | `45,786,624` | `f198055833b1b0204ac676c2f44545603af70003d01b7d22509ac37f6fc369ed` |
| `UnityPlayer.dll` | `84,237,744` | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted runtime task ran as logged-on `cixanla\mertk`, `LogonType=Interactive`, `RunLevel=Limited`, using `-force-d3d11 -pse-require-d3d11 -pse-eps12v-power-cable-assembly-handoff-smoke -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. Intel Iris Xe reports Direct3D 11.0 feature level 11.1. Host/readiness/success counts are `1/1/1`; forbidden count is `0`; player and task results are `0`; graceful Input System shutdown occurs; the task is deleted; player residue is `0`.

`runtime-issue107-r53-interactive.log` is `5,999` bytes / SHA-256 `7bca6e76daf599c6219f2d4d62a4e738b5ad7b9029f458574a624e9ebbc5e9fa`; its receipt is `867` bytes / `60f2208ebba1fe1d509e50760f6198b498d7de9e1d218f137a83eb3bd31f9708`.

The build wrapper's first three-second post-Unity sample observed one exact scoped child process. It failed closed instead of declaring immediate cleanup. The child exited naturally before exact final readback; no forced termination was required, and the accepted final process residue is `0`.

## Windows foreground OS-input gate

The accepted harness runs in interactive Session 2, verifies the exact player SHA before launch, requires exact r53 readiness and rechecks the exact player foreground window around every input stage. It uses `-force-d3d11` and verifies Unity's Direct3D 11.0 / Intel Iris Xe engine lines directly. It records:

- W/A/S/D scan-code down/up as `1/1` for each direction;
- relative mouse-only calls as `18/18`;
- one combined call delivering W + D + mouse as `3/3`;
- a further `30/30` relative mouse deltas while W+D remain held;
- W/D release as `1/1`, player residue `0` and scheduled-task residue `0`;
- final claim `HARNESS_RESULT=PASS human=false input=Win32-SendInput keyboard=W+A+S+D mouse=relative simultaneous=true`.

The accepted report is `1,787` bytes / SHA-256 `3db7a61befc6f270ae7e1f31cc6775c27e679f0706aa102dc7bc8d39a5dc90ab`; its runtime log is `4,121` bytes / `a34ed7bda467064d4d514e8524e87b32e2020ffb2f6d014405d0ea55c486faf4`. Runtime forbidden count is `0`. All eight screenshots are nonempty and have unique SHA-256 values; their exact hashes are retained in `windows-final-audit.json`.

The screenshots and OS delivery are combined with exact PlayMode same-frame movement/look assertions. They do not claim a real-human session, physical keyboard, physical gamepad or endurance test. Input System gamepad automation is likewise not a physical-gamepad claim.

## Final audit and readback

`windows-final-audit.json` uses schema `pcshop-issue107-windows-final-audit-v1`; all `28` checks pass. It is `8,039` bytes / SHA-256 `e53f8f5ce1c6151d1bd998be3a96ec2f2d927e082a8c1a60d85fa097ebd5860d`. Its 35-entry evidence/native-binary manifest is `9,074` bytes / `45d4cd6087e5aea87b40e49153a7a0b0a0211f58320c11cafcd6fae9353f2070`.

The first final audit correctly failed only because Unity had created two exact disposable-player TCP/UDP Query User firewall rules. An initial cleanup command had an over-escaped expected path and therefore made no mutation. The subsequent guarded cleanup first proved each exact rule's live `Program` equals the disposable Issue #107 executable path, removed only those two rule names and read back both rule-name and executable-path counts as `0`. The final audit then passed with process/task/firewall residue `0`. Both pre-cleanup audit generations remain preserved as negative evidence.

Windows evidence returned to the Mac as `issue107-windows-evidence.tar.gz`, `3,146,658` bytes / SHA-256 `239614d2c0c4e1a0fc652aa1db106c71df0563161a810a482cffb2e479a53525`. The Mac readback matches the Windows hash. Every `32/32` transported file listed by manifest matches bytes and SHA-256; the two self-referential extras, `windows-file-manifest.json` and `windows-final-audit.json`, independently match their Windows bytes and hashes. Canonical raw evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue107-9cd3276-r1`.

Final Windows health reports approximately `10 GB` free RAM, one healthy/OK Samsung NVMe physical disk, no scoped Unity/player/compiler/PowerShell process, no scheduled task, no validation firewall rule, no removable volume and no USB disk. No USB write, physical checkpoint or USB acceptance claim is made for Issue #107.

## Issue #107 acceptance matrix — technical state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical EPS12V line/kind/family/product/item/reservation/allocation tuple. | PASS |
| 2 | Historical `10/10` receipts, owned #89/#91/#93/#95/#97/#99/#102 live chain and routed #105 ATX24. | PASS |
| 3 | Exact motherboard/CPU/DDR5/M.2/cooler/GPU/PSU/ATX24 custody/state/receipt chain. | PASS |
| 4 | Exact capacity-one route container, canonical cable, two endpoints, three waypoints and `Loose` state. | PASS |
| 5 | Stable distinct EPS12V handoff operation bound to exact staging receipt. | PASS |
| 6 | Immediate/delayed replay exactly once with no second mutation. | PASS |
| 7 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | PASS |
| 8 | Stale revisions, full hands, occupied route and overflow are no-mutation. | PASS |
| 9 | Only registered BuildKit→hands and exact hands↔EPS12V-route transfer is accepted. | PASS |
| 10 | Reservation/allocation remains live through pickup, route and unroute. | PASS |
| 11 | Ten receipts/history, installed seven components, routed ATX24 and staged PCIe remain protected. | PASS |
| 12 | Existing Issue #62 route and #61/#105/#63 cable authorities remain exact. | PASS |
| 13 | Same Unity instance and stable ItemId survive BuildKit→Hands→Routed→Hands. | PASS |
| 14 | Range/focus/LOS/pause/orientation/host/topology/clearance/obstruction/preview gates fail closed. | PASS |
| 15 | Authority-first projection and same-instance recovery are atomic. | PASS |
| 16 | Generic drop/box/stack/cart/raw-transfer/receipt-free bypasses are blocked. | PASS |
| 17 | `EPS12V MONTAJDA`, immutable ticket and `10/10` history remain readable. | PASS |
| 18 | Keyboard/mouse and Input System gamepad pickup→route→unroute flow. | PASS — physical gamepad not claimed |
| 19 | WASD, simultaneous movement+mouse-look, pause/focus and single-consumer regressions. | PASS |
| 20 | Routed-ATX24 prerequisite and EPS12V-dependent PSU/motherboard/CPU detach blocks. | PASS |
| 21 | Electrical readiness remains blocked until PCIe/GPU power route exists. | PASS |
| 22 | Retail/economy/customer/price, Save/Guardian and unrelated systems remain untouched. | PASS |
| 23 | Targeted and full EditMode/PlayMode have zero fail/skip/inconclusive. | PASS |
| 24 | Diff, Repository Guard and universal Mac native gates. | PASS — Guard 33044086315 |
| 25 | Exact-head clean Windows IL2CPP/only-D3D11 runtime, foreground OS input and zero residue. | PASS |
| 26 | Bible/ADR/Evidence/CHANGELOG and private PR/CI chain. | TECHNICAL PASS — PR #108 integration follows this docs commit |
| 27 | Claim explicitly preserves physical human/HID/gamepad/endurance certification for Steam 1.0. | PASS |

The bounded technical acceptance count is `27/27`. Administrative Issue/Roadmap closure is intentionally separate and occurs only after PR #108 source/docs integration and final required Guard checks. Parent Epic #10 and the full Steam 1.0 Goal remain open for PCIe/GPU, electrical, product and release work. Physical human/HID/gamepad and endurance remain mandatory before Steam 1.0 release certification.

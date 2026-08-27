# Canonical ATX24 BuildKit-to-Route Reversible Assembly Handoff — Checkpoint Evidence

**Date:** 27 August 2026<br>
**Issue:** [#105](https://github.com/cixanla/PC-Shop-Empire-3D/issues/105)<br>
**PR:** [#106](https://github.com/cixanla/PC-Shop-Empire-3D/pull/106)<br>
**Technical head:** `5d6a39892cf3c585abd1046cc799a93418329cd0`<br>
**Technical tree:** `263307821aeba8df6648a39756bec431e548938f`<br>
**Branch:** `codex/issue105-atx24-buildkit-route-handoff`<br>
**Current state:** bounded technical acceptance `27/27`; source/domain/scene/input/full regression, universal macOS native, detached-clean Windows full tests/IL2CPP/D3D11/runtime/foreground input, cross-machine readback and technical Repository Guard passed; PR #106 integration is the remaining administrative chain at this document snapshot

## Delivered playable result

GarageGraybox r52 connects the canonical reserved ATX24 split cable in the completed `10/10` custom-PC BuildKit to the existing Assembly-owned ATX24 endpoint/waypoint route. Handoff starts only after the exact motherboard is secured, CPU retained, DDR5 retained in A2, M.2 secured in the primary slot, processor cooler retained, graphics card retained in PCIe x16 and ATX PS/2 power supply four-fastener retained. The domain resolves only the accepted work-order/ticket/allocation line, exact `ModularAtx24SplitPsuToMotherboard` family, product, serialized item, reservation and original staging-receipt tuple.

The player takes the same Unity cable instance with `E / Gamepad South`, carries it from exact ATX24 BuildKit custody into exact hands, enters the existing guided Issue #61 route with `Mouse Left / Gamepad RT`, rotates between only the canonical keyed orientations with `R / Right Shoulder`, commits with `G / Gamepad East`, keeps the cable visibly routed, proves generic drop and PSU unretain/remove are blocked, then focuses the routed cable with empty hands and unroutes the exact same instance back to `ActorHands`. Keyboard/mouse and Input System gamepad automation complete the reversible cycle. Domain-first projection and recovery preserve the same instance after physical failure.

This is a custody bridge, not a second Inventory, BuildKit, cable, connector, endpoint, waypoint or route authority. Issue #61 remains the only ATX24 route/unroute truth. Exact installed cooler, GPU and chassis-right-rail roots are the only authored route-obstruction exclusions; foreign objects remain blockers. Generic reserved transfer, world drop, box, stack, cart, raw Inventory move and receipt-free Assembly bypasses cannot steal or route the item.

Reservation/allocation remains live; the original ten staging receipts and visible `10/10` history remain immutable; installed motherboard/CPU/DDR5/M.2/cooler/GPU/PSU do not move. EPS12V and PCIe/GPU item/container/state/revision/receipt/operation lineages remain exact and untouched. Routing ATX24 does not manufacture power-on readiness: the assembly remains `BuildIncomplete` until the other two power cables are routed; after ATX24 unroute the exact failure becomes `PowerCableMissing`.

The exact runtime success marker is:

```text
GARAGE_ATX24_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE work-ticket=ok prerequisites=10/10 assembly-chain=7/7 pickup=exact custody=build-kit-to-hands-to-route-to-hands reservation=alive physical-identity=stable input=keyboard+mouse generic-drop=blocked route=ok psu-unretain=blocked unroute=ok history=10/10-preserved cables=2/2-untouched replay=immediate+delayed receipts=ok revisions=ok electrical-readiness=blocked no-duplicate-loss=ok invariants=ok
```

The marker is automation evidence, not a real-human or physical-device claim.

## Exact source and tests

| Platform | Gate | Result | Duration | Bytes | SHA-256 |
|---|---|---:|---:|---:|---|
| macOS | Targeted BuildKit/domain EditMode | `79/79` | `43.2190022 s` | `65,506` | `480ea5a2e5d4475b07bbb67b8a220c0fbc3496561dcbb5ed21536e3e5210b16d` |
| macOS | Targeted P1 PlayMode | `4/4` | `66.7478383 s` | `15,592` | `842807ad06ff74ff35a6d15bc97965dcc5117d94af7a60144a6ddfef78bec480` |
| macOS | Full EditMode | `744/744` | `57.4242178 s` | `618,041` | `2e54e4aae6732982f0546a622115ed7e2a607041bf6c377effedf291cadd01c3` |
| macOS | Full PlayMode | `148/148` | `631.3997053 s` | `469,095` | `6e6716c55ca7d213959a2144074ab6462ff10e451b6222eacb0be9be1d1773da` |
| Windows | Full EditMode | `744/744` | `19.0438839 s` | `622,794` | `d052599b39538f98e49ccc5346258169d78f6df0ed09b5fee7b446c4ab0f4782` |
| Windows | Full PlayMode | `148/148` | `445.9538542 s` | `470,490` | `0756afb7f494df1585494ff06f33b11d6556d724fd62b27200b8034468a5a2e4` |

Every accepted XML has failed, skipped and inconclusive `0`. The targeted PlayMode set covers exact keyboard/mouse and Input System gamepad pickup→route→unroute, obstruction/recovery, same-instance custody and electrical-readiness boundaries. Full suites include exact replay, forgery, stale revision, overflow, route solver and scene/runtime regressions.

The first three native Mac attempts are retained as diagnostics. They exposed two incorrect smoke expectations rather than product-authority failures: ATX24 uses its independent cable revision/receipt counters, and the exact post-unroute readiness result is `PowerCableMissing`. The final r4 smoke asserts those independent counters and explicit diagnostic gates. Only r4 is accepted.

`git diff --check` and local Repository Guard pass. Exact-head technical Repository Guard [33038180913](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33038180913) passed at `5d6a39892cf3c585abd1046cc799a93418329cd0`.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build log | Success | `pse-issue105-macos-build-r4.log` | `602,919` | `134e320f452631cff4911e45d04760617147cbc2cce1a4a0fa3e113c8ce2fed9` |
| Apple M1/Metal runtime | Success | `pse-issue105-macos-runtime-r4.log` | `9,048` | `c0b2a4388e778fb663a606b1f38c09ae217def806ff89b0f0957febfa53aca07` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | `117,179` | `7fc9fcfea4379660763fe6b269c974fc32043d07dde026e627b55760ffe4378a` |

The build report is `330,311,979` bytes and the application contains `302` files. `file` confirms Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The final player emits one exact r52 readiness marker including `atx24-power-cable-assembly-handoff=ready`, one exact success marker, zero handoff failure/exception markers, reaches Input System `Shutdown`, exits `0` and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,678,445` bytes with SHA-256 `a9c331a43ed7da50376df2a9ac0906a7396faa008619d050227a4388dbb28503`. Windows readback matched that hash before restoring a collision-free detached-clean clone at `C:\Users\mertk\AppData\Local\Temp\PCShopIssue105-5d6a3989-r1\Game`. Final head/tree remain exact technical source and `git status --porcelain` is empty.

Unity 6000.3.21f1 built an x64 IL2CPP Development player with Direct3D11 only. The build report is `1,347,195,309` bytes. `windows-build-il2cpp-d3d11.log` is `566,024` bytes with SHA-256 `f31320b69814f516e223eb49402993032eeb08ef2226178cfa3744252015fc71`; expanded compiler/AOT/native-link fatal count is `0`; exact success marker count is `1`; `ProjectSettings.asset` returns byte-exact to SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` and the clone remains clean.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | `667,136` | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | `45,753,856` | `71314ceb6978d9d68599528154a01d199a02dfc8decb0f5d8324ec0ea734ea22` |
| `UnityPlayer.dll` | `84,237,744` | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted runtime task ran as logged-on `cixanla\mertk`, `LogonType=Interactive`, `RunLevel=Limited`, using `-force-d3d11 -pse-require-d3d11 -pse-atx24-power-cable-assembly-handoff-smoke -screen-fullscreen 0 -screen-width 1280 -screen-height 720`. Intel Iris Xe reports Direct3D 11.0 feature level 11.1. Host/readiness/success counts are `1/1/1`; forbidden count is `0`; player and task results are `0`; graceful Input System shutdown occurs; the task is deleted; player residue is `0`.

`runtime-issue105-r52-interactive.log` is `5,944` bytes / SHA-256 `55976096c2c456fc6c2007b7469031b16e415afa7ab1761ddb3c82bbdbc58a0b`; its receipt is `869` bytes / `d4126973e104e985397ac30fc7fdc1e471b5431bfbffdff8c7d89050cab33f08`.

## Windows foreground OS-input gate

The accepted r2 harness runs in interactive Session 2, verifies the exact player SHA before launch, requires exact r52 readiness and rechecks the exact player foreground window before and after every input stage. It uses `-force-d3d11` and verifies Unity's Direct3D 11.0 / Intel Iris Xe engine lines directly. It records:

- W/A/S/D scan-code down/up as `1/1` for each direction;
- relative mouse-only calls as `18/18`;
- one combined call delivering W + D + mouse as `3/3`;
- a further `30/30` relative mouse deltas while W+D remain held;
- W/D release as `1/1`, player residue `0` and scheduled-task residue `0`;
- final claim `HARNESS_RESULT=PASS human=false input=Win32-SendInput keyboard=W+A+S+D mouse=relative simultaneous=true`.

The accepted report is `1,794` bytes / SHA-256 `f50c61d67b7b630cf4f724908f4f0a382072cd3079509c4808d4308eaa19e24c`; its clean runtime log is `4,079` bytes / `e52ce6dcdf3c33e1a328961a4ac9502568d3740b09dec7f73dd6d05a0b71b7f2`. Runtime forbidden count is `0`. All eight screenshots are nonempty and have unique SHA-256 values; readback visually shows both translation and camera rotation during the combined W+D+mouse phase.

The first r1 harness run is retained as negative harness evidence. It used `-pse-require-d3d11` without a supported smoke flag; the production fail-closed contract intentionally emitted `GARAGE_CUSTOM_PC_WORK_TICKET_RUNTIME_SMOKE ... smoke.graphics-api-mismatch`. The r1 SendInput receipt is not counted as acceptance. The separately named r2 removes that invalid flag, proves D3D11 from Unity engine output and has zero forbidden markers.

A transient Windows Defender Firewall prompt appears as a topmost overlay in part of the screenshot sequence while the exact player remains foreground and continues receiving movement/look. The development player generated two exact-path inbound TCP/UDP block rules when it exited. Both rules were identified by full disposable executable path, removed without touching any other firewall rule, and read back as PersistentStore `0` / ActiveStore `0`.

The screenshots and OS delivery are combined with exact PlayMode same-frame movement/look assertions. They do not claim a real-human session, physical keyboard, physical gamepad or endurance test. Input System gamepad automation is likewise not a physical-gamepad claim.

## Final audit and readback

`windows-final-audit.json` uses schema `pcshop-issue105-windows-final-audit-v1`; all `33` checks pass. It is `8,328` bytes / SHA-256 `bbac78a7bcaec3e3b5d3cf2fae0ed522d05aa2d9deb9e00143338270e2f71454`. Its 39-entry evidence manifest is `10,109` bytes / `0a7d1c4c86e699e7d0525c5abde4486fd71565c40f1982a335e501fb8dc5660c`.

Windows evidence returned to the Mac as `issue105-windows-evidence-r3.tar.gz`, `6,091,832` bytes / SHA-256 `5e3674ded54f9ef8dc115061b8e0112d5946e2e2b7fe32a01b7e72b5a35f0c21`. The Mac readback matches the Windows hash; all `36/36` transported evidence entries match bytes and SHA-256 after excluding the three native binaries that are referenced by manifest but not copied into the evidence tar. Canonical raw evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue105-5d6a3989-r1`.

Final Windows health reports approximately `10.08 GB` free RAM, one healthy/OK Samsung NVMe physical disk, no scoped Unity/player/compiler/PowerShell process, no scheduled task, no validation firewall rule, no removable volume and no USB disk. No USB write, physical checkpoint or USB acceptance claim is made for Issue #105.

## Issue #105 acceptance matrix — technical state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact order and canonical ATX24 line/kind/family/product/item/reservation/allocation tuple. | PASS |
| 2 | Historical `10/10` receipts and owned #89/#91/#93/#95/#97/#99/#102 live chain. | PASS |
| 3 | Exact motherboard/CPU/DDR5/M.2/cooler/GPU/PSU custody/state/receipt chain. | PASS |
| 4 | Exact capacity-one route container, canonical cable, three endpoints, three waypoints and `Loose` state. | PASS |
| 5 | Stable distinct ATX24 handoff operation bound to exact staging receipt. | PASS |
| 6 | Immediate/delayed replay exactly once with no second mutation. | PASS |
| 7 | Foreign/value-equal identities, receipts, prerequisites and targets fail closed. | PASS |
| 8 | Stale revisions, full hands, occupied route and overflow are no-mutation. | PASS |
| 9 | Only registered BuildKit→hands and exact hands↔ATX24-route transfer is accepted. | PASS |
| 10 | Reservation/allocation remains live through pickup, route and unroute. | PASS |
| 11 | Ten receipts/history, installed seven components and other two cable records remain untouched. | PASS |
| 12 | Existing Issue #61 route and #62/#63 cable authorities remain exact. | PASS |
| 13 | Same Unity instance and stable ItemId survive BuildKit→Hands→Routed→Hands. | PASS |
| 14 | Range/focus/LOS/pause/orientation/host/topology/clearance/obstruction/preview gates fail closed. | PASS |
| 15 | Authority-first projection and same-instance recovery are atomic. | PASS |
| 16 | Generic drop/box/stack/cart/raw-transfer/receipt-free bypasses are blocked. | PASS |
| 17 | `ATX24 MONTAJDA`, immutable ticket and `10/10` history remain readable. | PASS |
| 18 | Keyboard/mouse and Input System gamepad pickup→route→unroute flow. | PASS — physical gamepad not claimed |
| 19 | WASD, simultaneous movement+mouse-look, pause/focus and single-consumer regressions. | PASS |
| 20 | PSU unretain/remove and predecessor detach paths remain blocked while ATX24 is routed. | PASS |
| 21 | Electrical readiness remains blocked until EPS12V and PCIe/GPU routes exist. | PASS |
| 22 | Retail/economy/customer/price, Save/Guardian and unrelated systems remain untouched. | PASS |
| 23 | Targeted and full EditMode/PlayMode have zero fail/skip/inconclusive. | PASS |
| 24 | Diff, Repository Guard and universal Mac native gates. | PASS — Guard 33038180913 |
| 25 | Exact-head clean Windows IL2CPP/only-D3D11 runtime, foreground OS input and zero residue. | PASS |
| 26 | Bible/ADR/Evidence/CHANGELOG and private PR/CI chain. | TECHNICAL PASS — PR #106 integration follows this docs commit |
| 27 | Claim explicitly preserves physical human/HID/gamepad/endurance certification for Steam 1.0. | PASS |

The bounded technical acceptance count is `27/27`. Administrative Issue/Roadmap closure is intentionally separate and occurs only after PR #106 source/docs integration and final required Guard checks. Parent Epic #10 and the full Steam 1.0 Goal remain open for EPS12V, PCIe/GPU, electrical, product and release work. Physical human/HID/gamepad and endurance remain mandatory before Steam 1.0 release certification.

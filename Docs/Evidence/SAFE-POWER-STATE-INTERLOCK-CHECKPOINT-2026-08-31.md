# Safe Power-State Interlock — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#125](https://github.com/cixanla/PC-Shop-Empire-3D/issues/125)<br>
**Draft PR:** [#126](https://github.com/cixanla/PC-Shop-Empire-3D/pull/126)<br>
**Parent branch head:** `3df0ada189d4caf8d047b4d5c4e4f2083a1092c0` — Issue #123 docs checkpoint<br>
**Technical head:** `01b89e21e4329489b9a3c666edf5391710eb9c2f`<br>
**Technical tree:** `bc1e5a8ec2e9852dd6d0b32c08b514bbd2c224a4`<br>
**Technical branch:** `codex/issue125-safe-power-state-interlock`<br>
**Current state:** Exact source, Off/Energized authority, immutable on/off receipts, Assembly maintenance interlock, player input/presentation integration, targeted/full Mac tests, universal native build, Apple M1/Metal player smoke, two bounded review rounds, Repository Guard, codesign and settings preservation pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #125 and its Roadmap card remain `OPEN / In Progress`; PR #126 remains draft and no merge or closure is claimed.

## Delivered result

`PcPowerStateAuthority` is reference-bound to the exact current Issue #123 `PowerTestAttemptAuthority` and canonical `AssemblyBuildAuthority`. It owns only a deterministic `Off` or benign `Energized` state and its immutable transition history. It does not own POST, display, firmware, OS, driver, benchmark, thermals, electrical fault/damage, packaging, delivery or settlement.

Power-on requires:

- a non-empty stable power-state operation ID;
- exact expected power-state revision;
- one exact known preflight receipt object;
- that receipt to remain the currently valid receipt after fresh Assembly/cable/budget recomputation;
- one live interlock binding to the same Assembly authority.

Successful power-on stores exactly one immutable receipt and marks only the bound Assembly's energized bit. Exact command replay returns the same receipt instance. Reusing an operation with changed command lineage conflicts. A distinct power-on while already energized fails; revision overflow, foreign receipt and stale preflight fail closed.

Power-off is a separate explicit command. It requires the exact active power-on receipt and current expected revision, stores its own immutable receipt, clears the energized bit and returns the state to Off. Exact replay is same-instance. An unchanged current preflight can support later explicit cycles with distinct deterministic operation IDs.

## Assembly maintenance interlock

One narrow `AssemblyBuildAuthority` boundary owns the power-state binding. While Energized, all live maintenance validators reject before mutation, including:

- motherboard fastener unsecure/remove paths;
- DIMM, storage, cooler, graphics card and PSU unretain/remove paths;
- ATX24, EPS12V and PCIe/GPU cable unroute paths;
- related detach/reseat commands that would change live Assembly or cable lineage.

Exact historical replay is resolved before the maintenance gate. This preserves accepted receipt identity without allowing a distinct command to mutate energized hardware. Domain and player-path tests require inventory, BuildKit, Assembly, all cable revisions and receipt counts to remain byte-for-byte logically unchanged after a blocked attempt.

The native smoke performs the maintenance attempt through `PlayerCarryController.TryPickup` on the routed PCIe/GPU cable. Authority rejection occurs before physical carry begins. The cable remains world-owned/routed; player hands remain empty; no geometry/identity/custody/revision/receipt change is accepted.

## Player interaction and presentation

GarageGraybox version is `garage-safe-power-state-r61-v1`. The existing Assembly Workbench focus anchor, status text, indicator and normal Interact action are reused.

Visible command states are:

```text
E / A: GÜÇ TESTİ ÖN KONTROLÜNÜ ÇALIŞTIR
ÖN KONTROL GEÇTİ • E / A: GÜCÜ AÇ
E / A: GÜCÜ KAPAT • POST BEKLİYOR
GÜÇ AÇIK • POST BEKLİYOR
BAKIM KİLİDİ AKTİF
```

Prompt, readiness and gate reads are observational. They use `TryGet...` APIs and fresh read-only power-budget assessment without instantiating optional gameplay authority. Result-bearing `EnsurePowerTestAttemptsAuthority()` and `EnsurePowerStateAuthority()` run only in the accepted command path after Interact has passed range/focus/LOS/pause/busy/competing-owner gates and has been consumed.

Keyboard/mouse and Input System gamepad South share one command. Concurrent same-frame keyboard+gamepad presses produce one transition. Pause blocks power-off without consuming the press. Existing input replay, held-input, focus, LOS and competing-owner behavior remains fail-closed.

## Independent review and corrections

The first bounded read-only review found that conflicting smoke flags logged an error without quitting native player and that a supported duplicate/foreign power-state binding could escape through `.Value` as an exception. It also identified presentation state reset/authority-creation risks and recommended player-path maintenance proof. The implementation now exits conflicting native smoke with code `1`, returns result-bearing failures, preserves observed energized state, keeps readiness side-effect-free and tests the real carry/binding path.

The follow-up read-only review found two remaining P2 gaps. Merely reading the station prompt could still instantiate `PowerTestAttemptAuthority`, and externally disposing the smoke wrapper could skip nested cleanup `finally` blocks. The final source moves attempt creation behind consumed Interact, makes gate/prompt budget checks read-only and wraps the stack pump in `try/finally` disposal. The follow-up reported no other concrete P0/P1/P2 finding in energized-state preservation, receipt replay, same-frame consumption or player-path maintenance interlock.

The runtime identity was separately audited during the long full-suite run. A stale r60 version string was found, that obsolete run was stopped to avoid wasting resources, all contracts were updated to r61 and the complete accepted test chain was rerun on final source.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Targeted authority/interlock/scene contracts | `6/6` | `19.0634873 s` | `9,104` | `ac3ed499fb5a3674cb104c1f521f68458aaf3d9652fa1d58b15e0a4ce67a1c6d` |
| Targeted preflight/power keyboard+gamepad/presentation contracts | `4/4` | `21.1734558 s` | `16,481` | `6ad193ec948dc734fdcf3bbf2d278199b759e56cc02da35a2ece6c287701e034` |
| Full EditMode | `778/778` | `68.5340744 s` | `644,891` | `22b5fa8adf1d12a3da35e84d29885d8b4074fedb5bce699022889b0598f13cd8` |
| Full PlayMode | `164/164` | `796.9418652 s` | `556,786` | `050a356f7f42129d496ff1e29ab3c6bcc6ab6b218232042c85ff1cf3a05102b7` |

Every accepted XML reports failed, skipped and inconclusive `0`.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Targeted EditMode | `34,762` | `b49147a0431a92ab536831b1ad1e6f5a857c1d106b7a4bd90e78e0dc6e02244a` |
| Targeted PlayMode | `55,714` | `8505d5aa3a215222cfe3dd017e4fd72d0de5f3db87c4e78ba33afb665f0f7500` |
| Full EditMode | `49,131` | `ca5fb0fe103077061c2ce480e18c827b2c4ea6fda5519f6abb3b11ea486d42c1` |
| Full PlayMode | `661,052` | `33d2f70557e85aa086085e29f2524098013aa2a95127c7c448b7d5158b25f47b` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `597,898` | `42e684cf58fb1e9c8ea47c9abae7bd6459eddf1bb9070a53b1144214684be494` |
| Universal app executable | `117,179` | `cd5643fbe7e455ca049ae29350a8847b984bf8a040efbdea419b42a32c989e26` |
| Apple M1/Metal safe-power-state runtime log | `9,618` | `408b778e18679e6fbe224103e508d8cac40022a4569b5ba14352dfd36585c17e` |

Unity reports `330,540,613` build bytes. The app contains `302` files. Its executable is a universal Mach-O with `x86_64 + arm64`, and `codesign --verify --deep --strict` passes.

The graphical Apple M1/Metal player publishes the exact r61 readiness line and exactly one success marker:

```text
GARAGE_POWER_STATE_INTERLOCK_RUNTIME_SMOKE prerequisite-setup=assisted preflight=current power-on=player-triggered power-off=player-triggered input=keyboard+gamepad state=off cycles=1 maintenance-while-energized=blocked receipt=immutable replay=ok presentation=ok post=not-started benchmark=untouched invariants=ok
```

The player exits `0`, Input System reaches `Shutdown`, forbidden/failure marker count is `0` and final player/Unity/shader/IL2CPP residue is `0`.

## Settings, repository and raw evidence

The Issue #125 technical commit contains exactly `34` source, meta and test paths. It contains no ProjectSettings or Packages path. The only local tracked difference is the separately preserved user/editor-owned ProBuilder setting, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it remains unstaged and unreverted.

The universal build transiently serialized one `preloadedAssets` entry into `ProjectSettings/ProjectSettings.asset`. The exact build-induced hunk was inspected and restored to repository baseline with `preloadedAssets: []`; final explicit diff is clean.

Repository Guard run [33361533350](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33361533350) passed on source `01b89e2`. Draft PR #126 is open and mergeable. Local `Tools/verify-repository.sh`, `git diff --check`, staged scope audit, fatal-token audit, codesign and residue checks pass. Raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue125-power-state-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, screenshot, process/task/firewall cleanup or Mac readback claim is made. Closure still requires a clean checkout of the exact accepted technical source/tree, full EditMode/PlayMode, native r61 smoke, fatal-token audit, evidence readback and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification, exact accepted source/evidence, collision-safe incoming path, complete SHA/size/path/Git comparison, atomic rename and second readback. Absence is not a pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #125 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Dedicated power-state authority is reference-bound to exact canonical preflight and Assembly authorities. | PASS — domain, initialization and invariant contracts |
| 2 | Power-on requires stable operation, expected revision and freshly current exact preflight lineage. | PASS — domain plus player command contracts |
| 3 | Success publishes one immutable receipt; exact replay is same-instance and changed reuse conflicts. | PASS — domain and native smoke |
| 4 | Explicit power-off is deterministic/replay-safe and preserves all unrelated gameplay authority state. | PASS — domain, PlayMode and native smoke |
| 5 | Every player-reachable maintenance path is blocked before domain/world mutation while Energized. | PASS — validator coverage, player carry path and invariant receipts |
| 6 | Existing Workbench surface is reused and distinguishes Off/Energized/POST-waiting without duplicate geometry or authority. | PASS — scene, presentation and hero regressions |
| 7 | Keyboard/mouse and virtual-gamepad share one gated single-consumer command; pause and concurrent input fail closed. | PASS — targeted/full PlayMode and native smoke |
| 8 | POST/firmware/OS/benchmark/fault/damage and later workflow state remain absent; benchmark remains blocked. | PASS — domain and native boundary assertions |
| 9 | Targeted/full suites, universal Mac native and clean physical Windows IL2CPP/D3D11 runtime pass. | PARTIAL — Mac PASS; physical Windows DEFERRED |

Issue #125 remains open and its Roadmap card In Progress until physical Windows validation and later integration/closure steps complete. PR #126 is draft and intentionally does not auto-close the issue while acceptance #9 is partial.

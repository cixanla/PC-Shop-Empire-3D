# Active-UEFI-Bound Deterministic Fictional OS Installation — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#131](https://github.com/cixanla/PC-Shop-Empire-3D/issues/131)<br>
**Draft PR:** [#132](https://github.com/cixanla/PC-Shop-Empire-3D/pull/132)<br>
**Parent branch head:** `b6070cec7da818bce9d333f97def192603b520cf` — Issue #129 docs checkpoint<br>
**Technical head:** `9e6a2334a3d6d778b97ebb9ee6d43e7cd8dbc31f`<br>
**Technical tree:** `dd06f64f295f17d7285938845217e19b9e30fe57`<br>
**Technical branch:** `codex/issue131-active-uefi-bound-fictional-os-install`<br>
**Current state:** Exact UEFI/storage-bound fictional OS authority, immutable receipt/history, same-storage power-off/reseat persistence, deterministic two-step player path, P0 power-off recovery, full Mac tests, universal native build, Apple M1/Metal player smoke and Repository Guard pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #131 and its Roadmap card remain `OPEN / In Progress`; PR #132 remains draft and no merge or closure is claimed.

## Delivered result

`PcFictionalOsInstallationAuthority` is a separate downstream authority bound to one exact `PcPowerStateAuthority` and its exact `AssemblyBuildAuthority`. One successful immutable receipt binds:

- one non-empty stable OS installation operation ID;
- the exact owner authority pair;
- exact current `PcFirmwareBaselineReceipt`;
- that firmware receipt's exact POST, power-on and accepted preflight lineage;
- exact physical M.2 storage item and product identity;
- the preflight snapshot's exact storage-secure operation and full Assembly revision;
- expected current power-state revision;
- expected and resulting monotonically increasing OS revision;
- bounded `WorkshopStandard / InstalledForDriverStage` result.

Creation and history validation both compare the stored storage lineage with the immutable preflight snapshot. The source Assembly secure receipt must be the exact `SecureStorageDevice` operation for the same item/product. Its revision may precede the later complete-build snapshot, which preserves the physically correct order of assembly operations.

Exact command replay returns the same object instance; changed reuse conflicts. A second distinct install for the same storage item or same source firmware receipt is blocked. Null, foreign, stale, historical, Off-cycle, owner mismatch, missing/unsecured storage and malformed history fail closed.

The installed result belongs to the storage item, not the active power cycle. Power-off preserves it. Removing the item makes current-build evaluation `NotCurrent`; reseating and re-securing the same physical item restores Installed without a reinstall. A different item is not considered installed. Inventory, BuildKit, reservation, custody, Assembly, cable, Economy, power, POST, firmware and benchmark state remain untouched.

## Player path and P0 recovery

The existing Workbench/station presents this sequence without adding scene geometry or a new input action:

```text
E / A: GÜCÜ AÇ
LMB / RT: UEFI SETUP'I AÇ
LMB / RT: KAYDET VE ÇIK
LMB / RT: KURGUSAL OS KURULUMUNU AÇ
LMB / RT: KURULUMU BAŞLAT VE TAMAMLA
E / A: GÜCÜ KAPAT
KURGUSAL OS KURULDU • DEPOLAMADA KALICI • SONRAKİ AŞAMA: DRIVER
```

The first OS Primary Action opens review; the second completes installation. Pause state, a raw Pause edge, range/focus/LOS loss, busy hands and a competing world-interact owner invalidate an open review without consuming the Primary edge. Returning to the station requires a fresh review press before completion. Keyboard/mouse and virtual gamepad paths use the same authority command.

Interact has strict priority. Same-frame power-off plus Primary Action performs exactly one power-off, consumes both accepted edges and emits no OS receipt. A deliberately malformed OS history still leaves the normal `GÜCÜ KAPAT` prompt and player Interact route available, so downstream history cannot energized-softlock the machine.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Storage lineage/domain contracts | `4/4` | `5.4405719 s` | `6,675` | `659ec57c698a8509c8f7522cbb70b24e0ccbc80663516a864d8bf57ad2a47bc1` |
| Existing scene/r64 wiring contract | `1/1` | `0.6450836 s` | `4,298` | `7ce8155417acf28da30221d22add205cecc60d151813ea7196dd8ed8a05362a9` |
| OS keyboard/mouse/gamepad/context/P0 input | `6/6` | `32.6920784 s` | `23,430` | `f9a3efd0f56827d532df8dce7e78ab7be8abeb11bb18c37bf46b6069d10a542b` |
| Power/preflight/POST/UEFI/OS regression class | `17/17` | `73.5998754 s` | `60,729` | `a42bb0a93f14b448bb851d933f51abdfd914118bbcd0522628f4b962cfe168b8` |
| Full EditMode | `788/788` | `81.21818 s` | `652,620` | `46114439c30e06453c6c4b60439f14b08b0a84f4ca6740b94e55d4f17cd51fee` |
| Full PlayMode | `175/175` | `783.460076 s` | `604,675` | `c4fb6f93967ad12719109a27ac310dce5156ef795616b9a47a8aab5964d6d615` |

Every accepted XML reports failed, skipped and inconclusive `0`. Diagnostic r03/r09 compile failures, r10's intentionally discovered over-strict secure-operation revision comparison and r16's test-only unsafe-drop assumption are not accepted evidence. The corrected unique-path r11/r17/r19/r18 results above supersede them.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Storage lineage EditMode | `46,559` | `5d26a78ec95144b9c7a58c3b2a4e01715731da8456ef66b7757d5f4947a52622` |
| Scene contract | `32,445` | `56edf71ce2246534feb870350dcc126798c130933b21b821631370e7948f36eb` |
| OS input/context/P0 PlayMode | `68,089` | `62061353a747072e8f10dacada99636c57a93205606ffbba7ef739b5e6700dbf` |
| Power/POST/UEFI/OS regression | `103,297` | `53aed0feeccc42c9bc6685bcb46ede1a5df96376af40fa63e36bfaee0f608e8a` |
| Full EditMode | `35,103` | `2b03445f79d334513337f03c53eb364594cbe827e09e5bc9cfd65499847b9f57` |
| Full PlayMode | `713,474` | `c3a29615a8c07f2c3e2ddb079caa69937b498b373e600573a8266847280d66e6` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `603,513` | `a2388ae90a119d1f54bddb97845aea8db2b98a78a5eb21fdc33280e96e87cbcf` |
| Universal app executable | `117,179` | `815d1e34a208eddd8272168f0859c1e7dc58b942f71d04eb0fded2f3f46d2244` |
| Apple M1/Metal r64 runtime log | `9,553` | `a4cd77cd43e421bb1511f7e5dfca57a6ef2d8c3f778f78d1012315fa94b2664b` |

Unity reports `330,604,881` build bytes. The app contains `302` files. Its executable is a universal Mach-O with `x86_64 + arm64`; `codesign --verify --deep --strict` reports valid on disk and satisfies the designated requirement.

The graphical 1280×720 Apple M1/Metal player publishes the exact r64 readiness line and exactly one success marker:

```text
GARAGE_FICTIONAL_OS_INSTALLATION_RUNTIME_SMOKE prerequisite-setup=assisted preflight=current power-on=player-triggered post=passed firmware=optimized-defaults-saved os=workshop-standard-installed storage=identity-bound review=player-triggered install=player-triggered input=keyboard+mouse+gamepad power-off=player-triggered state=off persistence=power-off-preserved receipt=immutable replay=ok benchmark=untouched invariants=ok
```

Readiness count is `1`, success count `1`, failure/fatal count `0` and Input System shutdown count `1`. The player exits `0`; final player/Unity/crash-handler/shader residue is `0`.

## Repository, settings and raw evidence

Technical commit `9e6a2334a3d6d778b97ebb9ee6d43e7cd8dbc31f` contains exactly `20` source, meta and test paths with `2,328` insertions and `30` deletions. It contains no ProjectSettings or Packages path. The separately preserved user/editor-owned ProBuilder setting remains the only unrelated tracked difference, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is unstaged and unreverted.

`ProjectSettings/ProjectSettings.asset` is SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` after tests/build/smoke and unchanged from the accepted baseline. No ProjectSettings edit or build-induced restoration was needed.

Repository Guard run [33372528502](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33372528502) passed on source `9e6a233`. Draft PR #132 is clean and mergeable. Local `Tools/verify-repository.sh`, `git diff --check`, staged-scope audit, fatal-token audit, codesign and residue checks pass. Raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue131-fictional-os-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r64 smoke, fatal-token audit and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification, exact accepted source/evidence, collision-safe incoming path, complete SHA/size/path/Git comparison, atomic rename and second readback. Absence is not a pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #131 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1–2 | Separate owner-bound authority; completion requires exact current UEFI, revisions and secured M.2 identity. | PASS — domain/full suites |
| 3–6 | Immutable exact lineage, replay/conflict/duplicate rules and storage-bound power-off/reseat persistence. | PASS — lineage/domain/full suites/native smoke |
| 7–9 | Existing station/input reused; two-step review/install; strict power-off priority; all context losses preserve input. | PASS — scene, targeted input and full PlayMode |
| 10 | Malformed OS history cannot block explicit power-off. | PASS — direct P0 player test/native power-off |
| 11–12 | Gameplay/benchmark authorities remain untouched; Workbench is observer-only. | PASS — domain/input/full suites/native smoke |
| 13 | Targeted/full Mac, universal build, native smoke and Repository Guard pass. | PASS — exact evidence above |
| 14 | Clean physical Windows IL2CPP/D3D11 and physical USB checkpoint/readback pass. | DEFERRED — devices unavailable |

Issue #131 remains open and its Roadmap card In Progress until physical Windows validation and later integration/closure steps complete. PR #132 is draft and intentionally does not auto-close the issue while acceptance #14 is deferred.

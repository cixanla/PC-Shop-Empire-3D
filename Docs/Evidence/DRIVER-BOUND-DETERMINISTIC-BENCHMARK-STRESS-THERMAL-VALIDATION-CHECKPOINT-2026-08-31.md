# Driver-Bound Deterministic Benchmark, Stress and Thermal Validation — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#135](https://github.com/cixanla/PC-Shop-Empire-3D/issues/135)<br>
**Draft PR:** [#136](https://github.com/cixanla/PC-Shop-Empire-3D/pull/136)<br>
**Parent branch head:** `712dec942d82f1887952de7a15e18fa3be93ae4e` — Issue #133 Mac checkpoint<br>
**Technical head:** `f082ef5df913ce6a4664cdda5eb64d1b26f007d6`<br>
**Technical tree:** `c387100c6dd7e314768756ebfb78104f6557081d`<br>
**Technical branch:** `codex/issue135-driver-bound-validation-receipt`<br>
**Current state:** Driver-bound validation authority, immutable receipt/history, fictional integer-only performance catalog/profile, exact current-cycle/electrical/power lineage, controlled rerun and tamper-proof replay, two-step player path, observer-only five-state Workbench presentation, aggregate benchmark-readiness correction, full Mac tests, universal native build and Apple M1/Metal player smoke pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #135 and its Roadmap card remain `OPEN / In Progress`; PR #136 remains draft and no merge or closure is claimed.

## Delivered result

`PcValidationAuthority` is a separate downstream authority bound to one exact `PcFictionalDriverInstallationAuthority`, its exact `PcPowerStateAuthority` and `AssemblyBuildAuthority`, and the exact `PcPowerBudgetAuthority`. One successful immutable receipt binds:

- one non-empty stable validation operation ID;
- exact owner authority instances and expected power/validation revisions;
- exact current installed fictional driver and operating-system/storage receipts;
- exact current firmware, POST, power-on and accepted preflight lineage;
- exact build/chassis, seven component items/products and retain/secure operations;
- exact ATX24, EPS12V and PCIe-GPU cable items, route operations and revisions;
- exact electrical-readiness and sufficient power-budget snapshots;
- versioned performance catalog and validation profile identities;
- deterministic benchmark, stress, thermal, power and quality metrics.

Completion re-evaluates current driver installation, power cycle, firmware baseline, aggregate mechanical benchmark readiness, exact electrical readiness and power budget. Null, foreign, stale, historical, Off/new-cycle, owner mismatch, OS/storage/hardware/cable drift, missing cooler/TIM, malformed history, insufficient margin, overflow and score/thermal limit failures produce no receipt and no validation revision.

Exact same-command replay returns the same receipt object instance. Changed reuse conflicts. A distinct operation ID allows a controlled rerun in the same exact current context and appends a monotonically increasing immutable receipt. `ValidateReceiptHistory()` executes before replay lookup; deliberate receipt-field corruption makes replay itself fail with `ReceiptHistoryInvalid`.

Power-off keeps history and replay evidence but makes `EvaluateCurrentValidation()` return `NotCurrent`. A new energized/POST/firmware cycle requires a new explicit validation run.

## Deterministic fictional result

The prototype uses catalog ID `catalog.performance.prototype.legacy-v1` and profile ID `assembly.validation-profile.workshop-v1`.

| Metric | Exact fictional input/calculation | Result |
|---|---|---:|
| Aggregate benchmark | Motherboard `34` + CPU `117` + DDR5 `31` + M.2 `49` + cooler `25` + GPU `121` + PSU `24` | `401` |
| Fixed stress policy | Profile-owned integer step count | `300` steps / `Stable` |
| CPU peak | `22 °C` ambient + integer-ceiling rise from `125 W`, scale `50`, cooler `140 W` | `67 °C` |
| GPU peak | `22 °C` ambient + integer-ceiling rise from `200 W`, scale `50`, cooling `240 W` | `64 °C` |
| Power | Existing exact budget snapshot | `380 W` draw / `500 W` minimum / `550 W` installed |
| Margin | Installed minus minimum | `+50 W` |
| Quality | Minimum `300`, Good `380`, Excellent `500` | `Good` |
| Terminal result | All exact lineage and boundaries pass | `PassedForQualityStage` |

No calculation uses wall-clock, frame duration, FPS, random values, floating-point performance truth, host CPU/GPU query or physical sensor data.

## Player path and presentation recovery

The existing Workbench/station presents the extended sequence without adding scene geometry or a new input action:

```text
E / A: GÜCÜ AÇ
LMB / RT: UEFI SETUP'I AÇ
LMB / RT: KAYDET VE ÇIK
LMB / RT: KURGUSAL OS KURULUMUNU AÇ
LMB / RT: OS KURULUMUNU BAŞLAT VE TAMAMLA
LMB / RT: DRIVER KURULUMUNU AÇ
LMB / RT: DRIVER KURULUMUNU BAŞLAT VE TAMAMLA
LMB / RT: VALIDATION İNCELEMESİNİ AÇ
LMB / RT: VALIDATION RUN'I BAŞLAT VE TAMAMLA
E / A: GÜCÜ KAPAT
```

The first validation Primary Action opens review; the second completes the run. Workbench has five presentation-only states: `Waiting`, `Reviewing`, `Passed`, `Rejected`, `NotCurrent`. Passed state shows score, fixed stress stability, CPU/GPU peaks, draw/minimum/installed/margin and quality. Rejected state exposes the exact failure code and creates no receipt. Workbench only calls `TryGetValidation`; observation never creates authority.

Motor pause, raw Pause edge, range/focus/LOS loss, busy hands, competing owner and exact context changes invalidate an open review without consuming the Primary edge. Returning to the station requires a fresh review press. Keyboard/mouse and virtual gamepad use the same authority command.

Interact has strict priority. Same-frame power-off plus Primary Action performs exactly one power-off and emits no validation receipt. A deliberately malformed validation history still leaves the normal power-off prompt and player Interact path available, preventing downstream history from energized-softlocking the machine.

## Exact Mac tests

| Gate | Accepted run | Result | Duration | XML bytes | XML SHA-256 |
|---|---|---:|---:|---:|---|
| Performance catalog/profile contracts | `r03-catalog` | `5/5` | `0.0417073 s` | `7,471` | `2d5dd84f18fba92114bd0d3f29b7c7d41d4e5754ac5424100d756cbd6f67f5b6` |
| Validation authority, lineage, deterministic boundaries and history | `r23-domain-history-hardening` | `125/125` | `145.9844652 s` | `101,211` | `69c5a18216f9960d494621107a2e59438ee7d2a703fe088d17e291f185dfa8c1` |
| Keyboard/mouse/gamepad/context/P0 validation input | `r09-validation-input` | `6/6` | `25.4002262 s` | `23,631` | `b7589e6ad4247c64437bfad0afbad299d0fa884f5aae7abc5f50b428575b13de` |
| Existing scene/r66 wiring and smoke compile contract | `r16-smoke-compile-scene` | `12/12` | `1.1123641 s` | `12,636` | `fabd11772dda5e49fb171d3783e12aab93d4ba7165ddec164f02c6b73ac25d41` |
| Power/POST/UEFI/OS/driver/validation regression class | `r12-power-validation-chain` | `29/29` | `151.5541486 s` | `102,787` | `5cffcc1d14b6800c5ac1e1b41e2399e371c50c3590f7ade6b6dfcdf84ba907e3` |
| Final full EditMode after replay hardening | `r24-full-editmode-post-hardening` | `804/804` | `147.1077425 s` | `665,545` | `b1639995a00186ec0d72849d0d79fb11b672112f26b5a56704f0518f8e84f062` |
| Final full PlayMode after replay hardening | `r25-full-playmode-post-hardening` | `187/187` | `1017.6613419 s` | `654,816` | `5ccfe1b1a4ae5c14fd7f6e9bf6feb4fa16367fb9eb2e68bb7ac6fdb347cdf660` |

Every accepted XML reports failed, skipped and inconclusive `0`. Diagnostic failed runs `r04`, `r08`, `r11`, `r19` and `r21` exposed stale expectation/test defects that were corrected; they are not accepted evidence. The r23 history hardening and final r24/r25 runs supersede earlier green r20/r22 results.

| Accepted log | Bytes | SHA-256 |
|---|---:|---|
| Catalog | `32,120` | `a564fe16dabbd878735922ef42ef6f44bd75da1ed34bb020a35feb499a6111a9` |
| Validation authority/history | `46,755` | `b38056e483c3b0c055f3a3b901011c6d1518ea972f344d4fc4d0c333d4b73634` |
| Validation input/context/P0 | `75,060` | `51a24bcb7fb8a75d2b411c1c1f28e393839be3c944940c7a693cb81557b33ad1` |
| Scene/r66 contract | `47,776` | `52d2ceb91ba7ed3dbda4d0ce525bfa4c08505cf562fe722de9aeefb9d4ef33b5` |
| Power-to-validation regression | `158,044` | `db830612c486560f4452070dbffdec6d85c9db8ca561dc18f3bfa27001000c48` |
| Final full EditMode | `35,147` | `68e74e0b1b79e0f53394eff413b73347b1a8a487f19c025979b9d25bdc8063cc` |
| Final full PlayMode | `770,593` | `dcb2a9b0cf849a7f19c5d34c269a452a051f22317c6c9e2f02260ff551e44fea` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `601,732` | `352714cc97f4423580e98ecaa1d47f494b65c0d16267d9a062c1d78f07f6d043` |
| Universal app executable | `117,179` | `0e5bbb99a8eef26e6d121660788c5bec6c3de3c667725defb7e4f8b388a7672f` |
| Apple M1/Metal r66 runtime log | `9,596` | `4197d3e16e7d82045aed1833797023df01c6c054faac4dd02ad57d7bcf8917a6` |

Unity reports `330,709,325` build bytes. The app contains `302` files. Its executable is universal Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes. Bundle identifier is `com.cixanla.pcshopempire3d`, sealed resources count is `299`, and the ad-hoc signature is valid on disk.

The graphical 1280×720 Apple M1/Metal player publishes exact r66 readiness with `validation=ready` and exactly one success marker:

```text
GARAGE_VALIDATION_RUNTIME_SMOKE prerequisite-setup=assisted driver=current firmware=current review=player-triggered validation=player-triggered input=keyboard+mouse+gamepad result=passed score=401 stress-steps=300 stress=stable cpu-peak=67C gpu-peak=64C power-draw=380W minimum-psu=500W installed-psu=550W power-margin=50W quality=good receipt=immutable replay=ok power-off=player-triggered current-after-power-off=false history=preserved upstream=unchanged invariants=ok
```

Readiness count is `1`, success count `1`, failure/fatal count `0`. Player exits `0`; final player/Unity/shader/IL2CPP residue is `0`.

## Repository, settings and raw evidence

Technical commit `f082ef5df913ce6a4664cdda5eb64d1b26f007d6` contains exactly `52` source, meta and test paths with `4,624` insertions and `96` deletions. It contains no ProjectSettings or Packages path. The separately preserved user/editor-owned ProBuilder setting remains the only unrelated tracked difference, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is unstaged and unreverted.

`ProjectSettings/ProjectSettings.asset` is SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` after tests/build/smoke and unchanged from the accepted baseline. No ProjectSettings edit or build-induced restoration was needed.

Local `Tools/verify-repository.sh`, `git diff --check`, staged-scope audit, GUID uniqueness, deterministic-code scan, codesign and residue checks pass. Draft PR #136 Repository Guard run [33389640619](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33389640619) passed on docs head `7eaf2d53d7c905bf1f7cfb328f9d81b2e2d73994`. Raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue135-validation-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r66 smoke, fatal-token audit and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification, exact accepted source/evidence, collision-safe incoming path, complete SHA/size/path/Git comparison, atomic rename and second readback. Absence is not a pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #135 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Separate validation-only authority and immutable receipt ledger. | PASS — source/domain/full suites |
| 2 | Versioned fictional integer-only performance catalog/profile. | PASS — catalog tests/static audit |
| 3–4 | Exact owner instances and current driver/OS/power/firmware/electrical/power-budget lineage. | PASS — authority/regression/native smoke |
| 5 | Aggregate benchmark readiness succeeds only for complete 10-part/three-route build. | PASS — focused regression/full suites |
| 6 | Receipt preserves exact immutable driver-to-hardware/cable/profile lineage. | PASS — domain/history tests |
| 7–8 | Integer-only deterministic score/stress/thermal/power/quality result. | PASS — boundary tests/native exact marker |
| 9 | Invalid/foreign/stale/off/drift/overflow/limit failures are no-mutation fail-closed. | PASS — authority boundary matrix |
| 10 | Same-instance replay, conflict detection and controlled rerun history. | PASS — direct tests; replay-before-history gap hardened |
| 11 | Power-off preserves history but makes current result NotCurrent. | PASS — domain/input/native smoke |
| 12 | Validation leaves all upstream authorities unchanged. | PASS — zero-mutation snapshots/native marker |
| 13–15 | Existing interaction surface; two-step review/run; strict P0 power-off and context-loss recovery. | PASS — scene/input/full PlayMode/native smoke |
| 16 | Workbench is observer-only with five distinct states and exact metrics/failure code. | PASS — input/presentation/static audit |
| 17 | Targeted/full Mac, universal build, native smoke and Repository Guard. | PASS — Repository Guard `33389640619` |
| 18 | Clean physical Windows and physical USB checkpoint/readback. | DEFERRED — devices unavailable |

Issue #135 remains open and its Roadmap card In Progress until the deferred physical gates and integration/closure steps complete. PR #136 is draft and intentionally does not auto-close the issue.

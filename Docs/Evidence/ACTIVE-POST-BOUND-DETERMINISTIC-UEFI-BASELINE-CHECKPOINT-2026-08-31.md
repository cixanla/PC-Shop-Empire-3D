# Active-POST-Bound Deterministic UEFI Baseline — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#129](https://github.com/cixanla/PC-Shop-Empire-3D/issues/129)<br>
**Draft PR:** [#130](https://github.com/cixanla/PC-Shop-Empire-3D/pull/130)<br>
**Parent branch head:** `c10f8f314e096465ac0a1df49cedd46347660a39` — Issue #127 docs checkpoint<br>
**Technical head:** `86df0bc236e2bf90bfc3fa0482715f06242e6f13`<br>
**Technical tree:** `953a09fd3c462e387229a78148c8b28040d797f3`<br>
**Technical branch:** `codex/issue129-post-bound-uefi-baseline`<br>
**Current state:** Exact source, immutable active-POST-bound UEFI baseline receipt, deterministic review/save/exit input, P0 power-off recovery, full Mac tests, universal native build, Apple M1/Metal player smoke, Repository Guard, codesign and settings preservation pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #129 and its Roadmap card remain `OPEN / In Progress`; PR #130 remains draft and no merge or closure is claimed.

## Delivered result

Existing `PcPowerStateAuthority` now owns a third, separate immutable ledger for `PcFirmwareBaselineReceipt`; no shadow firmware or presentation authority was introduced. One successful receipt binds:

- one non-empty stable firmware operation ID;
- the exact owner `PcPowerStateAuthority`;
- the exact current `PcPostStartupReceipt`;
- that POST's exact power-on and accepted preflight receipt lineage;
- expected/current power-state revision;
- expected and resulting monotonically increasing firmware revision;
- the bounded fictional `OptimizedDefaults / SavedAndExited` result.

Save is accepted only while the exact source POST is current and Energized. Null, foreign, stale, Off-cycle and historical POST sources fail closed. Exact replay returns the same object instance; changed reuse conflicts; a second distinct save for the same POST is blocked. Receipt-history validation checks owner/source identity, mappings, monotonic revision, exact POST/power/preflight lineage, one-per-POST and active/Off folding.

Power-off clears only the active firmware pointer. Historical lookup and exact replay remain immutable. A later power cycle requires a new power-on, POST, firmware operation ID and next revision. Firmware save changes no Inventory, BuildKit, reservation, custody, Assembly, component/cable route, Economy, power-transition, POST or benchmark authority.

## Player path and P0 recovery

The existing Workbench/station accepts the following sequence without adding scene geometry or a new input action:

```text
E / A: GÜCÜ AÇ
E / A: GÜCÜ KAPAT • POST GEÇTİ • LMB / RT: UEFI SETUP'I AÇ
UEFI SETUP • OPTIMIZED DEFAULTS • LMB / RT: KAYDET VE ÇIK • E / A: GÜCÜ KAPAT
E / A: GÜCÜ KAPAT • UEFI BASELINE KAYDEDİLDİ • SONRAKİ AŞAMA: OS
```

Interact has strict priority over Primary Action. Same-frame power-off plus firmware input performs one power-off, consumes both accepted edges and emits no firmware receipt. Paused and competing-owner inputs are not executed or consumed. Review state is local presentation state and resets when its exact POST/power context disappears.

The P0 malformed-history test deliberately invalidates the test authority's firmware revision ledger, confirms `ValidateReceiptHistory()` fails, confirms `GÜCÜ KAPAT` remains visible and then executes the normal player Interact path. The machine reaches Off, active power/POST/firmware pointers clear, and the test restores its private corruption before final invariant validation. Downstream firmware history cannot softlock explicit power-off.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Firmware authority contracts | `3/3` | `7.3003171 s` | `5,875` | `15cdaf4d4bb5f3884bf5b9258859fc013124f7f207fe3559b24e492a8afdb127` |
| Existing scene/R63 wiring contract | `1/1` | `0.4365741 s` | `4,298` | `cbd27f61734830ce440fa5a56f21d351c32968e731e630446ca9634a1a35842e` |
| Keyboard/mouse/gamepad/co-edge/P0 input | `5/5` | `13.4167009 s` | `19,875` | `7bacca84d4119a85614dfe6e875386bab9511b68c8cc8fce8f58c75baee725a` |
| Power/preflight/POST/firmware regression class | `10/10` | `38.7389138 s` | `36,647` | `c80e6bdd491e15d598a03521c747f7ed4ce58fccfe6f81d0d1c857150f064ed8` |
| Full EditMode | `784/784` | `90.0777855 s` | `649,516` | `9a0096d49ac614350113ac84db7245831c46228862381427af41d3ffb7526159` |
| Full PlayMode | `169/169` | `750.3263804 s` | `577,630` | `f6d718a817a63b635985b2e544560f7c41785748b82fef5a3532e9e7eb4e61b0` |

Every accepted XML reports failed, skipped and inconclusive `0`. Earlier r01 had no XML because `-quit` ended the runner before discovery; r04 discovered `0` because the original method names did not match the filter. Neither is accepted evidence. The later unique-path results above supersede diagnostic runs.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Firmware authority EditMode | `47,230` | `f2844e24951dffa0e13630cb60cab3e7f92780b4e90b999e08afa0d4bc522162` |
| Scene contract | `32,829` | `b299d42609f1cd673c90f18d2f1355a244f4f69bd3677cc35c2b22eef471099b` |
| Firmware input/P0 PlayMode | `70,402` | `316f3c2732a3ace4f6dfd1d9464d3e47495a39601ede8399a585540005b630b6` |
| Power/POST regression | `76,299` | `0efac58da1bc42c3129a385a09b4f70f97ad58a2b633c2b88595dcea642aed11` |
| Full EditMode | `35,433` | `0130fed9798ea222094ed606e0ca551f21616478e0ec0281e26b68a4f56b323e` |
| Full PlayMode | `683,933` | `81e7a8446b458583f4a4ebe03086d4846e7fa5674410666afa25c5999df611d6` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `602,084` | `98c543622ac769389f5c98d1a580cdb38229ffe1eea0818d2dcb9e33b871de09` |
| Universal app executable | `117,179` | `2d55d534a6b692f2594c7135cb4b13b4fabc6085165e27d244187f8881700a1f` |
| Apple M1/Metal r63 runtime log | `9,683` | `6c7a75031480931bba77798d34a08b1cb431a2b4b56c24e5e7af2be8d9cf3b56` |

Unity reports `330,573,681` build bytes. The app contains `302` files. Its executable is a universal Mach-O with `x86_64 + arm64`; `codesign --verify --deep --strict` reports valid on disk and satisfies the designated requirement.

The graphical 1280×720 Apple M1/Metal player publishes the exact r63 readiness line and exactly one success marker:

```text
GARAGE_FIRMWARE_BASELINE_RUNTIME_SMOKE prerequisite-setup=assisted preflight=current power-on=player-triggered post=passed firmware=optimized-defaults-saved review=player-triggered save-exit=player-triggered input=keyboard+mouse+gamepad power-off=player-triggered state=off receipt=immutable replay=ok active-clear=ok history=preserved benchmark=untouched invariants=ok
```

Readiness count is `1`, success count `1`, failure/fatal count `0` and Input System shutdown count `1`. The player exits `0`; final player/Unity/crash-handler/shader residue is `0`.

## Repository, settings and raw evidence

Technical commit `86df0bc236e2bf90bfc3fa0482715f06242e6f13` contains exactly `18` source, meta and test paths. It contains no ProjectSettings or Packages path. The separately preserved user/editor-owned ProBuilder setting remains the only unrelated tracked difference, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is unstaged and unreverted.

`ProjectSettings/ProjectSettings.asset` is SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` after the build and unchanged from the accepted baseline. No ProjectSettings edit or build-induced restoration was needed.

Repository Guard run [33367768909](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33367768909) passed on source `86df0bc`. Draft PR #130 is open and mergeable. Local `Tools/verify-repository.sh`, `git diff --check`, staged-scope audit, fatal-token audit, codesign and residue checks pass. Raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue129-uefi-baseline-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task/firewall cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r63 smoke, fatal-token audit, evidence readback and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification, exact accepted source/evidence, collision-safe incoming path, complete SHA/size/path/Git comparison, atomic rename and second readback. Absence is not a pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #129 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1–2 | Existing power authority owns the receipt; save requires exact current POST and revisions. | PASS — domain/full suites |
| 3–4 | Immutable optimized-default receipt, exact lineage, separate revision, same-instance replay and duplicate rejection. | PASS — domain contracts |
| 5–6 | Power-off clears active only; gameplay/benchmark/power/POST state remains untouched. | PASS — domain, input and native smoke |
| 7 | Existing Workbench geometry/focus/range/LOS/input is reused. | PASS — scene and full suites |
| 8–10 | Interact priority, no double dispatch, pause/owner gates and review reset. | PASS — targeted input/full PlayMode |
| 9 P0 | Review/save/malformed history can never block explicit power-off. | PASS — direct malformed-history player test and native power-off |
| 11–12 | Side-effect-free presentation and exact receipt-history/session invariants. | PASS — targeted/full suites/native smoke |
| 13 | Targeted/full Mac, universal build and native keyboard/mouse/gamepad smoke pass. | PASS — exact evidence above |
| 14 | Clean physical Windows IL2CPP/D3D11 and physical USB checkpoint/readback pass. | DEFERRED — devices unavailable |

Issue #129 remains open and its Roadmap card In Progress until physical Windows validation and later integration/closure steps complete. PR #130 is draft and intentionally does not auto-close the issue while acceptance #14 is deferred.

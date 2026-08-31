# Validation-Bound Quality Sign-Off and Packaging Release — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#137](https://github.com/cixanla/PC-Shop-Empire-3D/issues/137)<br>
**Draft PR:** [#138](https://github.com/cixanla/PC-Shop-Empire-3D/pull/138)<br>
**Parent branch head:** `17f74a6f953707b9cd7240957a0cc906f614aeda` — Issue #135 docs checkpoint<br>
**Technical head:** `b6c0f629b78566d743dbb041bfaf792f7c0164c8`<br>
**Technical tree:** `36f8cb6cec9340966181511a18f3caa276eb12f2`<br>
**Technical branch:** `codex/issue137-validation-bound-quality-release`<br>
**Current state:** Validation-bound quality authority, immutable release receipt/history, exact work-order/ticket/reservation and ten-line component/cable lineage, matching safe shutdown, current Assembly drift gate, controlled rerun/tamper-proof replay, two-step player path, seven-state Workbench presentation, full Mac tests, universal native build and Apple M1/Metal player smoke pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #137 and its Roadmap card remain `OPEN / In Progress`; PR #138 remains draft and no merge or closure is claimed.

## Delivered result

`CustomPcQualityReleaseAuthority` is a separate Unity-independent downstream authority bound to one exact `CustomPcWorkOrderAuthority` and one exact `PcValidationAuthority`. One successful immutable receipt binds:

- one non-empty stable quality-release operation ID and expected quality revision;
- exact owner work order and exact owner physical work ticket;
- exact request, quote, customer binding, inventory claim and workbench lineage;
- exact ten serialized reservation lines: seven component item/product pairs and typed ATX24, EPS12V and PCIe-GPU cable items;
- exact owner `PassedForQualityStage / Stable` validation receipt;
- exact validation source firmware, POST, power-on and accepted preflight lineage by reference;
- exact matching owner `PowerOff / Off` receipt and current de-energized power revision;
- exact build/chassis, seven component items and retain/secure operations;
- exact three cable items, route operations and cable revisions;
- immutable benchmark, stress, thermal, power and quality metrics;
- terminal `ReadyForPackaging` result.

Completion re-evaluates owner histories, exact serialized reservations, current safe power state and full electrical readiness. Null, foreign, stale, historical, changed job/ticket, mismatched customer/claim, missing reservation, wrong cable kind, non-passed validation, wrong shutdown cycle, energized state, new power cycle, Assembly/cable drift, revision mismatch/overflow and malformed history produce no release receipt.

Exact same-command replay returns the same receipt object instance. Changed reuse conflicts. A distinct operation ID permits a controlled second review of the same exact current evidence and appends a monotonically increasing receipt. `ValidateReceiptHistory()` executes before replay lookup; deliberate receipt-field corruption makes replay itself fail with `ReceiptHistoryInvalid`.

Power-on or Assembly drift keeps historical evidence but makes `EvaluateCurrentRelease()` return `NotCurrent`. A fresh package release therefore requires a fresh current validation and its matching safe shutdown.

## Player path and presentation recovery

The existing Workbench/station extends the physical chain without adding scene geometry or a new input action:

```text
LMB / RT: VALIDATION İNCELEMESİNİ AÇ
LMB / RT: VALIDATION RUN'I BAŞLAT VE TAMAMLA
E / A: GÜCÜ KAPAT
LMB / RT: KALİTE DOSYASINI İNCELE
LMB / RT: PAKETLEME SERBEST BIRAK
```

The first quality Primary Action opens review; the second creates the release receipt. Workbench distinguishes `WaitingForValidation`, `AwaitingSafeShutdown`, `ReadyForReview`, `Reviewing`, `ReadyForPackaging`, `Rejected` and `NotCurrent`. Ready state exposes the exact work order/ticket result, score `401`, quality `Good` and safe-shutdown provenance. Workbench calls only `TryGetQualityRelease`; observation never creates authority.

Motor pause, raw Pause edge, range/focus/LOS loss, busy hands, competing owner and exact context changes invalidate an open review without consuming the Primary edge. Returning to the station requires a fresh review press. Keyboard/mouse and virtual gamepad use the same command path.

Interact has strict priority. Same-frame power-off plus Primary Action performs exactly one power-off and cannot skip directly into review or release. Deliberately malformed quality history blocks quality Primary Action but leaves the normal power-on prompt and player Interact path available, preventing downstream history from softlocking the station.

## Exact Mac tests

| Gate | Accepted run | Result | Duration | XML bytes | XML SHA-256 |
|---|---|---:|---:|---:|---|
| Final scene/r67 wiring and smoke compile contract | `r23-final-scene` | `12/12` | `1.369341 s` | `12,637` | `77febac9db87b7f08fd03fee5fa3486050da064c8bcbe647d02b071d745a1517` |
| Final quality authority, lineage, currentness and history | `r24-final-quality-edit` | `5/5` | `8.1807401 s` | `7,506` | `d57abc735d99c3ed27b346344d285f324d48abee75fa2b35182a6198fe2b5c02` |
| Final keyboard/mouse/gamepad/context/P0 quality input | `r25-final-quality-play` | `4/4` | `31.7840556 s` | `17,062` | `ad4f03f067ed05e785521568bde2318aecbc22537932cbbf5f1c70997d6b20a7` |
| Validation player-path regression after r67 presentation extension | `r18-validation-play` | `6/6` | `41.0752446 s` | `23,881` | `c1ecf84bcd26d49797bc24050b24af94508bb79fb6c5f80cbce877600385e9e4` |
| Final full EditMode | `r21-final-full-edit` | `810/810` | `80.9964666 s` | `670,284` | `b9df3c799a9ce5df9eb84c0abf195b9d4bf1c4807054ee2d8406e6037931691d` |
| Final full PlayMode | `r22-final-full-play` | `191/191` | `955.0906592 s` | `674,528` | `b824110473acea4de728b6beee62efd4cb0d509da261c28638331fa14b108c3e` |

Every accepted XML reports failed, skipped and inconclusive `0`. Diagnostic r09 exposed a test-fixture ownership gap and r17 exposed the predecessor validation smoke's stale post-shutdown UI expectation; both were corrected before final gates. An experimental direct full-chain PlayMode test was stopped after proving uneconomic for the acceptance suite; its temporary test and generated init scenes were removed before final-source runs and are not accepted evidence.

| Accepted log | Bytes | SHA-256 |
|---|---:|---|
| Final scene/r67 contract | `35,083` | `1d13e0d0fa4020c6a7a01f1f32f690d9332561e8ce5987f69938341c5da1ae8e` |
| Final quality authority/history | `31,949` | `663d3f71229200daeacd2518e2be7a6f28903206f5fa19b34aa3610ebc31f7a3` |
| Final quality input/context/P0 | `53,586` | `00d42c19a0574abf4c608b2aa6f86251736b2d081ee8f4af959de299239df20d` |
| Validation input regression | `73,881` | `306ec402f2fd89c02c8e08faeafd9e616ae8887d95b3454f4d27199f10eab5c0` |
| Final full EditMode | `35,063` | `6eaa20a396065643438491aeed1954250f5cb568ddb22beb6665ec40a62eac0a` |
| Final full PlayMode | `790,214` | `8ddcf0e3eba85c6ac2f4b5e25b87f8b9a5bec88d8eb6ebaedf681d633de49999` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `598,000` | `aa4a23fe327f51748366e65baad424ffc753fe3086e4fbca8ad7e7beccf581fc` |
| Universal app executable | `117,179` | `de920bcd2d1c0ac8c8e7317ba082356487d4c50999b2acb20cecb04fded00941` |
| Apple M1/Metal r67 runtime log | `9,557` | `d2fae1e5154fe632c8eb9dd9752c3eb841f21d3fac49afa39572e01e76e36b0d` |

Unity reports `330,776,338` build bytes. The app contains `304` files and occupies `322,904 KiB` by filesystem blocks. Its executable is universal Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes. Bundle identifier is `com.cixanla.pcshopempire3d`, sealed resources count is `301`, and the ad-hoc signature is valid on disk.

The graphical 1280×720 Apple M1/Metal player publishes r67 readiness with `validation=ready quality-release=ready` and exactly one success marker:

```text
GARAGE_QUALITY_RELEASE_RUNTIME_SMOKE prerequisite-setup=assisted validation=passed stress=stable power-off=player-triggered safe-shutdown=exact quality-review=player-triggered release=player-triggered input=keyboard+mouse+gamepad job=exact line-lineage=10 result=ready-for-packaging score=401 quality=good receipt=immutable replay=ok history=preserved upstream=unchanged invariants=ok
```

Success count is `1`; explicit quality failure/fatal-token count is `0`. The run exited `0`, Input System reached `Shutdown`, and final PC Shop/Unity/player process residue was `0`. Unity's final structured `MemoryLeaks` telemetry record is a shutdown allocation report, not by itself a leak failure, and is not represented as one.

## Repository, settings and raw evidence

Technical commit `b6c0f629b78566d743dbb041bfaf792f7c0164c8` contains exactly `36` source, meta and test paths with `2,749` insertions and `23` deletions. It contains no ProjectSettings or Packages path. The separately preserved user/editor-owned ProBuilder setting remains the only unrelated tracked difference, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is unstaged and unreverted.

`ProjectSettings/ProjectSettings.asset` is SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` after tests/build/smoke and unchanged from the accepted baseline. No ProjectSettings edit or build-induced restoration was needed.

Local `Tools/verify-repository.sh`, `git diff --check`, staged-scope audit, domain-boundary tests, codesign and residue checks pass. Draft PR #138 final Repository Guard run will be recorded after the documentation commit reaches GitHub.

Forty-four raw diagnostic and accepted Mac XML/log artifacts were copied byte-for-byte from `/private/tmp/pse-issue137-*` into `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue137-quality-release-2026-08-31/raw`; source/destination comparison passed and the durable directory occupies `6,656 KiB`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r67 smoke, fatal-token audit and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint, incoming directory, SHA/size/path/Git manifest, atomic rename or second readback claim is made. When the known USB returns, its identity and health must be rediscovered before any write.

## Issue #137 acceptance matrix

| Acceptance area | Mac status | Remaining physical status |
|---|---|---|
| Unity-independent quality authority and boundary | PASS | — |
| Exact work-order/ticket/customer/claim lineage | PASS | — |
| Ten serialized component/cable reservations | PASS | — |
| Passed/stable owner validation receipt | PASS | — |
| Exact matching safe shutdown | PASS | — |
| Current Assembly/cable drift rejection | PASS | — |
| Immutable receipt, replay/conflict/rerun/history | PASS | — |
| Keyboard/mouse and virtual-gamepad two-step path | PASS | Physical-human HID pending |
| Same-frame power priority and context recovery | PASS | Physical-human endurance pending |
| Observer-only seven-state Workbench presentation | PASS | Physical visual acceptance pending |
| Full Mac tests/build/native runtime | PASS | — |
| Clean Windows x64 IL2CPP/D3D11/Iris Xe | — | DEFERRED |
| Immutable USB checkpoint and second readback | — | DEFERRED |
| GitHub integration | Draft PR #138 open | Merge/Issue closure deferred |

Issue #137 remains open and its Roadmap card In Progress until the deferred physical gates and integration/closure steps complete. PR #138 is draft and intentionally does not auto-close the issue.

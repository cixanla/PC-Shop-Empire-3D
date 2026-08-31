# Active-Power-On-Bound Deterministic Baseline POST Self-Test — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#127](https://github.com/cixanla/PC-Shop-Empire-3D/issues/127)<br>
**Draft PR:** [#128](https://github.com/cixanla/PC-Shop-Empire-3D/pull/128)<br>
**Parent branch head:** `ec84e84c42aae16d7b979b7e643e399a52a5a0b2` — Issue #125 docs checkpoint<br>
**Technical head:** `30ca892c4c3411b8771c10a39856089ecc5cd3f1`<br>
**Technical tree:** `eaf87358b42f96beb4f5b62d2bf65af78484d03b`<br>
**Technical branch:** `codex/issue127-active-power-on-post-self-test`<br>
**Current state:** Exact source, immutable active-power-on-bound baseline POST receipt, player input/presentation integration, targeted/full Mac tests, universal native build, Apple M1/Metal player smoke, bounded independent review, Repository Guard, codesign and settings preservation pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #127 and its Roadmap card remain `OPEN / In Progress`; PR #128 remains draft and no merge or closure is claimed.

## Delivered result

`PcPowerStateAuthority` now owns a separate immutable `PcPostStartupReceipt` history in addition to its existing safe power-state transition history. It does not create a second gameplay authority. A POST receipt binds:

- one non-empty stable POST operation ID;
- the exact owner `PcPowerStateAuthority`;
- the exact currently active `PcPowerStateReceipt` whose transition is `PowerOn`;
- the same exact current preflight receipt inherited from that power-on;
- the expected/current power-state revision;
- a separate monotonically increasing POST revision.

Success records one `Passed` receipt for that exact active cycle. Exact replay returns the same object instance. Reusing the operation with changed command lineage conflicts. A second distinct completion for the same active cycle is blocked. Foreign receipt, stale revision, Off state, invalid operation and history mismatch all fail closed.

Power-off clears only the active POST pointer. Historical lookup and exact replay remain immutable; current evaluation after power-off returns `NotCurrent`. A later power-on can create one new receipt with a new operation ID and next POST revision. Receipt-history validation enforces owner/source/preflight identity, exact power revision, monotonic ordering, operation mapping and at-most-one POST per source power-on.

## Player path and presentation

The existing `ElectricalPowerTestStationProjection` remains the only player command surface. Its accepted command sequence is:

```text
E / A: GÜÇ TESTİ ÖN KONTROLÜNÜ ÇALIŞTIR
ÖN KONTROL GEÇTİ • E / A: GÜCÜ AÇ
E / A: GÜCÜ KAPAT • POST GEÇTİ
GÜÇ AÇIK • POST GEÇTİ
FIRMWARE BEKLİYOR • BAKIM KİLİDİ AKTİF
```

After a successful player-triggered power-on, the station immediately requests the bounded POST receipt in the same consumed Interact path. At domain level, power-on and POST completion remain two explicit commands; a POST-less power cycle is valid and tested. If POST completion fails after power-on, Energized state remains visible and the next Interact still reaches explicit power-off, preventing a softlock.

Presentation reads are side-effect-free. They observe the separate POST revision and current receipt without creating authority or mutating state. Issue #125 maintenance blocking remains active while Energized. Inventory, BuildKit, reservation, custody, Assembly, three cable authorities, Economy and benchmark state remain unchanged by POST completion.

## Independent review

The final bounded read-only review inspected receipt ownership, replay/conflict ordering, skipped/repeated power cycles, revision lineage, foreign inputs, POST-failure power-off recovery, presentation side effects, session invariants and native-smoke null/error paths. It found no implementation P0/P1/P2 after one stale class-summary comment was corrected. A separate read-only documentation audit found no P0 implementation issue and identified two claim boundaries now captured here:

1. `POST GEÇTİ` means this deterministic baseline receipt passed; it is not real hardware POST code, firmware, BIOS/UEFI or electrical fault diagnosis.
2. The player path completes POST immediately after power-on, but the two domain commands are intentionally not atomically fused.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Final-source targeted POST authority contracts | `3/3` | `4.5113179 s` | `5,880` | `fd05ee11bb03deda5b292b7f251f7c6f727a63cd41aa92dc1466b9b5ec62625b` |
| Final-source full EditMode | `781/781` | `86.4685282 s` | `647,229` | `3d913be8fe6bcf89c8e991224ee7b2f9c12ec3da59abb1054956eb48660b5192` |
| Final-source full PlayMode | `164/164` | `689.9733003 s` | `556,130` | `2abbc32a70ff0744353c286d2b294254475ed39f36bea394a2918a827e82a408` |

Every accepted XML reports failed, skipped and inconclusive `0`.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Targeted EditMode | `47,273` | `2892c00c8dc3f1599df74ddbb765435bd1740f97c42cf3f6c044291a2ec60e3a` |
| Full EditMode | `35,409` | `4f62462117285ffacc842412b8fd4edf7c5f7c42fb9f746c7035f288d888086c` |
| Full PlayMode | `660,194` | `3c2c746786e2b12d70195959de6f98a4760a9e26c4fc692b48091d545d82d573` |

Earlier `r01` produced no XML and is not accepted evidence. Earlier targeted scene/input/hero and the first full EditMode run were diagnostic/pre-comment runs; the accepted full final-source suites supersede them.

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `601,930` | `f04c92e6b4559ba7308abd9ee9e498061a2c05d8434e0766333ee34f4f6a902b` |
| Universal app executable | `117,179` | `4e1ebbba08867a7fa592d7b6b1868747ab4bc74210f86247e2446c80de86a87e` |
| Apple M1/Metal r62 runtime log | `9,609` | `43f70ea6e3220e0ee884f258a749e0aed24d666abe457d5ab13254871027f286` |

Unity reports `330,548,985` build bytes. The app contains `302` files. Its executable is a universal Mach-O with `x86_64 + arm64`, and `codesign --verify --deep --strict` passes.

The graphical 1280×720 Apple M1/Metal player publishes the exact r62 readiness line and exactly one success marker:

```text
GARAGE_POWER_STATE_INTERLOCK_RUNTIME_SMOKE prerequisite-setup=assisted preflight=current power-on=player-triggered power-off=player-triggered input=keyboard+gamepad state=off cycles=1 maintenance-while-energized=blocked receipt=immutable replay=ok presentation=ok post=passed benchmark=untouched invariants=ok
```

The player exits `0`, Input System reaches `Shutdown`, failure/fatal marker count is `0` and final player/Unity/shader/IL2CPP residue is `0`.

## Settings, repository and raw evidence

The Issue #127 technical commit contains exactly `13` source, meta and test paths. It contains no ProjectSettings or Packages path. The only local tracked difference is the separately preserved user/editor-owned ProBuilder setting, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it remains unstaged and unreverted.

`ProjectSettings/ProjectSettings.asset` is SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` before and after build; no build-induced ProjectSettings restoration was necessary.

Repository Guard run [33364272612](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33364272612) passed on source `30ca892`. Draft PR #128 is open, clean and mergeable. Local `Tools/verify-repository.sh`, `git diff --check`, staged-scope audit, fatal-token audit, codesign and residue checks pass. Raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue127-post-startup-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task/firewall cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r62 smoke, fatal-token audit, evidence readback and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification, exact accepted source/evidence, collision-safe incoming path, complete SHA/size/path/Git comparison, atomic rename and second readback. Absence is not a pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #127 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Receipt binds exact current active power-on, preflight, owner and expected power-state revision. | PASS — domain/full suites |
| 2 | Separate POST revision is monotonic and at most one receipt exists per source power-on. | PASS — history/cycle contracts |
| 3 | Exact replay is same-instance; changed operation, duplicate cycle, foreign and stale inputs fail closed. | PASS — domain contracts |
| 4 | Power-off clears active POST state while preserving immutable historical lookup and replay. | PASS — domain, PlayMode and native smoke |
| 5 | Player path completes baseline POST immediately after accepted power-on without making power-off unreachable on failure. | PASS — input/softlock review and full PlayMode |
| 6 | Existing Workbench exposes POST-passed and firmware-waiting states without side effects or duplicate authority/geometry. | PASS — scene/presentation/full suites/native smoke |
| 7 | Inventory, Assembly, cable, Economy and benchmark authorities remain untouched; energized maintenance lock remains active. | PASS — domain and native boundary assertions |
| 8 | Targeted/full suites, universal Mac build and Apple M1/Metal native marker pass. | PASS — exact evidence above |
| 9 | Clean physical Windows IL2CPP/D3D11 runtime and USB immutable checkpoint/readback pass. | DEFERRED — devices unavailable |

Issue #127 remains open and its Roadmap card In Progress until physical Windows validation and later integration/closure steps complete. PR #128 is draft and intentionally does not auto-close the issue while acceptance #9 is deferred.

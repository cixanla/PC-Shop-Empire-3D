# Installed-OS-Bound Deterministic Fictional Driver Installation — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#133](https://github.com/cixanla/PC-Shop-Empire-3D/issues/133)<br>
**Draft PR:** [#134](https://github.com/cixanla/PC-Shop-Empire-3D/pull/134)<br>
**Parent branch head:** `f1bca88386b89309269418f56092cc1ff0a87dd1` — Issue #131 docs checkpoint<br>
**Technical head:** `b144a3ef1a0ac5fcbd9704c850426baa9a727044`<br>
**Technical tree:** `271bf53012e44e5162cdc5bdd2f41fa2cbbd3052`<br>
**Technical branch:** `codex/issue133-installed-os-bound-fictional-driver`<br>
**Current state:** Installed-OS-bound fictional driver authority, immutable receipt/history, exact pre-completion hardware/cable lineage gate, same-OS/storage persistence, deterministic two-step player path, explicit review/rejection presentation, P0 power-off recovery, full Mac tests, universal native build, Apple M1/Metal player smoke and Repository Guard pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #133 and its Roadmap card remain `OPEN / In Progress`; PR #134 remains draft and no merge or closure is claimed.

## Delivered result

`PcFictionalDriverInstallationAuthority` is a separate downstream authority bound to one exact `PcFictionalOsInstallationAuthority`, its exact `PcPowerStateAuthority` and exact `AssemblyBuildAuthority`. One successful immutable receipt binds:

- one non-empty stable driver installation operation ID;
- the exact owner authority chain;
- exact current installed fictional OS receipt and its storage identity;
- exact current completion-cycle firmware receipt, POST, power-on and accepted preflight lineage;
- exact physical M.2 storage item and product identity;
- completion snapshot's exact storage-secure operation and full Assembly revision;
- expected current power-state and driver revisions;
- bounded `WorkshopDriverBundle / InstalledForBenchmarkStage` result.

Completion validates immutable OS/firmware source lineage plus current electrical-readiness. The current snapshot must preserve every component item, component product, retain/secure operation, ATX24/EPS12V/PCIe-GPU cable item, route operation and cable revision captured by the source lineage. The completion firmware's storage-secure operation and full Assembly revision must exactly match the current snapshot. Product, retain-operation and cable-revision drift tests all fail closed before receipt creation and leave driver revision/count at zero.

Exact command replay returns the same object instance; changed reuse conflicts. A second distinct completion for the same storage or OS is blocked. Null, foreign, stale, historical, Off-cycle, missing/unsecured storage and malformed history fail closed.

After completion, installed evaluation intentionally binds to the exact current OS receipt and its exact storage item/product, not mutable non-storage hardware. Removing/changing the storage or OS makes the result non-current; keeping the same OS/storage preserves Installed even if other hardware changes later. Historical receipt and replay remain immutable.

Inventory, BuildKit, reservation, custody, Assembly, three cable ledgers, Economy, power, POST, firmware, OS and benchmark revisions remain untouched by driver completion. `EvaluateBenchmarkReadiness()` remains `BuildIncomplete`.

## Player path and presentation recovery

The existing Workbench/station presents this sequence without adding scene geometry or a new input action:

```text
E / A: GÜCÜ AÇ
LMB / RT: UEFI SETUP'I AÇ
LMB / RT: KAYDET VE ÇIK
LMB / RT: KURGUSAL OS KURULUMUNU AÇ
LMB / RT: OS KURULUMUNU BAŞLAT VE TAMAMLA
LMB / RT: DRIVER KURULUMUNU AÇ
LMB / RT: DRIVER KURULUMUNU BAŞLAT VE TAMAMLA
E / A: GÜCÜ KAPAT
KURGUSAL DRIVER KURULDU • DEPOLAMADA KALICI • SONRAKİ AŞAMA: BENCHMARK
```

The first driver Primary Action opens review; the second completes installation. Workbench has four presentation-only states: `Waiting`, `Reviewing`, `Installed`, `Rejected`. A valid-history completion rejection exposes the exact failure code and creates no authority/receipt; a fresh Primary Action reopens review. Workbench never calls `EnsureFictionalDriverInstallationAuthority`.

Motor pause, raw Pause edge, range/focus/LOS loss, busy hands and a competing world-interact owner invalidate an open review without consuming the Primary edge. Tests cover pause and competing-owner both before and after review opens. Returning to the station requires a fresh review press. Keyboard/mouse and virtual gamepad paths use the same authority command.

Interact has strict priority. Same-frame power-off plus Primary Action performs exactly one power-off, consumes both accepted edges and emits no driver receipt. A deliberately malformed driver history still leaves the normal `GÜCÜ KAPAT` prompt and player Interact route available, so downstream history cannot energized-softlock the machine.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Driver lineage/domain contracts | `5/5` | `10.7862592 s` | `7,419` | `4ab621be3fbce97d2a9c6e52eb0a2a3d67628962518b64827744657f0b130edd` |
| Driver keyboard/mouse/gamepad/context/P0 input | `6/6` | `33.3822469 s` | `23,693` | `3be47d5f68a4c937542fc53d520c200973dfe86f623bf6ae3636f9443a8d467d` |
| Existing scene/r65 wiring contract | `1/1` | `0.6569787 s` | `4,298` | `4c9cf14f1fbeafe741afb60020ad4fa0ef489518bd7ace4c368074fab05da50e` |
| Power/preflight/POST/UEFI/OS/driver regression class | `23/23` | `112.8344253 s` | `81,972` | `b1a0d8af0c552f0e11157d04c00812d40371fbd7940a26b3d7f2cad80cb88d43` |
| Full EditMode | `793/793` | `101.1487708 s` | `656,471` | `569553f6b1181987335eb20350284b637d4696f44bf5adf5716c1170ffcac4be` |
| Full PlayMode | `181/181` | `902.1326592 s` | `632,237` | `e2b0b1ae39003b694e89fabb6451746c758ba7d46775df5cefdac8d91f74ba47` |

Every accepted XML reports failed, skipped and inconclusive `0`. Diagnostic r10's test-only missing namespace compile failure, r11's over-specific expected failure code and the pre-fix terminated r09 full run are not accepted evidence. The unique-path r12–r17 results above supersede them. Test Runner temporary `InitTestScene` files were absent after accepted completion.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Driver lineage/domain EditMode | `39,178` | `80c2174d27c56aa779588ab31e21a95f319cea4dd8ae91be3f61d82f66863ea6` |
| Driver input/context/P0 PlayMode | `61,315` | `c2e0fb06532c21b11902377b38819c32a83a6c43a775b134783436550def7f22` |
| Scene contract | `32,644` | `77617bcc55eb2c49e16d1fde8e451c93dd864e719ecb6b946fa830097a8b3a9d` |
| Power/POST/UEFI/OS/driver regression | `127,296` | `163796849247690e3bdbf8e8001fec7a6a07ea310cf707ddee30bb27de44229a` |
| Full EditMode | `35,133` | `2e4881fef0cc3813089c77b527c1e45b36da33c5594cadff0945cb38034ecad8` |
| Full PlayMode | `743,812` | `67f8299a609d21afeda348c05dfdaa7c6c05317c3520c6330a479f60d5ac9943` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `603,411` | `f80011bfd9260e95120825d61700f237b2a8a671f6f4578f3a6469fa98b23d2d` |
| Universal app executable | `117,179` | `c347fd358af6c1afe8e5d89699995ebaf81a4e9c65b4ff0cc9ac3a9f79ad2ad7` |
| Apple M1/Metal r65 runtime log | `9,676` | `8a7efe7dde5b076762af57066ccafe6c4e5c7f8dd1605386b9a1131bd710631d` |

Unity reports `330,641,904` build bytes. The app contains `302` files. Its executable is a universal Mach-O with `x86_64 + arm64`; `codesign --verify --deep --strict` reports valid on disk and satisfies the designated requirement.

The graphical 1280×720 Apple M1/Metal player publishes the exact r65 readiness line and exactly one success marker:

```text
GARAGE_FICTIONAL_DRIVER_INSTALLATION_RUNTIME_SMOKE prerequisite-setup=assisted preflight=current power-on=player-triggered post=passed firmware=optimized-defaults-saved os=workshop-standard-installed driver=workshop-driver-bundle-installed storage=identity-bound review=player-triggered install=player-triggered input=keyboard+mouse+gamepad power-off=player-triggered state=off persistence=power-off-preserved receipt=immutable replay=ok benchmark=untouched invariants=ok
```

Readiness count is `1`, success count `1`, failure/fatal count `0` and Input System shutdown count `1`. The player exits `0`; final player/Unity residue is `0`.

## Repository, settings and raw evidence

Technical commit `b144a3ef1a0ac5fcbd9704c850426baa9a727044` contains exactly `20` source, meta and test paths with `3,042` insertions and `28` deletions. It contains no ProjectSettings or Packages path. The separately preserved user/editor-owned ProBuilder setting remains the only unrelated tracked difference, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is unstaged and unreverted.

`ProjectSettings/ProjectSettings.asset` is SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` after tests/build/smoke and unchanged from the accepted baseline. No ProjectSettings edit or build-induced restoration was needed.

Repository Guard run [33378476265](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33378476265) passed on source `b144a3e`. Draft PR #134 is clean and mergeable. Local `Tools/verify-repository.sh`, `git diff --check`, staged-scope audit, fatal-token audit, codesign and residue checks pass. A bounded final static audit reports no P0/P1. Raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue133-fictional-driver-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r65 smoke, fatal-token audit and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification, exact accepted source/evidence, collision-safe incoming path, complete SHA/size/path/Git comparison, atomic rename and second readback. Absence is not a pass.

Automated keyboard/mouse and Input System virtual-gamepad paths pass, but the claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #133 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1–2 | Separate exact-owner authority; completion requires exact current installed OS, current firmware cycle, revisions and secured M.2 identity. | PASS — domain/full suites |
| 3–5 | Immutable OS/firmware/storage/hardware/cable lineage and replay/conflict/duplicate rules; completion-time drift fails closed. | PASS — drift/domain/full suites/native smoke |
| 6 | Installed result persists for the same current OS/storage after power-off and later non-storage hardware changes. | PASS — direct domain regression/native persistence |
| 7 | Existing station/input/scene geometry reused without a second interaction surface. | PASS — scene contract/static audit |
| 8–9 | Two-step review/install, strict power-off priority and all context losses preserve input. | PASS — targeted input and full PlayMode |
| 10 | Malformed driver history cannot block explicit power-off. | PASS — direct P0 player test/native power-off |
| 11 | Gameplay/benchmark authorities remain untouched; benchmark remains incomplete. | PASS — domain/input/full suites/native smoke |
| 12 | Workbench distinguishes Waiting/Reviewing/Installed/Rejected without creating authority. | PASS — targeted input/static audit |
| 13 | Targeted/full Mac, universal build, native smoke and Repository Guard pass. | PASS — exact evidence above |
| 14 | Clean physical Windows IL2CPP/D3D11 and physical USB checkpoint/readback pass. | DEFERRED — devices unavailable |

Issue #133 remains open and its Roadmap card In Progress until physical Windows validation and later integration/closure steps complete. PR #134 is draft and intentionally does not auto-close the issue while acceptance #14 is deferred.

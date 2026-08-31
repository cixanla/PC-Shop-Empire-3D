# Exact Electrical Readiness and Workbench Feedback — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#119](https://github.com/cixanla/PC-Shop-Empire-3D/issues/119)<br>
**Draft PR:** [#120](https://github.com/cixanla/PC-Shop-Empire-3D/pull/120)<br>
**Base main:** `e5a4f6b110f1eaaa9e6e5eb22dd5314877a100f4`<br>
**Parent branch head:** `f26e95ce288e2e7f17d0aa642f59ee9fa0fffef5` — Issue #115 docs checkpoint<br>
**Technical head:** `f33a052d3f3ef25d48ff8b5d5f4d4a149f414fdc`<br>
**Technical tree:** `986ff174209dc55bb98cf7f1151fc8cc480384fc`<br>
**Technical branch:** `codex/issue119-electrical-readiness-feedback`<br>
**Current state:** Exact source, scene, authority, targeted/full Mac tests, universal native build, Apple M1/Metal route/readiness and Assembly-readability smokes, screenshots, repository guard and diff checks pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because those devices are unavailable. Issue #119 and its Roadmap card remain `OPEN / In Progress`; PR #120 remains draft and no merge or closure is claimed.

## Delivered result

`AssemblyBuildAuthority.EvaluateElectricalReadiness()` is a pure decision over the existing canonical Assembly state. It checks, in fixed order:

1. secured motherboard,
2. retained processor,
3. retained A2 DDR5,
4. secured primary M.2,
5. retained cooler with consumed TIM,
6. retained graphics card,
7. retained power supply,
8. routed ATX24,
9. routed EPS12V,
10. routed PCIe/GPU 6+2 cable.

Unsupported topology and invariant/lineage drift fail closed. A successful immutable snapshot binds exact build/chassis identity, ten item IDs, ten source operation IDs, the Assembly revision and three cable revisions. Re-evaluation changes no Inventory, Assembly, BuildKit, reservation, custody, receipt or replay state. Existing benchmark readiness remains blocked and no power-on/POST/BIOS/OS/driver/benchmark state is emitted.

The single `ElectricalReadinessWorkbenchProjection` reads only an already initialized `GarageStockFlowSession`. It does not initialize the session and owns no gameplay mutation. Its cached Inventory/Assembly/cable revisions avoid a full invariant audit and snapshot allocation every frame. The scene adds one TextMesh renderer and one indicator renderer on `Ignore Raycast`; light, camera, collider, NavMesh obstacle, waypoint, item and input-owner deltas are all zero.

Exact ready presentation:

```text
ELEKTRİK HAZIR
10/10 PARÇA • 3/3 KABLO
GÜÇ TESTİ BEKLİYOR
```

The failure presentation shows the first deterministic blocker and `GÜÇ HAZIR DEĞİL`. The final native route smoke proves `PCIe missing → ready after exact route → PCIe missing after exact unroute` on the same authority.

## Scene and presentation budget

| Contract | Issue #115 r57 | Issue #119 r58 | Delta |
|---|---:|---:|---:|
| Authored scene MeshRenderer components | `483` | `485` | `+2` |
| Authored lights / cameras | `5 / 1` | `5 / 1` | `0 / 0` |
| Retail runtime total / active renderers | `486 / 462` | `488 / 464` | `+2 / +2` |
| Assembly runtime total / smoke-active renderers | `477 / 468` | `479 / 470` | `+2 / +2` |
| Electrical status renderers | `0` | `2` | `+2` |
| Electrical status colliders / lights / input owners | `0 / 0 / 0` | `0 / 0 / 0` | `0 / 0 / 0` |

Scene generation attempt r06 emits:

```text
GARAGE_GRAYBOX_BUILD_OK scene=Assets/Scenes/Prototypes/GarageGraybox.unity version=garage-electrical-readiness-r58-v1
```

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Committed scene electrical contract | `1/1` | `0.6591523 s` | `4,298` | `8628740684fc5c2b5d7ff5bd26def8efe284e61493790d84001fa0c9e2a508b8` |
| Fail-closed/read-only projection guards | `3/3` | `2.83397 s` | `8,137` | `0a6f1cabbdc4ca758af0c07e86e7aa97d8d2fadb1d837e9fbdd3a38213f43a55` |
| Keyboard/mouse plus virtual-gamepad final wording | `2/2` | `24.1083408 s` | `9,712` | `b56893993f09a3ca97a522f2f0cca8ebf7e1a679828374d9e527a9974eacefd1` |
| Assembly readability regression contract | `1/1` | `0.9356758 s` | `6,462` | `e0668b9194e036c87d5a41ffa79513280ddca1952382233a9216094b3c347408` |
| Full EditMode release-final | `758/758` | `30.5949662 s` | `628,918` | `3d64b4f9c9ab36fe6a6a64d7715070ce99759ce631868f7cb277e57dd2a6cd8e` |
| Full PlayMode release-final | `158/158` | `722.2649397 s` | `529,514` | `71018019e9f78e845686bc03f3936ea86890c47ad70a71f5590fadf2ea0868b7` |

Every accepted XML reports failed, skipped and inconclusive `0`. Targeted domain, exact snapshot, scene, presentation compile and keyboard/gamepad attempts r02–r10 also passed. The r12 and r26 failures are retained only as diagnostic evidence: adding two deliberate status renderers exposed stale retail/Assembly renderer budgets. Both contracts were corrected to exact r58 counts before the final targeted and full reruns.

Accepted full-suite Unity logs:

| Log | Bytes | SHA-256 |
|---|---:|---|
| Full EditMode r28 | `35,456` | `0fbc4133e77ac00e7b07efdf4886be65bc8733c6488a96c2e8fe01be4d970c89` |
| Full PlayMode r29 | `631,721` | `63cdae39255f639b7608a0ad9a1dc5001073130b091463a1e2e104760d50ba9d` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `596,119` | `2dec50248f1ed83f45fbd0f2f4fa462add27f9b518531c6b6bb682791a7dee6f` |
| Universal app executable | `117,179` | `4a903a2b3c4ef6f9c283a603d74c891d3cb70f402e3c69d1a15ec9750c093b85` |
| Apple M1/Metal PCIe/readiness runtime log | `9,534` | `ef777b37ec1bc95402f184fed8ae51aa4eea71bcea19cfe5115d11c1195fbe84` |
| Apple M1/Metal Assembly-readability runtime log | `11,643` | `8816104eb5d592767e3285e4594912f96489c3c8df291ab1e1886332e5183109` |

The build marker reports `330,441,141` bytes. The app contains `302` files; its executable is universal Mach-O `x86_64 + arm64`, and `codesign --verify --deep --strict` passes.

The native PCIe/readiness player runs graphically at 1280×720 on Apple M1/Metal and emits exactly one readiness marker plus exactly one success marker:

```text
GARAGE_PCIE_GPU_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE ... electrical-readiness=ready-then-blocked monitor=ok no-duplicate-loss=ok invariants=ok
```

The Assembly readability regression emits:

```text
GARAGE_ASSEMBLY_WORKBENCH_HERO_READABILITY_RUNTIME_SMOKE states=loose+preview+routed hero=ready ... total-renderers=479 lights=4 cameras=1 screenshots=3 ... human=false active-renderers=470 max-central-glare-pixels=0
```

| Screenshot | Bytes | SHA-256 |
|---|---:|---|
| `assembly-workbench-hero-loose-r55.png` | `704,298` | `4710d5bcf8b24ec12a528af13691ac44ceb15d1551dc790f34fc227f29a246cc` |
| `assembly-workbench-hero-preview-r55.png` | `715,142` | `4ba3e2e4e3e594f93010dc49522139a0c4c876b3a3c98bd4a8596c16d9645595` |
| `assembly-workbench-hero-routed-r55.png` | `728,308` | `1c14403a0a771f055d9698a05dae5ed50d58809460a1a329c1f68a5851548d2f` |

All three captures are byte-distinct 1280×720 images and were visually inspected. Loose, preview and routed workbench states remain distinguishable; central saturated-white glare is `0`. The lookdev smoke intentionally suppresses world text while capturing composition; the separate PCIe/readiness smoke validates the exact live electrical status text and reversible transition.

Both native players exit `0`, reach graceful Input System shutdown and leave Unity/player/shader/IL2CPP residue `0`.

## Settings, repository and raw evidence

All tracked ProjectSettings hashes are byte-exact across the accepted r30 build. `ProjectSettings/ProjectSettings.asset` remains SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`. The separately preserved user/editor-owned ProBuilder setting is also byte-exact across build execution at SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is outside the Issue #119 commit and remains unstaged.

The staged source-and-docs repository guard reports `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=1158`. `git diff --check` and staged diff checks pass. Canonical raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue119-electrical-readiness-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, screenshot, process/task/firewall cleanup or evidence readback claim is made for Issue #119. UTM was deliberately not started because it cannot prove the required physical Intel Iris Xe/D3D11 lane. Closure requires a clean checkout of the exact accepted commit/tree, full EditMode/PlayMode, native readiness and Assembly smokes, fatal-token audit, readback and zero final residue.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity and health verification, exact accepted source/evidence and two complete readbacks. Absence is not treated as pass.

Automated keyboard/mouse and Input System virtual-gamepad tests pass, but the current claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #119 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Deterministic exact ten-part electrical-readiness decision; invalid state fails closed. | PASS — domain, scene and full Mac suites |
| 2 | Immutable exact snapshot; re-evaluation read-only and idempotent. | PASS — targeted guards and full EditMode |
| 3 | One presentation-only workbench projection; immediate reversible blocker feedback. | PASS — scene, keyboard/gamepad and native route smoke |
| 4 | Benchmark/power-on/POST/OS/customer completion remains blocked. | PASS — authority and regression contracts |
| 5 | Keyboard/mouse, virtual gamepad, scene, targeted and full suites; zero fail/skip/inconclusive. | PASS — Mac accepted suites |
| 6 | Universal Mac native plus clean physical Windows x64 IL2CPP/D3D11 runtime; ProjectSettings exact. | PARTIAL — Mac and settings PASS; physical Windows DEFERRED |

Issue #119 must remain open and its Roadmap card In Progress until the exact physical Windows gate, docs/PR/CI integration and any chosen USB checkpoint policy are resolved. PR #120 is draft and intentionally does not auto-close the issue while acceptance #6 is partial.

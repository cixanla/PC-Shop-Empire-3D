# Exact System Power Budget and PSU Headroom — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#121](https://github.com/cixanla/PC-Shop-Empire-3D/issues/121)<br>
**Draft PR:** [#122](https://github.com/cixanla/PC-Shop-Empire-3D/pull/122)<br>
**Parent branch head:** `8f0671fd04016b1115e622d963e9d28e7ef08c19` — Issue #119 docs checkpoint<br>
**Technical head:** `57e6b54883ef6756c5522d1de9c17479e7cda481`<br>
**Technical tree:** `8652882bb5e791c969b9c8648cfe7e242a5a92d7`<br>
**Technical branch:** `codex/issue121-power-budget-headroom-preflight`<br>
**Current state:** Exact source, immutable catalog/policy contracts, targeted/full Mac tests, universal native build, Apple M1/Metal route/power-budget and Assembly-readability smokes, screenshots, repository guard, codesign and settings checks pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because those devices are unavailable. Issue #121 and its Roadmap card remain `OPEN / In Progress`; PR #122 remains draft and no merge or closure is claimed.

## Delivered result

The authoritative mechanical component catalog remains unchanged. A separate immutable `PcElectricalCatalog` is bound to the exact same `PcComponentCatalog` instance and accepts only exact product IDs with matching component kinds. The catalog contains five continuous-load profiles and one PSU rated-output profile for the prototype; null, empty, duplicate, foreign, unsupported and mismatched metadata fails closed.

The versioned legacy-v1 policy is deterministic and integer-only:

```text
system draw = 35 W platform + 4 W chassis + 125 W CPU + 200 W GPU
            + 6 W DDR5 + 5 W NVMe + 5 W cooler
            = 380 W

recommended PSU = ceil((380 W × 130 / 100) / 50 W) × 50 W
                = 500 W

installed PSU = 550 W
capacity margin = +50 W
```

Every `AssessPowerBudget()` call first re-evaluates canonical exact electrical readiness. It resolves current installed product IDs directly from the Assembly authority, checks expected kinds and returns an immutable snapshot bound to the exact readiness lineage and policy ID. It does not mutate Inventory, Assembly, BuildKit, reservation, custody, receipts, replay or benchmark state.

An undersized PSU is represented as a valid blocked assessment, preserving the exact draw, recommendation, installed rating and negative margin. Missing or mismatched profiles, foreign catalogs, invalid policy or arithmetic bounds fail closed as operation failures.

Exact ready presentation:

```text
GÜÇ BÜTÇESİ UYGUN
380W / EN AZ 500W / PSU 550W
GÜÇ TESTİ BEKLİYOR
```

The projection remains presentation-only and refreshes only when existing Inventory, Assembly or cable revisions change. No input, collider, renderer, light, camera, NavMesh, receipt or power state was added. Power-on, POST, BIOS/UEFI, OS, driver, benchmark, fault/damage and customer completion remain outside this slice.

## Exact Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Electrical catalog contracts | `4/4` | `0.0398679 s` | `6,582` | `31f96eebc484eb7b80ac2e1fdd4bd2b0dd38751344c8a1b31c07f439f67c7c5e` |
| Power-budget authority/policy contracts | `6/6` | `4.1855152 s` | `8,162` | `c3dc17cb314e527b09a24bfa71efddb63ad9299e091ea1d7e6ffc89df2c4f11f` |
| Committed workbench scene contract | `1/1` | `0.6969814 s` | `4,298` | `50d73b3ebb509da1fbfde647cf253fea0a4e5fdbd29ff99a15b6bc996a2e7111` |
| Keyboard/mouse plus virtual-gamepad route/budget cycle | `2/2` | `20.3555915 s` | `9,678` | `7c020a3499c61e127e884baac06b4a734f053c9b9b0f7ed0571971c7aa2f6268` |
| Assembly plus retail hero regressions | `2/2` | `1.0825228 s` | `10,269` | `964410f3fd4a9b1a69a6f94f7e6faa62efe68f546bbea5d0f8c8517b5739be30` |
| Full EditMode | `768/768` | `51.3103074 s` | `637,099` | `1434f1849d8ee08dbd3c595c7fed0e8e1645de83b4687fe76403e244889253b5` |
| Full PlayMode | `158/158` | `637.9719241 s` | `526,983` | `a87fc42e961bdb141b1e01c049c161f3cbfbab38b5f3961ad5c37fbda7596e38` |

Every XML reports failed, skipped and inconclusive `0`. The full-suite logs are:

A separate bounded, read-only review of exact commit `57e6b54883ef6756c5522d1de9c17479e7cda481` found no P0, P1 or P2 defect. The review covered exact product/catalog binding, immutable metadata, integer arithmetic and overflow bounds, readiness-first fail-closed behavior, insufficient-PSU semantics, authority no-mutation, stale/revision lineage and the presentation/input/collider/renderer boundary. It made no file, Git, GitHub, test, build, ProjectSettings or device change.

| Log | Bytes | SHA-256 |
|---|---:|---|
| Full EditMode | `35,393` | `e5d488224e62aac8bc157059f535aaa427903cd5ab7e16a70196de3cc07a5bbb` |
| Full PlayMode | `629,004` | `ec69dba5272bbe68d424f9cadaae33d7b5c10b27add59642c52573773ab512ed` |

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `614,478` | `5c2f1cebfb8aa4cba8dbfd0d1b6c5a120c4ef765d94d5a24223028ab8d4941d9` |
| Universal app executable | `117,179` | `ed6ede7c7cdb48c359df33cad4bbfd228489271abb972a898f88e39a4ef70798` |
| Apple M1/Metal PCIe/power-budget runtime log | `9,568` | `f225f356078dc426ff1c3b14f9a70d17b92f8640f4eb20b6e91a1887fd129fec` |
| Apple M1/Metal Assembly-readability runtime log | `11,630` | `9f7f2c5e804aa1ededca1cac869b8b4733634695f3086c3f6ca56fae6f3c198a` |

The build marker reports `330,465,045` bytes. The app contains `302` files; its executable is universal Mach-O `x86_64 + arm64`, and `codesign --verify --deep --strict` passes.

The native PCIe/readiness player runs graphically at 1280x720 on Apple M1/Metal and emits exactly one readiness marker and one success marker:

```text
GARAGE_PCIE_GPU_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE ... electrical-readiness=ready-then-blocked power-budget=380/500/550 monitor=ok no-duplicate-loss=ok invariants=ok
```

The Assembly readability regression emits:

```text
GARAGE_ASSEMBLY_WORKBENCH_HERO_READABILITY_RUNTIME_SMOKE states=loose+preview+routed hero=ready ... total-renderers=479 lights=4 cameras=1 screenshots=3 ... human=false active-renderers=470 max-central-glare-pixels=0
```

| Screenshot | Bytes | SHA-256 |
|---|---:|---|
| `assembly-workbench-hero-loose-r55.png` | `706,308` | `37f56cddbb6dea91afd86aaf4f00ee0890ff2e753c0e98bea00820d8ea04e855` |
| `assembly-workbench-hero-preview-r55.png` | `714,633` | `059d188f26c2b7155e646b4736d6ad6287eeee34c0d022afac4ea8b5f4b26cd3` |
| `assembly-workbench-hero-routed-r55.png` | `728,662` | `88850a30e47470c802bd91d3945a91c36e2cb9481b7140f0066bd4bfe49916a9` |

All three captures are byte-distinct 1280x720 images and were visually inspected. Loose, preview and routed states remain distinguishable; central saturated-white glare is `0`. The lookdev capture intentionally suppresses world text; the separate PCIe/power-budget smoke validates exact live workbench values.

Both native players exit `0`, reach graceful Input System shutdown and leave player/Unity/shader/IL2CPP residue `0`.

## Settings, repository and raw evidence

The complete `ProjectSettings + Packages` before/after manifest is byte-exact; each manifest is `2,919` bytes with SHA-256 `a3d3a2eaed31f7a86a2d3ffa02066458b1a0c677d0015a907ed228132433bec9`. The separately preserved user/editor-owned ProBuilder setting remains SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is outside the Issue #121 commit and remains unstaged.

The source commit repository guard reports `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=1170`. `git diff --check` and staged diff checks pass. Canonical raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue121-power-budget-2026-08-31`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, screenshot, process/task/firewall cleanup or evidence readback claim is made for Issue #121. UTM was deliberately not used as replacement evidence. Closure requires a clean checkout of the exact accepted commit/tree, full EditMode/PlayMode, native route/power-budget and Assembly smokes, fatal-token audit, readback and zero final residue.

No USB device is connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity and health verification, exact accepted source/evidence and two complete readbacks. Absence is not treated as pass.

Automated keyboard/mouse and Input System virtual-gamepad tests pass, but the current claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #121 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact immutable electrical metadata bound to the canonical component catalog; malformed/foreign state fails closed. | PASS — catalog and full EditMode |
| 2 | Deterministic legacy-v1 `380 W → 500 W`, installed `550 W`, margin `+50 W`; threshold/undersized cases exact. | PASS — policy/authority tests |
| 3 | Every assessment revalidates exact electrical readiness and mutates no gameplay authority. | PASS — authority and input-cycle regressions |
| 4 | One presentation-only workbench view; no new input/collider/renderer/light/camera/authority. | PASS — scene, hero and native contracts |
| 5 | Power-on/POST/fault/BIOS/OS/benchmark/customer completion remains absent. | PASS — source and regression contracts |
| 6 | Universal Mac native plus clean physical Windows x64 IL2CPP/D3D11 runtime; settings exact. | PARTIAL — Mac and settings PASS; physical Windows DEFERRED |

Issue #121 remains open and its Roadmap card In Progress until the exact physical Windows gate, docs/PR/CI integration and any chosen USB checkpoint policy are resolved. PR #122 is draft and intentionally does not auto-close the issue while acceptance #6 is partial.

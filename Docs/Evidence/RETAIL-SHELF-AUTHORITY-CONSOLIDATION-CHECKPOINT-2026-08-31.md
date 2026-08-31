# Retail Shelf Authority Consolidation — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#115](https://github.com/cixanla/PC-Shop-Empire-3D/issues/115)<br>
**Draft PR:** [#118](https://github.com/cixanla/PC-Shop-Empire-3D/pull/118)<br>
**Base main:** `e5a4f6b110f1eaaa9e6e5eb22dd5314877a100f4`<br>
**Technical head:** `96d72d5202cdb72b1c017ce5e063948c892ce88d`<br>
**Technical tree:** `fb8821c2dc84d887e5ef9c1940d2bef255258d3c`<br>
**Technical branch:** `codex/issue115-retail-shelf-authority-consolidation`<br>
**Current state:** Mac source, exact scene/authority contracts, targeted and full regression suites, universal native build, Apple M1/Metal retail and Assembly smokes, visual inspection, repository guard and diff checks pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because those devices are unavailable. Issue #115 and its Roadmap card remain `OPEN / In Progress`; PR #118 remains draft and no merge or closure is claimed.

## Delivered result

GarageGraybox `garage-retail-shelf-authority-r57-v1` removes the pre-existing legacy `StarterShelf` hierarchy: `17` child objects, `16` mesh renderers and `10` colliders. The scene now contains exactly one retail placement/inventory authority:

- one `AuthoritativeRetailShelfA`,
- exactly `5` child colliders,
- one `PlacementSurface` with ID `prototype.retail-shelf-a`,
- one `InventoryPlacementZone` bound to `GarageStockFlowSession.ShelfContainerIdValue`, kind `Shelf`, and the same placement surface.

There is no second `StarterShelf`, shelf placement surface or shelf inventory zone. The accepted r56 customer approach, real shelf offer/reserved basket, checkout/payment/receipt and fulfilled-exit states still use their existing authorities. Customer NavMesh route failure/fallback remains zero. Presentation geometry owns no gameplay state.

## Exact runtime markers and deterministic budget

```text
GARAGE_RETAIL_CHECKOUT_HERO_READABILITY_RUNTIME_SMOKE states=customer-approach+shelf-offer-basket+checkout-payment-receipt hero=ready materials=dark-metal+brushed-steel+rubber+safety-accent+label-paper light=focused shelf-authority=single legacy-starter-shelf=absent total-renderers=486 lights=5 cameras=1 screenshots=3 glare=bounded glare-pixels<=256 contrast=bounded contrast-ratio>=1.25 ui=hud-suppressed world-text=preserved human=false active-renderers=462 max-glare-pixels=0 min-contrast-ratio=1.348 capture-directory=<platform-evidence>/captures
```

```text
GARAGE_ASSEMBLY_WORKBENCH_HERO_READABILITY_RUNTIME_SMOKE states=loose+preview+routed hero=ready materials=wood+rubber+dark-metal+brushed-steel+concrete+pcb+safety-accent+connector-polymer+psu-intake+gpu-hardware connector-glare=bounded light=focused total-renderers=477 lights=4 cameras=1 screenshots=3 glare-pixels<=64 ui=lookdev-suppressed human=false active-renderers=468 max-central-glare-pixels=0 capture-directory=<platform-evidence>/captures
```

| Contract | r56 accepted scene/runtime | r57 technical head | Delta |
|---|---:|---:|---:|
| Authored scene `MeshRenderer` components | `499` | `483` | `-16` |
| Authored lights | `5` | `5` | `0` |
| Authored cameras | `1` | `1` | `0` |
| Retail runtime total renderers | `502` | `486` | `-16` |
| Retail runtime smoke-active renderers | `478` | `462` | `-16` |
| Assembly runtime total renderers | `493` | `477` | `-16` |
| Assembly runtime smoke-active renderers | `484` | `468` | `-16` |
| Legacy shelf colliders | `10` | `0` | `-10` |
| Authoritative shelf colliders | `5` | `5` | `0` |
| Shelf placement surfaces / inventory zones | legacy plus authoritative | `1 / 1` | single authority |

## Exact source and Mac tests

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Committed scene contracts | `11/11` | `1.0411932 s` | `11,887` | `13c27d68ec49c218789a4b8e7a009de01a932178b30e05e0d3c26e5d85d74882` |
| Retail authority/readability PlayMode | `1/1` | `0.8436642 s` | `6,407` | `95e36ac0608dc1025a264049bcb6b98ed5925b5c778837038a5bd1daed3962c8` |
| Assembly regression PlayMode | `1/1` | `0.8407857 s` | `6,426` | `a320a716df6285795f143eafb0f069abbafe3cc0410b3f7532e32a9eb0017fd6` |
| Keyboard/mouse plus virtual-gamepad retail flows | `2/2` | `15.0031665 s` | `9,556` | `4e8df24264fb45932d5dba8f6d227601d7f1d374454f948118187dd09fdaef94` |
| Technical baseline after evidence-driven correction | `5/5` | `0.0516154 s` | `6,332` | `8f8f4e90c8b839f9c1e04e7c3cc5b0cac98aba720c0f36cbf66c23885c8a9317` |
| Full EditMode accepted rerun | `754/754` | `28.8905693 s` | `625,807` | `7c79919b8092a61796c0f433e6d0ac0032a7dd1e08a06d9884840b2878a79c5c` |
| Full PlayMode | `158/158` | `686.5190062 s` | `523,977` | `55d60ce8800cb2a39d701d327ae89459b4b87a3dea471a923446c408599fbae7` |

Every accepted result has failed, skipped and inconclusive `0`. The first full EditMode attempt (`753/754`) is retained only as diagnostic evidence: it exposed a pre-existing `VersionControlSettings.asset` mode of `Unity Version Control` while the repository contract requires `Visible Meta Files`. That single evidence-backed value was returned to repository baseline; the targeted baseline and full rerun then passed. The separate pre-existing user/editor-owned ProBuilder setting remained untouched and unstaged.

Scene-builder attempts r01 and r02 are also diagnostic only; they exposed compile-time parameter/namespace mistakes while integrating the bounded scene edit. r03 is the accepted generation attempt and emits:

```text
GARAGE_GRAYBOX_BUILD_OK scene=Assets/Scenes/Prototypes/GarageGraybox.unity version=garage-retail-shelf-authority-r57-v1
```

`ProjectSettings/ProjectSettings.asset` remained byte-exact at SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244`. The technical-source pre-docs Repository Guard reports `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=1145`; the staged closure-docs rerun reports the same contract with `tracked=1146`. `git diff --check` passes and accepted build logs contain no C# compiler warning/error token.

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `597,480` | `38036e9571f5d068be2709896bebee848423d8e0c3e6a46c512158be39453401` |
| Apple M1/Metal retail r57 runtime log | `19,984` | `d13cb0aea3f4c45456bc7ea00a62779c35f817f95f77e132bf8bf7f3780ceb35` |
| Apple M1/Metal Assembly regression log | `11,583` | `abf5a1cc5c2b4496606e08a839a5146869b9a869acf7606e410a36b3d2435947` |
| Universal app executable | `117,179` | `3dddce859bb357ef420719891be12ba624be6967aea79765cecd82fa7b7c1ae9` |

The build marker reports `330,421,709` bytes. The app contains `302` files; its executable is universal Mach-O `x86_64 + arm64`, and `codesign --verify --deep --strict` passes. Both native players exited cleanly with no Unity/player/shader residue.

| Retail screenshot | Bytes | SHA-256 | Glare pixels | Contrast ratio |
|---|---:|---|---:|---:|
| `retail-customer-approach-r57.png` | `607,839` | `deeb1a79d1114ea41cd9e1a5f97708f8b5faddb2c2278a7c891557716bf27dcd` | `0` | `1.348` |
| `retail-shelf-offer-basket-r57.png` | `617,928` | `2eb579c22f59db6d0de33b82d9798dde1605f22645f43a4ffa956f6432c166a3` | `0` | `1.818` |
| `retail-checkout-payment-receipt-r57.png` | `692,602` | `f828c65eaa9462a8a1232d8ea829f8fa40a76558e5cfc2a67e0d5d1984b0670e` | `0` | `2.185` |

All captures are `1280x720`, byte-distinct and visually inspected. No duplicate shelf or physical overlap is visible. The Assembly r55 regression also produced three byte-distinct `1280x720` captures with central glare `0`.

Canonical raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue115-retail-shelf-authority-2026-08-31`.

## Deferred physical gates

The physical Windows machine was unavailable. No Windows test, build, D3D11 runtime, screenshot, cleanup or readback claim is made for Issue #115. UTM was deliberately not started because it cannot prove the required Intel Iris Xe/Direct3D11 release lane and all Mac gates already passed. When the physical machine returns, validation must use a clean exact-head checkout, x64 IL2CPP with Direct3D11 only, full EditMode/PlayMode, native retail and Assembly smoke, screenshot/readback and zero final residue.

No USB device was connected. No immutable checkpoint or readback was attempted. A future USB write requires fresh device identity/health verification and must contain the exact accepted checkpoint; absence is not treated as pass.

Automated keyboard/mouse and Input System virtual-gamepad tests pass, but the current claim remains `human=false`. Physical keyboard/gamepad/endurance certification stays a Steam 1.0 release gate.

## Issue #115 acceptance matrix

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact one retail placement/inventory authority and collision-free collider volume. | PASS — scene, EditMode, PlayMode and native marker |
| 2 | Route failure/fallback zero; approach/browse/checkout/fulfilled exit preserved. | PASS — automated Mac contracts and native smoke |
| 3 | Retail/checkout, simultaneous WASD+mouse, virtual gamepad and full Edit/Play regressions. | PASS — accepted Mac suites |
| 4 | Mac native plus clean Windows x64 IL2CPP/D3D11 smoke/readability and zero residue. | PARTIAL — Mac PASS; physical Windows DEFERRED |
| 5 | Evidence/Bible/CHANGELOG/Roadmap plus PR/CI integration chain. | IN PROGRESS — docs authored on draft PR #118; CI/merge/closure pending |

Issue #115 must remain open and PR #118 must remain draft until the physical Windows gate passes and the exact accepted head receives the required CI/integration record. Parent visual Epic #18 and the full Steam 1.0 Goal remain open.

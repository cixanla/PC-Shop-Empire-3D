# Retail Checkout Hero Readability — Checkpoint Evidence

**Date:** 27 August 2026<br>
**Issue:** [#114](https://github.com/cixanla/PC-Shop-Empire-3D/issues/114)<br>
**PR:** [#116](https://github.com/cixanla/PC-Shop-Empire-3D/pull/116)<br>
**Technical head / merged main:** `0ea82e826aff9d245e0d4002386193278f25b483`<br>
**Technical tree:** `8cbe7bd7c7628d923930213de30e1bda73cb7619`<br>
**Technical branch:** `codex/issue114-retail-checkout-hero-readability`<br>
**Current state:** Issue acceptance `5/5`; exact scene/presentation contracts, full Mac and Windows regression suites, universal macOS native build, clean Windows x64 IL2CPP/D3D11 runtime, screenshot/readability, independent Mac readback, cleanup, PR/merge and main Repository Guard passed. PR #116 fast-forwarded the exact technical head to `main`; Issue #114 is `CLOSED` and Roadmap is `Done`. The physical USB checkpoint was not written and remains a separate deferred delivery gate.

## Delivered visible result

GarageGraybox r56 extends the accepted readable-semi-realistic direction from the Assembly Workbench to the customer-facing retail loop. Three comparable 1280x720 native compositions now expose:

1. the customer approach and store threshold,
2. the real authoritative shelf offer plus reserved basket state,
3. the real checkout terminal, payment and receipt state.

`RetailCheckoutHeroProjection` reads existing offer, basket, checkout, payment and receipt authorities and projects their current state onto presentation-only geometry. It does not create a second product, placement zone, reservation, payment, receipt or inventory authority. Customer browse/checkout/exit navigation, input ownership, gameplay colliders and the canonical `AuthoritativeRetailShelfA` remain unchanged.

The pass adds nine bounded presentation mesh renderers under `RetailCheckoutHeroReadability`. They use `Ignore Raycast`, contain no collider, do not cast or receive shadows, produce no motion vectors and own no gameplay state. One soft fill spot light is added at intensity `0.42`, range `4.4`, outer angle `110` and inner angle `68.2`; it casts no shadows. Existing cameras remain unchanged.

The exact runtime success marker is:

```text
GARAGE_RETAIL_CHECKOUT_HERO_READABILITY_RUNTIME_SMOKE states=customer-approach+shelf-offer-basket+checkout-payment-receipt hero=ready materials=dark-metal+brushed-steel+rubber+safety-accent+label-paper light=focused total-renderers=502 lights=5 cameras=1 screenshots=3 glare=bounded glare-pixels<=256 contrast=bounded contrast-ratio>=1.25 ui=hud-suppressed world-text=preserved human=false active-renderers=478 max-glare-pixels=0 min-contrast-ratio=1.348 capture-directory=<platform-evidence>/lookdev-r56
```

The marker and screenshots are automated visual evidence. They are not a real-human, physical-keyboard or physical-gamepad certification claim.

## Deterministic render and authority budget

| Contract | Issue #111 baseline | r56 technical head | Delta |
|---|---:|---:|---:|
| Authored scene `MeshRenderer` components | `490` | `499` | `+9` |
| Authored lights | `4` | `5` | `+1` |
| Authored cameras | `1` | `1` | `0` |
| Runtime scene mesh renderers, including inactive | — | `502` | bounded exact contract |
| Runtime smoke active mesh renderers | — | `478` | bounded exact contract |

The renderer/light additions are presentation-only and deliberately do not alter NavMesh, waypoints, inventory placement surfaces, shelf authority, checkout authority or serialized item identity. The pre-existing `StarterShelf` collider cluster is not silently changed by this look-development slice; its consolidation is tracked by [Issue #115](https://github.com/cixanla/PC-Shop-Empire-3D/issues/115).

`ProjectSettings/ProjectSettings.asset` remained byte-exact at SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` throughout the accepted Mac and Windows gates.

## Exact source and tests

| Platform | Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---|---:|---:|---:|---|
| macOS | Retail hero PlayMode contracts | `2/2` | `2.01924 s` | `10,226` | `fd788ccbf024ada74f95932ed8081503651ca6389371ebee51e853a850e244e8` |
| macOS | Full EditMode | `754/754` | `69.2747982 s` | `625,815` | `e8ba7ed11a84401e84e27b5b6a799c16d7a9122608ae7b6538454a2e7141b7f7` |
| macOS | Full PlayMode | `158/158` | `909.8057512 s` | `523,682` | `0aabecb8fbca5446f6c481c4e7d146778aeeb601debedd3cb9b9e7a372c913a6` |
| Windows | Full EditMode | `754/754` | `27.702872 s` | `630,619` | `9b887decb1ded61cffa51baaa57a105f064a4820782ae117692fa42509557078` |
| Windows | Full PlayMode | `158/158` | `518.4591613 s` | `525,128` | `9431702c90ae7d17bac8127109d69fe12474650e7805f11a4cd0c028ec40b895` |

Every accepted result has failed, skipped and inconclusive `0`. The full suites include retail/checkout state projection, committed-scene render/light/no-collider budgets, PNG decode and readability budgets, simultaneous WASD plus mouse-look, keyboard/mouse and Input System virtual-gamepad regressions, the existing physical retail flow and the complete ten-part Assembly chain. `git diff --check` and local repository checks passed. Technical Repository Guard [33109651186](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33109651186) passed at the exact technical head.

## macOS exact-head native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `599,565` | `fc571273d52bc44e07273851e0facc269fb6f4fb538db7e19d5740832bfd2423` |
| Apple M1/Metal retail r56 runtime log | `19,599` | `5bdbcd66bb7573545e371c62ceb5d35746ba951c9ff1045e07b83c1c1abf6f06` |
| Apple M1/Metal Assembly r55 regression log | `11,251` | `356f662db6d99b68565882ca82459c1f80360aba390d4039cc09aa3bb0ef8d7a` |
| Universal app executable | `117,179` | `1b9326d3bf80f81b867368457fe41301c6e5881e9a93b75cef08272eafb53b01` |

The macOS build marker reports `330,481,405` bytes. The application contains `302` files; `file` confirms Mach-O `x86_64 + arm64`, and `codesign --verify --deep --strict` passes. The retail native runtime emits one exact readiness marker, one exact r56 success marker and three byte-distinct captures. The independent historical Assembly r55 smoke also remains green at `493` renderers, `4` lights and `1` camera.

| macOS screenshot | Bytes | SHA-256 | Glare pixels | Contrast ratio |
|---|---:|---|---:|---:|
| `retail-customer-approach-r56.png` | `608,250` | `3c6d57a54700c3e8758aa8349db4aac9bc4431a5a5647cdf6d7026167432396c` | `0` | `1.348` |
| `retail-shelf-offer-basket-r56.png` | `615,030` | `54213829cd09299503e77cc0ed1f2236d89d0dfc665fd73ad8a37ef8736d17ba` | `0` | `1.856` |
| `retail-checkout-payment-receipt-r56.png` | `677,043` | `9fc6671d4db140bd173db42cf436d71c8efb7dc9611e66fe3dadf74b2ba15f92` | `0` | `2.176` |

Canonical raw Mac evidence lives under `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue114-retail-checkout-r46` through `r51`.

## Windows clean IL2CPP, D3D11 and screenshot gate

The Windows canonical checkout remained a clean validation lane. A complete Git bundle (`7,942,603` bytes, SHA-256 `a90af9aba58fcbf25a8b634ea6d44e0d8a88eef74493e5bd0f1b7287ad8e67c5`) produced a disposable exact-head/tree clone. Windows did not author or push source.

Full EditMode `754/754` and PlayMode `158/158` passed. Unity 6000.3.21f1 built an x64 IL2CPP Development player with Direct3D11 only. The build marker reports `1,351,471,280` bytes; output contains `674` files and `1,351,640,274` bytes. The `572,468`-byte build log has SHA-256 `d893eb3e2254f2c49d9609c3d0deb0bf4efec7729f8ebb8d5186b9e2b7582aa9` and zero expanded fatal tokens.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | `667,136` | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | `45,930,496` | `e5c5af9198466d9ee7f042139fcbd32b3f569dc6f34d7a31b008f66f281683f1` |
| `UnityPlayer.dll` | `84,237,744` | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted interactive runtime used `-force-d3d11`, exact r56 smoke and the logged-on user. Intel Iris Xe is device `0x46a8`. All `27/27` runtime checks pass: task/result/residue, host/readiness/success, exact arguments, source/tree/ProjectSettings, screenshot names/hashes/composition/metrics, Direct3D11, Iris Xe, graceful shutdown and final process state. The player peaked at `502,153,216` working-set bytes and `18.219` CPU seconds, then exited normally; forced cleanup count is `0`.

| Windows screenshot | Bytes | SHA-256 | Glare pixels | Contrast ratio |
|---|---:|---|---:|---:|
| `retail-customer-approach-r56.png` | `599,292` | `70ae4b422d7de8d99662fa1a3f85d13844d8829000960f15da48c4c5896d2e22` | `0` | `1.348` |
| `retail-shelf-offer-basket-r56.png` | `607,708` | `abd066fbdd2d33f672733041d31c149ff9f08c55c7be97acb3f0b37d0a3d59ee` | `0` | `1.857` |
| `retail-checkout-payment-receipt-r56.png` | `669,043` | `f9eeb551ccc4f6874ba55e4c6100ab04523f225db015113acd2fbae3cdb64652` | `0` | `2.178` |

Independent Mac/Pillow readback reproduced every Windows screenshot hash, 1280x720 dimensions, glare count and contrast ratio exactly. The Windows cleanup receipt records disposable root absent, transferred temp/task/firewall/heavy-process residue `0/0/0/0`, approximately `10.4 GB` free RAM and `199.64 GB` free on C:. Canonical raw Windows evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue114-0ea82e8-r1/windows`.

The original PowerShell build-summary serialization expanded one short marker into extended string metadata. No game build or runtime artifact was affected. A validation-only finalizer flattened the already independently rechecked result; the normalized summary is `1,738` bytes / SHA-256 `a38e62e769d1386c836b18c521c4ecc52cd6212854fb061815036f09420b2db0`.

## Issue #114 acceptance matrix — technical state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact single retail placement/inventory authority and presentation-only hero geometry. | PASS |
| 2 | Customer approach, authoritative shelf/offer/basket and checkout/payment/receipt states remain readable in comparable native captures. | PASS — automated, `human=false` |
| 3 | Renderer/light/material budgets are exact; glare and contrast budgets are enforced. | PASS |
| 4 | Targeted/full Mac tests, universal native smoke, clean exact-commit Windows x64 IL2CPP/D3D11 runtime and zero final residue. | PASS |
| 5 | Evidence/Bible/CHANGELOG/Roadmap/PR integration chain. | PASS after closure-record integration |

Technical acceptance is `5/5`. PR #116 fast-forwarded exact technical head `0ea82e826aff9d245e0d4002386193278f25b483` to `main`; technical Guard `33109651186` and post-merge main Guard `33127652290` passed. Issue #114 is closed and Roadmap is `Done`; parent visual Epic #18 and the full Steam 1.0 Goal remain open.

No USB was connected during closure-record recovery, so no Issue #114 immutable USB checkpoint was created. This is recorded as an explicit deferred physical-delivery gate, not silently treated as passed. The current work does not claim physical-human keyboard/gamepad/endurance certification or final shop/open-world/character/art completion. The next bounded technical issue is #115: consolidate the pre-existing legacy `StarterShelf` collider/NavMesh volume with the authoritative retail shelf while preserving the accepted r56 visual and retail authority contracts.

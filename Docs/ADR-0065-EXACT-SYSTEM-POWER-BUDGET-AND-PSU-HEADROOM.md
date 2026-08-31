# ADR-0065 — Exact System Power Budget and PSU Headroom

**Status:** Accepted for Mac technical source `57e6b54`, tree `8652882`; physical Windows and USB gates are deferred; draft PR #122 is the integration record<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #121, dependent on Issue #119 exact electrical readiness

## Context

Issue #119 proves that the canonical motherboard, CPU, A2 DDR5, primary M.2, processor cooler, graphics card and power supply are mechanically retained and that ATX24, EPS12V and PCIe/GPU power cables are routed with exact stable lineage. It deliberately does not prove that the installed power supply has enough continuous output for the selected parts.

The legacy Dashboard calculates estimated system draw from a fixed 35 W platform base plus case, CPU, GPU, RAM, storage and cooler loads. Its recommended PSU capacity is that draw with 30 percent headroom, rounded upward to the next 50 W. The Unity project needs this useful product rule without trusting display text, creating a second Assembly authority or pretending that a successful estimate is a physical power-on, POST or benchmark result.

## Decision

- Add immutable `PcElectricalSpecification` records and one `PcElectricalCatalog` bound by reference to the exact authoritative `PcComponentCatalog`. Product ID and component kind must resolve in that same catalog; null, empty, foreign, duplicate, unsupported or mismatched metadata fails closed.
- Keep mechanical component metadata and electrical metadata separate. The first slice supports continuous loads for CPU, memory, storage, processor cooler and graphics card plus rated continuous output for the PSU. Motherboard/platform demand remains the explicit versioned base-load policy and the chassis/case demand remains a separate policy value.
- Add a versioned integer-only `PcPowerBudgetPolicy`. Policy v1 uses `35 W` platform base, `4 W` chassis, `30%` headroom and a `50 W` capacity quantum. Integer ceiling avoids floating-point drift.
- Add a read-only `PcPowerBudgetAuthority`. Every assessment first calls the canonical `EvaluateElectricalReadiness()` query and accepts no cached UI boolean or caller-supplied readiness snapshot.
- Bind the result to the successful immutable `ElectricalReadinessSnapshot`, exact component product IDs and policy ID. Resolve every load and the installed PSU by exact `ProductDefinitionId` and expected component kind.
- For the prototype catalog use CPU `125 W`, GPU `200 W`, DDR5 `6 W`, NVMe `5 W`, cooler `5 W`, platform `35 W` and chassis `4 W`. The exact draw is `380 W`; v1 recommends `500 W`; the installed PSU is `550 W`; capacity margin is `+50 W`.
- Treat an undersized PSU as a valid assessment with `IsSufficient == false`, a negative margin and exact `PowerSupplyInsufficient` blocker. Missing/foreign/malformed metadata or arithmetic/policy failure remains an operation failure.
- Keep the authority pure: assessment changes no Inventory, BuildKit, reservation, custody, Assembly, cable revision, receipt, replay or benchmark state.
- Extend the existing presentation-only workbench projection to show `GÜÇ BÜTÇESİ UYGUN`, `380W / EN AZ 500W / PSU 550W`, `GÜÇ TESTİ BEKLİYOR` only after exact readiness and capacity both pass. Insufficient/missing state shows an exact blocker and remains not ready.
- Do not add an input action, collider, power switch, power-test attempt, receipt, electrical fault, damage, rail/transient model, POST, BIOS/UEFI, OS, driver, benchmark, packaging or delivery state in this issue. Those require later bounded authorities.
- Preserve existing renderer, light, camera, collider, NavMesh and input ownership budgets. Preserve ProjectSettings and keep the user/editor-owned ProBuilder setting unstaged and unreverted.
- Require targeted and full Mac tests, universal native build and Apple M1/Metal smokes now. Keep clean exact-commit physical Windows x64 IL2CPP/only-D3D11/Intel Iris Xe validation mandatory before closure; UTM is not equivalent evidence.

## Consequences

GarageGraybox r59 can answer a concrete preflight question before a future power-test command: the exact installed prototype requires `500 W` under the accepted headroom policy and its `550 W` PSU has `50 W` margin. The answer is recomputed from current canonical readiness and immutable product metadata, so Presentation cannot manufacture or preserve stale success.

The split keeps later simulation honest. A sufficient budget means only that the configured continuous-output rule passes. It does not prove connector pinout/polarity, transient behavior, short-circuit protection, actual energization, POST, boot stability or benchmark quality. A later benign power-test authority must revalidate current lineage and own its own operation ID, revision and receipt rather than mutating this read-only snapshot.

No scene object is added. The accepted Assembly readability budget remains `479` total / `470` smoke-active renderers, four lights and one camera; the workbench status projection remains colliderless, inputless and on `Ignore Raycast`.

## Current verification

- Technical source `57e6b54883ef6756c5522d1de9c17479e7cda481`, tree `8652882bb5e791c969b9c8648cfe7e242a5a92d7`; draft PR #122; Issue/Roadmap remain open and In Progress.
- A bounded, read-only exact-commit review found no P0, P1 or P2 defect in product identity binding, immutable metadata, arithmetic bounds, readiness-first behavior, mutation safety, revision lineage or the presentation-only boundary.
- Targeted catalog, authority, scene, keyboard/mouse, virtual-gamepad and hero-regression tests pass `15/15`.
- Unity 6000.3.21f1 full Mac EditMode `768/768` and PlayMode `158/158`; failed, skipped and inconclusive `0`.
- Universal macOS Development build report `330,465,045` bytes across `302` files. The executable is deep/strict-valid universal Mach-O `x86_64 + arm64`.
- Apple M1/Metal 1280x720 native PCIe/readiness smoke emits exact `power-budget=380/500/550`, exits `0`, shuts Input System down cleanly and leaves no player residue.
- Assembly-readability native regression preserves `479/470`, produces three byte-distinct 1280x720 captures and reports central glare `0`.
- ProjectSettings/package manifests and the separately preserved ProBuilder user-setting hash are byte-exact across the build.
- Physical Windows x64 IL2CPP/D3D11/Iris Xe, physical-human HID/endurance and USB checkpoint/readback remain deferred and are not claimed.

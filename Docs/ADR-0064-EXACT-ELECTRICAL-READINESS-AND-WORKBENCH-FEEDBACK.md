# ADR-0064 — Exact Electrical Readiness and Workbench Feedback

**Status:** Accepted for Mac technical source `f33a052`, tree `986ff174`; physical Windows and USB gates are deferred; draft PR #120 is the integration record<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #119, child of Epic #10

## Context

Issues #89 through #109 preserve the canonical custom-PC work order, exact ten-part BuildKit history and the same serialized identities while moving seven components into their existing Assembly authorities and routing the exact ATX24, EPS12V and PCIe/GPU cables. That chain proves mechanical retention and cable routing, but it deliberately stops before electrical power, POST, firmware, operating-system or benchmark state.

The player needs an unambiguous physical-workbench result when the final required cable is routed, and an exact reason when the build is not ready. Reusing the broad benchmark-readiness result would be incorrect because it also represents later product phases. Creating a second Assembly or work-order authority in Presentation would split truth and could silently drift from the retained/routed state.

## Decision

- Add `AssemblyBuildAuthority.EvaluateElectricalReadiness()` as a pure, read-only query. It never mutates Assembly, Inventory, BuildKit, reservation, receipt or replay state.
- Evaluate blockers in deterministic physical order: motherboard, CPU, A2 DDR5, primary M.2, processor cooler plus consumed TIM, graphics card, power supply, ATX24, EPS12V and PCIe/GPU cable.
- Fail closed for missing, unsecured/unretained, unsupported-topology or invariant-invalid state. Do not infer readiness from item count, display text, value-equal identity or presentation state.
- Publish a successful immutable `ElectricalReadinessSnapshot` only after the normal Assembly invariant audit and exact-lineage audit both pass. Bind exact build/chassis IDs, all ten stable serialized item IDs, the seven retain/secure operations, the three route operations, the main Assembly revision and all three cable revisions.
- Keep existing benchmark readiness unchanged and blocked. Electrical readiness is only a prerequisite for a future physical power-test command; it is not power-on, POST, BIOS/UEFI, OS, drivers, benchmark completion, packaging or delivery.
- Add one presentation-only `ElectricalReadinessWorkbenchProjection` to the existing workbench. It reads the already initialized canonical session; it must not initialize or repair gameplay state, own input, create receipts or write authority.
- Refresh only when Inventory, Assembly or one of the three cable revisions changes. Manual refresh remains exact and idempotent; the per-frame path does not run the full invariant audit or allocate a snapshot while state is unchanged.
- Render the successful player text as `ELEKTRİK HAZIR`, `10/10 PARÇA • 3/3 KABLO`, `GÜÇ TESTİ BEKLİYOR`. Render the first exact blocker plus `GÜÇ HAZIR DEĞİL` for failure.
- Keep the two presentation renderers on `Ignore Raycast`, without collider, NavMesh obstacle, light, waypoint, item or input owner. They are not gameplay authority.
- Extend the existing final PCIe/GPU native smoke to prove `blocked → ready after route → blocked after unroute`, exact workbench text and unchanged authority invariants.
- Preserve ProjectSettings. The separate user/editor-owned ProBuilder setting remains outside the Issue #119 commit and must neither be staged nor reverted.
- Require full Mac tests, universal native build and Apple M1/Metal smokes now. Keep clean exact-head physical Windows x64 IL2CPP/only-D3D11/Intel Iris Xe validation mandatory for closure; UTM is not equivalent evidence.

## Consequences

GarageGraybox r58 now gives the player a deterministic, localized status at the physical Assembly Workbench. Routing the exact final PCIe/GPU cable changes the canonical read-only result to ready; unrouting it immediately returns the exact PCIe/GPU blocker. No power state is manufactured and no later benchmark or customer-delivery state advances.

The snapshot is strong enough for a future power-test authority to validate exact lineage without trusting a UI boolean. Presentation remains disposable: deleting the projection would remove only the visible feedback, not the electrical-readiness decision.

The scene adds exactly two decorative renderers and no light, camera or collider. The accepted Assembly hero budget becomes `479` total / `470` smoke-active renderers, and the retail regression becomes `488` total / `464` active renderers. Existing workbench and retail readability contracts remain intact.

## Current verification

- Technical source `f33a052d3f3ef25d48ff8b5d5f4d4a149f414fdc`, tree `986ff174209dc55bb98cf7f1151fc8cc480384fc`; draft PR #120; Issue/Roadmap remain open and In Progress.
- Unity 6000.3.21f1 final Mac EditMode `758/758` and PlayMode `158/158`; failed, skipped and inconclusive `0`.
- Targeted scene, fail-closed projection/read-only and keyboard/mouse plus Input System virtual-gamepad contracts pass.
- Universal macOS Development build report `330,441,141` bytes across `302` files. The deep/strict-valid executable is Mach-O `x86_64 + arm64`.
- Apple M1/Metal 1280×720 native PCIe smoke emits readiness and exact `electrical-readiness=ready-then-blocked monitor=ok` once, exits `0`, shuts the Input System down cleanly and leaves no player residue.
- Apple M1/Metal Assembly-readability regression emits the accepted `479/470` renderer contract, three byte-distinct 1280×720 captures and central glare `0`.
- All ProjectSettings hashes and the separately preserved ProBuilder user-setting hash are byte-exact across the accepted build.
- Physical Windows x64 IL2CPP/D3D11/Iris Xe, physical-human HID/endurance and USB checkpoint/readback remain deferred and are not claimed.

# ADR-0061 — Canonical ATX24 BuildKit-to-Route Reversible Assembly Handoff

**Status:** Accepted at technical source `5d6a398`, tree `2633078`; exact-head Mac and detached-clean Windows gates passed; Repository Guard `33038180913` passed; PR #106 is the integration record<br>
**Date:** 27 August 2026<br>
**Scope:** Issue #105, child of Epic #10

## Context

Issues #68 through #87 stage the exact ten reserved custom-PC components in component-specific capacity-one BuildKit slots. Issues #89, #91, #93, #95, #97, #99 and #102 move the motherboard, CPU, DDR5, M.2, processor cooler, graphics card and ATX PS/2 power supply into their existing Assembly authorities while preserving immutable staging history and live reservation/allocation lineage.

Issue #83 stages the canonical reserved ATX24 cable. Issue #61 already owns the only deterministic ATX24 Assembly route: exact PSU 18-pin and PSU 10-pin split endpoints, exact motherboard 24-pin endpoint, three ordered waypoints, keyed orientation, clearance/obstruction checks, route/unroute, replay and recovery. Issues #62 and #63 independently own EPS12V and PCIe/GPU 6+2 routes. The missing boundary is a reservation-safe bridge from the exact ATX24 BuildKit slot to actor hands and then into the existing Issue #61 route after all seven installed component prerequisites are proven.

A second Inventory, BuildKit, cable, connector, endpoint, waypoint or route authority would split truth. A presentation-only pickup would also be invalid: the same serialized cable Unity instance, stable ItemId and reservation/allocation lineage must survive `BuildKit → ActorHands → Routed → ActorHands`, and every failed preflight or projection must remain no-mutation or recover to the current authoritative pose.

## Decision

- Resolve only the owned work order's exact canonical `PowerCable` line with family `ModularAtx24SplitPsuToMotherboard` and its complete `LineId`, `ProductId`, serialized `ItemId`, `ReservationId`, parent allocation and immutable staging-receipt tuple. Ordinal, display-name, enum-only and value-equivalent identities cannot acquire authority.
- Require the historical exact ten-receipt `10/10` BuildKit aggregate plus the live exact Issue #89/#91/#93/#95/#97/#99/#102 Assembly chain. Motherboard must be `SeatedSecured`; CPU `ProcessorRetained`; DDR5 exact A2 `MemoryModuleRetained`; M.2 exact primary slot `StorageDeviceSecured`; cooler `CoolerRetained`; GPU exact PCIe x16 `GraphicsCardRetained`; PSU exact bay `PowerSupplyRetained`.
- Require the configured capacity-one managed ATX24 route container to be `Loose`, foreign-container-free and bound to the exact canonical cable, three typed endpoints and three distinct ordered waypoints. Full hands, occupied/routed target, stale authority/revision, wrong host state, topology mismatch, invalid orientation, range/focus/LOS/pause failure, clearance/obstruction failure or revision overflow fails closed.
- Use a stable ATX24 handoff operation identity distinct from staging, every component handoff, PSU seat/retention, ATX24 route/unroute and EPS12V/PCIe operations. Immediate and delayed replay return the same immutable handoff receipt without a second BuildKit/Inventory mutation.
- Add only the narrowly registered exact ATX24 BuildKit → `ActorHands` release. Subsequent reversible transfer remains the existing exact `ActorHands ↔ ATX24 route container` Assembly path; generic reserved transfer, checkout, world drop, box, stack, cart, raw Inventory move and receipt-free route bypass stay closed.
- Preserve reservation and parent allocation through pickup, route and unroute. Preserve all ten staging receipts and visible `10/10` history while tracking current custody separately. EPS12V and PCIe/GPU cable item/container/state/revision/receipt/operation lineages remain exact and untouched.
- Reuse Issue #61 endpoint, waypoint, orientation, clearance, obstruction, preview-equals-commit, route/unroute and replay rules. This handoff authorizes that existing authority; it neither recreates nor relaxes it.
- Accept only exact installed host roots when solving route obstruction exclusions: the retained processor cooler, graphics card and chassis right rail may be ignored as authored hosts; foreign objects or value-equal substitutes remain obstructions.
- Commit authoritative custody before changing parent, pose, physics or visibility. Projection failure recovers the same Unity object and stable ItemId to authoritative hands/route pose. Duplicate, ghost and loss counts remain zero.
- Keep generic drop blocked while carrying the reserved ATX24 cable. Keep PSU unretain/remove blocked while ATX24 is routed. Keep the single-consumer input boundary across BuildKit pickup, ATX24 route, PSU interaction, other cable routes and generic carry actions.
- Do not manufacture electrical readiness. With ATX24 routed, the assembly remains `BuildIncomplete` until EPS12V and PCIe/GPU are routed. After ATX24 unroute, the exact readiness failure is `PowerCableMissing`.
- Do not change ProjectSettings. Bind acceptance to exact technical commit/tree, targeted and full XMLs, universal Mac build/native smoke, detached-clean Windows full tests, x64 IL2CPP/only-D3D11 build/runtime, foreground bounded OS input, binary/procedure/evidence hashes and zero scoped process/task/firewall residue.

## Consequences

GarageGraybox r52 lets the player approach the completed BuildKit after all seven components are installed; take the canonical ATX24 cable with `E / Gamepad South`; carry the same instance; open the existing guided ATX24 route with `Mouse Left / Gamepad RT`; rotate only between the canonical keyed orientations with `R / Right Shoulder`; commit with `G / Gamepad East`; keep the cable visibly routed; block generic drop and PSU removal; then focus the routed cable with empty hands and unroute the exact same instance back to `ActorHands`. The BuildKit reports `ATX24 MONTAJDA`; immutable `10/10` history stays visible; EPS12V and PCIe/GPU remain staged and untouched.

The automated r52 smoke and PlayMode flow are invariant evidence, not a real-human or physical-device session. Windows foreground `SendInput` proves bounded OS delivery of W/A/S/D, relative mouse and W+D-held simultaneous mouse deltas to the real r52 player window, with `human=false`. Input System gamepad automation is not a physical-gamepad claim.

The PC is still not electrically ready. EPS12V and PCIe/GPU BuildKit-to-route bridges, wattage/headroom, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging/delivery/settlement, Save/Guardian, staff/customer/world expansion and final art remain dependent Steam 1.0 work.

## Current verification

- Technical source commit `5d6a39892cf3c585abd1046cc799a93418329cd0`, tree `263307821aeba8df6648a39756bec431e548938f`; no ProjectSettings change.
- Unity 6000.3.21f1 targeted Mac EditMode `79/79`, targeted P1 PlayMode `4/4`, full EditMode `744/744` and full PlayMode `148/148`; failed, skipped and inconclusive `0`.
- Universal macOS Development build report `330,311,979` bytes across `302` files. The deep/strict-valid `x86_64 + arm64` executable emits one r52 readiness marker and one exact ATX24 Assembly success marker on Apple M1/Metal, reaches Input System shutdown, exits `0` and leaves no player residue.
- Exact-head technical Repository Guard [33038180913](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33038180913) passed.
- Complete bundle `7,678,445` bytes / SHA-256 `a9c331a43ed7da50376df2a9ac0906a7396faa008619d050227a4388dbb28503` restored a detached-clean Windows clone at the exact technical commit/tree.
- Windows Unity 6000.3.21f1 full EditMode `744/744` and full PlayMode `148/148` passed. The x64 IL2CPP/only-D3D11 build report is `1,347,195,309` bytes with fatal-token count `0` and byte-exact `ProjectSettings.asset` restoration.
- Intel Iris Xe Direct3D 11.0 feature level 11.1 interactive runtime has exact host/readiness/success counts `1/1/1`, forbidden count `0`, exit `0`, graceful shutdown, deleted task and validation-owned residue `0`.
- Accepted foreground Session 2 OS-input r2 delivers W/A/S/D down/up `1/1`, relative mouse `18/18`, initial W+D+mouse `3/3` and further held-key mouse deltas `30/30`; exact player foreground is rechecked around each stage and all eight screenshots have nonzero, unique hashes. Its runtime forbidden count is `0`.
- The first OS-input r1 run is retained only as negative harness evidence: `-pse-require-d3d11` without a supported smoke flag intentionally produced `smoke.graphics-api-mismatch`. It is not counted as acceptance. The clean r2 removes that flag and proves D3D11 directly from Unity engine lines.
- Windows final audit schema `pcshop-issue105-windows-final-audit-v1` passes all `33` checks. Exact validation-created TCP/UDP firewall block rules for the disposable player path were removed; process, scheduled-task and firewall-rule residue are `0`.
- Windows evidence returned to the Mac as a `6,091,832`-byte tar with SHA-256 `5e3674ded54f9ef8dc115061b8e0112d5946e2e2b7fe32a01b7e72b5a35f0c21`; `36/36` transported evidence files match the 39-entry manifest after excluding the three referenced native binaries.
- No Windows removable volume or USB disk was identified. This issue performs no USB write or USB checkpoint claim.
- Real-human, physical keyboard, physical gamepad and endurance certification remain explicitly deferred to the final Steam 1.0 release gate.

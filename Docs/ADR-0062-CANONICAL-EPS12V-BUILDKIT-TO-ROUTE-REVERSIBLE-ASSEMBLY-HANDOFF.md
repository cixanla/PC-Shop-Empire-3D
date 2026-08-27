# ADR-0062 — Canonical EPS12V BuildKit-to-Route Reversible Assembly Handoff

**Status:** Accepted at technical source `9cd3276`, tree `01f3edc`; exact-head Mac and detached-clean Windows gates passed; Repository Guard `33044086315` passed; PR #108 is the integration record<br>
**Date:** 27 August 2026<br>
**Scope:** Issue #107, child of Epic #10

## Context

Issues #68 through #87 stage the exact ten reserved custom-PC components in component-specific capacity-one BuildKit slots. Issues #89, #91, #93, #95, #97, #99 and #102 move the motherboard, CPU, DDR5, M.2, processor cooler, graphics card and ATX PS/2 power supply into their existing Assembly authorities while preserving immutable staging history and live reservation/allocation lineage. Issue #105 then hands the exact ATX24 cable to the existing Issue #61 route and leaves it routed.

Issue #85 stages the canonical reserved EPS12V cable. Issue #62 already owns the only deterministic EPS12V Assembly route: exact modular PSU EPS12V 8-pin endpoint, exact motherboard CPU EPS12V 8-pin endpoint, three ordered waypoints, keyed orientation, clearance/obstruction checks, route/unroute, replay and recovery. Issues #61/#105 and #63 independently own ATX24 and PCIe/GPU 6+2 routes. The missing boundary is a reservation-safe bridge from the exact EPS12V BuildKit slot to actor hands and then into the existing Issue #62 route after all seven installed component prerequisites and the exact routed ATX24 prerequisite are proven.

A second Inventory, BuildKit, cable, connector, endpoint, waypoint or route authority would split truth. A presentation-only pickup would also be invalid: the same serialized cable Unity instance, stable ItemId and reservation/allocation lineage must survive `BuildKit → ActorHands → Routed → ActorHands`, and every failed preflight or projection must remain no-mutation or recover to the current authoritative pose.

## Decision

- Resolve only the owned work order's exact canonical `PowerCable` line with family `ModularEps12v8PinPsuToMotherboard` and its complete `LineId`, `ProductId`, serialized `ItemId`, `ReservationId`, parent allocation and immutable staging-receipt tuple. Ordinal, display-name, enum-only and value-equivalent identities cannot acquire authority.
- Require the historical exact ten-receipt `10/10` BuildKit aggregate plus the live exact Issue #89/#91/#93/#95/#97/#99/#102 Assembly chain and exact Issue #105 ATX24 routed receipt. Motherboard must be `SeatedSecured`; CPU `ProcessorRetained`; DDR5 exact A2 `MemoryModuleRetained`; M.2 exact primary slot `StorageDeviceSecured`; cooler `CoolerRetained`; GPU exact PCIe x16 `GraphicsCardRetained`; PSU exact bay `PowerSupplyRetained`; ATX24 exact route `Routed`.
- Require the configured capacity-one managed EPS12V route container to be `Loose`, foreign-container-free and bound to the exact canonical cable, two typed endpoints and three distinct ordered waypoints. Full hands, occupied/routed target, stale authority/revision, wrong host state, topology mismatch, invalid orientation, range/focus/LOS/pause failure, clearance/obstruction failure or revision overflow fails closed.
- Use a stable EPS12V handoff operation identity distinct from staging, every component handoff, PSU seat/retention, ATX24 route/unroute, EPS12V route/unroute and PCIe operations. Immediate and delayed replay return the same immutable handoff receipt without a second BuildKit/Inventory mutation.
- Add only the narrowly registered exact EPS12V BuildKit → `ActorHands` release. Subsequent reversible transfer remains the existing exact `ActorHands ↔ CpuPowerCableRoute` Assembly path; generic reserved transfer, checkout, world drop, box, stack, cart, raw Inventory move and receipt-free route bypass stay closed.
- Preserve reservation and parent allocation through pickup, route and unroute. Preserve all ten staging receipts and visible `10/10` history while tracking current custody separately. ATX24 item/container/state/revision/receipt/operation lineage remains exact-routed; PCIe/GPU cable lineage remains exact and untouched.
- Reuse Issue #62 endpoint, waypoint, orientation, clearance, obstruction, preview-equals-commit, route/unroute and replay rules. This handoff authorizes that existing authority; it neither recreates nor relaxes it.
- Accept only exact installed host roots when solving route obstruction exclusions: the chassis right rail, retained graphics card and exact PCIe power connector may be ignored as narrowly authored hosts; foreign objects or value-equal substitutes remain obstructions.
- Commit authoritative custody before changing parent, pose, physics or visibility. Projection failure recovers the same Unity object and stable ItemId to authoritative hands/route pose. Duplicate, ghost and loss counts remain zero.
- Keep generic drop blocked while carrying the reserved EPS12V cable. Keep PSU unretain/remove, motherboard detach/unsecure and CPU retention-open/remove blocked while EPS12V is routed. Keep the single-consumer input boundary across BuildKit pickup, EPS12V route, ATX24/PCIe routes, PSU interaction and generic carry actions.
- Do not manufacture electrical readiness. With ATX24 and EPS12V routed, the assembly remains `BuildIncomplete` until PCIe/GPU power is routed. After EPS12V unroute the exact readiness failure is `PowerCableMissing`.
- Do not change ProjectSettings. Bind acceptance to exact technical commit/tree, targeted and full XMLs, universal Mac build/native smoke, detached-clean Windows full tests, x64 IL2CPP/only-D3D11 build/runtime, foreground bounded OS input, binary/procedure/evidence hashes and zero scoped process/task/firewall residue.

## Consequences

GarageGraybox r53 lets the player approach the completed BuildKit after all seven components are installed and ATX24 is routed; take the canonical EPS12V cable with `E / Gamepad South`; carry the same instance; open the existing guided EPS12V route with `Mouse Left / Gamepad RT`; rotate only between the canonical keyed orientations with `R / Right Shoulder`; commit with `G / Gamepad East`; keep the cable visibly routed; block generic drop and dependent removal; then focus the routed cable with empty hands and unroute the exact same instance back to `ActorHands`. The BuildKit reports `EPS12V MONTAJDA`; immutable `10/10` history stays visible; ATX24 remains routed and PCIe/GPU remains staged and untouched.

The automated r53 smoke and PlayMode flow are invariant evidence, not a real-human or physical-device session. Windows foreground `SendInput` proves bounded OS delivery of W/A/S/D, relative mouse and W+D-held simultaneous mouse deltas to the real r53 player window, with `human=false`. Input System gamepad automation is not a physical-gamepad claim.

The PC is still not electrically ready. The PCIe/GPU BuildKit-to-route bridge, wattage/headroom, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging/delivery/settlement, Save/Guardian, staff/customer/world expansion and final art remain dependent Steam 1.0 work.

## Current verification

- Technical source commit `9cd3276d60c03cec1b5b15049027523dddbee8b6`, tree `01f3edc99dd94aeeb125323048bf8532891c028a`; no ProjectSettings change.
- Unity 6000.3.21f1 targeted Mac domain EditMode `83/83`, scene contract EditMode `9/9`, targeted P1 PlayMode `4/4`, full EditMode `748/748` and full PlayMode `152/152`; failed, skipped and inconclusive `0`.
- Universal macOS Development build report `330,340,220` bytes across `302` files. The deep/strict-valid `x86_64 + arm64` executable emits one r53 readiness marker and one exact EPS12V Assembly success marker on Apple M1/Metal, reaches Input System shutdown, exits `0` and leaves no player residue.
- Exact-head technical Repository Guard [33044086315](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33044086315) passed.
- Complete bundle `7,708,889` bytes / SHA-256 `ffd2d43a3c0182c8c0e21565b21fb85b16fc72416dd2623a4a237c1875bebe55` restored a detached-clean Windows clone at the exact technical commit/tree.
- Windows Unity 6000.3.21f1 full EditMode `748/748` and full PlayMode `152/152` passed. The x64 IL2CPP/only-D3D11 build report is `1,348,030,823` bytes with fatal-token count `0` and byte-exact `ProjectSettings.asset` restoration.
- Intel Iris Xe Direct3D 11.0 feature level 11.1 interactive runtime has exact host/readiness/success counts `1/1/1`, forbidden count `0`, exit `0`, graceful shutdown, deleted task and validation-owned residue `0`.
- Accepted foreground Session 2 OS input delivers W/A/S/D down/up `1/1`, relative mouse `18/18`, initial W+D+mouse `3/3` and further held-key mouse deltas `30/30`; exact player foreground is rechecked around each stage and all eight screenshots have nonzero, unique hashes. Its runtime forbidden count is `0`.
- Windows final audit schema `pcshop-issue107-windows-final-audit-v1` passes all `28` checks. Two exact-path development-player TCP/UDP Query User rules were removed after exact program-path verification; process, scheduled-task and firewall-rule residue are `0`.
- Windows evidence returned to the Mac as a `3,146,658`-byte tar with SHA-256 `239614d2c0c4e1a0fc652aa1db106c71df0563161a810a482cffb2e479a53525`; all `32/32` transported manifest evidence files match bytes and SHA-256, while the two self-referential final audit/manifest files independently match their Windows hashes.
- No Windows removable volume or USB disk was identified. This issue performs no USB write or USB checkpoint claim.
- Real-human, physical keyboard, physical gamepad and endurance certification remain explicitly deferred to the final Steam 1.0 release gate.

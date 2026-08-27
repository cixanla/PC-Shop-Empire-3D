# ADR-0063 — Canonical PCIe/GPU 6+2 BuildKit-to-Route Reversible Assembly Handoff

**Status:** Accepted at technical source `1acba16`, tree `eb40a392`; exact-head Mac and clean Windows gates passed; Repository Guard `33054757532` passed; PR #110 is the integration record<br>
**Date:** 27 August 2026<br>
**Scope:** Issue #109, child of Epic #10

## Context

Issues #68 through #87 stage the exact ten reserved custom-PC components in component-specific capacity-one BuildKit slots. Issues #89, #91, #93, #95, #97, #99 and #102 move the motherboard, CPU, DDR5, M.2, processor cooler, graphics card and ATX PS/2 power supply into their existing Assembly authorities while preserving immutable staging history and live reservation/allocation lineage. Issue #105 routes the exact ATX24 cable through the existing Issue #61 authority; Issue #107 routes the exact EPS12V cable through the existing Issue #62 authority.

Issue #87 stages the canonical reserved PCIe/GPU 6+2 cable. Issue #63 already owns the only deterministic PCIe/GPU Assembly route: exact modular-PSU monolithic 8-pin endpoint, exact GPU-side 6-pin + 2-pin endpoint pair, three ordered waypoints, keyed orientation, clearance/obstruction checks, route/unroute, replay and recovery. The missing boundary is a reservation-safe bridge from the exact PCIe/GPU BuildKit slot to actor hands and then into the existing Issue #63 route after all seven installed component prerequisites and both exact routed ATX24/EPS12V prerequisites are proven.

A second Inventory, BuildKit, cable, connector, endpoint, waypoint or route authority would split truth. A presentation-only pickup is also invalid: the same serialized cable Unity instance, stable ItemId and reservation/allocation lineage must survive `BuildKit → ActorHands → Routed → ActorHands`, while every failed preflight or projection remains no-mutation or recovers to the current authoritative pose.

## Decision

- Resolve only the owned work order's exact canonical `PowerCable` line with family `ModularPcie8PinPsuToGraphicsCard` and complete `LineId`, `ProductId`, serialized `ItemId`, `ReservationId`, parent allocation and immutable staging-receipt tuple. Ordinal, display-name, enum-only and value-equivalent identities cannot acquire authority.
- Require the historical exact ten-receipt `10/10` BuildKit aggregate plus the live exact Issue #89/#91/#93/#95/#97/#99/#102 Assembly chain and exact Issue #105/#107 routed-cable receipts. Motherboard must be `SeatedSecured`; CPU `ProcessorRetained`; DDR5 exact A2 `MemoryModuleRetained`; M.2 exact primary slot `StorageDeviceSecured`; cooler `CoolerRetained`; GPU exact PCIe x16 `GraphicsCardRetained`; PSU exact bay `PowerSupplyRetained`; ATX24 and EPS12V exact routes `Routed`.
- Require the configured capacity-one managed `GpuPowerCableRoute` container to be `Loose`, foreign-container-free and bound to the exact canonical cable, exact typed/keyed connectors and three distinct ordered waypoints. Full hands, occupied/routed target, stale authority/revision, wrong host state, topology mismatch, invalid orientation, range/focus/LOS/pause failure, clearance/obstruction failure or revision overflow fails closed.
- Use a stable PCIe/GPU handoff operation identity distinct from staging, every component handoff, PSU/GPU seat-retention, ATX24/EPS12V route/unroute and PCIe/GPU route/unroute operations. Immediate and delayed replay return the same immutable handoff receipt without a second BuildKit/Inventory mutation.
- Add only the narrowly registered exact PCIe/GPU BuildKit → `ActorHands` release. Subsequent reversible transfer remains the existing exact `ActorHands ↔ GpuPowerCableRoute` Assembly path; generic reserved transfer, checkout, world drop, box, stack, cart, raw Inventory move and receipt-free route bypass stay closed.
- Preserve reservation and parent allocation through pickup, route and unroute. Preserve all ten staging receipts and visible `10/10` history while tracking current custody separately. ATX24 and EPS12V item/container/state/revision/receipt/operation lineages remain exact-routed.
- Reuse Issue #63 endpoint, waypoint, orientation, clearance, obstruction, preview-equals-commit, route/unroute and replay rules. This handoff authorizes that existing authority; it neither recreates nor relaxes it.
- Keep route obstruction exclusions narrowly authored to exact installed host geometry required by the canonical route. Foreign colliders, value-equal substitutes and unrelated Assembly geometry remain blockers.
- Commit authoritative custody before changing parent, pose, physics or visibility. Projection failure recovers the same Unity object and stable ItemId to authoritative hands/route pose. Duplicate, ghost and loss counts remain zero.
- Keep generic drop blocked while carrying the reserved PCIe/GPU cable. Keep PSU and GPU unretain/remove paths blocked while PCIe/GPU power is routed. Preserve the single-consumer input boundary across BuildKit pickup, all three cable routes, GPU/PSU interactions and generic carry actions.
- Let a directly focused routed cable win focus arbitration over other routed power cables. This removes fixed cable-priority stealing without allowing multiple consumers to process one input.
- Do not manufacture power-on or electrical simulation. Even with all three canonical power cables routed, the current bounded assembly remains `BuildIncomplete`; power delivery, short-circuit, POST, BIOS/UEFI, OS, drivers and benchmark are subsequent product systems.
- Do not change ProjectSettings. Bind acceptance to exact technical commit/tree, targeted and full XMLs, universal Mac build/native smoke, clean Windows full tests, x64 IL2CPP/only-D3D11 build/runtime, foreground bounded OS input, binary/procedure/evidence hashes and zero scoped process/task/firewall residue.

## Consequences

GarageGraybox r54 lets the player approach the completed BuildKit after all seven components are installed and ATX24 plus EPS12V are routed; take the canonical PCIe/GPU 6+2 cable with `E / Gamepad South`; carry the same instance; open the existing guided PCIe/GPU route with `Mouse Left / Gamepad RT`; rotate only between canonical keyed orientations with `R / Right Shoulder`; commit with `G / Gamepad East`; keep the cable visibly routed; block generic drop and dependent PSU/GPU removal; then focus the routed cable with empty hands and unroute the exact same instance back to `ActorHands`. The BuildKit reports the cable in assembly custody while immutable `10/10` history stays visible and ATX24/EPS12V remain routed.

The automated r54 smoke and PlayMode flow are invariant evidence, not a real-human or physical-device session. Windows foreground `SendInput` proves bounded OS delivery of W/A/S/D, relative mouse and W+D-held simultaneous mouse deltas to the real r54 player window with `human=false`. Input System virtual-gamepad automation is not a physical-gamepad claim; no physical gamepad was available.

Issue #109 completes the canonical ten-part BuildKit-to-installed/routed handoff chain, but it does not complete the product. Electrical power, POST/BIOS/OS/drivers, thermal/benchmark/diagnostics, visual-production quality, retail/service loops, packaging/delivery/settlement, Save/Guardian, staff/customer/world expansion and Steam release work remain.

## Current verification

- Technical source commit `1acba166855efffa906112e2df24b9b5cef550a7`, tree `eb40a392169e5288e29bc59ae75367029cc00f57`; no ProjectSettings change.
- Unity 6000.3.21f1 targeted Mac domain EditMode `87/87`, scene contract EditMode `9/9`, targeted P1 PlayMode `4/4`, full EditMode `752/752` and full PlayMode `156/156`; failed, skipped and inconclusive `0`.
- Universal macOS Development build report `330,366,591` bytes across `302` files. The deep/strict-valid `x86_64 + arm64` executable emits one r54 readiness marker and one exact PCIe/GPU Assembly success marker on Apple M1/Metal, reaches Input System shutdown, exits `0` and leaves no player residue.
- Exact-head technical Repository Guard [33054757532](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33054757532) passed.
- A clean Windows validation clone was created locally from the exact clean Windows canonical checkout; both source and clone remained detached at the exact technical commit/tree with empty status.
- Windows Unity 6000.3.21f1 full EditMode `752/752` and full PlayMode `156/156` passed. The x64 IL2CPP/only-D3D11 build report is `1,349,053,878` bytes with fatal-token count `0` and byte-exact `ProjectSettings.asset` restoration.
- Intel Iris Xe Direct3D 11.0 feature level 11.1 interactive runtime has exact host/readiness/success counts `1/1/1`, forbidden count `0`, exit `0`, graceful shutdown, deleted task and validation-owned residue `0`.
- Accepted foreground Session 2 OS input delivers W/A/S/D down/up `1/1`, relative mouse `18/18`, initial W+D+mouse `3/3` and further held-key mouse deltas `30/30`; exact player foreground is rechecked around each stage and all eight screenshots have nonzero, unique hashes. Runtime forbidden count is `0`.
- Windows final audit schema `pcshop-issue109-windows-final-audit-v1` passes all `28` checks. Two exact-path development-player TCP/UDP Query User rules were removed after exact program-path verification; process, scheduled-task and firewall-rule residue are `0`.
- Windows evidence returned to the Mac as a `4,599,837`-byte tar with SHA-256 `924792e2c4dd239e8b5209b9f8eaed8b8d248a9ca93cfe597d39450785db74e4`; all `30/30` transported manifest evidence files match bytes and SHA-256, while the three native-binary records and two self-referential final audit/manifest files independently match.
- The temporary Windows validation clone/build/evidence root was removed only after Mac readback passed. Final Windows cleanup reports process/task/firewall residue `0/0/0`.
- No Windows removable volume or USB disk was identified during acceptance. After acceptance, the user's USB was safely ejected from the Mac, connected to Windows and discovered read-only as `D:`, label `cixanla`, exFAT, Intenso Alu Line, serial `900B00076010`, `Healthy/OK`. Issue #109 did not write to it and makes no USB checkpoint claim.
- Real-human, physical keyboard, physical gamepad and endurance certification remain explicitly deferred to the final Steam 1.0 release gate.

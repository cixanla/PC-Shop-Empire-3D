# ADR-0054 — Canonical Motherboard BuildKit-to-Chassis Assembly Handoff

**Status:** Accepted for source/domain/scene/input/full-regression, technical Repository Guard and exact-head macOS + detached-clean Windows native gates; source/docs CI, immutable-package, human-session, healthy physical-USB and administrative gates pending
**Date:** 26 August 2026
**Scope:** Issue #89, child of Epic #10

## Context

Issues #68, #71, #73, #75, #77, #79, #81, #83, #85 and #87 stage the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device, processor cooler, graphics card, power supply, ATX24 cable, EPS12V cable and PCIe/GPU 6+2 cable into ten component-specific BuildKit slots. That chain completes the physical preparation history as canonical `10/10`; it does not install any component in a chassis.

Issues #53 and #54 already own motherboard seating, detaching and fastener secure/unsecure semantics. The missing boundary is a reservation-safe custody bridge from the accepted work order's completed BuildKit to those existing Assembly authorities. A second Inventory, shadow Assembly state or regenerated motherboard identity would split authority and permit duplicate, lost or value-equivalent replacement objects.

## Decision

- Resolve the motherboard only from the exact owned work order, canonical motherboard line and full `LineId`, `ProductId`, `ItemInstanceId`, `ReservationId` and parent allocation receipt tuple. Ordinal, display-name, component-only or value-equivalent matches cannot acquire authority.
- Require the authoritative historical ten-receipt staged aggregate before handoff. Preserve all ten original staging receipts and visible `10/10` completion history while separately validating the motherboard's current physical custody.
- Use a new stable handoff operation identity that is distinct from BuildKit staging, Assembly attach and fastener operations. Immediate and delayed replay return the same immutable handoff receipt without a second custody or revision change.
- Add only the narrowly registered BuildKit → `ActorHands` release and exact Assembly Workbench ↔ `ActorHands` reversible transfer. Keep the live reservation and parent allocation exact throughout release, attach, detach and reseat; do not widen generic reserved transfer or checkout authority.
- Reuse `AssemblyBuildAuthority.AttachMotherboard`, detach, secure and unsecure state/replay rules. The handoff authorizes the existing exact workbench path; it does not recreate or relax keyed orientation, support, obstruction, preview-equals-commit or fastener gates.
- Preserve the same Unity instance and stable serialized ItemId through BuildKit → hands → workbench → seated-unsecured → secured → unsecure → detach → hands → reseat. The other nine BuildKit items, containers, receipts, revisions and projections remain untouched.
- Fail closed with zero mutation for foreign order/operation/line/product/item/reservation/allocation/receipt, value-equal forgery, source drift, stale BuildKit or Inventory revision, revision overflow, full hands and missing, occupied or foreign workbench.
- Commit domain custody before world parent, pose, physics or visibility changes. Projection failure recovers the same instance at its authoritative hands or seat pose; duplicate, ghost and lost-item outcomes are forbidden.
- While the handoff receipt is active, BuildKit pickup, guided seat, fastener and generic drop edges have one deterministic consumer. Range, focus, line of sight, pause, empty-hands, held/co-edge and release/repress rules remain fail closed for keyboard/mouse and gamepad.
- Keep dependent CPU, DIMM, storage, cooler, GPU, PSU and cable installation blocked until the motherboard is secured. This issue installs only the motherboard and does not claim electrical readiness, power-on or POST authority.
- Bind acceptance to the exact technical commit/tree, full test XML, Mac and Windows native artifacts, procedure hashes, task cleanup and immutable package readbacks. Runtime acceptance requires one r45 readiness marker containing `motherboard-assembly-handoff=ready`, one exact success marker, zero assembly-handoff failure markers and zero process/task residue.

## Consequences

GarageGraybox r45 lets the player approach the completed `10/10` BuildKit, take the canonical reserved motherboard with `E / Gamepad South`, move that exact object to the open chassis, use the existing guided seat flow, secure one canonical fastener, then unsecure, detach and reseat the same instance. The BuildKit slot reports `ANAKART MONTAJDA`; immutable ticket identity and the completed preparation history remain visible and unchanged.

The PC is not complete or electrically ready. CPU socket/retention, DIMM latches, M.2 retention, cooler/TIM, GPU/PSU installation, cable routing, electrical validation, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, staff/customer/world expansion and final art remain separate dependent work.

## Current verification

- Technical source commit `2fdf371206bc58c32e1c20d471f4abe7c0bfba01`, tree `c5e6de5942993a98735984caca4a04fd396105f6`.
- Unity 6000.3.21f1 full EditMode `712/712` and full PlayMode `119/119`; failed, skipped and inconclusive `0`.
- macOS Development build report `330,104,684` bytes. The deep/strict-valid universal `x86_64 + arm64` executable on Apple M1/Metal emits one r45 readiness marker and one exact motherboard Assembly handoff success marker, exits gracefully and leaves no player residue.
- The detached-clean Windows Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 `issue89-hardened-v1` build reports `1,340,592,635` bytes. Intel Iris Xe Direct3D 11.0 feature level 11.1 runtime passed at the exact technical commit/tree, with exit `0`, graceful shutdown, deleted scheduled task and zero player/Unity/task residue.
- Technical Repository Guard run `32930403290` passed at the exact technical commit. Source/docs Repository Guard, final source receipt, canonical 14-file evidence, immutable local package, healthy physical-USB lifecycle, exact-r45 human-player and Issue/Project administrative gates remain pending. Automated smoke is not relabelled as human evidence.

# ADR-0051 — Canonical ATX24 Power-Cable Physical Build-Kit Handoff

**Status:** Accepted for source/domain/scene/input/full-regression and exact-head macOS + detached-clean Windows native gates; source/docs CI, immutable-package, human-session and administrative gates pending
**Date:** 26 August 2026
**Scope:** Issue #83, child of Epic #10

## Context

Issues #68, #71, #73, #75, #77, #79 and #81 move the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device, processor cooler, graphics card and power supply into seven component-specific custom-PC BuildKit slots. Issue #83 continues the same accepted work order with the canonical reserved ATX24 split power cable and advances the visible physical ticket from `7/10` to `8/10`.

This handoff is not cable routing. Issue #61 owns the installed-system ATX24 route from the PSU branches to the motherboard endpoint; Issue #60 owns PSU chassis seating and retention. The BuildKit handoff must preserve those authorities unchanged while moving the same serialized cable only from its exact source, through player hands, into an ATX24-specific capacity-one staging tray.

ATX24 is also a cable-family boundary. A product-value-equivalent EPS12V or PCIe/GPU cable, a regenerated item identity, an ordinal match or a display-name match must never acquire ATX24 authority.

## Decision

- Resolve the canonical cable only from the accepted work-order/ticket/allocation `ComponentKind.Atx24PowerCable` line and its complete `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple. The exact cable family remains part of the authority decision.
- Append the ATX24 pickup/staged operations without renumbering persisted stages. Use a distinct stable operation identity and an ATX24-specific managed capacity-one BuildKit container.
- Claim all eight BuildKit containers atomically through Inventory Octuple access. Duplicate, foreign, partial, ghost or aliased topology fails before authority creation and produces zero mutation.
- Permit custody only through exact source → `ActorHands` → ATX24 BuildKit. Generic Inventory transfer, world drop, box, stack, cart, Assembly and installed-route paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor, DDR5 memory, M.2 storage, processor cooler, graphics card and power supply staged receipts before ATX24 pickup or placement. Ticket progress derives from authoritative receipts and changes only from `7/10` to `8/10` after the ATX24 commit.
- Commit domain custody before changing world parent, pose, physics or visibility. Physical projection failure recovers the same cable at the authoritative BuildKit pose; duplicate, ghost and lost-item outcomes are forbidden.
- Keep ATX24 BuildKit, ATX24 installed routing, PSU-bay Assembly, EPS12V routing and PCIe/GPU routing as separate authorities. While an ATX24 BuildKit pickup receipt is active, primary, rotate, interact and drop edges have one consumer; receipt-free legacy paths remain available.
- Use one raycastable non-trigger support collider and one exact snap anchor. Preview orientation is keyed `0° ↔ 180°`; decorative guides, identity card and progress text remain non-authoritative and `Ignore Raycast`.
- Validate keyboard/mouse and gamepad input with real Unity Input System edges, including same-frame co-edge arbitration, held input, pause and release/repress behavior. Range, focus, line of sight, empty hands, obstruction and support failures remain fail-closed.
- Drive native smoke through production pickup/carry/rotate/place code and preserve all eight staged receipts, reservation/allocation identity, replay, revision and custody invariants. Teleport assistance is allowed only for prerequisite positioning and must be stated verbatim in the marker; it is not evidence of a real human route.
- Bind evidence to the exact technical commit/tree, full test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes, task cleanup and immutable package readbacks. Runtime acceptance requires one r42 readiness marker, one exact ATX24 success marker, zero failure markers and zero process/task residue.

## Consequences

GarageGraybox r42 lets the player take the exact reserved ATX24 split cable, carry and rotate that same physical object and place it into a dedicated ATX24 BuildKit tray after the first seven components are staged. Reservation/allocation identity stays live and the ticket visibly becomes `8/10`. The cable is not connected to the PSU or motherboard and no waypoint of the installed ATX24 route is committed.

EPS12V and PCIe/GPU cable handoffs remain the two bounded transfers that advance `8/10 → 10/10`. Component installation, cable routing, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, world expansion, final art and Steam release remain separate dependent work.

## Current verification

- Technical source commit `a36d713120283bd106aeca76509756d6dbb1dd30`, tree `2619dd8e1db812c9e3249657a2031a6268492b5a`.
- Unity 6000.3.21f1 focused domain `39/39`, focused r42 scene `9/9`, full EditMode `701/701` and full PlayMode `110/110`; failed, skipped and inconclusive `0`.
- macOS Development build report `329,963,160` bytes. Apple M1/Metal at 1280×720 emits one r42 readiness marker and one exact ATX24 success marker, exits gracefully and leaves no player residue.
- The native marker truthfully records `prerequisite-positioning=teleport-assisted`; no `human-route=ok`, `no-teleport=ok` or equivalent claim is made.
- The detached-clean Windows Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 `issue83-hardened-v1` build and Intel Iris Xe Direct3D 11.0 feature level 11.1 runtime passed at the exact technical commit/tree, with exit `0`, graceful shutdown, deleted scheduled task and zero player/Unity/task residue.
- Canonical final source receipt, source/docs CI, immutable local/physical-USB, physical metadata, exact-r42 human-player and Issue/Project administrative gates remain pending at this ADR revision. Automated smoke is not relabelled as human evidence.

# ADR-0052 — Canonical EPS12V Power-Cable Physical Build-Kit Handoff

**Status:** Accepted for source/domain/scene/input/full-regression and exact-head macOS + detached-clean Windows native gates; source/docs CI, immutable-package, human-session, physical-USB and administrative gates pending
**Date:** 26 August 2026
**Scope:** Issue #85, child of Epic #10

## Context

Issues #68, #71, #73, #75, #77, #79, #81 and #83 move the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device, processor cooler, graphics card, power supply and ATX24 cable into eight component-specific custom-PC BuildKit slots. Issue #85 continues the same accepted work order with the canonical reserved EPS12V CPU power cable and advances the visible physical ticket from `8/10` to `9/10`.

This handoff is not cable routing. Issue #62 owns the installed-system EPS12V route between PSU and motherboard CPU-power endpoints; Issue #60 owns PSU chassis seating and retention. The BuildKit handoff preserves those authorities unchanged while moving the same serialized cable only from its exact world source, through player hands, into an EPS12V-specific capacity-one staging tray.

EPS12V is a cable-family boundary. A product-value-equivalent ATX24 or PCIe/GPU cable, a regenerated item identity, an ordinal match or a display-name match must never acquire EPS12V BuildKit authority.

## Decision

- Resolve the canonical cable only from the accepted work-order/ticket/allocation `ComponentKind.PowerCable` line whose exact family is `PowerCableType.ModularEps12v8PinPsuToMotherboard`, and verify its complete `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Append the EPS12V pickup/staged operations without renumbering persisted stages. Use a distinct stable operation identity and an EPS12V-specific managed capacity-one BuildKit container.
- Claim all nine BuildKit containers atomically through Inventory Nonuple access. Duplicate, foreign, partial, ghost or aliased topology fails before authority creation and produces zero mutation.
- Permit custody only through exact WorldFloor → `ActorHands` → EPS12V BuildKit. Generic Inventory transfer, world drop, box, stack, cart, Assembly and installed-route paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor, DDR5 memory, M.2 storage, processor cooler, graphics card, power supply and ATX24 staged receipts before EPS12V pickup or placement. Ticket progress derives from authoritative receipts and changes only from `8/10` to `9/10` after the EPS12V commit.
- Commit domain custody before changing world parent, pose, physics or visibility. Physical projection failure recovers the same cable at the authoritative BuildKit pose; duplicate, ghost and lost-item outcomes are forbidden.
- Keep EPS12V BuildKit, EPS12V installed routing, ATX24/PCIe routing, PSU-bay Assembly and all component Assembly authorities separate. While an EPS12V BuildKit pickup receipt is active, primary, rotate, interact and drop edges have one consumer; receipt-free legacy paths remain available.
- Use one raycastable non-trigger support collider and one exact snap anchor. Preview orientation is keyed `0° ↔ 180°`; decorative guides, identity card and progress text remain non-authoritative and `Ignore Raycast`.
- Validate keyboard/mouse and gamepad input with real Unity Input System edges, including same-frame co-edge arbitration, held input, pause and release/repress behavior. Range, focus, line of sight, empty hands, obstruction and support failures remain fail-closed.
- Drive native smoke through production pickup/carry/rotate/place code and preserve all nine staged receipts, reservation/allocation identity, replay, revision and custody invariants. Teleport assistance is allowed only for prerequisite positioning and must be stated verbatim in the marker; it is not evidence of a real human route.
- Bind evidence to the exact technical commit/tree, full test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes, task cleanup and immutable package readbacks. Runtime acceptance requires one r43 readiness marker containing `eps12v-power-cable-build-kit=ready`, one exact EPS12V success marker, zero failure markers and zero process/task residue.

## Consequences

GarageGraybox r43 lets the player take the exact reserved EPS12V 8-pin cable, carry and rotate that same physical object and place it into a dedicated EPS12V BuildKit tray after the first eight parts are staged. Reservation/allocation identity stays live and the ticket visibly becomes `9/10`. The cable is not connected to the PSU or motherboard and no waypoint of the installed EPS12V route is committed.

The PCIe/GPU 6+2 cable handoff remains the final bounded transfer that advances `9/10 → 10/10`. Component installation, cable routing, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, world expansion, final art and Steam release remain separate dependent work.

## Current verification

- Technical source commit `b6a74e932f4744b17388df7c7eb4d88f26d195f4`, tree `bd763ea0c8c6d2f5d256e467c4fca8b762ca4d84`.
- Unity 6000.3.21f1 full EditMode `705/705` and full PlayMode `115/115`; failed, skipped and inconclusive `0`.
- macOS Development build report `330,018,708` bytes. The deep/strict-valid universal `x86_64 + arm64` executable on Apple M1/Metal at 1280×720 emits one r43 readiness marker and one exact EPS12V success marker, exits gracefully and leaves no player residue.
- The native marker truthfully records `prerequisite-positioning=teleport-assisted`; no `human-route=ok`, `no-teleport=ok` or equivalent claim is made.
- The detached-clean Windows Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 `issue85-hardened-v1` build reports `1,338,310,618` bytes. Intel Iris Xe Direct3D 11.0 feature level 11.1 runtime passed at the exact technical commit/tree, with exit `0`, graceful shutdown, deleted scheduled task and zero player/task residue.
- Canonical final source receipt, source/docs Repository Guard, immutable local package, healthy physical-USB lifecycle, exact-r43 human-player and Issue/Project administrative gates remain pending at this ADR revision. Automated smoke is not relabelled as human evidence.

# ADR-0053 — Canonical PCIe/GPU Power-Cable Physical Build-Kit Handoff

**Status:** Accepted for source/domain/scene/input/full-regression, technical source CI and exact-head macOS + detached-clean Windows native gates; source/docs CI, immutable-package, human-session, physical-USB and administrative gates pending
**Date:** 26 August 2026
**Scope:** Issue #87, child of Epic #10

## Context

Issues #68, #71, #73, #75, #77, #79, #81, #83 and #85 move the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device, processor cooler, graphics card, power supply, ATX24 cable and EPS12V cable into nine component-specific custom-PC BuildKit slots. Issue #87 continues the same accepted work order with the canonical reserved PCIe/GPU 6+2 power cable and advances the visible physical ticket from `9/10` to `10/10`.

This handoff is not cable routing. Issue #63 owns the installed-system PCIe/GPU route between PSU and graphics-card endpoints; Issues #59 and #60 own GPU and PSU chassis seating/retention. The BuildKit handoff preserves those authorities unchanged while moving the same serialized cable only from its exact world source, through player hands, into a PCIe/GPU-specific capacity-one staging tray.

PCIe/GPU is the final cable-family boundary in the immutable ten-line work order. A product-value-equivalent ATX24 or EPS12V cable, a regenerated item identity, an ordinal match or a display-name match must never acquire PCIe/GPU BuildKit authority.

## Decision

- Resolve the canonical cable only from the accepted work-order/ticket/allocation `ComponentKind.PowerCable` line whose exact family is `PowerCableType.ModularPcie8PinPsuToGraphicsCard`, and verify its complete `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Append the PCIe/GPU pickup/staged operations without renumbering persisted stages. Use a distinct stable operation identity and a PCIe/GPU-specific managed capacity-one BuildKit container.
- Claim all ten BuildKit containers atomically through Inventory Decuple access. Duplicate, foreign, partial, ghost or aliased topology fails before authority creation and produces zero mutation.
- Permit custody only through exact WorldFloor → `ActorHands` → PCIe/GPU BuildKit. Generic Inventory transfer, world drop, box, stack, cart, Assembly and installed-route paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor, DDR5 memory, M.2 storage, processor cooler, graphics card, power supply, ATX24 and EPS12V staged receipts before PCIe/GPU pickup or placement. Ticket progress derives from authoritative receipts and changes only from `9/10` to `10/10` after the PCIe/GPU commit.
- Commit domain custody before changing world parent, pose, physics or visibility. Physical projection failure recovers the same cable at the authoritative BuildKit pose; duplicate, ghost and lost-item outcomes are forbidden.
- Keep PCIe/GPU BuildKit, PCIe/GPU installed routing, ATX24/EPS12V routing, GPU/PSU Assembly and all other component Assembly authorities separate. While a PCIe/GPU BuildKit pickup receipt is active, primary, rotate, interact and drop edges have one consumer; receipt-free legacy paths remain available.
- Use one raycastable non-trigger support collider and one exact snap anchor. Preview orientation is keyed `0° ↔ 180°`; decorative guides, identity card and progress text remain non-authoritative and `Ignore Raycast`.
- Validate keyboard/mouse and gamepad input with real Unity Input System edges, including same-frame co-edge arbitration, held input, pause and release/repress behavior. Range, focus, line of sight, empty hands, obstruction and support failures remain fail-closed.
- Drive native smoke through production pickup/carry/rotate/place code and preserve all ten staged receipts, reservation/allocation identity, replay, revision and custody invariants. Teleport assistance is allowed only for prerequisite positioning and must be stated verbatim in the marker; it is not evidence of a real human route.
- Bind evidence to the exact technical commit/tree, full test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes, task cleanup and immutable package readbacks. Runtime acceptance requires one r44 readiness marker containing `pcie-gpu-power-cable-build-kit=ready`, one exact PCIe/GPU success marker, zero failure markers and zero process/task residue.

## Consequences

GarageGraybox r44 lets the player take the exact reserved modular PCIe/GPU 8-pin 6+2 cable, carry and rotate that same physical object and place it into a dedicated PCIe/GPU BuildKit tray after the first nine parts are staged. Reservation/allocation identity stays live and the ticket visibly becomes `10/10`. The cable is not connected to the PSU or graphics card and no waypoint of the installed PCIe/GPU route is committed.

The immutable ten-line BuildKit collection is now physically staged, but the PC is not assembled or electrically ready. Component installation, cable routing, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, world expansion, final art and Steam release remain separate dependent work.

## Current verification

- Technical source commit `25dc39ab02de93a416800acd17f53aacf83dca09`, tree `a736a764d0a52e950a4139002d6febc629df5987`.
- Unity 6000.3.21f1 full EditMode `709/709` and full PlayMode `116/116`; failed, skipped and inconclusive `0`.
- macOS Development build report `330,073,048` bytes. The deep/strict-valid universal `x86_64 + arm64` executable on Apple M1/Metal emits one r44 readiness marker and one exact PCIe/GPU success marker, exits gracefully and leaves no player residue.
- The native marker truthfully records `prerequisite-positioning=teleport-assisted`; no `human-route=ok`, `no-teleport=ok` or equivalent claim is made.
- The detached-clean Windows Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 `issue87-hardened-v1` build reports `1,339,592,274` bytes. Intel Iris Xe Direct3D 11.0 feature level 11.1 runtime passed at the exact technical commit/tree, with exit `0`, graceful shutdown, deleted scheduled task and zero player/task residue.
- Technical source Repository Guard run `32921526334` passed at the exact technical commit. Canonical final source receipt, source/docs Repository Guard, immutable local package, healthy physical-USB lifecycle, exact-r44 human-player and Issue/Project administrative gates remain pending at this ADR revision. Automated smoke is not relabelled as human evidence.

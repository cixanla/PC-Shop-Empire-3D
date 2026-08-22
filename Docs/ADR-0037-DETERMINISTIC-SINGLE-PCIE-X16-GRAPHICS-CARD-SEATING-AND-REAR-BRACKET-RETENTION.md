# ADR-0037 — Deterministic Single PCIe x16 Graphics-Card Seating and Rear-Bracket Retention

**Status:** Accepted and implemented<br>
**Date:** 22 August 2026<br>
**Scope:** Issue #59, child of Epic #10

## Context

The physical PC assembly line needed its first discrete graphics card after the secured motherboard, retained CPU, retained DIMM, secured M.2 drive and retained processor-cooler gates. The bounded slice had to make PCIe seating and chassis retention physically readable without introducing a second Inventory authority, physics-driven attachment, a shadow retail SKU, cable routing, power delivery, completed benchmark scoring or final art.

## Decision

- Add append-only `GraphicsCard` catalog classification and typed `Pcie4X16FullHeightDualSlot` metadata. The assembly card reuses the canonical Northstar A60 product identity but has its own exact serialized item identity; no shadow SKU is created.
- Claim Workbench, ProcessorSocket, MemorySlot, StorageSlot, ProcessorCoolerSlot and capacity-one GraphicsCardSlot atomically in one Inventory revision; partial or ghost claims fail closed.
- Model one immutable motherboard-owned PCIe x16 slot, latch, rear bracket and captive bracket-fastener topology.
- Model the reversible runtime sequence `EmptyOpen ↔ GraphicsCardSeatedUnsecured ↔ GraphicsCardRetained`. Seat/remove transfers the exact serialized item between Hands and GraphicsCardSlot. Retain/unretain does not mutate Inventory custody or revision.
- Require the primary keyed orientation, secured host, exact PCIe x16 interface, insertion support, chassis clearance, processor-cooler clearance and obstruction-free insertion. Range, focus, LOS, tie and fixed NonAlloc saturation fail closed.
- Preserve exact seat/retain/unretain/remove receipt lineage and delayed replay. Duplicate seating, retained removal, stale/conflicting operations and motherboard detachment while any graphics card is installed are no-mutation failures.
- Keep `PlayerCarryController` as the only input owner. Keyboard/mouse and gamepad use the existing actions and dynamic prompts; generic placement, stacking and cart paths cannot consume a held graphics card.
- Bind presentation to one stable Unity instance. Failed physical projection is compensated or recovered to the exact last-safe pose without duplicating or losing the item.
- GarageGraybox r28 shows a readable dual-fan Northstar A60 card with PCB, PCIe contacts, rear bracket, slot latch and visible bracket screw.

## Consequences

The game now has a visible, reversible PCIe graphics-card installation loop with stable item identity, authoritative custody, deterministic slot retention and an installed-card host-detach gate. A retained card satisfies only the bounded graphics-card readiness requirement; the PC remains `BuildIncomplete`. PSU, PCIe power cabling, alternate card sizes/slots, risers, multi-GPU, POST/BIOS/OS, completed benchmark scoring, final art/audio/VFX/UI and native Windows/Steam validation remain separate gates.

## Verification

- EditMode: 548/548.
- Real Input System PlayMode: 43/43.
- Universal macOS Development/StrictMode build: 328,781,520 bytes.
- Active Apple Silicon/Metal workstation (Apple M1), 1280×720: r28 readiness and exact `GARAGE_GPU_RUNTIME_SMOKE` passed.
- Feature commit: `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`; tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`.

# ADR-0038 — Deterministic Single ATX PS/2 Power-Supply Seating and Four-Screw Retention

**Status:** Accepted and implemented<br>
**Date:** 23 August 2026<br>
**Scope:** Issue #60, child of Epic #10

## Context

The physical PC assembly line needed one chassis-owned power supply after the secured motherboard, retained CPU, retained DIMM, secured M.2 drive, retained processor cooler and retained graphics-card gates. The bounded slice had to make ATX PS/2 seating and rear retention physically readable without introducing cable routing, electrical power delivery, wattage simulation, completed benchmark scoring, a second Inventory authority, physics-driven attachment or final art.

## Decision

- Add append-only `PowerSupply` catalog classification and typed `AtxPs2` metadata. Use one canonical fictional product and one exact serialized assembly item; no shadow SKU or second authority is created.
- Claim Workbench, ProcessorSocket, MemorySlot, StorageSlot, ProcessorCoolerSlot, GraphicsCardSlot and capacity-one PowerSupplyBay atomically in one Inventory revision; partial or ghost claims fail closed.
- Model one immutable chassis-owned ATX PS/2 bay, rear mount and four distinct rear fasteners. Retention uses one stable crossed order and release uses its exact reverse.
- Model the reversible runtime sequence `EmptyOpen ↔ PowerSupplySeatedUnsecured ↔ PowerSupplyRetained`. Seat/remove transfers the exact serialized item between Hands and PowerSupplyBay. Retain/unretain does not mutate Inventory custody or revision.
- Preserve exact seat/retain/unretain/remove receipt lineage, receipt-history fold and immediate/delayed replay. Duplicate seating, retained removal, stale/conflicting operations, occupied bay and full-hands removal are no-mutation failures.
- Keep the PSU chassis-owned: its assembly order does not force motherboard, CPU, DIMM, storage, cooler or graphics-card order, and unrelated authority revisions remain isolated.
- Require the exact ATX PS/2 interface, one of two keyed 180-degree fan-intake orientations, full filtered-floor support, rear-plane alignment and obstruction-free seating. Range, focus, LOS, tie and fixed NonAlloc saturation fail closed.
- Bind production chassis clearance to exactly `ChassisBack`, `ChassisLeftRail`, `ChassisRightRail` and `MotherboardTray`. `ChassisBase` and `PowerSupplyFilteredFloorIntake` are support/reference surfaces rather than blockers. Cable blockers remain empty because cable routing is outside Issue #60.
- Keep `PlayerCarryController` as the only input owner. Keyboard/mouse and gamepad use the existing actions and dynamic compact prompts; generic placement, stacking and cart paths cannot consume a held PSU.
- Bind presentation to one stable Unity instance. Failed physical projection is compensated or recovered to the exact last-safe pose without duplicating or losing the item.
- GarageGraybox r29 shows a readable fictional ATX PSU housing, fan and grille, filtered floor intake, AC inlet, rocker switch, disconnected modular panel, rear plate and four rear screws.

## Consequences

The game now has a visible, reversible ATX PSU installation loop with stable item identity, authoritative custody, deterministic rear retention and explicit clearance failures. A retained PSU satisfies only the bounded PSU-readiness requirement; the PC remains `BuildIncomplete`. ATX 24-pin, EPS/CPU, PCIe/GPU and storage/fan cabling, electrical power-on, POST/BIOS/OS, wattage/headroom, completed benchmark scoring, final art/audio/VFX/UI and native Windows/Steam validation remain separate gates.

## Verification

- EditMode: 577/577.
- Real Input System PlayMode: 47/47.
- Universal macOS Development/StrictMode build: 328,937,592 bytes.
- Active Apple Silicon/Metal workstation (Apple M1), 1280×720: r29 readiness and exact `GARAGE_PSU_RUNTIME_SMOKE` passed.
- Feature commit: `f998d7d1c400c9328afa226f0727e6591c02d4e2`; tree `78d62c46354cda45422ca947df10ba9d6823b7c9`.
- Authored-clearance fix: `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`; tree `a15865346f52b6b39d84cec49c70babbc6550b89`.

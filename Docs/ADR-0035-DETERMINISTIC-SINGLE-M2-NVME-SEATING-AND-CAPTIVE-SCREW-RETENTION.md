# ADR-0035 — Deterministic Single M.2 NVMe Seating and Captive Screw Retention

**Status:** Accepted and implemented<br>
**Date:** 16 August 2026<br>
**Scope:** Issue #57, child of Epic #10

## Context

The physical PC assembly line needed one storage component without introducing a second inventory authority, physics-driven attachment, SATA/RAID scope, or a completed-PC benchmark. The slice must preserve the stable-item, authoritative-custody, exact-replay and fail-closed contracts already established by motherboard, CPU and DIMM assembly.

## Decision

- Add append-only `StorageDevice` catalog classification and one typed `NvmePcie4X4_2280` product.
- Claim Workbench, ProcessorSocket, MemorySlot and capacity-one StorageSlot atomically in one Inventory revision.
- Model one immutable M-key/2280 topology and the reversible state `EmptyOpen ↔ StorageDeviceSeatedUnsecured ↔ StorageDeviceSecured`.
- Seat/remove transfers the exact serialized item between Hands and StorageSlot. Secure/unsecure operates the motherboard-owned captive screw and does not mutate Inventory custody or revision.
- Preserve immutable seat/secure/unsecure/remove receipts, exact lineage, immediate and delayed replay, cross-kind conflict rejection, installed-storage motherboard detach gate and same-instance recovery.
- Use a deterministic two-stage visual contract: an 18-degree guided insertion pose and a flat seated pose. The solver requires range, focus, LOS, keyed orientation, support and obstruction clearance; fixed NonAlloc saturation fails closed.
- Keep `PlayerCarryController` as the only input owner. Keyboard/mouse and gamepad use existing actions and dynamic prompts; generic placement, stacking and cart paths cannot consume a held SSD.
- GarageGraybox r26 shows one readable semi-realistic 2280 PCB with controller, NAND, label, gold M-key contacts, connector, standoff and captive screw. Presentation never becomes authority.

## Consequences

The game now has a visible, reversible first storage-installation loop whose item identity and custody remain authoritative. A secured SSD only satisfies the storage portion of benchmark readiness; the PC remains `BuildIncomplete`. A second drive/slot, SATA, RAID, heatsink/thermal behavior, performance scoring, POST/BIOS/OS, final art and Windows/Steam validation remain separate gates.

## Verification

- EditMode: 490/490.
- Real Input System PlayMode: 35/35.
- Universal macOS Development/StrictMode build: 328,362,356 bytes.
- Apple M4/Metal 1280x720: r26 readiness and exact `GARAGE_STORAGE_RUNTIME_SMOKE` passed.
- Feature commit: `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`; tree `1aedb833983df256c500c6a1815b075fa29c254c`.

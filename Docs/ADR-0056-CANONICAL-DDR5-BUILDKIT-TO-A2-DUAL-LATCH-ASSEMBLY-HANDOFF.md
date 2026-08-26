# ADR-0056 — Canonical DDR5 BuildKit-to-A2 Dual-Latch Assembly Handoff

**Status:** Accepted for source/domain/scene/input/full-regression and exact-head macOS native gates; Windows, source/docs CI, immutable-package, human-session, healthy physical-USB and administrative gates pending
**Date:** 26 August 2026
**Scope:** Issue #93, child of Epic #10

## Context

Issues #68 through #87 stage the exact ten reserved custom-PC components into component-specific capacity-one BuildKit slots. Issue #89 moves the same reserved motherboard into the existing chassis Assembly authority and secures it; Issue #91 then moves the same reserved CPU into the existing ProcessorSocket and leaves retention closed. The original ten staging receipts remain the immutable preparation history even after current custody moves into Assembly.

Issue #56 already owns DDR5 family validation, A2/Channel A/Bank 2 topology, keyed notch orientation, capacity-one seat/remove and deterministic dual-latch close/open semantics. The missing boundary is a reservation-safe custody bridge from the canonical DDR5 BuildKit slot to that existing MemorySlot authority. A second Inventory, shadow slot/latch state, regenerated DIMM identity or receipt-free shortcut would split authority and permit duplicate, lost or value-equivalent replacement objects.

## Decision

- Resolve the DIMM only from the exact owned work order, canonical `MemoryModule` line and full `LineId`, `ProductId`, `ItemInstanceId`, `ReservationId`, parent allocation and original staging-receipt tuple. Ordinal, display-name, component-only and value-equivalent matches cannot acquire authority.
- Require the authoritative historical ten-receipt staged aggregate plus the live Issue #89 and #91 Assembly chain. The exact motherboard must remain in Inventory Workbench custody and Assembly `SeatedSecured`; the exact CPU must remain in ProcessorSocket custody and Assembly `ProcessorRetained`. Their source receipts must be live and exact before DDR5 release.
- Require the configured exact A2/Channel A/Bank 2/population-priority-1 `MemorySlot` to be managed, capacity one, foreign-container-free and `EmptyOpen`. Full hands, occupied/retained slot, stale authority and revision overflow fail closed before mutation.
- Use a new stable DDR5 assembly-handoff operation identity distinct from staging, motherboard handoff, CPU handoff, DIMM seat and latch operations. Immediate and delayed replay return the same immutable receipt without a second custody or revision change.
- Add only the narrowly registered DDR5 BuildKit → `ActorHands` release. Subsequent reversible movement uses the existing exact Assembly-owned `MemorySlot` ↔ `ActorHands` authority; generic reserved transfer and checkout rules remain closed.
- Preserve live reservation and parent allocation through pickup, seat, latch close/open, remove and reseat. Preserve all ten original staging receipts and visible `10/10` history while current custody is tracked separately.
- Reuse `AssemblyBuildAuthority.SeatMemoryModule`, dual-latch close/open and remove state/replay rules. The handoff authorizes the existing Issue #56 path; it does not recreate or relax family, topology, keyed orientation, obstruction, preview-equals-commit or latch gates.
- Preserve the same Unity instance and stable serialized ItemId through BuildKit → hands → A2 → retained → open → detach → hands → reseat. The motherboard stays secured, the CPU stays retained and the other seven uninstalled BuildKit components, containers, receipts, revisions and projections remain untouched.
- Fail closed with zero mutation for foreign order/operation/line/product/item/reservation/allocation/staging/motherboard/CPU/target, value-equal forgery, source drift, stale BuildKit/Inventory/Assembly revision, full hands, occupied slot and overflow.
- Commit domain custody before world parent, pose, physics or visibility changes. Projection failure recovers the same instance at its authoritative hands or exact A2 pose; duplicate, ghost and lost-item outcomes are forbidden.
- While the handoff receipt is active, BuildKit pickup, DIMM seat, latch and generic drop edges have one deterministic consumer. Range, focus, line of sight, pause, empty-hands, held/co-edge and release/repress rules remain fail closed for keyboard/mouse and gamepad.
- Keep motherboard detach/unsecure blocked while DIMM is seated or retained, keep DIMM remove blocked while latches are closed, and keep the exact CPU retained throughout this slice.
- Bind acceptance to the exact technical commit/tree, full test XML, Mac and Windows native artifacts, procedure hashes, cleanup and immutable package readbacks. Runtime acceptance requires one r47 readiness marker containing `memory-module-assembly-handoff=ready`, one exact success marker, zero handoff-failure markers and zero process/task residue.

## Consequences

GarageGraybox r47 lets the player approach the completed `10/10` BuildKit after the motherboard is secured and CPU retained, take the canonical DDR5 UDIMM with `E / Gamepad South`, carry that exact object, open the existing keyed DIMM seat mode with `Mouse Left / Gamepad RT`, commit the notch-aligned A2 seat with `G / Gamepad East`, close and reopen both latches, detach and reseat the same instance. The BuildKit slot reports `DDR5 MONTAJDA`; immutable ticket identity and completed preparation history remain visible and unchanged.

The PC is not complete or electrically ready. M.2, cooler/TIM, GPU, PSU and cable installation, electrical validation, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, staff/customer/world expansion and final art remain separate dependent work.

## Current verification

- Technical source commit `0caca090d2859dfb78219abb089274fe599eaca2`, tree `e52c75872a8ec59a98b63c0c46d5e3f6f9c5e084`.
- Unity 6000.3.21f1 full EditMode `718/718` and full PlayMode `125/125`; failed, skipped and inconclusive `0`.
- macOS Development build report `330,173,019` bytes. The deep/strict-valid universal `x86_64 + arm64` executable emits one r47 readiness marker and one exact DDR5 Assembly handoff success marker, reaches Input System shutdown, exits `0` and leaves no player residue.
- Local Repository Guard passed before the technical commit; exact-head GitHub Repository Guard and detached-clean Windows IL2CPP/D3D11 gates are pending at this document revision.
- Source/docs Repository Guard, final source receipt and canonical `14/14`, immutable local package, healthy physical-USB lifecycle, exact-r47 human-player and Issue/Project administrative gates remain pending. Automated smoke is not relabelled as human evidence.

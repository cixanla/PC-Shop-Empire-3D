# ADR-0055 — Canonical Processor BuildKit-to-Socket Retention Handoff

**Status:** Accepted for source/domain/scene/input/full-regression, technical Repository Guard and exact-head macOS plus detached-clean Windows native gates; source/docs CI, immutable-package, human-session, healthy physical-USB and administrative gates pending
**Date:** 26 August 2026
**Scope:** Issue #91, child of Epic #10

## Context

Issues #68, #71, #73, #75, #77, #79, #81, #83, #85 and #87 stage the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device, processor cooler, graphics card, power supply, ATX24 cable, EPS12V cable and PCIe/GPU 6+2 cable into ten component-specific BuildKit slots. Issue #89 then moves the same reserved motherboard from that historical `10/10` preparation state into the existing chassis Assembly authority and leaves it `SeatedSecured`.

Issue #55 already owns processor socket family, keyed orientation, capacity-one seat/remove and retention open/close semantics. The missing boundary is a reservation-safe custody bridge from the canonical CPU BuildKit slot to that existing socket authority. A second Inventory, shadow socket state, regenerated processor identity or receipt-free shortcut would split authority and permit duplicate, lost or value-equivalent replacement objects.

## Decision

- Resolve the processor only from the exact owned work order, canonical CPU line and full `LineId`, `ProductId`, `ItemInstanceId`, `ReservationId`, parent allocation and original staging-receipt tuple. Ordinal, display-name, component-only or value-equivalent matches cannot acquire authority.
- Require the authoritative historical ten-receipt staged aggregate and the live Issue #89 motherboard handoff. The exact motherboard must remain in Inventory Workbench custody and Assembly `SeatedSecured`; attach and secure source receipts must be live and exact before CPU release.
- Require the configured exact capacity-one `ProcessorSocket` to be managed, foreign-container-free and `EmptyOpen`. Full hands, occupied/retained socket, stale authority and revision overflow fail closed before any mutation.
- Use a new stable CPU handoff operation identity distinct from BuildKit staging, motherboard handoff, processor seat and retention operations. Immediate and delayed replay return the same immutable receipt without a second custody or revision change.
- Add only the narrowly registered Processor BuildKit → `ActorHands` release. Subsequent reversible movement uses the existing exact Assembly-owned `ProcessorSocket` ↔ `ActorHands` authority; generic reserved transfer and checkout rules remain closed.
- Preserve the live reservation and parent allocation through pickup, seat, retain, open, remove and reseat. Preserve all ten original staging receipts and visible `10/10` history while separately tracking current custody.
- Reuse `AssemblyBuildAuthority.SeatProcessor`, retention close/open and remove state/replay rules. This handoff authorizes the existing socket path; it does not recreate or relax family, keyed orientation, obstruction, preview-equals-commit or retention gates.
- Preserve the same Unity instance and stable serialized ItemId through BuildKit → hands → socket → retained → open → detach → hands → reseat. The motherboard stays secured and the other eight uninstalled BuildKit components, containers, receipts, revisions and projections remain untouched.
- Fail closed with zero mutation for foreign order/operation/line/product/item/reservation/allocation/staging/motherboard-handoff/target, value-equal forgery, source drift, stale BuildKit/Inventory/Assembly revision, full hands, occupied socket and revision overflow.
- Commit domain custody before world parent, pose, physics or visibility changes. Projection failure recovers the same instance at its authoritative hands or socket pose; duplicate, ghost and lost-item outcomes are forbidden.
- While the handoff receipt is active, BuildKit pickup, socket seat, retention and generic drop edges have one deterministic consumer. Range, focus, line of sight, pause, empty-hands, held/co-edge and release/repress rules remain fail closed for keyboard/mouse and gamepad.
- Keep motherboard unsecure/detach blocked while the CPU is seated or retained, and keep processor remove blocked while retention is closed.
- Bind acceptance to the exact technical commit/tree, full test XML, Mac and Windows native artifacts, procedure hashes, task cleanup and immutable package readbacks. Runtime acceptance requires one r46 readiness marker containing `processor-assembly-handoff=ready`, one exact success marker, zero processor/assembly handoff failure markers and zero process/task residue.

## Consequences

GarageGraybox r46 lets the player approach the completed `10/10` BuildKit after the motherboard is secured, take the canonical reserved CPU with `E / Gamepad South`, carry that exact object, use the existing keyed processor-seat mode with `Mouse Left / Gamepad RT`, commit the seat with `G / Gamepad East`, close and reopen retention, detach and reseat the same instance. The BuildKit CPU slot reports `CPU MONTAJDA`; immutable ticket identity and completed preparation history remain visible and unchanged.

The PC is not complete or electrically ready. DIMM, storage, cooler/TIM, GPU, PSU and cable installation, electrical validation, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, staff/customer/world expansion and final art remain separate dependent work.

## Current verification

- Technical source commit `003c93f2de191ff3b295a8a88454e74617521970`, tree `1e46049a9a253559b2f9f4ab41524e8be5e0f9ab`.
- Unity 6000.3.21f1 full EditMode `715/715` and full PlayMode `122/122`; failed, skipped and inconclusive `0`.
- macOS Development build report `330,127,900` bytes. The deep/strict-valid universal `x86_64 + arm64` executable on Apple M1/Metal emits one r46 readiness marker and one exact processor Assembly handoff success marker, exits gracefully and leaves no player residue.
- Technical Repository Guard run `32937325469` passed at the exact technical commit.
- Detached-clean Windows `issue91-hardened-v2` at the exact technical commit/tree stayed detached and clean before/after build and runtime. The x64 IL2CPP only-D3D11 build report is `1,342,422,475` bytes; Intel Iris Xe reports Direct3D 11.0 feature level 11.1, emits one exact r46 readiness and one exact processor handoff marker, exits gracefully, deletes the interactive task and leaves zero scoped player/Unity/build-task residue.
- Source/docs Repository Guard, final source receipt and canonical `14/14`, immutable local package, healthy physical-USB lifecycle, exact-r46 human-player and Issue/Project administrative gates remain pending. Automated smoke is not relabelled as human evidence; strict acceptance is `24/25`.

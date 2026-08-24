# ADR-0044 — Canonical Motherboard Physical Build-Kit Handoff

**Status:** Accepted technical design; verified on macOS and Windows; physical USB and Issue/Roadmap lifecycle remain separate closure gates<br>
**Date:** 24 August 2026<br>
**Scope:** Issue #68, child of Epic #10

## Context

Issue #64 froze one accepted ten-line custom-PC quote and its exact serialized reservation set. Issue #66 converted that commercial result into one immutable build order and one physical work ticket without moving or consuming any item. The next playable boundary had to move the canonical reserved motherboard from its exact authoritative source through the player's hands into a dedicated build-kit location, while preserving the reservation/allocation lineage and proving that physical assembly had not started.

An ordinal line, display name, value-equal receipt or regenerated identifier could not be trusted as authority. Likewise, moving the Unity object before the domain commit could create teleport, duplicate, ghost or rollback drift. The handoff therefore required one domain-first custody transaction and one same-instance physical projection spanning source → ActorHands → BuildKit.

## Decision

- Select the motherboard only by exact `ComponentKind.Motherboard` plus the complete work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple. Ordinal order, display name, suffix and regenerated reservation identity are non-authoritative.
- Add stable append-only build-kit operation identity and immutable pickup/placement receipts bound to the exact work order, ticket, allocation and serialized item lineage.
- Add a managed capacity-one `InventoryContainerKind.BuildKit` container distinct from Assembly Workbench and physical motherboard seat custody.
- Permit reserved-item movement only through the narrow work-order-allocation build-kit bridge. Generic raw transfer, world drop, stacking, cart loading and Assembly seat paths cannot bypass active build-kit custody.
- Commit custody in the domain before changing world parent, pose, visibility, physics or carry state. A failed projection mutation rolls back or recovers the exact same Unity object at the authoritative pose; no replacement projection is created.
- Preserve the live reservation and work-order allocation receipt across source → ActorHands → BuildKit. Placement increments build-kit progress from `0/10` to exact `1/10`; Assembly revision, state and receipts remain untouched.
- Require real `E / Gamepad South` pickup and placement input with authored range, focus, line of sight, empty-hands, capacity, obstruction, pose and revision gates. Same-frame Interact/Drop/Primary, held input, pause co-edge and release-repress resolve to one deterministic consumer.
- Keep the motherboard projection's stable `ItemId` and Unity component instance unchanged through pickup, rotation, preview, placement, failure recovery and exact replay.
- Treat build-kit presentation as a view of authoritative custody. Decorative labels and geometry are non-authoritative and cannot steal interaction raycasts.
- Bind promoted Windows evidence to exact technical commit/tree, exact native binary hashes and the hashes of the build, launch and runtime procedures. A GUI runtime is accepted only after Direct3D11 readiness, the exact Issue #68 marker, graceful shutdown, zero forbidden tokens and zero residue.

## Consequences

GarageGraybox r35 now supports the first physical component handoff for an accepted custom-PC job. The player takes the exact reserved motherboard, carries and rotates the same object, and places it into the dedicated build-kit slot. The work ticket advances to `1/10` while the reservation and allocation stay live and the existing Assembly prototype remains unchanged.

The other nine component transfers, `10/10` kit completion, motherboard seating/fastening for this job, electrical readiness, power-on, POST/BIOS, fictional OS and drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian and Steam release remain separate dependent packages.

## Verification

- Feature chain `2a69436` + `b0d2a97`; current technical head `480874191ee2c950e046ab2aee8be92d61d79fe4`, tree `e229788741df4c456840d356633e2a4bc1702516`.
- Exact detached clean-clone full regression on Unity 6000.3.21f1: EditMode `675/675`, PlayMode `73/73`; failed, skipped and inconclusive `0`. The two Unity-generated editor-setting deltas were removed in the dedicated validation clone and post-test HEAD/tree/clean readback passed.
- Universal macOS Development/StrictMode build report `329,571,495` bytes. The `117,179`-byte signed executable contains `x86_64` and `arm64`; Apple M1/Metal r35 readiness and exact build-kit success markers appeared once, failure markers `0`, and Input System shut down.
- Exact detached clean Windows source produced a `1,327,308,678`-byte x64 IL2CPP/Direct3D11 build report. The three native binaries were hash-read back from the binary manifest.
- Logged-on interactive Windows player used Intel Iris Xe / Direct3D 11.0 feature level 11.1. Host, readiness, exact Issue #68 success and shutdown markers each appeared once; forbidden tokens `0`, player/task exit `0`, graceful shutdown true, task deleted and process residue `0`.
- Technical-source [Repository Guard 32744068996](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32744068996) passed for `480874191ee2c950e046ab2aee8be92d61d79fe4`; draft [PR #69](https://github.com/cixanla/PC-Shop-Empire-3D/pull/69) is the integration vehicle.
- Exact hashes, byte counts, commands, Mac/Windows receipts, interactive-task cleanup, the 20-row acceptance map and the canonical `14/14` procedure-bound evidence contract are recorded in `Docs/Evidence/CANONICAL-MOTHERBOARD-PHYSICAL-BUILD-KIT-HANDOFF-CHECKPOINT-2026-08-24.md`. Canonical local evidence source: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue68-4808741`.
- Local immutable staging, physical USB incoming/final double readback, final metadata/Guard, Issue closure and Roadmap Done are deliberately not claimed by this technical ADR until their independent gates pass.

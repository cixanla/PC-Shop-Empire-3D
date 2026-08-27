# ADR-0060 — Canonical Power-Supply BuildKit-to-Bay Four-Fastener Assembly Handoff

**Status:** Source/domain/scene/full-regression, macOS/Windows native and agent-operated foreground Windows OS-input gates passed at exact technical source `740a886`; source/docs PR integration is in progress<br>
**Date:** 27 August 2026<br>
**Scope:** Issue #102, child of Epic #10

## Context

Issues #68 through #87 stage the exact ten reserved custom-PC components in component-specific capacity-one BuildKit slots. Issues #89, #91, #93, #95, #97 and #99 move the same motherboard, CPU, DDR5, M.2, processor cooler and graphics card into their existing Assembly authorities while preserving immutable staging history and live reservation/allocation lineage.

Issue #60 already owns deterministic ATX PS/2 compatibility, two keyed fan orientations, support and clearance checks, guided `PowerSupplyBay` seating, four-fastener retention, retained-removal blocking, reverse unretention, detach and replay semantics. Issue #81 stages the canonical reserved PSU. Issues #61, #62 and #63 separately own ATX24, EPS12V and PCIe/GPU power-cable route authority. The missing boundary is a reservation-safe bridge from exact PowerSupply BuildKit custody into the existing PSU bay after all six live component-installation prerequisites are proven, without starting or mutating any cable route.

A second Inventory, shadow PSU/bay/rear-mount/fastener authority, regenerated identity or implicit electrical state would split truth. A presentation-only pickup would also be invalid: the same serialized Unity instance must move through authoritative custody and recover fail closed after projection failure or current obstruction.

## Decision

- Resolve only the exact owned work order's canonical `PowerSupply` line and complete `LineId`, `ProductId`, serialized `ItemId`, `ReservationId`, parent allocation and original staging-receipt tuple. Ordinal, display-name, component-only and value-equivalent matches cannot acquire authority.
- Require the historical exact ten-receipt `10/10` aggregate plus live exact Issue #89/#91/#93/#95/#97/#99 receipts. Motherboard custody/state must be Workbench + `SeatedSecured`; CPU `ProcessorRetained`; DDR5 exact A2 + `MemoryModuleRetained`; M.2 exact primary slot + `StorageDeviceSecured`; cooler `CoolerRetained`; GPU exact PCIe x16 slot + `GraphicsCardRetained`.
- Require the configured `PowerSupplyBay` to be managed, capacity one, ATX PS/2 compatible, foreign-container-free and `EmptyOpen`, with exact rear mount and four-fastener topology. Full hands, occupied bay, stale authority/revision, wrong prerequisite, invalid orientation, support/rear-plane/clearance failure, current obstruction or overflow fails closed with zero mutation.
- Use a stable PSU assembly-handoff operation identity distinct from staging, every predecessor component handoff, Issue #60 seat/retention operations and Issue #61/#62/#63 cable operations. Immediate and delayed replay return the same immutable receipt without a second custody or revision change.
- Add only the narrowly registered PowerSupply BuildKit → `ActorHands` release. Subsequent reversible transfer remains the existing exact `PowerSupplyBay` ↔ `ActorHands` Assembly path; generic reserved transfer, checkout and world drop stay closed.
- Preserve reservation and parent allocation through pickup, seat, four-fastener retain, unretain and detach. Preserve all ten staging receipts and visible `10/10` history while tracking current custody separately. Installed motherboard/CPU/DDR5/M.2/cooler/GPU states stay live and unchanged.
- Preserve all three cable authorities independently. ATX24, EPS12V and PCIe/GPU cable item/product/container/state/revision/receipt/operation identities remain exact across the reversible PSU flow. Any routed cable continues to block unretain/remove; this handoff never creates a route.
- Reuse Issue #60 compatibility, orientation, support/chassis/rear-plane clearance, obstruction, preview-equals-commit, four-fastener and replay rules. This handoff authorizes that existing authority; it does not recreate or relax it.
- Commit authoritative custody before changing parent/pose/physics/visibility. Projection failure recovers the same Unity object and stable ItemId to authoritative hands/bay pose; duplicate, ghost and loss counts remain zero. Recovery rechecks current obstruction and waits fail closed until the bay is physically clear.
- Keep retained removal blocked. Keep motherboard/GPU and other prerequisite component detach/unretain transitions blocked according to the existing PSU/cable dependency contract.
- Keep BuildKit pickup, guided seat, retention, detach, cable route and generic drop edges single-consumer across keyboard/mouse and gamepad. Range, focus, line of sight, pause/focus neutralization, release/repress and held/co-edge rules remain fail closed.
- Open the physical lower PSU chamber by moving/scaling the authored motherboard tray and status plate, while keeping the PSU bay's assembly-root topology authoritative. Seat the PSU upright at the existing rear-bay coordinate system; do not change ProjectSettings.
- Bind acceptance to exact technical commit/tree, targeted and full test XML, macOS and detached-clean Windows native artifacts, interactive D3D11 runtime, foreground Windows OS-input evidence, binary/procedure hashes and zero scoped residue. Runtime acceptance requires one r51 readiness marker with `power-supply-assembly-handoff=ready`, one exact PSU Assembly success marker and zero failure/fatal markers.

## Consequences

GarageGraybox r51 lets the player approach the completed BuildKit after motherboard, CPU, DDR5, M.2, cooler and GPU are installed; take the canonical PSU with `E / Gamepad South`; carry the same instance; open the existing guided PSU-bay seat with `Mouse Left / Gamepad RT`; rotate only between the two keyed 180° fan orientations with `R / Right Shoulder`; prove invalid orientation is blocked; commit the seat with `G / Gamepad East`; retain the stable four-fastener sequence; prove retained removal is blocked; reverse the fasteners; detach; and reseat the same instance. The BuildKit reports `PSU MONTAJDA` while immutable `10/10` history and all three staged cable records remain visible and unchanged.

The automated r51 smoke explicitly records `prerequisite-setup=assisted`; it is native invariant evidence, not a real-human or physical-device session. Windows foreground `SendInput` proves bounded OS delivery of W/A/S/D, relative mouse and W+D-held simultaneous mouse deltas to the real r51 player window, with `human=false`. Input System gamepad automation is not a physical gamepad claim.

The PC is still not electrically ready. ATX24/EPS12V/PCIe cable installation bridges, wattage/headroom, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging/delivery/settlement, Save/Guardian, staff/customer/world expansion and final art remain dependent work.

## Current verification

- Technical source commit `740a8869e2efc1f525b9560d4d5638343c957eb5`, tree `d64e70bb6bd2d7f0d8583555146050f7060db0f2`.
- Unity 6000.3.21f1 targeted Mac EditMode `6/6`, targeted Mac PlayMode `5/5`, scene contract `1/1`, runtime-readiness `1/1`, recovery `1/1`, full EditMode `739/739` and full PlayMode `144/144`; failed, skipped and inconclusive `0`.
- Universal macOS Development build report `330,279,904` bytes across `302` files. The deep/strict-valid `x86_64 + arm64` executable emits one r51 readiness marker and one exact PSU Assembly success marker on Apple M1/Metal, reaches Input System shutdown, exits `0` and leaves no player residue.
- Exact-head technical Repository Guard [33027397901](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33027397901) passed.
- Complete bundle `7,632,290` bytes / SHA-256 `3936b661bd1dc711099838949613f745729f30719320632a0ee2c5de535bb53f` restored a detached-clean Windows clone at the exact technical commit/tree.
- Windows Unity 6000.3.21f1 full EditMode `739/739` and full PlayMode `144/144` passed. The x64 IL2CPP/only-D3D11 build report is `1,346,115,186` bytes with fatal-token count `0` and byte-exact `ProjectSettings.asset` restoration.
- Intel Iris Xe Direct3D 11.0 feature level 11.1 interactive runtime has exact host/readiness/success counts `1/1/1`, forbidden count `0`, exit `0`, graceful shutdown, deleted task and validation-owned residue `0`.
- Foreground Session 2 OS-input r2 acceptance delivers W/A/S/D down/up `1/1`, relative mouse `18/18`, initial W+D+mouse `3/3` and further held-key mouse deltas `30/30`; all eight screenshots have nonzero, unique hashes. The first r1 harness stopped only on a PowerShell `[ushort]` alias incompatibility, left zero residue and is retained as diagnostic evidence.
- Windows final audit `pcshop-issue102-windows-final-audit-v2` passes every one of its `27` checks. Exact Windows evidence returned to the Mac as a `6,587,392`-byte tar with SHA-256 `c15e21ff64ab08328072470120df62b69d8f0c52c364301a079a56dbd90c44a0`.
- No ProjectSettings change is present in the source commit. No Windows removable volume or USB disk was identified, and this issue performs no USB write or USB checkpoint claim.
- Real-human, physical keyboard, physical gamepad and endurance certification remain explicitly deferred to the final Steam 1.0 release gate.

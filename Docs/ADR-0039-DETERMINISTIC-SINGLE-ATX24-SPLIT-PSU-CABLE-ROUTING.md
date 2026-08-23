# ADR-0039 — Deterministic Single ATX24 Split-PSU Cable Routing

**Status:** Accepted and implemented<br>
**Date:** 23 August 2026<br>
**Scope:** Issue #61, child of Epic #10

## Context

The physical PC assembly line had a retained canonical ATX PS/2 power supply and a secured canonical motherboard, but no physical or authoritative power-cable connection between them. The bounded slice had to add one visible and reversible modular ATX 24-pin cable without introducing electrical simulation, wattage/headroom, POST/BIOS/OS, completed benchmark scoring, free-rope physics, a shadow SKU or a second Inventory authority.

## Decision

- Add one canonical fictional `PowerCable` product and one exact serialized cable item. The PSU 18-pin primary, PSU 10-pin sense and motherboard 24-pin connectors are typed endpoints of that item, not separate products, Inventory records or pickup targets.
- Claim Workbench, ProcessorSocket, MemorySlot, StorageSlot, ProcessorCoolerSlot, GraphicsCardSlot, PowerSupplyBay and a capacity-one CableRoute atomically in one Inventory revision. Any conflict, stale revision or overflow fails before partial custody is created.
- Model one immutable topology with three stable typed endpoints and three distinct ordered route waypoints. Its fingerprint is deterministic and is included in every route receipt.
- Model the reversible lifecycle `Loose ↔ Routed`. Route transfers the exact serialized item from Hands to CableRoute; unroute transfers it back to Hands using the exact source-route lineage.
- Preserve immutable route/unroute receipts, immediate and delayed exact replay, conflict detection and a complete receipt-history fold over cable item, product, topology, connector, waypoint, host and source-operation lineage.
- Require a retained PSU, secured motherboard, exact keyed orientation, range, focus, line of sight and an obstruction-free authored five-segment route. Wrong key, blocked route, duplicate route, stale/conflicting operation and saturated NonAlloc query fail closed without mutation.
- Keep the routed cable dependent on both hosts: PSU unretain and motherboard unsecure/detach are rejected while the route exists.
- Keep `PlayerCarryController` as the only gameplay input consumer. Keyboard/mouse and real Input System gamepad use a dedicated route mode and compact dynamic prompts; generic placement, stacking and cart paths cannot consume a held cable.
- Use one kinematic physical item, three visible connector children, latch/key geometry and authored `LineRenderer` branches/trunk. No joints, rope particles, duplicate cable roots or physics-driven attachment are created.
- Keep the routed connector focus target available for deliberate unroute interaction while ignoring only connector-owned host geometry; real chassis obstruction still fails closed. The same Unity instance and stable ItemId survive pickup, route, unroute and recovery.
- Expand the prototype WorldFloor capacity from eight to exactly nine units so the eight loose assembly items and one active delivery item can coexist; capacity remains bounded and tested.

## Consequences

The game now has a visible and reversible motherboard-power wiring loop with stable identity, authoritative custody and deterministic authored routing. A retained PSU plus routed ATX24 cable satisfies only its bounded readiness reason; the build remains `BuildIncomplete`. EPS/CPU, PCIe/GPU, SATA/Molex/fan/front-panel/data/RGB cables, electrical power-on, circuit simulation, wattage/headroom, POST/BIOS/OS, completed benchmark scoring, free-rope physics, final art/audio/VFX/UI and native Windows/Steam validation remain separate gates.

## Verification

- EditMode: 589/589.
- Real Input System PlayMode: 49/49.
- Universal macOS Development/StrictMode build: 329,082,160 bytes.
- Active Apple Silicon/Metal workstation (Apple M1), 1280×720: r30 readiness and exact `GARAGE_POWER_CABLE_RUNTIME_SMOKE` passed.
- Feature commit: `1fc29f13171925c2445eaa7334158e0f058e76a5`; tree `d265332f1d6655639e55db31f9b5a11e3d177f49`.
- Feature Repository Guard: [32613813494](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32613813494), success.

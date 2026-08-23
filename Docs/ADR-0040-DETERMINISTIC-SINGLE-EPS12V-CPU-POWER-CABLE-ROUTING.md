# ADR-0040 — Deterministic Single EPS12V/CPU Power Cable Routing

**Status:** Accepted and implemented; physical checkpoint verified, repository closure pending<br>
**Date:** 23 August 2026<br>
**Scope:** Issue #62, child of Epic #10

## Context

The physical assembly line had a retained canonical ATX PS/2 power supply, a secured canonical motherboard, a retained canonical CPU and an isolated ATX24 route, but no physical or authoritative EPS12V/CPU power connection. The bounded slice had to add one visible, keyed and reversible CPU-power cable without introducing electrical readiness, PSU wattage/headroom, POST/BIOS/OS, completed benchmark scoring, free-rope physics, a shadow SKU or a second Inventory authority.

## Decision

- Add one canonical serialized EPS12V cable item with two typed eight-pin endpoints and three distinct ordered authored waypoints. Connector and latch children are visual parts of the one item, never independent Catalog products, Inventory records or pickup targets.
- Extend the existing all-or-none managed Assembly claim to nine containers, adding one capacity-one `CpuPowerCableRoute` without adding a raw-transfer API or another Inventory authority.
- Model the reversible lifecycle `Loose ↔ Routed`. Route transfers the exact item from Hands to `CpuPowerCableRoute`; unroute transfers the exact source lineage back to Hands.
- Preserve immutable route/unroute receipts, exact immediate and delayed replay, conflict detection and receipt-history validation over item, product, topology, endpoints, waypoints, retained PSU, secured motherboard, retained CPU and source-operation lineage.
- Require the exact keyed orientation, retained PSU, secured motherboard, retained CPU, range, focus, line of sight and an obstruction-free authored route. Wrong key/endpoint, missing host, duplicate route, stale/conflict, full hands and saturated NonAlloc queries fail closed without mutation.
- Block PSU unretain, motherboard unsecure/detach and CPU retention-open/remove while the EPS route exists. Keep ATX24 and EPS12V product, item, container, revision, operation and receipt identities isolated.
- Keep `PlayerCarryController` as the only gameplay input consumer. Keyboard/mouse and real Input System gamepad use a dedicated route mode, two keyed preview orientations, deliberate commit/unroute and compact dynamic prompts; generic placement, stacking and cart paths cannot consume the cable.
- Use one kinematic physical item, two visible eight-pin connector housings, latch/key geometry, a loose braided presentation and authored preview/committed `LineRenderer` segments. Cable visuals are colliderless and `Ignore Raycast`; only the intentional routed connector focus trigger is interactable.
- Preserve the same Unity component instance and stable ItemId through pickup, route, unroute and recovery. Preview and committed poses use the same authored topology; no rope joints, duplicate roots or physics-driven attachment are created.
- Keep routed EPS12V as a physical assembly prerequisite only. It does not produce electrical readiness or a completed benchmark; the build remains `BuildIncomplete`.

## Consequences

GarageGraybox now contains a second, separately authoritative power-cable family: the player can physically route and undo the CPU-power lead while host lineage, custody and identity stay deterministic. ATX24 state cannot satisfy or mutate EPS12V state. PCIe/GPU, SATA/Molex, fan, front-panel, RGB and data cabling; electrical power-on; wattage/headroom; POST/BIOS/OS; completed benchmark scoring; final art/audio/VFX/UI and native Windows/Steam validation remain separate gates.

## Verification

- EditMode: 610/610; failed, skipped and inconclusive 0.
- Real Input System PlayMode: 51/51; failed, skipped and inconclusive 0.
- Universal macOS Development/StrictMode build: 329,206,153 bytes.
- Active Apple Silicon/Metal workstation (Apple M1), 1280×720: canonical r31 readiness and exact `GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE` passed.
- Feature commit: `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`; tree `c14524fecee561eff3a144bd15e67be5a48f8335`.
- Feature Repository Guard: [32642211422](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642211422), success.
- Source/docs commit `cff75f8876f893888ca3a98fe5f149dab0f74a1b`, tree `aa5acd799a8190d871aa0c5493fd7484a83b4c4f`; [Repository Guard 32642638437](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642638437), success.
- Verified local staging passed two full 832/832 payload, 826/826 exact Git-source and 5/5 evidence readbacks with manifest `afa89feb…6a73` and all mismatch counters zero.
- The physical USB source-plus-evidence milestone passed two full 832/832 payload readbacks, 826/826 exact Git-source equality and 5/5 evidence equality with manifest `afa89feb…6a73`; Issue #62 remains open/In Progress only until the final metadata Guard and GitHub state transition complete.

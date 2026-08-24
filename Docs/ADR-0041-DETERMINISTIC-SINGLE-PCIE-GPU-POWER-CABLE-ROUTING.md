# ADR-0041 — Deterministic Single PCIe/GPU Power Cable Routing

**Status:** Implemented and verified on macOS and Windows; physical USB closure pending<br>
**Date:** 24 August 2026<br>
**Scope:** Issue #63, child of Epic #10

## Context

The assembly prototype already had a retained canonical ATX PS/2 power supply, a secured canonical motherboard, a retained canonical Northstar A60 graphics card, and isolated ATX24 and EPS12V routes. It still lacked the physical PCIe/GPU power connection required by the retained graphics card. The bounded slice had to add one visible, keyed, reversible 8-pin 6+2 route without introducing electrical readiness, wattage/headroom, POST/BIOS/OS, benchmark completion, free-rope physics, a shadow SKU, or a second Inventory authority.

## Decision

- Add one canonical serialized PCIe/GPU 8-pin cable item with two distinct typed endpoints and three distinct ordered authored waypoints. Connector, 6+2 split, latch, and key children are visual parts of the one item and are never independent Catalog products, Inventory records, containers, or pickup targets.
- Extend the existing all-or-none managed Assembly claim to ten containers by adding one capacity-one `GpuPowerCableRoute`. Do not add a raw-transfer API or another Inventory authority.
- Model the reversible lifecycle `Loose ↔ Routed`. Route transfers the exact item from Hands to `GpuPowerCableRoute`; unroute transfers the exact source lineage back to Hands.
- Preserve immutable route/unroute receipts, exact immediate and delayed replay, conflict detection, and receipt-history validation over item, product, topology, endpoints, waypoints, retained PSU, secured motherboard, retained GPU, and source-operation lineage.
- Require the exact keyed orientation, retained PSU, secured motherboard, retained GPU, range, focus, line of sight, and an obstruction-free authored route. Wrong key/endpoint, missing host, duplicate route, stale/conflict, full hands, and saturated NonAlloc queries fail closed without mutation.
- Block PSU unretain/remove, GPU unretain/remove, and motherboard unsecure/detach while the PCIe/GPU route exists. Keep ATX24, EPS12V, and PCIe/GPU product, item, container, revision, operation, state, and receipt identities isolated.
- Keep `PlayerCarryController` as the only gameplay input consumer. Keyboard/mouse and real Input System gamepad use a dedicated route mode, two keyed preview orientations, deliberate commit/unroute, pause draining, co-edge protection, and compact dynamic prompts. Generic placement, stacking, cart, and raw Inventory paths cannot consume the cable.
- Use one kinematic physical item, a monolithic PSU-side 8-pin housing, physically separate GPU-side 6-pin and 2-pin housings, a keyed 6-pin latch, a 2-pin retention clip, explicit `6`/`2` labels, a loose braided presentation, and authored preview/committed `LineRenderer` segments. Presentation geometry is colliderless and `Ignore Raycast`; only the intentional routed GPU connector focus trigger is interactable.
- Parent the GPU-side anchor to the moving canonical graphics-card item. Preserve the same Unity component instance and stable ItemId through pickup, route, unroute, world drop, and recovery. Preview and committed poses use the same authored topology; no rope joints, duplicate roots, or physics-driven attachment are created.
- Keep routed PCIe/GPU power as a physical assembly prerequisite only. It does not produce electrical readiness or a completed benchmark; the build remains `BuildIncomplete`.

## Consequences

GarageGraybox r32 now contains three separately authoritative power-cable families. The player can route the visible PSU-to-GPU 6+2 lead and undo it while host lineage, custody, replay, and identity stay deterministic. ATX24 or EPS12V state cannot satisfy or mutate the PCIe/GPU route. SATA/Molex, fan, front-panel, RGB and data cabling; electrical power-on; PSU wattage/headroom; POST/BIOS/OS; completed benchmark scoring; final art/audio/VFX/UI; Save/Guardian; and Steam release work remain separate gates.

## Verification

- EditMode: 626/626; failed, skipped, and inconclusive 0.
- Real Input System PlayMode: 53/53; failed, skipped, and inconclusive 0.
- Universal macOS Development/StrictMode build: 329,334,656 bytes; `arm64` and `x86_64` slices.
- Active Apple Silicon/Metal workstation (Apple M1), 1280×720: canonical r32 readiness and exact `GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE` passed.
- Feature commit: `ea1e51f862d4094936c03bccf9fbfaee7bb7d12b`; tree `ecc32279a8e17e8179114a9b6cfcfe4737827601`; Repository Guard [32676069923](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676069923), success.
- Repeatable Windows IL2CPP build gate commit: `cdfe9d6a3bed20a6529fb045f69d7394b3b147c8`; tree `3b1b06966cbc39759756ec0ec2220647b5348319`; Repository Guard [32676154473](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676154473), success.
- Explicit 6+2 visual correction commit: `d655f1a5aab0c882cf40702472ec1b8ad44747ad`; tree `c3fff116317db7e3388e0faf04e38a7ffaa7ce77`; Repository Guard [32677267023](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677267023), success. A bounded independent read-only re-audit found no remaining P0/P1 visual or physics-contract issue.
- Source/docs checkpoint `d597941a20afd0491547513abbc68e0b9d890aab` passed Repository Guard [32677495639](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677495639).
- The Windows validation clone was clean and exact at `d597941`. Unity 6000.3.21f1, Windows IL2CPP support, Visual Studio Build Tools, MSVC, MSBuild and the Windows SDK produced a Development/StrictMode x64 IL2CPP player with report size `1,320,679,269` bytes. The active Windows console session then ran the player on Intel Iris Xe / Direct3D 11.0 feature level 11.1 and emitted canonical r32 readiness plus the exact PCIe/GPU success marker once.
- The Windows build log is `1,554,549` bytes with SHA-256 `459e95bb43ab79a1004e13e71b74c8500f484c9cd33e1f698deb7f277f844799`; the interactive runtime log is `4,765` bytes with SHA-256 `853dd5bd75b63d8938dcd6f9b664e979b43aeafa1409b3678dad143d931b3f9e`. The earlier exit-198 license attempts remain diagnostic history and are not the final Windows result.
- The user currently reports that the physical USB is disconnected. No USB query or write is performed. Local final staging, two-pass physical USB readback, final metadata commit, Issue closure, and Roadmap `Done` remain pending.

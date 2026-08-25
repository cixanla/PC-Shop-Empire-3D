# ADR-0050 — Canonical Power Supply Physical Build-Kit Handoff

**Status:** Accepted technically; lifecycle closure pending<br>
**Date:** 25 August 2026<br>
**Scope:** Issue #81, child of Epic #10

## Context

Issues #68, #71, #73, #75, #77 and #79 moved the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device, processor cooler and graphics card into the first six custom-PC BuildKit slots. Issue #81 continues the same accepted work order with the canonical reserved power supply and advances the visible physical ticket from `6/10` to `7/10`.

The PSU handoff is not PSU installation. Issue #60 already owns chassis-bay seating, orientation and four-screw retention; Issues #61, #62 and #63 own ATX24, EPS12V and PCIe/GPU power-cable routing. This BuildKit slice must keep all four authorities isolated while preserving the same serialized PSU and Unity object through source, player hands, preview, placement, replay and recovery.

Native smoke also exposed a test-harness defect: nested prerequisite setup injected `E` and called the work-ticket station in the same frame, bypassing the normal Unity player loop. The resulting false missing-ticket failure did not represent human gameplay. The accepted repair drives neutral, pressed and released frames through the real player loop, captures pressed-frame diagnostics before release and keeps direct station processing out of the positive route.

## Decision

- Resolve the canonical PSU only by `ComponentKind.PowerSupply` and the complete accepted work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple. Ordinal, display name, product-value equality and regenerated identities never grant authority.
- Append the PSU pickup/staged operations without renumbering persisted stages. Use a distinct stable operation identity and a power-supply-specific managed capacity-one BuildKit container.
- Claim all seven BuildKit containers atomically through the existing Inventory Septuple access. Duplicate, foreign, partial or ghost topology fails before authority creation and produces zero mutation.
- Permit custody only through exact source → `ActorHands` → power-supply BuildKit. Generic Inventory transfer, world drop, box, stack, cart and Assembly paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor, DDR5 memory, M.2 storage, processor cooler and graphics card staged prerequisites before PSU pickup or placement. Work-ticket progress derives from authoritative staged receipts and changes only from `6/10` to `7/10` after the PSU commit.
- Commit domain custody before changing world parent, pose, physics or visibility. Physical placement failure recovers the same PSU instance at the authoritative BuildKit pose; clone, ghost and lost-item states are forbidden.
- Keep power-supply BuildKit, Issue #60 PSU-bay Assembly and Issues #61–#63 cable routing as separate authorities. With an active BuildKit pickup receipt, primary, rotate, interact and drop edges have one consumer; receipt-free legacy Assembly remains available.
- Use one raycastable non-trigger support collider and one exact snap anchor. Preview orientation is keyed `0° ↔ 180°`; decorative geometry and progress presentation remain non-authoritative and `Ignore Raycast`.
- Validate the post-prerequisite player route with real `CharacterController` movement and mouse delta. Teleport assistance is permitted only to stage prerequisite components; the measured PSU route returns to the authored spawn, walks spawn → PSU → PSU BuildKit without transform snapping, preserves the player parent, stays inside the horizontal-step envelope and crosses no blocking collision.
- Drive native BuildKit work-ticket interaction through neutral → `E` pressed → released player frames. Capture pressed-frame gate/unconsumed/station diagnostics before release so native failures are attributable without bypassing production Update order.
- Bind native evidence to the exact technical commit/tree, full test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes and interactive task cleanup. Runtime acceptance requires one exact host marker, one r41 readiness marker, one exact PSU success marker, zero forbidden markers and zero process/task residue.

## Consequences

GarageGraybox r41 lets the player take the exact reserved PSU, carry and rotate that same physical object and place it into a dedicated PSU BuildKit tray after the first six components are staged. Reservation/allocation identity stays live and the ticket visibly becomes `7/10`. The PSU is not seated in the chassis, retained, connected to AC/modular power or attached to ATX24/EPS12V/PCIe cables.

The three remaining BuildKit transfers—ATX24, EPS12V and PCIe/GPU cable—advance `7/10 → 10/10`. Job-specific assembly, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, final art and Steam release remain dependent work.

## Verification

- Technical source commit `f3d80629e09c05afde97fa778c4b220ca456c5f0`, tree `851954879c1ff1e2ef98bc9a7a8469750304d992`.
- Unity 6000.3.21f1 full regression: EditMode `697/697`, PlayMode `105/105`; failed, skipped and inconclusive `0`. The focused native-frame lifecycle regression and `git diff --check` also pass.
- Universal macOS Development build report `329,907,140` bytes. The valid deep/strict universal executable is `x86_64 + arm64`; Apple M1/Metal r41 readiness and exact PSU marker each occur once, forbidden markers and player residue are `0`.
- The complete Git bundle is `7,212,319` bytes with SHA-256 `7074973b9864154efe1114053140bc60f70513176b7e3042cbbcbe9e53c2b99e`. It produced a collision-free detached-clean Windows checkout at the exact technical commit/tree.
- Windows x64 IL2CPP/Direct3D11 build report is `1,335,888,266` bytes. Hardened build fatal-token count is `0`; the three required binaries are hash-bound.
- Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive runtime at 1280×720 emitted exact host, r41 readiness and PSU success markers once. It exited `0`, shut down gracefully, deleted task `PSE-Issue81-f3d80629-R1` and left task/player/Unity residue `0`.
- Canonical technical evidence has 13 immutable artifacts. `source-receipt.json`, exact Issue #81 verifier, source/docs Repository Guard, immutable local package, physical USB double-readback and Issue/Project closure remain explicit lifecycle gates and must not be inferred from technical success.

# Deterministic Single PCIe x16 Graphics-Card Seating and Rear-Bracket Retention — Checkpoint Evidence

**Date:** 22 August 2026<br>
**Issue:** [#59](https://github.com/cixanla/PC-Shop-Empire-3D/issues/59)<br>
**Feature:** `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`<br>
**Tree:** `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`

## Delivered playable result

GarageGraybox r28 now contains one canonical serialized Northstar A60 assembly graphics card and one motherboard-owned PCIe x16 slot, latch, rear bracket and bracket fastener. The assembly card has an exact ItemId distinct from the retail Northstar A60 item while reusing the same canonical ProductId; shadow SKU count is zero. The player can pick up the exact card, enter guided mode, inspect its keyed 0°/180° orientation, seat it only when the secured host, PCIe interface, support, chassis clearance, processor-cooler clearance and obstruction gates are valid, retain it with the visible slot latch and rear screw, release it and remove the same item. Compact prompts switch between real keyboard/mouse and gamepad bindings, state the orientation/retention result in text and do not rely on color alone. Wrong orientation, duplicate seating, retained removal, installed-card motherboard detachment, stale/conflicting replay and failed recovery are no-mutation paths.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 548/548 | `editmode-issue59-final.xml` (457,934 bytes) | `3f7d0f5ce12fbfb9c1b773c0ae6d9ad442313292f98c59a8816343caaedd2b50` |
| PlayMode | 43/43 | `playmode-issue59-final.xml` (87,661 bytes) | `4313d949fa91d1f58278533f3befa6f3181212aa0f47ff1431e219644956313b` |
| Targeted GPU input/host gate | 5/5 | `gpu-input-host-gate-final-r2.xml` (12,875 bytes) | `cdc442785a0adaa9096ec0ebc6e0db23677d8be13d13a30e5169f32ee472894c` |
| macOS build | Success | `build-macos-issue59-final-r2.log` (584,629 bytes) | `ed9ff2282c816a159eb6947c15c5076f7c91125b52ca70a84ef7a27a5a6f80d9` |
| Native runtime | Success | `runtime-gpu-issue59-metal-final-r2.log` (5,386 bytes) | `f8c1d5d8c79c58a7fc3b2a7ca162a8d6f3a1d27b30ae44a2046f77ebee1fccd2` |
| Scene | Deterministic r28 | `Assets/Scenes/Prototypes/GarageGraybox.unity` (2,293,982 bytes) | `14f405a657c8d9b5a1719be85d8be4c254fbf91983852170892dd00118dfaf5b` |

Both XML suites report zero failed, skipped and inconclusive tests. The full PlayMode suite retains the shared `PlayerCarryController` pause, release/repress and one-consumer co-edge gates; the targeted 5/5 slice maps the real keyboard/gamepad GPU cycle, both cooler/GPU installation orders and retained-GPU host-detach no-mutation path. Scene/runtime invariants require one canonical GPU binding and one physical projection, so duplicate/orphan presentation cannot become authority. The build is a 328,781,520-byte ad-hoc-signed Universal Mach-O (`arm64` + `x86_64`) macOS application with identifier `com.cixanla.pcshopempire3d`.

## Native marker

```text
GARAGE_GPU_RUNTIME_SMOKE gpu-flow=ok preflight=ok pcie-interface=ok keyed-orientation=ok clearance=ok slot-latch=ok rear-bracket=ok duplicate-seat-blocked=ok retained-remove-gate=ok host-detach-gate=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The build was launched windowed at 1280×720 on the active Apple Silicon/Metal workstation with `-pse-gpu-smoke`; the runtime identified the migrated machine as Apple M1. The process was intentionally stopped only after the exact r28 readiness and GPU markers appeared.

## Repository closure and deferred physical USB copy

- Feature Repository Guard: [32599710154](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32599710154), success.
- Source/docs checkpoint: `a5bbca473e81455c44d2f95469c8faf2a11046ff`; tree `1aa335510910bc4ebd60367b41e67dbf99d039b8`.
- Source/docs Repository Guard: [32600012769](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32600012769), success.
- Acceptance: 20/20; Issue #59 `CLOSED/COMPLETED` and Development Roadmap item `Done`. Parent Epic #10 remains open/In Progress.
- The user explicitly reported that the physical USB is unavailable and will announce when it is connected. No volume was inspected and no physical USB milestone or readback is claimed for Issue #59.
- GitHub `main` plus the hashed external test/build/runtime artifacts above is the current checkpoint. A local USB-ready package and physical `.incoming-*`/full-readback milestone will be created only after the user reports the expected USB connected.

## Bounded exclusions

PCIe power connectors and cabling, PSU installation, alternate card dimensions/slots, risers, multi-GPU, fan curves, POST/BIOS/OS, completed benchmark scoring, final art/audio/VFX/UI and native Windows/Steam release validation are not part of Issue #59.

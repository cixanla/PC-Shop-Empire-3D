# Deterministic Single PCIe/GPU Power Cable Routing — Checkpoint Evidence

**Date:** 24 August 2026<br>
**Issue:** [#63](https://github.com/cixanla/PC-Shop-Empire-3D/issues/63)<br>
**Feature:** `ea1e51f862d4094936c03bccf9fbfaee7bb7d12b`<br>
**Feature tree:** `ecc32279a8e17e8179114a9b6cfcfe4737827601`<br>
**Explicit 6+2 visual fix:** `d655f1a5aab0c882cf40702472ec1b8ad44747ad`<br>
**Visual-fix tree:** `c3fff116317db7e3388e0faf04e38a7ffaa7ce77`<br>
**Closure status:** Technical macOS checkpoint verified; Windows license and USB gates pending

## Delivered playable result

GarageGraybox r32 contains one canonical serialized PCIe/GPU 8-pin 6+2 power cable. The player picks up that exact item, opens its dedicated route preview with Mouse Left or Gamepad RT, toggles only the two keyed orientations with R or Gamepad Right Shoulder, and commits the visible three-waypoint route with G or Gamepad East only when the PSU is retained, the motherboard is secured, the graphics card is retained, and the authored route is clear.

The PSU-side connector remains one monolithic 8-pin housing. The GPU-side plug is visibly and structurally split into separate 6-pin and 2-pin housings with distinct spacing, a keyed six-pin latch, a two-pin retention clip, and separate `6`/`2` labels. These presentation children contain no colliders or joints and do not become pickup, Inventory, or raycast authorities. A bounded independent scene audit confirmed that the earlier monolithic-GPU visual P1 is closed and found no remaining evidence-backed P0/P1 issue.

The routed cable remains the same Unity component instance and stable ItemId. The GPU-side connector follows the moving canonical graphics-card item. Looking at that visible connector and pressing E or Gamepad South unroutes the exact source lineage back to Hands; world drop and recovery restore the authored loose pose without duplication or loss. Generic placement, stacking, cart, and raw-transfer bypasses fail closed. A routed PCIe/GPU cable blocks dependent PSU, motherboard, and GPU detach operations. Compact prompts switch with the active input device and expose endpoint, orientation, route, and blocked reasons in text.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 626/626 | `issue63-r32-6plus2-surgical-editmode-final.xml` (523,340 bytes) | `238c8c6d0d7c8ec6ae8519a658e44d0bbc986492ef5d82bdcb47d5ee19365c38` |
| PlayMode | 53/53 | `issue63-r32-6plus2-surgical-playmode-final.xml` (132,432 bytes) | `d3fc23aed631e90a0321f83093fd40f67f9230ae7397e76775aace19795cb94f` |
| macOS build | Success | `issue63-r32-6plus2-surgical-macos-build-final.log` (490,119 bytes) | `3816daa098bd895ef80622c9c9260778ff6573dbf6e00acf6aae6b1f5fd07bdb` |
| Native runtime | Success | `issue63-r32-6plus2-surgical-native-smoke-final.log` (6,141 bytes) | `0d31f2903038221a350bf8a7e32c7a61da30491bac27262bd2f03eca20777e0c` |
| Surgical scene patch | Success | `issue63-r32-6plus2-scene-surgical-patch.log` (37,634 bytes) | `ef34f8fd8df7ff2d23afdbf2221617e30cabfd1a243a5f0452cc8327d4d1491a` |
| Scene | Deterministic r32 | `Assets/Scenes/Prototypes/GarageGraybox.unity` (2,854,602 bytes) | `7fc63ba4686db17f5ca7800bf2421a526df591659dc84c201439f153416ff338` |
| Windows IL2CPP | Blocked before compile | `issue63-r32-windows-il2cpp-license-blocked.log` (6,669 bytes) | `e876e5d8fe08873484f7e6dfbf9b33680ed4658f558d334b54105dd8a9385429` |

Both full XML suites report zero failed, skipped, and inconclusive tests. The build is a 329,334,656-byte Universal Mach-O macOS application with `arm64` and `x86_64` slices. Its 117,179-byte executable has SHA-256 `3e74cdd08573d81381e152e56828fd5b1b38cae520dac0b544c1e7736f7d8062`. `/Users/cixanla/Desktop/PC Shop Empire 3D.app` resolves to this current build.

## Native marker

```text
GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE cable-flow=ok preflight=ok psu-retained-gate=ok motherboard-secured-gate=ok gpu-retained-gate=ok endpoint-key=ok route-waypoints=ok route-clearance=ok generic-bypass-blocked=ok duplicate-route-blocked=ok dependent-detach-blocked=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The player was launched windowed at 1280×720 on the active Apple Silicon/Metal workstation with `-pse-pcie-gpu-power-cable-smoke`. The runtime identified Apple M1/Metal and emitted canonical readiness `garage-pcie-gpu-power-cable-routing-r32-v1`. The exact success marker appeared once; no PCIe/GPU failure marker, assertion, missing-reference, or unhandled exception appeared.

## Repository, Windows, and external checkpoint status

- Feature Repository Guard: [32676069923](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676069923), success.
- Repeatable Windows IL2CPP gate commit `cdfe9d6a3bed20a6529fb045f69d7394b3b147c8` passed [Repository Guard 32676154473](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676154473).
- Explicit GPU-side 6+2 visual correction `d655f1a5aab0c882cf40702472ec1b8ad44747ad` passed [Repository Guard 32677267023](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677267023).
- The Windows validation clone was clean and exact at `cdfe9d6` when the build gate ran. Unity 6000.3.21f1 with Windows IL2CPP support and the required Microsoft C++ toolchain is installed. It must be fast-forwarded to the final Issue #63 source/docs head before the retry.
- The first Windows build attempt ended before compilation with `No valid Unity Editor license found` and process exit code 198. This is an environment-license blocker, not a source/build failure. Windows IL2CPP build size, executable hash, DirectX device, and native r32 smoke are not claimed.
- The expected USB is now safely mounted at `/Volumes/cixanla/CIXANLA`; `90_BACKUPS/PCShopEmpire3D` and the prior Issue #62 milestone chain are verified. No Issue #63 write is attempted before Windows native proof and final source/docs identity. Local final staging and physical two-pass manifest readback remain pending.
- Issue #63 remains open and Roadmap `In Progress`; acceptance closure is intentionally withheld until Windows native and USB gates pass.

## Bounded exclusions

Electrical power-on, current/voltage simulation, PSU wattage/rail/transient/headroom, POST/BIOS/OS, completed benchmark scoring, SATA/Molex, fan, front-panel, RGB and data cabling, free-rope physics, final art/audio/VFX/UI, Save/Guardian, and Steam packaging/signing are not part of Issue #63.

# Deterministic Single EPS12V/CPU Power Cable Routing — Checkpoint Evidence

**Date:** 23 August 2026<br>
**Issue:** [#62](https://github.com/cixanla/PC-Shop-Empire-3D/issues/62)<br>
**Feature:** `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`<br>
**Feature tree:** `c14524fecee561eff3a144bd15e67be5a48f8335`

## Delivered playable result

GarageGraybox r31 contains one canonical serialized EPS12V/CPU power cable. The player picks up that exact item, opens its dedicated route preview with Mouse Left or Gamepad RT, toggles only the two keyed orientations with R or Gamepad Right Shoulder, and commits the visible three-waypoint route with G or Gamepad East only when the PSU is retained, the motherboard is secured, the CPU is retained and the authored route is clear.

The routed cable remains the same Unity component instance and stable ItemId. Looking at its visible motherboard-side eight-pin connector and pressing E or Gamepad South unroutes the exact source lineage back to Hands; recovery restores the authored loose pose without duplication or loss. Generic placement, stacking, cart and raw-transfer bypasses fail closed. A routed EPS cable blocks PSU, motherboard and CPU dependent removal/open operations. Compact prompts switch with the active input device and expose endpoint, orientation, route and blocked reasons in text.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 610/610 | `issue62-r31-editmode-canonical-final.xml` (509,692 bytes) | `4059e5c2180480623c45b4abb0a31d721aebb28b01a36c842603deb43a3e60c7` |
| PlayMode | 51/51 | `issue62-r31-playmode-canonical-final.xml` (120,236 bytes) | `3dd69fc6801fa4ee8c10859ebed0b7576ef78411acf0f9e4c4915248ff2806a8` |
| macOS build | Success | `issue62-r31-macos-build-canonical-final.log` (586,786 bytes) | `cb966a558c57662ca21687346a4292ee2cca15060829add6d938127788a8c379` |
| Native runtime | Success | `issue62-r31-eps12v-native-smoke-canonical-final.log` (6,034 bytes) | `1e6ca9d8327adcb91d270ae5370f5ee2de908266241539dedddc88842f66c40c` |
| Scene rebuild | Success | `issue62-r31-scene-build-canonical-final.log` (47,766 bytes) | `5409b059036732f517a3acecdf52a99b928fedca7b6f4a8fec16d5f08c5c0b0f` |
| Scene | Deterministic r31 | `Assets/Scenes/Prototypes/GarageGraybox.unity` (2,731,756 bytes) | `ab4c8fc87979c357f07679ffdd99735424dafbfa2c5d4a185bf1ff234fb22f3a` |

Both full XML suites report zero failed, skipped and inconclusive tests. The build is a 329,206,153-byte Universal Mach-O macOS application with `arm64` and `x86_64` slices. Its 117,179-byte executable has SHA-256 `365f1cec067cb09663d4388846a21e254627184503688e3171ab168bcfb333cb`.

## Native marker

```text
GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE cable-flow=ok preflight=ok psu-retained-gate=ok motherboard-secured-gate=ok cpu-retained-gate=ok endpoint-key=ok route-waypoints=ok route-clearance=ok generic-bypass-blocked=ok duplicate-route-blocked=ok dependent-detach-blocked=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The build was launched windowed at 1280×720 on the active Apple Silicon/Metal workstation with `-pse-eps12v-power-cable-smoke`. The runtime identified Apple M1/Metal and emitted canonical readiness `garage-eps12v-cpu-power-cable-routing-r31-v1`. The exact success marker appeared once; no EPS failure marker, assertion, missing-reference or unhandled exception appeared.

## Repository and pending external checkpoint

- Feature Repository Guard: [32642211422](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642211422), success.
- Source/docs checkpoint metadata will be recorded after this evidence commit reaches private `main` and its Guard passes.
- No physical USB is currently available. No external milestone path, USB manifest, atomic rename, full USB readback or Issue completion is claimed.
- Issue #62 remains `OPEN/In Progress`; the first twenty acceptance lines have current source/test/build/runtime evidence, while the final combined checkpoint line remains pending until source/docs/Guard and physical USB readback are complete.

## Bounded exclusions

Electrical power-on, current/voltage simulation, PSU wattage/rail/transient/headroom, POST/BIOS/OS, completed benchmark scoring, PCIe/GPU, SATA/Molex, fan, front-panel, RGB and data cabling, free-rope physics, final art/audio/VFX/UI, Save/Guardian and native Windows/Steam validation are not part of Issue #62.

# Deterministic Single ATX PS/2 Power-Supply Seating and Four-Screw Retention — Checkpoint Evidence

**Date:** 23 August 2026<br>
**Issue:** [#60](https://github.com/cixanla/PC-Shop-Empire-3D/issues/60)<br>
**Feature:** `f998d7d1c400c9328afa226f0727e6591c02d4e2`<br>
**Feature tree:** `78d62c46354cda45422ca947df10ba9d6823b7c9`<br>
**Authored-clearance fix:** `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`<br>
**Fix tree:** `a15865346f52b6b39d84cec49c70babbc6550b89`

## Delivered playable result

GarageGraybox r29 now contains one canonical serialized ATX PS/2 assembly PSU and one chassis-owned bay, rear mount and four-fastener topology. The player can pick up the exact PSU, enter guided mode, toggle only the two keyed 180-degree fan-intake orientations, seat it when support, rear-plane, ATX interface, clearance, range, focus, LOS and obstruction gates are valid, retain it through the four visible rear screws, release it and remove the same item. Compact prompts switch between real keyboard/mouse and gamepad bindings and state orientation, failure and retention results in text rather than relying on color alone.

The final authored-clearance pass binds production geometry to the real `ChassisBack`, `ChassisLeftRail`, `ChassisRightRail` and `MotherboardTray` colliders. The PlayMode proof starts at `ValidSeat`, moves the actual authored `ChassisBack` collider into the seat envelope, receives exact `ChassisClearanceBlocked` with no held-item, assembly, Inventory, receipt or instance mutation, restores the same collider and returns to `ValidSeat` before same-instance recovery. `ChassisBase` and the filtered floor intake remain support surfaces; the cable-blocker list is intentionally empty because cabling is a later issue.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 577/577 | `editmode-issue60-final.xml` (482,181 bytes) | `7b16cf2548d1562a904a05b99870aabaf0ec5c1c3fcf8d4261b07deccb60e058` |
| PlayMode | 47/47 | `playmode-issue60-final.xml` (99,730 bytes) | `8d46ce6c0c586a6865725a47784c353e44a24017b2f7d35a7f9e15a288d9cd20` |
| Authored scene-clearance EditMode | 1/1 | `power-supply-scene-clearance-final.xml` | Verified pass |
| Authored `ChassisBack` PlayMode | 1/1 | `power-supply-authored-clearance-final.xml` | Verified pass |
| macOS build | Success | `build-macos-issue60-final.log` (585,248 bytes) | `462d0f5d3d07de4314ab89b356adc529e854541a332d1d43bf954a457e2dd305` |
| Native runtime | Success | `runtime-psu-issue60-final-activated.log` (7,468 bytes) | `574eb272912dcac4ca18590954a18fd6e711c4ef88576f713bccaba14b437b40` |
| Scene | Deterministic r29 | `Assets/Scenes/Prototypes/GarageGraybox.unity` (2,474,613 bytes) | `6bd7a0fd914841cbf366d860eabb3f2892d55460ddd86235dcb7f19220cd516f` |

Both full XML suites report zero failed, skipped and inconclusive tests. The build is a 328,937,592-byte ad-hoc-signed Universal Mach-O (`arm64` + `x86_64`) macOS application. Its 117,179-byte executable has SHA-256 `44045bf514841be7bd268e9032448583499bc416fe809ceac0196dd51b0e91f6`.

## Native marker

```text
GARAGE_PSU_RUNTIME_SMOKE psu-flow=ok preflight=ok atx-ps2=ok keyed-orientation=ok clearance=ok rear-mount=ok four-screw=ok duplicate-seat-blocked=ok retained-remove-gate=ok alternate-order=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The build was launched windowed at 1280×720 on the active Apple Silicon/Metal workstation with `-pse-psu-smoke`; the runtime identified the machine as Apple M1, Unity 6000.3.21f1 and multi-threaded PhysX. The exact marker appeared once and no PSU failure marker, assertion or unhandled exception appeared.

## Repository closure and external checkpoint

- Feature Repository Guard: [32606958882](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32606958882), success.
- Authored-clearance Repository Guard: [32607437408](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607437408), success.
- Source/docs checkpoint: `SOURCE_DOCS_PENDING`; Repository Guard: `SOURCE_DOCS_GUARD_PENDING`.
- Acceptance, Issue state and Roadmap state are finalized only after the source/docs Guard succeeds.
- macOS currently exposes no external physical disk and no `/Volumes/cixanla/CIXANLA` mount. No wrong-volume write or physical USB milestone/readback is claimed. The exact source-plus-evidence package is prepared locally and may be copied only after the expected volume and prior milestone chain are revalidated.

## Bounded exclusions

ATX 24-pin, EPS/CPU, PCIe/GPU, SATA/Molex/fan cabling and routing; electrical power-on; wattage, rails, transient and efficiency simulation; POST/BIOS/OS; completed benchmark scoring; alternate PSU form factors; final art/audio/VFX/UI; and native Windows/Steam release validation are not part of Issue #60.

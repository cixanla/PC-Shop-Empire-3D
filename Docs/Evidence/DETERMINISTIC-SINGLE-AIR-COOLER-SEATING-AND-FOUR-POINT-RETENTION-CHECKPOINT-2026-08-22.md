# Deterministic Single Air-Cooler Seating and Four-Point Retention — Checkpoint Evidence

**Date:** 22 August 2026<br>
**Issue:** [#58](https://github.com/cixanla/PC-Shop-Empire-3D/issues/58)<br>
**Feature:** `e2f10a22c37101cb12c5d6530c8f104deb72e99d`<br>
**Tree:** `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`

## Delivered playable result

GarageGraybox r27 now contains one canonical top-down LGA1700 air cooler and one motherboard-owned slot, bracket and four-point retention topology. The player can pick up the exact serialized cooler, enter guided mode, toggle its two keyed orientations, seat it only when the motherboard, retained CPU, socket interface, RAM clearance, support and obstruction gates are valid, retain it in the visible `1→3→2→4` cross order, release it in reverse and remove the same item. Compact prompts switch between real keyboard/mouse and gamepad bindings. Invalid orientation, consumed TIM, obstruction, duplicate seat, retained removal, host mutation, stale/conflicting replay and failed recovery are no-mutation paths.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 521/521 | `editmode-issue58-final.xml` (435,456 bytes) | `8d996c9638ae265bffa30c1910cac5c70f86e7ac472f76bc91e071e2e32a086c` |
| PlayMode | 38/38 | `playmode-issue58-final.xml` (74,158 bytes) | `28bd9a30913e33e13510d89165d2b34828cd8143f58eca7de51b0aa6a84c8e15` |
| macOS build | Success | `build-macos-issue58-final.log` (585,965 bytes) | `e32a2a1c8b661a8320e14511eee9d415d6b07c649594cd503221c9e23de99bed` |
| Native runtime | Success | `runtime-cooler-issue58-metal-final.log` (5,282 bytes) | `365bfd3ad8302f65af5a2121a4c36f0c5029d4128694a263cce1dc439b3f32d1` |
| Scene | Deterministic r27 | `Assets/Scenes/Prototypes/GarageGraybox.unity` | `ddb638519d4701dd4c303f328d6a5801a818416a7653938cec7e420d3168dbc3` |

Both XML suites report zero failed, skipped and inconclusive tests. The build is a 328,534,723-byte ad-hoc-signed Universal Mach-O (`arm64` + `x86_64`) macOS application with identifier `com.cixanla.pcshopempire3d`.

## Native marker

```text
GARAGE_COOLER_RUNTIME_SMOKE cooler-flow=ok preflight=ok socket-interface=ok keyed-orientation=ok tim=pre-applied cross-order=ok duplicate-seat-blocked=ok retained-remove-gate=ok host-gates=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The build was launched windowed at 1280×720 on the active Apple Silicon/Metal workstation with `-pse-cooler-smoke`; the runtime identified the migrated machine as Apple M1. The process was intentionally stopped only after the exact readiness and cooler markers appeared. The older Apple M4 device-specific wording is therefore not claimed for this checkpoint.

## Repository closure and deferred physical USB copy

- Source/docs commit: `2e848e3bdc5795a349e6c857973c7c88fef36cd7`.
- Feature Repository Guard: [32591206866](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591206866), success.
- Source/docs Repository Guard: [32591381804](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591381804), success.
- Acceptance: 19/19; Issue #58 and Development Roadmap item `Done`.
- The physical USB device was not enumerated by the migrated Mac. The user explicitly deferred the USB copy so gameplay development can continue.
- USB-ready local staging: `2026-08-22_STAGE_B_DETERMINISTIC_SINGLE_AIR_COOLER_FOUR_POINT_RETENTION`; 712 exact Git source files + four evidence files + source record = 717/717 verified payload files and 13,204,343 bytes.
- Staging manifest SHA-256: `f7b2b9bafee9529d95431bbc90914ba51ab24e01de9a0d5d77a53f26cb5626a5`; Git-blob, hash, size, path, forbidden/cache/credential and AppleDouble mismatch counts are zero.
- No physical USB milestone or USB readback is claimed. It remains ready for `.incoming-*` copy and full readback when the user reports the USB connected.

## Bounded exclusions

Separate thermal paste, cleaning/reapplication, liquid cooling, alternate sockets/brackets, fan curves and cabling, GPU, PSU, cable routing, POST/BIOS/OS, completed benchmark scoring, final art/audio/VFX/UI and native Windows/Steam release validation are not part of Issue #58.

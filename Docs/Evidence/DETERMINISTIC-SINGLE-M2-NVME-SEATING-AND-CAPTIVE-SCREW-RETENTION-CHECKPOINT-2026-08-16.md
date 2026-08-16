# Deterministic Single M.2 NVMe Seating and Captive Screw Retention — Checkpoint Evidence

**Date:** 16 August 2026<br>
**Issue:** [#57](https://github.com/cixanla/PC-Shop-Empire-3D/issues/57)<br>
**Feature:** `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`<br>
**Tree:** `1aedb833983df256c500c6a1815b075fa29c254c`

## Delivered playable result

The GarageGraybox r26 now contains one canonical M.2 2280 NVMe SSD and one motherboard M-key slot with 2280 standoff and captive retention screw. The player can pick up the exact serialized drive, enter guided mode, toggle the two keyed orientations, seat it at the deterministic insertion angle, tighten/loosen the captive screw and remove the same item. Compact prompts switch between real keyboard/mouse and gamepad bindings. Invalid orientation, obstruction, duplicate seat, secured removal, installed-device host detach, replay conflict and failed recovery are no-mutation paths.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 490/490 | `editmode-issue57-final-r2.xml` (409,420 bytes) | `bd34675ca4daa871920a661169c2f10fad08dd9dc5301222da8914a932a217a2` |
| PlayMode | 35/35 | `playmode-issue57-final-r1.xml` (64,933 bytes) | `d73bc518ce358ded3b15130edd8c2fb9ec1e1f380ef91a2ac87827bf7d7f9700` |
| macOS build | Success | `build-macos-issue57-final.log` (600,974 bytes) | `560a20ee380ffe5fd76e12b5c48d5dc843557e27b5a571ea90c6eefac51baad3` |
| Native runtime | Success | `runtime-storage-issue57-final.log` (5,206 bytes) | `5e8a250452c5a487692646b0626dd6aa03ccacd68267a6c37cab62e083ebb858` |
| Scene | Deterministic r26 | `Assets/Scenes/Prototypes/GarageGraybox.unity` | `422d833fa6f47fa10e481c8a39d83eb69f3ab0f9ca36be8b773afa48aebf56be` |

All XML suites report zero failed, skipped and inconclusive tests. The build is a 328,362,356-byte ad-hoc-signed Universal Mach-O (`arm64` + `x86_64`) macOS application with identifier `com.cixanla.pcshopempire3d`.

## Native marker

```text
GARAGE_STORAGE_RUNTIME_SMOKE storage-flow=ok preflight=ok slot-interface=ok keyed-orientation=ok insertion-angle=ok captive-screw=ok duplicate-seat-blocked=ok secured-remove-gate=ok host-detach-gate=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The build was launched at 1280x720 on Apple M4/Metal with `-pse-storage-smoke`; the process was intentionally stopped only after the exact marker appeared.

## Repository and USB closure

- Source/docs commit: `6e0627ec7a76a70abdba8bb507e6ef6979e34236`.
- Repository Guard: [31970813717](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31970813717), success.
- USB milestone: `2026-08-16_STAGE_B_DETERMINISTIC_SINGLE_M2_NVME_CAPTIVE_SCREW`.
- Exact Git source plus four evidence files and source record: 689/689 SHA-256/size/path readback.
- Manifest SHA-256: `19da758c8ab03453092482efc80d6e7dd62aa590a2e57f27c94fe1b8e51e21b8`; AppleDouble sidecars: 0.

## Bounded exclusions

Second M.2/slot, SATA, RAID, hot-swap, heatsink/thermal pad, capacity/performance scoring, GPU/PSU/cooler/cabling, POST/BIOS/OS, final art/audio/VFX/UI and native Windows/Steam release validation are not part of Issue #57.

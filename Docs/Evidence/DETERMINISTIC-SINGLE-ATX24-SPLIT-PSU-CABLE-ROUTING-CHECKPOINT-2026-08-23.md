# Deterministic Single ATX24 Split-PSU Cable Routing — Checkpoint Evidence

**Date:** 23 August 2026<br>
**Issue:** [#61](https://github.com/cixanla/PC-Shop-Empire-3D/issues/61)<br>
**Feature:** `1fc29f13171925c2445eaa7334158e0f058e76a5`<br>
**Feature tree:** `d265332f1d6655639e55db31f9b5a11e3d177f49`

## Delivered playable result

GarageGraybox r30 contains one canonical serialized modular ATX24 cable. The player picks up that exact item, opens a dedicated route preview, toggles only the two keyed connector orientations, and commits a visible PSU 18+10 → three-waypoint channel → motherboard 24 route only when the PSU is retained, the motherboard is secured and the authored five route segments are clear. The connected cable remains the same Unity instance and stable ItemId. Looking at the visible motherboard connector and pressing interact unroutes the exact source lineage back into Hands; recovery restores the authored loose pose without duplication or loss.

The connector children are visual parts of the one cable, never independent Inventory items or pickup targets. Generic placement, stack, cart and raw transfer bypasses fail closed. Routed cable blocks dependent PSU and motherboard detach operations. Compact prompts switch between actual keyboard/mouse and gamepad bindings and state route, orientation and blocked reasons in text rather than relying only on color.

## Automated evidence

| Gate | Result | Artifact | SHA-256 |
|---|---:|---|---|
| EditMode | 589/589 | `issue61-full-editmode-final.xml` (492,247 bytes) | `1e94dd1a48ad7f19ce6bea6f2e7c3bdaba49dced68647e5c498933b15e399f50` |
| PlayMode | 49/49 | `issue61-full-playmode-final.xml` (108,623 bytes) | `73a94e85b70e9c5d6e05eb564f0c4ff81fe7ff09c02b323851d8e621d6f30a39` |
| macOS build | Success | `build-macos-issue61-final.log` (605,068 bytes) | `e8730ba8f2975c16fbdc8034f6554aeb32313048c2ce64202087795d7d930c4e` |
| Native runtime | Success | `runtime-power-cable-issue61-final.log` (5,628 bytes) | `c25c8cb9e95039d57b0ec70294f95efb69112705cd32baa171d65e34833df2d3` |
| Scene | Deterministic r30 | `Assets/Scenes/Prototypes/GarageGraybox.unity` (2,629,606 bytes) | `79c534c7749a60521fae605f29c65db2d224d0fb75b444ea109a6f5c3b0040b2` |

Both full XML suites report zero failed, skipped and inconclusive tests. The build is a 329,082,160-byte ad-hoc-signed Universal Mach-O (`arm64` + `x86_64`) macOS application. Its 117,179-byte executable has SHA-256 `04060db71ecd39f083a526b88e9468bd26ca26c18b12499b6e9dca3da19d85ab`.

## Native marker

```text
GARAGE_POWER_CABLE_RUNTIME_SMOKE cable-flow=ok preflight=ok psu-retained-gate=ok motherboard-secured-gate=ok endpoint-key=ok route-waypoints=ok route-clearance=ok generic-bypass-blocked=ok duplicate-route-blocked=ok dependent-detach-blocked=ok replay=ok authority-isolated=ok identity=stable recovery=ok
```

The build was launched windowed at 1280×720 on the active Apple Silicon/Metal workstation with `-pse-power-cable-smoke`; the runtime identified the machine as Apple M1, Unity 6000.3.21f1 and multi-threaded PhysX. The exact marker appeared once and no cable failure marker, assertion or unhandled exception appeared.

## Repository and external checkpoint

- Feature Repository Guard: [32613813494](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32613813494), success.
- Source/docs checkpoint, final Repository Guard, exact USB source-plus-evidence readback, acceptance 20/20 and Issue/Roadmap closure are the remaining checkpoint metadata steps.
- No USB success is claimed until the expected external physical volume, prior milestone chain, `.incoming-*` copy, full hash/size/path readback, Git-blob equality and atomic final rename have all been verified.

## Bounded exclusions

EPS/CPU, PCIe/GPU, SATA/Molex/fan/front-panel/data/RGB cables; electrical power-on; pin-level circuit simulation; PSU wattage, rails, transient, efficiency or headroom; POST/BIOS/OS; completed benchmark scoring; free-rope physics; final art/audio/VFX/UI; and native Windows/Steam release validation are not part of Issue #61.

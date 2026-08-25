# Canonical Power Supply Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#81](https://github.com/cixanla/PC-Shop-Empire-3D/issues/81)<br>
**Technical head:** `f3d80629e09c05afde97fa778c4b220ca456c5f0`<br>
**Technical tree:** `851954879c1ff1e2ef98bc9a7a8469750304d992`<br>
**Closure status:** source/domain/scene/input/full-regression, exact-head macOS/Windows native, procedure-bound provenance, source/docs CI and immutable local/physical-USB double-readback gates passed; final physical-metadata CI and Issue/Project administrative closure pending

## Delivered playable result

GarageGraybox r41 adds the seventh physical component handoff for the accepted custom-PC work order. The domain resolves the canonical PSU by `ComponentKind.PowerSupply` and exact work-order/ticket/allocation line, product, serialized item and reservation identity. The player takes that exact object using real `E / Gamepad South`, carries it, rotates its keyed 180° preview and places it into a power-supply-specific capacity-one managed BuildKit slot.

The authoritative custody chain is source → `ActorHands` → power-supply BuildKit. Motherboard, processor, DDR5 memory, M.2 storage, processor cooler and graphics card remain staged in their own slots, reservation/allocation stays live and progress becomes exact `7/10`. World mutation follows domain commit. Placement recovery, replay and every failure path preserve the same Unity object and stable ItemId; generic transfer/drop/box/stack/cart, PSU-bay Assembly and all three cable-routing paths cannot bypass active BuildKit custody.

The post-prerequisite portion is not a transform-snap shortcut. Native smoke resets to the authored spawn and drives actual `CharacterController` movement and mouse delta through spawn → PSU → PSU BuildKit. It records bounded horizontal movement, stable player parent, no transform snap and collision-safe arrival. Teleport assistance is confined to prerequisite setup and is named explicitly in the marker.

The exact runtime success marker is:

```text
GARAGE_POWER_SUPPLY_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card-staged power-supply-pickup=exact physical-identity=stable carry=ok prerequisite-positioning=teleport-assisted post-prerequisite-input=keyboard+mouse post-prerequisite-return=authored-spawn post-prerequisite-route=authored-spawn>power-supply>power-supply-build-kit movement=character-controller look=mouse-delta route-horizontal-step-envelope=bounded player-parent=stable post-prerequisite-route-no-transform-snap=ok route-collision=ok custody-guards=ok rotation=180 placement=ok progress=7/10 reservation=alive custody=power-supply-build-kit receipts=ok revisions=ok assembly=untouched power-supply-bay=untouched atx24-route=untouched eps12v-route=untouched pcie-route=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

## Native-frame lifecycle repair

The first universal Mac native attempt surfaced a real test-harness flaw: nested prerequisite setup pressed `E`, forced `InputSystem.Update()` and directly called the work-ticket station in the same frame. This bypassed production Update order and falsely reported a missing ticket.

The accepted technical head replaces that shortcut with neutral → pressed → released Unity player frames for motherboard, processor, memory, storage and cooler prerequisite callers. Pressed-frame gate/unconsumed/station diagnostics are captured before release. Storage smoke ends on a normal player frame, so `-nographics` PlayMode terminates cleanly. The focused regression executes the production Storage coroutine and proves exact `4/10` progress with stable identity/replay/invariants.

## Exact source and full regression

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 697/697 | `editmode.xml` | 581,077 | `fa8e521f7c9de01860937083dc6472ef9b75509a41c79e70543c0ca6503d9727` |
| PlayMode | 105/105 | `playmode.xml` | 293,619 | `588b1e351362aeffe981b51f1df42ede18e008bd209351c23764b0d6c7e4c1e1` |

Both suites report failed, skipped and inconclusive `0`. The committed technical tree is clean and `git diff --check` passes. Focused domain, Septuple ownership, input/placement, scene contract, human-route and native-frame lifecycle gates also pass.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development build | Success | `macos-build.log` | 590,529 | `1af41bdb0d2d5c240f8b3313ad4c34266f038f51384b26979f046de43ea74f9d` |
| Apple M1/Metal native runtime | Success | `macos-runtime.log` | 9,020 | `7b8d6cf8cd102134532e1a156e68ee3a7d92c3331b1025fbb93e849b9e87879b` |
| App executable | Universal + valid deep/strict signature | `PC Shop Empire 3D` | 117,179 | `947aaa5748a1bce44d521f82fc32ada442567e692251b50098262d8d9a4b55e6` |

The build report is `329,907,140` bytes. `file` confirms `x86_64` and `arm64`; deep/strict `codesign --verify` passes. The 1280×720 player emits exact r41 readiness and exact PSU success once, emits no BuildKit failure/assertion/unhandled-exception marker, exits `0` through graceful close and leaves no player process.

## Windows exact-source IL2CPP and Direct3D11 gate

The complete Git bundle is `7,212,319` bytes, SHA-256 `7074973b9864154efe1114053140bc60f70513176b7e3042cbbcbe9e53c2b99e`. Windows readback is byte exact. It exposes only `feature/issue81-power-supply-build-kit-handoff` at the technical head and passes `git bundle verify`.

The accepted validation root is `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue81-f3d80629-cold-v1`. It is detached, clean and exact to technical commit/tree before and after EditMode, PlayMode, build and runtime. Unity-generated URP/ProBuilder/SceneTemplate deltas from cold import were inspected and restored before the native gate; source remained clean.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/Direct3D11 build | Success | `build-il2cpp-d3d11.log` | 567,058 | `06096e251a43541040618ca0c407873bf87477d1df5973467568367de2019731` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `dd4c93e38bfeb37c2475d5cd19ea983a5d01be8ecbe5b7ecb48cd4f7ae8827cc` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,963 | `1530a779f0eaafce40a381bc583a1ff29a8c4a7e5067328588e2c66135d3fd22` |
| Runtime summary | Exit/task/residue contract passed | `runtime-summary.json` | 3,294 | `c092c2389699aff310192cf185a2b7e45614ef75fba2339722d243cbc95dd318` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,698 | `e5cb25d36c80dd19c064a701973863ecb9bab8ee3b1be141fd432ca18f195b62` |

The Windows build report is `1,335,888,266` bytes and the `issue81-hardened-v1` Burst/native-link fatal-token count is `0`. The binary manifest locks:

- `PC Shop Empire 3D.exe`: `667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: `45,285,376` bytes, `b3151a42f0a4860b7b193d020f671d5a90eddfe0be4fef7a6a4eceb4db61d907`.
- `UnityPlayer.dll`: `84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

The logged-on-user player reports Intel Iris Xe, Direct3D 11.0 feature level 11.1 and 1280×720. Exact host, r41 readiness and PSU success markers each appear once; forbidden markers are `0`. The player exits `0`, graceful shutdown is true, scheduled task `PSE-Issue81-f3d80629-R1` is deleted, cleanup is unnecessary and player/Unity/task residue is `0`.

Procedure source is hash-bound:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,610 | `fa42bc578590c5831caba809a250679bbef25d112b346ede6c7bf57b5d4d9753` |
| `launch-procedure.ps1` | 7,951 | `dde6f02a0f239ff0413c4dcf4b71aabc06ce1695bb8e5bac651dbe63ece6ad69` |
| `runtime-procedure.ps1` | 14,937 | `5832b63da6c0c5133d0c2317aa434326ccf6f452aaf10581bc5eb1feaca0bed5` |
| `procedure-manifest.json` | 997 | `7cc84026456e311efbdc7b44c36edc6a975c984155a41041f62dfe074e7eaa8e` |

## Canonical evidence and immutable physical package

The thirteen immutable test/build/runtime/binary/procedure artifacts returned to the Mac with exact size/hash readback. Clean source/docs commit `dc118bf0d26a11f3937cb114ef12f85666facc48`, tree `ac9fcb5d38855ed37f2ee36449100b5094287cb8` and [Repository Guard 32896033674](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32896033674) then authorized final `source-receipt.json` (`5,046` bytes, SHA-256 `ef6f720ab762872815224fdc169d8fadd80b4c627651cc5353411f3f4e19c238`). Promoted-artifact path/size/hash equality is `13/13`; canonical evidence is exact `14/14` at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue81-f3d80629e09c/canonical-evidence`.

`Tools/verify-checkpoint-package.sh ... issue81` is fail-closed on:

- Exact technical commit/tree and source/docs ancestry.
- Exact nine-file technical→source/docs closure allowlist.
- EditMode `697/697`, PlayMode `105/105` and zero failures/skips/inconclusive.
- `issue81-hardened-v1`, build fatal-token `0`, one exact r41 PSU marker and runtime forbidden-token `0`.
- Three procedure hashes, three native binary hashes, promoted-artifact equality, interactive task deletion, graceful exit and residue `0`.
- Exact `14/14` evidence names, Git-source equality, full manifest readback, no symlink/cache/secret/AppleDouble and no final-side incoming residue.

- Local final: `/Users/cixanla/Developer/PCShopEmpire3D/CheckpointStaging/2026-08-25_STAGE_B_CANONICAL_POWER_SUPPLY_BUILDKIT_HARDENED_V1`.
- Physical final: `D:\CIXANLA\90_BACKUPS\PCShopEmpire3D\2026-08-25_STAGE_B_CANONICAL_POWER_SUPPLY_BUILDKIT_HARDENED_V1` on `Intenso Alu Line`, disk `1`, serial `900B00076010`, USB/exFAT volume label `cixanla`, volume serial `B2CC-F8C9`.
- Previous-chain proof: the existing Issue #79 milestone remained read-only and its manifest stayed `d2d399fa71ee37ed972b2e709987d0a375fe62fd8da3e5cfda5eb0ec571bb324` before the new write.
- Read-only `chkdsk D:` found no filesystem problem, no further action requirement and `0` bad sectors; no repair operation was performed.
- Local incoming and same-filesystem atomically named local final both returned `CHECKPOINT_PACKAGE_OK` in exact Issue #81 mode.
- Physical `.incoming-issue81-dc118bf` passed a full hash/size/path readback, was atomically renamed without collision, and the final path passed the same full Windows readback plus an independent Mac pullback verification.
- Every full readback is identical: `1002/1002` payload, `987/987` exact Git source, `14/14` evidence, source/docs commit `dc118bf0d26a11f3937cb114ef12f85666facc48`, tree `ac9fcb5d38855ed37f2ee36449100b5094287cb8`, `19,368,668` payload bytes and manifest SHA-256 `69cc892b816820918b5612944266dc2cb077cd01ed41f78a3d6523c7409206ab`.
- Physical incoming directory/sidecar residue, internal AppleDouble, exact final sidecar and Windows issue-specific process residue counts are all `0`; older milestones and unrelated user data were not modified.

## Issue #81 acceptance matrix — current technical state

| # | Acceptance contract | Current gate | Evidence |
|---:|---|---|---|
| 1 | Exact PowerSupply role and full line/product/item/reservation tuple. | TECHNICAL PASS | Domain resolver and exact allocation cross-check tests. |
| 2 | No ordinal/display-name/value/regenerated identity authority. | TECHNICAL PASS | Forgery/value-equal/no-mutation matrix. |
| 3 | Separate stable operation and capacity-one PSU BuildKit slot. | TECHNICAL PASS | Append-only stages and capacity tests. |
| 4 | Seven containers are claimed atomically through Septuple access. | TECHNICAL PASS | Duplicate/foreign/partial topology tests. |
| 5 | First six slot/receipt/replay/revision/staged states stay unchanged. | TECHNICAL PASS | Aggregate preservation assertions. |
| 6 | Immediate/delayed replay returns one canonical receipt. | TECHNICAL PASS | Pickup/place history and replay matrix. |
| 7 | Foreign/wrong kind/line/product/item/reservation/order/operation fails closed. | TECHNICAL PASS | Expanded identity-forgery matrix. |
| 8 | Full hands, occupied kit, drift and stale revisions are no-mutation. | TECHNICAL PASS | Conflict/capacity/stale tests. |
| 9 | Custody is only source → hands → PSU BuildKit; generic bypasses fail. | TECHNICAL PASS | Narrow allocation bridge and bypass tests. |
| 10 | Reservation and allocation remain exact/live. | TECHNICAL PASS | Receipt assertions and both native markers. |
| 11 | First six staged prerequisites prevent skipping `6/10`. | TECHNICAL PASS | Prerequisite matrix and smoke setup. |
| 12 | Real `E / Gamepad South` pickup preserves range/focus/LOS/empty hands. | TECHNICAL PASS | Keyboard/mouse and gamepad PlayMode paths. |
| 13 | Separate PSU target, one support collider, exact anchor, gates and 180° preview. | TECHNICAL PASS | Scene contract and preview tests. |
| 14 | BuildKit is the single input consumer; PSU bay and cable modes cannot steal the frame. | TECHNICAL PASS | Input arbiter/isolation tests. |
| 15 | Co-edge, held, pause and release-repress are deterministic. | TECHNICAL PASS | Real Input System matrix. |
| 16 | Domain failure keeps the same PSU in hands before world mutation. | TECHNICAL PASS | Domain-first projection tests. |
| 17 | Physical placement failure recovers the same instance. | TECHNICAL PASS | Same-instance recovery tests. |
| 18 | Unity instance, ItemId, collider/layer/ownership and authority container remain exact. | TECHNICAL PASS | World→Hands→BuildKit identity assertions. |
| 19 | Progress derives from staged receipts and becomes `7/10`. | TECHNICAL PASS | Work-ticket aggregate and visible runtime marker. |
| 20 | PSU Assembly, three cable routes, quote/reservations/items remain untouched. | TECHNICAL PASS | Isolation snapshots and regression suite. |
| 21 | WASD/mouse/gamepad human scenarios work. | TECHNICAL PASS | CharacterController + mouse-delta native route and PlayMode matrix. |
| 22 | Focused and full EditMode/PlayMode regression pass. | TECHNICAL PASS | `697/697`, `105/105`, zero non-pass. |
| 23 | Diff, Guard, Mac and Windows native gates pass. | TECHNICAL PASS | Diff, source/docs Guard `32896033674`, exact-head Mac and detached-clean Windows IL2CPP/D3D11 gates pass. |
| 24 | Docs, private push/CI and physical USB lifecycle complete. | FINAL METADATA PENDING | Source/docs Guard, final receipt, local package and two physical USB readbacks pass; this metadata commit/Guard and administrative closure remain. |

## Pending lifecycle sequence

1. Commit and push this physical lifecycle metadata and require Repository Guard/CI success.
2. Mark all `24/24` acceptance boxes, close Issue #81 and set its Roadmap item to `Done`; make PR #82 ready for integration.
3. Record the exact physical-metadata commit/Guard and closed lifecycle state in the final project handoff before starting the next gameplay slice.

Parent Epic #10 remains open for the three cable BuildKit handoffs and later assembly/electrical/POST/OS/QA stages. Issue #77 remains independently open/In Progress until its own physical lifecycle closes.

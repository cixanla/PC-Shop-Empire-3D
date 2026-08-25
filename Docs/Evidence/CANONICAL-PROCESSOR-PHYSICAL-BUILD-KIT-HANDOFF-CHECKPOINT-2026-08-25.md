# Canonical Processor Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 25 August 2026<br>
**Issue:** [#71](https://github.com/cixanla/PC-Shop-Empire-3D/issues/71)<br>
**Draft PR:** [#72](https://github.com/cixanla/PC-Shop-Empire-3D/pull/72)<br>
**Technical head:** `11683c8b567ad6edcd6777610875aeebd0e509ef`<br>
**Technical tree:** `6890157f3f3625661314b34700259e0933ff2677`<br>
**Closure status:** technical domain/scene/input, full regression and macOS/Windows native gates passed; source/docs, immutable package, physical USB and Issue/Project lifecycle gates remain pending

## Delivered playable result

GarageGraybox r36 adds the second physical component handoff for the accepted custom-PC work order. The domain resolves the canonical processor by `ComponentKind.Processor` and the exact work-order/ticket/allocation line, product, serialized item and reservation tuple. The player takes that exact object using real `E / Gamepad South`, carries and rotates it, and places it into a processor-specific capacity-one managed BuildKit slot.

The authoritative custody chain is source → `ActorHands` → processor BuildKit. The motherboard prerequisite remains staged in its own slot, reservation/allocation identity stays live, and progress becomes exact `2/10`. World mutation occurs only after domain success. Placement recovery, exact replay and failure paths keep the same Unity component and stable `ItemId`; generic transfer/drop/stack/cart and ProcessorSocket/Assembly paths cannot bypass active BuildKit custody.

The runtime success marker is:

```text
GARAGE_PROCESSOR_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisite=motherboard-staged processor-pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=ok placement=ok progress=2/10 reservation=alive custody=processor-build-kit receipts=ok revisions=ok assembly=untouched processor-socket=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

## Exact source and full regression

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 677/677 | `editmode.xml` | 565,203 | `a604d5a5b52339d0b62f5faf37330f0bd23296071e85c0026af0ad2cb16a2bc7` |
| PlayMode | 81/81 | `playmode.xml` | 215,630 | `187071ba92db4d6feda3f19b25975678766163b5a432219420f7d42875e86e0e` |

Both suites report failed, skipped and inconclusive `0`. The final PlayMode run generated only one ProBuilder preference entry and one untracked scene-template settings file. Both were created by this run, were restored/removed without touching game source, and the authoritative checkout returned to exact clean HEAD. `git diff --check` passes. The technical source [Repository Guard 32827174483](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32827174483) is successful.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development/StrictMode build | Success | `macos-build.log` | 588,102 | `df017e9454cce7abf53942c0670e1434de01ea85e5a69b0ae2f76991188f6404` |
| Apple M1/Metal native runtime | Success | `macos-runtime.log` | 6,385 | `ab1421cffec4b9134c4c8211f81a078892509658395d73a26884d56a305735e4` |
| App executable | Universal + valid ad-hoc signature | `PC Shop Empire 3D` | 117,179 | `e146c3e8165499248d16f3cad8f4d33b800186a5aecbafc07f179df485535c21` |

The build report is `329,627,927` bytes. `file` confirms `x86_64` and `arm64`; deep/strict `codesign --verify` passes. The 1280×720 player used Apple M1/Metal, emitted exact r36 readiness and exact CPU BuildKit success once, emitted no BuildKit failure/assertion/unhandled-exception marker and left no player process. `/Users/cixanla/Desktop/PC Shop Empire 3D.app` resolves to this current build.

## Windows exact-source IL2CPP and Direct3D11 evidence

The accepted isolated validation root is `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue71-11683c8b567a-hardened-v2`. A `6,634,318`-byte Git bundle with SHA-256 `3a4f99c4f4cc32463ba251512a35bc59fdbdd8b7df78b569c1d67f4d69cc0196` produced one detached clean clone at the exact technical head/tree. The first validation root remains immutable and is not acceptance evidence: its early import phase contained a recovered Burst linker error that the original narrow forbidden filter did not count. The `hardened-v2` procedure expands the fatal policy to Burst internal compiler errors, `AotLinkerException`, native-link failure, `Win32 IO returned 232` and `burst-lld` command failures; the accepted build log contains zero such tokens. Older validation roots were not modified.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/D3D11 build | Hardened-v2 success | `build-il2cpp-d3d11.log` | 1,582,982 | `51883cbe132dd0004e585f75336f9c9481bd1baa3004d0bf6aad32c2015d8bac` |
| Native binary receipt | 3/3 | `binary-manifest.json` | 1,292 | `9335de3fd1e9ea945a37902d37a32eaec07aa6ebbf663aa49a70f819018d0617` |
| Interactive D3D11 runtime | Success | `runtime-d3d11.log` | 5,350 | `13def312ca350e4028d90a2b542d84315d3c699fc51bc756384a9e9fc1e673d9` |
| Runtime summary | Accepted | `runtime-summary.json` | 3,293 | `8c67f519a30abe5a679cf1d21b324b013dab2bb18e0edf365b7c39d82a778c6a` |
| Interactive task cleanup | Success | `task-receipt.json` | 1,701 | `947854407f8ca9c6f9780a36fe23cfe3120635a6758878a9e6b088ff070efba2` |

The Windows build report is `1,329,802,474` bytes and `ProjectSettings.asset` pre/post hashes are byte-exact. The exact build marker count is `1`, the `issue71-hardened-v2` fatal-token count is `0`, and Unity exited `0`. Native readback:

- `PC Shop Empire 3D.exe`: 667,136 bytes, SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: 45,020,160 bytes, SHA-256 `520c2cac627376ec32ef5c19adcec2ba996e9f791c81687ab9834441f44b4f5f`.
- `UnityPlayer.dll`: 84,237,744 bytes, SHA-256 `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

The logged-on interactive player used Intel Iris Xe / Direct3D11. Player exit and scheduled-task result are `0`; r36 readiness and exact success counts are `1`; forbidden count is `0`; shutdown is graceful; the temporary task was deleted; cleanup was unnecessary; process residue is `0`; and the detached validation source remains clean.

## Procedure-bound canonical evidence

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,295 | `8e33e3740286e0f161cedc82ff59244756ab6cca1194fb674fb7f42a34ccb68d` |
| `launch-procedure.ps1` | 7,963 | `1847d8fd1c66f8f37bd3ebda6e3d4b469253292c376249eecfa76711cbea8b90` |
| `runtime-procedure.ps1` | 13,832 | `77068805c3fd0df4c5c6a1029916023ab5dc3d080908f0b18719e6bf9323e983` |
| `procedure-manifest.json` | 997 | `9fc238a7428f97f08609d6700dafddc77e2567aa895361349b5f654f932d4470` |

Canonical evidence root: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue71-11683c8b567a/canonical-evidence`. It contains exactly these `14/14` top-level files:

1. `binary-manifest.json`
2. `build-il2cpp-d3d11.log`
3. `build-procedure.ps1`
4. `editmode.xml`
5. `launch-procedure.ps1`
6. `macos-build.log`
7. `macos-runtime.log`
8. `playmode.xml`
9. `procedure-manifest.json`
10. `runtime-d3d11.log`
11. `runtime-procedure.ps1`
12. `runtime-summary.json`
13. `source-receipt.json`
14. `task-receipt.json`

`source-receipt.json` is 4,483 bytes with SHA-256 `9bf6aa7b9ade2e1bae519a130443e658306e569d55f91cb9dc7d9cec05d14539`. It binds technical commit/tree, clean status, Repository Guard, full tests, final Mac build/runtime, accepted `hardened-v2` Windows build/runtime/task cleanup and exact readback of the other 13 promoted artifacts. The outer checkpoint manifest supplies the receipt's own immutable hash.

`Tools/verify-checkpoint-package.sh ... issue71` requires the exact 14-name evidence set plus full manifest path/size/SHA, exact Git-source, forbidden/cache/credential, secret-signature, symlink and AppleDouble gates. Its Issue #71 semantic gate additionally parses source/binary/procedure/runtime/task receipts, binds the fixed technical commit/tree and all procedure/promoted-artifact hashes, requires the packaged source/docs commit to descend from that technical commit with an exact nine-file docs/verifier-only delta, requires the `issue71-hardened-v2` policy, rejects Burst/native-link tokens and requires one exact CPU runtime marker. On a final package it enforces remaining sibling `.incoming-*`, `._.incoming-*` and exact final-name AppleDouble residue `0` without touching unrelated historical milestone sidecars. Omitting `issue71` intentionally falls back to generic canonical behavior and is not valid for Issue #71 closure.

## Canonical Issue #71 acceptance matrix

`TECHNICAL PASS` means exact technical source plus promoted test/Mac/Windows evidence proves the item. Row 22 remains pending until source/docs push/CI, immutable local package, physical USB incoming/final readbacks and Issue/Project lifecycle are separately complete.

| # | Acceptance contract | Current gate | Canonical evidence |
|---:|---|---|---|
| 1 | Exact Processor role and full line/product/item/reservation tuple are cross-validated. | TECHNICAL PASS | `CustomPcBuildKitAuthority`, work-order allocation checks and forgery tests. |
| 2 | Ordinal, display name, product-value equality and regenerated identity are non-authoritative. | TECHNICAL PASS | Wrong-line/value-equal/regenerated identity no-mutation matrix. |
| 3 | Processor uses a distinct stable operation and capacity-one managed BuildKit slot. | TECHNICAL PASS | Processor operation/container identities, capacity and ownership tests. |
| 4 | Motherboard slot, receipt, replay, revision and staged state remain unchanged. | TECHNICAL PASS | Two-component aggregate and motherboard preservation assertions. |
| 5 | Exact child operation immediate/delayed replay returns the same receipt without custody/revision duplication. | TECHNICAL PASS | Pickup/place receipt-history and replay tests. |
| 6 | Foreign/value-equal/wrong-kind/line/product/item/reservation/order/operation fails closed. | TECHNICAL PASS | Expanded exact forgery and owner-mismatch matrix. |
| 7 | Full hands, occupied slot, source drift and stale BuildKit/Inventory revisions are no-mutation. | TECHNICAL PASS | Conflict, stale and capacity tests. |
| 8 | CPU custody is only source → hands → CPU BuildKit; generic bypasses fail closed. | TECHNICAL PASS | Domain tests, generic cart/drop tests and native custody guards. |
| 9 | Reservation and allocation stay exact and live at each custody step. | TECHNICAL PASS | Receipt identity and runtime success marker. |
| 10 | Motherboard prerequisite blocks CPU and progress cannot skip `1/10`. | TECHNICAL PASS | Prerequisite tests and native `prerequisite=motherboard-staged`. |
| 11 | Real `E / Gamepad South` pickup preserves range/focus/LOS/empty-hands gates. | TECHNICAL PASS | Keyboard/mouse and gamepad PlayMode pickup tests. |
| 12 | Separate CPU tray has one support collider, exact anchor and placement gates. | TECHNICAL PASS | Scene contract, projection evaluation and static scene audit. |
| 13 | BuildKit contextual primary/rotate/drop is single-consumer; socket cannot steal it. | TECHNICAL PASS | Receipt-owned arbiter and legacy socket isolation test. |
| 14 | Same-frame/co-edge, held input, pause and release-repress are deterministic. | TECHNICAL PASS | Keyboard/gamepad same-frame and pause matrix. |
| 15 | Domain failure leaves the same CPU in hands before physical mutation. | TECHNICAL PASS | Preflight/domain-failure state and identity assertions. |
| 16 | Physical `PlaceAt` failure recovers the same instance at the authoritative tray pose. | TECHNICAL PASS | Recovery tests and no-clone runtime invariant. |
| 17 | Unity instance, stable ItemId, collider/layer/ownership and container stay exact. | TECHNICAL PASS | Scene/binding invariants and native identity marker. |
| 18 | Work ticket derives visible `1/10 → 2/10` without mutating immutable ticket data. | TECHNICAL PASS | Projection status, aggregate authority and runtime `progress=2/10`. |
| 19 | ProcessorSocket/Assembly, motherboard Assembly, electrical, quote and other reservations remain untouched. | TECHNICAL PASS | Revision/receipt snapshots and native isolation marker. |
| 20 | Domain/invariant/replay/forgery, scene and real Input System test matrices pass. | TECHNICAL PASS | EditMode `677/677`, PlayMode `81/81`. |
| 21 | Full regressions, diff check, Guard, Mac and exact-head Windows native gates pass. | TECHNICAL PASS | Guard `32827174483`, exact test/Mac/Windows artifacts above. |
| 22 | Living docs, private push/CI and physical USB incoming→readback→atomic final lifecycle pass. | PENDING | Source/docs, package, USB and Issue/Project closure have not yet been claimed. |

## Remaining closure sequence

1. Commit and push ADR-0045, this Evidence record, Project Bible/CHANGELOG and `issue71` verifier mode.
2. Require the source/docs Repository Guard to pass.
3. Build a collision-free immutable local package from that exact source/docs commit and require full `issue71` verifier readback.
4. Rediscover the correct external physical USB and prior milestone chain; use only a collision-free `.incoming-*` target, full readback, same-filesystem atomic rename and a second full readback.
5. Record physical manifest/count/bytes/path metadata, pass final Guard, then mark all 22 Issue boxes, move the Roadmap item to Done and complete PR/Issue lifecycle while parent Epic #10 remains open.

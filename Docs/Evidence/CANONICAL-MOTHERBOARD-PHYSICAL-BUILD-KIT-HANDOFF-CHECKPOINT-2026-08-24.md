# Canonical Motherboard Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 24 August 2026<br>
**Issue:** [#68](https://github.com/cixanla/PC-Shop-Empire-3D/issues/68)<br>
**Draft PR:** [#69](https://github.com/cixanla/PC-Shop-Empire-3D/pull/69)<br>
**Feature chain:** `2a69436` + `b0d2a97`<br>
**Technical head:** `480874191ee2c950e046ab2aee8be92d61d79fe4`<br>
**Technical tree:** `e229788741df4c456840d356633e2a4bc1702516`<br>
**Closure status:** domain/scene/input, exact-head regression, macOS and Windows native technical gates passed; source/docs commit, immutable local package, physical USB two-readback, final metadata/Guard and Issue/Roadmap lifecycle remain independent pending gates

## Delivered playable result

GarageGraybox r35 adds the first physical component transfer for the accepted custom-PC work order. The domain resolves the canonical motherboard from exact work-order/ticket/allocation lineage using its full line, product, serialized item and reservation tuple. The player then takes that exact object with real `E / Gamepad South`, carries and rotates it, and places it into a separate capacity-one managed BuildKit slot.

The authoritative custody chain is exact source → ActorHands → BuildKit. Domain success precedes world mutation. The same Unity component and stable `ItemId` remain alive across pickup, preview, placement, exact replay and recovery. A failed physical projection change recovers the same instance at the authoritative pose rather than creating a replacement. Generic world drop, stacking, cart and Assembly seat paths cannot bypass active build-kit custody.

The work ticket remains `0/10` before authoritative placement and becomes exact `1/10` with motherboard role after placement. The live reservation and work-order allocation receipt remain exact. The quote price, other nine reservations/items and all Assembly revision/state/receipts remain unchanged; the motherboard is staged, not seated or consumed.

The runtime success marker is:

```text
GARAGE_MOTHERBOARD_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=ok placement=ok progress=1/10 reservation=alive custody=build-kit assembly=untouched replay=ok invariants=ok
```

## Source identity and exact-head full regression

The exact validation clone was created detached at technical head `480874191ee2c950e046ab2aee8be92d61d79fe4`, tree `e229788741df4c456840d356633e2a4bc1702516`, with empty Git status. Unity 6000.3.21f1 ran the verified batch flow without `-quit`. The first import-only attempt that used `-quit` produced no XML and is retained only as diagnostic r0; it is not promoted evidence.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 675/675 | `editmode.xml` | 563,666 | `ce58006c0a39ff305d3e21379707e2330b01c2802a8f4af846065e2280996116` |
| PlayMode | 73/73 | `playmode.xml` | 193,242 | `d8879a51ccc9c15c62a8f250102cd72939bf651792867ba59a0351d516881f5c` |
| Committed scene | r35 | `Assets/Scenes/Prototypes/GarageGraybox.unity` | 2,987,636 | `ba7fad7361d29164dd714bfe124da7d64c0fb1b17f713d32b52c4e800350b124` |

Both XML suites report failed, skipped and inconclusive `0`. Unity generated only `ProjectSettings/Packages/com.unity.probuilder/Settings.json` and `ProjectSettings/SceneTemplateSettings.json` editor-setting deltas in the dedicated validation clone; those generated deltas were removed, and the same clone then passed exact HEAD/tree, empty status and `git diff --check` readback. No authoritative game, scene, domain or test source changed.

## macOS native evidence

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Universal Development/StrictMode build | Success | `macos-build.log` | 593,139 | `88a9b5dc5263445784cd7160ac108a99e9385ad1ccb7f3e6f867fc7e2aa23281` |
| Apple M1/Metal native runtime | Success | `macos-runtime.log` | 8,254 | `6fcce8f65f7118345caaecfb3b173c86b861afdb39a440ba286feb7e3264d193` |
| App executable | Universal + signed | `PC Shop Empire 3D` | 117,179 | `eee696587e727edfd7d6f344f8a4f229778da013cebfed0d3d857d5d8126bed0` |

The build report is `329,571,495` bytes. The executable contains `x86_64` and `arm64` Mach-O slices and passes deep/strict codesign verification. The 1280×720 player used Apple M1/Metal, emitted `garage-motherboard-build-kit-r35-v1` readiness and the exact success marker once, emitted no build-kit failure/assertion/unhandled-exception marker, and shut the Input System down. `/Users/cixanla/Desktop/PC Shop Empire 3D.app` resolves to this verified build.

## Windows exact-source IL2CPP and Direct3D11 evidence

The Windows validation root is `C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue68-480874191ee2`. A `6,518,675`-byte Git bundle with SHA-256 `9489a57c0821a784ad1cd965a45fb629cabfd6c732582f9d97eea31fb5e477d8` produced one detached clean source at the exact technical head/tree. Unity 6000.3.21f1 built x64 IL2CPP with only Direct3D11 and restored `ProjectSettings.asset` byte-exactly in `finally`.

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/D3D11 build | Success | `build-il2cpp-d3d11.log` | 1,582,868 | `c769d348c411297600b467f30570e2fc4a538c4bccc3ad8e9d4156addc7be483` |
| Native binary receipt | 3/3 | `binary-manifest.json` | 1,239 | `34492cc9205bcb1050c99a6985f114f3a1398be49d430f2a31a25c09e326a444` |
| Interactive D3D11 runtime | Success | `runtime-d3d11.log` | 5,190 | `1ca8e29cd6413e51db2f0d994ef37401dafca0ebcd58ce8c972141b5a20939cb` |
| Runtime summary | Accepted | `runtime-summary.json` | 3,293 | `1377a30ce464f9f51597fd42868d2a1f5df2a84148f494dae3d237ebd8d73987` |
| Interactive task cleanup receipt | Success | `task-receipt.json` | 1,699 | `66c685f6ee395841f162431ee6bbb52657191cde52c5c02010075bce8afc316c` |

The build report is `1,327,308,678` bytes. Native readback:

- `PC Shop Empire 3D.exe`: 667,136 bytes, SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: 44,928,512 bytes, SHA-256 `8b9b7401d2d41f01de0e5d06dac713231270ee8fe79bb3ddc9925880b271c3ed`.
- `UnityPlayer.dll`: 84,237,744 bytes, SHA-256 `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

The final logged-on interactive player used Intel Iris Xe / Direct3D 11.0 feature level 11.1. Host, r35 readiness, exact Issue #68 success and Input System shutdown marker counts are each `1`; forbidden token count is `0`. Player and scheduled task exit codes are `0`, shutdown is graceful, the temporary task was deleted, the validation source remained clean and process residue is `0`.

Two failed wrapper attempts are retained as diagnostics only. They proved fail-closed behavior and exposed a PowerShell empty-log-line parameter-binding defect before canonical evidence promotion. The final wrapper accepts empty log lines, writes a top-level error receipt on any terminating exception, and succeeded without cleanup. Diagnostic attempts are excluded from the canonical evidence directory and checkpoint contract.

## Procedure-bound evidence contract

The promoted Windows runtime does not trust only its own summary. It first re-reads exact source identity, the three native binary hashes and all three procedure hashes. The launcher independently repeats procedure-manifest readback and accepts the runtime summary only when its source, binary-manifest and procedure-manifest hashes match.

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 8,991 | `e67a0d8c5dc0c2ad1b69c62bf76a35e4c7d81b7480fb0259f3bc4ec98515c527` |
| `launch-procedure.ps1` | 7,948 | `3ac0ca7246f72fc35447af8837bf3721bf5f472e0ac11f30697398bdf85f351a` |
| `runtime-procedure.ps1` | 13,509 | `094fcb06e332ec607740492ec7ddb0934f0f08bf9e95e2d2bb398ee692512877` |
| `procedure-manifest.json` | 635 | `6a195ceedf04cee27704d57d7d6bed5b47ee8b5b29c8d074bb4ea4a1818258dd` |

The canonical local evidence source is `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue68-4808741`. It contains exactly 14 files and `2,991,859` bytes:

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

`source-receipt.json` is 8,186 bytes with SHA-256 `f9fbaed3f80ff42dd1856ab2766455d68feda5dcfd957a95ed96c1e3854c61bd`. It binds the exact technical commit/tree, clean-clone test commands/results, Mac build/runtime, Windows source/build/binaries/runtime/procedures/task cleanup and technical Guard. It lists the other 13 artifacts by path/size/hash and deliberately does not self-hash; the outer checkpoint manifest supplies its immutable hash.

`Tools/verify-checkpoint-package.sh ... issue68` requires this exact `14/14` name set in addition to manifest, path, size, SHA-256, exact Git-source, forbidden/cache/credential, secret-signature, symlink and AppleDouble gates. Omitting the Issue-specific mode intentionally uses generic canonical behavior and is not valid for Issue #68 closure.

## Canonical Issue #68 acceptance matrix

This is the repository-owned one-to-one rendering of the 20 acceptance bullets. `TECHNICAL PASS` means the exact technical source plus promoted test/Mac/Windows evidence proves the behavior. `PASS` on row 20 additionally requires source/docs push/CI, immutable local staging, physical USB incoming/final double readback and Issue/Project lifecycle closure.

| # | Acceptance contract | Current gate | Canonical evidence |
|---:|---|---|---|
| 1 | Exact motherboard line/ProductId/ItemId/ReservationId is cross-validated with work order, ticket and allocation. | TECHNICAL PASS | `CustomPcBuildKitAuthority` tuple validation and exact-domain tests. |
| 2 | Ordinal, display name, suffix and regenerated reservation identity are non-authoritative. | TECHNICAL PASS | Wrong-line/value-equal/forged identity no-mutation tests and ADR-0044. |
| 3 | Stable child operation ID returns the same receipt on immediate/delayed exact replay without revision drift. | TECHNICAL PASS | Pickup/place replay and receipt-history tests. |
| 4 | Foreign/value-equal/wrong-kind/wrong-line/wrong-item/wrong-reservation/wrong-work-order replay fails closed without mutation. | TECHNICAL PASS | Expanded EditMode forgery and cross-owner matrix. |
| 5 | Dedicated managed capacity-one BuildKit custody is separate from Assembly Workbench. | TECHNICAL PASS | `InventoryContainerKind.BuildKit`, create/capacity tests and r35 authored target. |
| 6 | Reserved movement uses only the narrow work-order-allocation bridge; generic raw transfer is not widened. | TECHNICAL PASS | Build-kit Inventory access layer plus generic-transfer bypass rejection. |
| 7 | Reservation and allocation receipt remain live and exact through source→Hands→BuildKit. | TECHNICAL PASS | Domain snapshots and runtime `reservation=alive custody=build-kit`. |
| 8 | Real pickup uses E/Gamepad South with range/focus/LOS/empty-hands gates. | TECHNICAL PASS | Real Input System pickup matrix and authored targeting. |
| 9 | Real placement uses dedicated BuildKit target; preview and commit pose match. | TECHNICAL PASS | Keyboard/mouse/gamepad placement, rotation and pose assertions. |
| 10 | Wrong item, full hands, occupied slot, obstruction, invalid pose, stale revision and competing target fail closed. | TECHNICAL PASS | Domain + PlayMode no-mutation failure matrix. |
| 11 | Same-frame Interact/Drop/Primary, hold, pause co-edge and release-repress remain deterministic and single-consumer. | TECHNICAL PASS | Real Input System arbitration tests. |
| 12 | Generic box/stack/cart and Assembly seat paths cannot bypass BuildKit custody. | TECHNICAL PASS | Bypass tests plus carried-item ownership guards. |
| 13 | Domain succeeds before world mutation; world failure rolls back or recovers the same instance. | TECHNICAL PASS | Domain-first implementation, projection compensation and recovery tests. |
| 14 | One Unity object and stable ItemId survive world→Hands→BuildKit without duplicate/ghost projection. | TECHNICAL PASS | Same-instance/identity assertions and native `physical-identity=stable`. |
| 15 | Work ticket is 0/10 before success and exact 1/10 with motherboard role after success. | TECHNICAL PASS | r35 presentation tests and native `progress=1/10`. |
| 16 | Assembly state/receipts, electrical state, quote price and other nine reservations/items remain unchanged. | TECHNICAL PASS | Cross-authority snapshot matrix and native `assembly=untouched`. |
| 17 | EditMode domain/invariant/replay/failure and real-input PlayMode pickup/carry/place/recovery matrices pass. | TECHNICAL PASS | Exact-head EditMode `675/675`, PlayMode `73/73`; failed/skipped/inconclusive `0`. |
| 18 | Full regression, Repository Guard and diff hygiene pass. | TECHNICAL PASS | Exact detached clean-clone regression, local guard, `git diff --check`, [Guard 32744068996](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32744068996). |
| 19 | Universal macOS and exact-head Windows x64 IL2CPP/D3D11 build/native smoke pass. | TECHNICAL PASS | Mac and Windows tables, receipts and exact marker/cleanup readbacks above. |
| 20 | Living docs, private push/CI, correct USB incoming→two full readbacks→atomic final and Issue/Project lifecycle are separately proven. | IN PROGRESS | ADR/Evidence/tool docs are authored locally. Source/docs commit/Guard, immutable package, physical USB and lifecycle are not yet claimed. |

## Closure boundary

Technical gameplay acceptance rows 1–19 are complete. Row 20 remains open until all of these independent gates pass in order:

1. Commit/push this ADR, Evidence, Project Bible, handoff, changelog and verifier contract; run exact Repository Guard.
2. Export exact source/docs commit into a collision-free local immutable milestone and verify it with `issue68` mode.
3. Verify the external physical USB identity and prior milestone chain read-only.
4. Copy only to a collision-free `.incoming-*` target, remove AppleDouble only inside that new target, and run a complete verifier readback.
5. Rename atomically on the same filesystem, then run the complete verifier a second time on the final name.
6. Commit/push physical metadata, pass final Guard, check all 20 Issue acceptance boxes, close Issue #68, move it to Roadmap Done, and keep parent Epic #10 open/In Progress.

Until those gates pass, this document does not claim a physical USB milestone, Issue closure, Project Done or merged PR.

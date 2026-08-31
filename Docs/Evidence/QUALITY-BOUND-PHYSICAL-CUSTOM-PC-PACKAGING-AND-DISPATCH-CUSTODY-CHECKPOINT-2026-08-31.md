# Quality-Bound Physical Custom-PC Packaging and Dispatch Custody — Checkpoint Evidence

**Date:** 31 August 2026<br>
**Issue:** [#139](https://github.com/cixanla/PC-Shop-Empire-3D/issues/139)<br>
**Draft PR:** [#140](https://github.com/cixanla/PC-Shop-Empire-3D/pull/140)<br>
**Parent branch head:** `e3f4e662daef437582e08c4d2919c5d1c0d41654` — Issue #137 repository-guard checkpoint<br>
**Technical head:** `79ea367af67549592a6ba58acd53afa74e7f25cb`<br>
**Technical tree:** `12dabe0220ffe759750d73cc25e96e2c6774221d`<br>
**Technical branch:** `codex/issue139-quality-bound-physical-packaging`<br>
**Current state:** Exact current quality release becomes one sealed physical LargeBox package. The same package instance moves through append-only `PackagingWorkbench / ActorHands / WorldFloor / TransportCart / DispatchStaging` custody with replay-safe receipts, physical rollback, no duplicate/lost item and no upstream mutation. Full exact-head Mac tests, universal build and Apple M1/Metal native r68 smoke pass. Physical Windows x64 IL2CPP/D3D11/Iris Xe and USB gates are explicitly deferred because the devices are unavailable. Issue #139 and its Roadmap card remain `OPEN / In Progress`; PR #140 remains draft and no merge or closure is claimed.

## Delivered result

`CustomPcPackageAuthority` is a separate Unity-independent downstream authority bound to one exact `CustomPcQualityReleaseAuthority`. One successful immutable package receipt binds:

- one non-empty stable package ID and seal operation ID;
- exact owner `ReadyForPackaging` quality receipt instance;
- exact work order and physical work ticket;
- exact customer binding and inventory claim;
- exact assembled build and chassis;
- expected fulfillment revision and monotonically increasing accepted revision;
- terminal `Sealed` state and initial `PackagingWorkbench` custody.

Seal creation re-evaluates owner quality history and current release before mutation. Null, foreign, stale, historical, changed or duplicate quality evidence produces no package. One quality release cannot be packaged twice, and package creation does not duplicate component inventory or create a second Assembly aggregate.

Exact same-command replay returns the same package receipt object instance. Reusing an operation ID with changed inputs conflicts. Revision mismatch and overflow fail closed. Package history is reconstructed before replay lookup; deliberate owner/mapping/revision/source corruption makes replay itself fail with `ReceiptHistoryInvalid`.

Custody is an explicit directed graph:

```text
PackagingWorkbench → ActorHands
ActorHands → WorldFloor | TransportCart | DispatchStaging
WorldFloor → ActorHands
TransportCart → ActorHands
DispatchStaging → ActorHands
```

Every accepted edge produces one immutable `CustomPcPackageCustodyReceipt` with stable operation ID, exact owner package, source/target custody, expected revision and accepted revision. Same-command replay is same-instance; changed reuse conflicts. Foreign package, wrong source, disallowed target, stale revision, malformed history and overflow reject before domain mutation.

## Physical projection and rollback safety

The GarageGraybox r68 scene contains one initially inactive `SealedCustomPcPackage` with:

- stable prototype item identity;
- `LargeBox` carry profile and 12 kg rigidbody;
- physical collider, carry offset and safe-pose recovery;
- cardboard carton, tape, two seal bands and identity label;
- exact packaging and dispatch anchors;
- one `CustomPcPackagePhysicalBinding` mapped to the owner package receipt;
- ten exact source component/cable projections.

The first packaging action reviews the exact quality file. The second seals exactly one package, activates that physical projection and hides all ten source projections so the completed PC is not represented twice. A busy-hand seal is blocked; context/pause/range/focus/LOS loss resets review without creating a package.

Pickup, cart load, cart unload, safe drop, recovery and dispatch use a three-stage protocol:

1. side-effect-free custody preflight;
2. physical item/cart operation;
3. authority custody commit.

If the domain commit fails after physical movement, the physical operation is rolled back. Rollback failure is surfaced explicitly instead of silently accepting split physical/domain state. Dispatch requires the exact package in ActorHands and places it at the exact staging anchor before committing `DispatchStaging` custody.

The packaging workbench was moved from the pre-existing work-ticket route at `z=2.05` to the clear left-wall service position `(-3.28, 0, 0.50)`. Exact keyboard/mouse and virtual-gamepad customer→work-ticket route tests both pass after the move.

## Scene and renderer budget

The r68 scene keeps one packaging station and one dispatch station. Decorative children stay `Ignore Raycast`; each station exposes only its deliberate trigger focus target. No new camera, light, NavMesh route or input action is introduced.

Intentional geometry adds nine active station/dispatch renderers and five initially inactive sealed-package renderers. Exact runtime budgets are:

| Contract | Active MeshRenderer | Total including inactive | Light | Camera |
|---|---:|---:|---:|---:|
| Retail/checkout whole scene | `473` | `502` | `5` | `1` |
| Assembly regression excluding retail hero | `468` | `493` | `4` excluding retail fill | `1` |

The authored scene has `499` direct MeshRenderer records; the player rig prefab contributes three runtime renderers. Existing Assembly and Retail hero material, shadow, motion-vector, collider and light contracts remain green.

## Exact-head Mac tests

Every accepted run below executed against technical source `79ea367af67549592a6ba58acd53afa74e7f25cb` and tree `12dabe0220ffe759750d73cc25e96e2c6774221d`.

| Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---:|---:|---:|---|
| Fulfillment seal/custody/replay/tamper domain | `4/4` | `5.8747677 s` | `6,705` | `f19bda387c784d5415371517e2e1955bce66fc277aff7138cb00d943c2bb4b83` |
| GarageGraybox r68 scene contract | `13/13` | `1.1485965 s` | `13,424` | `8b7dd0f2e7b79801199f8b5e63d241b098bca899787fcbf795c7a4a62a143bf5` |
| Packaging keyboard/gamepad/context/drop/cart/dispatch | `4/4` | `28.7115576 s` | `17,221` | `d096c815bfdde4bc6183dcb698d30741bf8faf4f6e641df6462becb435d86f95` |
| Work-ticket keyboard/mouse + gamepad route regression | `2/2` | `6.3426859 s` | `10,097` | `f48c7857f14f065aa99fb6f5b1e5348573f01f94b83651385ce3ed60d072493f` |
| Assembly + Retail hero readability budgets | `2/2` | `0.968813 s` | `10,732` | `810f6f96f6bd93eea819dd77b5741dee604e4ce9030545b49233fdec32b19ce2` |
| Final full EditMode | `815/815` | `86.8024811 s` | `674,245` | `b99edaea62a20bdf44f01222a0c58d83ada0ff29f1f1f352d47cb44d66fc2969` |
| Final full PlayMode | `195/195` | `742.2700101 s` | `693,548` | `3d07226ff44ed1fc4042fdca896b409f6ce2a1f5786bb90aeb8cdbcd8b1f7e77` |

All accepted XML files report failed, skipped and inconclusive `0`.

| Accepted log | Bytes | SHA-256 |
|---|---:|---|
| Fulfillment domain | `32,046` | `4bbd73d65b0d00c74672997733c8930e84373c067038056dd640c9af4c855ff7` |
| r68 scene | `35,282` | `a7c1f830958da13834a1afa6cc7d94353912c6b88a25127c06a4528052a12685` |
| Packaging PlayMode | `53,611` | `b65897c85610a13d1bedca0fe4d1c27d13cbed1d7828e64c771cd11929071b6c` |
| Work-ticket route regression | `45,884` | `c97e2db8490df403503e6d2b7743aa7f424e542acc3f02f1a863c07292a3b590` |
| Hero readability regression | `45,150` | `dec826e93f5a574a8a07e384f6ca2361eedd3ae0d24199e24d64d4600c7d148e` |
| Final full EditMode | `36,024` | `59504960f7c8afe581d0ead695701434633ff20f31d90696b1f961a975684e4c` |
| Final full PlayMode | `810,874` | `8b8334091bcb15be30278fa5df0065a3f81c284ef1c202b5e58f9c940cea3e90` |

The first full PlayMode diagnostic exposed four regressions: two stale r67 version expectations, and keyboard/gamepad work-ticket routes physically blocked by the initial packaging workbench position. The version/budget contracts were updated for intentional r68 geometry, the workbench was relocated outside the corridor, and the exact two-route test passed before final full reruns. No failing diagnostic result is accepted as closure evidence.

## macOS native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `501,375` | `00baff68fbb4476b4e922085eb99dab7441071a5c4f109291ebb6f25b64af49d` |
| Universal app executable | `117,179` | `392b5596e46d2a01b96965ecc51979afcb3b542b53272e13994339c8f65da71d` |
| Apple M1/Metal r68 runtime log | `9,556` | `ecea1f14105ece7802e1f8038c0b4fa759f62c5ed03742c14b3753ca8034d1bc` |

Unity reports `330,891,503` build bytes. The app contains `306` files and occupies `323,028 KiB` by filesystem blocks. Its executable is universal Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict` passes and the app satisfies its designated requirement.

The graphical 1280×720 Apple M1/Metal player publishes r68 readiness with `quality-release=ready custom-pc-packaging=ready` and exactly one success marker:

```text
GARAGE_CUSTOM_PC_PACKAGING_RUNTIME_SMOKE prerequisite-setup=assisted quality-release=current review=keyboard seal=gamepad package=one physical=large-box source-projections=10 hidden-after-seal=true pickup=ok cart=hands-cart-hands dispatch=staged custody=append-only custody-receipts=4 replay=ok upstream=unchanged invariants=ok
```

Success count is `1`; explicit packaging failure/fatal-token count is `0`. The run exited `0`, Input System reached shutdown, and final player/Unity/shader/IL2CPP process residue was `0`.

## Repository, settings and raw evidence

Technical commit `79ea367af67549592a6ba58acd53afa74e7f25cb` contains exactly `47` source, meta, scene and test paths with `40,363` insertions and `33,893` deletions. The large line delta is deterministic scene reserialization; no unrelated art/material/prefab reserialization remains. The commit contains no ProjectSettings or Packages path.

The separately preserved user/editor-owned ProBuilder setting remains the only unrelated tracked difference, SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`; it is unstaged and unreverted. `ProjectSettings/ProjectSettings.asset` remains SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` after exact-head tests/build/smoke.

Local `Tools/verify-repository.sh` reports `tracked=1326`; `git diff --check`, meta pairing, focused temporary-marker search, codesign and residue checks pass.

Sixteen accepted exact-head XML/log artifacts were copied byte-for-byte from `/private/tmp/pse-issue139-exact-head-*` into `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue139-quality-bound-packaging-2026-08-31/raw`. Source/destination SHA-256 comparison passed `16/16`; the durable raw directory occupies `2,960 KiB`.

## Deferred physical gates

The physical Windows machine is unavailable. No Windows full test, x64 IL2CPP/only-D3D11 build, Intel Iris Xe runtime, process/task cleanup or Mac readback claim is made. Closure still requires a clean checkout of the final accepted source/docs tree, full EditMode/PlayMode, native r68 smoke, fatal-token audit and zero final residue. UTM is deliberately not treated as physical Windows evidence.

No USB device is connected. No immutable checkpoint, incoming directory, SHA/size/path/Git manifest, atomic rename or second readback claim is made. When the known USB returns, its identity and health must be rediscovered before any write.

Physical-human keyboard, mouse, gamepad and endurance acceptance is not claimed. Automated Input System keyboard/mouse and virtual-gamepad coverage is technical evidence only.

## Issue #139 acceptance matrix

| Acceptance area | Mac status | Remaining physical status |
|---|---|---|
| Unity-independent package authority and boundary | PASS | — |
| Exact current quality-release lineage | PASS | — |
| Exactly one sealed physical package | PASS | — |
| Append-only custody and legal transition graph | PASS | — |
| Same-instance replay/conflict/history tamper gates | PASS | — |
| Physical preflight/commit/rollback safety | PASS | — |
| Hands, safe floor, cart and dispatch staging | PASS | — |
| Source projections hidden without duplicate/loss | PASS | — |
| Keyboard and virtual-gamepad two-step packaging | PASS | Physical-human HID pending |
| Work-ticket route regression | PASS | Physical-human navigation pending |
| Exact r68 scene/renderer/light/camera budgets | PASS | Physical visual acceptance pending |
| Full exact-head Mac tests/build/native runtime | PASS | — |
| Clean Windows x64 IL2CPP/D3D11/Iris Xe | — | DEFERRED |
| Immutable USB checkpoint and second readback | — | DEFERRED |
| GitHub integration | Draft PR #140 open | Merge/Issue closure deferred |

Issue #139 remains open and its Roadmap card In Progress until the deferred physical gates and integration/closure steps complete. PR #140 is draft and intentionally does not auto-close the issue.

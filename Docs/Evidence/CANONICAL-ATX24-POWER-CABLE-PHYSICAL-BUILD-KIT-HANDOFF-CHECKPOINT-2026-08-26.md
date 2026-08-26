# Canonical ATX24 Power-Cable Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 26 August 2026
**Issue:** [#83](https://github.com/cixanla/PC-Shop-Empire-3D/issues/83)
**Technical head:** `a36d713120283bd106aeca76509756d6dbb1dd30`
**Technical tree:** `2619dd8e1db812c9e3249657a2031a6268492b5a`
**Current closure:** source/domain/scene/input/full-regression and exact-head macOS/Windows native gates passed; source/docs CI, canonical immutable package, physical USB, human-player and Issue/Project administrative gates pending

## Delivered playable result

GarageGraybox r42 adds the eighth physical handoff for the accepted custom-PC work order. The domain resolves the canonical ATX24 cable only from the exact `PowerCable` line whose cable family is `ModularAtx24SplitPsuToMotherboard`, then cross-checks its work-order/ticket/allocation line, product, serialized item and reservation identities.

The player takes that exact Unity object using `E / Gamepad South`, carries it, rotates the keyed 180° preview and places it into an ATX24-specific managed capacity-one BuildKit tray. The custody chain is exact WorldFloor → `ActorHands` → ATX24 BuildKit. The first seven component slots and receipts remain staged, reservation/allocation stays live and ticket progress derives from the authoritative receipt aggregate as `7/10 → 8/10`.

This does not route or connect the cable. Issue #61 ATX24 endpoints, waypoints, route/unroute receipt and revision remain untouched. EPS12V, PCIe/GPU, PSU Assembly and other component Assembly authorities cannot consume the same input frame while the ATX24 BuildKit receipt is active. Domain commit precedes world mutation; physical failure recovers the same object and stable ItemId at the authoritative kit pose.

The exact runtime success marker is:

```text
GARAGE_ATX24_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card+power-supply-staged atx24-pickup=exact cable-family=modular-atx24-split physical-identity=stable carry=ok input=keyboard+mouse prerequisite-positioning=teleport-assisted custody-guards=ok route-consumer=blocked rotation=180 placement=ok progress=8/10 reservation=alive custody=atx24-build-kit receipts=ok revisions=ok assembly=untouched atx24-route=untouched eps12v-route=untouched pcie-route=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

`prerequisite-positioning=teleport-assisted` is an explicit limitation. This marker proves the production handoff and invariants after bounded prerequisite staging; it is not a no-teleport or real-human route claim.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Focused domain | 39/39 | `domain-atx24.xml` | 34,080 | retained working evidence |
| Focused r42 scene | 9/9 | `scene-r42.xml` | 10,357 | retained working evidence |
| BuildKit PlayMode fixture | 44/44 | `playmode-buildkit-r42.xml` | 125,423 | retained working evidence |
| Full EditMode | 701/701 | `editmode.xml` | 584,202 | `327633f7e794f038b5493a8d26d73bdb0273442df87e762cbc401a1123dd37eb` |
| Full PlayMode | 110/110 | `playmode.xml` | 311,569 | `5d271d3e7884f484c9e1415ccb8f0f6060205aecd78a90e5bd84043cf99474dc` |

Every listed suite has failed, skipped and inconclusive `0`. The committed technical tree passes `git diff --check`; a bounded independent audit found no P0/P1 in nested smoke suppression, identity/family selection, Octuple ownership, replay, revision or route-authority isolation.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 501,138 | `47a688729be84bd0c4551fc210fd41cd708ca37659cb6e335a4de702eb6096e2` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 8,795 | `f1aa652d1725ac762444dd3d74af6c5255b517b6bf88fced3cfa0463d7819bf5` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `fb1b178f53847186459f0741aacb1d480d28af8036bae86a2218e190b127e5b7` |

The build report is `329,963,160` bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The 1280×720 player emits the exact r42 readiness and ATX24 success markers once, emits zero forbidden marker, reaches Input System `Shutdown`, exits gracefully and leaves no player process. A prior manually interrupted attempt is retained separately as non-canonical and is not promoted.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,317,338` bytes with SHA-256 `140d44d103935f3842627b0f4431742b969f6c82881ac721d05a551cbb345daa`. It exposes one branch head and produced a detached, clean Windows checkout at the exact technical commit/tree in:

`C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue83-a36d713-cold-v1`

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/D3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,109 | `8a016b1664659e8cacd2ecc0d5e1fbf6244e53a5abdef6f7a60842612d6adf51` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `93074bd37b87a83b4ac88339507c4107aca43655d5ed153b23daa7d6616be0ae` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,732 | `1f3dd5fc29c3a91c4aaa9c5fca552e9f8d5091d48cbdf6d564777db893285603` |
| Runtime summary | Accepted, graceful, residue-free | `runtime-summary.json` | 3,294 | `0a6f122807b816f604da51f0941dee8e728b3f5ff1c8707fadb37ae447e09f1a` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,697 | `96efbcfd03797042febe6cfa51c9e7b8f16e168a2cd933b6d4a63826c8545a24` |

The strict `issue83-hardened-v1` build reports `1,337,139,191` bytes and fatal-token count `0`. Exact binaries are:

- `PC Shop Empire 3D.exe`: `667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: `45,339,136` bytes, `222ac0ccea02436cf5129d76f7c6c7b897e01f8a0a729ca46000e98b2f2ed706`.
- `UnityPlayer.dll`: `84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

Intel Iris Xe reports Direct3D 11.0 feature level 11.1 at 1280×720. Host, r42 readiness and ATX24 success occur exactly once; forbidden count is `0`. Player exit is `0`, graceful shutdown is true, scheduled task `PSE-Issue83-a36d713-R1` is deleted, cleanup is not required and task/player/Unity residue is `0`. The checkout remains detached-clean at the exact commit/tree after runtime.

Procedure source and Windows readback are exact:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,613 | `60289f5ff6359ce669108f1aa616ee17bad2484f4404d93027a41e67577478ed` |
| `launch-procedure.ps1` | 7,949 | `71a4f45a4ee00b3f90dc74b93b864ce09a009c6c579bd40393a9dc0524506811` |
| `runtime-procedure.ps1` | 14,714 | `a3918ceb15f0cf6868b112b459faf0b4b7da6afdfe7055c30292cc2d71f3dd24` |
| `procedure-manifest.json` | 635 | `e118d459cb2f9a5ac355ef1f42de7aa270d96e69c603d0e3e806bd4fbff2a880` |

## Issue #83 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact `PowerCable` + `ModularAtx24SplitPsuToMotherboard` line and full identity tuple. | TECHNICAL PASS |
| 2 | No ordinal/display/product/component-only/regenerated authority. | TECHNICAL PASS |
| 3 | Stable ATX24 operation and capacity-one managed slot. | TECHNICAL PASS |
| 4 | Eight containers claimed atomically through Octuple access. | TECHNICAL PASS |
| 5 | Previous seven slots/receipts/replays/revisions/staged states preserved. | TECHNICAL PASS |
| 6 | Immediate/delayed replay returns one receipt without duplicate custody/revision. | TECHNICAL PASS |
| 7 | Foreign/value-equal/wrong-family and all identity forgeries fail closed. | TECHNICAL PASS |
| 8 | Full hands/full kit/source drift/stale revisions are no-mutation. | TECHNICAL PASS |
| 9 | Custody only WorldFloor → hands → ATX24 BuildKit; generic/route bypass blocked. | TECHNICAL PASS |
| 10 | Live exact reservation/allocation survives each custody step. | TECHNICAL PASS |
| 11 | Seven staged prerequisites prevent `7/10` skipping. | TECHNICAL PASS |
| 12 | Real Input System `E / Gamepad South` pickup and focus gates. | TECHNICAL PASS |
| 13 | Separate target, one support collider, exact anchor, support/obstruction and keyed preview. | TECHNICAL PASS |
| 14 | BuildKit is the single primary/rotate/drop consumer while active. | TECHNICAL PASS |
| 15 | Installed ATX24 route visuals/interactions stay inert and authority untouched. | TECHNICAL PASS |
| 16 | Co-edge, held, pause and release/repress are deterministic. | TECHNICAL PASS |
| 17 | Domain failure leaves the same cable in hands before world mutation. | TECHNICAL PASS |
| 18 | Projection failure recovers the same cable at the exact kit pose. | TECHNICAL PASS |
| 19 | Unity instance, ItemId, collider/layer/ownership/container remain exact. | TECHNICAL PASS |
| 20 | Receipt aggregate advances immutable ticket `7/10 → 8/10`. | TECHNICAL PASS |
| 21 | ATX24/EPS12V/PCIe routes, PSU/other Assembly, quote/reservation/items untouched. | TECHNICAL PASS |
| 22 | WASD/mouse/gamepad exact-build real-human scenario. | HUMAN SESSION PENDING |
| 23 | Focused and full EditMode/PlayMode regressions. | TECHNICAL PASS |
| 24 | Diff, Repository Guard, Mac and Windows native gates. | PARTIAL: diff + Mac + Windows pass; source/docs Guard pending |
| 25 | Docs, private push/CI and physical USB lifecycle. | LIFECYCLE PENDING |

The current strict count is `22/25` fully passed, `1/25` human pending and `2/25` lifecycle partial/pending. Automated human-shaped tests and smoke are never substituted for item 22.

## Pending bounded closure

1. Commit and push the exact source/docs/verifier closure; open the Issue #83 PR and require Repository Guard at the exact source/docs head.
2. Create the canonical 14-file evidence receipt, immutable local checkpoint and collision-free physical USB `.incoming-*` lifecycle; perform full hash/size/path readback before and after atomic rename.
3. Record physical metadata and pass the final Repository Guard without changing older milestones.
4. Run one exact-r42 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md`. Until it passes, keep Issue #83 open/In Progress and the PR draft/not-ready.
5. Only after `25/25`, close Issue #83 and set its Roadmap item `Done`. Parent Epic #10 remains open for EPS12V/PCIe BuildKit and later assembly/electrical/POST/OS/QA work.

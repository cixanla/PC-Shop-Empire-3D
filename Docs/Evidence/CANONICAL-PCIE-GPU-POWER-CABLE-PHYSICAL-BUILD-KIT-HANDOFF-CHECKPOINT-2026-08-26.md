# Canonical PCIe/GPU Power-Cable Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 26 August 2026
**Issue:** [#87](https://github.com/cixanla/PC-Shop-Empire-3D/issues/87)
**Technical head:** `25dc39ab02de93a416800acd17f53aacf83dca09`
**Technical tree:** `a736a764d0a52e950a4139002d6febc629df5987`
**Current closure:** source/domain/scene/input/full-regression, technical source CI and exact-head macOS/Windows native gates passed; source/docs CI, canonical immutable package, healthy physical USB, human-player and Issue/Project administrative gates pending

## Delivered playable result

GarageGraybox r44 adds the tenth physical handoff for the accepted custom-PC work order. The domain resolves the canonical PCIe/GPU cable only from the exact `PowerCable` line whose cable family is `ModularPcie8PinPsuToGraphicsCard`, then cross-checks its work-order/ticket/allocation line, product, serialized item and reservation identities.

The player takes that exact Unity object using `E / Gamepad South`, carries it, rotates the keyed 180° preview and places it into a PCIe/GPU-specific managed capacity-one BuildKit tray. The custody chain is exact WorldFloor → `ActorHands` → PCIe/GPU BuildKit. The first nine component slots and receipts remain staged, reservation/allocation stays live and ticket progress derives from the authoritative receipt aggregate as `9/10 → 10/10`.

This does not route or connect the cable. Issue #63 PCIe/GPU endpoints, waypoints, route/unroute receipt and revision remain untouched. ATX24, EPS12V, GPU/PSU Assembly and other component Assembly authorities cannot consume the same input frame while the PCIe/GPU BuildKit receipt is active. Domain commit precedes world mutation; physical failure recovers the same object and stable ItemId at the authoritative kit pose.

The exact runtime success marker is:

```text
GARAGE_PCIE_GPU_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card+power-supply+atx24+eps12v-staged pcie-gpu-pickup=exact cable-family=modular-pcie-8pin physical-identity=stable carry=ok input=keyboard+mouse prerequisite-positioning=teleport-assisted custody-guards=ok route-consumer=blocked rotation=180 placement=ok progress=10/10 reservation=alive custody=pcie-gpu-build-kit receipts=ok revisions=ok assembly=untouched pcie-gpu-route=untouched atx24-route=untouched eps12v-route=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

`prerequisite-positioning=teleport-assisted` is an explicit limitation. This marker proves the production handoff and invariants after bounded prerequisite staging; it is not a no-teleport or real-human route claim.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Full EditMode | 709/709 | `editmode.xml` | 590,398 | `5bd8013ab4d71b1232521da8c6bc9e9bfc3a2ea5a72ff6bbc0176daa9802fe29` |
| Full PlayMode | 116/116 | `playmode.xml` | 336,883 | `33a7888b6e5517af69b11711fe81c88dbf18c0ba3e86e2338ea325710af44213` |

Both suites have failed, skipped and inconclusive `0`. The committed technical tree passes `git diff --check`. Independent bounded audits remain advisory; only the exact test/native/receipt artifacts are promoted as closure evidence.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 594,839 | `8d389e8042ac0db4cbce263f9830b5469da9a8aac2709bb88460aeaece774041` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 8,796 | `ffaa77f80b5e82426ecf8a71759347aa37cedb955f4e399e60d7b517e213b5b6` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `b7356febd8f59eb55ef56a3eed7a1c6fc44e5424e0d811d5b09372aaa5f4e02a` |

The build report is `330,073,048` bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The player emits the exact r44 readiness marker including `pcie-gpu-power-cable-build-kit=ready` and the PCIe/GPU success marker once, emits zero forbidden marker, reaches Input System `Shutdown`, exits gracefully and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,505,067` bytes with SHA-256 `e0555605581e4f106cddd3536e1794d8a5d97f50aedb0da7e68ef0d2edb014ff`. It exposes one branch head and produced a detached, clean Windows checkout at the exact technical commit/tree in:

`C:\Users\mertk\Developer\PCShopEmpire3D\WindowsValidation\issue87-25dc39a-cold-v1`

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/D3D11 build | Success | `build-il2cpp-d3d11.log` | 487,798 | `ed39d28588e3fcdf3708d7e46f2dc1539d72ac18557c44c5112451ce6a3ee280` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `54496d0f86074cb517ed58c0e4caa8bca84fa909bba980badb46377d1530b2df` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,841 | `d98637fe641d40f2fb57ca2d11e6e79155ec9b7dee8d2a9d5ea3161137314d2b` |
| Runtime summary | Accepted, graceful, residue-free | `runtime-summary.json` | 3,295 | `8c9b60074146fdca049edf3168f029aafbd882336b5928eef91b1555d63c20cf` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,697 | `f5d3cb5d03b9ece88c3d034536ebdc2acef712037b41c1260fbb647a9ee4a042` |

The strict `issue87-hardened-v1` build reports `1,339,592,274` bytes and expanded fatal-token count `0`. ProjectSettings is byte-exact before/after. Exact binaries are:

- `PC Shop Empire 3D.exe`: `667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: `45,443,072` bytes, `66affc5667dd0888431303c6bdf3b746d5795787801c1735df65e53e407419fe`.
- `UnityPlayer.dll`: `84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

Intel Iris Xe reports Direct3D 11.0 feature level 11.1 at 1280×720. Host, r44 readiness and PCIe/GPU success occur exactly once; forbidden count is `0`. Player exit is `0`, graceful shutdown is true, scheduled task `PSE-Issue87-25dc39a-R1` is deleted, cleanup is not required and player/task residue is `0`. The checkout remains detached-clean at the exact commit/tree after runtime.

Procedure source and Windows readback are exact:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,623 | `14c5b84404c58cad6d8db2e273c26586b05cd8995d9eb1db0e4878ecd1620bf4` |
| `launch-procedure.ps1` | 7,956 | `84e8e70f50ce3541f553b651acc94dcd978020342bdbcab8d23a251f60438dc7` |
| `runtime-procedure.ps1` | 14,944 | `73be484143045ccf8a404019c4dcb1d43158b6394b3e72594b49c4aa98ffa734` |
| `procedure-manifest.json` | 1,028 | `72aa02a988b1ba3444d9f6974c61360aa5157b75f4b905a01f4437ee76fb2184` |

## Issue #87 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact `PowerCable` + `ModularPcie8PinPsuToGraphicsCard` line and full identity tuple. | TECHNICAL PASS |
| 2 | No ordinal/display/product/component-only/regenerated authority. | TECHNICAL PASS |
| 3 | Stable PCIe/GPU operation and capacity-one managed slot. | TECHNICAL PASS |
| 4 | Ten containers claimed atomically through Decuple access. | TECHNICAL PASS |
| 5 | Previous nine slots/receipts/replays/revisions/staged states preserved. | TECHNICAL PASS |
| 6 | Immediate/delayed replay returns one receipt without duplicate custody/revision. | TECHNICAL PASS |
| 7 | Foreign/value-equal/wrong-family and all identity forgeries fail closed. | TECHNICAL PASS |
| 8 | Full hands/full kit/source drift/stale revisions are no-mutation. | TECHNICAL PASS |
| 9 | Custody only WorldFloor → hands → PCIe/GPU BuildKit; generic/route bypass blocked. | TECHNICAL PASS |
| 10 | Live exact reservation/allocation survives each custody step. | TECHNICAL PASS |
| 11 | Nine staged prerequisites prevent `9/10` skipping. | TECHNICAL PASS |
| 12 | Real Input System `E / Gamepad South` pickup and focus gates. | TECHNICAL PASS |
| 13 | Separate target, one support collider, exact anchor, support/obstruction and keyed preview. | TECHNICAL PASS |
| 14 | BuildKit is the single primary/rotate/drop consumer while active. | TECHNICAL PASS |
| 15 | Installed PCIe/GPU route visuals/interactions stay inert and authority untouched. | TECHNICAL PASS |
| 16 | Co-edge, held, pause and release/repress are deterministic. | TECHNICAL PASS |
| 17 | Domain failure leaves the same cable in hands before world mutation. | TECHNICAL PASS |
| 18 | Projection failure recovers the same cable at the exact kit pose. | TECHNICAL PASS |
| 19 | Unity instance, ItemId, collider/layer/ownership/container remain exact. | TECHNICAL PASS |
| 20 | Receipt aggregate advances immutable ticket `9/10 → 10/10`. | TECHNICAL PASS |
| 21 | PCIe/GPU, ATX24 and EPS12V routes; GPU/PSU/other Assembly; quote/reservation/items untouched. | TECHNICAL PASS |
| 22 | WASD/mouse/gamepad exact-build real-human scenario. | HUMAN SESSION PENDING |
| 23 | Focused and full EditMode/PlayMode regressions. | TECHNICAL PASS |
| 24 | Diff, Repository Guard, Mac and Windows native gates. | TECHNICAL PASS |
| 25 | Docs, private push/CI and healthy physical USB lifecycle. | LIFECYCLE PENDING |

The current strict count is `23/25` fully passed, `1/25` human pending and `1/25` lifecycle pending. Automated human-shaped tests and smoke are never substituted for item 22.

## Pending bounded closure

1. Commit and push the exact source/docs/verifier closure; update draft PR #88 and require Repository Guard at the exact source/docs head.
2. Create the final source receipt, validate the canonical 14-file evidence and create a collision-free immutable local checkpoint.
3. Use physical USB only when live disk/volume identity and health are clean. Then perform collision-free `.incoming-*` copy, full path/size/hash readback, atomic rename and second full readback; never overwrite an older milestone.
4. Record physical metadata and pass the final Repository Guard without changing older milestones.
5. Run one exact-r44 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md`. Until it passes, keep Issue #87 open/In Progress and the PR draft/not-ready.
6. Only after `25/25`, close Issue #87 and set its Roadmap item `Done`. Parent Epic #10 remains open for component installation, cable routing, electrical/POST/OS/QA and later product work.

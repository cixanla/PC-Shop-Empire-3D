# Canonical EPS12V Power-Cable Physical Build-Kit Handoff — Checkpoint Evidence

**Date:** 26 August 2026
**Issue:** [#85](https://github.com/cixanla/PC-Shop-Empire-3D/issues/85)
**Technical head:** `b6a74e932f4744b17388df7c7eb4d88f26d195f4`
**Technical tree:** `bd763ea0c8c6d2f5d256e467c4fca8b762ca4d84`
**Current closure:** source/domain/scene/input/full-regression and exact-head macOS/Windows native gates passed; source/docs CI, canonical immutable package, healthy physical USB, human-player and Issue/Project administrative gates pending

## Delivered playable result

GarageGraybox r43 adds the ninth physical handoff for the accepted custom-PC work order. The domain resolves the canonical EPS12V cable only from the exact `PowerCable` line whose cable family is `ModularEps12v8PinPsuToMotherboard`, then cross-checks its work-order/ticket/allocation line, product, serialized item and reservation identities.

The player takes that exact Unity object using `E / Gamepad South`, carries it, rotates the keyed 180° preview and places it into an EPS12V-specific managed capacity-one BuildKit tray. The custody chain is exact WorldFloor → `ActorHands` → EPS12V BuildKit. The first eight component slots and receipts remain staged, reservation/allocation stays live and ticket progress derives from the authoritative receipt aggregate as `8/10 → 9/10`.

This does not route or connect the cable. Issue #62 EPS12V endpoints, waypoints, route/unroute receipt and revision remain untouched. ATX24, PCIe/GPU, PSU Assembly and other component Assembly authorities cannot consume the same input frame while the EPS12V BuildKit receipt is active. Domain commit precedes world mutation; physical failure recovers the same object and stable ItemId at the authoritative kit pose.

The exact runtime success marker is:

```text
GARAGE_EPS12V_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card+power-supply+atx24-staged eps12v-pickup=exact cable-family=modular-eps12v-8pin physical-identity=stable carry=ok input=keyboard+mouse prerequisite-positioning=teleport-assisted custody-guards=ok route-consumer=blocked rotation=180 placement=ok progress=9/10 reservation=alive custody=eps12v-build-kit receipts=ok revisions=ok assembly=untouched eps12v-route=untouched atx24-route=untouched pcie-route=untouched no-duplicate-loss=ok replay=ok invariants=ok
```

`prerequisite-positioning=teleport-assisted` is an explicit limitation. This marker proves the production handoff and invariants after bounded prerequisite staging; it is not a no-teleport or real-human route claim.

## Exact source and tests

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Full EditMode | 705/705 | `editmode.xml` | 587,288 | `7848a594be4d3949f65ce478eeca1aa6866124074c440d1b7aa7373b214f8421` |
| Full PlayMode | 115/115 | `playmode.xml` | 329,565 | `78988c94474369e1e15778f584fe5209fe9157738681bbe7f43ff2aa91580a8b` |

Both suites have failed, skipped and inconclusive `0`. The committed technical tree passes `git diff --check`. Independent bounded audits remain advisory; only the exact test/native/receipt artifacts are promoted as closure evidence.

## macOS exact-head native gate

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Development build | Success | `macos-build.log` | 591,595 | `aa5ee1dcb0016687fa0d6c6cac960c7212355dc6b66f53cae32ee16ba4b36ad8` |
| Apple M1/Metal runtime | Success | `macos-runtime.log` | 8,867 | `22be7dfaaf061c0ce86b4d8f08e5d04129a32633b4551efa486ed81d14da137b` |
| App executable | Universal and deep/strict-valid | `PC Shop Empire 3D` | 117,179 | `961c6f230c720b6350c929841d165b86b5f3b0c0ca987aa3bc9dbf61c8fa51b5` |

The build report is `330,018,708` bytes. `file` confirms `x86_64 + arm64`; `codesign --verify --deep --strict` passes. The 1280×720 player emits the exact r43 readiness marker including `eps12v-power-cable-build-kit=ready` and the EPS12V success marker once, emits zero forbidden marker, reaches Input System `Shutdown`, exits gracefully and leaves no player process.

## Windows detached-clean IL2CPP and D3D11 gate

The complete Git bundle is `7,426,223` bytes with SHA-256 `b2588f1c4f44dee1d36cf78e725319cb406ae24b2d76aee37976684b1d699559`. It exposes one branch head and produced a detached, clean Windows checkout at the exact technical commit/tree in:

`C:\Users\mertk\Developer\PCShopEmpire3D\Validation\issue85-b6a74e9-cold-v1`

| Gate | Result | Canonical artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP/D3D11 build | Success | `build-il2cpp-d3d11.log` | 1,583,219 | `df9562455b3c5b08a7452ddf6c8f16c7dd8fae8d8d08d4b00f3267a081f43b2f` |
| Native binary manifest | Three exact binaries | `binary-manifest.json` | 1,292 | `f490f188b127676d6ea20c511a9e698c537c4069fef481dde5072e97e750e177` |
| Intel Iris Xe interactive runtime | Success | `runtime-d3d11.log` | 5,778 | `23c3b8b9c15cc29fb4defd804c6cb83f46a7d1c6efd4a9378efb1c7a3fa3ad88` |
| Runtime summary | Accepted, graceful, residue-free | `runtime-summary.json` | 3,294 | `abd67853d79cea9870719820db530b58bbed58007e863beb9528abf970d3c0cc` |
| Interactive task receipt | Created, completed and deleted | `task-receipt.json` | 1,697 | `81082bd2962d97d1751ab3ae778b45b75453b19bc2da1a050d3d3cb737a3bec2` |

The strict `issue85-hardened-v1` build reports `1,338,310,618` bytes and expanded fatal-token count `0`. ProjectSettings is byte-exact before/after. Exact binaries are:

- `PC Shop Empire 3D.exe`: `667,136` bytes, `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: `45,392,384` bytes, `d7f2267af1dac8f64e507bd9d6935431dabe4faa6e3c810d41da26529c1765ec`.
- `UnityPlayer.dll`: `84,237,744` bytes, `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

Intel Iris Xe reports Direct3D 11.0 feature level 11.1 at 1280×720. Host, r43 readiness and EPS12V success occur exactly once; forbidden count is `0`. Player exit is `0`, graceful shutdown is true, scheduled task `PSE-Issue85-b6a74e9-R1` is deleted, cleanup is not required and player/task residue is `0`. The checkout remains detached-clean at the exact commit/tree after runtime.

Procedure source and Windows readback are exact:

| Procedure | Bytes | SHA-256 |
|---|---:|---|
| `build-procedure.ps1` | 9,621 | `abefe678898c3165e75f3bfc8bcd62dc86849ce49f261e766fbbb39128624886` |
| `launch-procedure.ps1` | 7,949 | `4d2b81dfb76a688b3e0e1b6d646b7f3e1696f8fdd4cc2227b8aa99117bbcb3e8` |
| `runtime-procedure.ps1` | 14,823 | `e8eec0210ad70df77a4883c4a00853dacab371851f39f5a1213653b0c0fdbe8a` |
| `procedure-manifest.json` | 998 | `6006d78b3c3b2f135036b2946dfb8ed29a04097f4041ff30a8486b1f0c8ff61a` |

## Issue #85 acceptance matrix — current state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact `PowerCable` + `ModularEps12v8PinPsuToMotherboard` line and full identity tuple. | TECHNICAL PASS |
| 2 | No ordinal/display/product/component-only/regenerated authority. | TECHNICAL PASS |
| 3 | Stable EPS12V operation and capacity-one managed slot. | TECHNICAL PASS |
| 4 | Nine containers claimed atomically through Nonuple access. | TECHNICAL PASS |
| 5 | Previous eight slots/receipts/replays/revisions/staged states preserved. | TECHNICAL PASS |
| 6 | Immediate/delayed replay returns one receipt without duplicate custody/revision. | TECHNICAL PASS |
| 7 | Foreign/value-equal/wrong-family and all identity forgeries fail closed. | TECHNICAL PASS |
| 8 | Full hands/full kit/source drift/stale revisions are no-mutation. | TECHNICAL PASS |
| 9 | Custody only WorldFloor → hands → EPS12V BuildKit; generic/route bypass blocked. | TECHNICAL PASS |
| 10 | Live exact reservation/allocation survives each custody step. | TECHNICAL PASS |
| 11 | Eight staged prerequisites prevent `8/10` skipping. | TECHNICAL PASS |
| 12 | Real Input System `E / Gamepad South` pickup and focus gates. | TECHNICAL PASS |
| 13 | Separate target, one support collider, exact anchor, support/obstruction and keyed preview. | TECHNICAL PASS |
| 14 | BuildKit is the single primary/rotate/drop consumer while active. | TECHNICAL PASS |
| 15 | Installed EPS12V route visuals/interactions stay inert and authority untouched. | TECHNICAL PASS |
| 16 | Co-edge, held, pause and release/repress are deterministic. | TECHNICAL PASS |
| 17 | Domain failure leaves the same cable in hands before world mutation. | TECHNICAL PASS |
| 18 | Projection failure recovers the same cable at the exact kit pose. | TECHNICAL PASS |
| 19 | Unity instance, ItemId, collider/layer/ownership/container remain exact. | TECHNICAL PASS |
| 20 | Receipt aggregate advances immutable ticket `8/10 → 9/10`. | TECHNICAL PASS |
| 21 | EPS12V/ATX24/PCIe routes, PSU/other Assembly, quote/reservation/items untouched. | TECHNICAL PASS |
| 22 | WASD/mouse/gamepad exact-build real-human scenario. | HUMAN SESSION PENDING |
| 23 | Focused and full EditMode/PlayMode regressions. | TECHNICAL PASS |
| 24 | Diff, Repository Guard, Mac and Windows native gates. | PARTIAL: diff + Mac + Windows pass; source/docs Guard pending |
| 25 | Docs, private push/CI and healthy physical USB lifecycle. | LIFECYCLE PENDING |

The current strict count is `22/25` fully passed, `1/25` human pending and `2/25` lifecycle partial/pending. Automated human-shaped tests and smoke are never substituted for item 22.

## Pending bounded closure

1. Commit and push the exact source/docs/verifier closure; open the Issue #85 draft PR and require Repository Guard at the exact source/docs head.
2. Replace the initial build receipt with the final source receipt, validate the canonical 14-file evidence and create a collision-free immutable local checkpoint.
3. Use physical USB only after the Windows volume reports healthy. Then perform collision-free `.incoming-*` copy, full path/size/hash readback, atomic rename and second full readback; never overwrite an older milestone.
4. Record physical metadata and pass the final Repository Guard without changing older milestones.
5. Run one exact-r43 real-human acceptance session from `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md`. Until it passes, keep Issue #85 open/In Progress and the PR draft/not-ready.
6. Only after `25/25`, close Issue #85 and set its Roadmap item `Done`. Parent Epic #10 remains open for PCIe/GPU BuildKit and later assembly/electrical/POST/OS/QA work.

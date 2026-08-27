# Assembly Workbench Hero Readability — Checkpoint Evidence

**Date:** 27 August 2026<br>
**Issue:** [#111](https://github.com/cixanla/PC-Shop-Empire-3D/issues/111)<br>
**PR:** [#112](https://github.com/cixanla/PC-Shop-Empire-3D/pull/112)<br>
**Technical head:** `1e2106a822b36f888cb9ad53ee22054ae991cda2`<br>
**Technical tree:** `540992d186ff6e670569ee3cee51807798ffa427`<br>
**Branch:** `codex/issue111-assembly-workbench-hero-readability`<br>
**Current state:** bounded technical acceptance `6/6`; exact scene/material/light contracts, full Mac and Windows regression suites, universal macOS native, clean Windows x64 IL2CPP/D3D11 runtime, screenshot/readability, cross-machine evidence readback, process cleanup and technical Repository Guard passed. PR #112 is the source/docs integration record.

## Delivered visible result

GarageGraybox r55 turns the Assembly Workbench into the first bounded production proof of [ADR-0013](../ADR-0013-READABLE-SEMI-REALISTIC-VISUAL-DIRECTION.md). The open chassis, motherboard, GPU, PSU and three cable-route states now read as a layered hero work area instead of a flat gray cluster. The authored camera pose records directly comparable `loose`, `preview` and `routed` 1280x720 frames on both Apple M1/Metal and Intel Iris Xe/Direct3D 11.

The pass rebalances `WoodLaminate`, `WorkshopRubber` and `MotherboardPcb`, preserves the existing dark-metal, brushed-steel, concrete and bounded safety-accent family, and adds two narrowly shared URP Unlit materials:

- `CableConnectorPolymer` is assigned to the five exact cable/PSU-intake renderers and fourteen GPU fan blades that previously produced clipped or misleading highlights.
- `WorkshopMatteHardware` is assigned to the two exact GPU rear-bracket renderers.
- Both materials are dark, non-emissive presentation materials; they create no collider, raycast, authority or serialized identity surface.

The Workbench task light remains one existing soft spot light and is bounded to intensity `0.4`, range `2.8`, outer spot angle `62` and inner spot angle `38.44`. No light or camera is added. Work-ticket, monitor and status-board composition is repositioned for readability without moving gameplay colliders, anchors, route waypoints or custody authority.

The exact runtime success marker is:

```text
GARAGE_ASSEMBLY_WORKBENCH_HERO_READABILITY_RUNTIME_SMOKE states=loose+preview+routed hero=ready materials=wood+rubber+dark-metal+brushed-steel+concrete+pcb+safety-accent+connector-polymer+psu-intake+gpu-hardware connector-glare=bounded light=focused total-renderers=493 lights=4 cameras=1 screenshots=3 glare-pixels<=64 ui=lookdev-suppressed human=false active-renderers=484 max-central-glare-pixels=0 capture-directory=<platform-evidence>/lookdev-r55
```

The marker and screenshots are automated visual evidence. They are not a real-human, physical keyboard or physical-gamepad certification claim.

## Deterministic render and authority budget

| Contract | Base `e4114b0` | r55 technical head | Delta |
|---|---:|---:|---:|
| Authored scene `MeshRenderer` components | `486` | `490` | `+4` |
| Authored lights | `4` | `4` | `0` |
| Authored cameras | `1` | `1` | `0` |
| Runtime scene mesh renderers, including inactive | — | `493` | bounded exact contract |
| Runtime initially active mesh renderers | — | `473` | bounded exact contract |
| Nested smoke peak active renderers | — | `484` | bounded exact contract |

The four added hero renderers are decoration-only, live under `AssemblyWorkbenchHeroReadability`, use `Ignore Raycast`, contain no collider or light, cast/receive no shadows and force no motion vectors. Their deterministic pre-batching submission pressure is therefore bounded to at most four additional authored mesh submissions. URP/SRP may batch submissions, so this checkpoint does not invent an unsupported exact GPU draw-call number. Existing renderer material reassignment adds no renderer. The Windows native player peaked at `465,403,904` working-set bytes and `32.047` CPU seconds during the complete three-state smoke, then exited cleanly.

Gameplay authority, collider, joint, anchor, waypoint, route topology, stable serialized identity and input behavior remain unchanged. `ProjectSettings/ProjectSettings.asset` is byte-exact at SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` before and after every accepted Mac/Windows gate.

## Exact source and tests

| Platform | Gate | Result | Duration | XML bytes | XML SHA-256 |
|---|---|---:|---:|---:|---|
| macOS | Work-ticket/input P1 PlayMode | `7/7` | `26.2130327 s` | `25,725` | `dfe4e50959c2ed6efc1dd46b4df74be61b14d3a0adbf8246255206b8369ac477` |
| macOS | Full EditMode | `753/753` | `155.0968753 s` | `625,066` | `c9aa8c1499ed047923908360a569c13678edbbf00983e6b47ca461637f8c2883` |
| macOS | Full PlayMode | `157/157` | `1302.6964201 s` | `516,106` | `020eb1542fb2d1c0d0249a3def658a68d80b681edde1e1a67ada85ed88d23dc2` |
| Windows | Full EditMode | `753/753` | `28.0864986 s` | `629,852` | `8921b920f87ef098cce4b9dbb4991eca75b7b4e34bd0041854605c5669b7b9db` |
| Windows | Full PlayMode | `157/157` | `518.9768708 s` | `517,557` | `32c261c698c0b0905bcc34d6748ec190c2fb49253e30278f01fcda7b3487ee96` |

Every accepted XML has failed, skipped and inconclusive `0`. The full suites include the exact hero material/light/render budget test, committed-scene contract, PNG decode and glare-budget path, existing ten-part Assembly chain, simultaneous WASD plus mouse-look, keyboard/mouse and Input System virtual-gamepad regressions. The stale work-ticket route test was corrected to derive its approach from the current authored station collider instead of an obsolete hard-coded scene coordinate; the corrected target passed `7/7` before the final full `157/157` run.

`git diff --check` and local repository checks pass. Technical Repository Guard [33089682114](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/33089682114) passed at exact technical head `1e2106a822b36f888cb9ad53ee22054ae991cda2`.

## macOS exact-head native gate

| Artifact | Bytes | SHA-256 |
|---|---:|---|
| Universal build log | `598,818` | `80fb71ab4892183d63c82237e0515dec3de0a4d979e02f59c34ba991bd75d613` |
| Apple M1/Metal r55 runtime log | `11,136` | `66f60e76969d70e910a0452a1e98c5b0b7489e0bf8fc123b2cb0dd2208eb5b04` |
| Universal app executable | `117,179` | `64de99f99cbee9d4ce04806bdb9b5203166d947d71c54fa1a0e41d074c3e3d65` |

The macOS build marker reports `330,428,946` bytes. The application contains `302` files; `file` confirms Mach-O `x86_64 + arm64`, and `codesign --verify --deep --strict` passes. The native runtime emits one exact readiness marker, one exact r55 success marker, `max-central-glare-pixels=0`, graceful Input System shutdown and zero player residue.

| macOS screenshot | Bytes | SHA-256 | Central saturated-white pixels |
|---|---:|---|---:|
| `assembly-workbench-hero-loose-r55.png` | `688,200` | `ea5f1b37ba34a17d74d6e4bf56fdb82495ad07a97abfb6a60303c952fb6fb797` | `0` |
| `assembly-workbench-hero-preview-r55.png` | `712,963` | `a63d39a4a81d27fc73101ff68cb331092f57cbe5473e87dc512453d0d7b90de7` | `0` |
| `assembly-workbench-hero-routed-r55.png` | `724,538` | `b020beb6653dc59a304914a0842e5a74cc9b931dc00fc5ae2e0af390eeebaa6e` | `0` |

Canonical raw Mac evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue111-1e2106a-r1/mac`.

## Windows clean IL2CPP, D3D11 and screenshot gate

The Windows canonical checkout remained a clean validation lane. Because no Windows GitHub credential was available, a complete Git bundle (`7,836,314` bytes, SHA-256 `acf2d72db0d8934aef7a307eb85c44f1e1184fb6266d70cd48ef4596051209af`) produced a detached clean clone at exact technical head/tree. Windows did not author or push source.

Unity 6000.3.21f1 built an x64 IL2CPP Development player with Direct3D11 only. The build marker reports `1,350,529,280` bytes; output contains `674` files and `1,350,698,274` bytes. The `570,502`-byte build log has SHA-256 `449b46e064b81354352af37dcb4202f5df0cd165da782ab590ae39ac79994578`, exactly one success marker and zero expanded compiler/AOT/native-link fatal tokens.

| Native binary | Bytes | SHA-256 |
|---|---:|---|
| `PC Shop Empire 3D.exe` | `667,136` | `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a` |
| `GameAssembly.dll` | `45,889,024` | `a8d85e1747046d55a187a6962e36be4901246af5052f0a972e91e47e387f4d6d` |
| `UnityPlayer.dll` | `84,237,744` | `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59` |

The accepted interactive runtime used `-force-d3d11`, exact r55 smoke, 1280x720 windowed capture and the logged-on limited user. Intel Iris Xe reports Direct3D 11.0 feature level 11.1. All `26/26` final runtime checks pass: host/readiness/success and composition are each exact once; forbidden count is zero; binary, source, ProjectSettings, receipt and screenshot hashes match; player exit is `0`; graceful shutdown occurs; task and scoped-process residue are zero.

`runtime-issue111-r55-interactive.log` is `6,938` bytes / SHA-256 `43a27f6098200c6d044cea9ae5c94d913c793be262905743fff7b99248ada918`. Its receipt is `1,225` bytes / `3f05290dab343dd0993a9f7565ac66724e01389e40c2ef3aee8688bf67bb18bc`.

| Windows screenshot | Bytes | SHA-256 | Central saturated-white pixels |
|---|---:|---|---:|
| `assembly-workbench-hero-loose-r55.png` | `679,268` | `da680930089212bd3b5ce0f7337656d510b255d151c9096fbe37cd3ffa9b6902` | `0` |
| `assembly-workbench-hero-preview-r55.png` | `703,503` | `a5b43c18f77bb93b0add7bea49544671dc913680fee81e0777794acafab4316e` | `0` |
| `assembly-workbench-hero-routed-r55.png` | `714,484` | `6ffd8118cdc81092728714b8fa6fb7c7486c42ed696e895e22a7ce8a7f3d2055` | `0` |

The three files are nonempty and byte-distinct. Independent Mac/Pillow decode reproduced exact dimensions, hashes and central saturated-white count `0` for every Windows frame. Whole-frame pixels above all-RGB-channel threshold `250` are `245` in each frame and sit outside the bounded hero work region.

The build wrapper's first three-second post-Unity sample observed one transient scoped child and failed closed. It had exited naturally at the independent post-build exact readback; no build/player process was forced. After the player had already exited, screenshots were complete and the scheduled task had been removed, the first evidence launcher began serializing PowerShell extended string metadata instead of the intended two short receipt lines. Exact validation-only PowerShell PID `18188` was terminated; the wrapper was corrected to flatten strings and an independent no-rerun finalizer passed all `26/26` checks. No game result, binary, runtime log or screenshot was regenerated or overwritten.

Windows evidence was copied to the Mac and independently parsed before cleanup. Canonical raw Windows evidence lives at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue111-1e2106a-r1/windows`. The exact disposable validation root, two exact executable-path inbound Block firewall rules and five exact temporary Issue #111 files were then removed. Final Windows readback is process/task/firewall/temp residue `0/0/0/0`, canonical checkout exact and clean, approximately `10.3 GB` free RAM and `199.61 GB` free on C:.

USB `D:` was read-only identified as label `cixanla`, exFAT, Intenso Alu Line, serial `900B00076010`, `Healthy/OK`, `125,829,120,000` physical bytes. Issue #111 did not write to USB.

## Issue #111 acceptance matrix — technical state

| # | Acceptance contract | Current gate |
|---:|---|---|
| 1 | Exact scene/material/light contracts and existing input/Assembly regressions. | PASS |
| 2 | Comparable loose/preview/routed native screenshots visibly separate workbench, chassis, GPU, PSU and cable route. | PASS — automated, `human=false` |
| 3 | Renderer/light/material/submission pressure measured and bounded; glare budget enforced. | PASS |
| 4 | Targeted/full Mac tests, universal native smoke, clean diff and technical CI. | PASS — Guard 33089682114 |
| 5 | Clean Windows x64 IL2CPP/D3D11 runtime, visible screenshots/readability and zero final residue. | PASS |
| 6 | Bible, ADR/Evidence, CHANGELOG and Roadmap/PR integration chain. | TECHNICAL PASS — PR #112 is the integration record |

The bounded technical acceptance count is `6/6`. Administrative Issue/Roadmap closure remains tied to PR #112 source/docs integration and final required Guard checks. Parent visual Epic #18 and the full Steam 1.0 Goal remain open. This first hero look-development proof does not claim that the shop, open world, character art, animation, effects or final graphics are complete; it establishes the measured visual-production baseline for subsequent bounded slices.

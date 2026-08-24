# Accepted Custom-PC Request, Immutable Quote and Exact Serialized Reservation — Checkpoint Evidence

**Date:** 24 August 2026<br>
**Issue:** [#64](https://github.com/cixanla/PC-Shop-Empire-3D/issues/64)<br>
**Draft PR:** [#65](https://github.com/cixanla/PC-Shop-Empire-3D/pull/65)<br>
**Feature:** `c7d38845ffccb5ae6e5365e580c238d70f8dac95`<br>
**Feature tree:** `615c9c4398f6a0be16c3a693dd812aa3f5541291`<br>
**Closure status:** macOS and Windows technical gates verified; docs/CI and physical USB closure pending

## Delivered playable result

GarageGraybox r33 turns the owned customer consultation into one accepted graphics-first custom-PC request. A second deliberate input generates an immutable quote with exactly ten compatible lines: motherboard, CPU, DDR5 DIMM, M.2 SSD, cooler, GPU, PSU, ATX24, EPS12V and PCIe/GPU 6+2 cable. The visible customer status exposes accepted request, ready quote, ten BOM lines and ten exact reservations rather than hiding the operation behind a dashboard.

Each line binds one exact ProductId, ItemInstanceId and ReservationId. Inventory preflights the complete set and commits it as one managed claim with one revision. Partial success is impossible. Exact replay returns the existing result without another revision. Claim, operation, access, payload or revision mismatch; missing/incompatible BOM; budget overflow; duplicate identity; pre-existing reservation; direct release/consume and eleventh-item attempts fail closed without mutating Retail, Inventory, customer, checkout, economy or physical Assembly state.

Keyboard/mouse and real Input System gamepad tests cover range, focus, line of sight, pause, release/repress, single-input-consumer and competing carry-item paths. An accepted request survives its original consultation deadline so quote creation remains visible. The pause-toggle frame returns before movement/look processing, preventing resume lurch.

## macOS automated and native evidence

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 647/647 | `issue64-c7d3884-full-editmode.xml` | 540,984 | `0f95ae59a54eb7cb0741524f80852286c275801b4d530515f3094449c348a115` |
| PlayMode | 59/59 | `issue64-c7d3884-full-playmode.xml` | 151,877 | `f81266a3edea893b9f6f6933b84566c3f836f9869eb1aa093fcf623773cec371` |
| Universal macOS build | Success | `issue64-c7d3884-macos-build.log` | 490,034 | `2c0b989426a7690bdb2f5abd60bf321ed3b3da6ab4a459942b6ebb79f327acc8` |
| Native Metal runtime | Success | `issue64-c7d3884-native-smoke.log` | 6,167 | `3fe408de4be349de7548e5dd72b20648ca0edda08faf9b1c753a2555b7e280a5` |
| App executable | Universal | `PC Shop Empire 3D` | 117,179 | `9cfdbf7d17583135550bd6a507164f644b8242e9bfbcfaf26641191a69c249bf` |
| Committed scene | r33 runtime | `GarageGraybox.unity` | 2,854,602 | `7fc63ba4686db17f5ca7800bf2421a526df591659dc84c201439f153416ff338` |

Both XML suites report zero failed, skipped and inconclusive tests. The Unity build report is `329,396,456` bytes. The executable is a Universal Mach-O with `x86_64` and `arm64` slices. `/Users/cixanla/Desktop/PC Shop Empire 3D.app` resolves to this current build.

The active Apple M1/Metal player ran windowed at 1280×720 and emitted readiness `garage-custom-pc-quote-reservation-r33-v1` plus the exact success marker once. No custom-PC failure marker, assertion, missing-reference or unhandled exception appeared.

## Windows exact-source IL2CPP and Direct3D evidence

The Windows validation clone was detached, clean and exact at `c7d38845ffccb5ae6e5365e580c238d70f8dac95`. Unity 6000.3.21f1, Windows IL2CPP support and the Microsoft C++ toolchain produced the player from that SHA. The final smoke ran through the logged-on interactive console session rather than the non-interactive SSH service session.

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 647/647 | `editmode.xml` | 545,154 | `7213bd8788f46d9e6b9bb0f262f4f217341f592d1b3773853ff09026c74c8085` |
| PlayMode | 59/59 | `playmode.xml` | 152,452 | `6807517fd2744fe42418872e2ad7ff30439f3bc6ed24cd3f7315513a923e9b80` |
| Windows x64 IL2CPP build | Success | `build-il2cpp.log` | 477,111 | `e5fbe1884762495ac1209d705fc0dab8187b67cd21ecee7229506584f8e8f963` |
| Binary manifest | 3/3 | `binary-manifest.json` | 928 | `741f10df024fee6b355156c5a67fb55f0548667324d9dfcf21b657bd4d1e0011` |
| Interactive D3D11 runtime | Success | `runtime-d3d11.log` | 3,958 | `aa4f93e09e95ac9b3e2a86f9a2013ce03d60d89c3a2a1945f00e63eb137cef17` |
| Runtime summary | marker 1 / D3D11 true | `runtime-summary.json` | 226 | `f27dedcb8aa0cf7db22d90a6a5542bd76633b512444d60978b5458429c2399f9` |

The build report marker is:

```text
STAGE_A_BUILD_OK target=StandaloneWindows64 bytes=1326137709 path=C:\Users\mertk\Developer\PCShopEmpire3D\Builds\Local\Windows-IL2CPP-x64\PC Shop Empire 3D.exe
```

The final binary manifest records:

- `PC Shop Empire 3D.exe`: 667,136 bytes, SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: 44,777,472 bytes, SHA-256 `2978b79b47d4c6aefef58d81f7235940b9df4d4794fb0935dfa3a5233b960021`.
- `UnityPlayer.dll`: 84,237,744 bytes, SHA-256 `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

The player reported Direct3D 11.0 feature level 11.1, Intel Iris Xe Graphics, driver `32.0.101.7085`, readiness `garage-custom-pc-quote-reservation-r33-v1` and the exact custom-PC success marker once. The first SSH-service launch produced D3D11 device lines but stopped at window-resolution initialization before scene readiness; its preserved log is diagnostic evidence only and is not counted as the successful runtime gate. The interactive task completed with result `0` and was deleted after readback. `C:\Users\mertk\Desktop\PC Shop Empire 3D.lnk` was then created and read back against the verified player.

## Native marker

```text
GARAGE_CUSTOM_PC_QUOTE_RUNTIME_SMOKE custom-pc-flow=ok browse-route=ok focus-gate=ok consultation=ok request=accepted bom-lines=10 compatibility=ok budget=ok reservation-set=atomic conflict=fail-closed replay=ok authority-isolated=ok presentation=ok invariants=ok
```

## Visible human-play observation

A short visible run of the exact Mac application captured five frames: initial, right movement, back movement, paused left-input attempt and resumed left movement. The scene position changed on the movement inputs, did not change during pause, and changed only after resume for the fresh left inputs.

| Frame | Bytes | SHA-256 |
|---|---:|---|
| `00-initial.jpeg` | 46,992 | `56ad6e26a1a37570ec7f35eb337f1e6ca607823c21b5432af08a212dd2ef8171` |
| `01-right-movement.jpeg` | 48,145 | `89b834b15c06326f911db235cb521fd4ba37a3281ee02773c35e8e6e8027b59f` |
| `02-back-movement.jpeg` | 48,429 | `7060ca6168432b46524eed126c5e098269c69b6f5651ce2c482bc63c1f8c14e9` |
| `03-paused-no-movement.jpeg` | 48,030 | `64f00bc17e9139e525f947dabb3143798c0951d57909e21d7161d458d412517a` |
| `04-resumed-left-movement.jpeg` | 48,378 | `85cb27ae966f23916b264ce981a4c385d51f60452b5b71fdf2da199d37e33d63` |

These frames supplement rather than replace deterministic motor/Input System tests. The UI-control layer lost access during a mouse-drag attempt, so this run does not claim a separately observed mouse-look result.

## Repository and external checkpoint status

- Feature Repository Guard: [32698054990](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32698054990), success.
- Draft PR [#65](https://github.com/cixanla/PC-Shop-Empire-3D/pull/65) points exactly to `c7d3884` at the time of this technical evidence record.
- The expected physical USB and previous milestone chain must be revalidated immediately before writing. No old milestone is overwritten.
- The final package will be copied to a collision-free `.incoming-*` directory, stripped only of AppleDouble files created inside that new incoming target, verified by full hash+size+path and exact Git-source/evidence readback, atomically renamed, and fully read back a second time.
- Issue #64 remains open and Roadmap `In Progress`; no physical USB or final post-checkpoint CI success is claimed in this technical document revision.

## Bounded exclusions

Reservation consumption/release into a build order, physical assembly completion, electrical power-on, POST/BIOS, fictional OS installation, benchmark/QA, packaging, delivery, payment/final settlement, Save/Guardian, final art/audio/VFX/UI, Steam packaging/signing and release readiness are not part of Issue #64.

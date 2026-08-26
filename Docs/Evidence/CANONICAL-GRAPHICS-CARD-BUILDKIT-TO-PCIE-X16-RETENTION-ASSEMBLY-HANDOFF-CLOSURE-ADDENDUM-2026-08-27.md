# Canonical Graphics-Card BuildKit-to-PCIe x16 Retention Assembly Handoff — Exact-Head Closure Addendum

**Date:** 27 August 2026<br>
**Issue:** [#99](https://github.com/cixanla/PC-Shop-Empire-3D/issues/99)<br>
**PR:** [#100](https://github.com/cixanla/PC-Shop-Empire-3D/pull/100)<br>
**Exact source commit:** `034f862cfdc85b93e44cc0c9dded26aafdffbee6`<br>
**Exact source tree:** `191e9e1bfd85ef20c000fc171523c1861f3ecb21`<br>
**Branch:** `feature/issue99-graphics-card-buildkit-to-pcie-retention`<br>
**Scene blob:** `d2aaab350b2a72911929f11eed5bf694e1b9afaa` for `Assets/Scenes/Prototypes/GarageGraybox.unity`

## Authority and scope

This addendum is the authoritative Issue #99 closure record for the focus/input hardening added after the 26 August checkpoint. It supersedes only the old checkpoint's `real-human pending`, `30/31` and administrative-pending statements. The earlier source, local checkpoint and physical-USB history remains preserved evidence for its own `d5532bb`/`0f25960` lineage and is not relabelled as an exact `034f862` USB checkpoint.

Issue #99 closes under the v2 player-acceptance policy in `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md`: exact-source automated domain/scene/input coverage, clean Mac and Windows native runtime, and agent-operated foreground Windows OS-level keyboard/mouse acceptance are sufficient for this bounded issue. The accepted native-input claim is exactly:

```text
PASS — agent-operated Windows OS-level keyboard/mouse acceptance at exact 034f862;
human=false; physical-keyboard=not-tested; physical-gamepad=not-tested;
15-minute-human-endurance=not-performed.
```

Input System gamepad automation passed on Mac and Windows. It is not described as a physical gamepad test. Real-human, physical-HID and endurance certification remains mandatory before the final Steam 1.0 release and is not silently discarded.

## Exact macOS test gates

All accepted XMLs below were started from clean exact `034f862`; failed, skipped and inconclusive counts are zero.

| Gate | Result | Duration | Bytes | SHA-256 | Artifact |
|---|---:|---:|---:|---|---|
| Critical input/Issue #99 PlayMode | `8/8` | `66.6511898 s` | `27,952` | `8a0b53626d5285b1d3aaecf76528405daa257c0ac407d2dffcee79429888be7a` | `issue99-034f862-exact-r16.eAq1am/critical-playmode.xml` |
| Full EditMode | `733/733` | `23.2404899 s` | `609,374` | `ab6845261b0095ed84ced0a9730295088dadcc37d1ba37e1267a9e0ac16c7a4b` | `issue99-034f862-exact-r16.eAq1am/editmode.xml` |
| Full PlayMode, accepted isolated run | `140/140` | `753.7843685 s` | `434,522` | `80f03c90e61523cc010c61b75fdf688ec30330bcafce4b2eca85b0751c51d216` | `issue99-034f862-exact-r18.GNCwVQ/playmode.xml` |

Corresponding log SHA-256 values are `3bc9e573…e13f3`, `0a1beda7…dee87` and `51a5f537…f2504`. The eight-test set covers keyboard/mouse and Input System gamepad completion, focus recovery, fail-closed assembly input gates, same-instance recovery exactly once and current-obstruction rejection.

One earlier full run is deliberately retained as negative evidence rather than hidden. `issue99-034f862-exact-r16.eAq1am/playmode.xml` (`5fc5bcb2…bdc65f`) completed `137/140`; a transient scene-import/parser event caused three otherwise unrelated handoff tests to fail. Those exact three tests immediately passed `3/3` in `issue99-034f862-flake-isolation-r17.GyChyh/playmode.xml` (`09f8c301…9f1a2`). The subsequent isolated full run passed `140/140`, emitted none of the parser/transform/missing-reference markers, left no `InitTestScene` or backup residue, and preserved the scene blob byte-for-byte at `d2aaab35…f694e`. The failed run remains a test-infrastructure observation; it is not counted as a product PASS.

## Exact macOS native gate

Unity 6000.3.21f1 produced the Universal macOS Development player from exact `034f862`:

- build report: `330,252,284` bytes;
- build log: `590,915` bytes, SHA-256 `126fa8b92e813391db6deb8f2c4ac68cdbfc1510e16e7be09184106b624c1dfb`;
- runtime log: `9,110` bytes, SHA-256 `092eefc0b3120c016bf013665efa9d71543489b1c1b55a3c7fb6647fb788251f`;
- executable: `117,179` bytes, SHA-256 `a5913c45416b0b4e943ac5f2ee5544469089f4c8489cfbf47658fbd7fb4a5efa`;
- `file`: Mach-O `x86_64 + arm64`; `codesign --verify --deep --strict`: pass;
- Apple M1/Metal runtime: one r50 readiness marker, one exact Graphics Card Assembly handoff success marker, graceful Input System shutdown, exit `0`, player residue `0`.

Artifacts live under `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue99-034f862-final-r14.bLfAPK`.

## Exact Windows clean validation gate

The separate clean worker at `C:\Users\mertk\Developer\PCShopEmpire3D\Game` matched exact commit `034f862cfdc85b93e44cc0c9dded26aafdffbee6` with `git status --porcelain` count `0`. Mac readback hashes matched Windows `Get-FileHash` for every accepted artifact.

| Gate | Result | SHA-256 |
|---|---:|---|
| Critical PlayMode | `8/8` | `0bbc50964e2bcc3f1be5fda3c8d135bcb854c6c54a9ce2222cbc724e36142f90` |
| Full EditMode | `733/733` | `f59287acdf261739df4e48363b1e34ab140558a2723de8975d9bd6e576162a5f` |
| Full PlayMode | `140/140` | `c7306a0db4548987122bccc228d6c5a1fcee720c8675d0e40a1bf92e1c5ffdcc` |

The Unity 6000.3.21f1 Windows x64 Development player is IL2CPP and Direct3D11-only. The build report is `1,350,304,438` bytes and the build log SHA-256 is `6430ea1eb1c9ef9d4c77a1a3d8026b7120f8208ab5973377b9f8eb77d98557c7`. Its marker records `settings-restored=ok project-settings=byte-exact`. `PC Shop Empire 3D.exe` is `667,136` bytes with SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.

The first direct SSH/Session-0 player attempt could not obtain an interactive window and was boundedly terminated; it is rejected evidence, not a PASS. The accepted transient scheduled task ran in unlocked interactive Session 2. `windows-native-d3d11-runtime-interactive.log` is `5,984` bytes with SHA-256 `d6156b8f4053bb6eda9ded7a4ba825935273f62d1e739d71932d48f4c1f3d303` and records:

- Direct3D 11.0, feature level 11.1;
- Intel Iris Xe Graphics;
- exact r50 readiness and Graphics Card Assembly success markers;
- runtime host `WindowsPlayer`, `graphics-api=Direct3D11`, `force-d3d11=requested`;
- task/player exit result `0`, graceful shutdown and final task/player/Unity residue `0`.

## Foreground Windows OS-input acceptance

The final `os-input-r3` harness ran against the real foreground player window in Session 2. Evidence receipt SHA-256 is `b7ecb84ec84478d86530ab7965c450fb5e80fcb00d8fb1a5167086e9b90de1c5`; runtime log SHA-256 is `37e14b2726e5dd96a02df8eb211257364ec951d8983bc7a4a3c554dfe953fce2`; harness SHA-256 is `c19be7c39957554a6ec795d4b173c8330cb3700927b4c6572f577a2bd1eeab11`.

The receipt proves:

- Session `2`, foreground handle equal to the player handle;
- S scan code `31` down/up accepted `1/1`;
- D scan code `32` down/up accepted `1/1`;
- relative mouse-only input accepted `18/18`;
- one combined call delivered W scan `17` + D scan `32` + mouse as `3/3`;
- while W+D remained held, another `30/30` relative mouse deltas were accepted;
- W and D release accepted `1/1`; player residue `0`;
- final receipt: `HARNESS_RESULT=PASS human=false input=Win32-SendInput keyboard=W+D mouse=relative simultaneous=true`.

The six 1536×960 screenshots have these exact SHA-256 values:

| Capture | SHA-256 |
|---|---|
| `00-baseline.png` | `c584fd9129352bc58571a6da8be4eec23ec79f1c7524d3311716f9e477955983` |
| `01-s-only.png` | `a0e38f2ecf8e9d8a1ef51b1a70b1a54d8bd725800e09311e22d92175c46c2e15` |
| `02-d-only.png` | `1d2b81c60c29dd3ffcd59813f8275e57e13cd902aa6a3de52bcafe436f980651` |
| `03-mouse-only.png` | `11cee6e05ba64f73fd74010426c923ddeb79f844ea243117ba19abee4db836b3` |
| `04-wd-mouse-held.png` | `a7b398e5f18d50023991185008aa1b4b549449db464373f05137594d8d4ceb85` |
| `05-wd-mouse-after.png` | `0bcdd84d0864f3d90788799a2e0a534566846635676251150486eefc32c94052` |

Pixel readback uses RGB crop `[left=0, top=2, right=828, bottom=616]`, then counts a pixel as changed when `max(abs(RGB_a - RGB_b)) > 10`. Python `3.12.13`, Pillow `12.3.0` and NumPy `2.3.5` produced:

| Exact pair | `changed_gt10` |
|---|---:|
| baseline → S-only | `65.1938%` |
| S-only → D-only | `76.5002%` |
| D-only → mouse-only | `66.2170%` |
| mouse-only → W+D+mouse held | `78.4556%` |
| held → after release | `2.4629%` |

These images are not used alone to claim simultaneous physical movement and camera telemetry. Acceptance is the combined chain of isolated movement images, isolated mouse-look image, concurrent OS delivery while keys remain held, and the exact PlayMode same-frame assertion that both CharacterController displacement and yaw/pitch change.

## Final residue and device boundary

After all accepted runs:

- Mac Git head/tree and scene blob matched the exact contract; worktree and `git diff --check` were clean; Unity/player residue was `0`.
- Windows remained exact `034f862`, dirty count `0`, game/Unity process count `0`, running `PSE-*` task count `0`.
- The attached `Alu Line` `/dev/disk4s1`, ExFAT label `cixanla`, remained visible and was not written for this closure. No exact-`034f862` USB claim is made.

The bounded Issue #99 product, platform and OS-input P1 gates are complete. Source/docs Guard, PR integration and Issue/Roadmap transitions are administrative steps performed from this exact evidence; parent Epic #10 remains open for later PC assembly, electrical/POST/OS/QA and Steam 1.0 work.

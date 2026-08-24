# Immutable Custom-PC Work Order and Physical Work Ticket Handoff — Checkpoint Evidence

**Date:** 24 August 2026<br>
**Issue:** [#66](https://github.com/cixanla/PC-Shop-Empire-3D/issues/66)<br>
**Draft PR:** [#67](https://github.com/cixanla/PC-Shop-Empire-3D/pull/67)<br>
**Core feature:** `f9545605baff423f05615e7326902e24dc82aeeb`<br>
**Core feature tree:** `c0ed1add79162c334df4fc833eedf1dfaeb5cbc8`<br>
**Technical head:** `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`<br>
**Technical tree:** `69ea366cc49e99b653f5d02d9c0f238b4906de69`<br>
**Source/docs checkpoint:** `4e1ef4322d9ef049e3aac915c611474f6bee92fd`<br>
**Source/docs tree:** `4df76fb1b50da53bdee7e65cb64acf0e73a5c018`<br>
**Closure status:** macOS/Windows technical evidence, exact source/docs checkpoint CI and local immutable staging verified; physical USB, final metadata/lifecycle closure pending

## Delivered playable result

GarageGraybox r34 converts one accepted ten-line custom-PC quote into one immutable build order and one immutable work ticket. The order freezes the exact customer, request, quote, managed reservation claim, authored workbench and all ten ProductId/ItemInstanceId/ReservationId line identities. Inventory publishes one terminal operation-keyed allocation receipt exactly once while all ten reservations and serialized items remain live, in place and unchanged.

The player completes the visible customer-to-workbench route with real W/S/A/D, mouse-look, keyboard/mouse or Input System gamepad. At the canonical workbench, a dedicated collider-safe interaction target requires range, focus, line of sight, empty hands and a fresh Interact edge. Pause, held/co-edge input and competing carry/assembly targets cannot steal or replay the action. The physical ticket shows the job identity, `10/10` exact reservations and `MONTAJA HAZIR • HENÜZ BAŞLAMADI`.

For the accepted `DEMO-GAMING-001` fixture, the complete rendered ticket is exactly three lines:

```text
İŞ EMRİ • DEMO-GAMING-001
10/10 PARÇA AYRILDI
MONTAJA HAZIR • HENÜZ BAŞLAMADI
```

The first line is job-identity dependent and the second line is reservation-count dependent; the quoted one-line status elsewhere in the living docs is a summary, not a replacement for this complete rendered receipt.

Issue #66 records the ASCII acceptance shorthand `MONTAJA HAZIR — HENUZ BASLAMADI`. The canonical Turkish runtime rendering above is its localized equivalent: the separator is the UI bullet `•` and `HENÜZ BAŞLAMADI` preserves Turkish diacritics. This is an explicit presentation normalization, not a different state or a replacement acceptance contract.

Exact replay returns the same order, ticket and allocation receipt without revision drift. Interrupted publication recovers only from the exact stored allocation. Foreign, forged, stale, duplicate, missing/extra-line, reservation-drift, capacity and identity failures mutate nothing. Quote and Assembly authorities remain isolated. No part is moved, released, consumed, deleted or attached and assembly does not begin.

## Source identity and regression

The core feature commit contains the domain, Inventory, tests, scene and runtime presentation. The current technical head retains the editor-only byte-exact Windows settings restore, adds deterministic same-frame Interact ownership between the physical work ticket, carried items and loaded carts, excludes customer-reserved shelf stock from stealing that action, and expands both real-input routes to reject automatic player/item/Inventory/Assembly mutation at every progression edge. The committed r34 scene remains byte-identical.

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| EditMode | 661/661 | `editmode.xml` | 552,379 | `2b88a60b12fd115c0a87c63e05ccb89b7b4baa66591dde8284f0e78e13202460` |
| PlayMode | 66/66 | `playmode.xml` | 172,890 | `c77d023f78590465613bb7f284ead98aad6d392cbc564111372e761b44cc3ee7` |
| Universal macOS build | Success | `macos-build.log` | 589,503 | `2e6d73b4bb3f3ddd116163dfa492637734494d34996e0e14e12620d1cf2c3e51` |
| Native Metal runtime | Success | `macos-runtime.log` | 6,244 | `3b3dfaa71011815c56d585ca0491f2275af9e80b44f49fcd8ca40f774770f527` |
| App executable | Universal | `PC Shop Empire 3D` | 117,179 | `628f9c367f22c91dde3a0b87eade8d2808ae4b0ae709cc2581e08be228cc70d2` |
| Committed scene | r34 runtime | `GarageGraybox.unity` | 2,910,046 | `005813677c28bdd2a2ae4f656c3c5ef9b2d04c786cd552bcb1746928b146cc0f` |

Both XML suites report zero failed and skipped tests. The macOS build report is `329,478,891` bytes. The executable contains `x86_64` and `arm64` Mach-O slices. `/Users/cixanla/Desktop/PC Shop Empire 3D.app` resolves to this verified build.

The active Apple M1/Metal player ran windowed at 1280×720 and emitted readiness `garage-custom-pc-work-ticket-r34-v1` plus the exact work-ticket success marker once. No work-ticket failure marker, assertion, null/missing reference or unhandled exception appeared.

## Windows exact-source IL2CPP and Direct3D11 evidence

The Windows validation clone was detached, clean and exact at technical head `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, tree `69ea366cc49e99b653f5d02d9c0f238b4906de69`. Unity 6000.3.21f1, Windows IL2CPP support and the Microsoft C++ toolchain produced the player from that source. Windows also passed full EditMode `661/661` and PlayMode `66/66`. The promoted smoke ran through the logged-on interactive console session rather than the non-interactive SSH service session.

| Gate | Result | Artifact | Bytes | SHA-256 |
|---|---:|---|---:|---|
| Windows x64 IL2CPP build | Success | `build-il2cpp-d3d11-rerun.log` | 483,404 | `ea0fe60ae49091b7ec6f37f7b22dee7bb95e6517b02b41d33315aa26a7274261` |
| Binary manifest | 3/3 | `binary-manifest.json` | 1,090 | `428f76609c089e7143dbbd45621232494cb4c28e54150d73c1f45c8d9d61a907` |
| Source receipt | exact/clean | `source-receipt.json` | 401 | `e9b4108514b877603dcdf356075f75d9b55af3dc6a95939d4f0f4a4a57236725` |
| Interactive D3D11 runtime | Success | `runtime-d3d11.log` | 5,163 | `4689305558c261ced4c0ccb1b189c04e90757da139629ab14734915c355fde97` |
| Runtime summary | accepted | `runtime-summary.json` | 674 | `09ae7e2705785e409609c21672b6c042338e4fda9b1365284fcce61050c8a0c6` |

The final build marker is:

```text
STAGE_A_BUILD_OK target=StandaloneWindows64 bytes=1328828053 path=C:\Users\mertk\Developer\PCShopEmpire3D\Builds\Local\Windows-IL2CPP-x64\PC Shop Empire 3D.exe scripting-backend=IL2CPP graphics-api=Direct3D11 settings-restored=ok project-settings=byte-exact
```

The binary receipt records:

- `PC Shop Empire 3D.exe`: 667,136 bytes, SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`.
- `GameAssembly.dll`: 44,839,424 bytes, SHA-256 `d9b264bfcba0a7172381816d9b187fae78e70559710af4901ebbf876eeeabf0c`.
- `UnityPlayer.dll`: 84,237,744 bytes, SHA-256 `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.

The final player reported WindowsPlayer, Direct3D 11.0 feature level 11.1 and Intel Iris Xe Graphics. Host marker count `1`, r34 readiness count `1`, exact work-ticket success count `1` and forbidden-token count `0`; the interactive task returned `0` and was deleted after readback.

The current-source runtime wrapper first verified the exact source receipt, target contract and player-executable hash against the binary manifest. One fresh interactive scheduled task then produced the accepted log, returned `0`, and was deleted after readback; no task or player process remained. Prior-head diagnostics are not part of this canonical evidence set.

## Native marker

```text
GARAGE_CUSTOM_PC_WORK_TICKET_RUNTIME_SMOKE work-order=immutable ticket=visible reservation-set=10 allocation=atomic input=keyboard+gamepad fresh-press=ok single-consumer=ok range=ok los=ok pause=ok replay=ok duplicate=fail-closed items=unchanged assembly=untouched presentation=ok invariants=ok
```

## Canonical Issue #66 acceptance matrix

This is the repository-owned one-to-one rendering of the 18 Issue #66 acceptance bullets. `TECHNICAL PASS` means the committed feature plus the promoted Mac/Windows artifacts prove that bullet. It does not imply local staging, physical USB, Issue/Project closure or PR merge. Issue #66 cannot claim final `18/18` while row 18 remains partial.

| # | Acceptance contract | Current gate | Canonical evidence |
|---:|---|---|---|
| 1 | Stable typed `BuildOrderId`, `WorkTicketId` and handoff `OperationId`. | TECHNICAL PASS | Core domain identities and authority tests in feature `f954560`. |
| 2 | Immutable order/ticket freezes exact request, quote, customer, claim, workbench and all ten line identities. | TECHNICAL PASS | Build-order/work-ticket authority tests plus ADR-0043 identity boundary. |
| 3 | Exact managed reservation set is revalidated immediately before commit. | TECHNICAL PASS | Inventory handoff pre-commit validation and fail-closed domain tests. |
| 4 | Inventory publishes one terminal operation-keyed allocation receipt while all ten reservations and serialized items remain live and unchanged. | TECHNICAL PASS | Allocation receipt/revision tests and r34 invariant marker. |
| 5 | Generic checkout consume/release is unused; no item is deleted, released, duplicated or moved. | TECHNICAL PASS | No-mutation tests compare item, reservation, container and revision state. |
| 6 | One quote, claim and target workbench owns only one active build order/ticket. | TECHNICAL PASS | Ownership uniqueness and conflict tests. |
| 7 | Exact replay returns the same records/receipt without revision drift; mismatched replay fails closed. | TECHNICAL PASS | Exact replay and mismatched replay tests; native marker `replay=ok duplicate=fail-closed`. |
| 8 | Interrupted publication recovers only from the exact stored allocation; no orphan or second allocation is possible. | TECHNICAL PASS | Publication-recovery and orphan/second-allocation tests. |
| 9 | Foreign, forged, stale, duplicate, missing/extra-line, reservation-drift, capacity and identity failures mutate nothing. | TECHNICAL PASS | Negative-path domain and Inventory regression matrix. |
| 10 | `CustomPcQuoteAuthority` and `AssemblyBuildAuthority` remain isolated; assembly revision, receipts, slots and cable state do not change. | TECHNICAL PASS | Authority isolation tests and native `assembly=untouched` marker. |
| 11 | Collider-safe physical ticket at the authored workbench shows job identity, exact `10/10` parts and assembly-not-started status. | TECHNICAL PASS | `CustomPcWorkTicketStationProjection`, committed r34 scene, complete three-line receipt above and native presentation marker. |
| 12 | Handoff requires authored range, focus, line of sight, empty hands and one fresh Interact edge. | TECHNICAL PASS | Work-ticket PlayMode range/focus/LOS/busy-hands/fresh-edge coverage. |
| 13 | Pause/co-edge/hold/release-repress and competing carry/assembly targets have one deterministic input owner. | TECHNICAL PASS | Keyboard/gamepad pause/co-edge and competing-target PlayMode coverage; native `single-consumer=ok`. |
| 14 | W/S/A/D, mouse-look, keyboard/mouse and gamepad remain functional over the full customer-to-workbench route. | TECHNICAL PASS | `KeyboardMouseCustomerToWorkTicketRoutePostsTicket` and `GamepadCustomerToWorkTicketRoutePostsTicket` start at authored spawn, calibrate all four cardinal directions and traverse the complete route through real Input System events; Mac/Windows r34 native station smoke remains separate. |
| 15 | Dashboard cannot teleport player, items, kit or assembly state. | TECHNICAL PASS | Every customer/request/quote/ticket progression edge in both real-input routes snapshots and rejects automatic player pose, physical projection identity/parent/pose/active/ownership/carry/cart/stack/stable-placement/last-safe state, serialized item/container state and Assembly revision/receipt mutation; no Dashboard handoff path exists. |
| 16 | EditMode and PlayMode prove atomicity, replay, recovery, targeting, input, visible state and invariant preservation. | TECHNICAL PASS | Current-source full EditMode `661/661` and PlayMode `66/66` passed on Mac and Windows after the input-owner/no-teleport strengthening. |
| 17 | macOS Development/StrictMode and native Metal smoke pass. | TECHNICAL PASS | Universal Mac build and accepted Apple M1/Metal r34 receipt above. |
| 18 | Windows x64 IL2CPP/D3D11 and physical USB readback are separate final gates before Done. | PARTIAL — WINDOWS + LOCAL STAGING PASS / USB PENDING | Exact Windows IL2CPP/D3D11 receipts above. The immutable local package passed incoming and final readback; physical USB two-readback and final metadata are still pending. |

## Normative local and physical checkpoint package contract

The Issue #66 package is created only after a clean source/docs commit. `SOURCE/` must come from that exact Git commit through `git archive` or an equivalent exact-tree export; copying the dirty working tree is forbidden. The only accepted package layout and evidence allowlist are:

```text
2026-08-24_STAGE_B_IMMUTABLE_CUSTOM_PC_WORK_ORDER_PHYSICAL_WORK_TICKET_HANDOFF/
├── SOURCE/
├── EVIDENCE/
│   ├── binary-manifest.json
│   ├── build-il2cpp-d3d11-rerun.log
│   ├── editmode.xml
│   ├── macos-build.log
│   ├── macos-runtime.log
│   ├── playmode.xml
│   ├── runtime-d3d11.log
│   ├── runtime-summary.json
│   └── source-receipt.json
├── SOURCE_COMMIT.txt
├── MANIFEST.tsv
└── MANIFEST.sha256
```

`SOURCE_COMMIT.txt` records at minimum the full source/docs commit and tree, feature commit and tree, branch, Issue/PR links, promoted test/build/runtime identities, scope and the explicit statement that local staging is not physical USB closure.

`MANIFEST.tsv` represents every payload file under `SOURCE/`, `EVIDENCE/` and `SOURCE_COMMIT.txt`, but not `MANIFEST.tsv` or `MANIFEST.sha256` themselves. Each line is exactly `<sha256>\t<byte-size>\t<package-relative-path>`, paths are unique and lexicographically sorted, and absolute paths, empty paths, `..` components and backslashes are forbidden. `MANIFEST.sha256` contains only the SHA-256 of `MANIFEST.tsv`.

Every local, incoming-USB and final-USB readback must independently prove:

- manifest SHA, row count, total logical bytes and exact hash/size/path equality;
- `SOURCE/` path count and byte-exact content equality against every blob in the recorded Git commit, with missing/extra/type/hash/size/path mismatches all zero;
- `EVIDENCE/` exact `9/9` allowlist and byte equality against `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62`, with missing/extra/hash/size/path mismatches all zero;
- root layout equality; duplicate path, symlink, forbidden cache/build/credential/key path, secret-pattern match, internal `._*` and sibling AppleDouble counts all zero;
- the recorded source/docs commit's separate Repository Guard success. The exported `SOURCE/` tree has no `.git` directory, so package readback does not misrepresent an export-local Guard run as source-commit CI proof.

Physical USB uses only verified `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D`. The final milestone is never overwritten. Copy goes to one collision-free `.incoming-*` sibling; AppleDouble cleanup is limited to that newly created incoming target. A first complete readback must pass before same-filesystem atomic rename, and the final directory must pass the same complete readback a second time. Existing milestones, unrelated incoming directories and user data are never changed.

All Issue #66 local/incoming/final readbacks invoke `Tools/verify-checkpoint-package.sh` with the fourth argument `issue66`; generic canonical mode cannot satisfy this milestone's exact 9-file acceptance gate.

## Repository and external checkpoint status

- Technical-source Repository Guard [32721069982](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32721069982) passed for `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`.
- Exact source/docs checkpoint `4e1ef4322d9ef049e3aac915c611474f6bee92fd`, tree `4df76fb1b50da53bdee7e65cb64acf0e73a5c018`, passed [Repository Guard 32723213686](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32723213686). Draft PR [#67](https://github.com/cixanla/PC-Shop-Empire-3D/pull/67) points to that source/docs checkpoint.
- Local immutable package `/Users/cixanla/Developer/PCShopEmpire3D/CheckpointStaging/2026-08-24_STAGE_B_IMMUTABLE_CUSTOM_PC_WORK_ORDER_PHYSICAL_WORK_TICKET_HANDOFF` passed the verifier before and after atomic final naming: manifest `906/906`, exact Git source `896/896`, evidence `9/9`, payload `17,330,935` bytes and manifest SHA-256 `1514481a5b8dc90aae89f6de1d0e49ac4c6e964ee280c8170e6e224206444121`.
- Physical USB identity and the previous milestone chain must be revalidated immediately before any write; no alternate volume is used and no physical closure is claimed here.
- The intended local/USB milestone name is `2026-08-24_STAGE_B_IMMUTABLE_CUSTOM_PC_WORK_ORDER_PHYSICAL_WORK_TICKET_HANDOFF`.
- The final package must use a collision-free `.incoming-*` target, remove AppleDouble files only inside that new incoming target, pass complete hash+size+path plus exact Git-source/evidence readback, atomically rename and pass a second complete readback.
- Issue #66 remains open and Roadmap `In Progress`; physical USB, final metadata/Guard, acceptance `18/18` and PR merge are not yet claimed.

## Bounded exclusions

Physical component transfer/build-kit completion, component attachment or cable routing changes, electrical power-on, POST/BIOS, fictional OS/drivers, benchmark/QA, packaging, delivery, payment/final settlement, Save/Guardian gameplay authority, final art/audio/VFX/UI, Steam packaging/signing and release readiness are not part of Issue #66.

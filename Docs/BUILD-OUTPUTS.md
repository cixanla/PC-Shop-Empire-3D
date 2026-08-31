# Builds

`../../Builds/Local/` yalnız yerel doğrulama buildleri içindir ve UVCS workspace'inin dışında kalır. Yayın buildleri ayrı, imzalı ve denetlenmiş bir süreçte üretilecektir.

## Stage B validation-bound quality sign-off ve packaging release doğrulaması — 31 Ağustos 2026

Issue #137 technical source `b6c0f629b78566d743dbb041bfaf792f7c0164c8`, tree `36f8cb6cec9340966181511a18f3caa276eb12f2` Mac üzerinde doğrulandı:

- macOS: Development/StrictMode Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.776.338` bayt, `304` dosya. Executable `117.179` bayt / SHA-256 `de920bcd2d1c0ac8c8e7317ba082356487d4c50999b2acb20cecb04fded00941`.
- Testler: scene/r67 `12/12`, quality authority/history `5/5`, keyboard/mouse/virtual-gamepad/context/P0 `4/4`, validation regression `6/6`, final full EditMode `810/810`, full PlayMode `191/191`; failed/skipped/inconclusive `0`.
- Native r67 runtime: Apple M1/Metal 1280×720; assisted exact validation setup, player-triggered safe power-off, mouse quality review, virtual-gamepad release, exact work order/ticket + ten serialized line lineage, score `401`, quality `Good`, immutable same-instance replay, history preservation, upstream isolation ve invariants başarılı. Exact readiness/success markerları birer kez, quality failure/fatal `0`, observed exit/residue `0`.
- Existing Workbench/station surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/input action yoktur. `WaitingForValidation / AwaitingSafeShutdown / ReadyForReview / Reviewing / ReadyForPackaging / Rejected / NotCurrent` presentation-only durumları ayrıdır; same-frame Interact strict priority'dir, review-context kaybı input'u korur ve malformed quality history explicit power-on'u bloklamaz.
- Build log `598.000` bayt / SHA-256 `aa4a23fe327f51748366e65baad424ffc753fe3086e4fbca8ad7e7beccf581fc`; runtime log `9.557` bayt / SHA-256 `d2fae1e5154fe632c8eb9dd9752c3eb841f21d3fac49afa39572e01e76e36b0d`.
- Draft PR #138 docs checkpoint `c7f5fbc2b5bb1485d96c09b497d1cdf0fbc45d9e` Repository Guard `33401676887` PASS.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı bounded quality sign-off ve `ReadyForPackaging` receipt'i için Mac teknik geliştirme kabulüdür; fiziksel package item/workcell/custody, shipping/teslimat, warranty/final settlement, save/load persistence, fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B driver-bound deterministic validation doğrulaması — 31 Ağustos 2026

Issue #135 technical source `f082ef5df913ce6a4664cdda5eb64d1b26f007d6`, tree `c387100c6dd7e314768756ebfb78104f6557081d` Mac üzerinde doğrulandı:

- macOS: Development/StrictMode Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.709.325` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `0e5bbb99a8eef26e6d121660788c5bec6c3de3c667725defb7e4f8b388a7672f`.
- Testler: performance catalog `5/5`, validation authority/history `125/125`, keyboard/mouse/virtual-gamepad/context/P0 `6/6`, scene/r66 `12/12`, power/POST/UEFI/OS/driver/validation regression `29/29`, final full EditMode `804/804`, full PlayMode `187/187`; failed/skipped/inconclusive `0`.
- Native r66 runtime: Apple M1/Metal 1280×720; assisted exact driver/current-cycle setup, player-triggered two-step validation review/run, score `401`, fixed `300` stable stress step, CPU/GPU peak `67/64 °C`, power `380/500/550 W`, margin `+50 W`, quality `Good`, same-instance replay, explicit power-off, current-after-power-off false, history preservation, upstream isolation ve invariants başarılı. Exact readiness/success marker birer kez, failure/fatal `0`, exit/residue `0`.
- Existing Workbench surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/input action yoktur. `Waiting / Reviewing / Passed / Rejected / NotCurrent` presentation-only durumları ayrıdır; same-frame Interact strict priority'dir, bütün review-context kayıpları input'u korur ve malformed validation history explicit power-off'u bloklamaz.
- Build log `601.732` bayt / SHA-256 `352714cc97f4423580e98ecaa1d47f494b65c0d16267d9a062c1d78f07f6d043`; runtime log `9.596` bayt / SHA-256 `4197d3e16e7d82045aed1833797023df01c6c054faac4dd02ad57d7bcf8917a6`.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı bounded fictional benchmark/stress/thermal validation receipt'i için Mac teknik geliştirme kabulüdür; gerçek benchmark binary/process, physical sensor/telemetry, wall-clock endurance, fan/airflow/noise, overclock/fault/damage, repair/save/delivery, fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B installed-OS-bound fictional driver doğrulaması — 31 Ağustos 2026

Issue #133 technical source `b144a3ef1a0ac5fcbd9704c850426baa9a727044`, tree `271bf53012e44e5162cdc5bdd2f41fa2cbbd3052` Mac üzerinde doğrulandı:

- macOS: Development/StrictMode Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.641.904` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `c347fd358af6c1afe8e5d89699995ebaf81a4e9c65b4ff0cc9ac3a9f79ad2ad7`.
- Testler: driver lineage/domain `5/5`, keyboard/mouse/virtual-gamepad/context/P0 `6/6`, scene `1/1`, power/POST/UEFI/OS/driver regression `23/23`, full EditMode `793/793`, full PlayMode `181/181`; failed/skipped/inconclusive `0`.
- Native r65 runtime: Apple M1/Metal 1280×720; assisted preflight, player-triggered power-on/POST/UEFI/OS, two-step driver review/install, exact M.2 identity, completion-time hardware/cable gate, same-instance replay, explicit power-off, same-OS/storage persistence, untouched benchmark ve invariants başarılı. Exact readiness/success marker birer kez, failure/fatal marker `0`; Input System shutdown, exit `0`, residue `0`.
- Existing Workbench surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/input action yoktur. `Waiting / Reviewing / Installed / Rejected` presentation-only durumları ayrıdır; same-frame Interact strict priority'dir, bütün review-context kayıpları input'u korur ve malformed driver history explicit power-off'u bloklamaz.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı bounded fictional driver installation receipt'i için Mac teknik geliştirme kabulüdür; gerçek vendor driver/download/installer/kernel/device enumeration/update/reboot, benchmark/stress/thermals/quality sonucu, save/delivery, fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B active-UEFI-bound fictional OS doğrulaması — 31 Ağustos 2026

Issue #131 technical source `9e6a2334a3d6d778b97ebb9ee6d43e7cd8dbc31f`, tree `dd06f64f295f17d7285938845217e19b9e30fe57` Mac üzerinde doğrulandı:

- macOS: Development/StrictMode Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.604.881` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `815d1e34a208eddd8272168f0859c1e7dc58b942f71d04eb0fded2f3f46d2244`.
- Testler: storage lineage/domain `4/4`, scene `1/1`, keyboard/mouse/virtual-gamepad/context/P0 `6/6`, power/POST/UEFI/OS regression `17/17`, full EditMode `788/788`, full PlayMode `175/175`; failed/skipped/inconclusive `0`.
- Native r64 runtime: Apple M1/Metal 1280×720; assisted preflight, player-triggered power-on/POST/UEFI, two-step OS review/install, exact M.2 identity, same-instance replay, explicit power-off, storage persistence, untouched benchmark ve invariants başarılı. Exact readiness/success marker birer kez, failure/fatal marker `0`; Input System shutdown, exit `0`, residue `0`.
- Existing Workbench surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/input action yoktur. Same-frame Interact strict priority'dir; bütün review-context kayıpları input'u korur ve malformed OS history explicit power-off'u bloklamaz.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı bounded fictional OS installation receipt'i için Mac teknik geliştirme kabulüdür; gerçek Windows/Linux/SteamOS, ISO/download/disk yazımı, partition/bootloader/reboot/licensing, driver/update/benchmark, save/delivery, fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B active-POST-bound UEFI baseline doğrulaması — 31 Ağustos 2026

Issue #129 technical source `86df0bc236e2bf90bfc3fa0482715f06242e6f13`, tree `953a09fd3c462e387229a78148c8b28040d797f3` Mac üzerinde doğrulandı:

- macOS: Development/StrictMode Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.573.681` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `2d55d534a6b692f2594c7135cb4b13b4fabc6085165e27d244187f8881700a1f`.
- Testler: firmware authority `3/3`, scene `1/1`, keyboard/mouse/virtual-gamepad/P0 `5/5`, power/POST regression `10/10`, full EditMode `784/784`, full PlayMode `169/169`; failed/skipped/inconclusive `0`.
- Native r63 runtime: Apple M1/Metal 1280×720; assisted preflight, player-triggered power-on/POST, mouse review, virtual-gamepad `KAYDET VE ÇIK`, keyboard explicit power-off, immutable same-instance replay, active-clear/history-preserve, untouched benchmark ve invariants başarılı. Exact readiness/success marker birer kez, failure/fatal marker `0`; Input System shutdown, exit `0`, residue `0`.
- Existing Workbench surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/input action/second authority yoktur. Same-frame Interact strict priority'dir ve malformed firmware history explicit power-off'u bloklamaz.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı bounded fictional UEFI safe-default review/save receipt'i için Mac teknik geliştirme kabulüdür; gerçek firmware/BIOS flashing, XMP/EXPO, boot/security settings, OS/driver, benchmark/thermals/damage, fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B active-power-on-bound baseline POST doğrulaması — 31 Ağustos 2026

Issue #127 technical source `30ca892c4c3411b8771c10a39856089ecc5cd3f1`, tree `eaf87358b42f96beb4f5b62d2bf65af78484d03b` Mac üzerinde doğrulandı:

- macOS: Development/StrictMode Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.548.985` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `4e1ebbba08867a7fa592d7b6b1868747ab4bc74210f86247e2446c80de86a87e`.
- Testler: final-source targeted POST authority `3/3`, full EditMode `781/781`, full PlayMode `164/164`; failed/skipped/inconclusive `0`.
- Native r62 runtime: Apple M1/Metal 1280×720; assisted exact preflight, player-triggered keyboard+gamepad power-on, active-cycle-bound immutable POST receipt, same-instance replay, `POST GEÇTİ`, firmware-waiting presentation, explicit power-off, energized maintenance block, untouched benchmark ve invariants başarılı. Exact readiness/success marker birer kez, failure/fatal marker `0`; Input System shutdown, exit `0`, residue `0`.
- Existing Workbench surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/second authority yoktur. Domain power-on ve POST iki açık command'dır; player path POST'u hemen tamamlar fakat failure durumunda power-off ulaşılabilir kalır.
- `ProjectSettings/ProjectSettings.asset` build öncesi/sonrası SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı bounded deterministic baseline POST receipt'i için Mac teknik geliştirme kabulüdür; gerçek hardware POST code/fault, connector pinout/polarity/rail/short-circuit fiziği, firmware/BIOS/UEFI, OS/driver, benchmark/thermals/damage, fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B safe power-state and maintenance-interlock doğrulaması — 31 Ağustos 2026

Issue #125 technical source `01b89e21e4329489b9a3c666edf5391710eb9c2f`, tree `bc1e5a8ec2e9852dd6d0b32c08b514bbd2c224a4` Mac üzerinde doğrulandı:

- macOS: Development Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.540.613` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `cd5643fbe7e455ca049ae29350a8847b984bf8a040efbdea419b42a32c989e26`.
- Testler: targeted authority/interlock/scene `6/6`, targeted keyboard/mouse + virtual-gamepad/presentation `4/4`, full EditMode `778/778`, full PlayMode `164/164`; failed/skipped/inconclusive `0`.
- Native safe-power runtime: Apple M1/Metal; assisted exact route/preflight, player-triggered keyboard+gamepad power-on/off, one-cycle Off final state, player-carry cable maintenance block, immutable replay, presentation ve invariant markerları başarılı. Input System graceful shutdown, exit `0`, player/Unity/shader/IL2CPP residue `0`.
- Existing Workbench focus/status surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/second Assembly authority yoktur. `GÜÇ AÇIK • POST BEKLİYOR` sınırı ve `BAKIM KİLİDİ AKTİF` görünürdür; benchmark `BuildIncomplete` kalır.
- Build-induced tek `ProjectSettings.asset` preloaded-assets hunk'ı kanıtla repository baseline'a döndürüldü. User/editor-owned ProBuilder ayarı SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` ile korundu ve technical commit'e alınmadı.

Bu çıktı Mac teknik geliştirme kabulüdür; fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, connector/fault/POST/BIOS/OS/benchmark, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B player-triggered power-test preflight doğrulaması — 31 Ağustos 2026

Issue #123 technical source `3c26ce0d6de80c975b064f2dff68d96fbd4378bc`, tree `58dd983e314ecb78d94b3871dc672641e0a87b5d` Mac üzerinde doğrulandı:

- macOS: Development Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.507.808` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `c39ab49b5177b05935a18cc93e7e05d3327ba91c59405b10a421f6c13f558c1f`.
- Testler: targeted domain/scene `6/6`, targeted keyboard/mouse + virtual-gamepad/presentation `3/3`, full EditMode `773/773`, full PlayMode `161/161`; failed/skipped/inconclusive `0`.
- Native power-test runtime: Apple M1/Metal; assisted exact route readiness `380/500/550`, keyboard+gamepad single-consumer, range/focus/LOS/pause/co-edge, immutable same-instance replay, stale-current detection, zero gameplay mutation, untouched benchmark, presentation ve invariant markerları başarılı. Input System graceful shutdown, exit `0`, player residue `0`.
- Existing Workbench focus/status surface reused; yeni gameplay collider/renderer/light/camera/NavMesh/item/second authority yoktur. Power-on açıkça `not-started` kalır.
- User/editor-owned ProBuilder ayarı aynı SHA-256 ile korundu ve technical commit'e alınmadı; başka ProjectSettings veya Packages farkı yoktur.

Bu çıktı Mac teknik geliştirme kabulüdür; fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, gerçek power-on/connector/fault/POST/BIOS/OS/benchmark, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B exact system power budget and PSU headroom doğrulaması — 31 Ağustos 2026

Issue #121 technical source `57e6b54883ef6756c5522d1de9c17479e7cda481`, tree `8652882bb5e791c969b9c8648cfe7e242a5a92d7` Mac üzerinde doğrulandı:

- macOS: Development Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.465.045` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `ed6ede7c7cdb48c359df33cad4bbfd228489271abb972a898f88e39a4ef70798`.
- Testler: targeted catalog/authority/scene/input/hero `15/15`, full EditMode `768/768`, full PlayMode `158/158`; failed/skipped/inconclusive `0`.
- Native power-budget runtime: Apple M1/Metal 1280x720; exact final-cable route/readiness zinciri `power-budget=380/500/550`, monitor/no-duplicate-loss/invariant markerları başarılı. Input System graceful shutdown, exit `0`, player residue `0`.
- Native Assembly readability: `479` total / `470` smoke-active renderer, `4` light, `1` camera; üç byte-distinct 1280x720 capture ve central glare `0`.
- Complete `ProjectSettings + Packages` manifesti build öncesi/sonrası byte-exact. Ayrı user/editor-owned ProBuilder ayarı da build boyunca aynı kaldı ve technical commit'e alınmadı.

Bu çıktı Mac teknik geliştirme kabulüdür; fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, connector/pinout/short-circuit fiziği, power-test/power-on/POST/BIOS/OS/benchmark, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B exact electrical readiness ve workbench feedback doğrulaması — 31 Ağustos 2026

Issue #119 technical source `f33a052d3f3ef25d48ff8b5d5f4d4a149f414fdc`, tree `986ff174209dc55bb98cf7f1151fc8cc480384fc` Mac üzerinde doğrulandı:

- macOS: Development Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.441.141` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `4a903a2b3c4ef6f9c283a603d74c891d3cb70f402e3c69d1a15ec9750c093b85`.
- Testler: full EditMode `758/758`, full PlayMode `158/158`; failed/skipped/inconclusive `0`. Scene, fail-closed/read-only projection ve keyboard/mouse + Input System virtual-gamepad targeted contracts ayrıca geçti.
- Native readiness runtime: Apple M1/Metal 1280×720; exact final-cable route `blocked → ready`, aynı cable unroute `ready → blocked`, workbench monitor ve no-duplicate-loss/invariant markerları başarılı. Input System graceful shutdown, exit `0`, player residue `0`.
- Native Assembly readability: `479` total / `470` smoke-active renderer, `4` light, `1` camera; üç byte-distinct 1280×720 capture ve central glare `0`.
- ProjectSettings manifesti build öncesi/sonrası byte-exact. Ayrı user/editor-owned ProBuilder ayarı da build boyunca aynı kaldı ve technical commit'e alınmadı.

Bu çıktı Mac teknik geliştirme kabulüdür; fiziksel Windows x64 IL2CPP/only-D3D11 Intel Iris Xe, power-on/POST/BIOS/OS/benchmark, physical-human HID/endurance, USB checkpoint, Steam packaging/signing veya release-candidate iddiası değildir. UTM fiziksel Windows kapısının yerine geçmez.

## Stage B canonical PCIe/GPU 6+2 BuildKit-to-route reversible Assembly doğrulaması — 27 Ağustos 2026

Issue #109 technical source `1acba166855efffa906112e2df24b9b5cef550a7`, tree `eb40a392169e5288e29bc59ae75367029cc00f57` aynı kaynak kimliğiyle iki gerçek platformda doğrulandı:

- macOS: Development Universal Mach-O (`arm64` + `x86_64`), deep/strict codesign geçti; Unity report `330.366.591` bayt, `302` dosya. Executable `117.179` bayt / SHA-256 `80556318de7d2aa5e1f1f0abc8315cc0a0453c67a7a804fe0e9c4df467879dd0`.
- macOS testleri: targeted domain EditMode `87/87`, scene contract `9/9`, P1 PlayMode `4/4`, full EditMode `752/752`, full PlayMode `156/156`; failed/skipped/inconclusive `0`.
- macOS runtime: Apple M1/Metal, pencereli exact r54 readiness ve canonical PCIe/GPU BuildKit→route→unroute Assembly smoke başarılı; Input System graceful shutdown, exit `0`, player residue `0`.
- Windows: Development x64 IL2CPP, Direct3D11 only; Unity report `1.349.053.878` bayt, output `666` dosya / `1.349.222.872` bayt. Build fatal-token sayısı `0`, `ProjectSettings.asset` byte-exact restore edildi ve clone temiz kaldı.
- Windows native binaries: `PC Shop Empire 3D.exe` `667.136` bayt / `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`; `GameAssembly.dll` `45.821.952` bayt / `f327f7036c483fa6edcfcfcc1a6cfd261bd6472e23d337a2f59c01e8fd7522a7`; `UnityPlayer.dll` `84.237.744` bayt / `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.
- Windows testleri: full EditMode `752/752`, full PlayMode `156/156`; failed/skipped/inconclusive `0`.
- Windows runtime/input: Intel Iris Xe, Direct3D 11.0 feature level 11.1; exact host/readiness/success `1/1/1`, forbidden `0`, graceful exit. Foreground Win32 OS-input W/A/S/D, relative mouse ve W+D-held + mouse zinciri geçti; `human=false`.
- Windows final audit `28/28`; process/task/firewall residue `0/0/0`. Kanıt arşivi `4.599.837` bayt / SHA-256 `924792e2c4dd239e8b5209b9f8eaed8b8d248a9ca93cfe597d39450785db74e4` ile Mac'te exact readback verdi; geçici Windows validation kökü sonrasında kaldırıldı.

Bu çıktılar Issue #109 teknik geliştirme kabulüdür; üç cable routed olsa da electrical power-on/POST/BIOS/OS/benchmark, real-human fiziksel HID/gamepad/endurance, Steam packaging/signing ve release-candidate iddiası değildir. Kabulden sonra Windows'taki USB yalnız kimlik/sağlık için okundu ve yazılmadı.

## Stage B accepted custom-PC request, immutable quote and exact reservation doğrulaması — 24 Ağustos 2026

Issue #64 feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95` aynı kaynak kimliğiyle iki gerçek platformda doğrulandı:

- macOS: Development + StrictMode Universal Mach-O (`arm64` + `x86_64`), Unity report `329.396.456` bayt; executable `117.179` bayt, SHA-256 `9cfdbf7d17583135550bd6a507164f644b8242e9bfbcfaf26641191a69c249bf`.
- macOS testleri: EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`.
- macOS runtime: aktif Apple M1/Metal, 1280×720; readiness `garage-custom-pc-quote-reservation-r33-v1` ve exact custom-PC quote/reservation smoke başarılı.
- Windows: Development + StrictMode x64 IL2CPP, Unity report `1.326.137.709` bayt. `PC Shop Empire 3D.exe` `667.136` bayt / SHA-256 `9688ae089d590352dbe0cba8722328926cd4d9b7722f68ec9942243925c1282a`; `GameAssembly.dll` `44.777.472` bayt / `2978b79b47d4c6aefef58d81f7235940b9df4d4794fb0935dfa3a5233b960021`; `UnityPlayer.dll` `84.237.744` bayt / `cc018c912f461b0f7bdcaeadd5c1d8d9361d92fe5a1c3aeac27a0ac1186a4a59`.
- Windows testleri: EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`.
- Windows runtime: aktif console oturumunda Intel Iris Xe Graphics, Direct3D 11.0 feature level 11.1; readiness r33 ve exact custom-PC smoke bir kez başarılı. Windows masaüstü kısayolu bu doğrulanmış IL2CPP player'a okunarak bağlandı.
- Kısa görünür Mac insan turu sağ/geri/sol konum değişimini, pause sırasında sıfır hareketi ve resume sonrası fresh-input hareketini gözledi. Fare sürükleme aracı pencere erişimini kaybettiği için ayrıca manuel mouse-look iddiası kurulmadı.

Bu çıktılar geliştirme doğrulamasıdır; Steam packaging/signing, performans matrisi ve release candidate iddiası değildir. Ayrıntılı hashler tarihli Issue #64 evidence belgesindedir.

## Stage A doğrulama çıktıları — 11 Ağustos 2026

| Çıktı | Tür | Unity rapor boyutu | Yerel disk kullanımı | Sonuç |
|---|---|---:|---:|---|
| `macOS/PC Shop Empire 3D.app` | Development Player, Universal Mach-O (`arm64` + `x86_64`) | 325.608.373 bayt | yaklaşık 311 MiB | Build başarılı; `-batchmode -nographics` açılış/kapanış smoke testi başarılı |
| `Windows-Mono-x64/PC Shop Empire 3D.exe` | Development Player, PE32+ Windows x86-64, Mono | 166.141.340 bayt | yaklaşık 159 MiB | Build başarılı; gerçek Windows cihazında çalıştırma henüz yapılmadı |

Ana çalıştırılabilir dosya SHA-256 değerleri:

- macOS: `667db19ec9d71e1493ed412fc006a7323ae56834bb27e4e5e11f803a075254b5`
- Windows: `c8b0d73dc40e4f2cddbf656cfb7257fcb8273da22e44e12a8694cd8e275c6fb2`

Tüm dosyaların `SHA-256 + mantıksal boyut + göreli yol` satırlarının sıralı akışından alınan içerik-kümesi özetleri:

- macOS: `c02d984d0f83221ab2eb64bb31a552f7f549d6fc64b64cdcd8d5048db4bc32dd`
- Windows: `c4cbdb8b391a5130e4da383bfd031d7da1a584f4e84771a33cdb660a2ae5e949`

Windows çıktısı yalnız erken taşınabilirlik kanıtıdır. Final Windows sürümü gerçek Windows x64 makinede IL2CPP, DirectX/GPU, sürücü ve Steam testlerinden geçmeden yayın adayı sayılmaz. macOS çıktısı imzalı/notarize edilmiş dağıtım paketi değildir.

## Stage B küçük kutu placement doğrulaması — 13 Ağustos 2026

`macOS/PC Shop Empire 3D.app` güncel kaynakla yeniden üretildi:

- Tür: Development Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: 326.147.564 bayt.
- Ana executable SHA-256: `f338b7479b28766ffe965548e1b0167a31af42cc72dc3d366e481a3c761476bb`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `motor=ok input=ok carry=ok placement=ok`.
- Görsel smoke: gerçek player'da pickup, dinamik placement prompt'u ve kırmızı `ENGELLİ` ghost doğrulandı.

Bu çıktı yalnız yerel geliştirme kanıtıdır; imzalı/notarize yayın paketi veya Windows native doğrulaması değildir.

## Stage B authoritative motherboard seating doğrulaması — 15 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #53 feature `582a3cf` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: `328.020.817` bayt; app disk kullanımı `320.976 KiB`.
- Ana executable: `117.179` bayt; SHA-256 `cad75f5e070dfabe0335f9c6ee8d50659dc3ceddd1e036cb63c83b787e5da0f0`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-motherboard-seating-r22-v1` readiness ve exact assembly smoke başarılı.
- Test kapıları: EditMode `394/394`, PlayMode `26/26`; failed/skipped `0`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. Gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ve yayın imzası henüz tamamlanmamıştır.

## Stage B deterministic CPU socket seating ve retention doğrulaması — 16 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #55 feature `99cadad` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: `328.144.884` bayt.
- Ana executable: `117.179` bayt; SHA-256 `d87710b6c5f12fc832bd0a8a1eba317e1074e913beae24daa3d39436737e24f0`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-cpu-socket-retention-r24-v1` readiness ve exact CPU socket smoke başarılı.
- Test kapıları: EditMode `430/430`, PlayMode `31/31`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue55-r2.log`, `582.457` bayt, SHA-256 `042ffeeb60f45013dcf5c0c03a1d0a308e1cf1406fd5d3daa83e5e38c17ac34f`.
- Runtime log: `runtime-processor-issue55-r2.log`, `5.000` bayt, SHA-256 `b9d0fd1dff5d702f3c74d67e09c1b11dc5e30028effaece3045cd7993581e799`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single air-cooler seating ve four-point retention doğrulaması — 22 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #58 feature `e2f10a2` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.534.723` bayt.
- Ana executable: `117.179` bayt.
- Runtime: aktif Apple Silicon/Metal makinesi Apple M1, pencereli 1280×720; `garage-processor-cooler-r27-v1` readiness ve exact cooler smoke başarılı.
- Test kapıları: EditMode `521/521`, PlayMode `38/38`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue58-final.log`, `585.965` bayt, SHA-256 `e32a2a1c8b661a8320e14511eee9d415d6b07c649594cd503221c9e23de99bed`.
- Runtime log: `runtime-cooler-issue58-metal-final.log`, `5.282` bayt, SHA-256 `365bfd3ad8302f65af5a2121a4c36f0c5029d4128694a263cce1dc439b3f32d1`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single ATX24 split-PSU cable routing doğrulaması — 23 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #61 feature `1fc29f1` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `329.082.160` bayt.
- Ana executable: `117.179` bayt; SHA-256 `04060db71ecd39f083a526b88e9468bd26ca26c18b12499b6e9dca3da19d85ab`.
- Runtime: aktif Apple M1/Metal makinesi, pencereli 1280×720; `garage-atx24-power-cable-routing-r30-v1` readiness ve exact cable smoke başarılı.
- Test kapıları: EditMode `589/589`, PlayMode `49/49`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue61-final.log`, `605.068` bayt, SHA-256 `e8730ba8f2975c16fbdc8034f6554aeb32313048c2ce64202087795d7d930c4e`.
- Runtime log: `runtime-power-cable-issue61-final.log`, `5.628` bayt, SHA-256 `c25c8cb9e95039d57b0ec70294f95efb69112705cd32baa171d65e34833df2d3`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single ATX PS/2 PSU seating ve four-screw rear retention doğrulaması — 23 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #60 feature `f998d7d` + authored-clearance fix `b6c3ff8` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.937.592` bayt.
- Ana executable: `117.179` bayt; SHA-256 `44045bf514841be7bd268e9032448583499bc416fe809ceac0196dd51b0e91f6`.
- Runtime: aktif Apple Silicon/Metal makinesi Apple M1, pencereli 1280×720; `garage-psu-four-screw-r29-v1` readiness ve exact PSU smoke başarılı.
- Test kapıları: EditMode `577/577`, PlayMode `47/47`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue60-final.log`, `585.248` bayt, SHA-256 `462d0f5d3d07de4314ab89b356adc529e854541a332d1d43bf954a457e2dd305`.
- Runtime log: `runtime-psu-issue60-final-activated.log`, `7.468` bayt, SHA-256 `574eb272912dcac4ca18590954a18fd6e711c4ef88576f713bccaba14b437b40`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single PCIe x16 graphics-card seating ve rear-bracket retention doğrulaması — 22 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #59 feature `1b29ad2` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.781.520` bayt.
- Ana executable: `117.179` bayt; SHA-256 `c3849bc7dfa05c1116c772952ad77085cad86f7feab22bf8dcca43478ff8fbea`.
- Runtime: aktif Apple Silicon/Metal makinesi Apple M1, pencereli 1280×720; `garage-gpu-rear-bracket-r28-v1` readiness ve exact GPU smoke başarılı.
- Test kapıları: EditMode `548/548`, PlayMode `43/43`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue59-final-r2.log`, `584.629` bayt, SHA-256 `ed9ff2282c816a159eb6947c15c5076f7c91125b52ca70a84ef7a27a5a6f80d9`.
- Runtime log: `runtime-gpu-issue59-metal-final-r2.log`, `5.386` bayt, SHA-256 `f8c1d5d8c79c58a7fc3b2a7ca162a8d6f3a1d27b30ae44a2046f77ebee1fccd2`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single M.2 NVMe seating ve captive screw doğrulaması — 16 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #57 feature `4f14e7b` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`), ad-hoc signed.
- Unity build raporu: `328.362.356` bayt.
- Ana executable: `117.179` bayt.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-m2-nvme-captive-screw-r26-v1` readiness ve exact storage smoke başarılı.
- Test kapıları: EditMode `490/490`, PlayMode `35/35`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue57-final.log`, `600.974` bayt, SHA-256 `560a20ee380ffe5fd76e12b5c48d5dc843557e27b5a571ea90c6eefac51baad3`.
- Runtime log: `runtime-storage-issue57-final.log`, `5.206` bayt, SHA-256 `5e8a250452c5a487692646b0626dd6aa03ccacd68267a6c37cab62e083ebb858`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

## Stage B deterministic single DIMM seating ve dual-latch retention doğrulaması — 16 Ağustos 2026

`macOS/PC Shop Empire 3D.app`, Issue #56 feature `7482fc9` kaynak durumuyla yeniden üretildi:

- Tür: Development + StrictMode Player, Universal Mach-O (`arm64` + `x86_64`).
- Unity build raporu: `328.268.700` bayt.
- Ana executable: `117.179` bayt; SHA-256 `eba2a0baeecb9a214a3d0520f4a94641e84b697b3d79f785ec124e4d1932eb50`.
- Runtime: Apple M4/Metal, pencereli 1280×720; `garage-dimm-dual-latch-r25-v1` readiness ve exact DIMM smoke başarılı.
- Test kapıları: EditMode `461/461`, PlayMode `33/33`; failed/skipped/inconclusive `0`.
- Build log: `build-macos-issue56-final.log`, `582.591` bayt, SHA-256 `49fd863b79bb50b3138471c6efbf7d33a33f66e2f482175abf529b18baa38c3d`.
- Runtime log: `runtime-dimm-issue56-final.log`, `5.140` bayt, SHA-256 `03d45cac685bbe1295ec2181ff7d3a36aed16289ce272bb813b1de4f46b6cc4f`.

Bu çıktı yalnız yerel geliştirme kanıtıdır. İmzalı/notarize dağıtım paketi değildir; gerçek Windows x64 hostta IL2CPP/DirectX/Steam kapısı ayrı kalır.

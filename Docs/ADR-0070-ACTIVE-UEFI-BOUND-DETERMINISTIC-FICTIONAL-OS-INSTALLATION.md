# ADR-0070 — Active-UEFI-Bound Deterministic Fictional OS Installation

**Status:** Mac technical source `9e6a233`, tree `dd06f64` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #132 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #131; Issue #129 exact current UEFI baseline receipt'i üzerine kuruludur

## Context

Issue #129, exact active POST → power-on → preflight lineage'ına bağlı immutable kurgusal UEFI baseline receipt'ini kurdu. Steam 1.0 fiziksel PC servis zincirindeki sıradaki bounded adım, bu exact current UEFI sonucu üzerinden bir işletim sistemi kurulumunun hazırlandığını ve tamamlandığını oyuncunun mevcut elektrik test istasyonunda açıkça yürütmesidir.

Bu dilim gerçek Windows, Linux veya SteamOS değildir. ISO, download, disk yazımı, partition, bootloader, reboot, boot order, lisans, driver, update, benchmark, save veya teslimat üretmez. Ürün sözleşmesi yalnız fictional `WorkshopStandard / InstalledForDriverStage` sonucu ve onun exact fiziksel depolama provenance'ıdır.

## Decision

- Ayrı `PcFictionalOsInstallationAuthority`, exact `PcPowerStateAuthority` ve onun exact `AssemblyBuildAuthority` instance'ına bağlanır. Power, POST, firmware veya Assembly ledger'ının sahipliğini üstlenmez.
- Completion non-empty stable operation ID, `EvaluateCurrentFirmwareBaseline()` tarafından dönen exact same `PcFirmwareBaselineReceipt`, expected current power-state revision, expected OS revision ve exact secured M.2 item ister.
- Receipt exact firmware → POST → power-on → preflight lineage'ını; storage item/product kimliğini; source storage-secure operation'ını; source full Assembly revision'ını ve monotonik OS revision'ını immutable taşır.
- Source storage lineage hem command creation hem history fold sırasında preflight snapshot'ındaki exact item/product/secure-operation/full Assembly revision ile eşleştirilir. Source secure Assembly receipt'i aynı item/product ve `SecureStorageDevice` operation'ı olmalı; bu receipt full build snapshot'ından daha erken revision'da olabilir.
- Null, foreign, stale, historical, Off-cycle, owner-mismatch, unsecured/missing storage veya malformed history fail-closed olur. Exact command replay same-instance; changed reuse `OperationConflict`; aynı fiziksel storage item ya da aynı firmware kaynağı için ikinci distinct completion `AlreadyCompleted`dır.
- Installed sonucu power cycle'a değil exact storage item kimliğine aittir. Power-off sonucu silmez. Storage çıkarılınca current-build değerlendirmesi `NotCurrent`; aynı fiziksel item reseat/resecure edilince historical receipt üzerinden reinstall olmadan Installed olur. Farklı item kurulu sayılmaz.
- Existing station, focus anchor, range, LOS, hands/ownership gates ve Input System actions tekrar kullanılır. İlk `LMB / RT` review açar, ikinci `LMB / RT` install completion yürütür. Yeni modal cursor, scene geometry, collider veya input action eklenmez.
- Açık OS review; pause/Pause edge, range/focus/LOS kaybı, hands-busy veya competing-owner oluştuğunda input tüketmeden sıfırlanır. Oyuncu döndüğünde review ve completion için tekrar iki ayrı Primary Action gerekir.
- `E / A` explicit power-off strict priority taşır. Same-frame Interact + Primary Action yalnız power-off yürütür, co-edge'i tüketir ve OS receipt üretmez. Malformed OS history explicit power-off'u softlock edemez.
- Workbench yalnız observer'dır. Authority yaratmadan waiting, review, installed, Off-persistent ve rejected durumlarını ayırır; guidance gameplay state üretmez.
- Inventory, BuildKit, reservation, custody, Economy, Assembly, cable routes, power transition, POST, firmware ve benchmark authority'leri OS completion tarafından değiştirilmez. `EvaluateBenchmarkReadiness()` bu dilimde `BuildIncomplete` kalır.
- GarageGraybox r64 native smoke nested r63 UEFI prerequisite'ini tek üst sonuç altında çalıştırır; alt success marker'ını bastırır, alt failure'ı üst smoke'a taşır ve keyboard + mouse + virtual gamepad install/power-off zincirini doğrular.

## Consequences

GarageGraybox `garage-fictional-os-installation-r64-v1`, oyuncuya exact current UEFI baseline sonrasında bounded fictional OS review/install akışı sunar. Kurulum exact fiziksel M.2 item ile kalıcıdır; makine kapansa veya aynı item sökülüp yeniden takılsa bile kaybolmaz. Farklı disk veya bozuk lineage fail-closed kalır.

Yeni gerçek OS teknolojisi, driver sistemi veya benchmark sonucu oluşturulmamıştır. Sıradaki bounded ürün dilimi bu immutable OS receipt'ine bağlı ayrı driver hazırlık/kurulum akışıdır; real OS media, reboot ve release packaging kapsam dışı kalır.

## Current verification

- Technical source `9e6a2334a3d6d778b97ebb9ee6d43e7cd8dbc31f`, tree `dd06f64f295f17d7285938845217e19b9e30fe57`; draft PR #132 clean/mergeable'dır. Issue/Roadmap açık ve In Progress kalır.
- Targeted lineage `4/4`, scene `1/1`, OS input/context/P0 `6/6`, power→POST→UEFI→OS regression `17/17`, full EditMode `788/788`, full PlayMode `175/175`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `330,604,881` bayt ve `302` dosyadır. Executable `117,179` bayt, SHA-256 `815d1e34a208eddd8272168f0859c1e7dc58b942f71d04eb0fded2f3f46d2244`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 graphical r64 native smoke readiness/success markerlarını birer kez üretir; failure/fatal `0`, Input System shutdown `1`, exit `0`, player/Unity/shader residue `0`dır.
- Local Repository Guard ve run `33372528502` geçer. `git diff --check` temizdir.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.

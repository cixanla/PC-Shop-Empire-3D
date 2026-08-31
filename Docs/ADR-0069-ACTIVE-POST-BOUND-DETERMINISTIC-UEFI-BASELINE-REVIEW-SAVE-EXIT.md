# ADR-0069 — Active-POST-Bound Deterministic UEFI Baseline Review and Save/Exit

**Status:** Mac technical source `86df0bc`, tree `953a09f` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #130 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #129; Issue #127 exact active-cycle baseline POST receipt'i üzerine kuruludur

## Context

Issue #127, exact active power-on ve preflight lineage'ına bağlı immutable baseline POST receipt'ini kurdu. Oyuncu güç açıp deterministic POST sonucunu görebiliyor ve explicit power-off ile güvenli biçimde çıkabiliyordu; firmware/UEFI bilinçli olarak tamamlanmamıştı. Steam 1.0 ürün zincirindeki sıradaki bounded adım, aynı mevcut Workbench üzerinden tek bir kurgusal güvenli varsayılan profili gözden geçirip açıkça kaydetmek ve çıkmaktır.

Bu dilim gerçek BIOS üreticisi arayüzü, firmware binary'si veya flashing değildir. XMP/EXPO, voltaj, fan eğrisi, boot order, Secure Boot/TPM, gerçek hardware fault, OS kurulumu, driver veya benchmark sonucu üretmez. Böyle geniş sistemleri tek boolean ile sahte biçimde tamamlamak yerine, yalnız exact current POST cycle'ına bağlı küçük ve immutable bir ürün kanıtı gerekir.

## Decision

- İkinci firmware authority oluşturulmaz. Existing `PcPowerStateAuthority`, power ve POST ledger'larından ayrı monotonik revision'a sahip `PcFirmwareBaselineReceipt` history'sini de taşır.
- `TrySaveFirmwareBaseline(...)`; non-empty stable operation ID, exact current `ActivePostStartupReceipt` instance'ı, expected current power-state revision ve expected firmware revision ister. Null, foreign, stale, historical veya Off-cycle kaynaklar fail-closed olur.
- Başarılı receipt yalnız `Profile = OptimizedDefaults` ve `Result = SavedAndExited` taşır. Exact source POST üzerinden aynı power-on, preflight, Assembly, component, cable, route ve power-budget lineage'ına bağlıdır.
- Exact same-command replay aynı receipt instance'ını döndürür. Changed reuse `OperationConflict`; aynı POST için ikinci distinct completion `AlreadyCompleted` olur. Her source POST için en fazla bir firmware receipt bulunur.
- Power-off yalnız active firmware pointer'ını temizler. Historical lookup ve exact replay immutable kalır; sonraki power cycle yeni POST, operation ID ve firmware revision ister.
- Existing station ve Workbench surface reused. İlk Primary Action `UEFI SETUP • OPTIMIZED DEFAULTS` review state'ini açar; ikinci Primary Action `KAYDET VE ÇIK` komutunu yürütür. Yeni input action veya modal cursor haritası eklenmez.
- Interact, Primary Action'a strict priority taşır. Aynı frame `E/A + LMB/RT` gelirse yalnız power-off yürür, co-edge Primary Action tüketilir ve firmware save oluşmaz.
- Paused veya competing-owner durumda giriş çalıştırılmaz ve tüketilmez. Review state exact active POST/power context'i kaybolduğunda sıfırlanır.
- P0 recovery kuralı olarak energized Interact, downstream POST/firmware history validation'dan önce explicit power-off branch'ine ulaşır. Review, save rejection veya malformed firmware history makineyi energized softlock edemez.
- Presentation salt-okunurdur. `UEFI SETUP BEKLİYOR`, review, `UEFI BASELINE KAYDEDİLDİ • SONRAKİ AŞAMA: OS`, rejected ve Off durumlarını ayırır; guidance text OS state üretmez.
- Inventory, BuildKit, reservation, custody, Assembly, ATX24/EPS12V/PCIe-GPU, Economy, power transition, POST ve benchmark state'leri firmware save tarafından değiştirilmez. Benchmark `BuildIncomplete` kalır.
- GarageGraybox r63 native smoke keyboard power-on, mouse review, virtual-gamepad save/exit ve keyboard power-off zincirini; same-instance replay, active-clear/history-preserve, untouched benchmark ve full invariants ile doğrular.

## Consequences

GarageGraybox `garage-firmware-baseline-r63-v1`, exact current POST'tan sonra oyuncuya açık iki aşamalı kurgusal UEFI safe-default akışı sunar. Receipt history mevcut power authority'nin içinde kalır; session yalnız stable operation ID üretir, station command surface olur, Workbench ise salt-okunur observer kalır.

Yeni collider, renderer, light, camera, NavMesh obstacle, scene geometry, physical item, Assembly authority veya input action eklenmez. Existing focus anchor, range, LOS, empty-hands, competing-owner ve maintenance interlock sözleşmeleri değişmeden korunur. Sıradaki ürün dilimi gerçek firmware genişletmesi değil, bu receipt'e bağlı ayrı kurgusal OS hazırlık/kurulum sözleşmesidir.

## Current verification

- Technical source `86df0bc236e2bf90bfc3fa0482715f06242e6f13`, tree `953a09fd3c462e387229a78148c8b28040d797f3`; draft PR #130 open/mergeable'dır. Issue/Roadmap açık ve In Progress kalır.
- Targeted firmware authority `3/3`, scene contract `1/1`, firmware input/P0 `5/5`, power/POST regression class `10/10`, full EditMode `784/784`, full PlayMode `169/169`; failed, skipped ve inconclusive `0`.
- Universal macOS Development/StrictMode build `330,573,681` bayt ve `302` dosyadır. Executable `117,179` bayt, SHA-256 `2d55d534a6b692f2594c7135cb4b13b4fabc6085165e27d244187f8881700a1f`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 native r63 smoke exact readiness ve firmware success markerlarını birer kez üretir; exit `0`, Input System shutdown tamam, failure/fatal marker `0`, player/Unity/shader residue `0`dır.
- Repository Guard run `33367768909` source commit üzerinde geçti. Local Repository Guard ve `git diff --check` de geçer.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. Ayrı user/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.

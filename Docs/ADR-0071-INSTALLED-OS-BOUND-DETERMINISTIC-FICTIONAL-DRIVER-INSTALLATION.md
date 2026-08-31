# ADR-0071 — Installed-OS-Bound Deterministic Fictional Driver Installation

**Status:** Mac technical source `b144a3e`, tree `271bf53` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #134 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #133; Issue #131 exact installed fictional OS receipt'i üzerine kuruludur

## Context

Issue #131, exact current UEFI → POST → power-on → preflight lineage'ından üretilen fictional OS sonucunu exact fiziksel M.2 item/product kimliği üzerinde kalıcılaştırdı. Steam 1.0 fiziksel PC servis zincirindeki sıradaki bounded adım, bu installed OS için oyuncunun mevcut elektrik test istasyonunda açıkça fictional driver bundle incelemesi ve kurulumu yürütmesidir.

Bu dilim gerçek üretici driver'ı, internet/download, installer executable, kernel module, device enumeration, reboot, update, rollback, signing, compatibility database, benchmark sonucu veya Steam packaging üretmez. Ürün sözleşmesi yalnız `WorkshopDriverBundle / InstalledForBenchmarkStage` sonucu ve onun immutable provenance'ıdır.

## Decision

- Ayrı `PcFictionalDriverInstallationAuthority`, exact `PcFictionalOsInstallationAuthority`, onun exact `PcPowerStateAuthority` ve exact `AssemblyBuildAuthority` pair'ına bağlanır. OS, power, POST, firmware, Assembly veya benchmark ledger'ının sahipliğini üstlenmez.
- Completion non-empty stable operation ID, exact current installed `PcFictionalOsInstallationReceipt`, exact current-cycle `PcFirmwareBaselineReceipt`, expected current power-state revision, expected driver revision ve exact current secured M.2 item ister.
- Receipt exact OS receipt'ini; current completion firmware → POST → power-on → preflight lineage'ını; storage item/product kimliğini; completion storage-secure operation/full Assembly revision'ını ve monotonik driver revision'ını immutable taşır.
- Completion anında immutable OS/firmware source lineage'ı, current electrical-readiness snapshot'ındaki bütün component item kimlikleri, retain/secure operation kimlikleri, ATX24/EPS12V/PCIe-GPU item/route/revision değerleri ve catalog product kimlikleri fail-closed doğrulanır. Completion firmware snapshot'ının storage secure operation ve full Assembly revision'ı current snapshot ile exact eşleşir.
- Null, foreign, stale, historical, Off-cycle, owner-mismatch, hardware/product/retain/cable drift, malformed history veya missing/unsecured storage fail-closed olur. Exact command replay same-instance; changed reuse `OperationConflict`; aynı OS/storage için ikinci distinct completion `AlreadyCompleted`dır.
- Kurulum tamamlandıktan sonra sonuç exact current OS + storage item/product'a aittir. Aynı OS/storage current kaldığı sürece non-storage donanımın daha sonra değişmesi installed driver sonucunu gizlemez. Storage veya OS değişirse current evaluation `NotCurrent`/`NotInstalled` olur; historical receipt immutable kalır.
- Existing station, focus anchor, range, LOS, hands/ownership gates ve Input System actions tekrar kullanılır. İlk `LMB / RT` driver review açar, ikinci `LMB / RT` completion yürütür. Yeni scene geometry, collider, cursor modalı veya input action eklenmez.
- Açık driver review; motor pause, raw Pause edge, range/focus/LOS kaybı, busy hands, competing world-interact owner, current firmware/OS değişimi veya already-installed state oluştuğunda Primary edge'i tüketmeden sıfırlanır. Oyuncu döndüğünde fresh review gerekir.
- `E / A` explicit power-off strict priority taşır. Same-frame Interact + Primary Action yalnız power-off yürütür ve driver receipt üretmez. Malformed driver history normal power-off yolunu softlock edemez.
- Workbench presentation-only `Waiting / Reviewing / Installed / Rejected` durumlarını ve rejected failure code'unu ayırır. `TryGetFictionalDriverInstallation` ile yalnız mevcut authority'yi okur; `Ensure` çağırmaz ve gameplay state üretmez.
- Inventory, BuildKit, reservation, custody, Economy, Assembly, cable, power transition, POST, firmware, OS ve benchmark authority'leri driver completion tarafından değiştirilmez. `EvaluateBenchmarkReadiness()` bu dilimde `BuildIncomplete` kalır.
- GarageGraybox r65 native smoke nested r64 OS prerequisite'ini tek üst sonuç altında çalıştırır; alt success marker'ını bastırır, alt failure'ı üst smoke'a taşır ve keyboard + mouse + virtual gamepad review/install/power-off zincirini doğrular.

## Consequences

GarageGraybox `garage-fictional-driver-installation-r65-v1`, exact installed OS ve current firmware cycle sonrasında bounded fictional driver review/install akışı sunar. Kurulumdan önce hardware/cable drift fail-closed'dur; kurulumdan sonra aynı OS/storage üzerindeki sonuç kalıcıdır. Workbench artık waiting ile gerçek review/rejection durumlarını birbirine karıştırmaz.

Gerçek driver ekosistemi veya benchmark sonucu oluşturulmamıştır. Sıradaki bounded ürün dilimi immutable driver receipt'ini tüketen ayrı benchmark/stres/ısı/kalite authority ve görünür player workflow'udur; real driver media/update/reboot ve release packaging kapsam dışı kalır.

## Current verification

- Technical source `b144a3ef1a0ac5fcbd9704c850426baa9a727044`, tree `271bf53012e44e5162cdc5bdd2f41fa2cbbd3052`; draft PR #134 clean/mergeable'dır. Issue/Roadmap açık ve In Progress kalır.
- Targeted driver domain `5/5`, scene `1/1`, driver input/context/P0 `6/6`, power→POST→UEFI→OS→driver regression `23/23`, full EditMode `793/793`, full PlayMode `181/181`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `330,641,904` bayt ve `302` dosyadır. Executable `117,179` bayt, SHA-256 `c347fd358af6c1afe8e5d89699995ebaf81a4e9c65b4ff0cc9ac3a9f79ad2ad7`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 graphical r65 native smoke readiness/success markerlarını birer kez üretir; failure/fatal `0`, Input System graceful shutdown, exit `0`, player/Unity residue `0`dır.
- Bounded final static audit production/test tarafında P0/P1 bulmadı. Local Repository Guard ve run `33378476265` geçer; `git diff --check` temizdir.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.

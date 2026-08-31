# ADR-0068 — Active-Power-On-Bound Deterministic Baseline POST Self-Test

**Status:** Mac technical source `30ca892`, tree `eaf8735` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #128 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #127; Issue #125 exact safe power-state authority ve maintenance interlock üzerine kuruludur

## Context

Issue #125, exact current preflight receipt'i ve canonical Assembly instance'ına bağlı deterministic `Off → Energized → Off` döngüsünü kurdu. Energized state yalnız güvenli bakım kilidini ve immutable power transition history'yi kanıtlıyordu; presentation bilinçli olarak `POST BEKLİYOR` diyordu. Bir sonraki bounded ürün adımı, oyuncunun kabul edilmiş power-on cycle'ı için açık ve replay-safe bir baseline startup self-test receipt'i üretmelidir.

Bu receipt gerçek donanım POST code'u, pin/rail ölçümü, electrical fault, BIOS/UEFI, firmware, display output, işletim sistemi, driver, benchmark veya termal kararlılık değildir. Böyle bir kapsamı tek boolean ya da presentation text ile sahte biçimde tamamlamak yerine, yalnız exact active power-on lineage'ını kanıtlayan küçük ve immutable bir domain kaydı gerekir.

## Decision

- Baseline startup self-test, ikinci bir gameplay authority olarak oluşturulmaz. `PcPowerStateAuthority`, kendi power transition ledger'ından ayrı bir `PcPostStartupReceipt` ledger'ı ve bağımsız monotonik POST revision'ı taşır.
- `TryCompleteStartupSelfTest(...)`; non-empty stable operation ID, current exact power-state revision, aynı authority'nin exact active `PowerOn` receipt instance'ı ve onun current preflight/interlock lineage'ını ister. Null, foreign, stale, off veya mismatched command fail-closed olur.
- Receipt; owner identity, stable POST operation ID, exact source power-on receipt, exact preflight receipt, expected/current power-state revision ve kendi monotonik POST revision'ını immutable taşır. Sonuç bu bounded dilimde yalnız `Passed`dır.
- Exact same command replay aynı receipt instance'ını döndürür. Aynı operation ID farklı command ile `OperationConflict`; aynı active power cycle için farklı ikinci operation `AlreadyCompleted` olur. Bir power-on receipt için receipt history'de en fazla bir POST kaydı bulunabilir.
- Power-off active POST pointer'ını temizler fakat historical receipt ve operation replay'i korur. Off durumda ya da eski cycle için current evaluation `NotCurrent` olur. Yeni power cycle ayrı operation ID ve ayrı monotonik POST revision'ı ister.
- Domain seviyesinde `TryPowerOn(...)` ile `TryCompleteStartupSelfTest(...)` iki açık komuttur. POST'suz power cycle invariant açısından geçerli kalabilir. Existing player station path'i ise accepted player-triggered power-on'dan hemen sonra aynı consumed Interact command içinde POST completion çağrısını yürütür.
- POST completion player path'inde başarısız olsa bile accepted Energized transition geri alınmaz veya gizlenmez. Bir sonraki normal Interact her zaman explicit power-off branch'ine ulaşır; energized softlock yaratılmaz.
- Existing station/workbench yüzeyi reused. Başarılı player path'i `GÜCÜ KAPAT • POST GEÇTİ`, `GÜÇ AÇIK • POST GEÇTİ` ve `FIRMWARE BEKLİYOR • BAKIM KİLİDİ AKTİF` gösterir. Read-only presentation evaluation authority veya receipt üretmez.
- GarageGraybox r62 native smoke exact active receipt, same-instance replay, power-off sonrası active-clear/history-preserve, keyboard+gamepad input, energized maintenance block, untouched benchmark ve full invariant zincirini doğrular.
- BIOS/UEFI, firmware state machine, gerçek POST fault/code modeli, display output, OS, driver, benchmark, thermals, damage, packaging, delivery ve settlement bu ADR'nin dışındadır. Existing benchmark readiness `BuildIncomplete` kalır.

## Consequences

GarageGraybox `garage-post-startup-r62-v1`, player-triggered safe power-on sonrasında exact cycle'a bağlı immutable baseline POST kanıtı üretir. Power-state ve POST revisions birbirinden ayrıdır; replay ve historical audit korunurken active state yalnız mevcut energized cycle'ı temsil eder.

Sunum artık baseline self-test'in geçtiğini açıkça gösterir fakat firmware/UEFI kapısını tamamlanmış saymaz. Power-off her zaman erişilebilir kalır ve bütün Issue #125 maintenance interlock kuralları değişmeden sürer. Yeni collider, renderer, light, camera, NavMesh obstacle, physical item, Assembly authority veya benchmark mutation'ı eklenmez.

## Current verification

- Technical source `30ca892c4c3411b8771c10a39856089ecc5cd3f1`, tree `eaf87358b42f96beb4f5b62d2bf65af78484d03b`; draft PR #128 open/mergeable'dır. Issue/Roadmap açık ve In Progress kalır.
- Final-source targeted EditMode `3/3`, full EditMode `781/781`, full PlayMode `164/164`; failed, skipped ve inconclusive `0`.
- Universal macOS Development/StrictMode build `330,548,985` bayt ve `302` dosyadır. Executable `117,179` bayt, SHA-256 `4e1ebbba08867a7fa592d7b6b1868747ab4bc74210f86247e2446c80de86a87e`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 native r62 smoke exact readiness ve `post=passed benchmark=untouched invariants=ok` success markerlarını birer kez üretir; exit `0`, Input System shutdown tamam, failure/fatal marker `0`, player/Unity/shader residue `0`dır.
- Repository Guard run `33364272612` source commit üzerinde geçti. Local Repository Guard ve `git diff --check` de geçer.
- `ProjectSettings/ProjectSettings.asset` build öncesi/sonrası SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. Ayrı user/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.

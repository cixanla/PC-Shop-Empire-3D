# ADR-0031 — Authoritative Single-Motherboard Seating

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi; kaynak, test, macOS build ve native runtime kapıları tamamlandı<br>
**Bağlam:** Epic #10 / Issue #53 — ilk bounded fiziksel PC assembly dikey dilimi

## Bağlam

Garaj prototipi teslimat, stok, raf, müşteri ve fiziksel checkout zincirini taşıyordu; ancak PC toplama işi henüz görünür bir world etkileşimi değildi. İlk assembly adımı bütün PC yapım sistemini açmadan, tek serialized anakartın tek açık kasadaki doğru slota güvenle oturtulup yeniden sökülebilmesini kanıtlamalıydı. Mevcut Catalog ve Inventory tek doğruluk kaynağı kalmalı; Unity transformu, collider veya presentation state'i ikinci authority olmamalıydı.

## Karar

- Unity'den bağımsız `PSE.Assembly` sınırı yalnız `PSE.Core`, `PSE.Catalog` ve `PSE.Inventory`e bağımlıdır. Build, chassis, slot ve operation kimlikleri stable ve canonical'dır.
- Anakart, mevcut Catalog'daki `Motherboard` türü ve form-factor specification'ıyla doğrulanır. Bu dilimde tek açık kasa yalnız `MicroAtx` motherboard slotu kabul eder; uyumsuz tür/form factor no-mutation kapanır.
- `AssemblyBuildAuthority`, mevcut Inventory içindeki Workbench container'ını managed custody olarak claim eder. İkinci authority aynı Workbench'i claim edemez; dolu Workbench `slot-occupied`, foreign managed claim `plan-foreign`, revision max ise canonical `revision-overflow` üretir.
- Attach, aynı serialized item'ı `ActorHands → Workbench` taşır ve tam bir Assembly revision ile immutable `SeatedUnsecured` receipt üretir. Detach yalnız aynı unsecured item/slot için, boş kapasitesi olan ActorHands'e ters transfer yapar. Exact operation replay idempotent; payload conflict fail-closed'dur.
- Preview veya collider hiçbir zaman domain sonucu uydurmaz. `MotherboardSeatSolver` range, focus, raycast LOS, pause, keyed rotation, support ve clear-volume kapılarını deterministic değerlendirir. Valid preview pozu ile committed pose birebirdir.
- Seat sonucu varsayılan/reset durumda `Uninitialized` ve geçersizdir. Readiness yalnız aktif/enabled focus ve support collider'larıyla true olur; eksik veya devre dışı fiziksel bağ fail-closed'dur.
- World projection tek canonical fiziksel anakarttır. Seated durumda kinematic ve generic pickup/cart/stack/box-placement yollarına kapalıdır. Pickup, attach, detach, world drop ve recovery sırasında authoritative Inventory/Assembly transaction sırası fiziksel mutasyondan önce gelir; hata halinde held/seat/last-safe-pose korunur.
- `Mouse Left / Gamepad RT` guided seat mode'unu açıp kapatır; `G / Gamepad East` yalnız ayrı, yeni bir edge ile confirm eder. Aynı frame Primary+Drop co-edge'inde Primary tek geçiş sahibidir; Drop consume edilip attach veya world drop üretmez.
- Kontrol ipuçları son kullanılan cihaz ailesine göre dinamiktir: klavye/fare `E/G/R/LMB`, gamepad `A/B/RB/RT`. Geçerli/geçersiz preview şekil, kısa metin ve kontrollü renk birlikteliğiyle gösterilir.
- Native development smoke exact `garage-motherboard-seating-r22-v1` readiness ve `GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE` sözleşmesini taşır. Gerçek Input System co-edge, release–repress ve failed-drop retry davranışları PlayMode testleriyle ayrıca kanıtlanır.

## Sonuçlar

- Oyuncu GarageGraybox'ta hassas anakartı fiziksel olarak alabilir, kasaya hizalayabilir, güvenli koşullarda oturtabilir, tekrar sökebilir veya dolu world-floor gibi başarısız bırakma durumundan aynı kimlikle devam edebilir.
- Assembly, Inventory ve fiziksel projection aynı item kimliğinde kalır; duplicate, jitter, ghost custody ve presentation-only başarı üretilemez.
- `SeatedUnsecured` bilinçli ara durumdur. Benchmark readiness hâlâ `assembly.benchmark.motherboard-unsecured` ile kapalıdır; bu dilim tam PC build veya başarı skoru değildir.

## Bilinçli kapsam dışı

- Standoff/vida/fastener, tornavida, torque ve secured motherboard durumu.
- CPU/socket, RAM, GPU, storage, PSU, cooler, thermal paste, cable routing, POST/BIOS/OS/driver ve benchmark score.
- Çoklu kasa/slot/component, custom-PC order/deposit/kit reservation authority ve Inventory #7/#8 genişlemesi.
- Save/journal/Guardian, damage/ESD, final model/texture/animation/audio/UI ve Windows/DirectX/Steam/IL2CPP doğrulaması.

## Yerel doğrulama

- EditMode `394/394`; PlayMode `26/26`; failed/skipped `0`.
- Universal macOS Development Player; Mach-O `x86_64 + arm64`.
- Apple M4/Metal, 1280×720 native runtime readiness ve exact assembly smoke başarılı.
- Ayrıntılı hash, build boyutu, commit, CI ve USB durumu tarihli Issue #53 evidence/checkpoint belgesinde tutulur.

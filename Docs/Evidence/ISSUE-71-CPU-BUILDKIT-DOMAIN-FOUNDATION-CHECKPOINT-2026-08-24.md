# Issue #71 — CPU BuildKit Domain Foundation Ara Checkpoint'i

**Tarih:** 24 Ağustos 2026<br>
**Durum:** Testli ve devam edilebilir domain temeli; Issue #71 açık/In Progress<br>
**Branch:** `feature/issue71-cpu-build-kit-handoff`<br>
**Kaynak commit:** `28da056a703556768c533e51dfcc4220cbed1834`<br>
**Kaynak tree:** `00be75d089848c5244fb843cd6a67e26af8c1bd5`

## Bu checkpoint'te tamamlanan bounded dilim

- Accepted custom-PC iş emrindeki canonical CPU yalnız `ComponentKind == Processor` ile tekil çözülür; exact `LineId/ProductId/ItemId/ReservationId` ve owned allocation/receipt zinciri doğrulanır.
- CPU için motherboard slotundan ayrı stable operation ID ve ayrı managed capacity-one BuildKit container eklenmiştir.
- BuildKit authority kayıt anahtarı `BuildOrderId + ComponentKind` olarak genişletilmiştir. Motherboard'un mevcut operation/receipt/replay davranışı geriye uyumlu dört-parametreli factory ile korunur.
- Inventory'nin dar work-order BuildKit köprüsü yalnız `Motherboard` ve `Processor` rollerini kabul eder; generic raw transfer yetkisi genişletilmez.
- CPU pickup, canonical motherboard aynı work order için staged olmadan `BuildKitPrerequisiteMissing` ile sıfır mutasyonlu reddedilir.
- Başarılı CPU akışı exact `WorldFloor → ActorHands → dedicated CPU BuildKit` custody'sini ve canlı reservation/allocation identity'sini korur. ProcessorSocket/Assembly authority çağrılmaz ve Assembly revision değişmez.
- Exact pickup/place replay aynı receipt nesnesini revision artırmadan döndürür. Stale BuildKit veya Inventory revision placement öncesinde fail-closed kalır.
- `GarageStockFlowSession`, CPU BuildKit pickup ve placement için domain-first API yüzeyini sunar; oyuncu sahnesi ve input consumer'ı bu ara checkpoint'in dışında bırakılmıştır.

## Doğrulama

Unity sürümü: `6000.3.21f1`

| Kapı | Sonuç | Dosya | Bayt | SHA-256 |
|---|---:|---|---:|---|
| CPU BuildKit authority hedefli EditMode | `15/15`, failed/skipped/inconclusive `0` | `editmode-issue71-domain-targeted.xml` | 15.061 | `d423cf3fa9b9003794ab9936235b4da4c89c6ab637e9aeace5a0b670221feea8` |
| Hedefli Unity log | başarılı | `editmode-issue71-domain-targeted.log` | 39.355 | `670d847a3a4d57cebb077ca905a9590a599dc3a981be1b37b7bb8717e10afb56` |
| Tam EditMode regresyonu | `677/677`, failed/skipped/inconclusive `0` | `editmode-issue71-domain-full.xml` | 565.185 | `88774a5d0d7e9b7ddcf2caf118a1e7ab7b1186f3444ad57f2bbc09cfb6ec2cab` |
| Tam Unity log | başarılı | `editmode-issue71-domain-full.log` | 33.816 | `44ad2ada713affd189b4c64ca20bd42e74982c88ec697bf6831ea86c17dd8f97` |
| Repository whitespace | `git diff --check` başarılı | — | — | — |

Yeni iki test aşağıdaki sınırları doğrudan kilitler:

1. Motherboard staged olmadan CPU pickup no-mutation reddi; ardından exact motherboard→CPU sıra, iki ayrı BuildKit slotu, stable CPU kimliği ve Assembly izolasyonu.
2. CPU placement stale-revision reddi; başarılı placement sonrası exact pickup/place replay ve revision no-mutation.

## Bu checkpoint'in iddia etmediği kapılar

- CPU için authored scene support collider/snap anchor/preview veya raycast focus hedefi yoktur.
- Gerçek `E / Gamepad South`, rotate/drop/primary single-consumer ve pause/co-edge PlayMode matrisi henüz eklenmemiştir.
- Work-ticket görünümünde authoritative `1/10 → 2/10` aggregate projection henüz bağlanmamıştır.
- PlayMode, Universal macOS build/runtime smoke, exact-head Windows IL2CPP/D3D11 native gate ve Repository Guard sonucu bu ara checkpoint için henüz iddia edilmez.
- Issue #71 acceptance tamamlanmış sayılmaz; Issue ve Roadmap `In Progress` kalır.

## Yarın için tek devam noktası

`28da056a` domain temelinden ilerleyerek CPU'nun authored physical BuildKit hedefini, domain-success-sonrası presentation/recovery bağını, work-ticket `2/10` projection'ını ve gerçek input matrisi ekle. Ardından full EditMode + PlayMode, Mac/Windows native kapıları, final docs/CI ve fiziksel USB milestone yaşam döngüsünü ayrı kanıtlarla tamamla.

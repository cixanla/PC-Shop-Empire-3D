# ADR-0017 — Atomik Purchase Order ve Receiving Kabulü

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 15 Ağustos 2026<br>
**Bağlı işler:** Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8), Issue [#39](https://github.com/cixanla/PC-Shop-Empire-3D/issues/39)

## Bağlam

Catalog ve Inventory authority hazır olsa da ürünün ne zaman “stok” olduğu tanımlı değildi. Dashboard siparişi verildiği anda ürün eklemek fiziksel teslimatı anlamsızlaştırır; manifest satırlarını tek tek Inventory'ye yazmak ise son satırdaki hata halinde kısmi stok ve çoğalma/kayıp riski yaratır.

## Karar

- `PSE.Orders`, yalnız `PSE.Core`, `PSE.Catalog` ve `PSE.Inventory` referanslı, Unity bağımsız assembly olur.
- İlk purchase order stable `PurchaseOrderId`, `SupplierId`, pozitif ve duplicate olmayan ürün/adet satırları ile `Placed` durumunda oluşur.
- Tedarikçi onayı stable `DeliveryId` ve monotonik ETA aralığı atar. Durum sırası `Placed → Confirmed → InTransit → Arrived → Accepted` dışına çıkamaz.
- Fiziksel arrival manifesti serialized ürünlerde benzersiz `ItemInstanceId`, batch ürünlerde benzersiz `BatchId` + pozitif quantity taşır. Manifest product/tracking/adet toplamı purchase order ile birebir eşleşmeden `Arrived` kaydı oluşmaz.
- `InventoryIntake`, bütün serialized ve batch satırlarını deterministic sıralı immutable istek olarak taşır. `ReceiveIntake`, ürün politikası, duplicate identity, kondisyon, hedef container ve toplam kapasiteyi tamamen preflight eder; sonra bütün satırları tek Inventory revision'ında yazar.
- Kabul yalnız `Receiving` türündeki container'a yapılır. Inventory intake başarısızsa order `Arrived` kalır ve iki authority revision'ı da değişmez.
- Intake başarıyla döndükten sonra order geçişinde yeni failure kapısı yoktur; tek simulation thread'i içinde `Accepted` durumu yazılır. Kalıcı disk düzeyinde crash atomikliği Save/journal paketinin sorumluluğudur.
- Duplicate acceptance geçersiz state transition olur ve stok çoğaltmaz.

## Sonuçlar

Sipariş, onay veya yolda olma stok yaratmaz. Yalnız sayılmış, tam eşleşen ve fiziksel olarak gelmiş manifest açık kabul komutuyla receiving stoğuna dönüşür. İlk dilim yalnız eksiksiz/yeni ürün kabulünü kapsar; kısmi/hasarlı/şartlı kabul, claim, para/ledger, tedarikçi ekonomisi, Dashboard ve dünya spawn/raf projeksiyonu sonraki Issue #8 alt işlerindedir.

## Doğrulama

- `PSE.Orders` assembly sınırı ve Unity/Editor bağımsızlığı.
- Lifecycle, timestamp, duplicate order/delivery, exact manifest, tracking, capacity, receiving-kind ve duplicate acceptance testleri.
- Kısmi/ekstra/yanlış delivery veya önceden var olan stok kimliğinde order ve Inventory no-mutation kanıtı.
- EditMode `184/184`, regresyon PlayMode `14/14`.
- Kanıt: `Docs/Evidence/ORDERS-RECEIVING-CHECKPOINT-2026-08-15.md`.

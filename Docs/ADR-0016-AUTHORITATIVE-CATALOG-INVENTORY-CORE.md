# ADR-0016 — Authoritative Catalog ve Inventory Çekirdeği

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 15 Ağustos 2026<br>
**Bağlı işler:** Epic [#7](https://github.com/cixanla/PC-Shop-Empire-3D/issues/7), Issue [#38](https://github.com/cixanla/PC-Shop-Empire-3D/issues/38)

## Bağlam

Fiziksel kutu, istif ve taşıma arabası akışı dünyadaki nesne güvenliğini kanıtladı; ancak ekonomik stok için tek gerçek kaynağı oluşturmadı. Sipariş, teslimat, raf, satış ve save zinciri kurulmadan önce ürün tanımı ile sahip olunan stok birbirinden ayrılmalı; duplicate kimlik, negatif miktar, kapasite aşımı ve aynı ürünün iki kez ayrılması hiçbir state değişikliğine yol açmadan reddedilmelidir.

## Karar

- `PSE.Catalog`, yalnız `PSE.Core` referanslı ve `noEngineReferences` bir saf C# assembly'sidir. Ürün tanımı stable ürün/kategori kimliği, oyuncuya görünen doğrulanmış ad, açık serialized/batch takip politikası ve `0–3650` gün garanti sınırı taşır.
- `ProductCatalog` boş, null veya duplicate tanımı fail-closed reddeder; tanımları stable kimliğe göre ordinal sıralar ve oluşturulduktan sonra değiştirilemez.
- `PSE.Inventory`, yalnız `PSE.Core` + `PSE.Catalog` referanslı authoritative mantıksal stok assembly'sidir.
- Serialized ürün tek `ItemInstanceId`; fungible ürün tek `BatchId` ve pozitif quantity ile izlenir. Tracking policy karıştırılamaz.
- Her stok birimi kayıtlı tek container pozisyonundadır. İlk kapasite sözleşmesi fiziksel hacim değil, pozitif integer `unitCapacity` değeridir; hacim/planogram daha sonraki raf paketine bırakılır.
- Bir batch birden fazla container pozisyonuna bölünebilir fakat aynı `BatchId` korunur. Transfer, kaynak/target önkontrollerinin tamamı geçmeden state değiştirmez.
- Serialized rezervasyon item üzerinde exclusive'dir ve item transfer edilirse item ile birlikte mantıksal olarak devam eder. Batch rezervasyonu belirli batch+container pozisyonunda available quantity ile bounded'dır.
- Stok yalnız açık reservation consume komutuyla authoritative kayıttan çıkar. Release miktarı değiştirmez. Başarılı her mutation tek revision ilerletir; failure revision ve state'i değiştirmez.
- Toplam, kullanılabilir, container, batch-position ve sıralı snapshot sorguları ile bütün state'i yeniden denetleyen `ValidateInvariants` kapısı sağlanır.

## Sonuçlar

Sipariş, satış ve fiziksel raf projeksiyonlarının bağlanabileceği tek mantıksal stok gerçeği hazırdır. Unity sahne nesneleri, mevcut küçük/büyük kutular ve taşıma arabası hâlâ yalnız dünya projeksiyonudur; Issue #8 adaptörü kurulmadan Inventory'yi otomatik değiştirmez. Acquisition cost, fiyat/para, Orders, event publication, persistence/save, hacim ve planogram bu paketin parçası değildir.

## Doğrulama

- Assembly sınırı: Catalog yalnız Core; Inventory yalnız Core + Catalog; Unity/Editor referansı yok.
- Saf EditMode: ürün/katalog doğrulama, serialized/batch receipt, tracking ayrımı, mixed capacity, atomik transfer, batch conservation, exclusive/bounded reservation, release/consume, no-mutation revision ve invariant audit.
- Regresyon PlayMode: önceki gerçek Input System fiziksel etkileşim zincirinin tamamı değişmeden geçer.
- Kanıt: `Docs/Evidence/CATALOG-INVENTORY-CHECKPOINT-2026-08-15.md`.

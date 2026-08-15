# ADR-0023 — Atomic Checkout Fulfillment

**Tarih:** 15 Ağustos 2026
**Durum:** Kabul edildi ve Issue #45 ile uygulandı
**Bağlam:** Epic #8 — immutable checkout snapshot'ının authoritative stok fulfillment sınırı

## Bağlam

Issue #44 sonunda checkout fiyatı ve exact basket satırları immutable olarak donduruluyordu; Inventory reservation, serialized item ve basket satırı ise aktif kalıyordu. Bu üç authority'yi satır satır tüketmek, çok satırlı satışta ilk ürünün silinip sonraki satırın başarısız olması gibi kısmi mutation riski yaratırdı. Ayrıca tamamlanan işlemin stable, sorgulanabilir bir sonucu yoktu.

## Karar

- `InventoryAuthority.ConsumeReservations`, null/boş/duplicate/unknown setleri ve bütün serialized/batch hedeflerini mutation öncesi doğrular; başarıda exact reservation setini tek Inventory revision'ında tüketir.
- `RetailBasketAuthority`, immutable checkout satırlarının güncel basket line, item, reservation ve claim bağlarını eksiksiz preflight eder. Inventory bulk consume başarılı olduktan sonra önceden doğrulanmış basket satırlarını kaldırır ve Basket revision'ını tam bir kez ilerletir.
- `RetailCheckoutCompletionIdScope` ve immutable `RetailCheckoutCompletionRecord`; completion, checkout, basket, customer, simulation timestamp, currency, total ve exact line snapshot bağını korur.
- `RetailCheckoutAuthority.CompleteCheckout`, aktif checkout/basket/item/reservation/offer bağını yeniden doğrular. Başarı Inventory, Basket ve Checkout revision'larını birer kez ilerletir; ShelfOffer ve Orders değişmez.
- Exact aynı completion tekrarı ve tamamlanmış checkout için exact begin tekrarı idempotenttir. Başka checkout'a reused completion ID, aynı checkout için ikinci completion, erken timestamp ve cross-authority drift hiçbir authority'yi kısmen değiştirmez.
- Completion sonrasında immutable checkout snapshot'ı ve güncel/farklı raf fiyatı birlikte geçerli kalır; sold item, reservation ve basket line artık bulunmamalıdır.
- Garajda ilk `Mouse Left / Gamepad RT` checkout fiyatını dondurur; ikinci aynı binding fulfillment'ı tamamlar. Ürün projection'ı raftan kaldırılır, sepet/reservation/stok `0` olur ve shelf/HUD `549,99 EUR • TAMAMLANDI` gösterir.
- Completion kaydı fulfillment kanıtıdır; ödeme yöntemi, para tahsilatı, gelir/COGS, vergi, fiş/fatura veya garanti başlangıcı değildir.

## Sonuçlar

- Çok satırlı satış stok tüketimi failure durumunda kısmi ürün kaybı üretmez.
- Inventory, Basket ve Checkout revision sınırları açık ve test edilebilirdir.
- Dünya projection'ı yalnız başarılı authoritative commit sonrasında kaldırılır.
- Önceki pickup/drop, placement, stacking, cart, recovery, reservation release ve checkout-price invariantları korunur.
- Sıradaki ana geliştirme alanı Issue #9 altındaki bounded müşteri davranışı/kasa akışıdır; Economy settlement ve Save kendi authority paketlerinde kalır.

## Kanıt

- Feature commit: `bb89b0c297400f6eed22407df76dc1c85912cd74`
- Tree: `831b310717df32bbe2b6bb3465c8caf7323c74b8`
- EditMode: `242/242`
- PlayMode: `17/17`
- Universal macOS build ve Apple M4/Metal runtime: `checkout-completion=ready sale-completion=ok stock-consumed=ok completed-quantity=0`
- Repository Guard: [31870482690](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31870482690), başarılı
- Ayrıntı: `Docs/Evidence/ATOMIC-CHECKOUT-FULFILLMENT-CHECKPOINT-2026-08-15.md`

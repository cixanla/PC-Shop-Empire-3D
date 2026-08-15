# ADR-0022 — Immutable Checkout Price Snapshot

**Tarih:** 15 Ağustos 2026
**Durum:** Kabul edildi ve Issue #44 ile uygulandı
**Bağlam:** Epic #8 — aktif müşteri sepetinin satış başlangıcında fiyatının dondurulması

## Bağlam

Issue #43 sonunda exact serialized RAF A ürünü stable customer/basket/line kimliği ve Inventory reservation ile ayrılabiliyordu. Basket satırı bilinçli olarak fiyat taşımıyordu. Checkout başlangıcında raf teklifini yeniden okumaya devam etmek, işlem sırasında yapılan fiyat güncellemesinin müşterinin açık işlemini değiştirmesi ve birden fazla authority'nin kısmen mutasyona uğraması riskini doğuruyordu.

## Karar

- `RetailCheckoutAuthority`, Unity/Editor bağımlılığı taşımayan `PSE.Retail` içinde stable checkout transaction kimliğini yönetir.
- Begin checkout, basket'ın bütün aktif satırlarını mutation öncesinde exact basket/customer/offer/item/reservation/claim/product/shelf bağı bakımından doğrular.
- Her immutable satır snapshot'ı basket-line, offer, serialized item, Inventory reservation/claim, product ve shelf kimlikleriyle integer minor-unit fiyatı, currency'yi ve source offer revision'ı taşır.
- Transaction tek currency kullanır; total integer minor-unit olarak overflow-safe hesaplanır ve deterministic line sırasıyla saklanır.
- Başarı yalnız Checkout authority revision'ını bir kez ilerletir. Basket, Inventory, ShelfOffer ve Orders kayıtları/revision'ları değişmez.
- Exact aynı begin komutu cross-authority bağlar hâlâ tutarlıysa idempotenttir. Aynı basket için ikinci transaction, identity conflict, empty/unknown basket, mixed currency, missing/stale reservation ve drift failure yolları no-mutation kalır.
- Checkout başladıktan sonra raf offer fiyatı/revision'ı güncellenebilir; açık checkout'taki unit price, currency, total ve source revision değişmez. Stable product/shelf/reservation bağındaki drift ise invariant ihlalidir.
- Garaj kanıtında reserved RAF A ürününde `Mouse Left / Gamepad RT` checkout başlatır. Shelf etiketi, HUD ve prompt `549,99 EUR • DONDURULDU` gösterir.
- Checkout aktifken müşteri reservation release ve reserved pickup presentation katmanında fail-closed kalır; fiziksel item exact stable kimliğiyle rafta tutulur.
- Reservation consume, sold-item transition, ödeme, Economy ledger/COGS/nakit, vergi/indirim, fiş/fatura, fiziksel müşteri AI, Save ve final UI bu karara dahil değildir.

## Sonuçlar

- Açık işlem, daha sonra değişen raf fiyatından etkilenmez.
- Başarısız checkout başlangıcı hiçbir kaynak authority'yi yarım durumda bırakmaz.
- Checkout kaydı satış/ödeme sonucu değildir; yalnız doğrulanmış ve immutable işlem başlangıcıdır.
- Önceki acceptance, parcel open, world transfer, offer, basket reservation, pickup/drop, placement, stacking, cart ve recovery invariantları korunur.
- Sıradaki bounded sınır, exact checkout reservation'ını tüketip serialized item için atomik satış sonucu üretmektir; Economy ödeme/ledger ayrı authority hazır olana kadar kapsam dışıdır.

## Kanıt

- Feature commit: `294999f6ad48d4831f56031cc542cf43cac09d3e`
- EditMode: `233/233`
- PlayMode: `17/17`
- Universal macOS build ve Apple M4/Metal runtime: `checkout-snapshot=ok price-frozen=ok`
- Repository Guard: [31869105555](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31869105555), başarılı
- Ayrıntı: `Docs/Evidence/IMMUTABLE-CHECKOUT-PRICE-SNAPSHOT-CHECKPOINT-2026-08-15.md`

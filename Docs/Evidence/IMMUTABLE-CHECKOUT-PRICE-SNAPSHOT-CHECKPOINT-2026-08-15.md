# Immutable Checkout Price Snapshot Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#44](https://github.com/cixanla/PC-Shop-Empire-3D/issues/44), Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altında ilk immutable checkout başlangıç dilimini tamamlar:

1. Exact Northstar A60, acceptance → parcel open → ActorHands → RAF A → `549,99 EUR` offer → customer reservation zincirini kullanır.
2. Ayrılmış ürüne bakarken `Mouse Left / Gamepad RT`, stable checkout kimliğiyle bütün aktif basket satırlarını atomik preflight eder.
3. Snapshot exact line/offer/item/reservation/claim/product/shelf kimliklerini, `54999` minor-unit fiyatı, `EUR` currency'yi ve source offer revision `1`i immutable saklar.
4. Başarı yalnız Checkout revision'ını `0 → 1` ilerletir; Basket, Inventory, ShelfOffer ve Orders state/revision'ları sabit kalır. Exact tekrar idempotenttir.
5. Sonradan raf fiyatı `599,99 EUR` yapıldığında açık checkout hâlâ `549,99 EUR • DONDURULDU` gösterir.
6. Checkout aktifken `G / Gamepad East` reservation release ve `E / Gamepad South` pickup fail-closed kalır; item rafta ve aynı stable kimlikle durur.

Reservation consume, item'ın sold state'e geçmesi, ödeme/Economy ledger, vergi/indirim, fiş/fatura, fiziksel müşteri AI ve Save ayrı bounded paketlerdir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `294999f6ad48d4831f56031cc542cf43cac09d3e`
- Tree: `2f524430e2a3bf03ad3880ab29eb44a0b8120a25`
- Marker: `garage-checkout-snapshot-r13-v1`
- Repository Guard: [31869105555](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31869105555), başarılı

## Otomatik doğrulama

Ham kanıtlar `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altındadır.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r13 görünür checkout shelf/HUD/prompt sözleşmesi | `6764c560b793598eb3d89bf2016028d4b5f9cf2aaa0a707c62ddc1490b489aac` |
| `editmode-checkout44-integration.xml` | 233/233 geçti; failed/skipped 0 | `a8df3068a0680a494b8e957e28d4708c18b814686940549491ad50c3a51ac618` |
| `playmode-checkout44.xml` | 17/17 geçti; failed/skipped 0 | `b6b6a478efd599dcf1679277a233195a453b0457550fb37d68d7a4cc9586a679` |
| `build-checkout44-macos.log` | Universal development build; 327.551.161 bayt | `e83dfc95e56b3e8ca527b809671fdb2360b4ce2aa6d3454d0658a7b65bd568c6` |
| Player executable | Mach-O `x86_64 + arm64` | `1efa5d0dfe88e74fcae15570cb07c51c0d132baf28b30b15476721f2a7dddbdf` |
| `runtime-checkout44-macos-windowed.log` | Apple M4/Metal, 1280×720; checkout snapshot/fiyat dondurma başarılı | `0ada440a9c02746921664f30e8cfd8f05f1d0ec7ac8024816317848ce3db86ff` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-checkout-snapshot-r13-v1 inventory-flow=arrived parcel=sealed shelf-offer=ready basket-reservation=ready checkout-snapshot=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok world-floor=ok shelf-offer=ok price-minor=54999 currency=EUR basket-reservation=ok release=ok checkout-snapshot=ok price-frozen=ok stable=ok quantity=1
```

Gerçek Input System PlayMode kapsamı:

- Klavye/fare: bütün fiziksel stok akışı → `G` reserve → Mouse Left checkout → dondurulmuş label/prompt → checkout aktif `G` release engeli.
- Gamepad: South acceptance/open/pickup/publish; East placement/reserve/release; ikinci reserve sonrası RT checkout ve aktif East release engeli.
- Domain testleri deterministic line order, tek currency/total, exact repeat, duplicate basket/transaction, identity conflict, unknown/empty basket, mixed currency, stale reservation, item/shelf drift ve price-update immutability yollarını kapsar.

macOS oturumu kilitli olduğu için yeni pencere ekran görüntüsü alınmadı; sahne sözleşmesi, gerçek Input System testleri, Universal build ve native runtime logu başarılıdır. Görsel ekran görüntüsü iddiası yapılmamıştır.

## Bilinçli kapsam dışı

- Reservation consume ve serialized item'ın authoritative sold/fulfilled transition'ı.
- Ödeme, Economy ledger/COGS/nakit, vergi, indirim, fiş/fatura ve garanti başlangıcı.
- Fiziksel müşteri karakteri, navigation/AI, gerçek sepet ve kasa animasyonu.
- Save/journal/crash atomikliği, final model/animasyon/ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature Repository Guard [31869105555](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31869105555): başarılı.
- Docs checkpoint, final Repository Guard ve USB manifest bilgileri snapshot tamamlandığında bu bölüme yazılır.

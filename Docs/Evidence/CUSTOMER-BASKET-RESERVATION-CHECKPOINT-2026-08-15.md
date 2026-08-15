# Customer Basket Reservation Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#43](https://github.com/cixanla/PC-Shop-Empire-3D/issues/43), Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altında ilk authoritative müşteri sepeti rezervasyon dilimini tamamlar:

1. Exact Northstar A60 ürünü acceptance → parcel open → ActorHands → RAF A placement → `549,99 EUR` offer publish zincirini kullanır.
2. Fiyatı yayınlanmış raftaki ürüne bakarken `G / Gamepad East`, stable demo customer/basket/line kimliğiyle exact serialized item'ı Inventory'de ayırır.
3. Başarıdan sonra RAF etiketi ve durum panosu `1 ÜRÜN • AYRILDI` gösterir; available quantity `1 → 0`, total quantity `1` kalır.
4. Ayrılmış item'a `E / Gamepad South` pickup isteği `stock-projection.customer-reserved` ile fail-closed olur; fiziksel item rafta ve aynı stable kimlikle kalır.
5. Aynı `G / Gamepad East` eylemi reservation'ı kaldırır; basket satırı ve Inventory claim birlikte çözülür, available quantity yeniden `1` olur.
6. Exact reserve tekrarı idempotenttir. Duplicate customer/basket/item, unknown/mismatched offer/item, reservation conflict ve cross-authority drift bütün failure yollarında no-mutation kalır.

Basket satırı offer fiyatını snapshot etmez. Checkout başlangıcında immutable fiyat/ürün snapshot'ı, reservation tüketimi, ödeme ve ledger ayrı bounded paketlerdir. Fiziksel müşteri AI ve gerçek sepet taşıması da bu graybox kanıtına dahil değildir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `45c2cdc4f4f437824567c7e7cb5b6fcea1ecb4ce`
- Tree: `788e9a016a692a9e558d8fb3903e32830b3a8b08`
- Marker: `garage-customer-reservation-r12-v1`
- Repository Guard: [31867913964](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31867913964), başarılı

## Otomatik doğrulama

Ham kanıtlar `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altındadır.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r12 shelf/customer etiketi ve görünür graybox sözleşmesi | `1e945021980d17d000d223afd65c29ae125fa679f13bc591d250c4bf27e4582f` |
| `editmode-issue43.xml` | 220/220 geçti; failed/skipped 0 | `e7445a284e829861cc57675d57d8404500f85b741b49198bff82467f32edce71` |
| `playmode-issue43.xml` | 17/17 geçti; failed/skipped 0 | `3f78fa76c41ec2efbfba5b0e4f26401958b3429f9a1079ab4b09f65d68d40674` |
| `build-issue43.log` | Universal development build; 327.531.969 bayt | `971c6e941d64e38a12c826b41f4f6220ebcfeedf982ffaa323e2acb5fdb1e1f0` |
| Player executable | Mach-O `x86_64 + arm64` | `98a7d104383137bc74099f214d30923d13e3bd9d05e90d922524a9d4350d1add` |
| `basket-reservation-macos-runtime.log` | Apple M4/Metal, 1280×720; reserve/release smoke başarılı | `4a98c8937d034b38a4e84e7fb2c3572e45af8c8a84ae2c573d0764087d78f6ba` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-customer-reservation-r12-v1 inventory-flow=arrived parcel=sealed shelf-offer=ready basket-reservation=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok world-floor=ok shelf-offer=ok price-minor=54999 currency=EUR basket-reservation=ok release=ok stable=ok quantity=1
```

Gerçek Input System PlayMode kapsamı:

- Klavye/fare: acceptance → open → pickup → RAF A placement → offer publish → `G` reserve → reserved item için `E` pickup engeli → `G` release.
- Gamepad: South acceptance/open/pickup/publish; East WorldFloor drop, RAF A placement, customer reserve ve release.
- Reserve başarıda yalnız Inventory ve Retail basket revision'ları birer kez ilerler; offer/Orders sabit kalır. Release aynı iki authority'yi birer kez ilerletir.
- Domain testleri stable kimlikleri, exact idempotency'yi, deterministic query'yi, duplicate/unknown/mismatch/drift failure no-mutation yollarını ve release sonrası quantity restorasyonunu kapsar.

macOS oturumu kilitli olduğu için yeni pencere ekran görüntüsü alınmadı; sahne sözleşmesi, gerçek Input System testleri, Universal build ve native runtime logu başarılıdır. Görsel ekran görüntüsü iddiası yapılmamıştır.

## Bilinçli kapsam dışı

- Fiziksel müşteri karakteri, nav/AI, gerçek sepet modeli ve raftan sepete animasyonlu transfer.
- Checkout transaction, immutable price snapshot, reservation consume ve satış sonucu.
- Ödeme, vergi, indirim, iade, Economy ledger ve fiş/fatura.
- Sayısal fiyat düzenleme UI'si, Save/journal/crash atomikliği ve final model/animasyon/ses.
- Gerçek Windows x64 IL2CPP/DirectX/Steam doğrulaması.

## Uzak ve USB kapanışı

- Feature Repository Guard [31867913964](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31867913964): başarılı.
- Docs/USB checkpoint commit ve final Repository Guard, doğrulanmış USB snapshot üretildikten sonra bu bölüme eklenecektir.

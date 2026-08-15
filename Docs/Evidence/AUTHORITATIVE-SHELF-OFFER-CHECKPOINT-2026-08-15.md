# Authoritative Shelf Offer Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#42](https://github.com/cixanla/PC-Shop-Empire-3D/issues/42), Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altında ilk authoritative raf fiyatı dilimini tamamlar:

1. Exact Northstar A60 ürünü mevcut acceptance → parcel open → ActorHands zincirinden RAF A'ya fiziksel olarak yerleştirilir.
2. RAF A etiketi teklif yokken `FİYAT YOK` gösterir; stok konumu fiyat authority'si değildir.
3. Raf ürününe bakarken `E / Gamepad South`, etkin binding ile `549,99 EUR` teklifini kasıtlı yayınlar.
4. Başarılı komut aynı stable offer/product/shelf kimliğini kaydeder ve dünya etiketi `RAF A / 549,99 EUR` olur.
5. Publish Inventory quantity/revision veya Orders revision değiştirmez; repeated exact komut idempotenttir.
6. Geçersiz para/fiyat, bilinmeyen/non-shelf container, identity conflict ve duplicate shelf+product bütün state'i no-mutation bırakır.

Bu dilim sayısal fiyat düzenleme arayüzü, müşteri, sepet, checkout, vergi/indirim, ledger veya dinamik piyasa eklemez. `PSE.Retail.SetOffer` sonraki düzenleme ve satış snapshot'ı için bounded domain girişidir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `7a23cd92be6ff1169ff49530319b0759965cadf5`
- Tree: `623c2f52839847c098162371bb6f7c1073f4852d`
- Marker: `garage-shelf-offer-r11-v1`
- Repository Guard: [31866681324](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31866681324), başarılı

## Otomatik doğrulama

Ham kanıtlar `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altındadır.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `shelf-offer-scene-build.log` | GarageGraybox r11 builder derleme/üretim kapısı geçti | `9257a9dc11c9425c0441fb5a0b6c0ed81b4b445ca92a3abab9cc51b7dd15bbfc` |
| `shelf-offer-editmode.xml` | 207/207 geçti; failed/skipped 0 | `6b00fcd3ff3e12bf89a02b9a4e2a4b02a6fb954fafe70cb0953bbee5ae64bfd6` |
| `shelf-offer-playmode.xml` | 17/17 geçti; failed/skipped 0 | `61dcd25d269d85deb9e41712b40015cbfe4c9e10561f44df575c4bc16564065d` |
| `shelf-offer-macos-build.log` | Universal development build; 327.511.689 bayt | `8663033c052da2f3129be3160c4ef330edcb2410ec8c832d957e510f601223bc` |
| Player executable | Mach-O `x86_64 + arm64` | `517be2d1584c85a46570a948d781fb32d860edb8664d306b5b9bcbeafee792d3` |
| `shelf-offer-macos-runtime.log` | Apple M4/Metal, 1280×720, shelf-offer smoke başarılı | `45b124ad8f314ca98ebab631982ce993f485d69c0ac2c4469087afd804ee95f0` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-shelf-offer-r11-v1 inventory-flow=arrived parcel=sealed shelf-offer=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok world-floor=ok shelf-offer=ok price-minor=54999 currency=EUR stable=ok quantity=1
```

Gerçek Input System PlayMode kapsamı:

- Klavye/fare: acceptance → open → exact pickup → RAF A placement → `E` ile fiyat publish.
- Gamepad: South acceptance/open/pickup, East güvenli WorldFloor drop ve RAF A placement, South fiyat publish.
- Publish sonrası exact item Shelf'te ve world ownership'te kalır; Inventory ve Orders revision sabittir.
- Domain testleri valid create/update, exact idempotency, deterministic query, assembly sınırı ve bütün failure no-mutation yollarını kapsar.

macOS oturumu kilitli olduğu için yeni pencere ekran görüntüsü alınmadı; sahne sözleşmesi, gerçek Input System testleri, Universal build ve native runtime logu başarılıdır. Görsel ekran görüntüsü iddiası yapılmamıştır.

## Bilinçli kapsam dışı

- Sayısal fiyat düzenleme UI'si, kategori kuralı veya öneri sistemi.
- Currency metadata ve iki ondalık kullanmayan para birimleri.
- Müşteri AI, sepet, rezervasyon, checkout ve transaction price snapshot.
- Vergi, indirim, iade, Economy ledger ve ödeme.
- Save/journal/crash atomikliği ve final model/animasyon/ses.
- Gerçek Windows x64 IL2CPP/DirectX/Steam doğrulaması.

## Uzak ve USB kapanışı

- Feature Repository Guard [31866681324](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31866681324): başarılı.
- Docs checkpoint, Issue #42 kapanışı, Project Done ve USB hash manifesti bu feature commitini izleyen kapanış adımında kaydedilecektir.

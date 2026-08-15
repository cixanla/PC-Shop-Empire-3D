# Atomic Checkout Fulfillment Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#45](https://github.com/cixanla/PC-Shop-Empire-3D/issues/45), Epic [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) altındaki authoritative stoktan görünür satış fulfillment dilimini tamamlar:

1. Exact Northstar A60; acceptance → parcel open → ActorHands → RAF A → offer → customer reservation → immutable checkout zincirini kullanır.
2. İlk `Mouse Left / Gamepad RT` `549,99 EUR` checkout fiyatını dondurur. Aktif checkout release/pickup kilidi korunur.
3. İkinci `Mouse Left / Gamepad RT`, bütün checkout satırlarını ve Inventory reservation setini mutation öncesi yeniden doğrular.
4. Başarı Inventory, Basket ve Checkout revision'larını birer kez ilerletir; serialized item, reservation ve basket line birlikte kapanır. ShelfOffer ve Orders değişmez.
5. Stable immutable completion sonucu checkout/basket/customer/timestamp/currency/total ve exact line snapshot'ı taşır. Exact tekrar state değiştirmez.
6. Fiziksel ürün raftan kaldırılır; shelf/HUD `TAMAMLANDI`, müşteri alanı `TESLİM EDİLDİ`, ürün konumu `MÜŞTERİYE TESLİM EDİLDİ • STOK 0` gösterir.

Bu sonuç ürün fulfillment'ıdır; para tahsilatı veya Economy ledger kaydı değildir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `bb89b0c297400f6eed22407df76dc1c85912cd74`
- Tree: `831b310717df32bbe2b6bb3465c8caf7323c74b8`
- Marker: `garage-sale-completion-r14-v1`
- Repository Guard: [31870482690](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31870482690), başarılı

## Otomatik doğrulama

Ham kanıtlar `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altındadır.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r14 runtime-visible fulfillment/HUD sözleşmesi; sahne YAML'ı değişmeden korundu | `6764c560b793598eb3d89bf2016028d4b5f9cf2aaa0a707c62ddc1490b489aac` |
| `editmode-checkout45-final.xml` | 242/242 geçti; failed/skipped 0 | `4eb41ded6e045e6ddaae670b852a9e9330fb448a2de7e8c258d7dff8f98be9a0` |
| `playmode-checkout45-final.xml` | 17/17 geçti; failed/skipped 0 | `0a3f37bef977b9bdbb834d0cdd6b1bf9d3414f6477490da96cf26e1b1f810420` |
| `build-checkout45-macos-final.log` | Universal development build; 327.567.424 bayt | `33e389a9cf173fe3fe290cb0cf40655574ad23b7bb082fc8f9f16d19814ce206` |
| Player executable | Mach-O `x86_64 + arm64` | `7a454107504e5614799d09f4031827336ef5283d949f524125bfaa09e5157ea6` |
| `runtime-checkout45-macos-final.log` | Apple M4/Metal, 1280×720; atomik fulfillment ve stok tüketimi | `afab243a9bc7f007f76efde63e054fee6c7a0aab88cfd390c6ac8d9df63b6915` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-sale-completion-r14-v1 scene=GarageGraybox resolution=1280x720 ... checkout-snapshot=ready checkout-completion=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok ... checkout-snapshot=ok price-frozen=ok sale-completion=ok stock-consumed=ok stable=ok completed-quantity=0 projection-quantity=1
```

Gerçek Input System PlayMode kapsamı:

- Klavye/fare: bütün fiziksel stok akışı → `G` reserve → Mouse Left checkout → aktif `G` release engeli → ikinci Mouse Left completion → projection kapalı/stok 0/`TAMAMLANDI`.
- Gamepad: South acceptance/open/pickup/publish; East placement/reserve/release; RT checkout → aktif East release engeli → ikinci RT completion → projection kapalı/stok 0/`TAMAMLANDI`.
- Domain testleri serialized ve batch bulk consumption, iki satırı tek revision'da tüketme, empty/duplicate/unknown set, exact completion repeat, identity conflict, ikinci completion, erken timestamp, reservation drift, deterministic query ve tamamlanmış historical snapshot invariantını kapsar.

macOS oturumu için yeni ekran görüntüsü kanıtı üretilmedi; sahne sözleşmesi, gerçek input testleri, Universal build ve native runtime logu başarılıdır.

## Bilinçli kapsam dışı

- Ödeme yöntemi, Economy ledger, nakit, gelir, COGS, vergi, indirim, fiş/fatura ve garanti başlangıcı.
- Fiziksel müşteri karakteri, navigation/AI, danışmanlık ve kasa animasyonu.
- Save/journal/crash atomikliği, final model/animasyon/ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature Repository Guard [31870482690](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31870482690): başarılı.
- Docs checkpoint, Issue/Project kapanışı ve USB manifest/readback bilgisi final checkpoint güncellemesinde eklenecektir.

# Stale-Safe Leave Action and Offer-Declined Exit Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#49](https://github.com/cixanla/PC-Shop-Empire-3D/issues/49), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki dördüncü bounded müşteri dilimidir:

1. Garage'da görünür `KARAR: AYRIL` bağlamında gerçek `G / Gamepad East`, immutable kararı current visit ve current RAF A offer'ıyla tekrar doğrular.
2. Current `Leave`, action receipt üretir ve müşteri visit'ini `Browsing → Exiting` geçirir; stable çıkış nedeni `OfferDeclined`dır.
3. Offer fiyat/revision veya visit snapshot'ı karar gösterildikten sonra değişirse stable `retail.offer-action.decision-stale` görünür; Action, Actors, Basket, Inventory, Checkout, Offer ve Orders mutation üretmez.
4. Leave action hiçbir reservation, Basket line, Checkout veya stok tüketimi üretmez. RAF A serialized item'ı aynı kimlik, toplam/available quantity ve fiziksel projection ile korunur.
5. Exact replay idempotent; conflicting/cross-kind replay ve aynı visit için ikinci ActionId fail-closed'dur. Historical receipt terminal exit sonrasında invariant-safe kalır.
6. Actors prepared planı side-effect-free, revision/owner/watermark-bound ve Retail friend sınırındadır. `OfferDeclined`, normal exit arrival, iki denemeli route fallback ve timeout boyunca korunur.
7. Garage NavMesh kontratı explicit `Browse → Exit` complete path'ini doğrular; başarı `TEKLİF REDDEDİLDİ • ÇIKIYOR`, stale failure `AYRILMA ENGELLİ • <stable-code>` metniyle görünür.
8. Mevcut keyboard/gamepad Buy, reservation, checkout, fulfillment, pause, taşıma/placement/cart ve lookdev regresyonları korunur. Mevcut UI graybox kanıtıdır.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `67d858aff773610cff6d6c221c792cd793f27a1b`
- Tree: `dc76a89a5a9f0f9349509aca7374f30518b1c308`
- Source/docs commit ve tree: kapanış commitinde sabitlenecek
- Marker: `garage-leave-action-r18-v1`
- Feature Repository Guard: [31882228394](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882228394), başarılı
- Source/docs Repository Guard: kapanış commitinde sabitlenecek

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r18 runtime code marker; sahne reserialize edilmedi; 1.377.364 bayt | `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685` |
| `editmode-action49-r2.xml` | 298/298 geçti; failed/skipped 0; 250.992 bayt | `be7e56fad9418de9883100653bdf90722ebd13bc896fcf5432ee86a195d1feea` |
| `playmode-action49-r2.xml` | 22/22 geçti; failed/skipped 0; 32.538 bayt | `8856709e0fc3c193359d9e3576960512aa261d9a9efdff7b1b775b5ae0658ece` |
| `build-action49-macos-r1.log` | Universal development build; `STAGE_A_BUILD_OK ... bytes=327750560`; 584.226 bayt | `c1e317068753a9668ec3737ca0cd69b0c6a37a77c77ea03653a4debd56a75df8` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Mach-O `x86_64 + arm64`; 117.179 bayt | `ec4bbd1a532f1cafb92baec1401c71a874576e4db441046f90238d94b0db605d` |
| `runtime-action49-macos-r1.log` | Apple M4/Metal, 1280×720; current/stale Leave + mevcut Buy/fulfillment/fallback zinciri; 4.787 bayt | `02c2ed8900937c0693cf867486f746923920ec048990397e1a2db3374fed6891` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-leave-action-r18-v1 scene=GarageGraybox resolution=1280x720 ... customer-buy-action=ready customer-leave-action=ready customer-navmesh=ready lookdev=ok
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok offer-decision=ok buy-action=ok stale-blocked=ok fulfilled=ok leave-action=ok stale-leave-blocked=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

Final test/build/runtime kanıtlarında failed/skipped test, assertion, smoke failure, unhandled exception veya `JobTempAlloc` sızıntısı yoktur. Unity lisans modülünün çevrimdışı access-token güncelleme uyarısı sonucu etkilemeyen ortam uyarısıdır. Native başarı marker'ı alındıktan sonra yalnız doğrulama için başlatılan player süreci kapatıldı.

## Test kapsamı

- `OfferDeclined` lifecycle receipt, side-effect-free prepare, owner/revision/watermark guard, exact replay ve terminal historical invariant.
- Exit arrival, bounded route failure ve timeout boyunca `OfferDeclined` reason korunması.
- Current Leave başarı delta'ları: Action/Actors +1; Inventory/Basket/Checkout/Offer/Orders sıfır mutation.
- Stale offer/visit, binding/kind uyuşmazlığı, conflicting ve cross-kind replay, aynı visit için ikinci ActionId no-mutation.
- Leave receipt'in boş reservation kimlikleriyle Buy komutu üzerinden replay edilmesini engelleyen regression.
- Gerçek Keyboard `G` ve Gamepad East current Leave; görünür prompt/status ve Browse→Exit NavMesh terminal hide.
- Gerçek Keyboard displayed-stale-Leave failure metni ve bütün authority snapshotlarında izolasyon.
- Mevcut Buy/reservation/checkout/fulfillment, pause/resume, route fallback/timeout, pickup/drop/placement/cart ve lookdev regresyonları.

## Bilinçli kapsam dışı

- Ödeme ve Economy settlement, nakit/gelir/COGS/vergi/indirim/fiş.
- Çoklu customer/offer/product, alternatif item seçimi, utility scoring, danışmanlık ve memnuniyet.
- Save/journal/Guardian, final UI/model/animasyon/ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature push ve Repository Guard başarılıdır.
- Source/docs push, ikinci Repository Guard, Issue/Project kapanışı ve USB milestone değerleri kapanış commitinde bu bölüme yazılacaktır.

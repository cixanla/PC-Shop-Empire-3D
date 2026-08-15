# Stale-Safe Buy Action and Checkout Navigation Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#48](https://github.com/cixanla/PC-Shop-Empire-3D/issues/48), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki üçüncü bounded müşteri dilimidir:

1. Garage'da görünür `KARAR: SATIN AL` bağlamında gerçek `G / Gamepad East`, immutable kararı current visit ve current RAF A offer'ıyla tekrar doğrular.
2. Current `Buy`, exact serialized item için action-owned Basket/Inventory reservation oluşturur ve aynı bounded komutta müşteri visit'ini `Browsing → NavigatingToCheckout` geçirir.
3. Offer fiyatı veya visit snapshot'ı karar gösterildikten sonra değişirse stable `retail.offer-action.decision-stale` görünür; Action, Basket, Inventory, Actors, Checkout ve Orders mutation üretmez.
4. Explicit Actors↔Retail customer binding olmadan action uygulanmaz. Caller exact action/line/basket/item/reservation/claim kimliklerini verir; authority alternatif ürün seçmez.
5. Action-owned reservation legacy Basket toggle, public Inventory release veya public tekli/toplu consume ile bozulamaz. Yalnız checkout fulfillment internal consume sınırından ilerler.
6. Exact replay idempotent; conflicting replay ve aynı visit için ikinci ActionId fail-closed'dur. Historical action receipt fulfillment ve müşteri çıkışından sonra invariant-safe kalır.
7. Başarı `SATIN ALMA ONAYLANDI • REZERVASYON KİLİTLİ`, stale failure `SATIN ALMA ENGELLİ • <stable-code>` metniyle renkten bağımsız görünür. Mevcut UI graybox kanıtıdır.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `6951869c4a9f33662f322c02348fa4282b9cdbb6`
- Tree: `5f4c956423bbc07b9087d47f7886ab36cc6992f1`
- Marker: `garage-buy-action-r17-v1`
- Feature Repository Guard: [31880394269](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31880394269), başarılı

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r17 runtime code marker; sahne reserialize edilmedi; 1.377.364 bayt | `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685` |
| `editmode-action48-r6-final.xml` | 287/287 geçti; failed/skipped 0; 242.395 bayt | `3bd1e3169cfda36a8b13e6b4d5bbf5f4f7fa7b9c5e9b5ccc2acc0aebc32c9bd3` |
| `playmode-action48-r7-full.xml` | 19/19 geçti; failed/skipped 0; 28.078 bayt | `caee9b22125f698c6b3e6758c6f983e2be84b7cf25276e1e391a4b867df8735e` |
| `build-action48-macos-r2-final.log` | Universal development build; `STAGE_A_BUILD_OK ... bytes=327737593`; 579.629 bayt | `404bd6148bcc7a268f54d39e34722ae8701fe27bf0c8e547389af220dd0ef35c` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Mach-O `x86_64 + arm64`; 117.179 bayt | `b58c255a9ffcfca2032cf2bbf5008c372f0b7da8d20e020aaa267a909a2bb88d` |
| `runtime-action48-macos-r2-final.log` | Apple M4/Metal, 1280×720; current Buy + stale block + mevcut fulfillment/fallback zinciri; 4.718 bayt | `084b139a37337b4dcf5a4dea53d942ad206bf046c76945b41aa153abe7657585` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-buy-action-r17-v1 scene=GarageGraybox resolution=1280x720 ... customer-buy-action=ready customer-navmesh=ready lookdev=ok
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok offer-decision=ok buy-action=ok stale-blocked=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

Final test/build/runtime kanıtlarında failed/skipped test, assertion, smoke failure, unhandled exception veya `JobTempAlloc` sızıntısı yoktur. Unity lisans modülünün çevrimdışı access-token güncelleme uyarısı sonucu etkilemeyen ortam uyarısıdır. Native başarı marker'ı alındıktan sonra yalnız doğrulama için başlatılan player süreci kapatıldı.

## Test kapsamı

- Typed binding value equality ve structural input guard.
- Current Buy success için Action/Basket/Inventory/Actors exact revision delta; Offer/Orders/Checkout izolasyonu.
- Stale offer ve stale visit bütün authority snapshotlarında no-mutation.
- Geçerli `Leave` kararının `kind-not-buy` ile no-mutation reddi; binding mismatch.
- Reservation ve checkout-navigation preflight failure'ın karşı authority'yi değiştirmemesi.
- Exact/conflicting action replay, aynı visit için ikinci ActionId ve historical fulfillment invariantı.
- Inventory/Basket/Actors prepared plan no-mutation, exact replay, stale revision/offer/watermark ve foreign-owner guardları.
- Action-owned reservation'ın legacy Basket, public Inventory release ve public Inventory consume bypass'larına karşı kilidi.
- Gerçek Keyboard ve Gamepad Buy akışları; Keyboard displayed-stale-offer failure metni ve authority izolasyonu.
- Mevcut checkout fulfillment, pause/resume, route retry/timeout, terminal despawn, placement/carry/cart ve lookdev regresyonları.

## Bilinçli kapsam dışı

- `Leave` action, `OfferDeclined` exit reason ve Browsing→Exiting geçişi.
- Checkout başlatma değişikliği, ödeme ve Economy settlement.
- Çoklu offer/product/customer, alternatif item seçimi, utility scoring ve RNG.
- Save/journal/Guardian, final UI/model/animasyon/ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature push ve Repository Guard başarılıdır.
- Source/docs push, Issue #48 kapatma, Roadmap `Done` ve ayrı SHA-256 USB milestone'ı bu checkpointin kalan kapanış adımlarıdır.
- Epic #9 açık ve `In Progress` kalır.

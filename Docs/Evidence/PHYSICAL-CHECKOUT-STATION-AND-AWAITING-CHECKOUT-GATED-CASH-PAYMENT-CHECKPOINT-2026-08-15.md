# Physical Checkout Station and AwaitingCheckout-Gated Cash Payment Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#52](https://github.com/cixanla/PC-Shop-Empire-3D/issues/52), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki yedinci bounded müşteri/kasa dilimidir:

1. GarageGraybox'ta stable `world.checkout-station.garage-001` kimlikli görünür fiziksel checkout station vardır. Oyuncu gerçek kasa köşesine gitmeden checkout veya ödeme yapamaz.
2. Station etkileşimi pause kapalı, `2,75 m` range, `24°` focus ve gerçek raycast LOS gerektirir. Uzaklık, LOS blocker, yanlış focus ve pause bütün commerce/economy authority'lerinde no-mutation kapanır.
3. RAF A üzerindeki Mouse Left/Gamepad RT artık checkout veya settlement başlatmaz; `KASA İSTASYONUNA GİT` yönlendirmesi gösterir.
4. Yalnız exact matching current customer visit'i `AwaitingCheckout` durumundayken işlem açılır. Customer/visit/basket/offer/item/reservation/Buy-action zincirinin tamamı canonical provenance olarak yeniden doğrulanır.
5. Station'daki ilk primary press immutable checkout snapshotını bir kez başlatır. Fiyat, para birimi ve acquisition unit cost donar; sonraki offer değişikliği bu snapshotı değiştirmez.
6. Held veya aynı-frame input ödeme yapmaz. Release/repress sonrasındaki ikinci primary edge exact cash settlement'ı tek kez çalıştırır; replay/conflict state'i ilerletmez.
7. Matching canonical Economy receipt; settlement, transaction, completion, checkout, customer, payment, currency, amount, COGS, Buy action, lines, ledger ve zaman eşliğini taşır. Yalnız bu receipt ürün projection'ını kaldırır ve müşteriyi `Fulfilled` yapar.
8. Dinamik `Mouse Left / Gamepad RT` binding ipucu, `KASAYI BAŞLAT`, `NAKİT ÖDEMEYİ AL` ve stable başarısızlık metni görünürdür; geri bildirim yalnız renge dayanmaz.
9. Customer focus collider'ı trigger yapılarak station çevresindeki fiziksel NPC-player sıkışması kaldırıldı. Consultation raycast'i trigger hedefini görmeye devam eder; art arda üç final customer runtime koşusu güvenli çıkışı doğruladı.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `92a0f7b814ad5e597d8d4ca033f2e533f618f719`
- Feature tree: `4150bd36fa65d4043061e5979e08efb502338fc6`
- Marker: `garage-physical-checkout-station-r21-v1`
- Feature Repository Guard: [31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515), başarılı
- Source/docs commit: `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`
- Source/docs tree: `6d73d5ac6d675733c939f181d087da3aef90f496`
- Source/docs Repository Guard: [31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650), başarılı

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r21 fiziksel checkout/customer route composition; 1.397.931 bayt | `509e6c256a9a66850dfd3cdb22b04b53596c5080ff25e7b14d29000b289bd3fe` |
| `TestResults/editmode-issue52-r3.xml` | 352/352 geçti; failed/skipped 0; 295.494 bayt | `c6bd6e4fdbe7d06e5d986a23f7dbf7bd1da9b765d2df63c2136ed37d95e0ac6d` |
| `TestResults/playmode-issue52-r3.xml` | 24/24 geçti; failed/skipped 0; 39.375 bayt | `8c05afec6b0a91345d52a61482c922346f14b6a7f71addfcfb959f09ab4a9230` |
| `TestResults/build-macos-issue52-r4.log` | Universal development build; `STAGE_A_BUILD_OK ... bytes=327864494`; 579.886 bayt | `c9a0780e1a40cc432dbf78568a72d470082319922431bd2797a514565209c69c` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Mach-O `x86_64 + arm64`; 117.179 bayt | `cf66c67f4485fcb8adfa6e2b327b9d88bbb66c06a313d47bee42ecca90f179b2` |
| `TestResults/runtime-stock-flow-issue52-r4.log` | Apple M4/Metal, 1280×720; stock/checkout/Economy r21; 11.614 bayt | `f3efecbb91b2090dc055ddbd1497a757c0e0c069030cec2495a02dc7a551676a` |
| `TestResults/runtime-customer-flow-issue52-r6.log` | Final build tekrar 1; physical checkout + safe exit; 5.247 bayt | `4e571eb3506977d54e1e3d5dd9188088dc4e70d702bfeb374bacb369dfbad6b6` |
| `TestResults/runtime-customer-flow-issue52-r7.log` | Final build tekrar 2; physical checkout + safe exit; 5.248 bayt | `3fb8630f25e0ce31d57f9056cec9a3559c2c55b0cfc8729bf56603af70e435b6` |
| `TestResults/runtime-customer-flow-issue52-r8.log` | Final build tekrar 3; physical checkout + safe exit; 5.248 bayt | `b942bf732248c784554d61ecac953bc0f96bde1226ceacf05aa07c919d64bf1b` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-physical-checkout-station-r21-v1 scene=GarageGraybox resolution=1280x720 ... checkout-station=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok ... checkout-snapshot=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok ledger-balanced=ok stock-consumed=ok stable=ok
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok consultation=ok consultation-replay=ok decision-gated=ok stale-consultation-blocked=ok offer-decision=ok buy-action=ok stale-blocked=ok awaiting-checkout-gate=ok fulfilled=ok checkout-station=ok station-focus=ok station-los=ok shelf-bypass-blocked=ok checkout-start=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok leave-action=ok stale-leave-blocked=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

Final EditMode ve PlayMode sonuçlarında failed/skipped test yoktur. Stock r4 ve customer r6/r7/r8 loglarında assertion, unhandled exception, smoke failure veya `JobTempAlloc` izi yoktur. Üç ardışık customer smoke aynı final Universal build üzerinde `customer-hidden=ok` ile tamamlanmıştır.

## Test kapsamı

- Stable station kimliği, tek visible projection, serialized references, collider/status/HUD wiring ve r21 marker sözleşmesi.
- Range, focus, LOS, pause ve yanlış state gate'leri; failure yollarında commerce, customer, inventory ve economy no-mutation.
- Shelf primary checkout/payment bypass yasağı ve fiziksel station yönlendirmesi.
- Exact current customer/visit/basket/offer/item/reservation/action provenance; stale/foreign/forged zincir engeli.
- İlk press immutable checkout, dondurulmuş fiyat/currency/cost ve offer-update izolasyonu.
- Gerçek Keyboard/Mouse ile Gamepad Input System press-release-repress; held/same-frame/replay duplicate engeli ve dinamik binding promptları.
- Exact-cash Economy settlement, canonical receipt predicate, dengeli ledger, stock projection removal ve receipt-gated customer fulfillment/exit.
- Customer trigger collider, consultation LOS ve station çevresinde deterministik safe-exit regresyonu.
- Mevcut pickup/drop/placement/rotation/stacking/cart, parcel/stock/offer/reservation, consultation/Buy/Leave, pause, route fallback ve lookdev regresyonları.

## Bilinçli kapsam dışı

- Vergi/indirim/para üstü, kart veya çoklu ödeme yöntemi; çoklu kasa/queue/customer/product.
- Fiziksel fiş yazdırma, scanner/cash-drawer gameplay'i, final POS modeli, final HUD/UI, karakter, animasyon ve ses.
- İade/garanti, çalışan kasiyer, Save/journal/Guardian ve gerçek Windows/IL2CPP doğrulaması.
- Sıradaki bounded geliştirme Epic #10 altında ilk fiziksel PC assembly dilimidir; kapsam ayrı issue/acceptance ile açılacaktır.

## Uzak ve USB kapanışı

- Feature `92a0f7b814ad5e597d8d4ca033f2e533f618f719` ve source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc` private `origin/main`e ulaştı; Repository Guard `31892420515` ve `31892875650` başarılıdır.
- Issue #52 acceptance `17/17` işaretlendi; Issue kapatıldı ve Development Roadmap durumu `Done` yapıldı.
- Parent Epic #9 path fallback, ihtiyaç/öneri, danışmanlık, patience, stale-safe Buy/Leave, exact-cash settlement ve fiziksel checkout doğruluğu kanıtlarıyla kapatıldı; Roadmap `Done`dur.
- USB milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_PHYSICAL_CHECKOUT_STATION_AND_AWAITING_CHECKOUT_GATED_CASH_PAYMENT`.
- Snapshot exact source/docs `d6cd203` arşivinden 576 tracked `SOURCE`, 7 final `EVIDENCE` ve bir `SOURCE_COMMIT.txt` içerir; 584 manifest payload satırı, toplam 586 dosya ve 10.485.924 payload baytı vardır.
- `MANIFEST.tsv` SHA-256 `7fbb5f0ce2bdd0aa32f0baa943e12d1dcf331b4ea05a85c81e0215c969531fbd`dir. Tam geri okuma 584/584 hash+boyut+path, 576/576 exact Git source ve 7/7 evidence eşliğiyle geçti; forbidden/cache/credential, internal AppleDouble ve sibling sidecar mismatch sayıları `0`dır.
- USB source exact `d6cd203` arşividir; bu closure metadata commit'i snapshot kaynak kimliğini değiştirmez.
- Sıradaki bounded geliştirme Epic #10 altında ilk fiziksel PC assembly dilimidir.

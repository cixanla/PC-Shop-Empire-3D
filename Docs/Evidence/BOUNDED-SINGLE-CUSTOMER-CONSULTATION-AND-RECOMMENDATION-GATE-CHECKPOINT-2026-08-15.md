# Bounded Single-Customer Consultation and Recommendation Gate Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#51](https://github.com/cixanla/PC-Shop-Empire-3D/issues/51), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki altıncı bounded müşteri/kasa dilimidir:

1. Müşteri RAF A'da `Browsing` durumuna geldiğinde karar artık otomatik açılmaz; matching consultation receipt yokken stable `retail.offer-decision.consultation-required` ile kilitli kalır.
2. Oyuncu görünür müşteriye en çok `2,75 m` mesafe, `24°` focus ve gerçek raycast LOS içinde bakarken `E / Gamepad South` ile ihtiyacı sorar. Pause, uzaklık, görüş engeli, yanlış state veya tamamlanmış görüşme kapıyı açmaz.
3. Başarı exact customer/visit/intent/need/product, source `Browsing` timestampı ve consultation time taşıyan immutable receipt üretir. Exact replay revision ilerletmez; identity conflict, ikinci consultation, foreign/stale visit ve non-monotonic time fail-closed'dur.
4. `CustomerVisitAuthority` başına tek canonical consultation authority bağlanır. Decision ve `Buy/Leave` action yalnız bu authority'nin exact owned receipt'ini kabul eder; value-equal kopya veya başka session receipt'i yetki değildir.
5. Görüşme sonrası mevcut tek RAF A offer'ı için açıklanabilir `Buy/Leave` recommendation açılır. Paket Inventory enumerate etmez, alternatif/gizli stok aramaz ve yeni reservation/checkout/settlement üretmez.
6. Başarılı görüşme yalnız consultation revisionını `+1` ilerletir; salt karar okuması da dahil Actors visit, Inventory, Orders, Offer, Basket, Checkout ve Economy değişmez. Matching receipt sonrası mevcut Buy/Leave ve exact-cash zinciri korunur.
7. Interact press versioned tek-consumer sözleşmesidir. Customer `Update` akışı motor odağından sonra ve carry `LateUpdate`ından önce tüketir; aynı basış pickup/cart eylemine sızmaz. Runtime input reconfigure yalnız owned clone'ları değiştirir, source assetleri korur.
8. Dinamik binding promptu, renkten bağımsız `YARDIM BEKLİYOR`/`İHTİYACI SOR` durumu ve `EKRAN KARTIMI YÜKSELTMEK İSTİYORUM` cevabı görünürdür. Pause, patience, route fallback ve güvenli exit regresyonları geçer.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `846eb5d9912150a6ef3aae9a37678d71348f92a3`
- Tree: `9052d219f013fe007dd2bf16d4fc06726b2914eb`
- Marker: `garage-customer-consultation-r20-v1`
- Feature Repository Guard: [31888147505](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888147505), başarılı
- Source/docs commit: `TBD — ana ajan tamamlayacak`
- Source/docs tree: `TBD — ana ajan tamamlayacak`
- Source/docs Repository Guard: `TBD — ana ajan tamamlayacak`

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r20 consultation/customer speech composition; 1.378.085 bayt | `353424cd5d4a1e48d4b632f21e7343eb211762e4d1468b1e5bf9e45ebc8cbbaf` |
| `TestResults/editmode-issue51-r7.xml` | 347/347 geçti; failed/skipped 0; 290.895 bayt | `a2d0861ce019649d3f6553fe79b4768f398342ad3b249c16fb89df7046a0ecc1` |
| `TestResults/playmode-issue51-r6.xml` | 23/23 geçti; failed/skipped 0; 37.250 bayt | `d4a8711b37df66828c469e1b67ff21dfd9037020a86a5f6e461938ab1e99e90c` |
| `TestResults/build-macos-issue51-r4.log` | Universal development build; `STAGE_A_BUILD_OK ... bytes=327837998`; 581.640 bayt | `680c690e6460967d3338c0b866015a61d5b76aa96cbe58fa6147b220adf175c9` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Mach-O `x86_64 + arm64`; 117.179 bayt | `2c9db944316e9eda98bd4bb13edc4f9fffd5b4ac4c1208e933820552a05c1f86` |
| `TestResults/runtime-stock-flow-issue51-r4.log` | Apple M4/Metal, 1280×720; fiziksel stok, checkout ve Economy regresyon smoke; 11.587 bayt | `f8e9358bf247749dd8d7da8851bb6b68d44632265690c85134cd5ce0b6afc915` |
| `TestResults/runtime-customer-flow-issue51-r4.log` | Apple M4/Metal, 1280×720; consultation gate/replay/stale receipt ve mevcut müşteri zinciri; 5.099 bayt | `f89345340e3539b8812be29b8fcdfcc1ccbbd5de62a1783f416e7ae1cc61ccc0` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-customer-consultation-r20-v1 scene=GarageGraybox resolution=1280x720 ... customer-consultation=ready consultation-decision-gate=ready customer-buy-action=ready customer-leave-action=ready customer-navmesh=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok world-floor=ok shelf-offer=ok ... cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok ledger-balanced=ok ... stock-consumed=ok stable=ok completed-quantity=0 projection-quantity=1
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok consultation=ok consultation-replay=ok decision-gated=ok stale-consultation-blocked=ok offer-decision=ok buy-action=ok stale-blocked=ok fulfilled=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok leave-action=ok stale-leave-blocked=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

Final EditMode ve PlayMode sonuçlarında failed/skipped test yoktur. İki runtime r4 logunda assertion, unhandled exception, smoke failure veya `JobTempAlloc` izi yoktur; Universal macOS build başarı marker'ıyla tamamlanmıştır.

## Test kapsamı

- Stable consultation kimliği/failure kodları, exact immutable provenance, ordinal sorgu ve invariant doğrulaması.
- Tek canonical authority attachment; foreign, historical ve value-equal forged receipt owner/source guardları.
- Exact replay, identity conflict, aynı visit için ikinci consultation, non-`Browsing`, non-monotonic/watermark ve revision-overflow no-mutation yolları.
- Decision validation sırası: missing, mismatched, stale ve foreign consultation; current exact receipt sonrası deterministic Buy/Leave.
- Buy ve Leave action'larında canonical receipt sahipliği, receipt zamanı, current visit/offer revalidation ve bütün authority snapshotlarında stale no-mutation.
- Gerçek Keyboard `E` ve Gamepad South; range, focus, LOS blocker, pause, görünür prompt/cevap ve same-frame tek-consumer carry izolasyonu.
- Runtime input clone ownership/reconfigure/disable/destroy yaşam döngüsü ve positive customer execution-order kontratı.
- Mevcut Buy/Leave, checkout, exact-cash settlement, ledger, stok projection, pause, route fallback/timeout, pickup/drop/placement/cart ve lookdev regresyonları.

## Bilinçli kapsam dışı

- Branching/free-form/generative diyalog, çoklu soru, voice-over ve final karakter/animasyon/UI/ses.
- Çoklu müşteri/offer/product/queue, hidden profile veya hidden inventory discovery, alternatif ürün önerisi, ranking, utility scoring, RNG ve negotiation.
- Satisfaction, reputation, loyalty, relationship history ve Save/journal/Guardian.
- Fiziksel checkout station ve `AwaitingCheckout`-gated cash payment sıradaki Epic #9 bounded paketidir; bu checkpoint'te uygulanmış değildir.

## Uzak ve USB kapanışı

- Feature commit `846eb5d9912150a6ef3aae9a37678d71348f92a3` private remote'a ulaşmıştır; feature Repository Guard [31888147505](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888147505) başarılıdır.
- Source/docs commit, tree, push ve Repository Guard: `TBD — ana ajan tamamlayacak`.
- Issue #51 kapanış checkbox/state ve Project `Done` metadata işlemi: `TBD — ana ajan tamamlayacak`; parent Epic #9 açık/`In Progress` kalmalıdır.
- USB milestone yolu, payload sayıları, `MANIFEST.tsv` SHA-256 ve readback/Git-blob eşliği: `TBD — ana ajan tamamlayacak`; doğrulanmadan tamamlandı sayılmaz.
- Sıradaki Epic #9 paketi fiziksel checkout station ve yalnız matching customer `AwaitingCheckout` iken etkin cash payment'tır; henüz uygulanmamıştır.

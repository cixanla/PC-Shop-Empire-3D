# Atomic Cash Checkout and Initial Economy Settlement Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#50](https://github.com/cixanla/PC-Shop-Empire-3D/issues/50), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki beşinci bounded müşteri/kasa dilimidir:

1. Garage'da checkout aktifken ikinci gerçek `Mouse Left / Gamepad RT`, `nakit ödemeyi al` eylemiyle immutable checkout totalını exact cash olarak tahsil eder.
2. Pozitif integer minor-unit acquisition cost; purchase-order line, exact delivery manifest/intake, serialized veya batch Inventory kaydı ve immutable checkout line snapshotı boyunca aynı currency ve değerle korunur. Manifest maliyet uyuşmazlığı acceptance öncesinde no-mutation kapanır.
3. Tek başarılı settlement; exact reservation setini tüketir, Basket ve Checkout completionı oluşturur, stable payment/settlement receipt ile tek Economy ledger transactionı kaydeder.
4. Ledger tam dört pozitif ve dengeli posting üretir: `Cash debit = SalesRevenue credit` ve `CostOfGoodsSold debit = InventoryAsset credit`. Revenue, COGS, inventory-asset delta ve gross margin deterministic minor-unit değerlerinden okunur.
5. Eksik/fazla tutar, yanlış currency, unsupported payment method, cost/identity uyuşmazlığı, stale/foreign prepared plan ve ikinci settlement bütün authority'lerde no-mutation kalır. Exact replay idempotenttir.
6. Inventory → Basket → Checkout fulfillment owner/revision-bound, side-effect-free prepared plan zinciridir. Production public completion bypassı kapalıdır; Economy tek production koordinasyon sınırıdır.
7. `AwaitingCheckout` müşteri yalnız matching receipt ve completion doğrulandıktan sonra `Fulfilled` çıkışına geçer; stok projectionı da bu kapıdan önce gizlenmez. Başarı `NAKİT ALINDI`, failure `ÖDEME ENGELLİ • <stable-code>` olarak renkten bağımsız görünür.
8. Mevcut Buy/Leave, reservation, checkout snapshot, fulfillment, customer NavMesh, pickup/drop/placement/cart ve lookdev regresyonları korunur. Büyük durum metinleri ve ledger satırları graybox kabul kanıtıdır.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `547cf971882239c912d8221f344706afc993a37b`
- Tree: `2df21fe7c9b836eb189f12f211c58d06027a1ae8`
- Marker: `garage-cash-settlement-r19-v1`
- Feature Repository Guard: [31884497043](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884497043), başarılı
- Source/docs commit: pending
- Source/docs Repository Guard: pending

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r19 runtime code marker; sahne değiştirilmedi; 1.377.364 bayt | `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685` |
| `TestResults/editmode-issue50-r2.xml` | 328/328 geçti; failed/skipped 0; 275.608 bayt | `018955608d04377739b44316d63ba88bc7b75970cde5e698b7748c45c41e4389` |
| `TestResults/playmode-issue50-r1.xml` | 22/22 geçti; failed/skipped 0; 34.359 bayt | `f5d139d7aff945a23566a60e999426a2f761eb972c83bbfd22b6a759038029a1` |
| `TestResults/build-macos-issue50-r1.log` | Universal development build; `STAGE_A_BUILD_OK ... bytes=327809376`; 596.600 bayt | `c8be9f9d35728305b0eb827c845dbf4ff3df6da45d8814e1a2fe5fc660a58ad0` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Mach-O `x86_64 + arm64`; 117.179 bayt | `c4d5f040be9486dfa5063e60f733c97e4e44dad10c24cd68c17b23ff5a217bb5` |
| `TestResults/runtime-stock-flow-issue50-r1.log` | Apple M4/Metal, 1280×720; exact cash, receipt, ledger, replay/conflict ve stok tüketimi; 11.519 bayt | `ffad5af03ba760d19bf65aa7d5112a378c512d96c8405c679ca55ab366353568` |
| `TestResults/runtime-customer-flow-issue50-r1.log` | Apple M4/Metal, 1280×720; receipt-gated Fulfilled çıkışı ve authority izolasyonu; 4.945 bayt | `0c228a31ea5229b8ed898fea1559a7a218d86ca9dcbf1ea0aeb2bef1a56877bf` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-cash-settlement-r19-v1 scene=GarageGraybox resolution=1280x720 motor=ok input=ok carry=ok placement=ok large-carry=ok rotation=ok stacking=ok transport-cart=ok inventory-flow=arrived parcel=sealed shelf-offer=ready basket-reservation=ready checkout-snapshot=ready checkout-completion=ready cash-payment=ready payment-receipt=ready economy-settlement=ready cash-ledger=ready customer-visit=ready customer-buy-action=ready customer-leave-action=ready customer-navmesh=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok world-floor=ok shelf-offer=ok price-minor=54999 currency=EUR basket-reservation=ok release=ok checkout-snapshot=ok price-frozen=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok revenue=ok cogs=ok inventory-asset=ok ledger-balanced=ok payment-replay=ok payment-conflict-blocked=ok stock-consumed=ok stable=ok completed-quantity=0 projection-quantity=1
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok offer-decision=ok buy-action=ok stale-blocked=ok fulfilled=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok leave-action=ok stale-leave-blocked=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

EditMode ve PlayMode sonuçlarında failed/skipped test yoktur. Universal macOS build başarı marker'ı ile tamamlanmış; iki ayrı Apple M4/Metal runtime smoke akışı exact cash settlement ve receipt-gated müşteri çıkışı markerlarını üretmiştir.

## Test kapsamı

- `PSE.Economy` Unity/Editor bağımsızlığı, izin verilen downstream referanslar ve Retail/Inventory/Orders reverse dependency yasağı.
- Production `CompleteCheckout` public yüzeyinin kapanması ve Economy friend coordination sınırı.
- `InventoryUnitCost` currency, pozitif tutar, üst sınır ve value equality; intake ve purchase-order line maliyet zorunluluğu.
- PO → manifest/intake → serialized/batch Inventory → checkout snapshot cost provenanceı; transfer koruması ve manifest maliyet uyuşmazlığında Orders/Inventory no-mutation.
- Inventory, Basket ve Checkout prepared planlarının side-effect-free prepare; owner, payload ve revision/stale commit guardları; exact authority delta'ları.
- Exact cash başarı; eksik/fazla amount, yanlış currency ve unsupported method için bütün authority snapshotlarında no-mutation.
- Dört posting, debit/credit dengesi, Cash, SalesRevenue, COGS, InventoryAsset delta ve gross margin invariantları.
- Exact replay sıfır ikinci mutation; settlement/transaction/completion identity conflict ve aynı checkout için ikinci payment fail-closed davranışı.
- Garage binding üzerinden receipt, ledger ve stok tüketimi; current `nakit ödemeyi al`, `ÖDEME BEKLİYOR`, `NAKİT ALINDI` ve stable failure sunumu.
- Gerçek Keyboard/Mouse ve Gamepad RT ile fiziksel akıştan exact cash settlement; müşteri `Fulfilled` çıkışı, stok/customer projection hide ve authority izolasyonu.
- Mevcut Buy/Leave, checkout snapshot/fulfillment, customer route fallback/timeout, pause, pickup/drop/placement/cart ve lookdev regresyonları.

## Bilinçli kapsam dışı

- Vergi, indirim, kupon, para üstü, fiş/fatura belgesi, garanti kaydı ve numeric price/cost editörü.
- Kart, banka, bölünmüş/çoklu ödeme, refund, chargeback ve ödeme sağlayıcısı entegrasyonu.
- Supplier payment, procurement capitalization, opening balance, rent, payroll, debt, forecast ve tam muhasebe dönemi.
- Save/journal/migration/recovery, Guardian, çoklu customer/queue ve eşzamanlı kasa.
- Final UI, model, animasyon, sanat, ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature commit `547cf971882239c912d8221f344706afc993a37b` private remote'a ulaşmıştır; feature Repository Guard [31884497043](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884497043) başarılıdır.
- Source/docs commit ve push henüz oluşturulmadı; bu nedenle source/docs tree kimliği ve final Repository Guard run'ı pendingdir.
- Issue #50 henüz bu belgeyle kapatılmadı; acceptance kapanışı ve Roadmap `Done` geçişi pendingdir.
- USB milestone henüz oluşturulmadı; snapshot yolu, manifest SHA-256, payload sayıları ve readback/Git-blob/evidence eşliği pendingdir.
- Bu pending değerler doğrulanmadan belgeye commit, CI veya USB sonucu eklenmeyecektir.

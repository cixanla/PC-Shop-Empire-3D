# ADR-0028 — Atomic Cash Checkout and Initial Economy Settlement

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Feature commit ile uygulandı; source/docs ve USB final kapanışı bekleniyor<br>
**Bağlam:** Epic #9 — immutable checkout snapshotını authoritative maliyet kökeni, stok fulfillment ve ilk nakit Economy settlementı ile tek atomik sınırda birleştirme

## Bağlam

Issue #45–#49 zinciri, fiziksel stoktan authoritative ShelfOffer'a; oradan immutable müşteri kararı, reservation, checkout navigation ve fiyatı dondurulmuş checkout snapshotına kadar güvenli bir satış akışı kurdu. Buna rağmen checkout completion tek başına çağrılabildiğinde ürün ödeme kaydı olmadan stoktan düşebiliyor, müşteri `Fulfilled` olarak çıkabiliyor ve satışın gelir ile maliyet etkisi authoritative bir kayda bağlanamıyordu.

Issue #50 bu boşluğu bounded bir ilk Economy dilimiyle kapatır. Amaç tam muhasebe sistemi kurmak değil; exact cash ödeme, checkout fulfillment, stok tüketimi ve satış transaction deltalarını tek production eyleminde, bütün katmanların preflightı mutationdan önce tamamlanacak şekilde bağlamaktır.

## Karar

- Yeni `PSE.Economy`, Unity/Editor bağımlılığı olmayan downstream domain assembly'sidir. Yalnız `PSE.Core`, `PSE.Inventory` ve `PSE.Retail` referanslarını alır. Inventory, Orders ve Retail, Economy'ye reverse reference almaz; Presentation yalnız runtime projection ve oyuncu eylemini Economy sınırına yönlendirmek için downstream tüketicidir.
- Acquisition unit cost pozitif integer minor-unit tutarı ve üç harfli currency ile immutable tutulur. Değer `PurchaseOrderLine → DeliveryManifest/InventoryIntake → serialized item veya batch Inventory record → RetailCheckoutLineSnapshot` zincirinde aynen korunur; `float`/`double` kullanılmaz.
- Orders, manifestteki her serialized item ve batch maliyetini ilgili purchase-order satırıyla acceptance öncesinde birebir doğrular. Uyuşmazlık fail-closed olur ve Orders ile Inventory değişmez. Inventory transferleri aynı cost basisini korur; satış fiyatı değişse bile checkout snapshotındaki acquisition cost değişmez.
- Fulfillment üç katmanlı prepared-plan zinciri kullanır: Inventory exact reservation tüketimini, Basket exact checkout satırlarını, Retail ise exact completion kaydını side-effect-free biçimde hazırlar. Her plan onu üreten authority instance'ına ve gözlenen revision'a bağlıdır. Yanlış owner, bozuk payload veya araya giren revision değişikliği committen önce invalid/stale sonucu verir.
- `RetailCheckoutAuthority.CompleteCheckout` production için public değildir. Completion hazırlama ve commit yüzeyi `internal` tutulur; `PSE.Economy` friend assembly olarak stok fulfillmentını koordine eden tek production sınırıdır. Test friend erişimi yalnız sözleşme kanıtı içindir ve gameplay bypassı değildir.
- İlk desteklenen ödeme yöntemi yalnız `Cash`tir. Tender currency ve minor-unit amount, current immutable checkout currency ve gross totalıyla exact eşleşmelidir. Eksik/fazla ödeme, farklı currency veya başka payment method reddedilir; para üstü üretilmez.
- `CheckoutSettlementAuthority`, bütün kimlik, zaman, payment, cost basis, checkout ve prepared-plan kontrollerini mutationdan önce tamamlar. Başarı tek kullanıcı eyleminde exact reservation setini tüketir, Basket ve Checkout completionı oluşturur, bir settlement/payment receipt ile bir Economy ledger transactionı kaydeder.
- Her başarılı settlement tam dört pozitif posting üretir:
  - `Cash` debit = checkout gross totalı,
  - `SalesRevenue` credit = checkout gross totalı,
  - `CostOfGoodsSold` debit = checkout satırlarındaki authoritative unit cost toplamı,
  - `InventoryAsset` credit = aynı cost toplamı.
- Debit ve credit toplamları aynı currency içinde eşit olmalıdır. Gross margin, receipt üzerindeki `Gross − COGS` türevidir; beşinci bir posting değildir. Bu ilk ledger yalnız satış transaction deltalarını gösterir, sahte açılış bakiyesi üretmez.
- Receipt; stable settlement, ledger transaction, checkout, completion ve customer kimlikleriyle payment method, paid-at, currency, gross ve COGS provenanceını taşır. Receipt invariantı matching completion ve dengeli transaction olmadan geçerli sayılmaz.
- Exact replay idempotent success döndürür ve hiçbir authority revisionını yeniden ilerletmez. Aynı settlement veya transaction kimliğinin farklı payloadı, aynı completion kimliğinin farklı kullanımı ve aynı checkout için ikinci settlement stable conflict failure ile fail-closed kalır.
- Payment, cost, identity, stale-plan veya preflight failure halinde Inventory, Basket, Checkout, Offer, Orders, Actors ve Economy state/revisionları değişmez. Başarı Inventory, Basket, Checkout ve Economy'yi yalnız birer kez ilerletir; Offer ve Orders değişmeden kalır.
- Garage runtime'da checkout aktifken ikinci Mouse Left / Gamepad RT eylemi `nakit ödemeyi al` komutudur. `AwaitingCheckout` müşteri yalnız matching settlement receipt mevcutsa `Fulfilled` çıkışına geçebilir. Receipt kendi matching completionını doğruladığı için sadece fulfillment veya sadece UI durumu müşteri çıkış yetkisi olamaz; stok projectionı da receipt ve completion birlikte doğrulanmadan gizlenmez.

## Sonuçlar

- Fiziksel ürün teslimi, ödeme kanıtı ve ilk muhasebe etkisi birbirinden kopuk ilerleyemez.
- Checkout fiyatı ile acquisition cost ayrı immutable provenance olarak korunur; gelir, COGS, inventory-asset azalması ve gross margin deterministic minor-unit değerlerinden hesaplanır.
- Reverse domain dependency oluşmaz. Orders maliyet kaynağını teslimatta doğrular; Economy checkout snapshotını tüketir ve Orders'ı doğrudan bilmez.
- Owner/revision-bound planlar TOCTOU ve stale UI/runtime riskini mutation öncesinde kapatır; production public completion bypassı ödeme sınırını atlayamaz.
- Büyük Garage durum metinleri ve ledger satırları graybox kabul kanıtıdır; final kasa UI'sı, fiş görseli veya production sanat dili değildir.

## Bilinçli kapsam dışı

- Vergi, indirim, kupon, para üstü, fiş/fatura belgesi, garanti kaydı ve numeric price/cost editörü.
- Kart, banka, bölünmüş/çoklu ödeme, refund, chargeback ve ödeme sağlayıcısı entegrasyonu.
- Supplier payment, procurement capitalization, opening balance, rent, payroll, debt, forecast ve tam muhasebe dönemi.
- Save/journal/migration/recovery, Guardian, çoklu customer/queue ve eşzamanlı kasa.
- Final UI, model, animasyon, sanat, ses ve gerçek Windows doğrulaması.

## Kanıt

- Issue: [#50](https://github.com/cixanla/PC-Shop-Empire-3D/issues/50)
- Feature commit: `547cf971882239c912d8221f344706afc993a37b`
- Tree: `2df21fe7c9b836eb189f12f211c58d06027a1ae8`
- EditMode: `328/328`; failed/skipped `0`; sonuç SHA-256 `018955608d04377739b44316d63ba88bc7b75970cde5e698b7748c45c41e4389`
- PlayMode: `22/22`; failed/skipped `0`; sonuç SHA-256 `f5d139d7aff945a23566a60e999426a2f761eb972c83bbfd22b6a759038029a1`
- Universal macOS build: `327809376` bayt; Mach-O `x86_64 + arm64`; Apple M4/Metal runtime `1280×720`
- Runtime marker: `garage-cash-settlement-r19-v1`; exact cash, receipt, settlement, dört dengeli ledger postingi, replay/conflict guardı, stok tüketimi ve receipt-gated `Fulfilled` çıkışı başarılı
- Feature Repository Guard: [31884497043](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884497043), başarılı
- Source/docs commit ve final Repository Guard: pending
- Issue #50 kapanışı ve doğrulanmış USB milestone: pending
- Ayrıntı: `Docs/Evidence/ATOMIC-CASH-CHECKOUT-AND-INITIAL-ECONOMY-SETTLEMENT-CHECKPOINT-2026-08-15.md`

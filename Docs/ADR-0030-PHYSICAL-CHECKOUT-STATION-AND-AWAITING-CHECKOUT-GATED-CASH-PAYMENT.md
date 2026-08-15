# ADR-0030 — Physical Checkout Station and AwaitingCheckout-Gated Cash Payment

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi ve tamamlandı; Issue #52 kapalı/Done, parent Epic #9 kapalı/Done<br>
**Bağlam:** Epic #9 — immutable checkout ve exact-cash Economy settlement'ını görünür, kasıtlı ve fiziksel bir kasa etkileşimine bağlamak

## Bağlam

Issue #46–#51 zinciri deterministic müşteri ziyareti, consultation-gated `Buy/Leave`, authoritative reservation, immutable checkout ve exact-cash Economy settlement'ını kurdu. Ancak prototipte checkout/ödeme eylemi RAF A ürün odağı üzerinden tetiklenebiliyordu. Bu, oyuncunun ürüne uzaktan bakarak kasayı başlatıp parayı alması gibi sentetik bir davranış yaratıyor ve müşteri `AwaitingCheckout` durumuna gerçekten ulaşmadan presentation katmanının ticari sonucu tetiklemesine izin verme riski taşıyordu.

## Karar

- GarageGraybox içinde stable `world.checkout-station.garage-001` kimlikli, görünür tek `CustomerCheckoutStation` bulunur. Station projection mevcut Checkout, Basket, Customer, Inventory ve Economy authority'lerini yalnız açık bir presentation adaptörü olarak kullanır; yeni shadow inventory veya ödeme authority'si üretmez.
- Etkileşim yalnız pause kapalıyken, oyuncu en çok `2,75 m` uzaktayken, kamera `24°` focus sınırındayken ve station collider'ına unobstructed raycast LOS varken geçerlidir. Uzaklık, yanlış odak, LOS engeli, pause veya yanlış customer state bütün authority'lerde no-mutation fail-closed olur.
- RAF A primary action artık checkout veya ödeme başlatmaz. Dinamik prompt oyuncuyu `KASA İSTASYONUNA GİT` metniyle fiziksel hedefe yönlendirir; shelf bypass kapalıdır.
- Station yalnız exact current customer/visit/basket/offer/item/reservation/action provenance'ı eşleşen visit `AwaitingCheckout` durumundayken çalışır. Historical, foreign, forged/value-equal veya stale kimlik zinciri yetki değildir.
- İlk `Mouse Left / Gamepad RT` press'i immutable checkout snapshotını bir kez başlatır. Dondurulmuş fiyat, para birimi, satır ve acquisition unit cost sonraki offer değişikliklerinden etkilenmez.
- İlk press ödeme değildir. Oyuncu primary action'ı bırakıp yeniden bastığında ikinci edge exact cash settlement'ı bir kez çalıştırır. Held input, aynı-frame replay, event tekrarı ve yeni input olamadan frame ilerlemesi ikinci ticari sonuç üretemez.
- `PlayerInputAdapter`, primary action için versioned tek-consumer press sözleşmesi taşır. Runtime clone sahipliği source `InputActionAsset`i değiştirmez; reconfigure/disable/destroy yaşam döngüsü önceki input invariantlarını korur.
- Settlement yalnız `PSE.Economy` sınırında yapılır. Canonical receipt; exact settlement/transaction/completion/checkout/customer/payment/currency/amount/COGS, `Buy` action provenance'ı, line seti, dengeli ledger transaction'ı ve monotonik zaman eşliğini zorunlu kılar.
- Ürün projection'ı ve customer `Fulfilled` geçişi yalnız matching canonical receipt sonrasında oluşur. Checkout completion tek başına müşteri çıkışına veya projection removal'a yetmez.
- Station promptu etkin cihaz binding'ini dinamik gösterir. `KASAYI BAŞLAT`, `NAKİT ÖDEMEYİ AL`, stable failure reason ve geçerli/geçersiz durum metinleri yalnız renge bağlı değildir.
- Customer focus collider'ı trigger'dır; oyuncuyu station'da fiziksel olarak sıkıştırmaz. Customer consultation LOS raycast'i trigger'ı bilinçli olarak görür; böylece görünür müşteri focus sözleşmesi korunur.
- Runtime smoke, projection API'sini doğrudan kullanarak domain/presentation zincirini; gerçek Input System PlayMode testleri ise Keyboard/Mouse ve Gamepad press-release-repress davranışını kanıtlar.

## Sonuçlar

- Checkout ve ödeme artık oyuncunun fiziksel kasa köşesine gitmesini, doğru müşteriyi beklemesini ve iki kasıtlı input edge'i üretmesini gerektirir.
- Shelf, uzaklık, LOS, pause, yanlış state, stale provenance ve input replay yolları ticari state'i değiştiremez.
- Immutable fiyat/maliyet ve exact Economy receipt zinciri korunurken müşteri, stok projection'ı ve muhasebe aynı canonical sonuçta birleşir.
- Checkout köşesi görünür ve oynanabilirdir; mevcut primitive graybox/PBR görünümü final POS sanat paketi değildir.
- Customer collider trigger düzeltmesi, station çevresindeki transient fiziksel çıkış stall'ını deterministik biçimde kaldırır; art arda üç final runtime koşusu bu regresyonu doğrular.

## Bilinçli kapsam dışı

- Vergi, indirim, para üstü, kart/temassız ödeme, çoklu ödeme yöntemi ve sahte/eksik para senaryoları.
- Yazdırılan fiziksel fiş, cash-drawer animasyonu, scanner işlevi, kasa kuyruğu ve çoklu kasa/müşteri.
- Çoklu ürün/basket, fiyat pazarlığı, iade/garanti ve çalışan kasiyer.
- Save/journal/migration/recovery, Guardian ve gerçek Windows/IL2CPP doğrulaması.
- Final POS modeli, markalı cihaz, final HUD/UI, karakter, animasyon ve ses.

## Kanıt

- Issue: [#52](https://github.com/cixanla/PC-Shop-Empire-3D/issues/52)
- Parent Epic: [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9)
- Feature commit: `92a0f7b814ad5e597d8d4ca033f2e533f618f719`
- Feature tree: `4150bd36fa65d4043061e5979e08efb502338fc6`
- Feature Repository Guard: [31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515), başarılı
- Source/docs commit: `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`; tree `6d73d5ac6d675733c939f181d087da3aef90f496`; [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650), başarılı
- Marker: `garage-physical-checkout-station-r21-v1`
- EditMode: `352/352`; failed/skipped `0`; `editmode-issue52-r3.xml`
- PlayMode: `24/24`; failed/skipped `0`; `playmode-issue52-r3.xml`
- Universal macOS build: `327864494` bayt; `build-macos-issue52-r4.log`; Mach-O `x86_64 + arm64`
- Scene SHA-256: `509e6c256a9a66850dfd3cdb22b04b53596c5080ff25e7b14d29000b289bd3fe`
- Runtime: Apple M4/Metal, `1280×720`; stock r4 ve customer r6/r7/r8 smoke'ları başarılı
- Issue #52 acceptance `17/17`; Issue kapalı ve Roadmap `Done`. Parent Epic #9 ana kabul kapısı doğrulandı; kapalı/Done.
- USB milestone: `2026-08-15_STAGE_B_PHYSICAL_CHECKOUT_STATION_AND_AWAITING_CHECKOUT_GATED_CASH_PAYMENT`; manifest SHA-256 `7fbb5f0ce2bdd0aa32f0baa943e12d1dcf331b4ea05a85c81e0215c969531fbd`; 584/584 readback, 576/576 exact Git source ve 7/7 evidence eşliği
- Ayrıntı: `Docs/Evidence/PHYSICAL-CHECKOUT-STATION-AND-AWAITING-CHECKOUT-GATED-CASH-PAYMENT-CHECKPOINT-2026-08-15.md`

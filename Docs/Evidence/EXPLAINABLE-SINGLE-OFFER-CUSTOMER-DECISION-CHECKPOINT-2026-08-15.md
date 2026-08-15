# Explainable Single-Offer Customer Decision Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#47](https://github.com/cixanla/PC-Shop-Empire-3D/issues/47), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki ikinci bounded müşteri dilimidir:

1. `Browsing` durumundaki stable müşteri intent'i tek immutable RAF A offer'ı ve `600,00 EUR` kabul limitiyle saf biçimde karşılaştırılır.
2. Prototype offer `549,99 EUR` olduğu için görünür sonuç `KARAR: SATIN AL`; stable neden `retail.offer-decision.buy.exact-product-within-limit` olur.
3. Product mismatch ve limit üstü fiyat geçerli `Leave`; invalid/default/state/need/currency girdileri stable failure olarak ayrılır.
4. Exact replay value-equal'dır. Eski offer ve Browsing snapshot'ları tarihsel sonucu korur, güncel authority'leri yönetmez.
5. Karar okunurken Actors, Inventory, Orders, ShelfOffer, Basket ve Checkout revision/count değerleri değişmez; reservation/checkout/visit transition yalnız sonraki explicit input/action akışında başlar.
6. Karar metni renk dışında okunabilir. Mevcut world text ve primitive müşteri graybox kanıtıdır; final production UI/model/animasyon değildir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `f97ded34f00e0d0637fbf9b41c0c0d33a7969b8e`
- Tree: `e8cddbc13166b35a081786fed895417cf6270c16`
- Marker: `garage-offer-decision-r16-v1`
- Repository Guard: [31876993251](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31876993251), başarılı

## Otomatik doğrulama

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r16 görünür karar sözleşmesi; sahne reserialize edilmedi; 1.377.364 bayt | `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685` |
| `editmode-offer47-r3-final.xml` | 267/267 geçti; failed/skipped 0; 226.509 bayt | `06847e5696aa29a73d99672bb00e894205c4e840a950256398d91a81b9446129` |
| `playmode-offer47-r2-final.xml` | 18/18 geçti; failed/skipped 0; 26.410 bayt | `133c26469fa0c074b365be265567326bff1f84fcd25b04e71f0ccadfb960677c` |
| `build-offer47-macos-r1.log` | Universal development build; `STAGE_A_BUILD_OK ... bytes=327708376`; 583.125 bayt | `b2c109d4232c97e6ff17229057eb207e10299c60105611e1d3341b5555c95522` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Mach-O `x86_64 + arm64`; 117.179 bayt | `68fd897fafc53d2560bd3a1261767ff3a91a09c7a41df6da2df0b61493cd67de` |
| `runtime-offer47-macos-r1.log` | Apple M4/Metal, 1280×720; karar + mevcut müşteri/fulfillment akışı; 4.688 bayt | `d28254dc7e74a2723215fe20d0b82c84c4fd688e864840ef2ba92e0c2a023195` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-offer-decision-r16-v1 scene=GarageGraybox resolution=1280x720 ... customer-visit=ready customer-navmesh=ready lookdev=ok
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok offer-decision=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

Final test/build/runtime loglarında assertion, smoke failure, unhandled exception veya `JobTempAlloc` sızıntısı yoktur. Unity lisans modülünün çevrimdışı access-token güncelleme uyarısı test/build sonucunu etkilemeyen ortam uyarısıdır. Native runtime başarı marker'ı alındıktan sonra yalnız bu doğrulama için başlatılan player süreci kapatıldı.

## Test kapsamı

- Stable üç outcome reason, dört failure ve explicit enum değerleri literal sözleşmeye kilitlidir.
- Exact product below/equal limit `Buy`; mismatch ve above-limit `Leave`; currency mismatch product mismatch'ten önce kazanır.
- Structural default, non-Browsing ve currency failure no-mutation kalır. Unsupported-need branch fail-closed'dur; mevcut public Actors authority yalnız `GraphicsUpgrade` kaydı ürettiği için reflection veya test-only model genişletmesi yapılmadı.
- Exact replay value equality/hash, farklı accepted-limit provenance inequality, historical offer revision ve historical Browsing snapshot replay testlidir.
- Gerçek keyboard/mouse ve gamepad akışları RAF A teklifinden Browsing'e ulaşır; karar metni/reason code görünürken altı authority ailesi sabit ve Basket/Checkout boştur.
- Mevcut reservation, checkout, fulfillment, pause/resume, route retry/timeout ve terminal despawn regresyonları korunur.

## Bilinçli kapsam dışı

- Kararı basket reservation veya lifecycle action'a uygulamak; stale/current snapshot revalidation.
- Actors `CustomerIdScope` ile Retail customer kimliği arasında kalıcı köprü.
- Çoklu offer/product/customer, ranking, utility score, RNG, personality veya diyalog.
- Availability seçimi, ödeme, Economy ledger/nakit/COGS/vergi/indirim.
- Save/journal/Guardian, final UI/model/animasyon/ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature push ve Repository Guard başarılıdır.
- Source/docs commit, source/docs Guard, Issue #47 Done/close ve doğrulanmış USB milestone bu belgenin kapanış turunda exact değerlerle eklenecektir.
- Epic #9 açık ve `In Progress` kalır.

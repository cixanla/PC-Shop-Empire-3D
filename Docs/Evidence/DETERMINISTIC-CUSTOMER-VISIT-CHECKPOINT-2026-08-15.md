# Deterministic Customer Visit Checkpoint — 15 Ağustos 2026

## Görünür sonuç

Issue [#46](https://github.com/cixanla/PC-Shop-Empire-3D/issues/46), Epic [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) altındaki ilk bounded müşteri ziyaretini tamamlar:

1. Offer yayınlanınca stable kimlikli müşteri giriş noktasında görünür ve runtime NavMesh üzerinden RAF A'ya yürür.
2. Basket reservation sonrasında kasaya gider ve authoritative visit `AwaitingCheckout` durumunda bekler.
3. Atomik fulfillment sonrasında `Fulfilled` nedeniyle çıkışa yönelir; terminalde görünür projection güvenle kapanır.
4. Route state başına iki deneme ile bounded kalır; giriş/checkout rota tükenmesi `RouteUnavailable`, sabır süresi `PatienceExpired` sonucuna gider.
5. Pause simulation clock'u ve ajanı dondurur; resume tam bir 20 ms tick ile devam eder.
6. Customer domain veya NPC transformu Inventory, Orders, ShelfOffer, Basket ya da Checkout revision'ını kendi başına değiştirmez.

Mevcut büyük HUD/world metinleri ve primitive müşteri graybox kanıtıdır; final production UI, model ve animasyon değildir.

## Kaynak checkpoint'i

- Branch: `main`
- Feature commit: `b37b056271fac317e99ec47df0833b8ef219cf83`
- Tree: `cca44dcf50f262e64fa9d6b43b48d25722978f64`
- Marker: `garage-customer-visit-r15-v1`
- Repository Guard: [31875039147](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31875039147), başarılı

## Otomatik doğrulama

Test XML ve log kanıtları `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altındadır; Universal player executable'ı aşağıdaki tabloda yazan build yolundadır.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r15 müşteri/NavMesh/checkout görünür sahne sözleşmesi; 1.377.364 bayt | `16376412909b92e06eceae83e412111770ca06a6503c9cbd975427d8d25ed685` |
| `editmode-customer46-r17-final.xml` | 255/255 geçti; failed/skipped 0; 216.064 bayt | `8e4e8ab5f628214f07ccd7955e31788c95a64d3a031433df746b1eeaa7d6c6a8` |
| `playmode-customer46-r17-final.xml` | 18/18 geçti; failed/skipped 0; 26.407 bayt | `3787c37f5871866b1e4926fcaa070a65a79e31bcf2fc3bd3abf9c49edfe9c811` |
| `build-customer46-macos-r17-final.log` | Universal development build; 327.697.921 bayt; log 580.747 bayt | `3b6b2338469bf2b1957fff74b7826579bd55fcb60f64383d08d2f3344ebd6378` |
| `Builds/Local/macOS/PC Shop Empire 3D.app/Contents/MacOS/PC Shop Empire 3D` | Player executable; Mach-O `x86_64 + arm64`; 117.179 bayt | `f62879d166ed6359ee3a0df80a771aaf52d9c93efe2ceb456960ca256a4302aa` |
| `runtime-customer46-macos-r17-final.log` | Apple M4/Metal, 1280×720; canlı müşteri rotası ve fulfillment çıkışı; 4.646 bayt | `83fa744459883cb5b871254f583171d132888d7fa3d455d1796ceeda38482514` |
| `runtime-customer46-macos-r16-leakdiag.log` | Zorlaştırılmış memory-leak diagnostics koşusu da geçti; 4.647 bayt | `e254918c22650d719ad915f7156c535b8fb0c6acb8cb0c044cc69de426e26ba4` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-customer-visit-r15-v1 scene=GarageGraybox resolution=1280x720 ... customer-visit=ready customer-navmesh=ready lookdev=ok
GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok pause=ok fulfilled=ok domain-route-fallback=ok domain-timeout-fallback=ok authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok customer-hidden=ok
```

Gerçek Input System PlayMode kapsamı:

- Klavye/fare: acceptance → parcel open → pickup/RAF A → offer → gerçek `Escape` pause/resume → müşteri RAF A → `G` reserve → checkout bekleme → iki Mouse Left fulfillment → çıkış.
- Gamepad: South acceptance/open/pickup/publish; East placement/reserve; RT checkout/fulfillment; müşteri authored route üzerinde sıfır route failure ile çıkar.
- Ayrı PlayMode fallback testi `NavMeshAgent`ı kapatır; giriş ve çıkış için `2 + 2` route failure sonrası stable terminal `RouteUnavailable` üretir ve diğer authority revision'larını sabit tutar.
- Domain testleri identity conflict, monotonik timestamp, lifecycle, exact terminal replay, sekiz receipt üst sınırı, route retry/fallback, patience/exit timeout ve non-empty receipt ledger timeout korunmasını kapsar.

Normal ve leakdiag runtime loglarında `customer-visit=failed`, `GARAGE_CUSTOMER_FLOW_FAILED`, `smoke=failed`, exception veya `JobTempAlloc` yoktur. macOS oturumu kilitli olduğu için yeni ekran görüntüsü kanıtı üretilmedi; sahne sözleşmesi, test, Universal build ve native runtime logları başarılıdır.

## Bilinçli kapsam dışı

- Utility scoring, çoklu ürün seçimi, çoklu müşteri/sıra kapasitesi ve derin danışmanlık/diyalog.
- Ödeme yöntemi, Economy ledger, nakit, gelir, COGS, vergi, indirim ve fiş/fatura.
- Memnuniyet/itibar, çalışan AI, Save/Guardian, final model/animasyon/ses ve gerçek Windows doğrulaması.

## Uzak ve USB kapanışı

- Feature Repository Guard [31875039147](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31875039147): başarılı.
- Source/docs checkpoint, Issue/Project kapanışı ve doğrulanmış USB manifest bilgileri kapanış commitinde bu bölüme eklenecektir.

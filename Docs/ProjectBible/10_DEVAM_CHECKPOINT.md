# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #40 görünür teslimat ve authoritative raf transferi tamamlandı; Epic #8 koli açma/fiyat/satış alt işleriyle devam ediyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #40 / Epic #8

- Feature commit `9d75573a86e395d2fa74f3808d43310e4d65f760`, tree `6779e31aaa6ad186acfa3b1143653d51f47e75b7`.
- GarageGraybox'ta exact Northstar A60 teslimatı `Arrived` başlar; stok kabulden önce `0`dır.
- İlk `E / Gamepad South` order manifestini authoritative Receiving'e kabul eder. İkinci etkileşim aynı serialized item'ı Receiving → ActorHands taşır ve ancak sonra fiziksel pickup yapar.
- RAF A üzerindeki geçerli placement ActorHands → Shelf transferini fiziksel world mutation'dan önce tamamlar. Güvenli bırakma ActorHands → WorldFloor kullanır.
- Domain failure fiziksel sahipliği değiştirmez; domain sonrası fiziksel failure transferi geri alır. Recovery Inventory container'ı ve görünür nesneyi aynı son güvenli dünya durumuna döndürür.
- Teslimat alanı, carton, durum panosu/ışığı ve authoritative RAF A görünürdür; HUD order/konum durumunu dinamik gösterir.
- EditMode `188/188`, gerçek Input System PlayMode `17/17`, Universal macOS build ve Apple M4/Metal runtime smoke geçti.
- Karar: `Docs/ADR-0018-TRANSACTIONAL-WORLD-INVENTORY-PROJECTION.md`; kanıt: `Docs/Evidence/AUTHORITATIVE-STOCK-FLOW-CHECKPOINT-2026-08-15.md`.
- Çok satırlı koli açma, fiyat/para, müşteri checkout/satış, save ve final sanat sonraki bounded paketlerdir.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya ortam kapanana kadar bağımlılık sırasındaki küçük paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → gerektiğinde ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, token/credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core: stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministik event dispatcher tamam.
- Catalog: stable ürün/kategori ID, serialized/batch tracking policy ve immutable deterministic katalog tamam.
- Inventory: serialized item, batch position, container capacity, atomik transfer, reservation, consume/release, deterministic query ve invariant audit tamam.
- Orders: exact purchase-order manifesti, monotonik delivery lifecycle ve atomik receiving kabulü tamam.
- Explicit Presentation adaptörü: Receiving → ActorHands → Shelf/WorldFloor container zinciri ile fiziksel dünya projeksiyonu tamam.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; marker `garage-authoritative-stock-flow-r9-v1`.
- Küçük kutu `E / Gamepad South` ile alınır; `Mouse Left / Gamepad RT` placement önizlemesini açar; `R / Right Shoulder` 90° döndürür; `G / Gamepad East` yerleştirir veya güvenli bırakır.
- Placement 0,25 m grid/90° yaw, tam footprint desteği, overlap, stable stacking ve açık `InventoryPlacementZone` doğrulaması kullanır.
- Büyük kutu ayrı `0,65×` carry profili ve fail-closed drop kullanır. Platform arabası aynı stable item'ı hands→cart→hands taşır; yüklü/boş `0,85×`/`0,90×`, sprint kilitlidir.
- Tek slot, stable item ID, physics snapshot, disable/world-floor recovery ve önceki bağımsız prototype davranışları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `9d75573a86e395d2fa74f3808d43310e4d65f760`
- Tree: `6779e31aaa6ad186acfa3b1143653d51f47e75b7`
- USB snapshot source/docs checkpoint commit: `f20fd1741a7d51b1350e7c1e2785e72c0718be84`.
- Epic/issue: [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) / [#40](https://github.com/cixanla/PC-Shop-Empire-3D/issues/40)
- Repository Guard: [31864259779](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31864259779), başarılı.

## Test, build ve runtime kanıtı

Ham çıktılar Git dışındaki `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `stock-flow-editmode-final.xml` | 188/188 geçti; failed/skipped 0 | `a68f13ed6ad0acded29bbd2bb9f7d2022d33640d8e4d139eec3ac9dea6b61932` |
| `stock-flow-playmode-final.xml` | 17/17 geçti; failed/skipped 0 | `199a022b9e45556c23e7c2e6bcf5d68fdc5102c832c78445f10644522aff81b4` |
| `stock-flow-macos-build.log` | Universal development build, 327.462.869 bayt | `0cedd45fb8bf69cba64f2e2f991f42c1d2b009060bcfb6dc47d91dd150b39ab9` |
| Player executable | Mach-O `x86_64 + arm64` | `53cf8ab1e929fc0aace8f5eedbc4314ad29a05f33cf83690ab56c6406e71a642` |
| `stock-flow-macos-runtime.log` | Apple M4/Metal 1280×720; `stock-flow=ok accepted=ok carry=ok world-floor=ok stable=ok quantity=1` | `83991ad781087259a0426ead6eb9a2dd65c6764c7d170db4382b9817108d0c0b` |

PlayMode klavye tam raf akışını, gamepad WorldFloor akışını ve ActorHands capacity failure no-mutation davranışını gerçek device state ile doğrular. Kilitli macOS oturumu nedeniyle bu turda güvenilir pencere ekran görüntüsü alınmadı; test/build/native runtime log kanıtı başarılıdır. Mac kanıtı Windows native doğrulamasının yerine geçmez.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core/time/event: `8af2ad3d05906839c4b607e4958650e723060465`.
- PCG32/seed/dispatcher: `bbb3648`, `43e9217`, `3d819e5`.
- İlk oynanabilir garaj: `c7a3a26075998252d9ae8b88824d8285e5067069`.
- Safe pickup/drop, placement, large carry, rotation: `44b8162`, `720e6d4`, `e944198`, `661f2dc`.
- Lookdev benchmark, stacking, loaded cart: `c7214af`, `2e11e30`, `82bf74f`.
- Catalog + Inventory: `71935f11b80d02d03f9dcc1a3f08cafca7e301ff`.
- Atomic purchase-order receiving: `e596e079d90b6d5b9d94714d7821502574eba3c9`.
- Authoritative stock-flow projection: `9d75573a86e395d2fa74f3808d43310e4d65f760`.

## USB güvenlik katmanı

Korunan milestone kayıtları `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D` altındadır. Güncel snapshot `2026-08-15_STAGE_B_AUTHORITATIVE_STOCK_FLOW`, source commit `f20fd1741a7d51b1350e7c1e2785e72c0718be84` taşır. İçerik 467 tracked kaynak + 4 test/build/runtime kanıtı + source kaydıdır; 472 manifest satırı ve manifest SHA-256 `5521f869703d1ec480912f21fb70e21fdf0b235f7c15e4be65431e1fc0ae22a3` tam readback/source checksum ile doğrulandı. Hash/boyut mismatch, source mismatch, forbidden dir, credential filename ve AppleDouble sayısı `0`dır. Büyük `.app`, cache, build çıktısı ve credential snapshot dışındadır; USB güvenle çıkarılabilir.

## Sıradaki bounded paket

1. Epic #8 altında fiziksel teslimat kolisini açma ve exact manifest içeriğini birim birim, duplicate üretmeden Receiving world projection'ına çıkarma.
2. Sonra raf ürünü için authoritative fiyatlandırma/etiket sözleşmesini küçük saf domain paketi olarak kurma.
3. Ardından müşteri seçimi ve checkout/satış zincirine geçme; Save/Guardian sınırlarını kendi issue'larında tutma.
4. Benchmark görsel dilini yalnız tamamlanan gameplay alanlarına kademeli yayma; graybox'ı final art saymama.

Her adım ayrı issue, test, commit, CI ve checkpoint ile kapanır. Inventory quantity hiçbir aşamada dünya nesnesinden türetilmez.

## Güvenli devam komutu

> Authoritative stock-flow checkpointinden devam et. Önce bu belgeyi, `PROJECT_BIBLE.md`, birleşik hafızayı, temiz `origin/main` eşitliğini ve Epic #8'i doğrula. Sıradaki bounded paket fiziksel teslimat kolisini açıp exact manifest item'larını duplicate üretmeden Receiving projection'ına çıkarmaktır. Test, build/runtime uygunluk kanıtı, commit/push/CI ve yaşayan kayıt olmadan tamamlandı sayma.

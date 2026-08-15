# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #45 ve Epic #8 tamamlandı/Done; teknik order-to-sale graybox kapısı kapandı; sıradaki bounded alan Issue #9 müşteri davranışıdır<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #45 / Epic #8

- Feature commit `bb89b0c297400f6eed22407df76dc1c85912cd74`, tree `831b310717df32bbe2b6bb3465c8caf7323c74b8`.
- `InventoryAuthority.ConsumeReservations`, exact reservation setini ve serialized/batch hedeflerini mutation öncesi tamamen doğrular; başarıda bütün seti tek Inventory revision'ında tüketir.
- `RetailCheckoutAuthority`, aktif checkout, basket line, serialized item, reservation/claim, ürün ve offer bağlarını yeniden preflight eder. Başarı Inventory, Basket ve Checkout revision'larını birer kez ilerletir; ShelfOffer ve Orders sabit kalır.
- Stable immutable completion kaydı checkout/basket/customer, simulation timestamp, currency/total ve exact line snapshot'ını korur. Exact completion ve tamamlanmış checkout begin tekrarları idempotenttir.
- Empty/duplicate/unknown reservation seti, reused completion ID, ikinci completion, erken timestamp ve cross-authority drift bütün authority'lerde no-mutation kalır.
- Garajda ilk `Mouse Left / Gamepad RT` fiyatı dondurur; ikinci aynı input fulfillment'ı tamamlar. Ürün raftan kaldırılır; stok/sepet/reservation `0`, shelf/HUD `TAMAMLANDI` gösterir.
- EditMode `242/242`, gerçek Input System PlayMode `17/17`, Universal macOS build ve Apple M4/Metal 1280×720 `sale-completion=ok stock-consumed=ok completed-quantity=0` runtime smoke geçti.
- Karar: `Docs/ADR-0023-ATOMIC-CHECKOUT-FULFILLMENT.md`; kanıt: `Docs/Evidence/ATOMIC-CHECKOUT-FULFILLMENT-CHECKPOINT-2026-08-15.md`.
- Bu completion ürün fulfillment kanıtıdır; ödeme/Economy ledger, fiziksel müşteri AI, vergi/indirim, Save ve final UI ayrı bounded paketlerdir.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya ortam kapanana kadar bağımlılık sırasındaki küçük paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, token/credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`; Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministic event dispatcher tamam.
- Catalog/Inventory/Orders/Retail: immutable product, authoritative container/transfer/reservation, exact purchase-order receiving, shelf offer, customer basket, immutable checkout ve atomik fulfillment authority'leri tamam.
- Explicit Presentation adaptörü: Arrived → acceptance/Receiving → parcel open → ActorHands → Shelf/WorldFloor → offer publish → customer reserve → checkout begin → completion zinciri tamam.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-sale-completion-r14-v1`.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `bb89b0c297400f6eed22407df76dc1c85912cd74`
- Tree: `831b310717df32bbe2b6bb3465c8caf7323c74b8`
- Epic/issue: [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) / [#45](https://github.com/cixanla/PC-Shop-Empire-3D/issues/45)
- Feature Repository Guard: [31870482690](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31870482690), başarılı.
- Source/docs checkpoint commit: `80eea8f507ff74d7522e8260a0eca6fadf7b78c6`, tree `6c34948a54fefc941bc28e41ef74d9e69f3e3f9d`.
- Docs Repository Guard: [31870828169](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31870828169), başarılı.
- Issue #45 ve Epic #8 kapatıldı; iki Roadmap item'ı da Done yapıldı. Bu kapanış ödeme/Economy, final Dashboard UI, müşteri AI veya Save tamamlandı iddiası taşımaz.

## Test, build ve runtime kanıtı

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r14 runtime-visible fulfillment/HUD sözleşmesi | `6764c560b793598eb3d89bf2016028d4b5f9cf2aaa0a707c62ddc1490b489aac` |
| `editmode-checkout45-final.xml` | 242/242 geçti; failed/skipped 0 | `4eb41ded6e045e6ddaae670b852a9e9330fb448a2de7e8c258d7dff8f98be9a0` |
| `playmode-checkout45-final.xml` | 17/17 geçti; failed/skipped 0 | `0a3f37bef977b9bdbb834d0cdd6b1bf9d3414f6477490da96cf26e1b1f810420` |
| `build-checkout45-macos-final.log` | Universal development build, 327.567.424 bayt | `33e389a9cf173fe3fe290cb0cf40655574ad23b7bb082fc8f9f16d19814ce206` |
| Player executable | Mach-O `x86_64 + arm64` | `7a454107504e5614799d09f4031827336ef5283d949f524125bfaa09e5157ea6` |
| `runtime-checkout45-macos-final.log` | Apple M4/Metal 1280×720; fulfillment ve stok tüketimi | `afab243a9bc7f007f76efde63e054fee6c7a0aab88cfd390c6ac8d9df63b6915` |

Runtime işaretleri:

```text
GARAGE_GRAYBOX_RUNTIME_READY version=garage-sale-completion-r14-v1 scene=GarageGraybox resolution=1280x720 ... checkout-snapshot=ready checkout-completion=ready lookdev=ok
GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok ... checkout-snapshot=ok price-frozen=ok sale-completion=ok stock-consumed=ok stable=ok completed-quantity=0 projection-quantity=1
```

Gerçek klavye/fare ve gamepad, bütün physical stock-flow zincirinden sonra reserve → checkout begin → aktif release/pickup engeli → completion → projection kapalı/stok `0` sonucunu doğrular. Kilitli macOS oturumu nedeniyle yeni ekran görüntüsü yoktur; sahne sözleşmesi, test, Universal build ve native runtime log kanıtı başarılıdır.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core/time/event ve RNG/dispatcher: `8af2ad3`, `bbb3648`, `43e9217`, `3d819e5`.
- Oynanabilir garaj, lookdev, stacking ve loaded cart: `c7a3a26`, `44b8162`, `720e6d4`, `e944198`, `661f2dc`, `c7214af`, `2e11e30`, `82bf74f`.
- Catalog + Inventory: `71935f11b80d02d03f9dcc1a3f08cafca7e301ff`.
- Purchase-order receiving, stock projection ve parcel reveal: `e596e079`, `9d75573a`, `3766f3f0`.
- Shelf offer, customer basket ve immutable checkout: `7a23cd92`, `45c2cdc4`, `294999f6`.
- Atomic checkout fulfillment: `bb89b0c297400f6eed22407df76dc1c85912cd74`.

## USB güvenlik katmanı

Korunan milestone kayıtları `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D` altındadır. Güncel snapshot `2026-08-15_STAGE_B_ATOMIC_CHECKOUT_FULFILLMENT`, source/docs checkpoint `80eea8f507ff74d7522e8260a0eca6fadf7b78c6` taşır. İçerik 510 tracked kaynak + 4 test/build/runtime kanıtı + source kaydı, toplam 9.373.684 bayttır; 515 manifest satırı ve manifest SHA-256 `ce72122a6df5b7e567d7eeec4c0ba537b3c87bc594a388b7c94a8507162db50b` tam readback ve 510/510 Git blob eşliğiyle doğrulandı. Eksik/fazla yol, hash/boyut/Git-blob mismatch, forbidden dir, credential filename/kalıbı ve AppleDouble `0`; büyük `.app`, cache ve credential dışarıdadır.

## Sıradaki bounded paket

1. Issue #9 altında stable müşteri visit/intent/state kimliklerini ve yalnız geçerli monotonik transition sözleşmesini Unity bağımsız kur.
2. Bounded timeout ve açıklanabilir fallback/abandon sonucunu deterministic simulation time ile üret; path/navigation başarısızlığı item veya para uydurmasın.
3. İlk görünür graybox projection'ını giriş → RAF A göz atma → checkout bekleme → çıkış zincirine bağla; NPC transformu Inventory/Checkout authority'sini doğrudan değiştirmesin.
4. Ödeme/Economy ledger/COGS/nakit, derin danışmanlık, Save/Guardian ve final karakter sanatını ayrı bounded paketlerde tut.

Her adım ayrı issue, test, commit, CI ve checkpoint ile kapanır.

## Güvenli devam komutu

> Atomik checkout fulfillment checkpointinden devam et. Önce yaşayan belgeleri, temiz `origin/main` eşitliğini ve Issue #9'u doğrula. Sıradaki bounded paket müşteri visit/intent/state ile timeout/fallback sözleşmesidir; NPC projection'ı Inventory/Checkout authority, ödeme/Economy veya Save yerine geçmesin. Idempotency ve failure no-mutation korunsun; test, commit/push/CI ve yaşayan kayıt olmadan tamamlandı sayma.

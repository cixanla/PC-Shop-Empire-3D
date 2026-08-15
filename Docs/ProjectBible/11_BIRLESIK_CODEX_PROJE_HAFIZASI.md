# PC Shop Empire 3D — Birleşik Codex Proje Hafızası

**Konsolidasyon tarihi:** 15 Ağustos 2026
**Tek aktif Codex görevi:** `PC Shop Empire 3D — ANA GÖREV`
**Ana görev kimliği:** `019ff9d8-089c-71a1-93c5-8cb614d0b5ca`
**Canonical Unity/Git kökü:** `/Users/cixanla/Developer/PCShopEmpire3D/Game`
**Private GitHub:** `cixanla/PC-Shop-Empire-3D`
**Branch:** `main`

Bu belge, `PC Shop Empire Similator` altındaki üç Codex görevinin proje açısından anlamlı bütün bilgisini tek uygulanabilir hafızada birleştirir. Tam kullanıcı/Codex konuşmaları [CodexHistory indeksinde](../CodexHistory/README.md) korunur. Günlük teknik devam noktası için her zaman [10_DEVAM_CHECKPOINT.md](10_DEVAM_CHECKPOINT.md) daha günceldir.

## 1. Konsolidasyon kararı ve görev sınırı

Kullanıcı şu kararları açıkça onayladı:

- Yalnız `PC Shop Empire Similator` altında görünen üç Codex görevi birleştirilecektir.
- Güncel geliştirme görevi tek ana görev olarak kalacaktır.
- Eski ana planlama görevi ve birleştirme görevi, aktarım doğrulandıktan sonra arşivlenecektir; kalıcı olarak silinmeyecektir.
- Bütün kullanıcı/Codex konuşmaları, üretilen/değiştirilen dosyalar, kararlar, tamamlanan ve yapılacak işler merkezî arşive aktarılacaktır.
- Projenin bundan sonraki çalışması tek Codex kanalı üzerinden sürdürülecektir.
- Sistem/developer talimatları, iç düşünce zincirleri, ham kimlik doğrulama verileri ve güvenlik açısından taşınmaması gereken token/parola çıktıları aktarılmaz. Bunların proje üzerinde oluşturduğu sonuçlar aktarılır.

Birleştirilen görevler:

1. `019fec8c-cae9-7973-9ca2-33663c84e991` — uzun vadeli vizyon, araştırma, mimari, Stage A, deterministik Core, ilk oynanabilir garaj ve ilk fiziksel etkileşim geçmişi.
2. `019ff9d8-089c-71a1-93c5-8cb614d0b5ca` — Issue #6 altındaki placement, büyük kutu, rotation, lookdev ve stacking geliştirmeleri; bu görev artık ana görevdir.
3. `01a002ff-fbc6-74d1-819a-3844c98c6ce3` — kapsam belirleme, tam aktarım, ana görev seçimi ve arşivleme işlemi.

## 2. Projenin nihai gayesi

Mevcut PC Shop Empire, eski Electron/HTML tabanlı 2D yönetim oyunundan bağımsız olarak Unity 6 ve URP ile sıfırdan geliştirilen, büyük kapsamlı bir 3D bilgisayar mağazası ve teknoloji perakendesi simülasyonuna dönüşecektir.

Temel oyuncu fantezisi:

- Oyuncu küçük bir garajda sınırlı para, alan, stok ve ekipmanla başlar.
- Garajdan mahalle dükkânına, profesyonel mağazaya ve çok bölümlü büyük teknoloji işletmesine büyür.
- Mağazada birinci şahıs olarak yürür; görünür ellerle kutu, ürün ve PC parçalarını fiziksel olarak taşır.
- Sipariş verir, teslimat alır, stok alanını ve rafları düzenler, müşterilere satış yapar, kasayı ve servisi yönetir.
- Bilgisayarları tek düğmeyle menüden üretmez; fiziksel çalışma masasında parçaları seçer, takar, kablolar, test eder, paketler ve teslim eder.
- Çalışanlar satış, kasa, depo, teknisyenlik, temizlik, yönetim ve güvenlik gibi gerçek roller üstlenir.
- Müşteriler farklı bütçe, ihtiyaç, sabır, teknik bilgi, tercih ve memnuniyet davranışlarına sahip olur.
- Ekonomi; tedarikçi, stok, talep, fiyat, ürün eskimesi, garanti, iade, servis, ikinci el, reklam, itibar ve büyüme sistemleriyle birbirine bağlı çalışır.

İlham kaynaklarından yalnız tasarım ilkeleri alınır. Başka oyunların kodu, adı, görseli, sesi, arayüzü veya telifli özgün içeriği kopyalanmaz. Gerçek marka/model verisi doğrulanmadan kullanılmaz; özgün veya kurgusal içerik tercih edilir.

## 3. Kesinleşmiş deneyim kararları

- Kamera: birinci şahıs.
- Oyuncu gövdesi: en az görünür eller; ileride gelişmiş el modeli ve animasyon.
- Temel fiziksel işler 3D dünyada yapılır.
- Dashboard kaybolmaz; oyun içindeki fiziksel bilgisayar, tablet veya yönetim terminalinden açılan yönetim katmanı olur.
- Dashboard sipariş, stok, fiyat, finans, çalışan, görev, müşteri siparişi, reklam, anlaşma, kira/fatura/vergi, pazar ve servis yönetir.
- Dashboard fiziksel montaj, kutu taşıma, raf yerleştirme ve ürün teslimi yerine geçmez.
- Okunaklı yarı gerçekçi görsel yön kullanılır: gerçek oran, PBR yüzey, zemine oturan ışık, doğal ağırlık ve ölçülü stilizasyon.
- Mevcut primitive garaj, kutular ve eller final sanat değildir; mekanik ve kalite kanıtıdır.
- Ana ticari hedef Windows x64 ve Steam 1.0'dır.
- Geliştirme Mac üzerinde yapılabilir; gerçek Windows/DirectX/IL2CPP/Steam doğrulaması Faz 1 kapanmadan zorunludur.
- macOS sürümü Windows 1.0 sonrasındaki ayrı maliyet, signing ve notarization kapısıdır.
- Oyuncuyu yoran gereksiz mikro-yönetimden, tekdüze tekrardan ve gizli hileden kaçınılır.
- Guardian sistemi tanılama ve raporlama yapar; insan/Codex onayı olmadan üretim kodunu kendiliğinden değiştirmez.

## 4. Authoritative teknik temel

- Unity: `6000.3.21f1`.
- Render pipeline: URP `17.3.0`.
- Dil: C#.
- Core assembly: `PSE.Core`; Unity/Editor bağımlılığı yoktur.
- Gameplay sınırları: `PSE.World` ve `PSE.Presentation`.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`.
- Connected oyuncu prefabı: `Assets/Prefabs/Prototype/PlayerRig.prefab`.
- Legacy kaynak: `LegacyReference/PC-Shop-Empire-1.1.6/Source`; tasarım ve veri semantiği referansıdır, yeni Unity mimarisine doğrudan port edilmez.
- Git ve private GitHub tek authoritative sürüm kontrolüdür.
- Unity Version Control ilk uzak check-in bağlantı reseti nedeniyle tamamlanmadı; ikinci authoritative VCS sayılmaz.

Tamamlanmış Core sözleşmeleri:

- Scope tipli kararlı kimlikler ve canonical doğrulama.
- `Failure` ve `OperationResult`.
- Deterministik süre, timestamp, pause destekli simulation clock.
- Stable domain event type/ID, one-based sequence ve immutable envelope.
- PCG32 `pcg32-xsh-rr-64-32-v1`, golden vector, snapshot/restore ve bias'sız bounded integer.
- SHA-256 framed stream derivation `sha256-framed-be-pcg32-v1`.
- Correlation/direct-causation, global FIFO, breadth-first nested enqueue ve bounded in-memory dispatcher.

## 5. Tamamlanan üretim kilometre taşları

Korunan temel commit çizgisi:

- Stage A baseline: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`.
- Core assembly: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`.
- Stable identity/result: `4cd2d928dbfda1886632bacce4a141c2a43161df`.
- Deterministic time/event: `8af2ad3d05906839c4b607e4958650e723060465`.
- PCG32: `bbb3648c6e34eedd77e1bec948d5ee630f89679c`.
- Seed derivation: `43e92174ca3866dfde436fb180785a615772a886`.
- Event dispatcher hardening: `3d819e533fd3635bc9b32787730d6dd9be110875`.
- İlk oynanabilir garaj: `c7a3a26075998252d9ae8b88824d8285e5067069`.
- Güvenli fiziksel pickup/drop: `44b816289f942e57fc176b26b203711090d0e61c`.
- Kontrollü küçük-kutu placement: `720e6d4ac2b2afad9ee86f907c533cbabb1bf5ed`.
- Güvenli büyük-kutu taşıma: `e94419862b04f6f03f97ef2e43c9da393c5d30a9`.
- Deterministik placement rotation: `661f2dcc64246a8282fd63fbf303454ec856ea40`.
- Okunaklı yarı gerçekçi benchmark: `c7214afab81a360a3ca10a88cbdd29f67e741994`.
- Güvenli küçük-kutu stacking feature: `2e11e30a1a4b3435046ae18001004cacc170079e`.
- Stacking yaşayan checkpoint: `74070f7bbab041b1a978ef5f889f64b1cfcd6ff9`.
- Codex proje konsolidasyonu: `2c10873a7e6ec3984292418121bed19072dd6d79`.
- Yüklü taşıma arabası feature: `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1`.
- Atomik checkout fulfillment feature: `bb89b0c297400f6eed22407df76dc1c85912cd74`.
- Deterministik customer visit ve runtime NavMesh feature: `b37b056271fac317e99ec47df0833b8ef219cf83`.

Tamamlanan oynanabilir sistemler:

- CharacterController tabanlı birinci şahıs hareket.
- Klavye/fare ve gamepad Input System sözleşmesi.
- Rebind override store.
- FOV, hassasiyet, invert ve motion-reduce ayarları.
- Pause/cursor ve runtime-ready tanısı.
- Görünür prototip eller.
- İki metre hedef çözümleme, tek taşıma slotu ve stable item identity.
- Küçük kutuyu `E / Gamepad South` ile alma.
- Güvenli bırakma ve disable/world-floor recovery.
- Küçük kutu placement modu; grid/yaw snap, tam destek ve overlap doğrulaması.
- Yeşil/kırmızı ghost ve geçerli/engelli geri bildirimi.
- `R / Right Shoulder` ile deterministik 90° rotation.
- Büyük kutu için iki-el pozu, 0,65× hareket, sprint kilidi ve motion-safe FOV bedeli.
- Büyük kutu için gerçek boyuta göre fail-closed güvenli bırakma.
- Stable küçük kutu üzerinde merkez/90° snap, beş noktalı footprint, tek kat/tek üst ilişkisi ve dolu tabanı alma kilidi.
- Tek `LargeBox` kapasiteli stable platform arabasına hands→cart→hands transferi.
- Dört noktalı zemin desteği, swept obstruction, yüklü/boş hız profili, sprint kilidi, gerçek keyboard/gamepad kontrolü ve fail-closed cargo recovery.
- Tek referans garaj köşesinde bevel, prosedürel PBR yüzey, görev ışığı, ACES/bloom ve reflection probe.
- Unity-bağımsız `PSE.Actors` sınırında kararlı müşteri/intent/visit kimliği, immutable lifecycle state'i ve bounded command receipt ledger'ı.
- Garajda runtime-built NavMesh üzerinde giriş → RAF A göz atma → checkout bekleme → çıkış müşteri projection'ı.
- İki denemeli route fallback, patience/exit timeout, pause-safe `SimulationClock` ve stock/checkout/order authority izolasyonu.

## 6. Konsolidasyon anındaki kesin durum — tarihsel baz

- Son doğrulanmış kaynak feature: `2e11e30a1a4b3435046ae18001004cacc170079e`.
- Son yaşayan checkpoint ve konsolidasyon öncesi HEAD: `74070f7bbab041b1a978ef5f889f64b1cfcd6ff9`.
- `main` ve `origin/main` eşittir.
- Çalışma ağacı temizdir.
- Issue #35 tamamlanmış ve `Done` durumundadır.
- EditMode: `131/131` geçti.
- PlayMode: `12/12` geçti.
- Universal macOS development build başarılıdır.
- Apple M4/Metal 1280×720 gerçek player smoke: `rotation=ok stacking=ok lookdev=ok`.
- Repository Guard run `31856764087` başarılıdır.
- Taşıma arabası kodu başlamamıştır.
- Konsolidasyon sırasında açılan geçici taslak ve Issue #36 tamamen kaldırılmıştır.
- Bu konsolidasyon belgeleri dışında kullanıcıya ait veya ilişkisiz açık değişiklik yoktur.

### Konsolidasyon sonrası güncel checkpoint

- Son doğrulanmış kaynak feature: `67d858aff773610cff6d6c221c792cd793f27a1b`, tree `dc76a89a5a9f0f9349509aca7374f30518b1c308`.
- Issue #49 kapsamındaki stale-safe current `Leave`, kind-discriminated action receipt, `Browsing → Exiting`, stable `OfferDeclined` ve Browse→Exit Garage NavMesh dilimi source/docs/CI/USB dahil tamamlandı; Issue `15/15` acceptance ile kapalı/Done, Epic #9 açık/In Progress kalır.
- Feature Repository Guard [31882228394](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882228394) ve source/docs `868885a` Repository Guard [31882508496](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31882508496) başarılıdır.
- EditMode `298/298`, gerçek Input System PlayMode `22/22` geçti; failed/skipped `0`.
- Universal macOS development build `327.750.560` bayt, Mach-O `x86_64 + arm64`; Apple M4/Metal 1280×720 runtime smoke `garage-leave-action-r18-v1 leave-action=ok stale-leave-blocked=ok authority-isolated=ok` ile geçti.
- Exact serialized item teslimat → kabul/Receiving → parcel open → ActorHands → Shelf/WorldFloor → offer publish → basket reserve → checkout → atomic fulfillment zincirinde korunur; müşteri projection'ı bu authority'leri NPC transformundan yönetmez.
- Customer visit lifecycle yalnız ileri ilerler; route başarısızlığı iki denemeyle sınırlıdır. `RouteUnavailable`, patience ve exit timeout sonuçları açıklanabilir ve no-mutation'dır; exact terminal replay ve receipt ledger invariantları testlidir.
- Tek-offer evaluator yalnız immutable visit/offer/accepted-price provenance tüketir; action authority current snapshotı yeniden doğrular. Stale Buy/Leave bütün authority'lerde no-mutation; exact kind replay idempotent, cross-kind replay conflict ve explicit binding olmadan reservation/navigation/OfferDeclined çıkışı yoktur.
- Mevcut primitive müşteri, büyük durum panoları ve debug HUD final sanat/UI değildir. Okunaklı yarı gerçekçi görsel yön ve bağlamsal/diegetic UI ayrı bounded presentation paketlerinde geliştirilecektir.

## 7. Sıradaki işler ve bağımlılık sırası

En yakın bounded paket:

1. Issue #9 altında immutable checkout completion'ı atomik payment receipt ve ilk `PSE.Economy` nakit/gelir/COGS settlement'ına bağlayan bounded sözleşmeyi kur.
2. Vergi/indirim/fiş, çoklu ödeme yöntemi, çoklu ürün/müşteri, memnuniyet, Save ve final sanat ayrı bounded paketler olarak kalsın.

Sonraki ana geliştirme sırası:

- Issue #8: Sipariş, teslimat ve gerçek raf döngüsü.
- Issue #9: Müşteri gezinme, danışmanlık ve kasa.
- Issue #10: Fiziksel PC toplama teknik prototipi.
- Issue #11: Save, journal, migration ve recovery.
- Issue #12: Guardian event/invariant/report iskeleti.
- Issue #13: Baştan sona vertical slice.
- Sonraki fazlar: çalışanlar ve gelişmiş müşteri AI, servis/garanti/iade/ikinci el, dinamik ekonomi, itibar/reklam/rekabet, içerik/sanat/ses, alpha/erişilebilirlik/optimizasyon, Steam Playtest, Windows 1.0 ve en son macOS portu.

Henüz tamamlanmayan önemli alanlar:

- Çok satırlı/çok adetli delivery parcel unpack layout'u ve claim akışı.
- Çoklu slot/palet taşıma arabası ve lojistik ekipmanı.
- Çok katlı veya palet istifi.
- Gelişmiş el modeli/animasyonu.
- Garajın bütününe yayılmış final sanat.
- Orders'ın satış/servis varyantları, Economy ve diğer domain assembly'leri; Catalog/Inventory/Orders event-save entegrasyonu.
- Save/Guardian runtime.
- Steam entegrasyonu.
- Native Windows x64 IL2CPP/DirectX/Steam testi.

## 8. Yaşayan belgeler ve kanıtlar

Yeni bir çalışma şu sırayla başlamalıdır:

1. `PROJECT_BIBLE.md`.
2. `Docs/ProjectBible/00_OKU_BENI.md`.
3. `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md`.
4. Bu belge: `Docs/ProjectBible/11_BIRLESIK_CODEX_PROJE_HAFIZASI.md`.
5. `Docs/DEVELOPER-HANDOFF.md`.
6. `Docs/GITHUB-PROJECT-MAP.md`.
7. İlgili `Docs/ADR-*.md` ve `Docs/Evidence/*.md`.
8. Gerektiğinde [tam Codex geçmişi](../CodexHistory/README.md).

Tam konuşma ve dosya geçmişi:

- [Birleşik Codex geçmişi indeksi](../CodexHistory/README.md).
- [Codex dosya değişiklik envanteri](../CodexHistory/FILE_CHANGE_INVENTORY.md).
- [Git commit ve dosya geçmişi](../CodexHistory/GIT_COMMIT_AND_FILE_HISTORY.md).

## 9. USB ve yedek güvenlik katmanı

Korunan milestone snapshotları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`.
- Issue #35 stacking için 15 Ağustos 2026 tarihli doğrulanmış USB checkpointi yaşayan checkpoint belgesinde kayıtlıdır.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_LOADED_TRANSPORT_CART`; 396 tracked source + 6 evidence, 403 satırlı manifest ve SHA-256/readback doğrulaması geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_CATALOG_INVENTORY`; 428 tracked source + 4 test evidence + source kaydı, 433 satırlı `f481ddfa…49dc9` manifest, tam readback/source checksum ve AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ORDER_RECEIVING`; 449 tracked source + 4 test evidence + source kaydı, 454 satırlı `07480d15…485cff` manifest, tam readback/source checksum ve AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_AUTHORITATIVE_STOCK_FLOW`; source `f20fd17`, 467 tracked source + 4 test/build/runtime evidence + source kaydı, 472 satırlı `5521f869…22a3` manifest, tam readback/source checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_DELIVERY_PARCEL_UNPACKING`; source `756547f`, 471 tracked source + 5 scene/test/build/runtime evidence + source kaydı, 477 satırlı `37f95b3c…58ac` manifest, tam readback/source checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_AUTHORITATIVE_SHELF_OFFER`; source `6ae294e`, 488 tracked source + 5 scene/test/build/runtime evidence + source kaydı, 494 satırlı `a95d8457…de7a` manifest, tam readback/source path+checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_CUSTOMER_BASKET_RESERVATION`; source `109237a`, 498 tracked source + 4 test/build/runtime evidence + source kaydı, 503 satırlı `ff868e4c…20d7` manifest, tam readback/source path+checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_IMMUTABLE_CHECKOUT_SNAPSHOT`; source `0936cc0`, 508 tracked source + 4 test/build/runtime evidence + source kaydı, 513 satırlı `30c1e7fa…16efa` manifest, tam readback/source path+checksum ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ATOMIC_CHECKOUT_FULFILLMENT`; source `80eea8f`, 510 tracked source + 4 test/build/runtime evidence + source kaydı, 515 satırlı `ce72122a…db50b` manifest, 9.373.684 bayt; tam readback/source path+Git-blob ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_DETERMINISTIC_CUSTOMER_VISIT`; source/docs `d163328`, 535 tracked source + 5 test/build/runtime evidence + source kaydı, 541 satırlı `c82fc76d…cfd` manifest, 9.715.834 payload baytı; tam hash/boyut/path readback, 535/535 Git-blob ve forbidden/credential/AppleDouble `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_EXPLAINABLE_SINGLE_OFFER_CUSTOMER_DECISION`; source/docs `8832c13`, 541 tracked source + 4 final test/build/runtime evidence + source kaydı, 546 satırlı `d46e2433…d1b1` manifest, 9.780.828 payload baytı; 546/546 hash/boyut/path readback, 541/541 Git-blob ve forbidden/cache/credential/AppleDouble/sibling sidecar `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_STALE_SAFE_BUY_ACTION_AND_CHECKOUT_NAVIGATION`; source/docs `aa61700`, 547 tracked source + 4 final test/build/runtime evidence + source kaydı, 552 satırlı `05ed8205…e76f6` manifest, 9.902.727 payload baytı; 552/552 hash/boyut/path readback, 547/547 Git-blob ve evidence/forbidden/cache/credential/AppleDouble/sibling sidecar `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_STALE_SAFE_LEAVE_ACTION_AND_OFFER_DECLINED_EXIT`; source/docs `868885a`, 549 tracked source + 4 final test/build/runtime evidence + source kaydı, 554 satırlı `d685de7a…4209` manifest, 10.003.704 payload baytı; 554/554 hash/boyut/path readback, 549/549 Git-blob, 4/4 evidence ve forbidden/cache/credential/AppleDouble/sibling sidecar mismatch `0` kapısı geçti.

Snapshotlara `.git`, Unity cache, build, geçici log, token, parola veya credential eklenmez. Her snapshot manifest ve SHA-256 ile doğrulanır; kaynak Git geçmişinin yerine geçmez.

## 10. Bundan sonraki tek-kanal çalışma protokolü

- Kullanıcıyla proje hakkındaki bütün yeni konuşma ve geliştirme yalnız `PC Shop Empire 3D — ANA GÖREV` içinde yapılır.
- Eski iki görev geçmiş kayıt olarak arşivde kalır; normal geliştirme için yeniden açılmaz.
- Aynı karar kullanıcıya tekrar sorulmadan önce bu belge ve tam konuşma arşivi aranır.
- Küçük ve geri alınabilir teknik kararlar ana görev tarafından uygulanabilir.
- Büyük kapsam değişikliği, ücretli araç, büyük indirme, uygulama kurulumu, dış yayın, destructive işlem veya vizyon değişikliği kullanıcı onayı ister.
- Her bounded paket: salt-okunur repo doğrulaması → kod/test → gerçek Unity test/build/runtime kanıtı → yaşayan belge/ADR/Evidence → küçük commit → private push → CI/Repository Guard → gerekiyorsa USB milestone sırasıyla kapatılır.
- Kullanıcıya ait veya ilişkisiz değişiklikler silinmez, üzerine yazılmaz ya da başka pakete karıştırılmaz.
- Credential, token, parola, özel anahtar ve gizli dosyalar Git, Codex konuşma arşivi veya USB snapshotına alınmaz.
- Kalan kullanım düşükse yeni uzun paket başlatılmaz; en yakın temiz commit sınırında checkpoint bırakılır.

## 11. Hızlı devam cümlesi

Ana görev bir sonraki turda şu anlamla devam etmelidir:

> Issue #49 feature `67d858a`, source/docs `868885a`, iki başarılı Guard, EditMode 298/298, PlayMode 22/22, Mac `leave-action=ok stale-leave-blocked=ok authority-isolated=ok` ve doğrulanmış USB milestone ile kapalı/Done'dır. Epic #9 altında bounded atomik payment/ilk Economy settlement dilimine geç; vergi/indirim/Save sınırlarını karıştırma.

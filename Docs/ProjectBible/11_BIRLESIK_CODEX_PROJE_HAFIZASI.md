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
- Atomik nakit checkout ve ilk Economy settlement feature: `547cf971882239c912d8221f344706afc993a37b`.
- Bounded tek-müşteri danışmanlık ve recommendation gate feature: `846eb5d9912150a6ef3aae9a37678d71348f92a3`.

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
- Current `Browsing` visit için canonical one-per-visit consultation receipt; `2,75 m` range, `24°` focus, LOS ve gerçek `E / Gamepad South` görüşmesi olmadan tek-offer `Buy/Leave` kararı açılmaz.
- Versioned tek-consumer Interact, explicit customer execution order ve owned runtime `InputActionAsset` clone yaşam döngüsü aynı basışın carry/pickup'a sızmasını veya source assetin bozulmasını engeller.

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

- Son doğrulanmış kaynak feature: `92a0f7b814ad5e597d8d4ca033f2e533f618f719`, tree `4150bd36fa65d4043061e5979e08efb502338fc6`; [Repository Guard 31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515) başarılıdır.
- Issue #52 stable `world.checkout-station.garage-001` fiziksel checkout station ekledi. Station pause kapalı, `2,75 m` range, `24°` focus ve raycast LOS gerektirir; RAF A üzerindeki checkout/payment primary action bypass'ı kapalıdır.
- Yalnız exact matching current customer/visit/basket/offer/item/reservation/Buy-action provenance'ı ve `AwaitingCheckout` state'i station'ı yetkilendirir. Stale, foreign, historical veya forged/value-equal zincir bütün authority'lerde no-mutation fail-closed'dur.
- İlk `Mouse Left / Gamepad RT` edge'i immutable checkout snapshotını bir kez üretir; fiyat/currency/unit cost donar. Held/same-frame/replay ödeme değildir; release/repress sonrasındaki ikinci edge exact-cash Economy settlement'ını bir kez üretir.
- Canonical receipt exact settlement/transaction/completion/checkout/customer/payment/currency/amount/COGS/action/line/ledger/time provenance'ını kapılar. Stock projection ve customer fulfillment yalnız matching receipt sonrasında ilerler.
- Customer focus collider'ı trigger yapılarak station çevresindeki fiziksel player/NPC stall'ı kaldırıldı; consultation LOS trigger hedefini görür. Üç ardışık final customer smoke güvenli exit'i kanıtladı.
- EditMode `352/352`, gerçek Input System PlayMode `24/24` geçti; failed/skipped `0`. XML SHA-256 değerleri `c6bd6e4f…ac6d` ve `8c05afec…9230`dur.
- Universal macOS development build `327864494` bayt, Mach-O `x86_64 + arm64`; build log SHA-256 `c9a0780e…69c`, executable SHA-256 `cf66c67f…79b2`dir.
- Apple M4/Metal 1280×720 runtime markerı `garage-physical-checkout-station-r21-v1`dir. Stock r4 ve customer r6/r7/r8 smoke station access, shelf bypass, checkout-start, cash-payment, receipt/Economy/ledger, authority isolation, stock projection ve customer safe-exit kapılarını geçti.
- `GarageGraybox.unity` `1397931` bayt, SHA-256 `509e6c25…d3fe`dir. Primitive checkout terminali, müşteri ve büyük diagnostic/status textleri final POS/karakter/UI değildir.
- Source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`, tree `6d73d5ac6d675733c939f181d087da3aef90f496` ve [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) başarılıdır. USB 584/584 manifest, 576/576 exact Git source ve 7/7 evidence kapısını geçti. Issue #52 acceptance `17/17`, kapalı/Done; parent Epic #9 kapalı/Done'dır.

## 7. Sıradaki işler ve bağımlılık sırası

Issue #52 kaynak/test/build/runtime/CI/USB ve Issue metadata zinciri tamamlandı; acceptance `17/17`, kapalı/Done. Parent Epic #9 geniş kabulü de kapalı/Done'dır. Bundan sonraki bounded geliştirme Epic #10 altında ilk fiziksel PC assembly dilimidir; tek açık kasa/tek component/tek slot dışına, tam katalog/Inventory genişlemesi/Save/final sanat kapsamına gizlice büyütülmez.

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
- Orders'ın satış/servis varyantları, ilk exact-cash satış settlement'ı ötesindeki Economy kapsamı ve diğer domain assembly'leri; Catalog/Inventory/Orders/Economy event-save entegrasyonu.
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

## 8.1 Issue #53 authoritative motherboard seating checkpoint'i

- Epic #10'un ilk child paketi [Issue #53](https://github.com/cixanla/PC-Shop-Empire-3D/issues/53) ile sınırlandı: tek açık kasa, tek serialized `MicroAtx` anakart, tek doğru slot ve yalnız `SeatedUnsecured` sonucu.
- Feature `582a3cf3e81a2905e39148065bd5f6c7e35bbc06`, source/docs `8c6abe45d1f9b6c72def9b686b9c81bf3704d10d`, tree `387bcba701b8a959681e92bf29dc48a4d09f0ab7` ve başarılı [Repository Guard 31905540378](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378); `PSE.Assembly` mevcut Catalog/Inventory authority'lerini kullanır, managed Workbench dışında shadow authority yoktur.
- Attach/detach exact item identity, immutable receipt, replay/conflict, revision ve failure no-mutation sözleşmelerini taşır. World projection domain transaction'dan sonra değişir; failed drop/recovery aynı fiziksel instance ve last-safe pose'u korur.
- GarageGraybox açık kasa/keyed tray/standoff/anakart graybox'ını içerir. Solver pause/range/focus/LOS/orientation/support/obstruction kapılarını deterministic uygular; preview ve commit pozu aynıdır.
- Primary+Drop aynı frame'de yalnız seat-mode geçişi üretir. Gerçek Input System keyboard/mouse ve gamepad akışları, dynamic prompt ve release–repress ile testlidir.
- Final EditMode `394/394`, PlayMode `26/26`, Universal macOS `328020817` bayt ve Apple M4/Metal 1280×720 `garage-motherboard-seating-r22-v1 assembly-flow=ok ... recovery=ok` başarılıdır.
- Bu tarihsel USB gecikmesi 16 Ağustos 2026'da Issue #53–#55 birleşik milestone'uyla kapandı: source `07364b79`, 640 tracked source + 12 final evidence + source kaydı, 653/653 readback ve `0b5f3c61…aaba9e` manifesti; bütün güvenlik mismatch sayaçları `0`.
- Bu tarihsel checkpointin sonraki adımı Issue #54 motherboard fastener secure/unsecure idi; aşağıdaki güncel kayıtla tamamlandı.

## 8.2 Issue #54 deterministic motherboard fastener checkpoint'i

- Epic #10'un ikinci child paketi [Issue #54](https://github.com/cixanla/PC-Shop-Empire-3D/issues/54) ile tek Assembly-owned fastener, tek visible screwdriver ve `SeatedUnsecured ↔ SeatedSecured` geçişine sınırlandı.
- Feature `b6812394f835d64d5bf8422d8e7996ec433cd0f1`, tree `192f9d8f1334cf9e1ff1d21382c44a847bbfa7e6`; secure/unsecure exact receipt, historical replay, Inventory revision izolasyonu ve secured presentation+authority detach gate'i ekledi.
- Source/docs `7cec7cc4b6fd80997acd0dc2d6943ef08850f4ad` ve [Repository Guard 31909940414](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31909940414) başarılıdır; acceptance `18/18`, Issue kapalı/Roadmap `Done`dur.
- GarageGraybox r23 captive screw/cross recess, solid focus target, screwdriver ve plate'e bağlı tek satır metin taşır. Solver pause/range/focus/LOS/obstruction fail-closed'dur; screw/tool pose yalnız projection'dır ve drift authority'yi mutate etmeden invariantı bozar.
- Valid/blocked fastener context Primary/Interact/Drop edge'lerinin tek sahibidir. Gerçek keyboard/mouse ve gamepad PlayMode testleri dynamic prompt, same-frame blocker drain, pause co-edge ve release–repress sözleşmesini taşır.
- Final EditMode `411/411`, PlayMode `29/29`, Universal macOS `328057977` bayt ve Apple M4/Metal 1280×720 `garage-motherboard-fastener-r23-v1 assembly-flow=ok ... secure-delayed-replay=ok ... detach-authority-blocked=ok ... recovery=ok` başarılıdır.
- Issue #54 final kanıtları aynı doğrulanmış Issue #53–#55 birleşik USB milestone'undadır; 12/12 evidence ve 640/640 exact Git source eşliği geçti.
- Bu tarihsel checkpointin sonraki adımı Issue #55 CPU socket seating + retention idi; aşağıdaki güncel kayıtla doğrulandı.

## 8.3 Issue #55 deterministic CPU socket seating ve retention checkpoint'i

- Epic #10'un üçüncü child paketi [Issue #55](https://github.com/cixanla/PC-Shop-Empire-3D/issues/55) ile tek canonical serialized CPU, tek capacity-1 socket ve tek retention mechanism akışına sınırlandı.
- Feature `99cadad414789d3f440e08cc6e42e727c2b7a2ad`, tree `fea116af021d66efb31b96b4f3e7523929f8b8ad`; atomik managed container pair claim, four-operation Assembly authority/receipt lineage, secured-host gate ve same-instance recovery ekledi.
- Source/docs `d9d0722a1592a83b89938529f72b3170f17e94eb` ve [Repository Guard 31914774370](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914774370) başarılıdır; acceptance `20/20`, Issue kapalı/Roadmap `Done`dur.
- GarageGraybox r24 notched LGA-style package, ayrı substrate/IHS materyali, triangular mating key, simetrik aperture load plate ve retention lever taşır. Presentation authority değildir; drift invariantı fail-closed'dur ve `21/11/1` render/physics/text bütçesi korunur.
- Gerçek keyboard/mouse ve gamepad PlayMode testleri guided mode, keyed quarter-turn rejection, seat/retain/open/remove, CPU-installed motherboard detach gate, dynamic compact HUD, co-edge/pause drain ve recovery'yi taşır.
- Final EditMode `430/430`, PlayMode `31/31`, Universal macOS `328144884` bayt ve Apple M4/Metal 1280×720 `garage-cpu-socket-retention-r24-v1 cpu-socket-flow=ok ... keyed-orientation=ok retained-remove-gate=ok replay=ok identity=stable` başarılıdır.
- Issue #53–#55 birleşik USB milestone'u `2026-08-16_STAGE_B_PHYSICAL_ASSEMBLY_MOTHERBOARD_FASTENER_AND_CPU_SOCKET_RETENTION` adıyla doğrulandı: source `07364b79`, 653 satırlı `0b5f3c61…aaba9e` manifest, 13.500.119 payload baytı; hash/boyut/yol, Git source, evidence, forbidden, credential ve AppleDouble mismatch `0`.
- Sonraki bounded Epic #10 adımı yalnız dual-latch DIMM/RAM seating akışıdır. GPU/cooler/storage, tam build/benchmark, genel Inventory revision-max hardening, Save/Guardian ve Windows/Steam ayrı kalır.

## 8.4 Issue #56 deterministic single DIMM seating ve dual-latch retention checkpoint'i

- Epic #10'un dördüncü child paketi [Issue #56](https://github.com/cixanla/PC-Shop-Empire-3D/issues/56) ile tek canonical serialized DDR5 UDIMM, tek immutable A2/Channel A/Bank 2 topology ve tek dual-latch retention aggregate akışına sınırlandı.
- Feature `7482fc9aabe6a3a27ba41730db12c60e18aac515`, tree `291b23cb2fe774cb44ba71b26716d7c8131370a2`; atomik managed triple claim, four-operation Assembly authority/receipt lineage, secured-host gate, installed-DIMM detach gate ve same-instance recovery ekledi.
- Source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055) ve [31920258176](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920258176) başarılıdır.
- GarageGraybox r25 dört materyalli UDIMM package, matching notch, hard-surface A2 bed/rail ve iki ayrı latch pivotu taşır. Close sol→sağ, open sağ→sol görünür sıradadır; tek Assembly revision/receipt korunur ve `25/13/1` render/physics/text bütçesi sabittir.
- Gerçek keyboard/mouse ve gamepad PlayMode testleri pickup, guided mode, yalnız 0°↔180° keyed toggle, seat, dual-latch close/open, retained remove, DIMM-installed motherboard detach, dynamic compact HUD, co-edge/pause drain ve recovery'yi taşır.
- Final EditMode `461/461`, PlayMode `33/33`, Universal macOS `328268700` bayt ve Apple M4/Metal 1280×720 `garage-dimm-dual-latch-r25-v1 dimm-flow=ok ... keyed-orientation=ok latch-order=ok replay=ok authority-isolated=ok identity=stable recovery=ok` başarılıdır.
- Ayrı `2026-08-16_STAGE_B_DETERMINISTIC_SINGLE_DIMM_DUAL_LATCH_RETENTION` USB milestone'u 663 tracked source + 4 final evidence + source kaydıyla 668/668 readback, `8658b50a…c50` manifest ve 12.073.868 payload baytıyla doğrulandı; bütün güvenlik/AppleDouble mismatch sayaçları `0`dır. USB metadata `17af550`, Guard `31920923402`, acceptance `21/21`, Issue `Completed` ve Roadmap `Done`dur.
- Sonraki bounded Epic #10 adayı yalnız tek M.2 2280 NVMe SSD seating + captive retention screw akışıdır. İkinci storage yolu, SATA/RAID, GPU/cooler, tam build/benchmark, genel Inventory hardening, Save/Guardian ve Windows/Steam ayrı kalır.

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
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ATOMIC_CASH_CHECKOUT_AND_INITIAL_ECONOMY_SETTLEMENT`; source/docs `aea6e2b`, 566 tracked source + 5 final test/build/runtime evidence + source kaydı, 572 satırlı `b3168162…ecf8` manifest, 10.227.122 payload baytı; 572/572 hash/boyut/path readback, 566/566 Git-blob, 5/5 evidence ve forbidden/cache/credential/AppleDouble/sibling sidecar mismatch `0` kapısı geçti.
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_BOUNDED_SINGLE_CUSTOMER_CONSULTATION_AND_RECOMMENDATION_GATE`; source/docs `f9bc38d`, 572 tracked source + 5 final evidence + source kaydı, 578 satırlı `f8d3ce98…ccf20` manifest, 10.366.388 payload baytı; 578/578 readback, 572/572 Git-blob, 5/5 evidence ve güvenlik mismatch `0` kapısı geçti.

Snapshotlara `.git`, Unity cache, build, geçici log, token, parola veya credential eklenmez. Her snapshot manifest ve SHA-256 ile doğrulanır; kaynak Git geçmişinin yerine geçmez.

## 10. Bundan sonraki tek-kanal çalışma protokolü

## 9.1 Issue #57 deterministic single M.2 NVMe seating checkpoint'i

- Epic #10'un beşinci bounded child paketi [Issue #57](https://github.com/cixanla/PC-Shop-Empire-3D/issues/57) ile tek canonical serialized M.2 2280 NVMe, tek M-key/2280 standoff ve motherboard-owned captive screw akışına sınırlandı.
- Feature `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`, tree `1aedb833983df256c500c6a1815b075fa29c254c`; dört-container atomic claim, `EmptyOpen ↔ StorageDeviceSeatedUnsecured ↔ StorageDeviceSecured`, exact receipt replay, host-detach gate ve same-instance recovery ekledi.
- GarageGraybox r26'da 18° guided pose, flat seated pose, PCB/controller/NAND/label/gold M-key contacts, standoff ve captive screw görünürdür. Presentation authority değildir; generic placement/stack/cart yolu fail-closed'dur.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/obstruction/secured-remove no-mutation kapılarıyla testlidir.
- Final EditMode `490/490`, PlayMode `35/35`, Universal macOS `328362356` bayt ve Apple M4/Metal exact storage smoke başarılıdır. İkinci M.2/SATA/RAID, tam benchmark ve diğer PC bileşenleri ayrı kalır.
- Source/docs `6e0627e`, Guard `31970813717` ve 689/689 `19da758c…21b8` USB readback başarılıdır; Issue #57 ve Roadmap `Done`dur.

## 9.2 Issue #58 deterministic single air-cooler seating checkpoint'i

- Epic #10'un altıncı bounded child paketi [Issue #58](https://github.com/cixanla/PC-Shop-Empire-3D/issues/58) ile tek canonical serialized LGA1700 top-down air cooler, pre-applied single-use TIM ve stable four-point retention akışına sınırlandı.
- Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`; five-container atomic claim, `EmptyOpen ↔ CoolerSeatedUnsecured ↔ CoolerRetained`, exact receipt replay, TIM consumption, CPU-retention/motherboard-detach host gates ve same-instance recovery ekledi.
- GarageGraybox r27'de cold plate/TIM yüzeyi, fin stack, fan, bracket ve dört retention point görünürdür. Retain `1→3→2→4`, release ters sıradadır; presentation authority değildir ve generic placement/stack/cart yolu fail-closed'dur.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/RAM-clearance/obstruction/retained-remove/consumed-TIM no-mutation kapılarıyla testlidir.
- Final EditMode `521/521`, PlayMode `38/38`, Universal macOS `328534723` bayt ve aktif Apple Silicon/Metal 1280×720 exact cooler smoke başarılıdır. Source/docs `2e848e3`, Guard `32591206866` + `32591381804`; USB-erteleme kapanış metadatası `fce6bfa`, Guard `32593034745`; acceptance `19/19`, Issue/Roadmap `Done`dur.
- Fiziksel USB kullanıcı talimatıyla ertelendi; 717/717 doğrulanmış `f7b2b9bafee9529d95431bbc90914ba51ab24e01de9a0d5d77a53f26cb5626a5` yerel staging hazırdır. Kullanıcı USB'nin bağlandığını söyleyene kadar USB sorgulanmaz ve gameplay geliştirmesi sürer.
- Ayrı paste/reapplication, liquid cooling, GPU/PSU/cabling, tam benchmark ve Windows/Steam ayrı bounded kapılardır.

## 9.3 Issue #59 deterministic single PCIe x16 graphics-card seating checkpoint'i

- Epic #10'un yedinci bounded child paketi [Issue #59](https://github.com/cixanla/PC-Shop-Empire-3D/issues/59) ile canonical Northstar A60 ProductId'sini kullanan ayrı exact serialized assembly GPU item'ı, tek PCIe x16 slot, slot latch, rear bracket ve bracket fastener akışına sınırlandı; shadow SKU oluşturulmadı.
- Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`; six-container atomic claim, `EmptyOpen ↔ GraphicsCardSeatedUnsecured ↔ GraphicsCardRetained`, exact receipt replay, installed-GPU motherboard-detach gate ve same-instance recovery ekledi.
- GarageGraybox r28'de dual-fan shroud, PCB, PCIe contacts, rear bracket, slot latch ve screw görünürdür. Presentation authority değildir; generic placement/stack/cart yolu fail-closed'dur.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/interface/chassis-clearance/cooler-clearance/obstruction/duplicate-seat/retained-remove no-mutation kapılarıyla testlidir.
- Final EditMode `548/548`, PlayMode `43/43`, Universal macOS `328781520` bayt ve aktif Apple Silicon/Metal 1280×720 exact GPU smoke başarılıdır. Feature `1b29ad2`, source/docs `a5bbca4`; Guard `32599710154` + `32600012769`; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent Epic #10 açık/In Progress kalır.
- Fiziksel USB kullanıcı talimatıyla ertelendi; Issue #58'in 717/717 staging'i korunur, Issue #59 için henüz milestone/readback iddiası yoktur. Kullanıcı USB'nin bağlandığını söyleyene kadar USB sorgulanmaz ve gameplay geliştirmesi sürer.
- PSU, PCIe power cabling, alternate GPU dimensions/slots, tam benchmark ve Windows/Steam ayrı bounded kapılardır.

## 9.4 Issue #60 deterministic single ATX PS/2 PSU seating checkpoint'i

- Epic #10'un sekizinci bounded child paketi [Issue #60](https://github.com/cixanla/PC-Shop-Empire-3D/issues/60) ile tek canonical serialized ATX PS/2 PSU, chassis-owned bay/rear mount ve dört distinct fastener akışına sınırlandı; shadow SKU veya ikinci authority oluşturulmadı.
- Feature `f998d7d1c400c9328afa226f0727e6591c02d4e2`, tree `78d62c46354cda45422ca947df10ba9d6823b7c9`; authored-clearance fix `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`, tree `a15865346f52b6b39d84cec49c70babbc6550b89`; seven-container atomic claim, `EmptyOpen ↔ PowerSupplySeatedUnsecured ↔ PowerSupplyRetained`, exact receipt replay, alternate-order isolation ve same-instance recovery ekledi.
- GarageGraybox r29'da PSU housing, fan/grille, filtered floor intake, AC inlet, rocker switch, disconnected modular panel, rear plate ve dört screw görünürdür. Production clearance gerçek dört authored chassis collider'ına bağlıdır; support ve future cable blocker'ları ayrıdır.
- Gerçek keyboard/mouse ve gamepad akışı, dynamic compact HUD, pause/co-edge drain, wrong-orientation/interface/support/rear-plane/chassis-clearance/obstruction/duplicate-seat/retained-remove no-mutation kapılarıyla testlidir.
- Final EditMode `577/577`, PlayMode `47/47`, Universal macOS `328937592` bayt ve aktif Apple Silicon/Metal 1280×720 exact PSU smoke başarılıdır. Feature `f998d7d`, fix `b6c3ff8`, source/docs `4939a04`; Guard `32606958882` + `32607437408` + `32607886160`; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent Epic #10 açık/In Progress kalır.
- Exact source-plus-evidence staging 770 Git source + 4 evidence + source kaydıyla 775/775 readback, 770/770 Git-blob, 14.729.691 bayt ve `705784c6…53d4` manifestiyle doğrulandı. macOS beklenen harici fiziksel USB'yi mount etmedi; yanlış volume'a yazılmadı ve fiziksel milestone/readback iddiası yoktur.
- ATX/EPS/PCIe/SATA cabling, electrical power-on, wattage/headroom, POST/BIOS/OS, completed benchmark, final art ve Windows/Steam ayrı bounded kapılardır.

## 9.5 Issue #61 deterministic single ATX24 split-PSU cable routing checkpoint'i

- Epic #10'un dokuzuncu bounded child paketi [Issue #61](https://github.com/cixanla/PC-Shop-Empire-3D/issues/61) ile tek canonical serialized ATX24 power cable, typed PSU 18-pin + PSU 10-pin + motherboard 24-pin endpoint'leri ve üç stable ordered route waypoint'ine sınırlandı; connector child'lar ayrı ürün/Inventory item değildir.
- Feature `1fc29f13171925c2445eaa7334158e0f058e76a5`, tree `d265332f1d6655639e55db31f9b5a11e3d177f49`; eight-container atomic claim, capacity-one CableRoute, `Loose ↔ Routed`, exact route/unroute receipt history/replay, host lineage ve same-instance recovery ekledi.
- GarageGraybox r30'da tek kinematic cable root, üç visible connector/latch/key child ve jointsiz authored branch/trunk route görünürdür. Route focus görünür motherboard connector üzerinden deliberate unroute'a izin verirken gerçek chassis obstruction fail-closed kalır.
- Gerçek keyboard/mouse ve gamepad route mode, iki keyed orientation, dynamic compact HUD, pause/co-edge drain; wrong-key/host/range/focus/LOS/clearance/duplicate/dependent-detach/generic-bypass failure yollarıyla testlidir.
- Final EditMode `589/589`, PlayMode `49/49`, Universal macOS `329082160` bayt ve aktif Apple M1/Metal 1280×720 exact cable smoke başarılıdır. Feature Guard `32613813494`; source/docs `52795b66fee1eb933d0d9c4ff8cbd7eca512d718`, tree `d0bdb7bd39bb09a27565ed1d4a0fd77e22b7dfa3` ve Guard `32614187494` başarılıdır.
- Ayrı `2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_ATX24_SPLIT_PSU_CABLE_ROUTING` USB milestone'u atomik adlandırmayla sabitlendi: iki tam 801/801 hash/boyut/yol readback, 796/796 exact Git source, 4/4 evidence, 15.237.662 payload baytı ve `f2145ecb…1365` manifest; bütün güvenlik/AppleDouble mismatch sayaçları `0`dır. USB metadata `f9a5da8`, Guard `32632615041`, acceptance `20/20`; Issue #61 `CLOSED/COMPLETED`, Roadmap `Done`, parent #10 açık/In Progress'tir.
- EPS/CPU, PCIe/GPU, SATA/Molex/fan/front-panel/data/RGB cabling, electrical power-on, wattage/headroom, POST/BIOS/OS, completed benchmark, free-rope physics, final art ve Windows/Steam ayrı bounded kapılardır.

## 9.6 Issue #62 deterministic single EPS12V/CPU power cable routing başlangıcı

- [Issue #62](https://github.com/cixanla/PC-Shop-Empire-3D/issues/62) parent Epic #10 altında `OPEN/In Progress`, Assembly/P0/Critical olarak açıldı ve #61'e bağlandı.
- En küçük kapsam tek canonical serialized EPS12V/CPU 8-pin cable, iki typed/keyed endpoint, üç ordered waypoint, ayrı capacity-one `CpuPowerCableRoute` ve retained PSU + secured motherboard + retained CPU lineage'ında exact reversible routing'dir.
- ATX24 ile EPS12V product/item/container/receipt/revision/route identity tam izole kalır. Dokuz-container claim all-or-none olacak; mevcut prepared serialized transfer kullanılacak, genel Inventory authority refactoru yapılmayacaktır.
- Hedef scene marker `garage-eps12v-cpu-power-cable-routing-r31-v1`; exact native marker `GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE ... identity=stable recovery=ok` sözleşmesidir.
- Electrical power-on, completed benchmark, diğer kablo aileleri, Save/Guardian, free-rope physics, final art ve Windows/Steam ayrı bounded kapılardır.

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

> Issue #61 feature `1fc29f1`, source/docs `52795b6`, USB metadata `f9a5da8`, Guard `32613813494` + `32614187494` + `32632615041`, EditMode 589/589, PlayMode 49/49, Universal Mac `329082160` bayt, aktif Apple M1/Metal exact r30 cable smoke ve iki tam USB 801/801 + 796/796 readback ile kapandı. Acceptance 20/20, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent #10 açık/In Progress'tir. Aktif Issue #62 yalnız tek canonical EPS12V/CPU cable, ayrı identity/container/receipt/revision, üç ordered waypoint, real input ve same-instance recovery akışını electrical power-on ve completed benchmarktan ayrı uygulamalıdır.

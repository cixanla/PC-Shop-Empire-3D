# Developer Handoff — Başka Bilgisayarda veya Yeni Geliştiriciyle Devam

Bu belge, projeyi hiç bilmeyen bir geliştiricinin mevcut sağlam checkpoint'ten güvenle devam etmesi içindir.

## 1. Önce okuyun

1. Root `PROJECT_BIBLE.md`.
2. `Docs/ProjectBible/00_OKU_BENI.md`.
3. `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md`.
4. `Docs/ProjectBible/11_BIRLESIK_CODEX_PROJE_HAFIZASI.md`.
5. Çalışacağınız alanın ayrıntılı Game Design Bible/Yol Haritası bölümü.
6. `CONTRIBUTING.md` ve `Docs/REPOSITORY-GOVERNANCE.md`.
7. [GitHub Development Roadmap Project](https://github.com/users/cixanla/projects/2) içindeki atanmış issue ve kabul ölçütü.

Önceki Codex görevlerindeki tam kullanıcı/Codex yazışmaları veya tarihsel dosya değişiklikleri gerektiğinde `Docs/CodexHistory/README.md` indeksinden bulunur. Normal geliştirme yalnız `PC Shop Empire 3D — ANA GÖREV` adlı tek Codex görevi üzerinden sürdürülür.

### Güncel checkpoint — Issue #59

- Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`, canonical Northstar A60 ProductId'sini koruyan ayrı serialized assembly GPU item'ını stable PCIe x16 slot/latch/rear-bracket/fastener topology ile GarageGraybox r28'e ekler.
- Inventory authority altı managed container'ı atomik claim eder. Assembly supported runtime'da yalnız `EmptyOpen ↔ GraphicsCardSeatedUnsecured ↔ GraphicsCardRetained` state'ini, exact receipt lineage/replay'i, installed-card motherboard-detach gate'ini ve no-mutation failure yollarını kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile guided mode, keyed 0°/180° orientation, PCIe seat, görünür slot-latch/rear-bracket retain-unretain ve same-instance remove/recovery akışını oynar. Generic placement/stack/cart bypass'ı kapalıdır.
- Final EditMode `548/548`, PlayMode `43/43`; Universal macOS build `328781520` bayt ve aktif Apple Silicon/Metal 1280×720 exact r28 GPU smoke başarılıdır. Runtime makineyi Apple M1 olarak tanımlamıştır.
- Feature `1b29ad2` private `main`e push edilmiş ve Guard `32599710154` başarılıdır. Source/docs Guard ile acceptance `20/20` ve Issue/Roadmap `Done` final metadata adımında kaydedilecektir.
- Fiziksel USB kullanıcı talimatıyla ertelendi. Issue #58'in 717/717 yerel staging'i korunur; Issue #59 için henüz fiziksel USB milestone/readback iddiası yoktur. Kullanıcı USB'yi yeniden bağladığını söyleyene kadar USB sorgulanmaz ve geliştirme durmaz.
- Ayrıntı: `Docs/ADR-0037-DETERMINISTIC-SINGLE-PCIE-X16-GRAPHICS-CARD-SEATING-AND-REAR-BRACKET-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-PCIE-X16-GRAPHICS-CARD-SEATING-AND-REAR-BRACKET-RETENTION-CHECKPOINT-2026-08-22.md`.
- PSU, PCIe power cabling, alternate card/slot biçimleri, tam benchmark, final art ve native Windows/Steam ayrı kapılardır.

### Önceki checkpoint — Issue #58

- Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`, tek canonical serialized LGA1700 top-down air cooler'ı stable slot/bracket/four-point topology ile GarageGraybox r27'ye ekler.
- Inventory authority beş managed container'ı atomik claim eder. Assembly supported runtime'da yalnız `EmptyOpen ↔ CoolerSeatedUnsecured ↔ CoolerRetained` state'ini, exact receipt lineage/replay'i, single-use pre-applied TIM'i, installed-cooler host gates'i ve no-mutation failure yollarını kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile guided mode, iki keyed orientation, seat, görünür `1→3→2→4` retain/ters release ve same-instance remove/recovery akışını oynar. Generic placement/stack/cart bypass'ı kapalıdır.
- Final EditMode `521/521`, PlayMode `38/38`; Universal macOS build `328534723` bayt ve aktif Apple Silicon/Metal 1280×720 exact r27 cooler smoke başarılıdır. Yeni makine runtime'da Apple M1 olarak tanımlanmıştır.
- Feature `e2f10a2`, source/docs `2e848e3` ve USB-erteleme kapanış metadatası `fce6bfa` private `main`e push edilmiştir; Guard `32591206866`, `32591381804` ve `32593034745` başarılı, acceptance `19/19`, Issue/Roadmap `Done`dur.
- Fiziksel USB yeni Mac tarafından algılanmadığı için kullanıcı talimatıyla ertelendi. 712 exact Git source + 4 evidence + source kaydı içeren 717/717 yerel staging ve `f7b2b9bafee9529d95431bbc90914ba51ab24e01de9a0d5d77a53f26cb5626a5` manifest hazırdır; kullanıcı USB'yi yeniden bağladığını söyleyene kadar USB sorgulanmaz ve geliştirme durmaz.
- Ayrıntı: `Docs/ADR-0036-DETERMINISTIC-SINGLE-AIR-COOLER-SEATING-AND-FOUR-POINT-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-AIR-COOLER-SEATING-AND-FOUR-POINT-RETENTION-CHECKPOINT-2026-08-22.md`.

### Önceki checkpoint — Issue #57

- Feature `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`, tree `1aedb833983df256c500c6a1815b075fa29c254c`, tek canonical serialized M.2 2280 NVMe'yi stable M-key slot, 2280 standoff ve motherboard-owned captive screw ile GarageGraybox r26'ya ekler.
- Inventory authority dört managed container'ı atomik claim eder. Assembly yalnız `EmptyOpen ↔ StorageDeviceSeatedUnsecured ↔ StorageDeviceSecured` state'ini, exact receipt lineage/replay'i, installed-storage detach gate'ini ve no-mutation failure yollarını kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile guided 18° insertion, iki keyed orientation, seat, captive screw secure/unsecure ve same-instance remove/recovery akışını oynar. Generic placement/stack/cart bypass'ı kapalıdır.
- Final EditMode `490/490`, PlayMode `35/35`; Universal macOS build `328362356` bayt ve Apple M4/Metal exact r26 storage smoke başarılıdır.
- Source/docs `6e0627e` ve Repository Guard `31970813717` başarılıdır. Ayrı USB milestone exact source + 4 evidence + source kaydıyla 689/689 readback, `19da758c…21b8` manifest ve AppleDouble `0` doğrulamasını geçti; Issue #57 ve Project `Done`dur.
- Ayrıntı: `Docs/ADR-0035-DETERMINISTIC-SINGLE-M2-NVME-SEATING-AND-CAPTIVE-SCREW-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-M2-NVME-SEATING-AND-CAPTIVE-SCREW-RETENTION-CHECKPOINT-2026-08-16.md`.
- İkinci M.2/SATA/RAID, tam benchmark, kalan PC parçaları, final art ve native Windows/Steam ayrı kapılardır.

### Önceki checkpoint — Issue #56

- Feature commit `7482fc9aabe6a3a27ba41730db12c60e18aac515`, tree `291b23cb2fe774cb44ba71b26716d7c8131370a2` deterministic single-DIMM seating + dual-latch retention dilimini taşır; source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962`; USB metadata `17af550856e8bca288ed5c17924bc82586c76c27`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055), [31920258176](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920258176) ve [31920923402](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920923402) başarılıdır.
- `PSE.Assembly` stable memory slot/retention/channel/bank identity, `EmptyOpen ↔ MemoryModuleSeatedOpen ↔ MemoryModuleRetained` state'i, four-operation immutable receipt/historical replay fold'u ve installed-DIMM motherboard detach gate'ini taşır.
- Inventory atomik managed Workbench + ProcessorSocket + capacity-1 MemorySlot triple claim kullanır. Seat/remove exact serialized custody'yi değiştirir; close/open Inventory revision'ını değiştirmez ve bütün failure yolları no-mutation kalır.
- GarageGraybox marker'ı `garage-dimm-dual-latch-r25-v1`dir. Tek canonical DDR5 UDIMM; dört materyalli PCB/chip/spreader/notch, matching A2 slot rail ve iki ayrı latch pivotuyla görünürdür.
- Gerçek Input System guided mode/yalnız 0°↔180° keyed toggle/seat/dual-latch close-open/remove akışı; dynamic keyboard/gamepad prompt, compact HUD ownership, pause/co-edge drain ve release–repress sözleşmeleri testlidir. Mode kapalıyken ghost veya seat PhysX sorgusu çalışmaz.
- Final EditMode `461/461`, PlayMode `33/33`; Universal macOS build `328268700` bayt ve Apple M4/Metal 1280×720 exact DIMM smoke başarılıdır.
- Ayrıntı: `Docs/ADR-0034-DETERMINISTIC-SINGLE-DIMM-SEATING-AND-DUAL-LATCH-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-DIMM-SEATING-AND-DUAL-LATCH-RETENTION-CHECKPOINT-2026-08-16.md`.
- Ayrı SHA-256 USB milestone 663 tracked source + 4 final evidence + source kaydıyla 668/668 readback, `8658b50a…c50` manifest ve 12.073.868 payload baytıyla doğrulandı; güvenlik/AppleDouble mismatch `0`. Acceptance `21/21`, Issue `Completed` ve Project `Done`dur.
- Sonraki bounded Epic #10 adayı yalnız tek M.2 2280 NVMe SSD seating + captive retention screw akışıdır.

## 2. Gereken temel araçlar

- Git.
- Unity Hub.
- Unity Editor `6000.3.21f1`, hedef bilgisayarın native mimarisi.
- URP/paketler repository `Packages/manifest.json` üzerinden çözülür.
- Windows final doğrulaması için gerçek Windows x64 host ve ileride IL2CPP/C++ toolchain.
- IDE serbesttir; generated `.sln`/`.csproj` commit edilmez.

Blender, Steamworks SDK, ücretli asset/tool, telemetry SDK ve Apple signing araçları mevcut checkpoint için gerekli değildir; ayrı kapı olmadan kurulmaz.

## 3. Clone ve ilk doğrulama

```bash
git clone https://github.com/cixanla/PC-Shop-Empire-3D.git
cd PC-Shop-Empire-3D
git switch main
./Tools/verify-repository.sh
git status --short
```

Beklenen durum:

- Repo guard başarılı.
- Çalışma ağacı temiz.
- `ProjectSettings/ProjectVersion.txt` Unity `6000.3.21f1` gösterir.
- Legacy manifest 26/26 dosyayı doğrular.

## 4. Unity'yi açma

Unity Hub içinde **Add/Open project from disk** ile clone edilen repo kökünü seçin. `Assets`, `Packages` ve `ProjectSettings` aynı kökte görünmelidir.

İlk açılışta `Library` yeniden üretileceği için import sürebilir; bu klasör Git'e eklenmez. Paket çözümleme sırasında keyfi sürüm yükseltmeyin.

## 5. Test baseline

Unity Test Runner ile Edit Mode ve Play Mode testlerinin tamamını çalıştırın. Son sağlam baseline:

- Edit Mode `461/461` passed.
- Play Mode `33/33` passed.
- `0` failed.
- `0` skipped.

macOS batch örneği:

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath "$PWD" \
  -runTests -testPlatform EditMode \
  -testResults "$PWD/../TestResults/editmode.xml" \
  -logFile "$PWD/../TestResults/editmode.log"
```

Windows'ta Unity executable yolu kurulum dizinine göre değiştirilir. Test çalıştırırken Unity 6 Test Runner'ın tamamlanmasını bekleyin; `-quit` parametresini eklemek bu projedeki doğrulanmış batch akışında test başlamadan kapanmaya yol açmıştır.

## 6. Güncel kod sınırı

Tamamlanan saf Core sözleşmeleri:

- `PSE.Core` assembly, Unity/Editor referansı yok.
- `StableId<TScope>` ve canonical kimlik doğrulaması.
- `Failure` ve `OperationResult`.
- `SimulationDuration`, `SimulationTimestamp`, pause destekli `SimulationClock`.
- `IDomainEvent`, stable type/ID, one-based sequence ve immutable envelope.
- `pcg32-xsh-rr-64-32-v1`, official golden vector, raw state snapshot/restore ve bias'sız bounded integer.
- `sha256-framed-be-pcg32-v1`, canonical root seed, stable domain/context stream derivation ve reload-reroll engeli.
- Event correlation/direct-causation, global FIFO ve breadth-first nested enqueue uygulayan bounded in-memory dispatcher.
- `PSE.Catalog` assembly: immutable product definition, stable product/category kimliği, serialized/batch tracking policy, doğrulanmış görünür ad ve bounded garanti.
- `PSE.Inventory` assembly: authoritative serialized item/batch/container kayıtları, immutable currency + integer minor-unit `InventoryUnitCost`, unit capacity, atomik transfer, claim reservation, prepared consumption, release/consume, deterministic sorgu, revision ve invariant audit.
- Catalog yalnız Core; Inventory yalnız Core + Catalog; Orders Core + Catalog + Inventory; Actors yalnız Core + Catalog; Retail Core + Catalog + Inventory + Actors; Economy ise downstream Core + Inventory + Retail referanslıdır. `Retail → Actors` tek yönlüdür; Retail/Inventory/Orders Economy'ye ters referans taşımaz ve bu domain assembly'leri Unity/Editor bağımlılığı taşımaz.
- `PSE.Orders` assembly: stable purchase order/supplier/delivery kimliği, exact manifest, alış maliyeti provenance'ı, `Placed → Confirmed → InTransit → Arrived → Accepted` lifecycle ve atomik receiving kabulü.
- `PSE.Retail` assembly: stable shelf-offer, customer basket reservation, immutable checkout ve internal checkout completion authority'leri; exact serialized item/claim/maliyet bağları, owner/revision-bound prepared planlar, deterministic sorgu/snapshot, idempotent komutlar, drift denetimi ve failure no-mutation.
- `PSE.Economy` assembly: exact-cash checkout settlement, stable receipt ve immutable ledger transaction/entry kayıtları; Cash, SalesRevenue, COGS ve InventoryAsset hesaplarında dört dengeli posting, account delta/gross-margin sorgusu, replay/conflict/no-mutation ve invariant audit.
- `PSE.Actors` assembly: stable customer/intent/visit kimlikleri, immutable state/deadline/route kayıtları, bounded exact command receipt ledger'ı, visit-owned canonical `CustomerConsultationAuthority`, deterministic query/revision ve invariant audit.
- Customer visit zinciri `Entering → Browsing → NavigatingToCheckout → AwaitingCheckout → Exiting → Exited`; route state başına iki deneme, `RouteUnavailable`, `OfferDeclined`, patience/exit timeout ve güvenli terminal fallback kullanır.
- `CustomerOfferDecisionEvaluator`, yalnız canonical authority'nin current `Browsing` visit'iyle exact eşleşen immutable consultation receipt + tek shelf offer + accepted price girdisiyle stable `Buy/Leave` ve exact provenance üretir. Missing/foreign/stale receipt kararı kilitler; değerlendirme hiçbir authority'yi mutate etmez.
- `CustomerOfferDecisionActionAuthority`, explicit Actors↔Retail binding sonrası canonical consultation ownership'ini, receipt zamanını ve current visit/offer'ı yeniden doğrular. Current `Buy` exact action-owned serialized reservation ve `Browsing → NavigatingToCheckout`; current `Leave` reservation olmadan `Browsing → Exiting/OfferDeclined` üretir. Stale/mismatch/preflight failure bütün authority'lerde no-mutation, exact kind replay idempotent ve cross-kind replay conflict'tir.
- `InventoryIntake` bütün manifest satırlarını identity/tracking/capacity bakımından preflight eder; başarıda tek revision, failure'da sıfır stok mutation üretir.
- `GarageStockFlowSession` exact serialized item için `Arrived → Receiving → ActorHands → Shelf/WorldFloor` prototype composition'ını kurar.
- `InventoryItemWorldBinding` aynı Inventory item/world item kimliğini, `InventoryPlacementZone` ise doğrulanmış surface/container eşlemesini taşır.
- `PlayerCarryController` bound item'larda domain-first world mutation uygular; world failure domain rollback yapar, recovery authoritative container ve fiziksel pozu birlikte düzeltir.
- `DeliveryParcelProjection` kapalı dış parcel ile revealed ürün ve Receiving'de kalan açık kabuğu ayırır; opening accepted exact manifest/location doğrulaması sonrası idempotenttir ve domain revision/quantity değiştirmez.
- Aynı Interact binding'i acceptance → unpack → pickup olarak sıralanır; HUD/dünya panosu/prompt parcel durumunu klavye ve gamepad için dinamik gösterir.
- Exact ürün RAF A Shelf container'ındayken aynı Interact binding'i kasıtlı offer publish yapar; etiket authority başarısından önce `FİYAT YOK`, sonra `549,99 EUR` gösterir. Publish Inventory/Orders revision veya quantity değiştirmez.
- Fiyatlanmış RAF A ürününde `G / Gamepad East` exact item'ı demo customer basket için ayırır; available quantity `0`, total quantity `1` kalır. Etiket/pano reservation'ı gösterir, `E / Gamepad South` pickup fail-closed olur ve aynı `G / East` release sonrası available quantity `1`e döner.
- Ayrılmış RAF A ürününde primary action checkout veya ödeme başlatmaz; dinamik prompt `KASA İSTASYONUNA GİT` gösterir. Checkout/payment authority yalnız stable `world.checkout-station.garage-001` fiziksel hedefinden, pause kapalıyken `2,75 m` range + `24°` focus + raycast LOS ve exact matching current customer `AwaitingCheckout` gate'iyle açılır.
- Station'daki ilk `Mouse Left / Gamepad RT` edge'i bütün basket satırlarını exact customer/visit/offer/item/reservation/Buy-action provenance'ıyla preflight edip integer price/currency/total ve unit cost'u immutable checkout snapshot'ına alır. Sonraki offer update'i açık `549,99 EUR` fiyatını değiştirmez; checkout aktif release ve pickup fail-closed kalır.
- Held/same-frame input ödeme değildir. Release/repress sonrasındaki ikinci `Mouse Left / Gamepad RT`, exact-cash `nakit ödemeyi al` eylemidir. Inventory/Basket/Checkout prepared planını tek Economy settlement sınırında commit eder ve stable receipt + dört dengeli ledger posting'i bırakır. Projection, stok ve fulfilled müşteri çıkışı yalnız matching canonical receipt sonrası tamamlanır. Exact tekrar idempotent; yanlış access/state/provenance/tutar/currency/payment method, stale plan ve identity conflict bütün authority'lerde no-mutation'dır.
- `PSE.World` ve `PSE.Presentation` assembly sınırları.
- GarageGraybox sahnesi, connected `PlayerRig` prefabı ve CharacterController tabanlı birinci şahıs hareket.
- Klavye/fare + gamepad Input System sözleşmesi, runtime action izolasyonu ve rebind override store.
- FOV/hassasiyet/invert/motion-reduce ayarları, görünür prototip eller, pause/cursor ve runtime-ready tanısı.
- Canonical fiziksel ürün kimliği, 2 m hedef çözümleme, tek taşıma slotu, kinematic carry ve güvenli drop/recovery.
- `E / Gamepad South` ile alma, `G / Gamepad East` ile bırakma ve effective binding'i gösteren HUD prompt'u.
- `Mouse Left / Gamepad RT` ile kontrollü küçük-kutu placement modu; `G / Gamepad East` ile onay.
- İşaretli `PlacementSurface`, 0,25 m grid/90° yaw snap, tam taban/overlap doğrulaması, yeşil-kırmızı ghost + metin ve stabil kinematic placement.
- Ayrı büyük-kutu carry profili: turuncu bantlı graybox, iki-el pozu, `0,65×` hareket, sprint kilidi ve motion-safe `6°` istenen FOV bedeli.
- Büyük kutu `G / Gamepad East` ile gerçek boyutuna göre fail-closed güvenli bırakılır; etkin binding, ağır-yük ve engelli-drop durumu HUD prompt'unda görünür.
- Büyük-kutu placement girişi kapalıdır; stable ID, tek slot, physics snapshot ve disable/world-floor recovery korunur.
- Küçük kutu placement modunda `R / Right Shoulder` ile deterministik `90°` döner; etkin binding/açı promptu, ghost/confirm poz eşitliği ve döndürülmüş footprint güvenlik kontrolü vardır.
- Stable küçük kutu desteğinde merkez/90° snap, beş noktalı tam footprint, overlap engeli, tek kat/tek üst ilişkisi ve dolu taban pickup kilidi vardır; gerçek keyboard/mouse ve gamepad zinciri testlidir.
- Stable platform arabası tek `LargeBox` kabul eder; `E / Gamepad South` ile hands→cart→hands transferi, `Mouse Left / Gamepad RT` ile tut/bırak, 0,85× yüklü ve 0,90× boş hız, sprint kilidi ve dinamik prompt uygulanır.
- Araba hareketi dört noktalı zemin desteği, hedef overlap ve swept bounds obstruction kapılarından geçer; engelde son güvenli pozda kalır. Cart/controller disable yükü son güvenli dünya pozuna kurtarır.
- `GarageCustomerFlowRuntime`, runtime `NavMeshSurface` üzerinde explicit giriş/RAF A/checkout/çıkış anchor'larını izler; offer ziyareti başlatır, Buy reservation checkout'a, Economy settlement receipt'i `Fulfilled` çıkışına ve Leave `OfferDeclined` ile doğrudan Browse→Exit rotasına götürür. Mere completion kaydı müşteriyi çıkışa göndermez.
- `Browsing` müşteri; pause kapalıyken `2,75 m` range, `24°` focus ve raycast LOS içinde gerçek `E / Gamepad South` ile danışılabilir. Dinamik prompt ve kısa Türkçe ihtiyaç cevabı görünür; tek-consumer Interact versionı aynı basışın carry/pickup'a sızmasını engeller. Customer runtime motor `Update`ından sonra, carry `LateUpdate`ından önce çalışır; runtime input reconfigure source asset yerine owned clone kullanır.
- Customer focus CapsuleCollider trigger'dır; station çevresinde player ile fiziksel olarak kilitlenmez. Consultation raycast'i trigger hedefini bilinçli olarak görür; checkout/exit runtime akışı art arda üç final koşuda güvenle tamamlanır.
- Pause integer simulation clock ve NavMeshAgent'ı dondurur. Route/patience fallback'i Inventory/Retail/Orders revision'larını değiştirmez; terminal müşteri projection'ı güvenle gizlenir.
- Garage müşteri status'u yalnız `Browsing` sırasında `KARAR: SATIN AL / AYRIL` ve stable reason code gösterir. Gerçek `G / Gamepad East` current Buy/Leave kararını action authority'ye uygular; stale Buy `SATIN ALMA ENGELLİ`, stale Leave `AYRILMA ENGELLİ` stable metniyle engellenir.
- Görsel hedef `ADR-0013`teki okunaklı yarı gerçekçiliktir. Mevcut primitive garaj, kutu ve eller final sanat değil; mekanik kanıttır.
- Tek-köşe benchmarkında bevel'lı tezgâh/raf, prosedürel PBR yüzeyler, görev ışığı, ACES/bloom ve reflection probe uygulanmıştır; runtime tanısı `lookdev=ok` verir.
- Issue #52 feature `92a0f7b814ad5e597d8d4ca033f2e533f618f719`, tree `4150bd36fa65d4043061e5979e08efb502338fc6` ve [Repository Guard 31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515) ile doğrulandı. EditMode `352/352`, PlayMode `24/24`, failed/skipped `0`; Universal macOS build `327864494` bayt ve Mach-O `x86_64 + arm64`tır.
- Apple M4/Metal 1280×720 `garage-physical-checkout-station-r21-v1` stock r4 ve art arda üç customer r6/r7/r8 smoke; station access, shelf bypass, release/repress checkout+cash, receipt, Economy/ledger, authority isolation, stock projection ve safe customer exit kapılarını geçti. Scene ve final kanıt SHA-256 değerleri `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md` içindedir.
- Issue #52 source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`, tree `6d73d5ac6d675733c939f181d087da3aef90f496` ve [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) ile kapandı. USB milestone 584/584 manifest, 576/576 exact Git source ve 7/7 evidence kapısını geçti; acceptance `17/17`, Issue kapalı/Done ve parent Epic #9 kapalı/Done'dır.
- Önceki Issue #50 feature `547cf97`, source/docs `aea6e2b`, iki başarılı Guard ve doğrulanmış `2026-08-15_STAGE_B_ATOMIC_CASH_CHECKOUT_AND_INITIAL_ECONOMY_SETTLEMENT` USB milestone ile kapalı/Done tarihsel checkpoint olarak korunur.

Henüz yapılmayanlar:

- Gelişmiş el animasyonu, çok satırlı/çok adetli parcel unpack layout'u, çok katlı/palet istifi ve çoklu/palet taşıma.
- Garajın bütününe yayılmış final sanat ve gelişmiş el modeli/animasyonu.
- Orders'ın satış/servis varyantları, ilk exact-cash satış settlement'ı ötesindeki Economy kapsamı ve diğer domain assembly'leri; Catalog/Inventory/Orders/Economy event-save bağlantısı.
- Sayısal fiyat düzenleme UI'si, açıklanabilir çoklu-offer müşteri kararı, vergi/indirim/para üstü/kart/çoklu ödeme, receipt belgesi/fatura/refund/supplier payment/opening balance, gerçek fiziksel sepet transferi ve daha geniş item/cart container projection'ı.
- Final checkout POS/scanner/cash-drawer artı, fiziksel receipt, çoklu checkout station ve queue.
- Save/Guardian runtime.
- Steam entegrasyonu ve native Windows IL2CPP doğrulaması.

Issue #59'un kaynak/test/build/native-runtime/feature-CI kapıları tamamlandı; source/docs CI ile acceptance 20/20/Issue/Roadmap kapanışı final metadata adımıdır. Ardıl bounded Epic #10 adayı yalnız tek fiziksel PSU seating/retention akışıdır; PCIe/PSU cabling, tam PC benchmarkı, Inventory revision-max hardening, Save, Guardian veya final sanata gizlice büyütülmez.

## 7. Çalışma akışı

```bash
git switch main
git pull --ff-only
git switch -c feature/ISSUE-kisa-ad
```

Değişiklikten sonra:

1. Otomatik testleri çalıştırın.
2. `./Tools/verify-repository.sh` çalıştırın.
3. `PROJECT_BIBLE.md`, ADR/provenans/changelog gereksinimini değerlendirin.
4. Küçük, açıklayıcı commit oluşturun.
5. Branch'i push edip pull request açın; PR şablonunu eksiksiz doldurun.

## 8. Legacy sınırı

`LegacyReference/PC-Shop-Empire-1.1.6/Source` hash doğrulanmış tarihsel snapshot'tır. Doğrudan yeni Unity gameplay kodu olarak port edilmez ve normal feature PR'ında düzenlenmez.

Legacy'den alınabilecekler:

- Tema ve işletme niyeti.
- Dashboard bölüm anlamları.
- Veri alanlarının semantiği.
- İlerleme ve ekonomi tasarım soruları.

Doğrudan taşınmayacaklar:

- Electron/DOM uygulama mimarisi.
- Eski UI/CSS/görsel tasarımın kopyası.
- Gerçek marka/model verisi veya doğrulanmamış asset.
- Tek tuşla otomatik PC üretme davranışı.

## 9. Bir problemde önce kontrol edin

- Yanlış Unity sürümü mü?
- Paket lock değişmiş mi?
- `Library`/cache yanlışlıkla track edilmiş mi?
- Core assembly Unity referansı almış mı?
- Test raporu gerçekten oluşmuş mu, yoksa Editor erken mi kapandı?
- Git çalışma ağacı başka bir süreç tarafından değişiyor mu?
- Legacy manifest veya Project Bible kopyası ayrışmış mı?
- Secret/credential loga veya dosyaya yazılmış mı?

Sorunu düzeltmek için `main` history'sini force-push/reset etmeyin. Yeni branch, fix veya revert commit kullanın.

## 10. Devralma tamamlanma ölçütü

Yeni geliştirici şu beş şeyi gösterebildiğinde devir başarılıdır:

1. Projeyi clone edip doğru Unity sürümünde açtı.
2. Repo guard, güncel 548 Edit Mode ve 43 Play Mode testi geçti.
3. Vizyon ile vertical slice sınırını kendi cümlesiyle açıklayabildi.
4. GitHub Project'te sıradaki issue/acceptance kriterini buldu.
5. Küçük bir docs/test PR'ını yaşayan belge kurallarına uygun açabildi.

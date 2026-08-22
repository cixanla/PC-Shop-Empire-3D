# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 22 Ağustos 2026<br>
**Durum:** Issue #58 deterministic single air-cooler seating + four-point retention kaynak/test/build/runtime/CI/GitHub kapıları tamamlandı; Issue/Roadmap `Done`, fiziksel USB kullanıcı talimatıyla ertelendi<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #58 / Epic #10

- Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`; tek canonical serialized `Lga1700TopDownAirPreAppliedTim` cooler, immutable slot/bracket/dört-point topology ve atomik Workbench+ProcessorSocket+MemorySlot+StorageSlot+ProcessorCoolerSlot claim'i eklendi.
- Supported lifecycle `EmptyOpen ↔ CoolerSeatedUnsecured ↔ CoolerRetained`dır. Seat/remove exact serialized custody'yi değiştirir; retain/unretain Inventory revision'ını değiştirmez. Pre-applied TIM başarılı seat'te yalnız bir kez tüketilir ve aynı item üzerinde kalıcıdır.
- Retention çapraz `1→3→2→4`, release exact ters sıradadır. Immediate/delayed replay, duplicate TIM rejection, installed-cooler CPU-retention/motherboard-detach host gates, conflict/stale/full-hands no-mutation ve same-instance recovery testlidir.
- İki keyed orientation, range/focus/LOS/socket-interface/support/RAM-clearance/obstruction/tie/saturation fail-closed solver ile korunur. Mode kapalı ghost/query sıfırdır; generic placement/stack/cart bypass'ı kapalıdır.
- GarageGraybox `garage-processor-cooler-r27-v1`; cold plate, pre-applied TIM yüzeyi, fin stack, fan, bracket ve dört retention noktası görünürdür. Compact HUD gerçek keyboard/mouse ve gamepad bindinglerini dinamik gösterir.
- Final EditMode `521/521`, PlayMode `38/38`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `328534723` bayt; aktif Apple Silicon/Metal makinesinde 1280×720 readiness ve exact cooler smoke başarılıdır. Runtime yeni makineyi Apple M1 olarak tanımlamıştır; eski Apple M4 device ifadesi bu checkpoint için kullanılmaz.
- Source/docs `2e848e3bdc5795a349e6c857973c7c88fef36cd7`; [feature Guard 32591206866](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591206866) ve [source/docs Guard 32591381804](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591381804) başarılıdır. Acceptance `19/19`; Issue #58 ve Roadmap `Done`dur.
- USB fiziksel aygıtı yeni Mac tarafından görünmediği için kullanıcı talimatıyla ertelendi. `/Users/cixanla/Developer/PCShopEmpire3D/CheckpointStaging/2026-08-22_STAGE_B_DETERMINISTIC_SINGLE_AIR_COOLER_FOUR_POINT_RETENTION` altında 712 exact Git source + 4 evidence + source kaydı, toplam 717/717 payload ve `f7b2b9bafee9529d95431bbc90914ba51ab24e01de9a0d5d77a53f26cb5626a5` manifest hazırdır. Bu dış yedek kapısı gameplay geliştirmesini bloklamaz; kullanıcı USB'yi bağladığını söylediğinde fiziksel readback tamamlanır.
- Ayrıntı `Docs/ADR-0036-DETERMINISTIC-SINGLE-AIR-COOLER-SEATING-AND-FOUR-POINT-RETENTION.md` ve tarihli Evidence belgesindedir. Ayrı paste/reapplication, liquid cooling, GPU/PSU/cabling, tam benchmark, final art ve Windows/Steam ayrı kapılardır.

## Önceki checkpoint — Issue #57 / Epic #10

- Feature `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`, tree `1aedb833983df256c500c6a1815b075fa29c254c`; tek canonical M.2 2280 NVMe, immutable M-key/2280 standoff/captive-screw topology ve atomik Workbench+ProcessorSocket+MemorySlot+StorageSlot claim'i eklendi.
- State yalnız `EmptyOpen ↔ StorageDeviceSeatedUnsecured ↔ StorageDeviceSecured`dır. Seat/remove exact serialized custody'yi değiştirir; secure/unsecure Inventory revision'ını değiştirmez. Four-operation immediate/delayed replay, installed-storage host gate, conflict/stale/overflow no-mutation ve same-instance recovery testlidir.
- Guided pose 18°; seated pose düzdür. Range/focus/LOS/key/orientation/support/obstruction/tie/saturation fail-closed solver ile korunur. Mode kapalı ghost/query sıfırdır; generic placement/stack/cart bypass'ı kapalıdır.
- GarageGraybox `garage-m2-nvme-captive-screw-r26-v1`; PCB/controller/NAND/label/gold M-key contacts, connector, 2280 standoff ve captive screw görünürdür. Compact HUD gerçek keyboard/mouse ve gamepad bindinglerini dinamik gösterir.
- Final EditMode `490/490`, PlayMode `35/35`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `328362356` bayt; Apple M4/Metal 1280×720 readiness ve exact storage smoke başarılıdır.
- Source/docs `6e0627ec7a76a70abdba8bb507e6ef6979e34236` ve [Repository Guard 31970813717](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31970813717) başarılıdır. USB milestone `2026-08-16_STAGE_B_DETERMINISTIC_SINGLE_M2_NVME_CAPTIVE_SCREW`, exact source + 4 final evidence + source kaydıyla 689/689 readback, `19da758c…21b8` manifest ve AppleDouble `0` olarak doğrulandı.
- Acceptance `21/21`; Issue #57 kapalı ve Development Roadmap `Done`dur.
- Ayrıntı `Docs/ADR-0035-DETERMINISTIC-SINGLE-M2-NVME-SEATING-AND-CAPTIVE-SCREW-RETENTION.md` ve tarihli Evidence belgesindedir. İkinci storage/SATA/RAID, tam performans benchmarkı, diğer PC bileşenleri, final art ve Windows/Steam ayrı kapılardır.

## Önceki checkpoint — Issue #56 / Epic #10

- Feature commit `7482fc9aabe6a3a27ba41730db12c60e18aac515`, tree `291b23cb2fe774cb44ba71b26716d7c8131370a2`; source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962`, tree `16053753222d3166d5f59d61ec20b4f8bf8e23cb`; USB metadata `17af550856e8bca288ed5c17924bc82586c76c27`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055), [31920258176](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920258176) ve [31920923402](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920923402) başarılıdır.
- Tek canonical serialized DDR5 UDIMM, immutable A2/Channel A/Bank 2/population priority 1 topology'si ve atomik managed Workbench+ProcessorSocket+MemorySlot triple claim kullanılır. Başarı tek Inventory revision; bütün conflict/overflow yolları sıfır partial/ghost claim üretir.
- Assembly state'i `EmptyOpen → MemoryModuleSeatedOpen → MemoryModuleRetained` ve tersidir. Seat/close/open/remove stable slot/retention/item/product ile attach/secure/seat/retention lineage'ını doğrular; immediate ve delayed replay aynı receipt referansını döndürür.
- Oyuncu input'u `R / Right Shoulder` ile yalnız `0° ↔ 180°` keyed orientation üretir. Valid seat secured motherboard, exact DDR5/A2/channel/bank, range/focus/LOS ve obstruction-free insertion ister; reversed confirm tam no-mutation fail-closed'dur.
- İki görünür latch close sırasında sol→sağ, open sırasında sağ→sol hareket eder fakat tek retention operation/revision/receipt üretir. Retained remove ve DIMM-installed motherboard detach engellenir.
- GarageGraybox `garage-dimm-dual-latch-r25-v1`; dört materyalli UV'li DIMM PCB/chip/heat-spreader/notch, hard-surface A2 bed/rail ve iki ayrı latch pivotu taşır. Assembly bütçesi `25 Renderer / 13 Collider / 1 TextMesh`tir.
- Gerçek Input System keyboard/mouse ve gamepad pickup→guided mode→180° toggle→seat→close/open→remove→recovery akışı, dynamic compact HUD, co-edge/pause drain ve mode-kapalı sıfır ghost/query sözleşmesiyle testlidir.
- Final EditMode `461/461`, PlayMode `33/33`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `328268700` bayttır; ana executable Universal Mach-O `x86_64 + arm64`, SHA-256 `eba2a0ba…eb50`.
- Apple M4/Metal 1280×720 runtime readiness ve exact `GARAGE_DIMM_RUNTIME_SMOKE ... keyed-orientation=ok latch-order=ok ... replay=ok authority-isolated=ok identity=stable recovery=ok` marker'ı geçti.
- Final evidence `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altında korunur; ayrıntı `Docs/ADR-0034-DETERMINISTIC-SINGLE-DIMM-SEATING-AND-DUAL-LATCH-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-DIMM-SEATING-AND-DUAL-LATCH-RETENTION-CHECKPOINT-2026-08-16.md` içindedir.
- Ayrı USB milestone `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-16_STAGE_B_DETERMINISTIC_SINGLE_DIMM_DUAL_LATCH_RETENTION` altında doğrulandı: 663 tracked source + 4 final evidence + source kaydı, 668/668 readback, 12.073.868 payload baytı, `8658b50a…c50` manifesti ve bütün güvenlik/AppleDouble mismatch sayaçları `0`.
- Acceptance `21/21`; Issue #56 `Completed`, Development Roadmap `Done`dur.
- Sonraki bounded Epic #10 adayı tek M.2 2280 NVMe SSD seating + captive retention screw akışıdır. İkinci storage yolu, SATA/RAID, GPU/cooler, tam benchmark, Inventory hardening ve Windows/Steam ayrı kapılardır.

## Önceki checkpoint — Issue #55 / Epic #10

- Feature commit `99cadad414789d3f440e08cc6e42e727c2b7a2ad`, tree `fea116af021d66efb31b96b4f3e7523929f8b8ad`; yerel Repository Guard `tracked=624` ve [feature Repository Guard 31914489537](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914489537) başarılıdır.
- Source/docs `d9d0722a1592a83b89938529f72b3170f17e94eb` ve [Repository Guard 31914774370](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914774370) başarılıdır; acceptance `20/20`, Issue kapalı/Roadmap `Done`dur.
- Tek canonical serialized CPU, Inventory'nin atomik managed Workbench + capacity-1 ProcessorSocket pair claim'iyle korunur. Raw transfer bypass'ları kapanır; pair conflict/revision failure kısmi claim veya ghost custody üretmez.
- Assembly state'i `EmptyOpen → ProcessorSeatedOpen → ProcessorRetained` ve tersidir. Seat/close/open/remove operation'ları stable slot/retention/item/product ile attach/secure/seat/retention lineage'ını doğrular; immediate ve delayed replay aynı receipt referansını döndürür.
- CPU seat yalnız secured motherboard üzerinde geçerlidir. Motherboard CPU takılıyken unsecure olabilir; detach `assembly.processor-installed` ile kilitlenir. Retained CPU açılabilir, unsecured host üzerinde yeniden kapatılamaz ve seated-open CPU çıkarılabilir.
- Guided mode `Mouse Left / Gamepad RT`, keyed rotation `R / Right Shoulder`, seat `G / Gamepad East`, remove `E / Gamepad South` kullanır. Mode kapalıyken ghost ve PhysX seat sorgusu yoktur; co-edge'ler tek tüketicili ve pause-safe'dir.
- GarageGraybox `garage-cpu-socket-retention-r24-v1`; 45 × 37,5 × 4 mm notched LGA package, ayrı PCB/IHS materyalleri, hard-surface UV/normaller, matching triangular key, dört kenarda 2 mm aperture toleranslı load plate ve görünür lever taşır. Bütçe `21 Renderer / 11 Collider / 1 TextMesh` olarak sabittir.
- Pickup/seat/retain/unsecure/open/remove/recovery aynı Unity instance, stable item ID, parent, authored loose pose, Rigidbody/safe pose ve canonical projection sayısını korur. Wrong orientation ve retained remove gate'i Assembly/Inventory/receipt/pose no-mutation ile fail-closed'dur.
- Final EditMode `430/430`, gerçek Input System PlayMode `31/31`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `328144884` bayttır; ana executable Universal Mach-O `x86_64 + arm64`, SHA-256 `d87710b6…24f0`.
- Apple M4/Metal 1280×720 runtime readiness ve exact `GARAGE_CPU_SOCKET_RUNTIME_SMOKE ... keyed-orientation=ok retained-remove-gate=ok replay=ok identity=stable` marker'ı geçti.
- Final evidence `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altında korunur; ayrıntı `Docs/Evidence/DETERMINISTIC-CPU-SOCKET-SEATING-AND-RETENTION-CHECKPOINT-2026-08-16.md` içindedir.
- Issue #53–#55 birleşik USB milestone'u `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-16_STAGE_B_PHYSICAL_ASSEMBLY_MOTHERBOARD_FASTENER_AND_CPU_SOCKET_RETENTION` altında doğrulandı: source `07364b79`, 640 tracked source + 12 final evidence + source kaydı, 653 satırlı `0b5f3c61…aaba9e` manifest, 13.500.119 payload baytı; bütün mismatch sayaçları `0`.
- Sıradaki bounded Epic #10 child adayı tek dual-latch DIMM/RAM seating akışıdır. GPU/cooler/storage, tam build benchmarkı, Inventory genişlemesi ve Windows/Steam ayrı kapılardır.

## Önceki checkpoint — Issue #54 / Epic #10

- Feature commit `b6812394f835d64d5bf8422d8e7996ec433cd0f1`, tree `192f9d8f1334cf9e1ff1d21382c44a847bbfa7e6`; yerel Repository Guard `tracked=616` ile başarılıdır.
- Source/docs commit `7cec7cc4b6fd80997acd0dc2d6943ef08850f4ad`, tree `214381bd6c9d06a7ab2b2c5ea5e902437dca5914`; [Repository Guard 31909940414](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31909940414) başarılıdır. Acceptance `18/18`, Issue kapalı/Roadmap `Done`dur.
- Assembly aggregate'ı stable `assembly.fastener.motherboard-main-01` kimliğini ve `Empty / SeatedUnsecured / SeatedSecured` seat durumlarını taşır. Secure/unsecure exact receipt, replay, expected revision ve attach/secure lineage kapılarıyla yürür.
- Historical receipt fold Assembly revision sırasını, previous/result state'i, item/product provenance'ını, attach/secure lineage'ını ve Inventory revision monotonluğunu doğrular. Wrong identity/state/lineage/conflict/overflow hiçbir authority'yi mutate etmez.
- Secure/unsecure Inventory custody ve revision'ı değiştirmez. Secured board hem player pickup preflight'ında hem direct Assembly detach authority'sinde `assembly.component-secured` ile kilitlidir.
- GarageGraybox `garage-motherboard-fastener-r23-v1` marker'ıyla tek captive screw, solid focus target, cross recess, fiziksel screwdriver ve plate'e bağlı tek-satır status metni taşır. Vida secured durumda exact `4 mm` derine gider; screw/tool pose drift'i projection invariantını fail-closed bozar.
- Solver pause/range/focus/LOS/obstruction kapılarını tek NonAlloc raycast ile uygular; near-hit tie-break ve buffer saturation deterministic/fail-closed'dur.
- `Mouse Left / Gamepad RT` fastener'ı sıkar/gevşetir. Blocked context Primary/Interact/Drop edge'lerinin tek sahibidir; aynı-frame blocker kaldırma eski edge'i replay edemez. Pause co-edge release–repress ister.
- Final EditMode `411/411`, gerçek Input System PlayMode `29/29`; failed/skipped `0`.
- Universal macOS Development/StrictMode build `328057977` bayttır; ana executable Universal Mach-O `x86_64 + arm64`, SHA-256 `f9cc0403…aad69`.
- Apple M4/Metal 1280×720 runtime readiness ve exact smoke; direct authority detach gate'i, immediate+delayed secure/unsecure replay'i, Inventory izolasyonunu, stable identity ve recovery'yi birlikte geçti.
- Final evidence `/Users/cixanla/Developer/PCShopEmpire3D/TestResults` altında korunur; ayrıntı `Docs/Evidence/DETERMINISTIC-MOTHERBOARD-FASTENER-SECURE-UNSECURE-CHECKPOINT-2026-08-15.md` içindedir.
- USB bağlı değildir; `/Volumes` erişimi veya snapshot yazımı yapılmadı. USB yeniden bağlandığında Issue #53–#54 için ayrı manifest/readback milestone'u alınacaktır.
- Sıradaki bounded Epic #10 child adayı tek CPU socket seating + retention lever akışıdır. RAM/GPU/cooler/tam build, Inventory revision-max hardening ve Windows/Steam ayrı kapılardır.

## Önceki checkpoint — Issue #53 / Epic #10

- Feature commit `582a3cf3e81a2905e39148065bd5f6c7e35bbc06`, tree `fc80b7cd72e0fd8bc48f5917f9c303e84d72f4cd`; yerel Repository Guard `tracked=615` ile başarılıdır.
- Source/docs commit `8c6abe45d1f9b6c72def9b686b9c81bf3704d10d`, tree `387bcba701b8a959681e92bf29dc48a4d09f0ab7`; [Repository Guard 31905540378](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378) başarılıdır.
- Yeni Unity-bağımsız `PSE.Assembly`, existing Catalog/Inventory authority'lerini kullanır. Tek `MicroAtx` anakart exact serialized item/product kimliğiyle ActorHands↔managed Workbench arasında atomik attach/detach receipt'leri üretir.
- GarageGraybox'ta görünür açık kasa, keyed tray/slot, standoff/connector işaretleri ve tek canonical motherboard projection'ı vardır. Guided solver pause/range/focus/LOS/orientation/support/obstruction kapılarını domain mutation öncesi fail-closed uygular.
- `Mouse Left / Gamepad RT` seat modunu, fresh `G / Gamepad East` confirm'i yönetir. Aynı-frame Primary+Drop yalnız mode geçişini tüketir; attach/drop yapmaz. Promptlar son cihaz ailesine göre `E/G/R/LMB` veya `A/B/RB/RT` gösterir.
- Attach sonucu yalnız `SeatedUnsecured`dır. Exact replay idempotent; wrong kind/form factor/identity, occupied/foreign/stale/overflow/full-hands ve failed world-drop yollarında Assembly/Inventory/world projection değişmez.
- Detach ve recovery aynı fiziksel instance ile Inventory item ID'sini korur; generic pickup/cart/stack/box-placement bypass'ı, jitter, duplicate ve ghost custody engellenir.
- Final EditMode `394/394`, gerçek Input System PlayMode `26/26`; failed/skipped `0`.
- Universal macOS Development/StrictMode build `328020817` bayttır; ana executable Mach-O `x86_64 + arm64`, SHA-256 `cad75f5e…a0f0`.
- Apple M4/Metal 1280×720 runtime `garage-motherboard-seating-r22-v1` readiness ve exact `assembly-flow=ok ... input-single-consumer=ok ... recovery=ok` smoke verdi.
- Final evidence repo dışında `/Users/cixanla/Developer/PCShopEmpire3D/Builds/Local/Evidence/Issue53-2026-08-15` altında korunur; ayrıntı `Docs/Evidence/AUTHORITATIVE-SINGLE-MOTHERBOARD-SEATING-CHECKPOINT-2026-08-15.md` içindedir.
- USB kullanıcı tarafından geçici olarak çıkarıldı. `/Volumes` erişimi veya snapshot yazımı yapılmadı; USB yeniden bağlandığında manifest/readback checkpointi alınacaktır.
- Sıradaki bounded gameplay paketi, Issue #53 remote/backup kapanışından sonra tek motherboard fastener secure/unsecure akışıdır. CPU/RAM/GPU, tam build ve Inventory #7/#8 genişlemesi ayrı kalır.

## Önceki tamamlanmış checkpoint — Issue #52 / Epic #9

- Feature commit `92a0f7b814ad5e597d8d4ca033f2e533f618f719`, tree `4150bd36fa65d4043061e5979e08efb502338fc6`; [feature Repository Guard 31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515) başarılıdır.
- Source/docs commit `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`, tree `6d73d5ac6d675733c939f181d087da3aef90f496`; [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) başarılıdır.
- Issue #52 acceptance `17/17` doğrulandı; Issue kapatıldı ve Development Roadmap durumu `Done` yapıldı. Parent Epic #9 ana kabul kapısı da doğrulanarak kapatıldı/Done yapıldı.
- Stable `world.checkout-station.garage-001` kimlikli görünür checkout station yalnız pause kapalı, `2,75 m` range, `24°` focus ve raycast LOS içinde çalışır. RAF A primary checkout/payment bypass'ı kapalıdır ve oyuncuya `KASA İSTASYONUNA GİT` yönlendirmesi gösterilir.
- Yalnız exact matching current customer/visit/basket/offer/item/reservation/action provenance'ı taşıyan `AwaitingCheckout` ziyareti station'ı yetkilendirir. Stale, foreign, historical, forged/value-equal veya yanlış-state zinciri bütün gameplay/economy authority'lerinde no-mutation fail-closed olur.
- İlk `Mouse Left / Gamepad RT` edge'i immutable checkout snapshotını bir kez başlatır. Fiyat, currency ve acquisition unit cost donar. Held/same-frame/replay ödeme değildir; release/repress sonrasındaki ikinci edge exact-cash settlement'ı bir kez üretir.
- Canonical Economy receipt; exact settlement/transaction/completion/checkout/customer/payment/currency/amount/COGS, `Buy` action, line, ledger ve time provenance'ını kapılar. Ürün projection'ı ve customer `Fulfilled` yalnız matching receipt sonrasında ilerler.
- Customer focus collider'ı trigger'dır; station çevresinde player/NPC fiziksel sıkışması yaratmaz. Consultation LOS trigger hedefini görür; üç ardışık final customer smoke güvenli çıkışı doğrular.
- EditMode `352/352` (`editmode-issue52-r3.xml`), gerçek Input System PlayMode `24/24` (`playmode-issue52-r3.xml`); failed/skipped `0`.
- Universal macOS development build `327864494` bayttır; Apple M4/Metal 1280×720 stock r4 ile customer r6/r7/r8 koşuları `garage-physical-checkout-station-r21-v1` markerıyla başarılıdır.
- Stock smoke mevcut order→stock→offer→checkout→exact-cash settlement zincirini korudu; customer smoke `awaiting-checkout-gate=ok checkout-station=ok station-focus=ok station-los=ok shelf-bypass-blocked=ok checkout-start=ok cash-payment=ok authority-isolated=ok customer-hidden=ok` verdi.
- Doğrulanmış USB milestone'u 584/584 readback, 576/576 exact Git source, 7/7 evidence ve sıfır güvenlik/AppleDouble mismatch ile kapandı.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya gerçek bir dış engel oluşana kadar bağımlılık sırasındaki küçük, geri alınabilir paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset, motor/proje migration'ı ve destructive işlem ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.
- Yardımcı Codex görevleri yalnız ayrık, bounded işler alır; ana Git/Unity deposunun tek doğruluk kaynağı olma niteliğini değiştirmez.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`; Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Branch: `main`; Issue #56 feature `7482fc9aabe6a3a27ba41730db12c60e18aac515`, source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962` ve USB metadata `17af550856e8bca288ed5c17924bc82586c76c27` private remote'a ulaştı, üç Guard başarılıdır; ayrı USB milestone 668/668 doğrulandı, acceptance `21/21`, Issue kapalı/Roadmap `Done`dur.
- Core stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministic event dispatcher tamamdır.
- Catalog/Inventory/Orders/Retail zinciri; authoritative teslim alma, maliyet provenance'ı, parcel açma, shelf offer, basket reservation, checkout snapshot, prepared completion ve consultation-gated stale-safe Buy/Leave action katmanlarını içerir.
- Downstream `PSE.Economy`; exact-cash settlement receipt'i, immutable ledger transaction/entry kayıtlarını, Cash/SalesRevenue/COGS/InventoryAsset hesaplarını, balance ve gross-margin sorgularını içerir. Retail/Inventory/Orders Economy'ye ters referans taşımaz.
- Actors sınırı; kararlı müşteri intent/visit modeli, monotonik lifecycle, bounded route retry/fallback, `OfferDeclined`, command receipt ledger ve visit-owned immutable consultation authority'sini içerir. `AwaitingCheckout` sonrası fulfillment/çıkış canonical Economy settlement receipt'ine bağlıdır.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-dimm-dual-latch-r25-v1`.
- Fiziksel checkout station range/focus/LOS/pause ve exact visit/provenance gate'lerini taşır; shelf/uzak ödeme bypass'ı yoktur. Versioned primary press tek tüketicilidir ve release/repress settlement sözleşmesi gerçek Input System testleriyle kilitlidir.
- PC assembly dilimi exact serialized `MicroAtx` anakartı managed Workbench'e oturtur; Assembly-owned fastener ile `SeatedUnsecured ↔ SeatedSecured`, exact CPU ile capacity-1 socket'te `EmptyOpen ↔ ProcessorSeatedOpen ↔ ProcessorRetained` ve exact DDR5 UDIMM ile A2 slotunda `EmptyOpen ↔ MemoryModuleSeatedOpen ↔ MemoryModuleRetained` döngülerini korur. Attach/secure/seat/retention/detach/recovery lineage'ı ve stable world identity authoritative'dir.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj, kutular, eller ve müşteri final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Önceki tamamlanmış feature checkpoint ve doğrulama kanıtı — Issue #52

- Epic/issue: [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) / [#52](https://github.com/cixanla/PC-Shop-Empire-3D/issues/52).
- Feature commit: `92a0f7b814ad5e597d8d4ca033f2e533f618f719`.
- Feature tree: `4150bd36fa65d4043061e5979e08efb502338fc6`.
- Feature Repository Guard: [31892420515](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892420515), başarılı.
- Source/docs commit: `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`; tree `6d73d5ac6d675733c939f181d087da3aef90f496`.
- Source/docs Repository Guard: [31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650), başarılı.
- EditMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/editmode-issue52-r3.xml`; `352/352`, failed/skipped `0`; `295494` bayt; SHA-256 `c6bd6e4fdbe7d06e5d986a23f7dbf7bd1da9b765d2df63c2136ed37d95e0ac6d`.
- PlayMode XML: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/playmode-issue52-r3.xml`; `24/24`, failed/skipped `0`; `39375` bayt; SHA-256 `8c05afec6b0a91345d52a61482c922346f14b6a7f71addfcfb959f09ab4a9230`.
- Universal macOS build: `327864494` bayt. Build log `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/build-macos-issue52-r4.log`; `579886` bayt; SHA-256 `c9a0780e1a40cc432dbf78568a72d470082319922431bd2797a514565209c69c`.
- Universal app executable: Mach-O `x86_64 + arm64`; `117179` bayt; SHA-256 `cf66c67f4485fcb8adfa6e2b327b9d88bbb66c06a313d47bee42ecca90f179b2`.
- Stock runtime log: `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/runtime-stock-flow-issue52-r4.log`; `11614` bayt; SHA-256 `f3efecbb91b2090dc055ddbd1497a757c0e0c069030cec2495a02dc7a551676a`.
- Customer runtime tekrarları: `runtime-customer-flow-issue52-r6.log` / `r7.log` / `r8.log`; `5247/5248/5248` bayt; SHA-256 `4e571e…6b6`, `3fb863…5b6`, `b942bf…f1b`.
- Runtime host: Apple M4/Metal, 1280×720. Marker: `garage-physical-checkout-station-r21-v1`.
- Stock smoke: `stock-flow=ok checkout-snapshot=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok ledger-balanced=ok stock-consumed=ok stable=ok`.
- Customer smoke: `awaiting-checkout-gate=ok checkout-station=ok station-focus=ok station-los=ok shelf-bypass-blocked=ok checkout-start=ok cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok authority-isolated=ok stock-projection-hidden=ok customer-hidden=ok`.
- Sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; `1397931` bayt; SHA-256 `509e6c256a9a66850dfd3cdb22b04b53596c5080ff25e7b14d29000b289bd3fe`.

## Bilinçli kapsam dışı

- Vergi, indirim, para üstü, kart/çoklu ödeme yöntemi, receipt belgesi/fatura, refund ve supplier payment.
- Opening balance, kalıcı Save/journal/migration, final ekonomi UI/raporlama ve genel ledger entegrasyonu.
- Çok turlu diyalog, çoklu recommendation/ürün/offer seçimi, utility scoring, çoklu müşteri ve sıra kapasitesi.
- Final POS/scanner/cash-drawer prop artı, fiziksel receipt, çoklu checkout station ve queue.
- Memnuniyet/itibar, çalışan AI, final model/animasyon/ses ve gerçek Windows doğrulaması.
- İlk settlement yalnız satış anındaki delta'yı authoritative kaydeder; tam şirket muhasebesi veya başlangıç bilançosu iddiası taşımaz.

## Önceki tamamlanmış checkpoint — Issue #51

- Issue #51 feature `846eb5d9912150a6ef3aae9a37678d71348f92a3`, source/docs `f9bc38d8861f575909e36a331ab1cc6476a237a5`, Repository Guard `31888147505` + `31888842125`, EditMode `347/347`, PlayMode `23/23`, Mac `garage-customer-consultation-r20-v1` smoke ve doğrulanmış USB milestone ile acceptance `16/16`, kapalı/Done'dır.
- Bu tarihsel checkpointin ayrıntılı kanıtları ve aşağıdaki doğrulanmış USB milestone'u korunur; Issue #52'nin feature/source/docs/Guard veya USB kimliği olarak yorumlanmaz.

## USB güvenli checkpoint durumu

- Önceki Issue #50 milestone'u tarihsel olarak korunur: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_ATOMIC_CASH_CHECKOUT_AND_INITIAL_ECONOMY_SETTLEMENT`.
- Source/docs `aea6e2bd01642f4f72f1a9ee70f07e3dd0e5072b`; 566 tracked `SOURCE`, 5 final `EVIDENCE`, bir `SOURCE_COMMIT.txt`; 572 manifest payload satırı, toplam 574 dosya ve 10.227.122 payload baytı.
- `MANIFEST.tsv` SHA-256: `b31681628aa2da3e2dc1899f5f728bc28bf8425838d2178579a45d7b15ccecf8`.
- Tam geri okuma 572/572 hash+boyut+path, 566/566 Git-blob ve 5/5 evidence eşliğiyle geçti. Path-set farkı, forbidden/cache/credential, internal AppleDouble ve sibling sidecar sayıları `0`dır.
- Son tamamlanmış milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_BOUNDED_SINGLE_CUSTOMER_CONSULTATION_AND_RECOMMENDATION_GATE`.
- Source/docs `f9bc38d8861f575909e36a331ab1cc6476a237a5`; 572 tracked `SOURCE`, 5 final `EVIDENCE`, bir `SOURCE_COMMIT.txt`; 578 manifest payload satırı, toplam 580 dosya ve 10.366.388 payload baytı.
- `MANIFEST.tsv` SHA-256: `f8d3ce98e7daa5a014d3d4c79b9a247ac5e15f737914746bd130c191289ccf20`.
- Tam geri okuma 578/578 hash+boyut+path, 572/572 Git-blob ve 5/5 evidence eşliğiyle geçti. Path-set, forbidden/cache/credential, internal AppleDouble ve sibling sidecar mismatch sayıları `0`dır.
- Güncel milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_PHYSICAL_CHECKOUT_STATION_AND_AWAITING_CHECKOUT_GATED_CASH_PAYMENT`.
- Source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`; 576 tracked `SOURCE`, 7 final `EVIDENCE`, bir `SOURCE_COMMIT.txt`; 584 manifest payload satırı, toplam 586 dosya ve 10.485.924 payload baytı.
- `MANIFEST.tsv` SHA-256: `7fbb5f0ce2bdd0aa32f0baa943e12d1dcf331b4ea05a85c81e0215c969531fbd`.
- Tam geri okuma 584/584 hash+boyut+path, 576/576 exact Git source ve 7/7 evidence eşliğiyle geçti. Path-set, forbidden/cache/credential, internal AppleDouble ve sibling sidecar mismatch sayıları `0`dır.
- En yeni milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-16_STAGE_B_PHYSICAL_ASSEMBLY_MOTHERBOARD_FASTENER_AND_CPU_SOCKET_RETENTION`.
- Source `07364b79ad111aa778493c8936a7709c84b48464`, tree `bec3a18af5842b3b68bdfdebf38eddd44bc4dfc7`; 640 tracked `SOURCE`, 12 final `EVIDENCE` ve bir `SOURCE_COMMIT.txt`; 653 manifest payload satırı ve 13.500.119 payload baytı.
- `MANIFEST.tsv` SHA-256: `0b5f3c6100abeb3dc28e292ed515186fffabaa17f4c3ec66aef3399572aaba9e`.
- Tam geri okuma 653/653 hash+boyut+path, 640/640 exact Git source ve 12/12 evidence eşliğiyle geçti. Forbidden/cache/credential, internal AppleDouble ve sibling sidecar mismatch sayıları `0`dır.

## Sıradaki immediate geliştirme işi

1. Ardıl Epic #10 child'ını ayrı issue/acceptance ile tek fiziksel bileşene sınırlamak; GPU, PSU, kablolama ve tam benchmarkı tek pakette birleştirmemek.
2. Kullanıcı USB'nin yeniden bağlı olduğunu söylediğinde hazır Issue #58 staging paketinin fiziksel `.incoming-*`/readback kapanışını yapmak; o zamana kadar USB'yi sorgulamamak.
3. Inventory revision-max hardening, ayrı paste/liquid cooling, final art ve Windows/Steam kapılarını ayrı tutmak.

## Güvenli devam komutu

Issue #58 feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, source/docs `2e848e3bdc5795a349e6c857973c7c88fef36cd7`, EditMode `521/521`, PlayMode `38/38`, Universal Mac `328534723` bayt, aktif Apple Silicon/Metal exact r27 cooler smoke ve iki başarılı Guard ile kapandı. Acceptance `19/19`, Issue/Roadmap `Done`dur. Fiziksel USB ertelendi; 717/717 staging hazırdır ve sonraki bounded gameplay'i bloklamaz.

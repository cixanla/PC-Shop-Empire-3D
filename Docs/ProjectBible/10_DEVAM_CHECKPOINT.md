# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 24 Ağustos 2026<br>
**Durum:** Issue #64 Mac+Windows gameplay/domain/test/build/native teknik checkpoint'i tamamlandı; final docs/CI ve fiziksel USB kapanışı bekliyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni teknik checkpoint — Issue #64 / Epic #10

- Feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95`, tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`; exact customer/visit/consultation provenance'ına bağlı accepted graphics-first request ve immutable on-satırlı quote/BOM ekledi.
- BOM exact motherboard, CPU, DIMM, M.2 SSD, cooler, GPU, PSU, ATX24, EPS12V ve PCIe/GPU 6+2 rollerini stable line/reservation identity, integer price/currency, compatibility ve budget kapılarıyla bağlar.
- Inventory exact serialized seti tek managed operation/claim ve tek revision ile atomik reserve eder. Claim/operation indexleri aynı registration'ı tutar; access/revision/payload drift'i, external adoption, eleventh item, direct release/consume, duplicate/conflict ve partial failure no-mutation'dır.
- Quote publication kesintisi matching owned Inventory operation üzerinden exact recover edilir; exact replay revision artırmaz. Customer, basket/checkout/economy ve Assembly authority'leri izoledir.
- GarageGraybox `garage-custom-pc-quote-reservation-r33-v1`; gerçek keyboard/mouse ve gamepad input ile consultation→request accepted→visible ten-line quote/BOM→10 exact reservations akışını taşır. Range/focus/LOS/pause/release-repress/single-consumer, accepted deadline ve pause-toggle no-lurch testlidir.
- Mac ve Windows full EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`. Universal Mac build `329396456` bayt ve Apple M1/Metal exact r33 smoke; Windows x64 IL2CPP build `1326137709` report baytı ve Intel Iris Xe/Direct3D 11.0 exact r33 smoke başarılıdır.
- Kısa görünür Mac turu sağ/geri/sol konum değişimini ve pause/resume no-lurch davranışını gözledi. Mouse-look için ayrı manuel geçiş iddiası kurulmadı; automated Input System/native readiness kanıtı korunur.
- Feature [Guard 32698054990](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32698054990) başarılı; draft [PR #65](https://github.com/cixanla/PC-Shop-Empire-3D/pull/65) açıktır. Mac ve Windows masaüstü bağlantıları doğrulanmış build'lere çözülür.
- ADR-0042, tarihli Evidence ve `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` kapanış belge setidir. Kullanıcı USB kablosunun bağlı olduğunu bildirmiştir; ancak son salt-okunur denetimde beklenen `/Volumes/cixanla/CIXANLA` mount'u görünmemiş ve yanlış volume'a yazılmamıştır. Doğru mount göründüğünde iki tam fiziksel readback ve final metadata/Guard yapılacaktır. Issue #64/Roadmap açık/In Progress kalır.

## Önceki teknik checkpoint — Issue #63 / Epic #10

- Feature `ea1e51f862d4094936c03bccf9fbfaee7bb7d12b`, tree `ecc32279a8e17e8179114a9b6cfcfe4737827601`; tek canonical serialized PCIe/GPU 8-pin 6+2 cable, iki distinct typed/keyed endpoint, üç stable ordered waypoint ve capacity-one `GpuPowerCableRoute` eklendi.
- Mevcut dokuz managed container + `GpuPowerCableRoute` tek on-container all-or-none claim'dir. `Loose ↔ Routed`, exact Hands↔route custody, immutable route/unroute receipts, immediate/delayed replay, history fold, retained PSU + secured motherboard + retained GPU lineage ve ATX24/EPS12V isolation testlidir.
- Route yalnız exact keyed orientation, range/focus/LOS ve obstruction-free authored yol üzerinde geçerlidir. Wrong key/endpoint, missing host, duplicate, stale/conflict/full-hands/saturation, generic placement/stack/cart/raw transfer ve routed durumdaki PSU/motherboard/GPU removal no-mutation fail-closed'dur.
- GarageGraybox `garage-pcie-gpu-power-cable-routing-r32-v1`; aynı Unity component instance/stable ItemId ile pickup→preview→route→unroute→world drop/recovery akışını taşır. GPU anchor canonical moving GPU'ya bağlıdır; üç-waypoint preview/committed route jointsizdir.
- Explicit visual fix `d655f1a5aab0c882cf40702472ec1b8ad44747ad`, tree `c3fff116317db7e3388e0faf04e38a7ffaa7ce77`; PSU ucunu monolitik 8-pin bırakır, GPU ucunu ayrı 6-pin + 2-pin housing, 6-pin keyed latch, 2-pin retention clip ve ayrı `6`/`2` etiketleriyle gösterir. Dekoratif child'lar collider/joint/raycast authority değildir; bağımsız salt-okunur yeniden denetim kalan P0/P1 bulmadı.
- Final exact committed-scene kanıtı EditMode `626/626`, PlayMode `53/53`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `329334656` bayt; aktif Apple M1/Metal 1280×720 canonical readiness ve exact `GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE` başarılıdır.
- Feature [Guard 32676069923](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676069923), Windows gate [Guard 32676154473](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676154473) ve explicit 6+2 visual-fix [Guard 32677267023](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677267023) başarılıdır.
- Final source/docs `d597941a20afd0491547513abbc68e0b9d890aab`; clean/exact Windows clone Unity 6000.3.21f1 StrictMode x64 IL2CPP build'ini tamamladı. Build report `1320679269` bayt; build log SHA-256 `459e95bb43ab79a1004e13e71b74c8500f484c9cd33e1f698deb7f277f844799`dır.
- Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact r32 readiness ve `GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE ... recovery=ok` markerını verdi; runtime log SHA-256 `853dd5bd75b63d8938dcd6f9b664e979b43aeafa1409b3678dad143d931b3f9e`dir. [Guard 32677495639](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677495639) başarılıdır; önceki exit `198` denemeleri yalnız tarihsel lisans tanısıdır.
- Issue #63 Mac+Windows teknik kapılarını geçti; final fiziksel USB paketi ve Issue/Roadmap metadata kapanışı henüz yapılmadı. Electrical power-on, wattage/headroom, POST/BIOS/OS, completed benchmark ve diğer kablo aileleri kapsam dışıdır.

## Önceki checkpoint — Issue #62 / Epic #10

- Feature `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`, tree `c14524fecee561eff3a144bd15e67be5a48f8335`; tek canonical serialized EPS12V/CPU cable, iki typed/keyed 8-pin endpoint, üç stable ordered waypoint ve capacity-one `CpuPowerCableRoute` eklendi.
- Dokuz managed container tek all-or-none Assembly claim'inde kalır. `Loose ↔ Routed`, exact Hands↔CpuPowerCableRoute custody, immutable route/unroute receipts, immediate/delayed replay, history fold ve ATX24 isolation testlidir.
- Route yalnız retained PSU + secured motherboard + retained CPU, exact keyed orientation, range/focus/LOS ve obstruction-free authored route üzerinde geçerlidir. Wrong key/endpoint, duplicate, stale/conflict/full-hands, generic placement/stack/cart/raw-transfer ve dependent PSU/motherboard/CPU removal işlemleri no-mutation fail-closed'dur.
- GarageGraybox `garage-eps12v-cpu-power-cable-routing-r31-v1`; tek kinematic root, iki visible connector/latch/key, loose braided presentation ve üç-waypoint preview/committed route taşır. Aynı Unity component instance ve stable ItemId pickup→route→unroute→recovery boyunca korunur.
- Gerçek keyboard/mouse + Input System gamepad route/orientation/commit/unroute, dynamic compact prompt, pause/co-edge drain ve mode-kapalı sıfır-query sözleşmeleri testlidir.
- Final EditMode `610/610`, PlayMode `51/51`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `329206153` bayt; aktif Apple M1/Metal 1280×720 canonical readiness ve exact `GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE` başarılıdır.
- [Feature Guard 32642211422](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642211422) başarılıdır. Source/docs `cff75f8876f893888ca3a98fe5f149dab0f74a1b`, tree `aa5acd799a8190d871aa0c5493fd7484a83b4c4f` ve [Guard 32642638437](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642638437) ile private `main`e ulaştı.
- Yerel final staging `2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_EPS12V_CPU_POWER_CABLE_ROUTING` iki tam 832/832 hash+boyut+yol, 826/826 exact Git source ve 5/5 evidence readback'ini geçti. Payload `15.757.786` bayt, toplam dosya `834`, manifest `afa89feb0252ce5862e7b971949af27b0e2abdd65aafc7ae9a416c1b7adb6a73`; bütün mismatch/forbidden/AppleDouble sayaçları `0`dır.
- Doğru fiziksel USB `/Volumes/cixanla/CIXANLA` olarak, `90_BACKUPS/PCShopEmpire3D` kökü ve önceki Issue #61 milestone zinciriyle doğrulandı. Çakışmayan `.incoming-2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_EPS12V_CPU_POWER_CABLE_ROUTING-904d0c98` kopyası ilk 832/832 readback'i geçtikten sonra atomik final adına taşındı; ikinci 832/832 readback, 826/826 exact Git source ve 5/5 evidence eşliği de sıfır fark/AppleDouble ile geçti. USB metadata `2db7cf984974fd561873d3c06c815b7f47f41d07` ve [Guard 32672086464](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32672086464) başarılıdır; acceptance `21/21`, Issue #62 `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır. Electrical power-on, wattage/headroom, POST/BIOS/OS, completed benchmark, diğer kablo aileleri, final art ve Windows/Steam kapsam dışıdır.

## Önceki checkpoint — Issue #61 / Epic #10

- Feature `1fc29f13171925c2445eaa7334158e0f058e76a5`, source/docs `52795b66fee1eb933d0d9c4ff8cbd7eca512d718`, USB metadata `f9a5da8b23dedd3719c96d50846d4ba3143cc87f`; Guard `32613813494` + `32614187494` + `32632615041`, EditMode `589/589`, PlayMode `49/49`, exact r30 smoke ve iki tam 801/801 USB readback ile kapalı/Done'dur.
- Ayrıntı `Docs/ADR-0039-DETERMINISTIC-SINGLE-ATX24-SPLIT-PSU-CABLE-ROUTING.md` ve tarihli Evidence belgesindedir.

## Önceki checkpoint — Issue #60 / Epic #10

- Feature `f998d7d1c400c9328afa226f0727e6591c02d4e2`, tree `78d62c46354cda45422ca947df10ba9d6823b7c9`; authored-clearance fix `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`, tree `a15865346f52b6b39d84cec49c70babbc6550b89`; tek canonical serialized `AtxPs2` PSU, immutable chassis-owned bay/rear-mount/dört-fastener topology ve atomik yedi-container claim'i eklendi.
- Supported lifecycle `EmptyOpen ↔ PowerSupplySeatedUnsecured ↔ PowerSupplyRetained`dır. Seat/remove exact serialized custody'yi değiştirir; retain/unretain Inventory revision'ını değiştirmez. Exact receipt-history fold, immediate/delayed replay, conflict/stale/full-hands/occupied no-mutation ve alternate-order authority isolation testlidir.
- Yalnız iki keyed 180° fan-intake orientation, exact ATX PS/2 interface, filtered-floor full support, rear-plane, range/focus/LOS/obstruction/tie/saturation kabul edilir. Production authored clearance tam olarak `ChassisBack`, `ChassisLeftRail`, `ChassisRightRail` ve `MotherboardTray` collider'larını kullanır; support yüzeyleri blocker değildir, cable listesi bounded kapsam gereği boştur.
- GarageGraybox `garage-psu-four-screw-r29-v1`; housing, fan/grille, filtered floor intake, AC inlet, rocker switch, disconnected modular panel, rear plate ve dört screw görünürdür. Gerçek keyboard/mouse ve gamepad dynamic compact HUD, co-edge/pause, generic placement/stack/cart bypass yasağı ve same-instance recovery testlidir.
- Final EditMode `577/577`, PlayMode `47/47`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `328937592` bayt; aktif Apple Silicon/Metal makinesinde 1280×720 readiness ve exact PSU smoke başarılıdır. Runtime makineyi Apple M1 olarak tanımlamıştır.
- Source/docs `4939a041635a8864f53f6613a9dc9b4e8972f235`, tree `77a8f66e1fdd53ac2b21e748c6a26a934c49ed02`; [feature Guard 32606958882](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32606958882), [authored-clearance Guard 32607437408](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607437408) ve [source/docs Guard 32607886160](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607886160) başarılıdır. Acceptance `20/20`; Issue #60 `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır.
- Exact source-plus-evidence staging 770 Git source + 4 evidence + source kaydı, 775/775 hash/boyut/path readback, 770/770 Git-blob eşliği, 14.729.691 bayt ve `705784c6b619876fbe6a900a20f870292396a882e0bdcea1f6464bef9a8e53d4` manifestiyle doğrulandı. macOS beklenen USB'yi göstermedi; yanlış volume'a yazılmadı ve fiziksel milestone/readback iddiası kurulmadı.
- Ayrıntı `Docs/ADR-0038-DETERMINISTIC-SINGLE-ATX-PS2-POWER-SUPPLY-SEATING-AND-FOUR-SCREW-RETENTION.md` ve tarihli Evidence belgesindedir. ATX/EPS/PCIe/SATA kablolama, electrical power-on, POST/BIOS/OS, tamamlanmış benchmark, final art ve Windows/Steam ayrı kapılardır.

## Önceki checkpoint — Issue #59 / Epic #10

- Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`; canonical Northstar A60 ProductId'sini kullanan ayrı serialized assembly GPU item'ı, immutable PCIe x16 slot/latch/rear-bracket/fastener topology ve atomik Workbench+ProcessorSocket+MemorySlot+StorageSlot+ProcessorCoolerSlot+GraphicsCardSlot claim'i eklendi.
- Supported lifecycle `EmptyOpen ↔ GraphicsCardSeatedUnsecured ↔ GraphicsCardRetained`dır. Seat/remove exact serialized custody'yi değiştirir; retain/unretain Inventory revision'ını değiştirmez. Retail Northstar A60 item'ı ayrı kalır ve shadow SKU yoktur.
- Exact seat/retain/unretain/remove immediate/delayed replay, duplicate-seat rejection, retained-remove gate, installed-card motherboard-detach host gate, conflict/stale/full-hands no-mutation ve same-instance recovery testlidir.
- Yalnız primary keyed orientation ve exact PCIe x16 interface kabul edilir. Secured host, support, chassis clearance, cooler clearance, range/focus/LOS/obstruction/tie/saturation fail-closed solver ile korunur. Mode kapalı ghost/query sıfırdır; generic placement/stack/cart bypass'ı kapalıdır.
- GarageGraybox `garage-gpu-rear-bracket-r28-v1`; dual-fan GPU, PCB, PCIe contacts, rear bracket, slot latch ve bracket screw görünürdür. Compact HUD gerçek keyboard/mouse ve gamepad bindinglerini dinamik gösterir.
- Final EditMode `548/548`, PlayMode `43/43`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `328781520` bayt; aktif Apple Silicon/Metal makinesinde 1280×720 readiness ve exact GPU smoke başarılıdır. Runtime makineyi Apple M1 olarak tanımlamıştır.
- Source/docs `a5bbca473e81455c44d2f95469c8faf2a11046ff`, tree `1aa335510910bc4ebd60367b41e67dbf99d039b8`; [feature Guard 32599710154](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32599710154) ve [source/docs Guard 32600012769](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32600012769) başarılıdır. Acceptance `20/20`; Issue #59 `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır.
- Kullanıcı fiziksel USB'nin mevcut olmadığını ve yeniden bağlandığında haber vereceğini bildirdi. USB sorgulanmadı; Issue #58'in 717/717 staging'i korunur, Issue #59 için henüz fiziksel milestone/readback iddiası yoktur ve bu dış yedek kapısı gameplay geliştirmesini bloklamaz.
- Ayrıntı `Docs/ADR-0037-DETERMINISTIC-SINGLE-PCIE-X16-GRAPHICS-CARD-SEATING-AND-REAR-BRACKET-RETENTION.md` ve tarihli Evidence belgesindedir. PSU, PCIe power cabling, alternate GPU biçimleri, tam benchmark, final art ve Windows/Steam ayrı kapılardır.

## Daha önceki checkpoint — Issue #58 / Epic #10

- Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`; tek canonical serialized `Lga1700TopDownAirPreAppliedTim` cooler, immutable slot/bracket/dört-point topology ve atomik Workbench+ProcessorSocket+MemorySlot+StorageSlot+ProcessorCoolerSlot claim'i eklendi.
- Supported lifecycle `EmptyOpen ↔ CoolerSeatedUnsecured ↔ CoolerRetained`dır. Seat/remove exact serialized custody'yi değiştirir; retain/unretain Inventory revision'ını değiştirmez. Pre-applied TIM başarılı seat'te yalnız bir kez tüketilir ve aynı item üzerinde kalıcıdır.
- Retention çapraz `1→3→2→4`, release exact ters sıradadır. Immediate/delayed replay, duplicate TIM rejection, installed-cooler CPU-retention/motherboard-detach host gates, conflict/stale/full-hands no-mutation ve same-instance recovery testlidir.
- İki keyed orientation, range/focus/LOS/socket-interface/support/RAM-clearance/obstruction/tie/saturation fail-closed solver ile korunur. Mode kapalı ghost/query sıfırdır; generic placement/stack/cart bypass'ı kapalıdır.
- GarageGraybox `garage-processor-cooler-r27-v1`; cold plate, pre-applied TIM yüzeyi, fin stack, fan, bracket ve dört retention noktası görünürdür. Compact HUD gerçek keyboard/mouse ve gamepad bindinglerini dinamik gösterir.
- Final EditMode `521/521`, PlayMode `38/38`; failed/skipped/inconclusive `0`. Universal macOS Development/StrictMode build `328534723` bayt; aktif Apple Silicon/Metal makinesinde 1280×720 readiness ve exact cooler smoke başarılıdır. Runtime yeni makineyi Apple M1 olarak tanımlamıştır; eski Apple M4 device ifadesi bu checkpoint için kullanılmaz.
- Source/docs `2e848e3bdc5795a349e6c857973c7c88fef36cd7`; [feature Guard 32591206866](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591206866) ve [source/docs Guard 32591381804](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591381804) başarılıdır. USB-erteleme kapanış metadatası `fce6bfa2a6cbd6a425bc9baba1bd54bf1c1a445c` ve [Guard 32593034745](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32593034745) ile private `main`e ulaşmıştır. Acceptance `19/19`; Issue #58 ve Roadmap `Done`dur.
- USB fiziksel aygıtı yeni Mac tarafından görünmediği için kullanıcı talimatıyla ertelendi. `/Users/cixanla/Developer/PCShopEmpire3D/CheckpointStaging/2026-08-22_STAGE_B_DETERMINISTIC_SINGLE_AIR_COOLER_FOUR_POINT_RETENTION` altında 712 exact Git source + 4 evidence + source kaydı, toplam 717/717 payload ve `f7b2b9bafee9529d95431bbc90914ba51ab24e01de9a0d5d77a53f26cb5626a5` manifest hazırdır. Bu dış yedek kapısı gameplay geliştirmesini bloklamaz; kullanıcı USB'yi bağladığını söylediğinde fiziksel readback tamamlanır.
- Ayrıntı `Docs/ADR-0036-DETERMINISTIC-SINGLE-AIR-COOLER-SEATING-AND-FOUR-POINT-RETENTION.md` ve tarihli Evidence belgesindedir. Ayrı paste/reapplication, liquid cooling, GPU/PSU/cabling, tam benchmark, final art ve Windows/Steam ayrı kapılardır.

## Daha önceki checkpoint — Issue #57 / Epic #10

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
- Branch: `feature/issue64-custom-pc-request-quote-reservation`; exact remote HEAD `c7d38845ffccb5ae6e5365e580c238d70f8dac95`, tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`; draft PR #65 ve Guard `32698054990` başarılıdır. Issue #64 Mac+Windows teknik checkpoint'i tamamdır; final docs/CI ve fiziksel USB kapıları nedeniyle Issue/Roadmap açık/In Progress kalır.
- Core stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministic event dispatcher tamamdır.
- Catalog/Inventory/Orders/Retail zinciri; authoritative teslim alma, maliyet provenance'ı, parcel açma, shelf offer, basket reservation, checkout snapshot, prepared completion ve consultation-gated stale-safe Buy/Leave action katmanlarını içerir.
- Downstream `PSE.Economy`; exact-cash settlement receipt'i, immutable ledger transaction/entry kayıtlarını, Cash/SalesRevenue/COGS/InventoryAsset hesaplarını, balance ve gross-margin sorgularını içerir. Retail/Inventory/Orders Economy'ye ters referans taşımaz.
- Actors sınırı; kararlı müşteri intent/visit modeli, monotonik lifecycle, bounded route retry/fallback, `OfferDeclined`, command receipt ledger ve visit-owned immutable consultation authority'sini içerir. `AwaitingCheckout` sonrası fulfillment/çıkış canonical Economy settlement receipt'ine bağlıdır.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; runtime marker `garage-custom-pc-quote-reservation-r33-v1`.
- Fiziksel checkout station range/focus/LOS/pause ve exact visit/provenance gate'lerini taşır; shelf/uzak ödeme bypass'ı yoktur. Versioned primary press tek tüketicilidir ve release/repress settlement sözleşmesi gerçek Input System testleriyle kilitlidir.
- PC assembly dilimi exact serialized anakart, fastener, CPU, DDR5 DIMM, M.2 NVMe, air cooler, PCIe GPU, ATX PS/2 PSU, ATX24, EPS12V ve PCIe/GPU 6+2 kablo authority'lerini tek managed Inventory gerçeği üzerinde korur. Attach/secure/seat/retention/route/detach/recovery lineage'ı ve stable world identity authoritative'dir. Issue #64 accepted request'ten immutable on-satırlı quote/BOM ve aynı exact serialized setin atomik reservation'ına uzanır.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj, kutular, eller ve müşteri final sanat değildir.
- Gerçek Windows x64 IL2CPP toolchain ve Unity Editor lisansı hazırdır; exact r32 ve r33 Windows build/DirectX native smoke kapıları başarılıdır. Steam entegrasyonu, release depot/signing ve geniş uyumluluk QA'sı ayrıca yapılmamıştır.

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

- En yeni doğrulanmış milestone: `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_EPS12V_CPU_POWER_CABLE_ROUTING`.
- Source/docs `cff75f8876f893888ca3a98fe5f149dab0f74a1b`; 826 tracked `SOURCE`, 5 final `EVIDENCE` ve bir `SOURCE_COMMIT.txt`; 832 manifest payload satırı, toplam 834 dosya ve 15.757.786 payload baytı.
- `MANIFEST.tsv` SHA-256: `afa89feb0252ce5862e7b971949af27b0e2abdd65aafc7ae9a416c1b7adb6a73`.
- `.incoming-*` kopyası ve atomik final adlandırmadan sonra iki tam geri okuma da 832/832 hash+boyut+yol, 826/826 exact Git source ve 5/5 evidence eşliğiyle geçti. Path-set, forbidden/cache/credential, internal AppleDouble ve sibling sidecar mismatch sayıları `0`dır. Bu Issue #62 milestone'udur; Issue #63 ve #64 için henüz fiziksel final package/readback iddiası yoktur.
- Kullanıcı 24 Ağustos'ta USB kablosunun bağlı olduğunu bildirdi; son salt-okunur `diskutil` ve `/Volumes` denetiminde beklenen `/Volumes/cixanla/CIXANLA` mount'u görünmedi. Yanlış volume'a yazılmaz; doğru volume görünene kadar yalnız yerel staging/GitHub kapıları ilerletilir.
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

1. Issue #64 yaşayan belge setini doğrula, docs checkpoint commitini feature branch'e push et ve Repository Guard/PR #65 durumunu yeniden doğrula.
2. Issue #63 ve #64 için exact source-plus-evidence yerel staging paketlerini çakışmayan adlarla hazırla. Beklenen `/Volumes/cixanla/CIXANLA` görünürse yalnız doğrulanmış kökte `.incoming-*`, AppleDouble temizliği yalnız incoming hedefinde, iki tam manifest/Git/evidence readback ve atomik final adlandırma uygula; mount görünmüyorsa USB'ye yazma.
3. Ana gameplay hattında accepted request → immutable quote/BOM → exact serialized reservation akışını, tek fiziksel build-order/work-ticket ve iş tezgâhı devrine bağlayan bounded dilimi başlat. Reservation authority'sini Assembly custody ile karıştırma; electrical power-on/POST/OS/benchmark daha sonraki bağımlı kapılar olarak kalsın.

## Güvenli devam komutu

Issue #64 feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95`, tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`, Guard `32698054990`, EditMode `647/647`, PlayMode `59/59`, Universal Mac `329396456` bayt ve Windows x64 IL2CPP report `1326137709` bayt ile Apple M1/Metal ve Intel Iris Xe/Direct3D 11.0 exact r33 native smoke kapılarını geçti. Draft PR #65 açık; final docs/CI ve fiziksel USB kapanışı bekler. USB kablosu bildirilmiş olsa da beklenen mount görünmemiştir; yanlış volume'a yazma. Sonraki bounded gameplay işi exact reservation'dan fiziksel build-order/work-ticket ve iş tezgâhı devridir; electrical readiness ve completed benchmark iddiası yoktur.

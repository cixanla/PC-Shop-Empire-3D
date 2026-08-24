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

### Güncel checkpoint — Issue #66 teknik, source-docs ve yerel staging kapıları tamamlandı / USB kapanışı bekliyor

- Core feature `f9545605baff423f05615e7326902e24dc82aeeb`; current technical head `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, tree `69ea366cc49e99b653f5d02d9c0f238b4906de69`. Stable BuildOrder/WorkTicket/Operation kimlikleri accepted exact quote/reservation setini tek immutable job ve workbench ticket'a bağlar.
- Inventory exact ten-item managed seti commit öncesi yeniden doğrular ve tek terminal allocation receipt'i exactly-once yayınlar. Reservations/items yerinde ve canlı kalır; move/delete/release/consume, second allocation, orphan recovery ve mismatched replay fail-closed'dur.
- GarageGraybox `garage-custom-pc-work-ticket-r34-v1`; canonical workbench physical ticket'ı job identity, `10/10` ve `MONTAJA HAZIR • HENÜZ BAŞLAMADI` gösterir. Range/focus/LOS/empty-hands/fresh Interact, pause/co-edge/competing-target ve gerçek keyboard/mouse/gamepad customer→workbench rotası testlidir. Assembly untouched kalır.
- EditMode `661/661`, PlayMode `66/66`; Universal Mac `329478891` bayt ve Apple M1/Metal exact r34 smoke başarılıdır. Same-frame ticket/carry/cart Interact ownership ve kapsamlı player/item/Inventory/Assembly no-teleport snapshot'ları bu current source üzerinde yeniden doğrulandı.
- Clean exact Windows head x64 IL2CPP + only-Direct3D11 buildi `1328828053` report baytı üretmiş, ProjectSettings restore/readback `byte-exact` geçmiştir. Interactive Intel Iris Xe/Direct3D 11.0 player host/r34/work-ticket markerlarını birer kez, forbidden markerını sıfır vermiştir.
- Technical-source [Guard 32721069982](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32721069982) başarılıdır. Exact source/docs `4e1ef4322d9ef049e3aac915c611474f6bee92fd`, tree `4df76fb1b50da53bdee7e65cb64acf0e73a5c018` ve [Guard 32723213686](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32723213686) başarılı; draft [PR #67](https://github.com/cixanla/PC-Shop-Empire-3D/pull/67) bu checkpoint'e bağlıdır. ADR-0043 ve tarihli Evidence exact receiptsi ve canonical `9/9` allowlist'i taşır; kaynak `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62`dir.
- Yerel final staging `2026-08-24_STAGE_B_IMMUTABLE_CUSTOM_PC_WORK_ORDER_PHYSICAL_WORK_TICKET_HANDOFF` incoming ve final adlarında `906/906` manifest, `896/896` exact Git source, `9/9` evidence, `17.330.935` bayt ve `1514481a…4121` manifest ile geçti. USB kimliği ve önceki milestone zinciri yazımdan hemen önce yeniden doğrulanmalıdır; yanlış volume'a yazılmaz. Fiziksel USB iki tam readback, final metadata/Guard, acceptance `18/18`, Issue/Roadmap kapanışı ve PR merge bekler.

### Önceki checkpoint — Issue #64 Mac ve Windows teknik kapıları tamamlandı / final docs-CI ve USB kapanışı bekliyor

- Feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95`, tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`; exact customer/visit/consultation provenance'ına bağlı accepted graphics-first custom-PC request ve immutable on-satırlı quote/BOM ekler.
- BOM exact motherboard, CPU, DIMM, M.2 SSD, cooler, GPU, PSU, ATX24, EPS12V ve PCIe/GPU 6+2 item'larını stable line/reservation kimlikleri, integer price/currency, compatibility ve budget kapılarıyla bağlar.
- Inventory exact serialized seti tek managed operation ve tek revision ile atomik reserve eder. Claim/operation/access/revision/payload kayıtları aynı registration üzerinde çapraz doğrulanır; exact replay, interrupted-publication recovery, conflict/drift ve direct release/consume bypass'ları fail-closed'dur.
- GarageGraybox `garage-custom-pc-quote-reservation-r33-v1`; gerçek keyboard/mouse ve gamepad ile consultation→accepted request→visible quote/BOM→10 exact reservations akışını gösterir. Range/focus/LOS/pause/release-repress/single-consumer ve accepted-deadline dayanıklılığı testlidir.
- Mac ve Windows full regression EditMode `647/647`, PlayMode `59/59`; failed/skipped/inconclusive `0`. Universal Mac build `329396456` bayt, Apple M1/Metal r33 smoke; Windows x64 IL2CPP build `1326137709` report baytı, Intel Iris Xe/Direct3D 11.0 r33 smoke başarılıdır.
- Mac masaüstü bağlantısı güncel Universal app'e; Windows masaüstü `.lnk` dosyası doğrulanmış IL2CPP player'a çözülür. Kısa görünür Mac turu hareket ve pause/resume no-lurch kapısını gözledi; mouse-look için ayrı manuel başarı iddiası kurulmadı.
- Feature [Repository Guard 32698054990](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32698054990) başarılıdır; draft [PR #65](https://github.com/cixanla/PC-Shop-Empire-3D/pull/65) açıktır. Ayrıntı: ADR-0042 ve tarihli Issue #64 Evidence belgesi.
- `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` bundan sonraki playable paketlerde domain, sahne/girdi, full regression, native platform ve insan oturumu kanıtlarını ayrı zorunlu kapılar olarak tanımlar.
- Kullanıcı USB kablosunun bağlı olduğunu bildirdi; ancak son salt-okunur aygıt denetiminde beklenen `/Volumes/cixanla/CIXANLA` mount'u görünmedi. Yanlış volume'a yazılmadı. Doğru volume göründüğünde iki tam fiziksel readback, final metadata commit/Guard ve Issue/Roadmap kapanışı yapılacaktır. Issue #64 açık/In Progress kalır.

### Önceki checkpoint — Issue #63 Mac ve Windows teknik kapıları tamamlandı / final USB kapanışı bekliyor

- Feature `ea1e51f862d4094936c03bccf9fbfaee7bb7d12b`, tree `ecc32279a8e17e8179114a9b6cfcfe4737827601`; tek canonical serialized PCIe/GPU 8-pin 6+2 cable'ı iki typed/keyed endpoint, üç ordered waypoint ve capacity-one `GpuPowerCableRoute` ile GarageGraybox r32'ye ekler.
- Inventory on managed container'ı atomik claim eder. Assembly yalnız `Loose ↔ Routed`, exact Hands↔GpuPowerCableRoute custody, immutable receipt/history/replay, retained PSU + secured motherboard + retained GPU lineage ve ATX24/EPS12V isolation sözleşmelerini kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile dedicated route mode, iki keyed orientation, görünür authored route, exact unroute, world drop ve recovery akışını oynar. Generic placement/stack/cart/raw-transfer bypass'ları kapalıdır; routed cable PSU/motherboard/GPU dependent removal işlemlerini engeller.
- Explicit visual fix `d655f1a5aab0c882cf40702472ec1b8ad44747ad`, tree `c3fff116317db7e3388e0faf04e38a7ffaa7ce77`; PSU tarafını monolitik 8-pin, GPU tarafını ayrı 6-pin + 2-pin housing, keyed latch, retention clip ve `6`/`2` labels olarak kilitler. Presentation child'lar collider/joint/raycast authority değildir; bağımsız yeniden denetim kalan P0/P1 bulmadı.
- Final committed-scene EditMode `626/626`, PlayMode `53/53`; Universal macOS build `329334656` bayt ve aktif Apple M1/Metal 1280×720 exact r32 PCIe/GPU smoke başarılıdır. Mac masaüstü kısayolu güncel build'e çözülür.
- Feature [Guard 32676069923](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676069923), Windows gate [Guard 32676154473](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32676154473) ve visual-fix [Guard 32677267023](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677267023) başarılıdır. Ayrıntı: `Docs/ADR-0041-DETERMINISTIC-SINGLE-PCIE-GPU-POWER-CABLE-ROUTING.md` ve tarihli Evidence belgesi.
- Final source/docs `d597941a20afd0491547513abbc68e0b9d890aab`; Windows clean clone aynı exact head'de Unity 6000.3.21f1 StrictMode x64 IL2CPP build'i tamamladı. Build report `1320679269` bayt; log SHA-256 `459e95bb43ab79a1004e13e71b74c8500f484c9cd33e1f698deb7f277f844799`dır.
- Windows Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact r32 readiness ve `GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE ... recovery=ok` markerını verdi; runtime log SHA-256 `853dd5bd75b63d8938dcd6f9b664e979b43aeafa1409b3678dad143d931b3f9e`dir. [Repository Guard 32677495639](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32677495639) başarılıdır; önceki exit `198` denemeleri yalnız tarihsel lisans tanısıdır.
- Issue #63 için final fiziksel USB paketi henüz yazılmadı. Teknik Mac+Windows kapıları başarılıdır; yalnız doğru USB mount'u, iki tam readback ve final Issue/Roadmap metadata kapanışı bekler. Electrical power-on, wattage/headroom, POST/BIOS/OS ve completed benchmark ayrı kapılardır.

### Önceki checkpoint — Issue #62 teknik ve fiziksel USB kapıları tamamlandı / kapalı ve Done

- Feature `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`, tree `c14524fecee561eff3a144bd15e67be5a48f8335`; tek canonical serialized EPS12V/CPU cable'ı iki typed/keyed 8-pin endpoint ve üç ordered waypoint ile GarageGraybox r31'e ekler.
- Inventory dokuz managed container'ı atomik claim eder. Assembly yalnız `Loose ↔ Routed`, exact Hands↔CpuPowerCableRoute custody, immutable receipt/history/replay, retained PSU + secured motherboard + retained CPU lineage ve ATX24 isolation sözleşmelerini kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile dedicated route mode, iki keyed orientation, görünür authored route, exact unroute ve recovery akışını oynar. Generic placement/stack/cart/raw-transfer bypass'ları kapalıdır; routed cable PSU/motherboard/CPU dependent removal işlemlerini engeller.
- Tek physical root, iki connector/latch/key, loose braided presentation ve üç-waypoint jointsiz LineRenderer route aynı Unity component instance/stable ItemId'yi pickup, route, unroute ve recovery boyunca korur.
- Final EditMode `610/610`, PlayMode `51/51`; Universal macOS build `329206153` bayt ve aktif Apple M1/Metal 1280×720 exact r31 EPS12V smoke başarılıdır.
- Feature private `main`e push edilmiştir; [Guard 32642211422](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642211422) başarılıdır. Source/docs `cff75f8`, tree `aa5acd7`, [Guard 32642638437](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32642638437) başarılıdır. Ayrıntı: `Docs/ADR-0040-DETERMINISTIC-SINGLE-EPS12V-CPU-POWER-CABLE-ROUTING.md` ve tarihli Evidence belgesi.
- Yerel final staging iki tam 832/832 payload, 826/826 exact Git source ve 5/5 evidence readback'ini geçti; payload 15.757.786 bayt, manifest `afa89feb…6a73`, bütün fark sayaçları `0`dır.
- Doğru fiziksel USB `/Volumes/cixanla/CIXANLA` olarak doğrulandı. Çakışmayan incoming paket ilk tam readback sonrasında atomik final adına taşındı; fiziksel USB üzerinde iki tam 832/832 payload, 826/826 exact Git source ve 5/5 evidence readback sıfır fark/AppleDouble ile geçti. USB metadata `2db7cf9` ve [Repository Guard 32672086464](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32672086464) başarılıdır; acceptance `21/21`, Issue #62 `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır.
- Electrical power-on, PCIe/GPU, SATA/Molex/fan/front-panel/data/RGB cabling, POST/BIOS/OS, completed benchmark, final art ve native Windows/Steam ayrı kapılardır.

### Önceki checkpoint — Issue #60

- Feature `f998d7d1c400c9328afa226f0727e6591c02d4e2`, tree `78d62c46354cda45422ca947df10ba9d6823b7c9`; authored-clearance fix `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`, tree `a15865346f52b6b39d84cec49c70babbc6550b89`; tek canonical serialized ATX PS/2 PSU item'ını stable chassis-owned bay/rear-mount/four-fastener topology ile GarageGraybox r29'a ekler.
- Inventory authority yedi managed container'ı atomik claim eder. Assembly yalnız `EmptyOpen ↔ PowerSupplySeatedUnsecured ↔ PowerSupplyRetained` lifecycle'ını, exact receipt fold/replay'i, alternate-order isolation'ı ve no-mutation failure yollarını kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile guided mode, iki keyed fan orientation, exact ATX seat, görünür crossed four-screw retain/unretain ve same-instance remove/recovery akışını oynar. Generic placement/stack/cart bypass'ı kapalıdır.
- Production clearance tam dört gerçek collider'a bağlıdır: `ChassisBack`, `ChassisLeftRail`, `ChassisRightRail`, `MotherboardTray`. Support yüzeyleri blocker değildir; cable blocker listesi kablolama kapsam dışı olduğu için boştur.
- Final EditMode `577/577`, PlayMode `47/47`; Universal macOS build `328937592` bayt ve aktif Apple Silicon/Metal 1280×720 exact r29 PSU smoke başarılıdır. Runtime makineyi Apple M1 olarak tanımlamıştır.
- Feature `f998d7d`, authored-clearance fix `b6c3ff8` ve source/docs `4939a04` private `main`e push edilmiştir; Guard `32606958882`, `32607437408` ve `32607886160` başarılı, acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`dur. Parent Epic #10 açık/In Progress kalır.
- Exact local staging 770 Git source + 4 evidence + source kaydıyla 775/775 readback, 770/770 Git-blob eşliği, 14.729.691 bayt ve `705784c6…53d4` manifestiyle doğrulandı. macOS beklenen harici fiziksel USB'yi mount etmedi; yanlış volume'a yazılmadı ve fiziksel milestone/readback iddiası kurulmadı.
- Ayrıntı: `Docs/ADR-0038-DETERMINISTIC-SINGLE-ATX-PS2-POWER-SUPPLY-SEATING-AND-FOUR-SCREW-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-ATX-PS2-POWER-SUPPLY-SEATING-AND-FOUR-SCREW-RETENTION-CHECKPOINT-2026-08-23.md`.
- ATX/EPS/PCIe/SATA cabling, electrical power-on, POST/BIOS/OS, completed benchmark, final art ve native Windows/Steam ayrı kapılardır.

### Önceki checkpoint — Issue #59

- Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`, canonical Northstar A60 ProductId'sini koruyan ayrı serialized assembly GPU item'ını stable PCIe x16 slot/latch/rear-bracket/fastener topology ile GarageGraybox r28'e ekler.
- Inventory authority altı managed container'ı atomik claim eder. Assembly supported runtime'da yalnız `EmptyOpen ↔ GraphicsCardSeatedUnsecured ↔ GraphicsCardRetained` state'ini, exact receipt lineage/replay'i, installed-card motherboard-detach gate'ini ve no-mutation failure yollarını kabul eder.
- Oyuncu gerçek keyboard/mouse veya gamepad ile guided mode, keyed 0°/180° orientation, PCIe seat, görünür slot-latch/rear-bracket retain-unretain ve same-instance remove/recovery akışını oynar. Generic placement/stack/cart bypass'ı kapalıdır.
- Final EditMode `548/548`, PlayMode `43/43`; Universal macOS build `328781520` bayt ve aktif Apple Silicon/Metal 1280×720 exact r28 GPU smoke başarılıdır. Runtime makineyi Apple M1 olarak tanımlamıştır.
- Feature `1b29ad2` ve source/docs `a5bbca4` private `main`e push edilmiştir; Guard `32599710154` ve `32600012769` başarılı, acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`dur. Parent Epic #10 açık/In Progress kalır.
- Fiziksel USB kullanıcı talimatıyla ertelendi. Issue #58'in 717/717 yerel staging'i korunur; Issue #59 için henüz fiziksel USB milestone/readback iddiası yoktur. Kullanıcı USB'yi yeniden bağladığını söyleyene kadar USB sorgulanmaz ve geliştirme durmaz.
- Ayrıntı: `Docs/ADR-0037-DETERMINISTIC-SINGLE-PCIE-X16-GRAPHICS-CARD-SEATING-AND-REAR-BRACKET-RETENTION.md` ve `Docs/Evidence/DETERMINISTIC-SINGLE-PCIE-X16-GRAPHICS-CARD-SEATING-AND-REAR-BRACKET-RETENTION-CHECKPOINT-2026-08-22.md`.
- PSU, PCIe power cabling, alternate card/slot biçimleri, tam benchmark, final art ve native Windows/Steam ayrı kapılardır.

### Daha önceki checkpoint — Issue #58

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
- Windows final doğrulaması için SSH erişimli gerçek Windows x64 host, Unity 6000.3.21f1 IL2CPP/C++ toolchain ve Intel Iris Xe/DirectX runtime hazırdır.
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

- Edit Mode `661/661` passed.
- Play Mode `66/66` passed.
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
- Issue #64 exact customer/visit/consultation provenance'ından accepted graphics-first custom-PC request, immutable on-satırlı quote/BOM ve motherboard/CPU/DIMM/M.2/cooler/GPU/PSU/ATX24/EPS12V/PCIe 6+2 için exact serialized Inventory reservation ekledi. Atomik claim/operation/revision, exact replay ve interrupted-publication recovery fail-closed'dur.
- GarageGraybox r33 aynı akışı gerçek keyboard/mouse ve gamepad ile görünür kılar; Mac ve Windows native build/runtime kapıları ile `647/647` EditMode ve `59/59` PlayMode regression başarılıdır. Bu Issue #64 tarihsel sınırıdır.
- Issue #66 exact quote/reservation setini immutable BuildOrder + WorkTicket'a ve exactly-once Inventory allocation receipt'e bağladı. On reservation/item canlı, yerinde ve unchanged kalır; Assembly authority untouched'dır.
- GarageGraybox r34 canonical workbench physical ticket'ı job identity, `10/10` ve assembly-not-started statusuyla görünür kılar. Mac+Windows native r34 ve `661/661` EditMode + `66/66` PlayMode geçti. Sonraki bounded bağımlılık allocated exact item setinin fiziksel component transfer/build-kit completion sınırıdır; power-on/POST/OS/benchmark henüz iddia edilmez.
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
- Steamworks entegrasyonu, release signing/depot matrisi ve geniş Windows donanım/uzun oturum QA'sı; temel native Windows x64 IL2CPP/DirectX teknik kapısı artık başarılıdır.

Issue #66 core feature `f954560`, technical source `f8afd62`, tree `69ea366`, Guard `32721069982`, EditMode `661/661`, PlayMode `66/66`, Universal Mac `329478891` bayt ve Windows x64 IL2CPP report `1328828053` bayt ile Mac Apple M1/Metal ve Windows Intel Iris Xe/D3D11 exact r34 native smoke kapılarını geçti. Source/docs `4e1ef43`, tree `4df76fb`, Guard `32723213686` ve local immutable staging `906/906` payload, `896/896` Git source, `9/9` evidence, `17.330.935` bayt, `1514481a…4121` manifest ile doğrulandı. Canonical evidence kaynağı `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62`dir. Draft PR #67 açıktır. Fiziksel USB iki-readback kapanışı bekler; ardından bounded iş canonical motherboard'ın gerçek pickup/carry/placement ile dedicated build-kit slotuna taşınması ve `0/10 → 1/10` fiziksel ilerlemesidir. Assembly completion, electrical power-on, tam benchmark, Save, Guardian ve final art ayrı kapılardır.

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
2. Repo guard, güncel 647 Edit Mode ve 59 Play Mode testi geçti.
3. Vizyon ile vertical slice sınırını kendi cümlesiyle açıklayabildi.
4. GitHub Project'te sıradaki issue/acceptance kriterini buldu.
5. Küçük bir docs/test PR'ını yaşayan belge kurallarına uygun açabildi.

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

### Güncel checkpoint — Issue #83 r42 teknik ve exact-head Mac/Windows native kapıları tamamlandı; source/docs, USB, insan ve idari kapanış sürüyor

- Technical head `a36d713120283bd106aeca76509756d6dbb1dd30`, tree `2619dd8e1db812c9e3249657a2031a6268492b5a`. Canonical reserved ATX24 yalnız exact `PowerCable` + `ModularAtx24SplitPsuToMotherboard` role ve tam work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür.
- ATX24, staged canonical motherboard+CPU+DDR5+M.2+processor-cooler+graphics-card+power-supply sonrasında ayrı stable operation ve ATX24-specific managed capacity-one BuildKit üzerinden WorldFloor → ActorHands → ATX24 BuildKit custody'sine taşınır. Octuple container claim all-or-none'dur; ilk yedi slot/receipt/replay/revision/staged state korunur.
- GarageGraybox r42 ayrı tray/support/snap anchor, keyed `0° ↔ 180°` preview ve görünür `7/10 → 8/10` aggregate ekler. Active receipt primary/rotate/interact/drop input'unun tek sahibidir; Issue #61 ATX24 installed route, #62/#63 cable routes, #60 PSU bay ve diğer Assembly yolları aynı frame'i çalamaz.
- Full EditMode `701/701`, PlayMode `110/110`; failed/skipped/inconclusive `0`. Universal Mac report `329963160` bayt, valid deep/strict universal executable ve Apple M1/Metal exact r42 readiness + ATX24 markerı başarılıdır.
- Complete bundle `7317338` bayt / `140d44d…5daa`; detached-clean Windows exact head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1337139191` bayt, `issue83-hardened-v1` fatal-token `0` verdi. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve player/Unity/task residue `0` ile geçti.
- ADR-0051 ve tarihli Evidence exact source/tests/Mac/Windows/binary/procedure kanıtını bağlar. Marker açıkça `prerequisite-positioning=teleport-assisted` der; otomatik smoke exact-r42 gerçek insan oturumu yerine geçirilmez.
- Source/docs Repository Guard, canonical `14/14` receipt, immutable local/physical-USB çift readback, fiziksel metadata ve Issue #83 acceptance `25/25` henüz tamamlanmamıştır. Mevcut kesin durum `22/25` pass; insan kapısı ve iki lifecycle kapısı bekler. Issue #83 açık/In Progress, parent Epic #10 açık/In Progress kalır.
- Ana ürün kapsamı ayrıca küçükten büyüğe mağaza, personel/müşteri yönetimi, işlevsel mahalle, kişisel ev, araç/kargo/lojistik, dünya NPC ekolojisi, gerçekçi ses/zaman/hava, kaliteye bağlı içerik boyutu ve Guardian'dan ayrılmış güvenli/isteğe bağlı yerel danışman AI sınırlarıyla güncellendi. Retail çekirdek OpenAI/ChatGPT/Codex/internet/model indirme bağı olmadan çalışacaktır.

### Önceki checkpoint — Issue #81 r41 teknik/native, immutable local/physical-USB ve metadata CI kapıları tamamlandı; exact-build insan oturumu/idari kapanış sürüyor

- Technical head `f3d80629e09c05afde97fa778c4b220ca456c5f0`, tree `851954879c1ff1e2ef98bc9a7a8469750304d992`. Canonical reserved PSU yalnız exact `ComponentKind == PowerSupply` ve tam work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür.
- PSU, staged canonical motherboard, processor, DDR5, M.2, processor cooler ve graphics card prerequisites sonrasında ayrı stable operation ve power-supply-specific managed capacity-one BuildKit container üzerinden source → ActorHands → Power Supply BuildKit custody'sine taşınır. Mevcut altı slot/receipt/replay/revision/staged state korunur.
- GarageGraybox r41 ayrı PSU tray/support/snap anchor, keyed `0° ↔ 180°` preview ve görünür `6/10 → 7/10` aggregate ekler. Active PSU BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; Issue #60 PSU-bay Assembly ve Issue #61–#63 cable routes aynı-frame input'u çalamaz, receipt'siz normal yollar değişmez.
- Native prerequisite harness'ı production Update order'ını atlayan same-frame zorlanmış `InputSystem.Update()` + doğrudan work-ticket station çağrısından arındırıldı. Neutral → pressed → released gerçek player frame'leri kullanılır; pressed-frame failure diagnostics release öncesinde yakalanır.
- Final EditMode `697/697`, PlayMode `105/105`; failed/skipped/inconclusive `0`. Universal Mac report `329907140` bayt, valid deep/strict signed universal player ve Apple M1/Metal exact r41 readiness + Power Supply BuildKit smoke başarılıdır.
- Complete bundle'dan collision-free detached-clean Windows exact head üretildi. Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1335888266` bayt ve `issue81-hardened-v1` fatal-token `0` verdi. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact host/r41 readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve player/Unity/task residue `0` ile geçti.
- Teknik branch private GitHub'a push edildi; source/docs `dc118bf0d26a11f3937cb114ef12f85666facc48`, tree `ac9fcb5d38855ed37f2ee36449100b5094287cb8` ve [Repository Guard 32896033674](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32896033674) başarılıdır. ADR-0050 ve tarihli Evidence exact test/Mac/Windows/binary/procedure hashlerini bağlar; canonical evidence exact `14/14`dır.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming→atomik final→ikinci readback zinciri aynı `1002/1002` payload, `987/987` exact Git source, `14/14` evidence, `19368668` bayt ve `69cc892b…06ab` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar ve Windows işlem artığı `0`dır.
- Fiziksel metadata `ff935452c68bc77e66eb0742e0c3e6c0eb2894c7`, tree `e4f03fc3c2d6dfd44da61eaae3a161af4f104eae` ve [Guard 32897672990](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32897672990) başarılıdır.
- `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` uyarınca exact r41 build üzerinde gerçek insan oturumu henüz kaydedilmedi; otomatik native rota bunun yerine geçirilmez. Bu nedenle acceptance `23/24`, Issue #81 açık/In Progress ve PR #82 taslak kalır. İnsan oturumu geçince `24/24`, Roadmap `Done` ve idari kapanış yapılır; parent Epic #10 açık/In Progress kalacaktır.

### Önceki checkpoint — Issue #79 r40 teknik, fiziksel USB ve lifecycle kapıları tamamlandı / kapalı ve Done

- Technical head `f40ef21058caf1a2aca3054218abfc1dd7305c01`, tree `c7500e7300f75f5d9b089bf23657750dccc5ffed`. Canonical reserved graphics card yalnız exact `ComponentKind == GraphicsCard` ve tam work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür.
- GPU, staged canonical motherboard, processor, DDR5, M.2 ve processor-cooler prerequisites sonrasında ayrı stable operation ve graphics-card-specific managed capacity-one BuildKit container üzerinden source → ActorHands → Graphics Card BuildKit custody'sine taşınır. Mevcut beş slot/receipt/replay/revision/staged state korunur.
- GarageGraybox r40 ayrı GPU tray/support/snap anchor, keyed 180° half-turn preview ve görünür `5/10 → 6/10` aggregate ekler. Active GPU BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; Issue #59 GPU seat/retention ile Issue #63 PCIe route aynı-frame input'u çalamaz ve receipt'siz normal Assembly yolları değişmez.
- Final EditMode `690/690`, PlayMode `100/100`; failed/skipped/inconclusive `0`. Universal Mac report `329839788` bayt, valid signed universal player ve Apple M1/Metal exact r40 readiness + Graphics Card BuildKit smoke başarılıdır.
- Complete bundle'dan collision-free detached-clean Windows exact head üretildi. Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1334256694` bayt ve `issue79-hardened-v3` fatal-token `0` verdi; `ProjectSettings.asset` restoration byte-exact'tir. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact host/r40 readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti.
- Teknik branch private GitHub'a push edildi; source/docs `dd607d0af346bd1f0e28449f606761bc97e1495c`, tree `010b3a460c3241ed69d315bfb44047c1be82cb10` ve [Repository Guard 32874685021](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32874685021) başarılıdır. ADR-0049 ve tarihli Evidence exact test/Mac/Windows/procedure hashlerini bağlar; canonical evidence exact `14/14`dır.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming→atomik final→ikinci readback zinciri aynı `990/990` payload, `975/975` exact Git source, `14/14` evidence, `20086932` bayt ve `d2d399fa…b324` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar `0`dır.
- Fiziksel metadata `880523fcb71208796cce96564556a2170363c92a`, tree `448052665c3b64b1c565d460de6c648c498b698d` ve [Repository Guard 32876194890](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32876194890) başarılıdır. Acceptance `24/24`, Issue #79 `CLOSED`, Roadmap `Done`; PR #80 integration aracıdır, Issue #77 ve parent Epic #10 açık/In Progress kalır.

### Önceki checkpoint — Issue #77 r39 teknik ve Mac/Windows native kapıları tamamlandı / source-docs, CI ve fiziksel lifecycle bekliyor

- Technical head `197233688c4fe587097dbfc1cbee843cfc78603e`, tree `58458f400a7efaa68e452a0e85e35d6d7eb5a3ab`. Canonical reserved processor cooler yalnız exact `ComponentKind == ProcessorCooler` ve tam work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür.
- Cooler, staged canonical motherboard, processor, DDR5 ve M.2 prerequisites sonrasında ayrı stable operation ve cooler-specific managed capacity-one BuildKit container üzerinden source → ActorHands → Processor Cooler BuildKit custody'sine taşınır. Mevcut dört slot/receipt/replay/revision/staged state korunur.
- GarageGraybox r39 ayrı cooler tray/support/snap anchor, keyed 90° quarter-turn preview ve görünür `4/10 → 5/10` aggregate ekler. Active cooler BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; Issue #58 cooler-seat/four-point-retention/TIM aynı-frame input'u çalamaz ve receipt'siz normal Assembly yolu değişmez.
- Final EditMode `686/686`, PlayMode `96/96`; failed/skipped/inconclusive `0`. Universal Mac report `329787583` bayt, valid signed universal player ve Apple M1/Metal exact r39 readiness + Processor Cooler BuildKit smoke başarılıdır.
- Complete bundle'dan collision-free detached-clean Windows exact head üretildi. Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1333221634` bayt ve `issue77-hardened-v2` fatal-token `0` verdi; `ProjectSettings.asset` restoration byte-exact'tir. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact host/r39 readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti.
- Teknik branch private GitHub'a push edildi. ADR-0048 ve tarihli Evidence exact test/Mac/Windows/procedure hashlerini bağlar; diğer 13 immutable artifact Mac canonical evidence dizinindedir. Exact dokuz dosyalık source/docs commit, Repository Guard, final receipt, immutable local/physical USB double-readback ve acceptance `24/24` hâlâ kapanış kapılarıdır. Issue #77 açık/In Progress ve parent Epic #10 açık/In Progress kalır.

### Önceki checkpoint — Issue #75 r38 teknik, fiziksel USB ve lifecycle kapıları tamamlandı / kapalı ve Done

- Technical head `646e66cfa269a217ecb1f6942f9accb77f9e463c`, tree `ee9b0b2c0bb5e1fb07de397da222d00a7480b23c`. Canonical reserved M.2 NVMe yalnız exact `ComponentKind == StorageDevice` ve tam work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür.
- NVMe, staged canonical motherboard, processor ve DDR5 prerequisites sonrasında ayrı stable operation ve storage-specific managed capacity-one BuildKit container üzerinden source → ActorHands → Storage BuildKit custody'sine taşınır. Mevcut üç slot/receipt/replay/revision/staged state korunur.
- GarageGraybox r38 ayrı storage tray/support/snap anchor, 180° keyed preview ve görünür `3/10 → 4/10` aggregate ekler. Active Storage BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; Issue #57 M.2 seat/secure aynı-frame input'u çalamaz ve receipt'siz normal Assembly yolu değişmez.
- Final EditMode `683/683`, PlayMode `90/90`; failed/skipped/inconclusive `0`. Universal Mac report `329735698` bayt, valid signed universal player ve Apple M1/Metal exact r38 readiness + Storage BuildKit smoke başarılıdır.
- Complete bundle'dan collision-free detached-clean Windows exact head üretildi. Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1332182927` bayt ve `issue75-hardened-v2` fatal-token `0` verdi; `ProjectSettings.asset` restoration byte-exact'tir. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact host/r38 readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti.
- Teknik branch private GitHub'a push edildi; source/docs `af6578aa224b931fdcfdd6293dccfcfd77a29eac`, tree `39ec1c0573223899d2982f72fb877dbea58306ba` ve [Repository Guard 32849988087](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32849988087) başarılıdır. ADR-0047 ve tarihli Evidence exact test/Mac/Windows/procedure hashlerini bağlar; canonical evidence exact `14/14`dır.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming→atomik final→ikinci readback zinciri aynı `966/966` payload, `951/951` exact Git source, `14/14` evidence, `19598907` bayt ve `958ba6bc…f9d2b` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar `0`dır.
- Fiziksel metadata `b113c86f5c2b375b0bc31081a5764fe264c2af9d`, tree `9b7e7a7689ceb6fc8955d4de7a2cbdaa713722bd` ve [Repository Guard 32851553662](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32851553662) başarılıdır. Acceptance `23/23`, Issue #75 `CLOSED`, Roadmap `Done`; PR #76 integration aracıdır ve parent Epic #10 açık/In Progress kalır.

### Önceki checkpoint — Issue #73 r37 teknik, fiziksel USB ve lifecycle kapıları tamamlandı / kapalı ve Done

- Technical head `a2df663d6fa0e9d2004697bfb038a65a5e6c3d81`, tree `e32a8e143049c4059e402bafbfcd39b9760cd025`. Canonical reserved DDR5 DIMM yalnız exact `ComponentKind == MemoryModule` ve tam work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür.
- DIMM, staged canonical motherboard ve processor prerequisites sonrasında ayrı stable operation ve memory-specific managed capacity-one BuildKit container üzerinden source → ActorHands → DIMM BuildKit custody'sine taşınır. Mevcut iki slot/receipt/replay/revision/staged state korunur.
- GarageGraybox r37 ayrı DIMM tray/support/snap anchor, 180° keyed preview ve görünür `2/10 → 3/10` aggregate ekler. Active memory BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; A2 dual-latch aynı-frame input'u çalamaz ve receipt'siz normal Assembly yolu değişmez.
- Final EditMode `680/680`, PlayMode `86/86`; failed/skipped/inconclusive `0`. Universal Mac report `329681642` bayt, valid signed universal player ve Apple M1/Metal exact r37 readiness + DIMM BuildKit smoke başarılıdır.
- Complete bundle'dan collision-free detached-clean Windows exact head üretildi. Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1330930513` bayt ve hardened-v2 fatal-token `0` verdi; `ProjectSettings.asset` restoration byte-exact'tir. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive player exact host/r37 readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti.
- Teknik branch private GitHub'a push edildi; source/docs `e45f6e1b463cbe9686a9c349d0c6912a9657a28e`, tree `16f014a807a7733210bc9197981b4a8608c3d687` ve [Repository Guard 32841321015](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32841321015) başarılıdır. ADR-0046 ve tarihli Evidence exact test/Mac/Windows/procedure hashlerini bağlar; canonical evidence exact `14/14`dır.
- Immutable local final ve doğru Windows-attached fiziksel USB incoming→atomik final→ikinci readback zinciri aynı `954/954` payload, `939/939` exact Git source, `14/14` evidence, `19379146` bayt ve `912e35ff…e9cc8` manifest sonucunu verdi. Incoming/AppleDouble/final-sidecar `0`dır.
- Fiziksel metadata `28df8283b7fa5187fa1a0dd6ec72acebd6d539d4`, tree `2b31cb1cb79eaca78c08feb6a6943c610cf3ee25` ve [Repository Guard 32842669488](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32842669488) başarılıdır. Acceptance `23/23`, Issue #73 `CLOSED`, Roadmap `Done`; PR #74 integration aracıdır ve parent Epic #10 açık/In Progress kalır.

### Önceki checkpoint — Issue #71 r36 teknik, fiziksel USB ve lifecycle kapıları tamamlandı

- Technical head `11683c8b567ad6edcd6777610875aeebd0e509ef`, tree `6890157f3f3625661314b34700259e0933ff2677`. Canonical reserved Processor exact work-order/ticket/allocation line/product/item/reservation tuple'ıyla çözülür; staged motherboard prerequisite'i korunur.
- CPU ayrı stable operation ve capacity-one managed processor BuildKit üzerinden source → ActorHands → CPU BuildKit custody'sine taşınır. Domain commit world mutation'dan önce gelir; same-instance/stable ItemId, live reservation/allocation, exact replay ve stable recovery korunur.
- GarageGraybox r36 ayrı CPU tray/support/snap anchor, 90° preview ve görünür `1/10 → 2/10` aggregate ekler. Active CPU BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; ProcessorSocket aynı-frame input'u çalamaz, receipt'siz normal socket assembly yolu değişmez.
- Final EditMode `677/677`, PlayMode `81/81`; failed/skipped/inconclusive `0`. Universal Mac report `329627927` bayt, valid signed universal player ve Apple M1/Metal exact r36 smoke başarılıdır.
- Collision-free exact detached-clean Windows `hardened-v2` x64 IL2CPP/only-D3D11 report `1329802474` bayt; expanded Burst/native-link fatal-token `0`, three-binary and three-procedure hash readback, byte-exact ProjectSettings restore, Intel Iris Xe interactive success `1`, runtime forbidden `0`, graceful shutdown, task deletion ve residue `0` başarılıdır. İlk recovered-import evidence provisional geçmiş olarak ayrılmıştır.
- Technical [Guard 32827174483](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32827174483) geçti. ADR-0045, tarihli Evidence ve `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue71-11683c8b567a/canonical-evidence` exact procedure-bound `14/14` kaynağıdır. Source/docs/provenance `7501fa74335ca977364033025eb51f4f4fc7bebf`, tree `0fcfd59000cc5cdca915d86d4854862c3879f435`, Guard `32833455406`; canonical local + Windows USB incoming/final readback `942/942` payload, `927/927` exact Git source, `14/14` evidence, `19.139.923` bayt ve `f38ae282…3cb8ed` manifest ile geçti. Exact-target/internal AppleDouble ve incoming residue `0`; acceptance `22/22`, Roadmap `Done`, PR #72 hazırdır. Önceki domain-only USB checkpoint'i yalnız immutable tarihsel ara kayıttır.

### Önceki checkpoint — Issue #68 r35 teknik, fiziksel USB ve lifecycle kapılarıyla tamamlandı

- Feature chain `2a69436` + `b0d2a97`; current technical head `480874191ee2c950e046ab2aee8be92d61d79fe4`, tree `e229788741df4c456840d356633e2a4bc1702516`. Canonical reserved motherboard exact work-order/ticket/allocation line/product/item/reservation identity ile çözülür.
- Stable build-kit operation ve immutable pickup/place receipts replay-safe'tir. Capacity-one managed BuildKit, Assembly Workbench/seat custody'sinden ayrıdır; narrow allocation bridge source → ActorHands → BuildKit hareketinde live reservation/allocation'ı korur ve generic transfer/world/stack/cart/Assembly bypass'larını fail-closed tutar.
- Domain-first projection aynı Unity component/stable ItemId'yi carry, 90° rotation, preview, placement ve recovery boyunca korur. Ticket `0/10 → 1/10`; diğer dokuz reservation/item, quote price ve Assembly revision/state/receipts untouched'dır.
- Real keyboard/mouse ve Input System gamepad pickup/place; range/focus/LOS/empty-hands/capacity/obstruction/pose/revision; same-frame Interact/Drop/Primary, hold, pause co-edge ve release-repress single-consumer matrisi testlidir.
- Exact detached-clean `4808741/e2297887` clone Unity 6000.3.21f1 full EditMode `675/675`, PlayMode `73/73`; failed/skipped/inconclusive `0`. Universal Mac report `329571495` bayt, signed Universal executable ve Apple M1/Metal exact r35 smoke başarılıdır.
- Exact clean Windows x64 IL2CPP/only-D3D11 report `1327308678` bayt; three-binary hash readback, byte-exact ProjectSettings restore, Intel Iris Xe/feature level 11.1 host/readiness/success/shutdown `1/1/1/1`, forbidden `0`, graceful cleanup ve residue `0` başarılıdır.
- Technical-source [Guard 32744068996](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32744068996) geçti. Exact source/docs `374094ceda9f8f65991e3906c62e1e4ba768b134`, tree `65418d089bc88c9f3dd435b93536c754fd4fef41` ve [Guard 32750065918](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32750065918) başarılıdır; [PR #69](https://github.com/cixanla/PC-Shop-Empire-3D/pull/69) accepted checkpoint'i merge etmiştir. ADR-0044, tarihli Evidence ve `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue68-4808741` task-cleanup receipt dahil procedure-bound canonical `14/14` kanıt kaynağıdır.
- Collision-free local immutable incoming/final ve doğru physical USB incoming/atomik final hedefleri dört tam readback'te aynı `929/929` payload, `914/914` exact Git source, `14/14` evidence, `18.882.211` bayt ve `6d59ddb9…112a9` manifest sonucunu verdi; AppleDouble/incoming residue `0`, USB güvenle eject edildi. Physical metadata `3e1de005bfb7662ca74a00809a14810f45286c12`, tree `0973c18e1b09f01043737935564d57d01dc84730` ve [Guard 32751777063](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32751777063) başarılıdır. Acceptance `20/20`, Issue #68 `CLOSED/COMPLETED`, Roadmap `Done`; PR #69 merge commit `f60464db00bfa7262648248aebb18bfc6558ccb1` ile birleşti. Parent Epic #10 açık/In Progress kalır.

### Önceki checkpoint — Issue #66 teknik, source-docs, yerel/fiziksel USB ve lifecycle kapılarıyla tamamlandı

- Core feature `f9545605baff423f05615e7326902e24dc82aeeb`; current technical head `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, tree `69ea366cc49e99b653f5d02d9c0f238b4906de69`. Stable BuildOrder/WorkTicket/Operation kimlikleri accepted exact quote/reservation setini tek immutable job ve workbench ticket'a bağlar.
- Inventory exact ten-item managed seti commit öncesi yeniden doğrular ve tek terminal allocation receipt'i exactly-once yayınlar. Reservations/items yerinde ve canlı kalır; move/delete/release/consume, second allocation, orphan recovery ve mismatched replay fail-closed'dur.
- GarageGraybox `garage-custom-pc-work-ticket-r34-v1`; canonical workbench physical ticket'ı job identity, `10/10` ve `MONTAJA HAZIR • HENÜZ BAŞLAMADI` gösterir. Range/focus/LOS/empty-hands/fresh Interact, pause/co-edge/competing-target ve gerçek keyboard/mouse/gamepad customer→workbench rotası testlidir. Assembly untouched kalır.
- EditMode `661/661`, PlayMode `66/66`; Universal Mac `329478891` bayt ve Apple M1/Metal exact r34 smoke başarılıdır. Same-frame ticket/carry/cart Interact ownership ve kapsamlı player/item/Inventory/Assembly no-teleport snapshot'ları bu current source üzerinde yeniden doğrulandı.
- Clean exact Windows head x64 IL2CPP + only-Direct3D11 buildi `1328828053` report baytı üretmiş, ProjectSettings restore/readback `byte-exact` geçmiştir. Interactive Intel Iris Xe/Direct3D 11.0 player host/r34/work-ticket markerlarını birer kez, forbidden markerını sıfır vermiştir.
- Technical-source [Guard 32721069982](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32721069982) başarılıdır. Exact source/docs `4e1ef4322d9ef049e3aac915c611474f6bee92fd`, tree `4df76fb1b50da53bdee7e65cb64acf0e73a5c018` ve [Guard 32723213686](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32723213686) başarılı; draft [PR #67](https://github.com/cixanla/PC-Shop-Empire-3D/pull/67) bu checkpoint'e bağlıdır. ADR-0043 ve tarihli Evidence exact receiptsi ve canonical `9/9` allowlist'i taşır; kaynak `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62`dir.
- Yerel final staging `2026-08-24_STAGE_B_IMMUTABLE_CUSTOM_PC_WORK_ORDER_PHYSICAL_WORK_TICKET_HANDOFF` incoming ve final adlarında `906/906` manifest, `896/896` exact Git source, `9/9` evidence, `17.330.935` bayt ve `1514481a…4121` manifest ile geçti. Doğru external physical USB `/Volumes/cixanla/CIXANLA`, backup kökü ve önceki Issue #62 zinciri doğrulandı; yalnız `.incoming-issue66-6752927` hedefinde ilk tam readback, aynı dosya sisteminde atomik final adlandırma ve ikinci tam readback aynı sonuçlarla geçti. Internal/sibling AppleDouble ve kalan incoming `0`dır. Fiziksel metadata `a80e325` ve Guard `32726202296` başarılı; acceptance `18/18`, Issue #66 `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır.

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

- Edit Mode `677/677` passed.
- Play Mode `81/81` passed.
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

Issue #66 core feature `f954560`, technical source `f8afd62`, tree `69ea366`, Guard `32721069982`, EditMode `661/661`, PlayMode `66/66`, Universal Mac `329478891` bayt ve Windows x64 IL2CPP report `1328828053` bayt ile Mac Apple M1/Metal ve Windows Intel Iris Xe/D3D11 exact r34 native smoke kapılarını geçti. Source/docs `4e1ef43`, tree `4df76fb`, Guard `32723213686`; local staging metadata `2dc67d2`, provenance/current pre-USB head `6752927`, Guard `32724718603` ve local immutable staging `906/906` payload, `896/896` Git source, `9/9` evidence, `17.330.935` bayt, `1514481a…4121` manifest ile doğrulandı. Aynı paket doğru fiziksel USB'de incoming ve atomik final adlarından iki kez tam okundu; sonuçlar aynı, AppleDouble/incoming `0`dır. Fiziksel metadata `a80e325`, Guard `32726202296`, acceptance `18/18`, Issue `CLOSED/COMPLETED` ve Roadmap `Done`dur; parent Epic #10 açık/In Progress kalır. Canonical evidence kaynağı `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62`dir. PR #67 bu completed checkpoint'in `main` integration aracıdır; sıradaki bounded iş canonical motherboard'ın gerçek pickup/carry/placement ile dedicated build-kit slotuna taşınması ve `0/10 → 1/10` fiziksel ilerlemesidir. Assembly completion, electrical power-on, tam benchmark, Save, Guardian ve final art ayrı kapılardır.

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
2. Repo guard, güncel 677 Edit Mode ve 81 Play Mode testi geçti.
3. Vizyon ile vertical slice sınırını kendi cümlesiyle açıklayabildi.
4. GitHub Project'te sıradaki issue/acceptance kriterini buldu.
5. Küçük bir docs/test PR'ını yaşayan belge kurallarına uygun açabildi.

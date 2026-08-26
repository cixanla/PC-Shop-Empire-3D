# PC Shop Empire 3D — Yaşayan Proje Bible ve Ana Handoff

**Belge rolü:** Projenin ana fikrini, güncel durumunu, teknik sınırlarını, yapılmış ve yapılacak işleri tek giriş noktasında tutar.  
**Son kapsam güncellemesi:** 26 Ağustos 2026
**Authoritative ayrıntılar:** [`Docs/ProjectBible/`](Docs/ProjectBible/) ve tarihli ADR'ler.  
**Güncelleme kuralı:** Her GitHub checkpoint/pull request, etkilediği durum ve sıradaki işi bu belgede güncellemek zorundadır.

## Güncel geliştirme checkpoint'i — Issue #95 r48 teknik ve exact-head Mac/Windows native tamamlandı; source-docs/local/USB/insan/idari kapanış sürüyor

Epic #10'un dördüncü BuildKit→Assembly kurulum dilimi technical head `42c1ae4dff2421b38879c0bfc82b4bf52522be1e`, tree `16304340da0ae7e42d8e7dd1ea6aef66ffe27efc` üzerindedir. Canonical reserved M.2 2280 NVMe yalnız exact owned work-order/ticket/allocation line/product/serialized-item/reservation/staging-receipt tuple'ıyla; historical `10/10` aggregate, live Issue #89 secured-motherboard, Issue #91 retained-CPU ve Issue #93 retained-A2-DDR5 receipt zinciri sonrasında çözülür. Oyuncu aynı Unity SSD instance'ını keyboard/mouse veya gamepad ile Storage BuildKit'ten exact ActorHands'e alır, existing primary M-key slota 18° guided girişle oturtur, flat seat'e bastırır, motherboard-owned captive screw'u sıkar, secured remove block'unu doğrular, gevşetir, detach→same-instance hands→reseat döngüsünü tamamlar. Live reservation/allocation ve immutable `10/10` staging history korunur; motherboard/CPU/DDR5 ve diğer altı item/container/receipt/revision untouched kalır.

Pickup atomikliği Issue #95'te açık biçimde iki güvenli aşamadır: physical projection önce reversibly stage edilir, fakat held-input state yayınlanmaz; exact authority commit sonrasında item elde görünür. Authority reddederse aynı instance exact BuildKit safe pose'una geri alınır. Physical-stage failure ve authority-rejection rollback testleri duplicate/ghost/loss olmadığını kanıtlar. Generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass'ları fail-closed'dur; existing Issue #57 M.2 topology/seat/press/captive-screw authority tek gerçektir.

Pause/focus-regain de continuous input için fail-safe sınırdır: held keyboard/gamepad Move ve gamepad Look, tüm resolved controls neutral olmadan resume sonrasında hareket/look üretemez. `W+S` / `A+D` gibi karşıt tuşlar aggregate olarak sıfır olsa bile latch'i erken çözmez; rebind ve çoklu cihazlar korunur, fresh mouse delta duyarlı kalır.

Full EditMode `722/722` ve full PlayMode `130/130` geçmiştir. Universal Mac report `330195891` bayt, Apple M1/Metal exact r48 smoke ve technical Repository Guard [32962078481](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32962078481) başarılıdır. Complete bundle `7626490` bayt / `f11c846b…3748` ile collision-free detached-clean Windows `issue95-42c1ae4-opposing-neutral-v3` x64 IL2CPP/only-D3D11 report `1343654204` bayt ve fatal-token `0` verdi; Intel Iris Xe Direct3D 11.0 level 11.1 exact r48 runtime host/readiness/success `1/1/1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve player/Unity/task residue `0` ile geçti. ADR-0057, tarihli Evidence ve `issue95` verifier contract'ı exact kanıtları bağlar; canonical teknik evidence `13/14`dür. Source/docs Guard, final receipt `14/14`, immutable local/sağlıklı fiziksel USB ve exact-r48 gerçek insan oturumu bekler; strict acceptance `26/27`, Issue #95/Roadmap ve draft PR #96 açık In Progress kalır. Windows D: volume `Warning / Full Repair Needed` olduğu için USB checkpoint yazımı yasaktır.

Ana ürün hedefi fiziksel 3D teknoloji mağazası/servis/montaj döngüsünü legacy Dashboard parity, küçükten büyüğe mağaza, personel/müşteri, işlevsel mahalle, kişisel ev, araç/kargo/lojistik, dünya NPC ekolojisi, mekânsal ses/zaman/hava ve güvenli offline Local Advisor/Guardian sınırlarıyla birleştirir. İçerik boyutu kalite ve ölçüme bağlıdır; retail oyun OpenAI/ChatGPT/Codex/internet/model indirme bağı taşımaz ve kontrolsüz kendi kendini kodlayan sistem içermez. Mac tek authoritative write lane, Windows exact detached-clean IL2CPP/D3D11 worker'dır.

## Önceki geliştirme checkpoint'i — Issue #93 r47 teknik ve exact-head Mac/Windows native tamamlandı; source-docs/local/USB/insan/idari kapanış sürüyor

Epic #10'un üçüncü BuildKit→Assembly kurulum dilimi technical head `0caca090d2859dfb78219abb089274fe599eaca2`, tree `e52c75872a8ec59a98b63c0c46d5e3f6f9c5e084` üzerindedir. Canonical DDR5 UDIMM yalnız exact owned work-order/ticket/allocation line/product/serialized-item/reservation/staging-receipt tuple'ıyla; historical ten-receipt `10/10` aggregate, live Issue #89 secured-motherboard ve Issue #91 retained-CPU receipt zinciri sonrasında çözülür. Oyuncu aynı Unity DIMM instance'ını keyboard/mouse veya gamepad ile BuildKit'ten exact ActorHands'e alır, existing keyed A2 MemorySlot'a oturtur, iki mandalı kapatıp açar, detach→same-instance hands→reseat döngüsünü tamamlar. Live reservation/allocation ve immutable `10/10` staging history korunur; secured motherboard, retained CPU ve diğer yedi item/container/receipt/revision untouched kalır. Generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass'ları fail-closed'dur; domain commit physical mutation'dan önce gelir.

Full EditMode `718/718` ve full PlayMode `125/125` geçmiştir. Universal Mac report `330173019` bayt, Apple M1/Metal exact r47 smoke ve technical Repository Guard `32946849858` başarılıdır. Complete bundle üzerinden collision-free detached-clean Windows `issue93-0caca09-hardened-v1` x64 IL2CPP/only-D3D11 report `1342974093` bayt ve fatal-token `0` verdi; Intel Iris Xe Direct3D 11.0 level 11.1 exact r47 runtime, graceful exit, task deletion ve player/Unity/task residue `0` ile geçti. ADR-0056, tarihli Evidence ve `issue93` verifier contract'ı mevcut exact Mac/Windows hashlerini bağlar; canonical teknik evidence `13/14`dür. Source/docs Guard, final receipt `14/14`, immutable local/sağlıklı fiziksel USB ve exact-r47 gerçek insan oturumu bekler; strict acceptance `25/26`, Issue #93/Roadmap ve draft PR #94 açık In Progress kalır. Windows D: volume Dirty/`Full Repair Needed` olduğu için USB checkpoint yazımı yasaktır.

Ana ürün hedefi fiziksel 3D teknoloji mağazası/servis/montaj döngüsünü legacy Dashboard parity, küçükten büyüğe mağaza, personel/müşteri, işlevsel mahalle, kişisel ev, araç/kargo/lojistik, dünya NPC ekolojisi, mekânsal ses/zaman/hava ve güvenli offline Local Advisor/Guardian sınırlarıyla birleştirir. İçerik boyutu kalite ve ölçüme bağlıdır; retail oyun OpenAI/ChatGPT/Codex/internet/model indirme bağı taşımaz ve kontrolsüz kendi kendini kodlayan sistem içermez. Mac tek authoritative write lane, Windows exact detached-clean IL2CPP/D3D11 worker'dır.

## Önceki geliştirme checkpoint'i — Issue #91 r46 teknik/source-CI ve exact-head Mac/Windows native tamamlandı; source-docs/local/USB/insan/idari kapanış sürüyor

Epic #10'un ikinci BuildKit→Assembly kurulum dilimi technical head `003c93f2de191ff3b295a8a88454e74617521970`, tree `1e46049a9a253559b2f9f4ab41524e8be5e0f9ab` üzerindedir. Canonical CPU yalnız exact owned work-order/ticket/allocation line/product/serialized-item/reservation/staging-receipt tuple'ıyla; historical ten-receipt `10/10` aggregate, live Issue #89 motherboard handoff, exact Workbench custody, `SeatedSecured` ve attach/secure receipts sonrasında çözülür. Oyuncu aynı Unity CPU instance'ını keyboard/mouse veya gamepad ile BuildKit'ten exact ActorHands'e alır, existing keyed ProcessorSocket'e oturtur, retention'ı kapatıp açar, detach→same-instance hands→reseat döngüsünü tamamlar. Live reservation/allocation ve immutable `10/10` staging history korunur; secured motherboard ve diğer sekiz item/container/receipt/revision untouched kalır. Generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass'ları fail-closed'dur; domain commit physical mutation'dan önce gelir.

Full EditMode `715/715` ve full PlayMode `122/122` geçmiştir. Universal Mac report `330127900` bayt, Apple M1/Metal exact r46 smoke ve technical Repository Guard `32937325469` başarılıdır. Detached-clean Windows `issue91-hardened-v2` x64 IL2CPP report `1342422475` bayt; Intel Iris Xe Direct3D 11.0 level 11.1 exact r46 runtime, graceful exit, task deletion, player/Unity/build-task residue `0` ve exact-head clean checkout ile geçti. ADR-0055 ve tarihli Evidence mevcut exact hashleri bağlar; canonical teknik evidence `13/14`dür. Source/docs Guard, final source receipt `14/14`, immutable local/sağlıklı fiziksel USB ve exact-r46 gerçek insan oturumu bekler; strict acceptance `24/25`, Issue #91/Roadmap ve draft PR #92 açık In Progress kalır.

Ana ürün hedefi fiziksel 3D teknoloji mağazası/servis/montaj döngüsünü legacy Dashboard parity, küçükten büyüğe mağaza, personel/müşteri, işlevsel mahalle, kişisel ev, araç/kargo/lojistik, dünya NPC ekolojisi, mekânsal ses/zaman/hava ve güvenli offline Local Advisor/Guardian sınırlarıyla birleştirir. İçerik boyutu kalite ve ölçüme bağlıdır; retail oyun OpenAI/ChatGPT/Codex/internet/model indirme bağı taşımaz ve kontrolsüz kendi kendini kodlayan sistem içermez. Mac tek authoritative write lane, Windows exact detached-clean IL2CPP/D3D11 worker'dır.

## Önceki geliştirme checkpoint'i — Issue #89 r45 teknik/source-CI ve exact-head Mac/Windows native tamamlandı; source/docs/local/USB/insan/idari kapanış sürüyor

Epic #10'un ilk BuildKit→Assembly kurulum dilimi technical head `2fdf371206bc58c32e1c20d471f4abe7c0bfba01`, tree `c5e6de5942993a98735984caca4a04fd396105f6` üzerindedir. Accepted work-order/ticket/allocation içindeki canonical motherboard yalnız exact line/product/serialized-item/reservation tuple'ıyla ve authoritative historical ten-receipt `10/10` aggregate ile çözülür. Oyuncu aynı Unity motherboard instance'ını keyboard/mouse veya gamepad ile BuildKit'ten exact ActorHands'e alır, existing guided seat üzerinden açık kasadaki exact Assembly Workbench'e oturtur, canonical fastener ile secure eder, sonra unsecure→detach→same-instance hands→reseat döngüsünü tamamlar. Live reservation/allocation ve immutable `10/10` staging history korunur; diğer dokuz item/container/receipt/revision untouched kalır. Generic transfer/drop/box/stack/cart ve receipt-free Assembly bypass'ları fail-closed'dur; domain commit physical mutation'dan önce gelir.

Full EditMode `712/712` ve full PlayMode `119/119` geçmiştir. Universal Mac report `330104684` bayt, Apple M1/Metal exact r45 smoke ve technical Repository Guard `32930403290`; detached-clean Windows Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1340592635` bayt, Intel Iris Xe feature level 11.1 exact runtime, graceful exit, deleted task ve player/Unity/task residue `0` başarılıdır. ADR-0054 ve tarihli Evidence exact hashleri bağlar; canonical teknik evidence `13/14`dür. Source/docs Guard, final source receipt `14/14`, immutable local/sağlıklı fiziksel USB ve exact-r45 gerçek insan oturumu bekler; strict acceptance `22/25`, Issue #89/Roadmap ve draft PR #90 açık In Progress kalır.

Ana ürün hedefi fiziksel 3D teknoloji mağazası/servis/montaj döngüsünü legacy Dashboard parity, küçükten büyüğe mağaza, personel/müşteri, işlevsel mahalle, kişisel ev, araç/kargo/lojistik, dünya NPC ekolojisi, mekânsal ses/zaman/hava ve güvenli offline Local Advisor/Guardian sınırlarıyla birleştirir. İçerik boyutu kalite ve ölçüme bağlıdır; retail oyun OpenAI/ChatGPT/Codex/internet/model indirme bağı taşımaz ve kontrolsüz kendi kendini kodlayan sistem içermez. Mac tek authoritative write lane, Windows exact detached-clean IL2CPP/D3D11 worker'dır.

## Önceki geliştirme checkpoint'i — Issue #81 r41 teknik/native, immutable local/physical-USB ve metadata CI tamamlandı; exact-build insan oturumu/idari kapanış sürüyor

Epic #10'un yedinci fiziksel BuildKit component dilimi technical head `f3d80629e09c05afde97fa778c4b220ca456c5f0`, tree `851954879c1ff1e2ef98bc9a7a8469750304d992` ile [PR #82](https://github.com/cixanla/PC-Shop-Empire-3D/pull/82)'ye ulaştı. Accepted work-order/ticket/allocation içindeki canonical power supply yalnız exact PowerSupply role ve tam line/product/item/reservation tuple'ıyla çözülür. Staged motherboard, CPU, DDR5, M.2, processor cooler ve graphics card kendi slotlarında kalırken oyuncu aynı Unity PSU instance'ını gerçek keyboard/mouse veya gamepad pickup, carry, keyed `0° ↔ 180°` preview ve placement akışıyla ayrı capacity-one Power Supply BuildKit'e taşır. Domain commit world mutation'dan önce gelir; generic transfer/drop/box/stack/cart, Issue #60 PSU-bay Assembly ve Issue #61–#63 cable-route bypass'ları fail-closed'dur. PSU BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; receipt'siz normal Assembly/cable yolları değişmez. Work ticket authoritative aggregate `6/10 → 7/10` olurken reservation/allocation canlı, önceki altı BuildKit state'i ve bütün PSU-bay/cable-route/electrical/quote/diğer-item state'leri untouched kalır. Native prerequisite harness production Update order'ını atlayan same-frame input/station kısayolundan neutral → pressed → released gerçek player frame lifecycle'ına geçirilmiştir. Exact final EditMode `697/697`, PlayMode `105/105`; Universal Mac report `329907140` bayt ve Apple M1/Metal r41 exact smoke başarılıdır. Collision-free detached-clean Windows exact head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1335888266` bayt ve `issue81-hardened-v1` fatal-token `0` verdi; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve player/Unity/task residue `0` ile geçti. Source/docs `dc118bf0d26a11f3937cb114ef12f85666facc48`, tree `ac9fcb5d38855ed37f2ee36449100b5094287cb8` ve [Guard 32896033674](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32896033674) başarılı; canonical evidence exact `14/14`dır. Immutable local final ve doğru Windows-attached physical USB incoming/final readback'leri aynı `1002/1002` payload, `987/987` exact Git source, `14/14` evidence, `19368668` bayt ve `69cc892b…06ab` manifest sonucunu verdi; incoming/AppleDouble/final-sidecar ve Windows işlem artığı `0`dır. Fiziksel metadata `ff935452c68bc77e66eb0742e0c3e6c0eb2894c7`, tree `e4f03fc3c2d6dfd44da61eaae3a161af4f104eae` ve [Guard 32897672990](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32897672990) başarılıdır. Exact r41 gerçek insan oturumu kaydedilmediği için acceptance `23/24`, Issue #81 açık/In Progress ve PR #82 taslak kalır; geçince Roadmap `Done` idari kapanışı yapılır. Parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #79 r40 teknik, fiziksel USB ve lifecycle kapıları tamamlandı

Epic #10'un altıncı fiziksel BuildKit component dilimi technical head `f40ef21058caf1a2aca3054218abfc1dd7305c01`, tree `c7500e7300f75f5d9b089bf23657750dccc5ffed` ile [PR #80](https://github.com/cixanla/PC-Shop-Empire-3D/pull/80)'e ulaştı. Accepted work-order/ticket/allocation içindeki canonical graphics card yalnız exact GraphicsCard role ve tam line/product/item/reservation tuple'ıyla çözülür. Staged motherboard, CPU, DDR5, M.2 ve processor cooler kendi slotlarında kalırken oyuncu aynı Unity GPU instance'ını gerçek keyboard/mouse veya gamepad pickup, carry, keyed 180° half-turn preview ve placement akışıyla ayrı capacity-one Graphics Card BuildKit'e taşır. Domain commit world mutation'dan önce gelir; generic transfer/drop/box/stack/cart, Issue #59 GPU seat/retention ve Issue #63 PCIe route bypass'ları fail-closed'dur. GPU BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; receipt'siz normal Assembly yolları değişmez. Work ticket authoritative aggregate `5/10 → 6/10` olurken reservation/allocation canlı, önceki beş BuildKit state'i ve bütün GPU-seat/PCIe-route/electrical/quote/diğer-item state'leri untouched kalır. Exact final EditMode `690/690`, PlayMode `100/100`; Universal Mac report `329839788` bayt ve Apple M1/Metal r40 exact smoke başarılıdır. Collision-free detached-clean Windows exact head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1334256694` bayt ve `issue79-hardened-v3` fatal-token `0` verdi; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti. Source/docs `dd607d0af346bd1f0e28449f606761bc97e1495c`, tree `010b3a460c3241ed69d315bfb44047c1be82cb10` ve [Guard 32874685021](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32874685021) başarılı; canonical evidence exact `14/14`dır. Immutable local final ve doğru Windows-attached physical USB incoming/final readback'leri aynı `990/990` payload, `975/975` exact Git source, `14/14` evidence, `20086932` bayt ve `d2d399fa…b324` manifest sonucunu verdi; incoming/AppleDouble/final-sidecar `0`dır. Fiziksel metadata `880523fcb71208796cce96564556a2170363c92a`, tree `448052665c3b64b1c565d460de6c648c498b698d` ve [Guard 32876194890](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32876194890) başarılıdır. Acceptance `24/24`, Issue #79 `CLOSED`, Roadmap `Done`; PR #80 integration aracıdır, Issue #77 ve parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #77 r39 teknik ve Mac/Windows native kapıları tamamlandı

Epic #10'un beşinci fiziksel BuildKit component dilimi technical head `197233688c4fe587097dbfc1cbee843cfc78603e`, tree `58458f400a7efaa68e452a0e85e35d6d7eb5a3ab` ile private branch'e push edildi. Accepted work-order/ticket/allocation içindeki canonical processor cooler yalnız exact ProcessorCooler role ve tam line/product/item/reservation tuple'ıyla çözülür. Staged motherboard, CPU, DDR5 ve M.2 kendi slotlarında kalırken oyuncu aynı Unity cooler instance'ını gerçek keyboard/mouse veya gamepad pickup, carry, keyed 90° quarter-turn preview ve placement akışıyla ayrı capacity-one Processor Cooler BuildKit'e taşır. Domain commit world mutation'dan önce gelir; generic transfer/drop/box/stack/cart ve Issue #58 cooler-seat/four-point-retention/TIM Assembly bypass'ları fail-closed'dur. Cooler BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; normal receipt'siz cooler Assembly davranışı değişmez. Work ticket authoritative aggregate `4/10 → 5/10` olurken reservation/allocation canlı, önceki dört BuildKit state'i ve bütün cooler/Assembly/TIM/electrical/quote/diğer-item state'leri untouched kalır. Exact final EditMode `686/686`, PlayMode `96/96`; Universal Mac report `329787583` bayt ve Apple M1/Metal r39 exact smoke başarılıdır. Collision-free detached-clean Windows exact head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1333221634` bayt ve fatal-token `0` verdi; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti. ADR-0048 ve tarihli Evidence, diğer 13 canonical artifact'ı ve `issue77` verifier contract'ını bağlar. Source/docs commit/Repository Guard, final receipt, immutable local package, doğru fiziksel USB incoming/final çift readback, final metadata/Guard ve acceptance `24/24` henüz bekliyor; Issue #77 açık/Roadmap `In Progress` ve parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #75 r38 teknik, fiziksel USB ve lifecycle kapıları tamamlandı

Epic #10'un dördüncü fiziksel BuildKit component dilimi technical head `646e66cfa269a217ecb1f6942f9accb77f9e463c`, tree `ee9b0b2c0bb5e1fb07de397da222d00a7480b23c` ile [PR #76](https://github.com/cixanla/PC-Shop-Empire-3D/pull/76)'ya ulaştı. Accepted work-order/ticket/allocation içindeki canonical M.2 NVMe yalnız exact StorageDevice role ve tam line/product/item/reservation tuple'ıyla çözülür. Staged motherboard, CPU ve DDR5 kendi slotlarında kalırken oyuncu aynı Unity NVMe instance'ını gerçek keyboard/mouse veya gamepad pickup, carry, 180° keyed preview ve placement akışıyla ayrı capacity-one Storage BuildKit'e taşır. Domain commit world mutation'dan önce gelir; generic transfer/drop/box/stack/cart ve M.2 guided-insertion/captive-screw Assembly bypass'ları fail-closed'dur. Storage BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; normal receipt'siz M.2 Assembly davranışı değişmez. Work ticket authoritative aggregate `3/10 → 4/10` olurken reservation/allocation canlı, önceki üç BuildKit state'i ve bütün M.2/Assembly/electrical/quote/diğer-item state'leri untouched kalır. Exact final EditMode `683/683`, PlayMode `90/90`; Universal Mac report `329735698` bayt ve Apple M1/Metal r38 exact smoke başarılıdır. Collision-free detached-clean Windows exact head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1332182927` bayt ve fatal-token `0` verdi; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti. Source/docs `af6578aa224b931fdcfdd6293dccfcfd77a29eac`, tree `39ec1c0573223899d2982f72fb877dbea58306ba` ve [Guard 32849988087](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32849988087) başarılı; canonical evidence exact `14/14`dır. Immutable local final ve doğru Windows-attached physical USB incoming/final readback'leri aynı `966/966` payload, `951/951` exact Git source, `14/14` evidence, `19598907` bayt ve `958ba6bc…f9d2b` manifest sonucunu verdi; incoming/AppleDouble/final-sidecar `0`dır. Fiziksel metadata `b113c86f5c2b375b0bc31081a5764fe264c2af9d`, tree `9b7e7a7689ceb6fc8955d4de7a2cbdaa713722bd` ve [Guard 32851553662](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32851553662) başarılıdır. Acceptance `23/23`, Issue #75 `CLOSED`, Roadmap `Done`; PR #76 integration aracıdır ve parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #73 r37 teknik, fiziksel USB ve lifecycle kapıları tamamlandı

Epic #10'un üçüncü fiziksel BuildKit component dilimi technical head `a2df663d6fa0e9d2004697bfb038a65a5e6c3d81`, tree `e32a8e143049c4059e402bafbfcd39b9760cd025` ile [PR #74](https://github.com/cixanla/PC-Shop-Empire-3D/pull/74)'e ulaştı. Accepted work-order/ticket/allocation içindeki canonical DDR5 DIMM yalnız exact MemoryModule role ve tam line/product/item/reservation tuple'ıyla çözülür. Staged motherboard ve CPU kendi slotlarında kalırken oyuncu aynı Unity DIMM instance'ını gerçek keyboard/mouse veya gamepad pickup, carry, 180° keyed preview ve placement akışıyla ayrı capacity-one memory BuildKit'e taşır. Domain commit world mutation'dan önce gelir; generic transfer/drop/box/stack/cart ve A2/dual-latch Assembly bypass'ları fail-closed'dur. Memory BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; normal receipt'siz A2 Assembly davranışı değişmez. Work ticket authoritative aggregate `2/10 → 3/10` olurken reservation/allocation canlı, motherboard/CPU BuildKit state'i ve bütün A2/Assembly/electrical/quote/diğer-item state'leri untouched kalır. Exact final EditMode `680/680`, PlayMode `86/86`; Universal Mac report `329681642` bayt ve Apple M1/Metal r37 exact smoke başarılıdır. Collision-free detached-clean Windows exact head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 report `1330930513` bayt ve fatal-token `0` verdi; Intel Iris Xe/Direct3D 11.0 feature level 11.1 runtime exact host/readiness/success `1`, forbidden `0`, exit `0`, graceful shutdown, deleted task ve residue `0` ile geçti. Source/docs `e45f6e1b463cbe9686a9c349d0c6912a9657a28e`, tree `16f014a807a7733210bc9197981b4a8608c3d687` ve [Guard 32841321015](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32841321015) başarılı; canonical evidence exact `14/14`dır. Immutable local final ve doğru Windows-attached physical USB incoming/final readback'leri aynı `954/954` payload, `939/939` exact Git source, `14/14` evidence, `19379146` bayt ve `912e35ff…e9cc8` manifest sonucunu verdi; incoming/AppleDouble/final-sidecar `0`dır. Fiziksel metadata `28df8283b7fa5187fa1a0dd6ec72acebd6d539d4`, tree `2b31cb1cb79eaca78c08feb6a6943c610cf3ee25` ve [Guard 32842669488](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32842669488) başarılıdır. Acceptance `23/23`, Issue #73 `CLOSED`, Roadmap `Done`; PR #74 integration aracıdır ve parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #71 r36 teknik, fiziksel USB ve lifecycle kapıları tamamlandı

Epic #10'un ikinci fiziksel BuildKit component dilimi technical head `11683c8b567ad6edcd6777610875aeebd0e509ef`, tree `6890157f3f3625661314b34700259e0933ff2677` ile [PR #72](https://github.com/cixanla/PC-Shop-Empire-3D/pull/72)'ye ulaştı. Accepted work-order/ticket/allocation içindeki canonical CPU yalnız exact Processor role ve tam line/product/item/reservation tuple'ıyla çözülür. Staged motherboard kendi slotunda kalırken oyuncu aynı Unity CPU instance'ını gerçek keyboard/mouse veya gamepad pickup, carry, 90° preview ve placement akışıyla ayrı capacity-one CPU BuildKit'e taşır. Domain commit world mutation'dan önce gelir; generic transfer/drop/stack/cart ve ProcessorSocket/Assembly bypass'ları fail-closed'dur. CPU BuildKit receipt'i primary/rotate/interact/drop input'unun tek sahibidir; normal receipt'siz ProcessorSocket davranışı değişmez. Work ticket authoritative aggregate `1/10 → 2/10` olurken reservation/allocation canlı, motherboard BuildKit state'i ve bütün Assembly/electrical/quote/diğer-item state'leri untouched kalır. Exact final EditMode `677/677`, PlayMode `81/81`; Universal Mac report `329627927` bayt ve Apple M1/Metal r36 exact smoke; collision-free detached-clean Windows `hardened-v2` x64 IL2CPP/Direct3D11 report `1329802474` bayt, geniş Burst/native-link fatal-token `0` ve Intel Iris Xe interactive exact smoke başarılıdır. İlk recovered-import evidence provisional geçmiş olarak izole edilmiştir. Technical [Guard 32827174483](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32827174483) geçti. ADR-0045 ve tarihli Evidence exact procedure-bound `14/14` kanıtı `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue71-11683c8b567a/canonical-evidence` kaynağına bağlar. Source/docs/provenance head `7501fa74335ca977364033025eb51f4f4fc7bebf`, tree `0fcfd59000cc5cdca915d86d4854862c3879f435` ve [Guard 32833455406](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32833455406) başarılıdır. Canonical local ve doğru Windows USB milestone'u incoming tam readback → aynı-filesystem atomik final → ikinci tam readback zincirinde aynı `942/942` payload, `927/927` exact Git source, `14/14` evidence, `19.139.923` bayt ve `f38ae282a13e5cb070c633386f4118811e2554d61ba84875b407e208dd3cb8ed` manifest sonucunu verdi; exact-target/internal AppleDouble ve incoming residue `0`dır. Acceptance `22/22`, Roadmap `Done`; PR #72 hazır integration aracıdır ve parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #68 r35 teknik, fiziksel USB ve lifecycle kapılarıyla tamamlandı

Epic #10'un sıradaki fiziksel dilimi technical head `480874191ee2c950e046ab2aee8be92d61d79fe4`, tree `e229788741df4c456840d356633e2a4bc1702516` ile [PR #69](https://github.com/cixanla/PC-Shop-Empire-3D/pull/69)'a ulaştı. Exact work-order/ticket/allocation `LineId/ProductId/ItemId/ReservationId` tuple'ıyla seçilen canonical reserved motherboard, domain-first source → ActorHands → capacity-one dedicated BuildKit custody zincirinde taşınır. Stable operation/receipt replay-safe'tir; live reservation/allocation korunur, generic transfer/world drop/stack/cart/Assembly bypass'ları fail-closed kalır. Aynı Unity component ve stable ItemId pickup, carry, 90° rotation, preview, placement ve recovery boyunca korunur; world projection yalnız domain commit'ten sonra değişir. Work ticket `0/10 → 1/10` ilerlerken diğer dokuz item/reservation, quote price ve bütün Assembly state/receipt'leri untouched kalır. Exact detached-clean `4808741/e2297887` clone üzerinde EditMode `675/675`, PlayMode `73/73`; Universal Mac `329571495` report baytı ve Apple M1/Metal r35 exact smoke; Windows x64 IL2CPP/Direct3D11 `1327308678` report baytı ve Intel Iris Xe/feature level 11.1 interactive exact smoke başarılıdır. Technical-source [Guard 32744068996](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32744068996) geçti. Exact source/docs `374094ceda9f8f65991e3906c62e1e4ba768b134`, tree `65418d089bc88c9f3dd435b93536c754fd4fef41` ve [Guard 32750065918](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32750065918) başarılıdır. ADR-0044 ve tarihli Evidence, task-cleanup receipt dahil procedure-bound canonical `14/14` kanıtı `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue68-4808741` kaynağına bağlar. Collision-free yerel immutable package ile doğru fiziksel USB'nin incoming ve atomik final hedefleri dört tam doğrulamada aynı `929/929` payload, `914/914` exact Git source, `14/14` evidence, `18.882.211` bayt ve `6d59ddb9…112a9` manifest sonucunu verdi; AppleDouble ve incoming residue `0` bulundu, USB final readback sonrasında güvenle eject edildi. Fiziksel metadata `3e1de005bfb7662ca74a00809a14810f45286c12`, tree `0973c18e1b09f01043737935564d57d01dc84730` ve [Guard 32751777063](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32751777063) başarılıdır. Acceptance `20/20`; Issue #68 `CLOSED/COMPLETED`, Roadmap `Done` ve PR #69 merge commit `f60464db00bfa7262648248aebb18bfc6558ccb1` ile birleşmiştir. Parent Epic #10 açık/In Progress kalır.

## Önceki geliştirme checkpoint'i — Issue #66 r34 teknik, fiziksel USB ve lifecycle kapılarıyla tamamlandı

Epic #10'un accepted quote/reservation → immutable build order → physical work ticket dilimi core feature `f9545605baff423f05615e7326902e24dc82aeeb`, technical source `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, tree `69ea366cc49e99b653f5d02d9c0f238b4906de69` ile [PR #67](https://github.com/cixanla/PC-Shop-Empire-3D/pull/67)'ye ulaştı. Exact customer/request/quote/claim/workbench ve on line/item/reservation identity tek immutable BuildOrder + WorkTicket içinde donar; Inventory aynı managed set için bir terminal operation-keyed allocation receipt'i exactly-once yayınlar. On reservation ve serialized item canlı, yerinde ve değişmeden kalır; move/delete/release/consume, second allocation, orphan recovery ve mismatched replay fail-closed'dur. GarageGraybox r34 canonical workbench'te job identity, `10/10` ve `MONTAJA HAZIR • HENÜZ BAŞLAMADI` gösteren collider-safe physical ticket taşır. Range/focus/LOS/empty-hands/fresh Interact, pause/co-edge/competing-target ve gerçek keyboard/mouse/gamepad customer→workbench rotası testlidir; physical ticket/carry/cart same-frame Interact ownership'i deterministic, customer-reserved shelf stock istisnası bounded ve Quote/Assembly authority'leri izoledir. EditMode `661/661`, PlayMode `66/66`; Universal Mac `329478891` bayt ve Apple M1/Metal exact r34 smoke, clean Windows x64 IL2CPP `1328828053` report baytı ve Intel Iris Xe/Direct3D 11.0 feature level 11.1 exact r34 smoke başarılıdır. Technical-source [Guard 32721069982](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32721069982), exact source/docs `4e1ef4322d9ef049e3aac915c611474f6bee92fd` / tree `4df76fb1b50da53bdee7e65cb64acf0e73a5c018` için [Guard 32723213686](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32723213686) ve current pre-USB head `6752927` için [Guard 32724718603](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32724718603) başarılıdır. Yerel immutable milestone incoming ve final adlarında `906/906` manifest, `896/896` exact Git source, `9/9` evidence, `17.330.935` bayt ve `1514481a…4121` manifest ile geçti. External physical USB `/Volumes/cixanla/CIXANLA`, `90_BACKUPS/PCShopEmpire3D` ve önceki Issue #62 milestone zinciri salt-okunur doğrulandı; collision-free `.incoming-issue66-6752927` ilk tam readback'i geçtikten sonra atomik final adına taşındı ve final dizin aynı tam readback'i ikinci kez geçti. İki fiziksel okuma da `906/906`, `896/896`, `9/9`, `17.330.935` bayt ve `1514481a…4121` verdi; internal/sibling AppleDouble ile kalan incoming `0`dır. Fiziksel metadata `a80e325`, [Guard 32726202296](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32726202296) ve acceptance `18/18` başarılıdır; [Issue #66](https://github.com/cixanla/PC-Shop-Empire-3D/issues/66) `CLOSED/COMPLETED`, Roadmap `Done`, parent Epic #10 açık/In Progress durumundadır. Fiziksel component transfer/build-kit completion, montaj, power-on/POST/OS, benchmark/QA, paketleme/teslim, final settlement, Save/Guardian ve Steam sonraki bağımlı kapılardır.

## 1. Kuzey yıldızı

PC Shop Empire 3D, oyuncunun küçük bir garajda başlayıp fiziksel olarak büyüyen bir teknoloji perakende ve servis işletmesi kurduğu birinci şahıs 3D simülasyondur.

Oyunun ayırt edici birleşimi:

> **Fiziksel teknoloji perakendesi + teknik PC ustalığı + uzun vadeli müşteri güveni.**

Oyuncu yalnız menülerde sayı yönetmez. Siparişi terminalden verir; teslimatı fiziksel olarak kabul eder, kutuyu taşır, ürünü depoya veya rafa yerleştirir, müşteriye danışmanlık yapar, kasayı işletir ve özel PC'yi tezgâhta parça parça toplar. Dashboard yönetim katmanıdır; fiziksel işi sihirli biçimde tamamlamaz.

## 2. Ürün hedefi ve platform sırası

| Alan | Kilit karar |
|---|---|
| Ana platform | Windows x64 ve Steam zorunlu |
| Geliştirme bilgisayarı | Apple Silicon MacBook günlük geliştirme/prototip için uygun |
| Windows doğrulaması | İlk oynanabilirden önce gerçek Windows x64 PC; vertical slice sonrası düzenli native test |
| macOS | Windows/Steam 1.0 tamamlandıktan ve bütçe uygun olduktan sonra ayrı port, imzalama, notarization ve QA |
| Linux | Zorunlu değil; düşük maliyetle mümkünse daha sonra değerlendirilebilir |
| Kamera | Birinci şahıs; görünür eller ve fiziksel iş animasyonları |
| İş modeli | Öncelik premium tek oyunculu Steam oyunu; manipülatif monetization yok |
| Motor | Unity 6000.3.21f1 + URP başlangıç tabanı; alpha öncesi kontrollü LTS yükseltme kapısı |
| Sanat yönü | Okunaklı yarı gerçekçilik: gerçek oran/PBR malzeme/zemine oturan ışık/doğal ağırlık, hafif stilize okunabilirlik; fotogerçekçilik veya gerçek marka/asset kopyası yok ([ADR-0013](Docs/ADR-0013-READABLE-SEMI-REALISTIC-VISUAL-DIRECTION.md)) |
| Araç bütçesi | Ücretsiz araçlar varsayılan; yalnız büyük ve ölçülebilir etki sağlayan düşük maliyetli araç ayrı onay kapısı |

## 3. Oyuncu fantezisi

Oyuncu üç rolü aynı işletmede birleştirir:

1. **Mağaza sahibi:** nakit, kira, fiyat, stok, tedarikçi, itibar ve büyümeyi yönetir.
2. **Saha operatörü:** teslimat, kutu, raf, kasa, temizlik, güvenlik ve mağaza düzeniyle fiziksel olarak ilgilenir.
3. **PC uzmanı:** ihtiyacı dinler, uyumlu sistem tasarlar, parça parça toplar, test eder, paketler, teslim eder ve satış sonrasını yürütür.

Uzun vadeli bağ yalnız daha büyük sayılar değildir. Oyuncu garajdan mahalle dükkânına, gelişmiş teknoloji mağazasına ve çok bölümlü profesyonel işletmeye geçerken operasyon, müşteri beklentisi, risk ve uzmanlık da değişir.

## 4. Ana oyun döngüleri

### Günlük mağaza döngüsü

`Planla → sipariş ver → teslimatı kabul et → taşı/depolandır → rafla/fiyatlandır → müşteriye hizmet et → kasa/teslim → kapanış ve analiz`

Her aşama aynı authoritative stok ve para gerçeğini kullanır. Ürün hem ekonomide satılmış hem rafta fiziksel olarak var olamaz.

### Özel PC işi

`İhtiyaç görüşmesi → bütçe/öncelik → teklif → parça rezervasyonu → fiziksel montaj → kablo/soğutma → kurgusal OS → test/benchmark → kalite kapısı → paketleme → teslim/satış sonrası`

Uyumsuz, eksik veya kalitesiz montaj; yeniden iş, gecikme, masraf, müşteri memnuniyetsizliği veya arıza riski doğurur. Sistem gerçekçilik uğruna yorucu vida grind'ına dönüşmez; ustalıkla güvenli hızlandırmalar açılır.

### Servis ve yenileme döngüsü

`Cihaz kabulü → görünür kondisyon/veri izni → belirti → teşhis hipotezi → müşteri onayı → onarım/parça → test → temizlik → teslim/garanti`

İkinci el alım-satım, refurbish ve parça geri kazanımı aynı seri/kondisyon/maliyet kayıtlarına bağlanır.

### Büyüme döngüsü

`Kârlı ve güvenilir işletme → yeni kapasite → daha karmaşık müşteri/ürün → çalışan ve uzmanlık → yeni hizmet → bölge/şube`

Büyüme yalnız alan açmaz; kira, stok riski, hizmet standardı, rekabet, çalışan koordinasyonu ve tedarik ilişkisi de zorlaşır.

## 5. Dünya ve fiziksel oynanış

### Oyuncu

- Birinci şahıs hareket, hassasiyet/FOV ve yeniden atanabilir kontroller.
- Görünür eller; alma, bırakma, inceleme, kutu açma ve araç kullanma.
- Küçük nesnede elde taşıma; büyük kutuda görüş/hız kısıtı; ağır işte taşıma arabası.
- Serbest rigidbody kaosu yerine hassas görevlerde yönlendirilmiş snap ve doğrulanmış etkileşim.
- Erişilebilir hedef büyütme, hold/toggle seçenekleri, renk dışı işaretler ve hareket rahatsızlığı ayarları.

### Mağaza

- Raf, vitrin, kasa, depo, atölye, ofis, servis kabulü, teslimat alanı, güvenlik ve dekorasyon.
- Yerleştirme; serbest önizleme + grid/snap + erişim/nav doğrulaması.
- Raf planogramı bir zorunlu puzzle değil; okunabilirlik, kapasite ve müşteri bulma süresini etkileyen yönetim aracıdır.
- Fizik projeksiyonu bozulsa bile ekonomik ürün silinmez; son güvenli konum/karantina mekanizması kullanılır.

## 6. Ürün ve PC sistemi

İlk ürün aileleri; CPU, GPU, anakart, RAM, SSD/HDD, PSU, kasa, soğutucu, fan, monitör, klavye, mouse, kulaklık, webcam, oyun kolu, kablo, termal macun ve hazır sistemleri kapsar.

Her ürün tanımı şunları ayırır:

- Kurgusal marka, model, nesil ve kategori.
- Teknik özellik/uyumluluk.
- Performans, kalite, garanti ve arıza profili.
- Alış maliyeti, piyasa/talep ve önerilen fiyat sinyali.
- Fiziksel boyut, kutu/raf davranışı ve görsel varlık bağlantısı.

Her fiziksel ürün örneği ayrıca instance/batch kimliği, maliyet, tedarikçi, kondisyon, garanti, rezervasyon, konum ve test/hasar geçmişi taşır.

Uyumluluk tek bir yeşil/kırmızı sonuç değildir. Soket, chipset/BIOS, RAM nesli/kapasitesi, form factor, PSU güç/connector/headroom, GPU/soğutucu fiziksel açıklığı, depolama bağlantısı, termal yük ve müşteri gereksinimi ayrı neden kodlarıyla değerlendirilir.

## 7. Müşteriler ve çalışanlar

### Müşteri modeli

Müşteriler; bütçe, amaç, teknik bilgi, sabır, zaman baskısı, kalite/garanti hassasiyeti, marka eğilimi, pazarlık, sadakat ve geçmiş deneyim bakımından ayrılır.

Davranış zinciri:

`Giriş → yön bulma → göz atma → ürün/yardım → değerlendirme → kasa/teklif → çıkış → yorum/iade/takip`

AI sonsuza kadar aynı hedefe yürüyemez; her durumda timeout, yeniden çözümleme ve güvenli fallback vardır. Müşteri kararı anlaşılır nedenlere dayanır; gizli hileyle stok veya para üretilmez.

### Çalışan rolleri

- Satış danışmanı
- Kasa görevlisi
- PC teknisyeni
- Depo/raf çalışanı
- Temizlik görevlisi
- Yönetici
- Güvenlik görevlisi

Hız, uzmanlık, hata riski, maaş, eğitim, memnuniyet, güvenilirlik ve uzmanlık alanı farklıdır. Oyuncu her hareketi tek tek söylemek yerine görev ve politika verir; kritik kalite kapıları yetkin kişi onayı ister.

## 8. Ekonomi ve işletme

- Tek authoritative stok, rezervasyon ve transaction gerçeği.
- Nakit, gelir, COGS, kira, maaş, vergi karşılığı, fatura, kredi ve vadeli ödeme ayrımı.
- Talep, yeni ürün çıkışı, değer kaybı, tedarik kıtlığı, kampanya ve müşteri trendi.
- Fiyat değişimi işlem ortasında satış sonucunu değiştiremez.
- Tedarikçi; fiyat, minimum sipariş, kalite, teslim süresi, vade, hasar/eksik risk ve ilişki bakımından ayrılır.
- Şoklar bounded ve önceden sinyallidir; save reload ederek reroll yapılamaz.
- İflas ani ekran değildir: uyarı → nakit baskısı → kısıtlı seçenek → yeniden yapılandırma → kontrollü başarısızlık. Oyuncu isterse daha düşük zorlukla veya yeni şirket koşuluyla yeniden başlayabilir.

Gelir çeşitleri mağaza satışı dışında özel PC, servis, ikinci el/refurbish, online sipariş, kurumsal/okul/ofis anlaşması, e-spor sistemi ve ileride uygun kiralama modellerini içerir.

## 9. Dashboard

Dashboard fiziksel ofis bilgisayarı/tablet/terminal üzerinden açılır. Varsayılan olarak zamanı durdurabilir; isteyen oyuncu canlı simülasyonu seçebilir.

Ana modüller:

- Özet/KPI ve operasyon uyarıları
- Parça pazarı ve tedarikçi siparişi
- Stok ve seri/kondisyon takibi
- Fiyatlandırma ve kampanya
- Finans/muhasebe ve rapor
- Personel, vardiya, maaş ve görev
- Müşteri/özel PC/servis siparişleri
- Mağaza yükseltmeleri ve yerleşim planı
- Reklam, itibar ve müşteri yorumları
- Anlaşmalar ve tedarikçi ilişkileri
- Borç, vergi, kira ve faturalar
- Pazar trendleri ve rakip sinyalleri
- Garanti, iade, RMA ve servis
- Kariyer/hedef/başarım geçmişi

Dashboard sipariş verir ama kutuyu rafa ışınlamaz; PC işi kabul eder ama montaj/test/teslimi otomatik üretmez.

## 10. PSE Guardian sınırı

PSE Guardian, yayınlanan oyunun içinde oyuncuya kapalı çalışan bir tanı ve bütünlük katmanıdır; ChatGPT/OpenAI bağımlılığı değildir.

Yapabilecekleri:

- Olay zinciri ve invariant ihlali kaydetmek.
- Duplicate event, stok–dünya uyuşmazlığı, negatif para/quantity, takılmış görev ve bozuk save sinyali bulmak.
- Yalnız önceden tanımlı güvenli toparlamaları uygulamak.
- Offline ayrıntılı yerel rapor üretmek; açık opt-in ile online olduğunda pseudonymous rapor göndermek.

Yapamayacakları:

- Kendi kendine kaynak kodu değiştirmek.
- Codex/insan onayı olmadan patch indirmek veya oyun kuralı yazmak.
- Oyuncuya gizli avantaj/dezavantaj sağlamak.
- Para, ürün veya karar sonucu uydurmak.
- Kullanıcı dosyalarını ya da kişisel veriyi izinsiz toplamak.

Hard/native crash aynı proses içinden her zaman yakalanamaz; breadcrumb ve sonraki açılışta unclean-shutdown tespiti kullanılır. Crash SDK/online telemetry ayrı lisans, gizlilik ve onay kapısıdır.

## 11. Teknik mimari

Alan mantığı Unity nesnelerinden ayrıdır. Unity; input, fizik, animasyon, ses ve sunum adaptörüdür. Para, stok, uyumluluk ve iş kuralları saf C# modüllerinde test edilir.

| Modül | Sorumluluk |
|---|---|
| `PSE.Core` | Stable ID, sonuç/failure, deterministik zaman, sürümlü PRNG, event sözleşmeleri, temel invariant |
| `PSE.Catalog` | Ürün tanımı, teknik özellik, kalite, garanti |
| `PSE.Inventory` | Instance/batch, konteyner, konum, rezervasyon, kondisyon |
| `PSE.Orders` | Satın alma, satış, özel PC, servis ve kurumsal iş emirleri |
| `PSE.Economy` | Ledger, nakit, COGS, borç ve ödeme takvimi |
| `PSE.Retail` | Fiyat, sepet, checkout, kampanya, iade/garanti |
| `PSE.Assembly` | Build graph, uyumluluk, montaj, kalite ve benchmark |
| `PSE.Service` | Intake, teşhis, onarım, RMA ve refurbish |
| `PSE.Actors` | Müşteri/çalışan profili, ihtiyaç ve görev durumu |
| `PSE.World` | 3D etkileşim, placement, station ve nav rezervasyonu |
| `PSE.Dashboard` | Salt-okunur view model ve yetkili komutlar |
| `PSE.Save` | Sürümlü snapshot, journal, migration ve recovery |
| `PSE.Guardian` | Gözlem, invariant, anomali ve güvenli toparlama |
| `PSE.Presentation` | Unity sahne, prefab, animasyon, VFX, ses ve UI |
| `PSE.Platform` | Dosya sistemi, Steam, cloud ve izinli telemetry adaptörleri |

Bağımlılık yönü sunumdan alana doğrudur; `PSE.Core` Unity/Editor referansı taşımaz. Dairesel bağımlılık ve Dashboard'un sahne nesnesini doğrudan authoritative state olarak düzenlemesi kabul edilmez.

## 12. Determinizm, kayıt ve güvenlik

- Oyun zamanı integer ve açık fixed-step clock üzerinden ilerler; pause sırasında ilerlemez.
- Eventler stable ID/type, one-based sequence, schema, simulation timestamp ve zorunlu correlation/direct-causation bağlamı taşır; in-memory dispatcher global FIFO, breadth-first nested enqueue, duplicate/conflict ve handler hata izolasyonu uygular.
- Temel PRNG `pcg32-xsh-rr-64-32-v1` kimliğiyle sürümlüdür; raw state+odd increment snapshot/restore ve bias üretmeyen bounded integer davranışı testlidir.
- Root RNG seed save-safe canonical hex taşır; sürümlü SHA-256 framed domain/context türetmesi çağrı sırasından bağımsız PCG32 akışı üretir. Eksik veya bilinmeyen save metadata'sı sessiz fallback yapmaz; reload-reroll çekirdek testleriyle engellenir.
- Save; sürümlü snapshot, sınırlı journal, checksum, katalog fingerprint ve döner sağlam kopyalar kullanır.
- Yazma geçici dosya → flush/doğrulama → atomik replace yaklaşımıyla yapılır; gerçek platform fault-injection testi olmadan “kayıp olmaz” iddiası kurulmaz.
- Steam Cloud çatışması kullanıcıdan habersiz son-yazan-kazan yapmaz.
- Secret, token, sertifika, kişisel telemetry ve build cache Git'e girmez.

## 13. Kapsam ve tekrar önleme

Vertical slice'ın kilit çekirdeği:

- Tek garaj ve teslimat önü.
- Birinci şahıs hareket, görünür eller ve hibrit taşıma.
- Sipariş → fiziksel teslimat → depo/raf → fiyat → müşteri → kasa.
- Baştan sona tek özel PC işi.
- Temel Dashboard, ekonomi, save/recovery ve Guardian olay zinciri.
- Yaklaşık 50–80 anlamlı SKU; teknik prototipte daha az.

Vertical slice'a çalışan ordusu, şube ağı, yüzlerce ürün, geniş servis, tam online satış ve final sanat yığılmaz. Önce zincirin doğruluğu ve eğlencesi kanıtlanır.

Monotonluğu azaltma ilkeleri:

- Ustalıkla güvenli otomasyon; ayrıntılı moda geri dönüş.
- Toplu ama açıklanabilir görev/politika atama.
- Aynı fiziksel işi amaçsız tekrar ettirmeyen ergonomik etkileşim.
- Kriz ve trendlerin önceden sinyalli olması.
- İçerik sayısının değil yeni karar üretmesinin ölçülmesi.
- Zorlayıcı fakat adım adım, kurtarılabilir finansal başarısızlık.

## 14. Yol haritası ve güncel durum

| Faz | Hedef | Durum |
|---:|---|---|
| 0 | Keşif, ortak anlayış, kaynak güvenliği | Tamamlandı |
| A | Unity/paket/build/VCS teknik kurulum | Tamamlandı; private GitHub authoritative, UVCS beklemede |
| 1 | Proje temeli ve graybox etkileşim | Devam ediyor; hareket, küçük kutu pickup/drop/placement/rotation/istif, güvenli büyük-kutu taşıma, yüklü platform arabası ve ilk görsel benchmark tamam |
| 2 | Temel mağaza döngüsü | Tamamlandı; Catalog/Inventory, purchase-order receiving, fiziksel teslimat/raf, offer, basket, deterministic müşteri ziyareti + runtime NavMesh, consultation-gated stale-safe `Buy/Leave` ve `AwaitingCheckout`-gated fiziksel kasa üzerinden exact-cash `PSE.Economy` settlement kaynak/test/build/runtime/CI/USB kapılarıyla kapandı; Epic #9 Done |
| 3 | PC toplama teknik prototipi | Devam ediyor; anakart/fastener, CPU, DIMM, M.2, cooler, GPU, PSU, ATX24, EPS12V ve PCIe/GPU 6+2 fiziksel authority akışları ile accepted request→immutable quote/BOM→10 exact reservation→immutable build order/physical work ticket sınırı uygulandı. BuildKit component staging r44'te canonical PCIe/GPU cable ile `10/10` oldu ve exact technical source Guard + Mac/Windows test/build/native kapılarından geçti; component installation/retention ve electrical readiness henüz ayrı bağımlı işlerdir. Issue #87 source/docs/Guard/USB/insan/idari, Issue #77/#81/#83/#85 ayrı insan/lifecycle ve Issue #63–#64 tarihsel fiziksel lifecycle kapıları ayrı izleniyor |
| 4 | Vertical slice entegrasyonu | Planlandı |
| 5 | Çalışanlar ve gelişmiş müşteri AI | Planlandı |
| 6 | Servis, iade, garanti, ikinci el | Planlandı |
| 7 | Dinamik ekonomi ve tedarik | Planlandı |
| 8 | İtibar, büyüme, reklam, rekabet | Planlandı |
| 9 | İçerik, sanat, ses ve kariyer | Planlandı |
| 10 | Alpha, denge, optimizasyon, erişilebilirlik | Planlandı |
| 11 | Demo/Steam Playtest | Planlandı |
| 12 | Beta ve Windows/Steam 1.0 | Planlandı |
| 13 | Ayrı macOS portu ve QA | Windows 1.0 + bütçe sonrası |

Ayrıntılı bağımlılık, zorluk, risk ve kabul ölçütleri: [`Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md`](Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md).

## 15. Bugüne kadar tamamlanan teknik işler

| Checkpoint | Kanıt |
|---|---|
| Legacy keşif | Electron + düz JS/HTML/CSS; 14 Dashboard alanı haritalandı |
| Canonical legacy | USB ile yerel ayna 26/26 yol/boyut/SHA-256 eşleşti |
| Unity Stage A | Unity 6000.3.21f1 URP, paket kilidi, macOS Universal smoke ve Windows x64 Mono cross-build |
| VCS | Private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D), `main` ve Stage A etiketi; UVCS ikinci authoritative sistem değil |
| İş birliği/devir | Yaşayan Bible, governance, issue/PR şablonları, repo guard, 22 epic ve [private Project](https://github.com/users/cixanla/projects/2) |
| Legacy repository referansı | 26 canonical dosya private repoda byte-exact snapshot + SHA-256 manifest olarak korunuyor |
| Core assembly | `PSE.Core` `noEngineReferences`; Unity/Editor bağımlılık testi |
| Kimlik/sonuç | `StableId<TScope>`, `Failure.Code`, `OperationResult` |
| Zaman/olay | Integer açık-adımlı `SimulationClock`, pause güvenliği, event ID/type/sequence/schema zarfı |
| Rastgelelik | Sürümlü PCG32, 63-bit benzersiz stream selector, snapshot/restore, official golden vector ve bias'sız bounded integer |
| Bağlamsal stream | Canonical root seed, SHA-256 framed domain/context derivation, iki golden vector ve reload-reroll engeli |
| Event dispatch | Correlation/causation, global FIFO, breadth-first nested enqueue, duplicate/conflict, bounded drain ve handler hata izolasyonu |
| Catalog çekirdeği | Unity bağımsız `PSE.Catalog`; stable ürün/kategori kimliği, serialized/batch tracking policy, doğrulanmış görünür ad, bounded garanti ve immutable sıralı katalog |
| Inventory authority | Unity bağımsız `PSE.Inventory`; serialized item, bölünebilir batch position, unit-capacity container, atomik transfer, claim reservation, consume/release, revision ve invariant audit |
| Purchase order receiving | Unity bağımsız `PSE.Orders`; stable order/supplier/delivery, monotonik lifecycle, exact manifest, immutable unit-cost provenance ve tek-revision Inventory intake |
| Authoritative dünya/stok projection'ı | Görünür teslimat kabulü; aynı serialized item için Receiving→ActorHands→Shelf/WorldFloor domain-first transfer, rollback ve recovery |
| Fiziksel teslimat kolisi açma | Kapalı dış parcel → idempotent exact ürün reveal; opening domain revision/quantity değiştirmez, açık kabuk Receiving'de kalır |
| Authoritative RAF A teklifi | Unity bağımsız `PSE.Retail`; stable offer/product/shelf kimliği, 3 harf currency, pozitif integer minor-unit fiyat, idempotent publish/update revision ve failure no-mutation |
| Customer basket rezervasyonu | Stable customer/basket/line, exact offer + serialized item + Inventory claim; duplicate engeli, idempotent reserve, release, cross-authority no-mutation ve reserved pickup kilidi |
| Immutable checkout başlangıcı | Stable checkout/basket/customer ve deterministic line snapshot; exact offer/item/reservation preflight, integer minor-unit currency/total, idempotent begin, fiyat güncellemesine karşı immutable kayıt ve aktif checkout release/pickup kilidi |
| Atomik checkout fulfillment | Owner/revision-bound Inventory/Basket/Checkout prepared planı; side-effect-free tam preflight, tek Inventory/Basket/Checkout revision, public completion bypass'ı kapalı, exact repeat idempotency ve drift no-mutation |
| Atomik nakit ve ilk Economy settlement | Downstream Unity bağımsız `PSE.Economy`; immutable checkout fiyatı + alış maliyeti, exact cash, stable receipt, dengeli Cash/SalesRevenue/COGS/InventoryAsset postingleri, replay/conflict/no-mutation ve receipt-gated müşteri çıkışı |
| Deterministic müşteri ziyareti | Unity bağımsız `PSE.Actors`; stable customer/intent/visit, monotonik state + bounded receipt ledger, iki denemeli route fallback, patience/exit timeout ve Inventory/Retail/Orders izolasyonu |
| Bounded tek-müşteri danışmanlık/öneri kapısı | Unity bağımsız `CustomerConsultationAuthority`; current canonical `Browsing` ziyareti için tek immutable customer/visit/intent/need/product/timestamp provenance'ı, exact replay idempotency ve foreign/stale/non-browsing/conflict yollarında no-mutation |
| Runtime NavMesh müşteri projection'ı | Offer sonrası giriş→RAF A ve görünür yardım bekleme; odaklı `E / Gamepad South` danışmanlığı sonrası karar, Buy reservation sonrası checkout, Economy receipt sonrası çıkış; pause güvenli simulation clock, görünür durum/neden ve güvenli terminal gizleme |
| Fiziksel checkout station | Stable `world.checkout-station.garage-001`; `2,75 m` range, `24°` focus, LOS ve pause gate'i; RAF A ödeme bypass'ı kapalı; ilk `Mouse Left / Gamepad RT` immutable checkout, release/repress sonrası ikinci edge exact-cash settlement; canonical receipt-gated stock/customer completion |
| İlk authoritative PC assembly dilimi | Unity bağımsız `PSE.Assembly`; mevcut Catalog/Inventory ile tek serialized `MicroAtx` anakartın ActorHands↔managed Workbench transferi, immutable attach/detach receipt'i, stable identity/replay ve GarageGraybox'ta range/focus/LOS/support/obstruction gated `SeatedUnsecured` fiziksel slot akışı |
| Deterministic motherboard fastener | Assembly-owned stable fastener ID, exact secure/unsecure receipt ve historical replay; secured detach kilidi, Inventory-isolated revision, NonAlloc range/focus/LOS/pause/obstruction solver, gerçek keyboard/gamepad input ownership'i ve görünür screw/screwdriver/status-plate projection'ı |
| Deterministic CPU socket ve retention | Tek canonical serialized CPU için capacity-1 managed socket; `EmptyOpen → ProcessorSeatedOpen → ProcessorRetained` reversible authority, keyed 90° orientation, secured-host close gate'i, exact four-operation replay/lineage, aynı fiziksel instance recovery'si, gerçek keyboard/gamepad input ve r24 yarı-gerçekçi LGA package/load-plate/lever projection'ı |
| Deterministic single DIMM ve dual-latch retention | Tek canonical serialized DDR5 UDIMM, immutable A2/Channel A/Bank 2 topology, atomik managed triple claim; `EmptyOpen → MemoryModuleSeatedOpen → MemoryModuleRetained`, yalnız 0°↔180° keyed input, sol→sağ close/sağ→sol open, exact four-operation replay/lineage, installed-DIMM host gate, same-instance recovery, gerçek keyboard/gamepad ve r25 dört materyalli DIMM/slot/dual-latch projection'ı |
| Deterministic M.2 NVMe ve captive-screw retention | Tek canonical serialized M.2 2280 NVMe, atomik dört-container claim, immutable M-key/2280/captive-screw topology; 18° guided insertion, reversible seat/secure/unsecure/remove, exact replay, installed-storage host gate, same-instance recovery, gerçek keyboard/gamepad ve r26 SSD/slot projection'ı |
| Deterministic processor cooler ve four-point retention | Tek canonical serialized LGA1700 top-down air cooler, atomik beş-container claim, immutable slot/bracket/dört-point topology; iki keyed orientation, pre-applied TIM'in tek tüketimi, `1→3→2→4` retain/ters release, host gates, same-instance recovery, gerçek keyboard/gamepad ve r27 cold-plate/TIM/fin/fan projection'ı |
| Deterministic PCIe x16 ekran kartı ve rear-bracket retention | Canonical Northstar A60 ProductId'sini kullanan ayrı serialized assembly item, atomik altı-container claim, immutable PCIe x16 slot/latch/rear-bracket/fastener topology; keyed 0°/180° orientation, chassis+cooler clearance, reversible seat/retain/unretain/remove, installed-GPU host gate, same-instance recovery, gerçek keyboard/gamepad ve r28 dual-fan/PCB/contact/bracket projection'ı |
| Deterministic ATX PS/2 güç kaynağı ve four-screw retention | Tek canonical serialized PSU item, atomik yedi-container claim, immutable chassis-owned bay/rear-mount/four-fastener topology; iki keyed fan orientation, filtered-floor support, gerçek authored chassis clearance, reversible seat/retain/unretain/remove, alternate-order authority isolation, same-instance recovery, gerçek keyboard/gamepad ve r29 housing/fan-grille/AC/modular-panel/rear-plate projection'ı |
| Deterministic ATX24 split-PSU kablo routing | Tek canonical serialized power cable, typed PSU 18-pin + PSU 10-pin + motherboard 24-pin endpoints, üç ordered waypoint ve capacity-one CableRoute; atomik sekiz-container claim, `Loose ↔ Routed`, exact route/unroute replay, retained-PSU + secured-motherboard host gates, aynı fiziksel instance, gerçek keyboard/gamepad ve r30 connector/latch/authored-route projection'ı |
| Deterministic EPS12V/CPU güç kablosu routing | Tek canonical serialized EPS12V cable, iki typed/keyed 8-pin endpoint, üç ordered waypoint ve capacity-one `CpuPowerCableRoute`; atomik dokuz-container claim, retained PSU + secured motherboard + retained CPU lineage, ATX24 isolation, reversible same-instance route/recovery, gerçek keyboard/gamepad ve r31 connector/latch/braided-route projection'ı |
| Deterministic PCIe/GPU 8-pin 6+2 güç kablosu routing | Tek canonical serialized PCIe/GPU cable, iki typed/keyed endpoint, üç ordered waypoint ve capacity-one `GpuPowerCableRoute`; atomik on-container claim, retained PSU + secured motherboard + retained GPU lineage, ATX24/EPS12V isolation, reversible same-instance route/world-drop/recovery, gerçek keyboard/gamepad ve r32 6+2 connector/latch/braided-route projection'ı |
| Açıklanabilir tek-offer müşteri kararı | Tek yönlü `PSE.Retail → PSE.Actors`; owned current consultation + immutable visit/offer/accepted-price provenance, deterministic `Buy/Leave`, stable reason/failure code, exact replay ve bütün gameplay authority'lerinde no-mutation |
| Stale-safe müşteri Buy eylemi | Explicit Actors↔Retail kimlik bağı, current visit/offer yeniden doğrulaması, exact serialized action-owned reservation, `Browsing → NavigatingToCheckout`, idempotent replay ve stale no-mutation |
| Stale-safe müşteri Leave eylemi | Aynı kind-discriminated action ledger'ında current visit/offer revalidation, internal Actors prepared planı, `Browsing → Exiting`, stable `OfferDeclined`, Browse→Exit NavMesh ve bütün commerce authority'lerinde no-mutation |
| Oynanabilir garaj | `PSE.World`/`PSE.Presentation`, GarageGraybox, connected PlayerRig, görünür prototip eller, klavye/fare + gamepad hareket/kamera, sprint, pause ve rebind store |
| Fiziksel pickup/drop | Stable ürün kimliği, range+LOS hedefleme, tek slot, fizik snapshot/restore, dinamik prompt, güvenli drop ve recovery |
| Kontrollü küçük kutu placement | İşaretli stock surface, 0,25 m grid/90° yaw snap, tam destek/overlap doğrulaması, yeşil-kırmızı ghost + metin, stabil kinematic placement |
| Büyük kutu taşıma profili | Ayrı boyut/kimlik, iki-el pozu, 0,65× hareket, sprint kilidi, motion-safe bounded FOV, fail-closed drop ve recovery |
| Kontrollü küçük kutu rotation | `R / Right Shoulder` ile deterministik 90° adım, etkin binding/açı promptu, döndürülmüş footprint doğrulaması ve ghost/confirm poz eşitliği |
| Kontrollü küçük kutu istifleme | Stable küçük kutu desteği, merkez/90° snap, beş noktalı tam destek, overlap engeli, tek kat ilişkisi, dolu taban kilidi ve gerçek keyboard/gamepad akışı |
| Yüklü taşıma arabası | Tek `LargeBox` kapasitesi, hands→cart→hands stable ownership, dört noktalı destek + swept obstruction, 0,85× yüklü hız, sprint kilidi, dinamik prompt ve fail-closed recovery |
| Görsel yön sözleşmesi | Gerçek oran, PBR yüzey, zemine oturan ışık ve doğal ağırlık taşıyan okunaklı yarı gerçekçilik; ilk uygulama tek benchmark köşesiyle sınırlı |
| Garaj görsel benchmarkı | Bevel'lı tezgâh/raf, prosedürel PBR yüzeyler, görev ışığı, ACES/bloom/reflection probe; gameplay collider ve kimlik sözleşmeleri korunuyor |
| Son tamamlanmış USB milestone (Issue #62) | `2026-08-23_STAGE_B_DETERMINISTIC_SINGLE_EPS12V_CPU_POWER_CABLE_ROUTING`; source/docs `cff75f8`, USB metadata `2db7cf9`, iki tam 832/832 SHA-256 hash/boyut/yol readback, 826/826 exact Git source, 5/5 evidence, 15.757.786 payload baytı ve `afa89feb…6a73` manifest; bütün fark/AppleDouble sayaçları `0` |
| Issue #50 kapanış checkpoint'i | Feature `547cf971882239c912d8221f344706afc993a37b`, source/docs `aea6e2bd01642f4f72f1a9ee70f07e3dd0e5072b`, tree `84b14646fd549ce93e390bc33a626a8a7a6335fb`; [Repository Guard 31884807638](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31884807638) başarılı; acceptance `18/18`, Issue kapalı ve Roadmap `Done` |
| Issue #51 kapanış checkpoint'i | Feature `846eb5d9912150a6ef3aae9a37678d71348f92a3`, source/docs `f9bc38d8861f575909e36a331ab1cc6476a237a5`, tree `cb087b2a36a5030485c5835ababfcb8f6555ac98`; [Repository Guard 31888842125](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31888842125) başarılı; acceptance `16/16`, Issue kapalı ve Roadmap `Done` |
| Issue #52 kapanış checkpoint'i | Feature `92a0f7b814ad5e597d8d4ca033f2e533f618f719`, source/docs `d6cd203c5b9837c8eecc63ee3974dd2e76351bdc`, tree `6d73d5ac6d675733c939f181d087da3aef90f496`; [Repository Guard 31892875650](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31892875650) başarılı; acceptance `17/17`, Issue kapalı/Roadmap `Done`; parent Epic #9 kapalı/Done |
| Issue #53 kapanış checkpoint'i | Feature `582a3cf3e81a2905e39148065bd5f6c7e35bbc06`, source/docs `8c6abe45d1f9b6c72def9b686b9c81bf3704d10d`, tree `387bcba701b8a959681e92bf29dc48a4d09f0ab7`; [Repository Guard 31905540378](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31905540378) başarılı; acceptance `18/18`, Issue kapalı/Roadmap `Done`; birleşik USB milestone doğrulandı |
| Issue #54 kapanış checkpoint'i | Feature `b6812394f835d64d5bf8422d8e7996ec433cd0f1`, source/docs `7cec7cc4b6fd80997acd0dc2d6943ef08850f4ad`, tree `214381bd6c9d06a7ab2b2c5ea5e902437dca5914`; [Repository Guard 31909940414](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31909940414) başarılı; acceptance `18/18`, Issue kapalı/Roadmap `Done`; birleşik USB milestone doğrulandı |
| Issue #55 kapanış checkpoint'i | Feature `99cadad414789d3f440e08cc6e42e727c2b7a2ad`, source/docs `d9d0722a1592a83b89938529f72b3170f17e94eb`, final metadata `07364b79ad111aa778493c8936a7709c84b48464`; [Repository Guard 31914774370](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914774370) ve [31914933915](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31914933915) başarılı; acceptance `20/20`, Issue kapalı/Roadmap `Done`; birleşik USB milestone doğrulandı |
| Issue #56 kapanış checkpoint'i | Feature `7482fc9aabe6a3a27ba41730db12c60e18aac515`, source/docs `01c2b5a49f11b27b52af9e299d4d2e48cef3c962`, USB metadata `17af550856e8bca288ed5c17924bc82586c76c27`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055), [31920258176](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920258176) ve [31920923402](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31920923402) başarılı; ayrı USB milestone 668/668 doğrulandı; acceptance `21/21`, Issue kapalı/Roadmap `Done` |
| Issue #57 kapanış checkpoint'i | Feature `4f14e7bcb946f2f8e713a70d16d0e8f04216dbe1`, source/docs `6e0627ec7a76a70abdba8bb507e6ef6979e34236`; [Repository Guard 31970813717](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31970813717) başarılı; ayrı USB milestone 689/689 doğrulandı; acceptance `21/21`, Issue kapalı/Roadmap `Done` |
| Issue #58 kapanış checkpoint'i | Feature `e2f10a22c37101cb12c5d6530c8f104deb72e99d`, source/docs `2e848e3bdc5795a349e6c857973c7c88fef36cd7`, tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`; [Guard 32591206866](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591206866) ve [32591381804](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32591381804) başarılı; acceptance `19/19`, Issue/Roadmap `Done`; fiziksel USB ertelendi, 717/717 doğrulanmış yerel staging hazır |
| Issue #59 kapanış checkpoint'i | Feature `1b29ad29d6e4d8cc1ce09b0989a038a72297585c`, source/docs `a5bbca473e81455c44d2f95469c8faf2a11046ff`, tree `e7b3d5d4ad13bf08f822570966660ab6c48e6a55`; [feature Guard 32599710154](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32599710154) ve [source/docs Guard 32600012769](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32600012769) başarılı; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; fiziksel USB ertelendi ve Issue #59 için milestone/readback iddiası yok |
| Issue #60 kapanış checkpoint'i | Feature `f998d7d1c400c9328afa226f0727e6591c02d4e2`, authored-clearance fix `b6c3ff8e95a4da75161b51cdbcbab87cb529f076`, source/docs `4939a041635a8864f53f6613a9dc9b4e8972f235`; [Guard 32606958882](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32606958882), [32607437408](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607437408) ve [32607886160](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32607886160) başarılı; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; 775/775 local staging hazır, fiziksel USB readback iddiası yok |
| Issue #61 kapanış checkpoint'i | Feature `1fc29f13171925c2445eaa7334158e0f058e76a5`; source/docs `52795b66fee1eb933d0d9c4ff8cbd7eca512d718`; USB metadata `f9a5da8b23dedd3719c96d50846d4ba3143cc87f`; Guard `32613813494` + `32614187494` + `32632615041` başarılı; ayrı USB milestone 801/801 ve 796/796 doğrulandı; acceptance `20/20`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent #10 açık/In Progress |
| Issue #62 kapanış checkpoint'i | Feature `15d83aeba0d71238a31bf7b7db5fab3dbd9b5951`, source/docs `cff75f8876f893888ca3a98fe5f149dab0f74a1b`, USB metadata `2db7cf984974fd561873d3c06c815b7f47f41d07`; Guard `32642211422` + `32642638437` + `32672086464` başarılı; yerel staging ve fiziksel USB üzerinde iki tam 832/832 readback, 826/826 exact Git source ve 5/5 evidence geçti; acceptance `21/21`, Issue `CLOSED/COMPLETED`, Roadmap `Done`; parent #10 açık/In Progress |
| Issue #63 teknik checkpoint'i | Feature `ea1e51f862d4094936c03bccf9fbfaee7bb7d12b`, explicit GPU-side 6+2 fix `d655f1a5aab0c882cf40702472ec1b8ad44747ad`, source/docs `d597941a20afd0491547513abbc68e0b9d890aab`; Guard `32676069923` + `32676154473` + `32677267023` + `32677495639` başarılı. Mac r32 ve clean/exact Windows x64 IL2CPP/Intel Iris Xe/Direct3D 11 exact r32 smoke geçti; fiziksel USB/final metadata bekliyor, Issue/Roadmap açık/In Progress |
| Issue #64 teknik checkpoint'i | Feature `c7d38845ffccb5ae6e5365e580c238d70f8dac95`, tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`; accepted graphics-first request, immutable on-satırlı quote/BOM ve tek managed operation/revision altında atomik 10 exact-item reservation seti. Guard `32698054990`, draft PR #65; Mac+Windows full test/build/native r33 geçti. Fiziksel USB/final metadata bekliyor, Issue/Roadmap açık/In Progress |
| Issue #66 kapanış checkpoint'i | Core feature `f9545605baff423f05615e7326902e24dc82aeeb`, technical source `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, source/docs `4e1ef4322d9ef049e3aac915c611474f6bee92fd`, physical metadata `a80e325`; Guard `32721069982` + `32723213686` + `32724718603` + `32726202296`. Mac+Windows full test/build/native r34 ve yerel/fiziksel USB çift okuma `906/906`, `896/896`, `9/9`, `17.330.935` bayt, `1514481a…4121` manifest ile geçti; AppleDouble/incoming `0`, acceptance `18/18`, Issue `CLOSED/COMPLETED`, Roadmap `Done`, parent #10 açık/In Progress |
| Issue #71 kapanış checkpoint'i | Technical source `11683c8b567ad6edcd6777610875aeebd0e509ef`, tree `6890157f3f3625661314b34700259e0933ff2677`, Guard `32827174483`; canonical CPU gerçek pickup/carry/rotation/placement ile ayrı capacity-one BuildKit'e taşındı, work-ticket `1/10 → 2/10` ilerledi, reservation/allocation canlı ve Assembly/ProcessorSocket untouched kaldı. Mac r36 ve collision-free Windows `hardened-v2` r36 native kapıları geçti. Source/docs/provenance `7501fa7`, Guard `32833455406`; immutable local + fiziksel USB çift readback `942/927/14`, `19.139.923` bayt ve `f38ae282…3cb8ed` manifest ile geçti; acceptance `22/22`, Roadmap `Done`, PR #72 hazır |
| Issue #77 teknik checkpoint'i | Technical source `197233688c4fe587097dbfc1cbee843cfc78603e`, tree `58458f400a7efaa68e452a0e85e35d6d7eb5a3ab`; canonical processor cooler gerçek pickup/carry/keyed 90° preview/placement ile beşinci capacity-one BuildKit'e taşındı ve work-ticket `4/10 → 5/10` ilerledi. Mac r39 ve collision-free Windows `issue77-hardened-v2` native kapıları geçti; source/docs/Guard/final receipt/local+USB lifecycle henüz açık, Issue/Roadmap In Progress |
| Issue #79 kapanış checkpoint'i | Technical source `f40ef21058caf1a2aca3054218abfc1dd7305c01`, tree `c7500e7300f75f5d9b089bf23657750dccc5ffed`; canonical graphics card gerçek pickup/carry/keyed 180° preview/placement ile altıncı capacity-one BuildKit'e taşındı ve work-ticket `5/10 → 6/10` ilerledi. Mac r40 ve collision-free Windows `issue79-hardened-v3` native kapıları geçti. Source/docs `dd607d0af346bd1f0e28449f606761bc97e1495c`, Guard `32874685021`, local+fiziksel USB exact `990/975/14`, metadata `880523fcb71208796cce96564556a2170363c92a`, Guard `32876194890`; acceptance `24/24`, Issue `CLOSED`, Roadmap `Done` |
| Issue #81 fiziksel checkpoint'i | Technical source `f3d80629e09c05afde97fa778c4b220ca456c5f0`, tree `851954879c1ff1e2ef98bc9a7a8469750304d992`; canonical power supply gerçek pickup/carry/keyed 180° preview/placement ile yedinci capacity-one BuildKit'e taşındı ve work-ticket `6/10 → 7/10` ilerledi. Mac r41 ve collision-free Windows `issue81-hardened-v1` native kapıları geçti. Source/docs `dc118bf0d26a11f3937cb114ef12f85666facc48`, Guard `32896033674`, local+fiziksel USB exact `1002/987/14`, metadata `ff935452c68bc77e66eb0742e0c3e6c0eb2894c7`, Guard `32897672990`; exact-r41 insan oturumu ve idari kapanış sürüyor |
| Issue #83 teknik checkpoint'i | Technical source `a36d713120283bd106aeca76509756d6dbb1dd30`, tree `2619dd8e1db812c9e3249657a2031a6268492b5a`; canonical modular ATX24 split cable aynı Unity instance ve stable serialized identity ile gerçek pickup/carry/keyed 180° preview/placement üzerinden sekizinci capacity-one BuildKit'e taşındı ve work-ticket `7/10 → 8/10` ilerledi. Focused domain `39/39`, r42 scene `9/9`, BuildKit fixture `44/44`, full EditMode `701/701`, full PlayMode `110/110` geçti. Universal Mac ve detached-clean Windows `issue83-hardened-v1` x64 IL2CPP/only-D3D11 exact-head native kapıları başarılıdır. Source/docs/Guard, canonical `14/14`, immutable local/fiziksel USB, exact-r42 insan ve idari lifecycle kapıları sürüyor; strict acceptance `22/25` |
| Issue #85 teknik checkpoint'i | Technical source `b6a74e932f4744b17388df7c7eb4d88f26d195f4`, tree `bd763ea0c8c6d2f5d256e467c4fca8b762ca4d84`; canonical modular EPS12V 8-pin cable aynı Unity instance ve stable serialized identity ile gerçek pickup/carry/keyed 180° preview/placement üzerinden dokuzuncu capacity-one BuildKit'e taşındı ve work-ticket `8/10 → 9/10` ilerledi. Full EditMode `705/705`, full PlayMode `115/115` geçti. Universal Mac ve detached-clean Windows `issue85-hardened-v1` x64 IL2CPP/only-D3D11 exact-head native kapıları başarılıdır. Source/docs/Guard, final canonical `14/14`, immutable local/sağlıklı fiziksel USB, exact-r43 insan ve idari lifecycle kapıları sürüyor; strict acceptance `22/25` |
| Issue #87 teknik checkpoint'i | Technical source `25dc39ab02de93a416800acd17f53aacf83dca09`, tree `a736a764d0a52e950a4139002d6febc629df5987`; canonical modular PCIe/GPU 8-pin 6+2 cable aynı Unity instance ve stable serialized identity ile gerçek pickup/carry/keyed 180° preview/placement üzerinden onuncu capacity-one BuildKit'e taşındı ve work-ticket `9/10 → 10/10` ilerledi. Full EditMode `709/709`, full PlayMode `116/116`, technical Guard `32921526334` geçti. Universal Mac ve detached-clean Windows `issue87-hardened-v1` x64 IL2CPP/only-D3D11 exact-head native kapıları başarılıdır. Source/docs/Guard, final canonical `14/14`, immutable local/sağlıklı fiziksel USB, exact-r44 insan ve idari lifecycle kapıları sürüyor; strict acceptance `23/25` |
| Issue #89 teknik checkpoint'i | Technical source `2fdf371206bc58c32e1c20d471f4abe7c0bfba01`, tree `c5e6de5942993a98735984caca4a04fd396105f6`; canonical motherboard historical `10/10` BuildKit'ten same-instance/stable ItemId ile ActorHands ve existing chassis Assembly Workbench'e taşındı. Guided seat, secure→unsecure→detach→reseat, live reservation/allocation, immutable history, other-nine untouched, exact replay/recovery ve no-duplicate-loss korunur. Full EditMode `712/712`, full PlayMode `119/119`, technical Guard `32930403290` geçti. Universal Mac ve detached-clean Windows `issue89-hardened-v1` x64 IL2CPP/only-D3D11 exact-head native kapıları başarılıdır. Source/docs/Guard, final canonical `14/14`, immutable local/sağlıklı fiziksel USB, exact-r45 insan ve idari lifecycle kapıları sürüyor; strict acceptance `22/25` |
| Son test/build | Issue #89 EditMode `712/712`, gerçek Input System PlayMode `119/119`, failed/skipped/inconclusive `0`; Universal macOS build report `330104684` bayt ve Apple M1/Metal exact r45 smoke geçti. Detached-clean Windows `issue89-hardened-v1` x64 IL2CPP/only-D3D11 report `1340592635` bayt, expanded fatal-token `0`, Intel Iris Xe Direct3D 11.0 feature level 11.1 exact r45 smoke, exit `0`, graceful shutdown, task deletion ve player/Unity/task residue `0` verdi. Canonical teknik evidence `13/14`dür; final source receipt/source-docs Guard ve local+sağlıklı fiziksel USB readback henüz tamamlandı diye raporlanmaz |

Önceki zaman/olay Core commit'i `8af2ad3d05906839c4b607e4958650e723060465`, iş birliği/devir checkpoint'i `2ee421193833111f76c85dabb33910240c36db03` ve Issue #50–#62 kapanış checkpointleri tarihsel olarak korunur. Issue #63 ve #64 iki-platform teknik checkpointleri; Issue #66, #71 ve #79 tamamlanmış lifecycle checkpointleri; Issue #77, #81, #83, #85, #87 ve #89 açık lifecycle checkpointleri ilgili ADR-0041, ADR-0042, ADR-0043, ADR-0045, ADR-0048, ADR-0049, ADR-0050, ADR-0051, ADR-0052, ADR-0053 ve ADR-0054 ile tarihli Evidence belgelerinde kayıtlıdır. Fiziksel USB ve final metadata/Guard kapıları tamamlanmadan kalan açık Issue'lar kapalı sayılmaz.

## 16. Sıradaki uygulama sırası

1. Issue #89 exact dokuz dosyalık source/docs commit/push ve draft PR #90 üzerinden Repository Guard'ı geçir; final exact `14/14` canonical evidence/source receipt'i oluştur ve immutable local paketi doğrula. USB disk/volume identity ve sağlık kapısı temiz olmadan fiziksel USB'ye yazma.
2. Issue #89 exact-r45 gerçek insan pickup→seat→secure→unsecure→detach→reseat oturumunu `Docs/Quality/HUMAN-PLAY-ACCEPTANCE-GATE.md` matrisine göre keyboard/mouse ve gamepad ile ayrıca kaydet. Sağlıklı fiziksel USB geldiğinde collision-free incoming→tam readback→atomik final→ikinci readback zincirini uygula; ancak tüm `25/25` geçince Issue/Project/PR idari zincirini kapat. Önceki açık insan/lifecycle kayıtlarını Issue #89 kanıtıyla karıştırma.
3. BuildKit staging `10/10` ve motherboard Assembly handoff hazırdır. Sonraki bounded oyuncu-görünür paket CPU socket/retention installation olmalıdır; BuildKit receipt'lerini Assembly/route/electrical authority yerine kullanma. Ardından DIMM, M.2, cooler/TIM, GPU, PSU, cable routing, electrical readiness/power-on, POST/BIOS/OS/driver, benchmark/QA, paketleme/teslimat ve final settlement sırasını koru; graybox'ı final art veya Guardian'ı gameplay authority sayma.

Her adım ayrı issue, test, commit ve checkpoint olarak kapanır. Büyük asset, ücretli araç, Steam/Apple ödemesi veya gerçek Windows IL2CPP kurulumu ayrı maliyet/izin kapısıdır.

## 17. Açık büyük kararlar

- Nihai ticari oyun adı ve marka araştırması.
- Büyük binary asset öncesi Git LFS politikası.
- Steamworks onboarding ve mağaza sayfası zamanlaması.
- Online crash/telemetry sağlayıcısı kullanılıp kullanılmayacağı; gizlilik/opt-in sınırı.
- Windows 1.0 sonrası macOS bütçesi, imzalama ve Apple Developer planı.

## 18. Başlıca riskler

- Tek kişi için kapsamın sürdürülemez büyümesi.
- Ellerde/fizikte titreme ve hassas montajın yorucu olması.
- Müşteri/çalışan AI'nin performans ve edge-case yükü.
- Save migration ve stok/para invariant hataları.
- Gerçek Windows doğrulamasının geç kalması.
- Kurgusal ürün içeriğinin teknik doğruluk ve üretim yükü.
- Üçüncü taraf asset/lisans/provenans kaybı.
- Public paylaşımda marka, kişisel veri veya proprietary kaynak sızıntısı.

Riskler [`Docs/ProjectBible/06_PROJE_HAFIZASI.md`](Docs/ProjectBible/06_PROJE_HAFIZASI.md) içinde ID'lerle izlenir.

## 19. Repository gerçeği

Authoritative remote private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D), authoritative dal `main`, yerel çalışma kökü ise bu Unity Git deposudur. Codex'te yanlışlıkla oluşturulan ayrı `Game` proje kaydı 13 Ağustos 2026'da kaldırıldı; kaynak klasörü, `.git` ve GitHub remote'u değişmedi. Günlük konuşma mevcut ana `PC Shop Empire Similator` projesinde sürer. Kaynak türleri:

- **Canlı:** Unity kaynakları, root `PROJECT_BIBLE.md`, `Docs/`, `SourceAssets/`, `Tools/`.
- **Salt okunur geçmiş:** `LegacyReference/PC-Shop-Empire-1.1.6/Source/`; manifest değişmeden korunur.
- **Yeniden üretilebilir ve Git dışı:** `Library`, `Temp`, `Logs`, `UserSettings`, IDE dosyaları, build çıktıları.
- **Asla Git'e girmez:** token, credential, certificate/private key, kullanıcı telemetry ham verisi.

GitHub Issues iş birimi, [PC Shop Empire 3D — Development Roadmap](https://github.com/users/cixanla/projects/2) ise görünür durum panosudur. Tasarım gerçeği issue yorumlarında kaybolmaz; kalıcı karar root Bible, ilgili ayrıntılı belge veya ADR'ye işlenir. Eski public `cixanla/PC-Shop-Empire` repository'si yalnız legacy release/indirme geçmişidir ve bu migration sırasında değiştirilmemiştir.

## 20. Yeni geliştirici için 15 dakikalık devir

1. Bu belgeyi ve [`Docs/DEVELOPER-HANDOFF.md`](Docs/DEVELOPER-HANDOFF.md) dosyasını oku.
2. Private depoyu clone et; `main` üzerinde doğrudan deneme yapma.
3. Unity Hub ile tam `ProjectSettings/ProjectVersion.txt` sürümünü kur.
4. `./Tools/verify-repository.sh` çalıştır.
5. Güncel Issue #79 baseline'ı olan Edit Mode 690/690 ve Play Mode 100/100 testlerini doğrula.
6. GitHub Project'te atanmış issue'yu ve kabul ölçütünü oku.
7. Küçük branch aç; gameplay ile mimari migration'ı aynı PR'a yığma.
8. Test, `PROJECT_BIBLE`, ilgili ADR/provenans ve changelog kontrolünü tamamla.

Tam komutlar ve platform notları handoff belgesindedir.

## 21. Her değişiklikte zorunlu yaşayan kayıt

Her push/PR şu sorulara cevap vermelidir:

- Ne değişti ve neden?
- Hangi issue/karar ve kabul ölçütüne bağlı?
- Hangi sistem, ekonomi, AI, save, performans veya içerik yükünü etkiliyor?
- Hangi test/manuel doğrulama geçti?
- Yeni asset/paket/veri varsa kaynağı ve lisansı nedir?
- Şimdi tamamlanan nedir, sıradaki tek iş nedir?

Material değişiklikte güncellenecek yerler:

1. Bu dosyanın **güncel durum**, **tamamlananlar**, **sıradaki sıra** veya **risk** bölümü.
2. Ayrıntı için ilgili `Docs/ProjectBible` belgesi.
3. Kalıcı teknik karar için yeni/tarihsel ADR.
4. Kullanıcıya görünen/depo yapısını etkileyen değişiklik için `CHANGELOG.md`.
5. Asset/paket için `Docs/PROVENANCE.md`.

Pull request şablonu bu kontrolü zorunlu hatırlatır. Kapsam değişmediyse “Project Bible değişikliği gerekmiyor” gerekçesi açıkça yazılır.

## 22. Ayrıntılı belge haritası

| Belge | İçerik |
|---|---|
| [`00_OKU_BENI`](Docs/ProjectBible/00_OKU_BENI.md) | Ana dizin ve güncel sonuç |
| [`01_GAME_DESIGN_BIBLE`](Docs/ProjectBible/01_GAME_DESIGN_BIBLE.md) | Bütün oyun sistemleri ve deneyim |
| [`02_DONUSUM_MATRISI`](Docs/ProjectBible/02_MEVCUT_PROJE_VE_DONUSUM_MATRISI.md) | Legacy Dashboard ve korunacak/dönüşecek/çıkarılacaklar |
| [`03_RAKIP_ARASTIRMASI`](Docs/ProjectBible/03_RAKIP_ARASTIRMASI_VE_FARKLILASMA.md) | Rakip güçlü/zayıf yanları ve özgün fark |
| [`04_TEKNIK_MIMARI`](Docs/ProjectBible/04_TEKNIK_MIMARI_ARACLAR_VE_GUARDIAN.md) | Modüller, save, Guardian, araç/lisans |
| [`05_YOL_HARITASI`](Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md) | Fazlar, bağımlılıklar, risk ve doğrulama |
| [`06_PROJE_HAFIZASI`](Docs/ProjectBible/06_PROJE_HAFIZASI.md) | Karar ID'leri, varsayımlar, riskler, açık kapılar |
| [`07_KAYNAKLAR`](Docs/ProjectBible/07_KAYNAKLAR.md) | Araştırma kaynak defteri |
| [`08_KURULUM_PLANI`](Docs/ProjectBible/08_CANONICAL_KAYNAK_VE_KURULUM_PLANI.md) | Canonical kaynak, araç sürümü ve geri alma |
| [`09_STAGE_A_RAPORU`](Docs/ProjectBible/09_STAGE_A_KURULUM_RAPORU.md) | Kurulum/build/test kanıtı |
| [`10_CHECKPOINT`](Docs/ProjectBible/10_DEVAM_CHECKPOINT.md) | Son sağlam devam noktası ve kullanım protokolü |
| [`11_BIRLESIK_CODEX_HAFIZASI`](Docs/ProjectBible/11_BIRLESIK_CODEX_PROJE_HAFIZASI.md) | Üç Codex görevinin ortak bağlamı, üretim geçmişi ve tek-kanal devam protokolü |
| [`CODEX_HISTORY`](Docs/CodexHistory/README.md) | Tam kullanıcı/Codex konuşmaları, dosya değişiklik envanteri ve Git dosya geçmişi |
| [`GITHUB_HANDOFF`](Docs/Evidence/GITHUB-HANDOFF-2026-08-11.md) | Private remote, Project, Codex, fresh clone ve USB devir özeti |

## 23. Telif ve özgünlük ilkesi

Rakip araştırması yalnız tasarım ilkesi ve oyuncu sorunlarını anlamak içindir. Başka oyunun kodu, adı, UI'ı, görseli, sesi, logosu, metni veya özgün içeriği kopyalanmaz. Ürün markaları kurgusaldır; gerçek teknik ilişkiler özgün veri modeliyle uygulanır. Her dış katkı/asset için lisans ve katkı hakkı yazılı kayda bağlanır.

Bu Bible, projenin yaşayan ana haritasıdır; kod gerçeğinin yerine geçmez ama kodun nedenini ve sonraki yönünü kaybetmemeyi sağlar.

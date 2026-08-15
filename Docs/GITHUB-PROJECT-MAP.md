# GitHub Project Haritası

**Project:** [PC Shop Empire 3D — Development Roadmap](https://github.com/users/cixanla/projects/2)<br>
**Owner:** `cixanla`  
**Repository:** private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D)<br>
**Codex çalışma alanı:** mevcut ana `PC Shop Empire Similator`; authoritative Unity Git kökü `/Users/cixanla/Developer/PCShopEmpire3D/Game` (ayrı `Game` kaydı 13 Ağustos 2026'da kaldırıldı)

GitHub Project günlük görünür görev durumudur; ayrıntılı kapsam gerçeği `PROJECT_BIBLE.md` ve `Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md` içinde kalır.

## Alanlar

| Alan | Değerler |
|---|---|
| Status | Todo, In Progress, Done |
| Phase | Foundation, Retail, Assembly, Vertical Slice, AI, Service, Economy, Growth, Content, Alpha, Steam 1.0, macOS |
| Priority | P0, P1, P2 |
| Risk | Low, Medium, High, Critical |

## Başlangıç epic/issue seti

| Sıra | Issue | Epic | Öncelik | Bağımlılık | Ana doğrulama |
|---:|---|---|---|---|---|
| 1 | [#1](https://github.com/cixanla/PC-Shop-Empire-3D/issues/1) | Repository, handoff ve project governance | P0 | Yok | Farklı clone + repo guard + docs bulunabilirliği |
| 2 | [#2](https://github.com/cixanla/PC-Shop-Empire-3D/issues/2) | Deterministik RNG ve event bağlamı | P0 | Core time/event | Aynı seed/context aynı sonuç; reload reroll yok |
| 3 | [#3](https://github.com/cixanla/PC-Shop-Empire-3D/issues/3) | Domain event dispatcher/correlation sınırı | P0 | RNG/event envelope | Sıra, duplicate ve hata izolasyonu testleri |
| 4 | [#4](https://github.com/cixanla/PC-Shop-Empire-3D/issues/4) | Birinci şahıs graybox hareket ve input | P0 | Core temel | Mouse/gamepad/rebind, stabil frame ve hareket konforu |
| 5 | [#5](https://github.com/cixanla/PC-Shop-Empire-3D/issues/5) | Görünür eller ve alma/bırakma | P0 | Hareket/input | Titremesiz küçük nesne etkileşimi ve fallback |
| 6 | [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) | Hibrit kutu taşıma ve placement | P0 | Eller/etkileşim | Küçük/büyük kutu/araba, kaybolmayan stok projeksiyonu |
| 7 | [#7](https://github.com/cixanla/PC-Shop-Empire-3D/issues/7) | Catalog + Inventory çekirdeği | P0 | ID/result/event | Ürün/instance/batch/container/reservation invariant testleri |
| 8 | [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) | Sipariş, teslimat ve raf döngüsü | P0 | Catalog/Inventory/World | Dashboard siparişinden fiziksel satışa zincir |
| 9 | [#9](https://github.com/cixanla/PC-Shop-Empire-3D/issues/9) | Müşteri gezinme, danışmanlık ve kasa | P0 | Retail/World | Timeout/fallback, ihtiyaç ve checkout doğruluğu |
| 10 | [#10](https://github.com/cixanla/PC-Shop-Empire-3D/issues/10) | Fiziksel PC toplama teknik prototipi | P0 | Catalog/Inventory/Orders | Tek build'in teklif–montaj–test–teslim zinciri |
| 11 | [#11](https://github.com/cixanla/PC-Shop-Empire-3D/issues/11) | Save/journal/migration/recovery | P0 | Domain çekirdekleri | Fault injection ve son sağlam snapshot fallback |
| 12 | [#12](https://github.com/cixanla/PC-Shop-Empire-3D/issues/12) | Guardian event/invariant/report iskeleti | P0 | Event/save | Kod değiştirmeyen, neden zincirli offline rapor |
| 13 | [#13](https://github.com/cixanla/PC-Shop-Empire-3D/issues/13) | Vertical slice entegrasyonu | P0 | 4–12 | Baştan sona garaj günü ve tek PC işi |
| 14 | [#14](https://github.com/cixanla/PC-Shop-Empire-3D/issues/14) | Çalışanlar ve gelişmiş müşteri AI | P1 | Vertical slice | Roller, görev rezervasyonu, LOD tutarlılığı |
| 15 | [#15](https://github.com/cixanla/PC-Shop-Empire-3D/issues/15) | Servis, garanti, iade ve ikinci el | P1 | Assembly/Orders/Inventory | Intake–teşhis–onay–onarım–teslim |
| 16 | [#16](https://github.com/cixanla/PC-Shop-Empire-3D/issues/16) | Dinamik ekonomi, tedarikçi ve risk | P1 | Ledger/retail | Seed'li trend, bounded şok, iflas basamakları |
| 17 | [#17](https://github.com/cixanla/PC-Shop-Empire-3D/issues/17) | İtibar, reklam, rekabet ve büyüme | P1 | Ekonomi/AI | Nedensel KPI ve fiziksel müşteri etkisi |
| 18 | [#18](https://github.com/cixanla/PC-Shop-Empire-3D/issues/18) | İçerik, sanat, ses ve kariyer üretimi | P1 | Vertical slice kalite çubuğu | Provenans, performans ve içerik kabul kapısı |
| 19 | [#19](https://github.com/cixanla/PC-Shop-Empire-3D/issues/19) | Alpha, erişilebilirlik ve optimizasyon | P0 | Feature complete hedefi | Profil, uzun soak, save ve erişilebilirlik matrisi |
| 20 | [#20](https://github.com/cixanla/PC-Shop-Empire-3D/issues/20) | Demo ve Steam Playtest | P0 | Stabil alpha adası | Telemetry izinleri, geri bildirim ve crash triage |
| 21 | [#21](https://github.com/cixanla/PC-Shop-Empire-3D/issues/21) | Windows x64 IL2CPP + Steam 1.0 | P0 | Beta/gerçek Windows PC | Temiz PC, DirectX/GPU, Steam ve release checklist |
| 22 | [#22](https://github.com/cixanla/PC-Shop-Empire-3D/issues/22) | macOS port, signing ve notarization | P2 | Windows 1.0 + bütçe | Universal/native performans, Apple QA ve notarization |

Issue numaraları ve kalıcı URL'ler yukarıda kayıtlıdır. Project item ID'leri GitHub'ın iç uygulama detayıdır ve yaşayan belgelere sabitlenmez. Büyük epicler uygulama başlamadan daha küçük acceptance-odaklı issue'lara bölünür.

## Aktif epic alt işleri

| Epic | Alt issue | Sonuç |
|---|---|---|
| [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) | [#31](https://github.com/cixanla/PC-Shop-Empire-3D/issues/31) — Küçük kutu kontrollü placement ve ghost | 0,25 m grid/90° yaw snap, valid/invalid ghost, güvenli overlap ve gerçek input testleri tamamlandı |
| [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) | [#32](https://github.com/cixanla/PC-Shop-Empire-3D/issues/32) — Büyük kutu taşıma profili ve güvenli bırakma | Tamamlandı; 0,65× hız, sprint kilidi, motion-safe FOV, iki-el durumu, fail-closed drop ve gerçek input testleri |
| [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) | [#33](https://github.com/cixanla/PC-Shop-Empire-3D/issues/33) — Küçük kutu placement rotation inputu | Tamamlandı; `R / Right Shoulder`, 90° deterministik adım, etkin prompt ve döndürülmüş footprint doğrulaması |
| [#18](https://github.com/cixanla/PC-Shop-Empire-3D/issues/18) | [#34](https://github.com/cixanla/PC-Shop-Empire-3D/issues/34) — Garaj okunaklı yarı gerçekçi benchmark köşesi | Tamamlandı; bevel/PBR yüzey/ışık-post-process kalite çubuğu, 128/128 EditMode, 10/10 PlayMode ve gerçek player `lookdev=ok` |
| [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) | [#35](https://github.com/cixanla/PC-Shop-Empire-3D/issues/35) — Küçük kutu güvenli istifleme | Tamamlandı; stable destek, merkez/90° snap, tam footprint/overlap, tek üst kutu, 131/131 EditMode, 12/12 PlayMode ve `stacking=ok` |
| [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) | [#37](https://github.com/cixanla/PC-Shop-Empire-3D/issues/37) — Yüklü taşıma arabası graybox akışı | Tamamlandı; tek `LargeBox`, hands→cart→hands stable transfer, dört noktalı destek/swept obstruction, gerçek keyboard/gamepad, 136/136 EditMode, 14/14 PlayMode ve `cart-flow=ok` |
| [#7](https://github.com/cixanla/PC-Shop-Empire-3D/issues/7) | [#38](https://github.com/cixanla/PC-Shop-Empire-3D/issues/38) — Catalog ve authoritative Inventory temeli | Tamamlandı; saf Catalog/Inventory assembly'leri, serialized/batch/container/transfer/reservation invariantları, 161/161 EditMode ve 14/14 regresyon PlayMode |
| [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) | [#39](https://github.com/cixanla/PC-Shop-Empire-3D/issues/39) — Purchase order ve atomik receiving kabulü | Tamamlandı; exact manifest, lifecycle, mixed bulk intake, iki-authority no-mutation, 184/184 EditMode ve 14/14 regresyon PlayMode |
| [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) | [#40](https://github.com/cixanla/PC-Shop-Empire-3D/issues/40) — Görünür teslimat kabulü ve authoritative raf transferi | Tamamlandı; Receiving→ActorHands→Shelf/WorldFloor domain-first projection, rollback/recovery, 188/188 EditMode, 17/17 PlayMode ve gerçek player `stock-flow=ok` |
| [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) | [#41](https://github.com/cixanla/PC-Shop-Empire-3D/issues/41) — Teslimat kolisi açma ve exact manifest projection'ı | Tamamlandı; sealed→opened idempotent reveal, exact manifest/container no-mutation, 192/192 EditMode, 17/17 PlayMode ve gerçek player `parcel-open=ok` |

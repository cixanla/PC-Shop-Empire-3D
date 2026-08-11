# GitHub Project Haritası

**Project:** PC Shop Empire 3D — Development Roadmap  
**Owner:** `cixanla`  
**Repository:** private `cixanla/PC-Shop-Empire-3D`

GitHub Project günlük görünür görev durumudur; ayrıntılı kapsam gerçeği `PROJECT_BIBLE.md` ve `Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md` içinde kalır.

## Alanlar

| Alan | Değerler |
|---|---|
| Status | Todo, In Progress, Done |
| Phase | Foundation, Retail, Assembly, Vertical Slice, AI, Service, Economy, Growth, Content, Alpha, Steam 1.0, macOS |
| Priority | P0, P1, P2 |
| Risk | Low, Medium, High, Critical |

## Başlangıç epic/issue seti

| Sıra | Epic | Öncelik | Bağımlılık | Ana doğrulama |
|---:|---|---|---|---|
| 1 | Repository, handoff ve project governance | P0 | Yok | Farklı clone + repo guard + docs bulunabilirliği |
| 2 | Deterministik RNG ve event bağlamı | P0 | Core time/event | Aynı seed/context aynı sonuç; reload reroll yok |
| 3 | Domain event dispatcher/correlation sınırı | P0 | RNG/event envelope | Sıra, duplicate ve hata izolasyonu testleri |
| 4 | Birinci şahıs graybox hareket ve input | P0 | Core temel | Mouse/gamepad/rebind, stabil frame ve hareket konforu |
| 5 | Görünür eller ve alma/bırakma | P0 | Hareket/input | Titremesiz küçük nesne etkileşimi ve fallback |
| 6 | Hibrit kutu taşıma ve placement | P0 | Eller/etkileşim | Küçük/büyük kutu/araba, kaybolmayan stok projeksiyonu |
| 7 | Catalog + Inventory çekirdeği | P0 | ID/result/event | Ürün/instance/batch/container/reservation invariant testleri |
| 8 | Sipariş, teslimat ve raf döngüsü | P0 | Catalog/Inventory/World | Dashboard siparişinden fiziksel satışa zincir |
| 9 | Müşteri gezinme, danışmanlık ve kasa | P0 | Retail/World | Timeout/fallback, ihtiyaç ve checkout doğruluğu |
| 10 | Fiziksel PC toplama teknik prototipi | P0 | Catalog/Inventory/Orders | Tek build'in teklif–montaj–test–teslim zinciri |
| 11 | Save/journal/migration/recovery | P0 | Domain çekirdekleri | Fault injection ve son sağlam snapshot fallback |
| 12 | Guardian event/invariant/report iskeleti | P0 | Event/save | Kod değiştirmeyen, neden zincirli offline rapor |
| 13 | Vertical slice entegrasyonu | P0 | 4–12 | Baştan sona garaj günü ve tek PC işi |
| 14 | Çalışanlar ve gelişmiş müşteri AI | P1 | Vertical slice | Roller, görev rezervasyonu, LOD tutarlılığı |
| 15 | Servis, garanti, iade ve ikinci el | P1 | Assembly/Orders/Inventory | Intake–teşhis–onay–onarım–teslim |
| 16 | Dinamik ekonomi, tedarikçi ve risk | P1 | Ledger/retail | Seed'li trend, bounded şok, iflas basamakları |
| 17 | İtibar, reklam, rekabet ve büyüme | P1 | Ekonomi/AI | Nedensel KPI ve fiziksel müşteri etkisi |
| 18 | İçerik, sanat, ses ve kariyer üretimi | P1 | Vertical slice kalite çubuğu | Provenans, performans ve içerik kabul kapısı |
| 19 | Alpha, erişilebilirlik ve optimizasyon | P0 | Feature complete hedefi | Profil, uzun soak, save ve erişilebilirlik matrisi |
| 20 | Demo ve Steam Playtest | P0 | Stabil alpha adası | Telemetry izinleri, geri bildirim ve crash triage |
| 21 | Windows x64 IL2CPP + Steam 1.0 | P0 | Beta/gerçek Windows PC | Temiz PC, DirectX/GPU, Steam ve release checklist |
| 22 | macOS port, signing ve notarization | P2 | Windows 1.0 + bütçe | Universal/native performans, Apple QA ve notarization |

Issue numarası, URL ve Project item ID'leri remote kurulum tamamlandıktan sonra bu dosyaya eklenir. Büyük epicler uygulama başlamadan daha küçük acceptance-odaklı issue'lara bölünür.

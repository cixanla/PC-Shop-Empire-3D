# Changelog

Bu dosya teknik ve proje yönetimi checkpointlerini izler. Ayrıntılı oyun kararları `PROJECT_BIBLE.md`, `Docs/ProjectBible/06_PROJE_HAFIZASI.md` ve ADR'lerde tutulur.

## Unreleased

### Added

- Private GitHub collaboration/devir yapısı, living Project Bible, governance ve katkı şablonları tamamlandı.
- Full design/research package repository içine taşındı.
- Canonical PC Shop Empire 1.1.6 legacy kaynak snapshot'ı ve manifesti eklendi.
- Private `cixanla/PC-Shop-Empire-3D` remote, repository guard workflow, 22 epic ve Development Roadmap Project oluşturuldu.
- Sürümlü `pcg32-xsh-rr-64-32-v1` deterministik rastgele akışı, raw state snapshot/restore, official golden vector ve modulo-bias üretmeyen bounded integer eklendi.
- Save-safe canonical root seed ve sürümlü SHA-256 framed domain/context stream derivation eklendi; reload-reroll ve çağrı sırası bağımsızlığı golden testlerle kilitlendi.
- Domain event correlation/causation ve deterministik FIFO dispatcher; duplicate/conflict politikası, breadth-first nested enqueue, bounded drain ve handler hata izolasyonuyla eklendi.
- `PSE.World`/`PSE.Presentation`, oynanabilir GarageGraybox, connected PlayerRig, klavye/fare + gamepad hareket/kamera, rebind store ve görünür prototip eller eklendi.
- Stable fiziksel ürün kimliği, menzil/görüş hattı hedefleme, tek taşıma slotu, görünür el pozları, dinamik binding prompt'u ve güvenli pickup/drop eklendi.
- Engelli/zeminsiz drop fail-closed kaldı; player disable ve dünya-altı düşüş aynı nesneyi son güvenli pozuna kurtarıyor.
- Küçük kutu için işaretli stock surface, 0,25 m grid/90° yaw snap, tam destek/overlap doğrulaması, renk + metin ghost ve stabil kinematic placement eklendi.
- Mouse-left/gamepad RT placement modunu açar; `G / Gamepad East` mod açıkken onaylar, kapalıyken önceki güvenli drop'u korur.
- Büyük kutu için ayrı stable kimlik/boyut ve carry profili, turuncu bantlı graybox, geniş iki-el pozu, `0,65×` hareket, sprint kilidi ve motion-safe bounded FOV eklendi.
- Büyük kutu placement moduna girmez; etkin `G / Gamepad East` promptu, gerçek boyutlu fail-closed drop, engelli geri bildirim ve disable recovery aynı item kimliğini korur.
- Küçük kutu placement moduna `R / Right Shoulder` ile deterministik 90° clockwise rotation, etkin binding/açı promptu ve döndürülmüş footprint güvenlik doğrulaması eklendi.
- Dikdörtgen küçük kutu ve üst yön işareti GarageGraybox'ta rotation'ı görünür kılar; ghost ile onaylanan poz aynı solver sonucunu kullanır.
- Okunaklı yarı gerçekçilik görsel yönü kabul edildi: gerçek oran/PBR yüzey/zemine oturan ışık/doğal ağırlık, hafif stilize okunabilirlik ve ölçülü performans bütçesi.
- GarageGraybox tek-köşe benchmarkına bevel'lı tezgâh/raf, prosedürel beton/duvar/metal/karton/ahşap yüzeyler, etiket detayları, görev ışığı, ACES, ölçülü bloom ve reflection probe eklendi.
- Stable küçük kutu üstüne merkez/90° snap, beş noktalı tam footprint, overlap engeli, tek kat/tek üst ilişkisi, dolu taban pickup kilidi ve `İSTİF GEÇERLİ` geri bildirimi eklendi.
- Edit Mode baseline `131/131`, Play Mode baseline `12/12` teste yükseldi; Universal macOS build ve Apple M4/Metal 1280×720 `rotation=ok stacking=ok lookdev=ok` gerçek player smoke geçti.

### Changed

- PCG32'nin yalnız 63-bit benzersiz stream alanı açık sözleşmeye dönüştürüldü; high-bit selector alias'ı sessizce kabul edilmiyor.
- Yanlışlıkla oluşturulan ayrı Codex `Game` proje kaydı kaldırıldı; Unity kaynak klasörü ve GitHub bağlantısı korunuyor.
- Repository Guard checkout action, Node.js 20 deprecation uyarısını kaldırmak için resmî güncel major `actions/checkout@v7`ye yükseltildi.
- Pickup/drop + kontrollü placement milestone'ı ayrı USB hedefinde 336 tracked dosya ve SHA-256 manifest ile geri okunarak doğrulandı.
- Kontrollü küçük-kutu istifleme milestone'ı final tracked kaynak ve test/build/runtime kanıtlarıyla ayrı USB hedefinde SHA-256 manifest/readback kapısından geçirildi.

## 2026-08-11 — Stage B Core Foundation

### Added

- Unity bağımsız `PSE.Core` assembly sınırı.
- Tür kapsamlı `StableId<TScope>`.
- Makine-okunur `Failure.Code`, `OperationResult` ve `OperationResult<T>`.
- Integer `SimulationDuration` / `SimulationTimestamp`.
- Açık-adımlı, pause güvenli `SimulationClock`.
- Stable metadata ve schema taşıyan immutable domain event envelope.
- Toplam 42 geçen Edit Mode testi.

## 2026-08-11 — Stage A Technical Baseline

### Added

- Unity 6000.3.21f1 + URP projesi ve kilitli resmî paketler.
- macOS Universal development build/headless smoke.
- Windows x64 Mono cross-build.
- Yerel Git `main` geçmişi ve `stage-a-baseline-2026-08-11` etiketi.
- Hash doğrulamalı USB milestone snapshot'ı.

### Known limitations

- Gerçek Windows x64 runtime/IL2CPP/DirectX/Steam testi henüz yapılmadı.
- UVCS ilk check-in'i uzak bağlantı reseti nedeniyle beklemede; Git tek authoritative VCS'dir.
- Küçük kutu alma/bırakma/placement/rotation/tek-kat istif ve büyük-kutu güvenli taşıma çalışıyor; gelişmiş el animasyonu, taşıma arabası, gerçek raf/Inventory authority ve final sanat henüz tamamlanmadı.

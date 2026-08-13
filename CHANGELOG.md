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
- Edit Mode baseline `105/105` teste yükseldi.

### Changed

- PCG32'nin yalnız 63-bit benzersiz stream alanı açık sözleşmeye dönüştürüldü; high-bit selector alias'ı sessizce kabul edilmiyor.
- Yanlışlıkla oluşturulan ayrı Codex `Game` proje kaydı kaldırıldı; Unity kaynak klasörü ve GitHub bağlantısı korunuyor.
- Repository Guard checkout action, Node.js 20 deprecation uyarısını kaldırmak için resmî güncel major `actions/checkout@v7`ye yükseltildi.

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
- Gameplay, graybox, eller ve mağaza sahnesi henüz başlamadı.

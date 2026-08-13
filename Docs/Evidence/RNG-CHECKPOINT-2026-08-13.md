# Deterministik RNG Teknik Kanıtı — 13 Ağustos 2026

## Kaynak

- Feature commit: `bbb3648c6e34eedd77e1bec948d5ee630f89679c`
- Tree: `561eaeaf2bfa424ab075b42eeb084175efed01da`
- Issue: [#23 — Versioned deterministic PRNG state and golden vectors](https://github.com/cixanla/PC-Shop-Empire-3D/issues/23)
- ADR: `Docs/ADR-0007-DETERMINISTIC-RANDOM-STREAM.md`

## Uygulanan sözleşme

- Algorithm ID: `pcg32-xsh-rr-64-32-v1`.
- PCG XSH-RR 64/32 set-sequence, 64-bit state ve odd increment.
- Benzersiz stream selector alanı `0..0x7FFF_FFFF_FFFF_FFFF`; high-bit alias reddedilir.
- Raw state+increment capture/restore aynı sonraki draw'dan devam eder.
- `NextInt32(exclusiveMax)` unsigned rejection threshold ile modulo bias üretmez.
- Geçersiz bound/snapshot/selector state'i sessizce normalize etmez.
- Unity, Editor, OS clock, `System.Random` ve `UnityEngine.Random` bağımlılığı yoktur.

## Doğrulama

- Unity Editor: `6000.3.21f1` Apple Silicon.
- Edit Mode: 62 total, 62 passed, 0 failed, 0 skipped.
- Resmî 42/54 golden vector ve altı draw sonrası raw state/increment geçti.
- 1.000 draw eşitliği, farklı stream tekrarlanabilirliği, snapshot continuation, boundary, rejection ve exception-state testleri geçti.
- `./Tools/verify-repository.sh`: başarılı.
- `git diff --check`: başarılı.
- Bağımsız code review: kritik/önemli açık bulgu yok.

Ham Unity test XML/log dosyaları Git dışında tutulur; kesin boyut ve SHA-256 değerleri yaşayan `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md` içindedir.

## Sınır

Bu checkpoint yalnız PRNG çekirdeğini tamamlar. Saved root seed, canonical context hashing/stream derivation, save serializer ve gameplay entegrasyonu sonraki paketlerdir. Gerçek Windows x64 golden-vector/regresyon doğrulaması ilk Windows test kapısında ayrıca yapılacaktır.

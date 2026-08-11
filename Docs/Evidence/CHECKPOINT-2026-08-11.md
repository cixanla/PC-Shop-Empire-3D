# Doğrulanmış Teknik Checkpoint — 11 Ağustos 2026

Bu dosya ham build/test loglarını Git'e taşımadan devredilebilir kanıt özetini tutar. Ham loglar yerel doğrulama klasöründe ve hash doğrulamalı milestone snapshot'ında saklanır; credential, mutlak kullanıcı yolu ve yeniden üretilebilir gürültü nedeniyle repository kaynağı sayılmaz.

## Source

- Branch: `main`.
- Stage A tag: `stage-a-baseline-2026-08-11`.
- Stage A root commit: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`.
- Core assembly commit: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`.
- Stable identity/result commit: `4cd2d928dbfda1886632bacce4a141c2a43161df`.
- Deterministic time/event commit: `8af2ad3d05906839c4b607e4958650e723060465`.

## Unity ve test

- Unity: `6000.3.21f1`, Apple Silicon ARM64 Editor.
- URP: `17.3.0`.
- Son Edit Mode run: `42` total, `42` passed, `0` failed, `0` skipped.
- Süre: yaklaşık `0.023` saniye test gövdesi; Editor import/startup hariç.
- Core assembly sınır kontrolü: `PSE.Core` UnityEngine/UnityEditor referansı taşımıyor.

## Build kanıtı

- macOS Universal development build: üretildi.
- macOS headless smoke: motor açıldı ve temiz kapandı.
- Windows x64 Mono development cross-build: üretildi.
- Bu sonuç gerçek Windows runtime, DirectX/GPU, Steam veya Windows IL2CPP kanıtı değildir.

## Legacy

- Canonical USB kaynak: 26 dosya.
- Yerel inceleme aynası: 26/26 yol, byte boyutu ve SHA-256 eşleşmesi.
- Private repository snapshot: `LegacyReference/PC-Shop-Empire-1.1.6/Source`.
- Repository manifest: 26/26 index blobu byte-for-byte doğrulanmalıdır.

## Repository güvenliği

- Bilinen credential/private-key kalıbı: 0.
- Track edilen generated/cache/build yolu: 0.
- Tek yeni dosya 10 MB üstünde: 0.
- Legacy dosyaları normal executable mode taşımaz; yalnız `Tools/verify-repository.sh` executable'dır.
- Git LFS henüz gerekmez; büyük binary asset öncesi ayrı kapıdır.

## Yeniden doğrulama

```bash
./Tools/verify-repository.sh
```

Unity testi ayrıca `Docs/DEVELOPER-HANDOFF.md` komutuyla çalıştırılır. Bir checkpoint yalnız test raporu gerçekten oluşmuş, Git çalışma ağacı açıklanmış ve remote/USB manifest doğrulaması tamamlanmışsa sağlam sayılır.

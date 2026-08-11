# PC Shop Empire 1.1.6 — Canonical Legacy Reference

Bu klasör eski Electron tabanlı PC Shop Empire 1.1.6 oyununun doğrulanmış tarihsel kaynak snapshot'ıdır.

## Kaynak

Canonical fiziksel kaynak:

`/Volumes/cixanla/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

11 Ağustos 2026'da USB kaynağı ile yerel inceleme aynası 26/26 dosyada göreli yol, boyut ve SHA-256 düzeyinde eşleşmiştir. Repository içindeki `Source/` kopyası aynı 26 dosyadan oluşturulmuştur. `CANONICAL-MANIFEST.tsv` her dosyanın hash, byte boyutu ve göreli yolunu taşır.

## Politika

- `Source/` salt okunur referanstır; normal feature geliştirmesinde düzenlenmez.
- Yeni oyun bu kaynak kodun 3D portu değildir; Unity + C# ile sıfırdan kurulur.
- Tema, Dashboard semantiği ve tasarım niyeti `Docs/ProjectBible/02_MEVCUT_PROJE_VE_DONUSUM_MATRISI.md` üzerinden yeni sisteme çevrilir.
- Legacy görsel/UI/kod yeni oyuna doğrudan kopyalanmaz.
- Hak bildirimi `Source/GAME_LICENSE.txt`, üçüncü taraf koşulları `Source/THIRD_PARTY_NOTICES.txt` içindedir.

## Doğrulama

Repository kökünde:

```bash
./Tools/verify-repository.sh
```

Manifest değişirse bu sıradan bir feature değişikliği sayılamaz; yeni canonical kaynak, neden, hak/provenans incelemesi ve ADR gerekir.

# Tools

Bu UVCS kontrollü klasör proje dışı, tekrar üretilebilir yardımcı araçların kaynakları içindir. Üçüncü taraf binary veya gizli anahtar burada tutulmaz.

## Checkpoint package verification

`verify-checkpoint-package.sh`, yerel staging, USB `.incoming-*` ve atomik olarak adlandırılmış final milestone ağacını değiştirmeden denetler. Manifest, exact Git blob/path/size eşliği, dışarıdaki canonical evidence dizini, forbidden/cache/credential yolları, secret imzaları ve AppleDouble sidecar kapıları birlikte geçmeden başarı üretmez.

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue66
```

Opsiyonel dördüncü argüman verilmezse araç canonical evidence dizininin boş olmayan exact dosya setini zorlar. `issue66` modu ayrıca dokuz canonical kanıt adını ve exact `9/9` sayısını sabit sözleşme olarak doğrular. Araç, paket içindeki `SOURCE_COMMIT.txt` dosyasında tam `Source/docs commit:` ve `Source/docs tree:` satırlarını zorunlu kılar. Paket oluşturmaz, dosya temizlemez ve Git/USB/GitHub durumunu değiştirmez.

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

Issue #68'in procedure-bound on dört dosyalı sözleşmesi için mode açıkça verilmelidir:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue68
```

Opsiyonel dördüncü argüman verilmezse araç generic `canonical` moda düşer ve yalnız canonical evidence dizininin boş olmayan exact dosya setini zorlar. Issue'ya özel dosya adlarını ve sayısını kilitlemek için mode atlanmamalıdır. `issue66` modu dokuz canonical kanıt adını ve exact `9/9` sayısını; `issue68` modu `binary-manifest.json`, `build-il2cpp-d3d11.log`, `build-procedure.ps1`, `editmode.xml`, `launch-procedure.ps1`, `macos-build.log`, `macos-runtime.log`, `playmode.xml`, `procedure-manifest.json`, `runtime-d3d11.log`, `runtime-procedure.ps1`, `runtime-summary.json`, `source-receipt.json` ve `task-receipt.json` adlarını ve exact `14/14` sayısını sabit sözleşme olarak doğrular. `task-receipt.json`, final interactive Scheduled Task dönüşü, silinmesi ve process-residue cleanup kapısını immutable pakette korur. Araç, paket içindeki `SOURCE_COMMIT.txt` dosyasında tam `Source/docs commit:` ve `Source/docs tree:` satırlarını zorunlu kılar. Paket oluşturmaz, dosya temizlemez ve Git/USB/GitHub durumunu değiştirmez.

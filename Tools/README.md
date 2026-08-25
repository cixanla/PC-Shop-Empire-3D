# Tools

Bu UVCS kontrollü klasör proje dışı, tekrar üretilebilir yardımcı araçların kaynakları içindir. Üçüncü taraf binary veya gizli anahtar burada tutulmaz.

## Checkpoint package verification

`verify-checkpoint-package.sh`, yerel staging, USB `.incoming-*` ve atomik olarak adlandırılmış final milestone ağacını değiştirmeden denetler. Manifest, exact Git blob/path/size eşliği, dışarıdaki canonical evidence dizini, forbidden/cache/credential yolları, secret imzaları ve AppleDouble sidecar kapıları birlikte geçmeden başarı üretmez. Final ad üzerinde çalıştırıldığında aynı parent altındaki kalan `.incoming-*`, `._.incoming-*` ve exact final `._<package-name>` sidecar sayılarını sıfır zorlar; ilgisiz eski milestone sidecar'larına dokunmaz. Incoming ad üzerindeki ilk readback bu final-only residue kapısını bilinçli olarak atlar.

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

Issue #71'in fiziksel CPU BuildKit kapanışı da aynı procedure-bound on dört dosyalı native kanıt sözleşmesini ayrı mode ile kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue71
```

Issue #73'ün fiziksel DDR5 DIMM BuildKit kapanışı aynı kanıt setini ayrı teknik commit/tree, test sayıları, hardened politika ve exact r37 memory-module markerıyla kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue73
```

Issue #75'in fiziksel M.2 NVMe Storage BuildKit kapanışı aynı kanıt setini ayrı teknik commit/tree, test sayıları, hardened politika ve exact r38 storage markerıyla kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue75
```

Issue #77'nin fiziksel Processor Cooler BuildKit kapanışı aynı kanıt setini ayrı teknik commit/tree, test sayıları, hardened politika ve exact r39 processor-cooler markerıyla kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue77
```

Issue #79'un fiziksel Graphics Card BuildKit kapanışı da exact teknik commit/tree, `690/690` + `100/100`, `issue79-hardened-v3`, r40 readiness, exact GPU markerı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue79
```

Issue #81'in fiziksel Power Supply BuildKit kapanışı exact teknik commit/tree, `697/697` + `105/105`, `issue81-hardened-v1`, r41 readiness, gerçek post-prerequisite karakter/mouse rotası, exact PSU markerı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue81
```

Issue #83'ün fiziksel ATX24 power-cable BuildKit kapanışı exact teknik commit/tree, `701/701` + `110/110`, `issue83-hardened-v1`, r42 readiness, exact `ModularAtx24SplitPsuToMotherboard` markerı, açık `prerequisite-positioning=teleport-assisted` sınırı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue83
```

Opsiyonel dördüncü argüman verilmezse araç generic `canonical` moda düşer ve yalnız canonical evidence dizininin boş olmayan exact dosya setini zorlar. Issue'ya özel dosya adlarını ve sayısını kilitlemek için mode atlanmamalıdır. `issue66` modu dokuz canonical kanıt adını ve exact `9/9` sayısını; `issue68`, `issue71`, `issue73`, `issue75`, `issue77`, `issue79`, `issue81` ve `issue83` modları `binary-manifest.json`, `build-il2cpp-d3d11.log`, `build-procedure.ps1`, `editmode.xml`, `launch-procedure.ps1`, `macos-build.log`, `macos-runtime.log`, `playmode.xml`, `procedure-manifest.json`, `runtime-d3d11.log`, `runtime-procedure.ps1`, `runtime-summary.json`, `source-receipt.json` ve `task-receipt.json` adlarını ve exact `14/14` sayısını sabit sözleşme olarak doğrular. `issue71`, `issue73`, `issue75`, `issue77`, `issue79`, `issue81` ve `issue83` ayrıca receipt JSON semantiğini, kendi exact technical commit/tree bağını, packaged source/docs commit'inin technical commit soyundan gelmesini, diğer 13 promoted artifact'ın byte/SHA readback'ini, üç procedure manifestini, issue-specific hardened build politikasını, expanded Burst/native-link fatal-token sayısı `0`, exact CPU, DDR5 memory-module, M.2 NVMe storage, processor-cooler, graphics-card, power-supply veya ATX24 power-cable runtime marker sayısı `1`, task cleanup ve residue `0` alanlarını fail-closed zorlar. Technical→source/docs farkı eski modlarda exact dokuz dosyalı closure allowlist'idir; Issue #83'te genişletilmiş ana hedef mimarisi ve yol haritası belgeleriyle exact on bir dosyalı allowlist'tir. Araç, paket içindeki `SOURCE_COMMIT.txt` dosyasının yalnız tam `Source/docs commit:` ve `Source/docs tree:` satırlarından oluşmasını zorunlu kılar. Paket oluşturmaz, dosya temizlemez ve Git/USB/GitHub durumunu değiştirmez.

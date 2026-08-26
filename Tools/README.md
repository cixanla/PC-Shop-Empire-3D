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

Issue #85'in fiziksel EPS12V CPU power-cable BuildKit kapanışı exact teknik commit/tree, `705/705` + `115/115`, `issue85-hardened-v1`, `eps12v-power-cable-build-kit=ready` taşıyan r43 readiness, exact `ModularEps12v8PinPsuToMotherboard` markerı, açık `prerequisite-positioning=teleport-assisted` sınırı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue85
```

Issue #87'nin fiziksel PCIe/GPU 8-pin 6+2 power-cable BuildKit kapanışı exact teknik commit/tree, `709/709` + `116/116`, `issue87-hardened-v1`, `pcie-gpu-power-cable-build-kit=ready` taşıyan r44 readiness, exact `ModularPcie8PinPsuToGraphicsCard` markerı, açık `prerequisite-positioning=teleport-assisted` sınırı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue87
```

Issue #89'un canonical motherboard BuildKit→chassis Assembly kapanışı exact teknik commit/tree, `712/712` + `119/119`, `issue89-hardened-v1`, `motherboard-assembly-handoff=ready` taşıyan r45 readiness, exact pickup→guided-seat→secure→unsecure→detach→reseat markerı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue89
```

Issue #91'in canonical processor BuildKit→Assembly ProcessorSocket/retention kapanışı exact teknik commit/tree, `715/715` + `122/122`, `issue91-hardened-v2`, `processor-assembly-handoff=ready` taşıyan r46 readiness, exact pickup→seat→retain→open→detach→reseat markerı, üç procedure ve Windows task/residue sözleşmesini kilitler:

```bash
./Tools/verify-checkpoint-package.sh \
  /absolute/path/to/Game \
  /absolute/path/to/checkpoint-package \
  /absolute/path/to/canonical-evidence \
  issue91
```

Opsiyonel dördüncü argüman verilmezse araç generic `canonical` moda düşer ve yalnız canonical evidence dizininin boş olmayan exact dosya setini zorlar. Issue'ya özel dosya adlarını ve sayısını kilitlemek için mode atlanmamalıdır. `issue66` modu dokuz canonical kanıt adını ve exact `9/9` sayısını; `issue68`, `issue71`, `issue73`, `issue75`, `issue77`, `issue79`, `issue81`, `issue83`, `issue85`, `issue87`, `issue89` ve `issue91` modları `binary-manifest.json`, `build-il2cpp-d3d11.log`, `build-procedure.ps1`, `editmode.xml`, `launch-procedure.ps1`, `macos-build.log`, `macos-runtime.log`, `playmode.xml`, `procedure-manifest.json`, `runtime-d3d11.log`, `runtime-procedure.ps1`, `runtime-summary.json`, `source-receipt.json` ve `task-receipt.json` adlarını ve exact `14/14` sayısını sabit sözleşme olarak doğrular. `issue71`, `issue73`, `issue75`, `issue77`, `issue79`, `issue81`, `issue83`, `issue85`, `issue87`, `issue89` ve `issue91` ayrıca receipt JSON semantiğini, kendi exact technical commit/tree bağını, packaged source/docs commit'inin technical commit soyundan gelmesini, diğer 13 promoted artifact'ın byte/SHA readback'ini, üç procedure manifestini, issue-specific hardened build politikasını, expanded Burst/native-link fatal-token sayısı `0`, exact CPU, DDR5 memory-module, M.2 NVMe storage, processor-cooler, graphics-card, power-supply, ATX24, EPS12V, PCIe/GPU power-cable, motherboard veya processor Assembly handoff runtime marker sayısı `1`, task cleanup ve residue `0` alanlarını fail-closed zorlar. Issue #89 ve #91 ayrıca `assembly-handoff-flow=failed` markerını kesin forbidden sayar. Technical→source/docs farkı normal modlarda exact dokuz dosyalı closure allowlist'idir; yalnız Issue #83 genişletilmiş ana hedef mimarisi ve yol haritası belgeleri nedeniyle exact on bir dosyalı allowlist kullanır. Araç, paket içindeki `SOURCE_COMMIT.txt` dosyasının yalnız tam `Source/docs commit:` ve `Source/docs tree:` satırlarından oluşmasını zorunlu kılar. Paket oluşturmaz, dosya temizlemez ve Git/USB/GitHub durumunu değiştirmez.

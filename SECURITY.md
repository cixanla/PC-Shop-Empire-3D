# Security Policy

## Private reporting

Güvenlik açığı, credential sızıntısı, save bozulması veya veri gizliliği sorunu public issue/yorumda paylaşılmamalıdır. Repository sahibiyle GitHub'ın private iletişim kanalı veya önceden doğrulanmış özel iletişim yöntemi kullanılmalıdır.

## Repository secrets

- Token, parola, API key, certificate, provisioning profile ve private key commit edilmez.
- Steam, Apple, telemetry ve CI credential'ları yalnız ihtiyaç doğduğunda encrypted repository/environment secrets olarak eklenir.
- `.env`, crash dump ve gerçek kullanıcı raporları kaynak ağacına girmez.
- Secret şüphesinde önce credential revoke/rotate edilir; yalnız Git geçmişinden silmek yeterli sayılmaz.

## Runtime data boundary

Guardian ve tanı sistemleri varsayılan olarak offline çalışır. Online rapor açık opt-in, veri minimizasyonu, pseudonymous kimlik ve yayın öncesi gizlilik incelemesi olmadan etkinleştirilemez.

## Supported state

Proje henüz pre-alpha geliştirme aşamasındadır ve kamuya açık güvenlik desteği taahhüdü yoktur. Buna rağmen kaynak, dependency ve veri işleme sorunları proje risk kaydında izlenir.

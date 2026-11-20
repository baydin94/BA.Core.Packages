# BA.Core.Packages ajan çalışma düzeni

Bu depo, Web API projelerinde kullanılacak .NET paketlerini içerir.
Bu çalışma düzeni tüm depo için geçerlidir.

## İş akışı

Her yeni iş için aşağıdaki listeyi kopyala ve kapsama göre uyarla.
Bir adım tamamlandığında ilgili kutuyu `[x]` olarak işaretle.

### 1. Kapsam ve tasarım

- [ ] Şef hedefi ve işin hangi koşullarda tamamlanacağını yazsın.
- [ ] Mimari ajanı etkilenen katmanları ve sözleşmeleri belirlesin.
- [ ] Mimari ajanı öneriyi bir Web API örneğiyle açıklasın.
- [ ] Kullanıcıyla açık tasarım kararlarını netleştir.

Kullanıcı önce mimariyi görüşmek istiyorsa bu aşamada kal.
Uygulamaya kullanıcı istediğinde geç.

Kod ve Markdown taslaklarını sohbetin çalışma kopyasında hazırla.
Kullanıcı taslağı Değişiklikler panelinden inceler.
Ana depoya ancak kullanıcı incelediği değişikliği onayladığında aktar.

### 2. İş dağıtımı ve geliştirme

- [ ] Şef gereken rolleri seçip her birine beklenen çıktıyı versin.
- [ ] Şef görev sırasını ve düzenlenecek dosyaların sahiplerini belirlesin.
- [ ] Görev alan ajanlar kararlaştırılan değişiklikleri uygulasın.

Küçük bir işi tek ajan üstlenebilir. Bağımsız işler paralel yürüyebilir.
Bir dosyayı aynı anda tek ajan düzenler.

### 3. Doğrulama ve teslim

- [ ] Test ajanı kabul ölçütlerini kontrol edip sonuçları kaydetsin.
- [ ] Güvenlik incelemesi gerekiyorsa security ajanı bulgularını bildirsin.
- [ ] Şef bulguları değerlendirip değişiklikleri birleştirsin.
- [ ] Şef sonucu tüketen Web API açısından kontrol etsin.
- [ ] Şef teslimde değişikliği, kontrolleri ve açık noktaları özetlesin.

## Ajanların görevleri

- **Şef** kapsamı belirler, işleri dağıtır ve teslimi yönetir.
- **Mimari** katmanları, bağımlılık yönünü ve paket sözleşmelerini tasarlar.
  Kararların gerekçesini ve Web API'deki kullanımını açıklar.
- **Backend** sözleşmeleri, pipeline davranışlarını ve adaptörleri geliştirir.
  Servis kayıtlarını yapar ve genel API değişikliklerinin etkisini bildirir.
- **Test** kabul ölçütlerinden senaryolar çıkarır ve kontrolleri çalıştırır.
  Doğrulanamayan noktaları bildirir.
- **Security** kimlik ve tenant sınırlarını, sırları, logları ve girdileri
  inceler. Etkilenen kodu, bulgunun etkisini ve düzeltme önerisini bildirir.

Test ve security ajanları inceleme de yapabilir. `Core.Tests` ve
`Core.Security` projeleri kullanıcı yeniden istediğinde eklenir.

## Katman sınırları

- **Core.Abstractions** ortak imzaları, arayüzleri ve sonuç tiplerini taşır.
  Tüketici kod bağımlılığını bu sözleşmelere kurar.
- **Core.Application** uygulama akışını ve MediatR pipeline'ını kurar.
  Doğrulama, transaction ve cache davranışları sözleşmeleri kullanır.
- **Core.CrossCuttingConcerns** katmanları dikine kesen işleri üstlenir.
  Hedef kapsamı önbellekleme, hata yönetimi, loglama ve izlemedir.
  Problem Details hata yönetimine, Serilog loglamaya,
  OpenTelemetry ise izlemeye örnektir.
- **Redis, Mongo ve SQL paketleri** sağlayıcıya özgü tipleri, bağlantıları,
  veri erişimini ve kendi servis kayıtlarını yönetir.
- **Tüketen Web API** paketleri seçer. `appsettings.json` ve ortama özel
  yapılandırmadan ilgili bölümü paketin servis kayıt metoduna geçirir.
  Paket değerleri tipli ayarlara bağlar ve doğrular. Bağlantı sırları
  Web API'nin güvenli yapılandırma kaynağından gelir.

## Redis iş listesi

- [x] README'de tüketici için servis kaydını ve sorgu kullanımını göster.
- [x] Cache sözleşmesini, tenant kapsamını ve anahtar yapısını netleştir.
- [x] Süre dolumu ve grup geçersiz kılma davranışlarını belirle.
- [x] Tüketicinin JSON ayarlarını Redis paketinin README dosyasında göster.
- [x] Redis adaptöründe okuma, yazma ve silme işlemlerini tamamla.
- [x] Cache davranışını Core.Application pipeline'ına bağla.
- [x] Okuma, süre dolumu, tenant ayrımı ve geçersiz kılmayı test et.

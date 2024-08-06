# Dynamic Configuration Management Library

## Proje Amacı

Bu projenin amacı, web.config ve app.config gibi dosyalarda tutulan appkey’lerin ortak ve dinamik bir yapıyla erişilebilir olmasını sağlamak ve deployment veya restart, recycle gerektirmeden güncellemelerin yapılabilmesini sağlamaktır. Konfigürasyon kayıtları çeşitli storage sistemlerinde (MsSql, Redis, Mongo, File vs.) saklanabilir.

## Özellikler

- Dinamik konfigürasyon yönetimi
- Periyodik kontrol ve güncelleme
- Konfigürasyon verilerinin farklı tiplerde saklanması (integer, string, double, boolean)
- Sadece aktif olan (IsActive=1) kayıtların kullanılması
- Maksimum üç parametre ile initialize edilebilme
- Storage erişim problemlerinde son başarılı konfigürasyonlarla çalışma
- .NET 8 ile geliştirilmiş
- Docker Compose ile tüm ekosistemin çalıştırılabilmesi

## Gereksinimler

- .NET 8 SDK
- Docker ve Docker Compose

## Kurulum

### Adım 1: Depoyu Klonlayın

```bash
git clone https://github.com/kullaniciadi/proje-adi.git
cd proje-adi
```

### Adım 2: Docker Compose ile Servisleri Başlatın

```bash
docker-compose up --build
```

### Adım 3: Servisi Çalıştırın

Docker Compose komutu MongoDB'yi ve `ServiceA` servisini başlatacaktır.

## Kullanım

### API Endpoints

#### GET /config/{key}

Belirtilen anahtar için konfigürasyon değerini döner.

```bash
curl http://localhost:5000/config/your-key
```

## Proje Yapısı

- **ConfigLibrary**: Konfigürasyon yönetimini sağlayan kütüphane.
- **ConfigStorage**: Konfigürasyon kayıtlarını saklayan depolama sistemi (MongoDB).
- **ServiceA**: Konfigürasyon kütüphanesini kullanan örnek servis.

## ConfigLibrary

ConfigLibrary, çeşitli tiplerdeki konfigürasyon kayıtlarını dinamik olarak yükler ve erişilebilir hale getirir.

```csharp
public class ConfigurationManager
{
    public ConfigurationManager(string serviceName, TimeSpan checkInterval, Func<Task<Dictionary<string, string>>> loadConfigurations);

    public T GetConfiguration<T>(string key);
    public void Stop();
}
```

## ConfigStorage

MongoDB'de konfigürasyon kayıtlarını saklayan ve yükleyen sınıf.

```csharp
public class MongoConfigStorage
{
    public MongoConfigStorage(string connectionString, string databaseName, string collectionName);
    public Task<Dictionary<string, string>> LoadConfigurationsAsync(string serviceName);
}
```

## ServiceA

ServiceA, ConfigLibrary'yi kullanarak konfigürasyon kayıtlarını API üzerinden erişilebilir hale getirir.

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(provider => new ConfigurationManager("SERVICE-A", TimeSpan.FromSeconds(30), () => storage.LoadConfigurationsAsync("SERVICE-A")));
var app = builder.Build();
app.MapGet("/config/{key}", (string key, ConfigurationManager configManager) => { var value = configManager.GetConfiguration<string>(key); return value is not null ? Results.Ok(value) : Results.NotFound(); });
app.Run();
```

## Gelişmiş Özellikler ve Ekstralar

- **Message Broker Kullanımı**: RabbitMQ gibi bir message broker kullanarak konfigürasyon değişikliklerini anlık olarak güncelleyebilirsiniz.
- **TPL ve async/await Kullanımı**: Asenkron programlama teknikleri kullanılmıştır.
- **Concurrency Problemlerinin Engellenmesi**: ConcurrentDictionary gibi yapılar kullanılarak çözümler sunulmuştur.
- **Design & Architectural Patterns**: Singleton, Dependency Injection ve Repository Pattern gibi tasarım kalıpları kullanılmıştır.
- **Unit Testler ve TDD**: Proje, xUnit veya NUnit ile yazılmış unit testler içerebilir.
- **Storage Olarak MongoDB Kullanımı**: Proje, MongoDB'yi konfigürasyon kayıtlarını saklamak için kullanır.
- **Docker Compose ile Çalıştırılabilirlik**: Proje, Docker Compose ile kolayca çalıştırılabilir.

## Katkıda Bulunma

1. Bu depoyu fork edin.
2. Yeni bir branch oluşturun (`git checkout -b feature-isim`).
3. Değişikliklerinizi commit edin (`git commit -am 'Yeni bir özellik ekle'`).
4. Branch'e push edin (`git push origin feature-isim`).
5. Bir Pull Request açın.

## Lisans

Bu proje MIT lisansı ile lisanslanmıştır. Detaylar için LICENSE dosyasına bakabilirsiniz.

---

Bu README.md dosyası, projenizin ne yaptığı, nasıl çalıştırıldığı ve nasıl kullanılacağı konusunda ayrıntılı bilgi sağlar. İlgili bağlantıları ve açıklamaları ihtiyaçlarınıza göre özelleştirebilirsiniz.

# SlqStudio

Учебная веб-платформа для практики SQL: авторизация через Moodle, выдача персонального варианта заданий, проверка SQL-решений, формирование отчетов и администрирование учебного контента.

## Возможности системы

- Авторизация через Moodle API (email + выбранный курс), JWT хранится в cookie.
- Разграничение доступа по ролям Moodle (`editingteacher` для части административных функций).
- CRUD для курсов, лабораторных работ и заданий (MediatR + CQRS).
- Генерация персонального варианта заданий для студента.
- SQL-тренажер:
  - проверка синтаксиса SQL;
  - сравнение результата с эталонным SQL;
  - отдельная обработка сценариев с `CREATE TRIGGER`.
- Формирование отчета по выполнению (HTML + PDF) и отправка по email.
- Просмотр, скачивание и удаление лог-файлов через веб-интерфейс.
- Веб-редактор `appsettings.json` (вход по `ConfigUser`).

## Архитектура решения

- `SlqStudio.Web`  
  ASP.NET Core MVC: контроллеры, представления, middleware, auth, UI.
- `SlqStudio.Application`  
  Бизнес-логика: CQRS-обработчики, SQL-проверка, Moodle-клиент, генерация отчетов, email, JWT, сервисы.
- `SlqStudio.Persistence`  
  EF Core модели и `ApplicationDbContext`.

Связи доменной модели:

- `Course` 1..* `LabWork`
- `LabWork` 1..* `LabTask`

## Технологии

- .NET 10 / ASP.NET Core MVC
- Entity Framework Core + Pomelo MySQL
- MediatR (CQRS)
- Dapper + Microsoft.Data.SqlClient
- JWT Bearer + Session + Memory Cache
- iText7 (PDF)
- Materialize CSS, CodeMirror, Mermaid

## Конфигурация

Основные секции `SlqStudio.Web/appsettings.json`:

- `ConnectionStrings.MySQL` - БД приложения (курсы/работы/задания).
- `ConnectionStrings.LabsConnection` - SQL Server БД для проверки SQL-решений.
- `MoodleApi` - URL Moodle REST API и токен.
- `Jwt` - параметры подписи и валидации JWT.
- `SmtpSettings` - SMTP-параметры отправки отчетов.
- `Logging.File` - путь и параметры ротации логов.
- `DiagramSettings.Name` - имя файла диаграммы в `wwwroot/diagrams`.
- `ConfigUser` - логин/пароль для страницы редактирования конфигурации.

## Локальный запуск

1. Установить `.NET SDK 10`.
2. Поднять MySQL и SQL Server.
3. Настроить `SlqStudio.Web/appsettings.json`.
4. Выполнить:

```powershell
dotnet restore SlqStudio.sln
dotnet build SlqStudio.sln
dotnet run --project SlqStudio.Web/SlqStudio.Web.csproj
```

По умолчанию приложение доступно на `http://0.0.0.0:5000`.

## Развертывание системы (Production)

### 1. Подготовьте окружение

- Установите `.NET 10 Runtime` (или SDK, если будете собирать на сервере).
- Поднимите MySQL (БД приложения) и SQL Server (учебная БД для SQL-проверок).
- Настройте Moodle Web Service и получите токен для REST API.
- Подготовьте SMTP-аккаунт для отправки отчетов.

### 2. Подготовьте конфигурацию

Рекомендуется не хранить секреты в `appsettings.json`, а передавать их через переменные окружения:

```powershell
# Пример для Windows PowerShell
$env:ConnectionStrings__MySQL="Server=...;Database=sqlstudio;User=...;Password=...;"
$env:ConnectionStrings__LabsConnection="Server=...;Database=...;Trusted_Connection=True;TrustServerCertificate=True"
$env:Jwt__SecretKey="..."
$env:Jwt__Issuer="..."
$env:Jwt__Audience="..."
$env:MoodleApi__MoodleUrl="https://moodle.example.com/webservice/rest/server.php"
$env:MoodleApi__Token="..."
$env:SmtpSettings__Server="smtp.example.com"
$env:SmtpSettings__Port="587"
$env:SmtpSettings__SenderEmail="noreply@example.com"
$env:SmtpSettings__SenderName="SlqStudio"
$env:SmtpSettings__Password="..."
$env:SmtpSettings__EnableSsl="true"
```

### 3. Инициализируйте БД приложения (MySQL)

В проекте используются EF Core модели `Course`, `LabWork`, `LabTask`.  
Если миграций в репозитории нет, создайте базовую миграцию и примените ее:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate `
  --project SlqStudio.Persistence/SlqStudio.Persistence.csproj `
  --startup-project SlqStudio.Web/SlqStudio.Web.csproj `
  --output-dir Migrations
dotnet ef database update `
  --project SlqStudio.Persistence/SlqStudio.Persistence.csproj `
  --startup-project SlqStudio.Web/SlqStudio.Web.csproj
```

Примечание: для команд EF Core MySQL должен быть доступен, так как используется `ServerVersion.AutoDetect(...)`.

### 4. Добавьте стартовые данные

Для входа пользователь выбирает курс из БД приложения.  
После инициализации создайте минимум один курс (через БД или админ-интерфейс), иначе вход будет недоступен.

### 5. Соберите и опубликуйте приложение

```powershell
dotnet restore SlqStudio.sln
dotnet publish SlqStudio.Web/SlqStudio.Web.csproj -c Release -o ./publish
```

### 6. Запустите сервис

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet .\publish\SlqStudio.Web.dll
```

По умолчанию production-конфигурация слушает `http://0.0.0.0:5000` (см. `SlqStudio.Web/appsettings.Production.json`).

### 7. Рекомендуемая эксплуатационная схема

- Размещать приложение за reverse proxy (Nginx/IIS/Apache).
- Включить HTTPS на внешнем контуре.
- Выдать процессу приложения права на запись в директорию логов (`Logging.File.Path`).
- Настроить ротацию и мониторинг логов.

## Проверка после развертывания

- Страница логина открывается: `/Auth/Login`.
- Авторизация Moodle проходит успешно.
- SQL-проверка и сравнение запросов работают.
- Генерация PDF-отчета работает.
- Отправка email работает.
- Логи пишутся в указанный путь.

## Рекомендации по безопасности

- Не храните реальные токены и пароли в Git-репозитории.
- Для production установите `Secure=true` для JWT cookie и используйте HTTPS.
- Ограничьте доступ к странице конфигурации (`/Config/*`) на уровне сети или reverse proxy.
- Используйте отдельные сервисные учетные записи для MySQL, SQL Server, Moodle API и SMTP.

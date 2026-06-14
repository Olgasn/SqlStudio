# SlqStudio

[![Build and Test](https://github.com/Olgasn/SqlStudio/actions/workflows/ci.yml/badge.svg)](https://github.com/Olgasn/SqlStudio/actions/workflows/ci.yml)

`SlqStudio` представляет собой учебную веб-платформу, предназначенную для практической отработки навыков работы с языком SQL. Система обеспечивает авторизацию пользователей через систему дистанционного обучения Moodle, выдачу персонального варианта заданий, проверку предложенных SQL-решений, формирование итоговых отчётов, а также администрирование учебного содержания.

## Возможности системы

- Система выполняет авторизацию пользователя через REST API системы Moodle на основании адреса электронной почты и выбранного курса; по результатам успешной проверки выдаётся токен JWT, который сохраняется в cookie.
- Разграничение доступа осуществляется на основании ролей Moodle: ряд административных функций доступен только пользователям с ролью `editingteacher`.
- Система предоставляет полный набор операций создания, чтения, изменения и удаления для курсов, лабораторных работ и заданий; соответствующая логика реализована с применением библиотеки MediatR и шаблона CQRS.
- Для каждого студента формируется индивидуальный вариант заданий.
- Тренажёр SQL обеспечивает проверку синтаксиса запроса, сравнение результата выполнения студенческого запроса с результатом эталонного запроса, а также отдельную обработку сценариев, содержащих инструкцию `CREATE TRIGGER`.
- По итогам выполнения работ формируется отчёт в форматах HTML и PDF, который дополнительно направляется адресату по электронной почте.
- Пользователям с административными правами доступны просмотр, скачивание и удаление файлов журналов через веб-интерфейс.
- Предусмотрен веб-редактор файла конфигурации `appsettings.json`, доступ к которому предоставляется после отдельной авторизации по учётным данным секции `ConfigUser`.

## Архитектура решения

Решение состоит из четырёх проектов, организованных по слоям; зависимости направлены внутрь, от веб-слоя к слою хранения данных (`Web → Application → Persistence`):

- `SlqStudio.Web` — веб-приложение на платформе ASP.NET Core MVC: контроллеры, представления, компоненты промежуточного программного обеспечения, механизмы аутентификации, ведение журналов и настройка внедрения зависимостей.
- `SlqStudio.Application` — слой бизнес-логики: обработчики CQRS, проверка SQL-запросов, клиент Moodle, формирование отчётов, отправка электронной почты, работа с токенами JWT и генерация вариантов заданий.
- `SlqStudio.Persistence` — модели предметной области и контекст `ApplicationDbContext` на основе Entity Framework Core.
- `SlqStudio.Tests` — модульные тесты, реализованные с использованием платформы xUnit.

Связи доменной модели имеют следующий вид:

- один курс (`Course`) содержит одну и более лабораторных работ (`LabWork`);
- одна лабораторная работа (`LabWork`) содержит одно и более заданий (`LabTask`).

### Использование двух баз данных

Принципиальной особенностью архитектуры является взаимодействие приложения с двумя независимыми базами данных, предназначенными для различных целей:

- База данных MySQL (строка подключения `ConnectionStrings:MySQL`) хранит учебное содержание, а именно курсы, лабораторные работы и задания. Доступ к ней осуществляется через Entity Framework Core, при этом операции выполняются посредством обработчиков MediatR.
- База данных SQL Server (строка подключения `ConnectionStrings:LabsConnection`) выступает в роли изолированной среды, в которой фактически выполняются студенческие и эталонные SQL-запросы. Доступ к ней осуществляется с применением библиотеки Dapper и компонента `Microsoft.Data.SqlClient`. Все запросы выполняются в рамках транзакции, которая всегда отменяется, благодаря чему содержимое этой базы данных остаётся неизменным.

## Технологии

- Платформа .NET 10 и веб-каркас ASP.NET Core MVC.
- Entity Framework Core совместно с провайдером Pomelo для MySQL.
- Библиотека MediatR, реализующая шаблон CQRS.
- Библиотека Dapper совместно с компонентом Microsoft.Data.SqlClient.
- Аутентификация на основе JWT Bearer, механизм сессий и кэш в оперативной памяти.
- Библиотека iText7 для формирования документов в формате PDF.
- Клиентские библиотеки Materialize CSS, CodeMirror и Mermaid.
- Платформа xUnit для модульного тестирования.

## Конфигурация

Файл `SlqStudio.Web/appsettings.json` содержит все параметры приложения. Секретные значения (пароли, ключи, токены) **не следует хранить в репозитории** — передавайте их через переменные окружения с разделителем `__` (например, `ConnectionStrings__MySQL`). Значения в файле предназначены только для локальной разработки.

### ConnectionStrings

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `MySQL` | Строка подключения к MySQL-базе приложения (курсы, лабораторные работы, задания). Используется Entity Framework Core при запуске и при выполнении команд `dotnet ef`. | Администратор/DevOps перед первым запуском. |
| `LabsConnection` | Строка подключения к SQL Server — изолированной базе данных, в которой фактически выполняются студенческие и эталонные запросы. Все запросы выполняются в транзакциях, которые всегда откатываются, поэтому данные в этой базе не изменяются. | Администратор/DevOps перед первым запуском. |

```json
"ConnectionStrings": {
  "MySQL": "Server=localhost;Database=sqlstudio;User=root;Password=root;Allow User Variables=true;",
  "LabsConnection": "Server=MYSERVER;Database=StudentsDB;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### Jwt

Параметры формирования и проверки JWT-токенов. Токен хранится в HttpOnly-cookie с именем `jwt` и используется для аутентификации всех запросов к приложению.

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `SecretKey` | Симметричный ключ подписи токенов. Длина должна быть не менее 32 символов (256 бит). Хранить только через переменную окружения. | Администратор при первоначальной настройке. |
| `Issuer` | Значение поля `iss` (издатель) в JWT. Произвольная строка, например имя приложения или домен. | Администратор при первоначальной настройке. |
| `Audience` | Значение поля `aud` (получатель) в JWT. Произвольная строка, как правило совпадает с `Issuer` или указывает на целевых клиентов. | Администратор при первоначальной настройке. |

```json
"Jwt": {
  "SecretKey": "your-very-long-secret-key-at-least-32-chars",
  "Issuer": "slqstudio",
  "Audience": "slqstudio-users"
}
```

> **Важно:** При смене `SecretKey` все ранее выданные токены становятся недействительными и все пользователи будут выведены из системы.

### MoodleApi

Параметры интеграции с REST API системы Moodle. Подробное руководство по настройке — в документе [moodle-setup.md](moodle-setup.md).

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `MoodleUrl` | Полный URL точки входа REST API Moodle. Обязательно должен оканчиваться на `/webservice/rest/server.php`. | Администратор при подключении к Moodle. |
| `Token` | Токен доступа к веб-сервису Moodle. Создаётся в интерфейсе Moodle для служебной учётной записи. | Администратор Moodle при первоначальной настройке. |

```json
"MoodleApi": {
  "MoodleUrl": "https://moodle.example.com/webservice/rest/server.php",
  "Token": "your-moodle-webservice-token"
}
```

### SmtpSettings

Параметры почтового сервера для отправки итоговых отчётов студентам.

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `Server` | Адрес SMTP-сервера, например `smtp.gmail.com`. | Администратор при настройке почты. |
| `Port` | Порт SMTP-сервера. Типичные значения: `587` (STARTTLS), `465` (SSL), `25` (без шифрования). | Администратор при настройке почты. |
| `SenderEmail` | Адрес электронной почты отправителя (поле «От»). | Администратор при настройке почты. |
| `SenderName` | Отображаемое имя отправителя (поле «От»). | Администратор при настройке почты. |
| `Password` | Пароль учётной записи SMTP. Для Gmail рекомендуется пароль приложения, а не пароль аккаунта. Хранить только через переменную окружения. | Администратор при настройке почты. |
| `EnableSsl` | Включить шифрование: `true` или `false`. Для портов `587` и `465` должно быть `true`. | Администратор при настройке почты. |

```json
"SmtpSettings": {
  "Server": "smtp.gmail.com",
  "Port": "587",
  "SenderEmail": "noreply@example.com",
  "SenderName": "SlqStudio",
  "Password": "your-smtp-password",
  "EnableSsl": "true"
}
```

### Logging

Параметры журналирования. Логи записываются одновременно в файл и в консоль.

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `LogLevel.Default` | Минимальный уровень журналирования для всех компонентов. Допустимые значения: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`. | Разработчик или администратор по необходимости. |
| `LogLevel.Microsoft.AspNetCore.Authentication` | Уровень журналирования для событий аутентификации ASP.NET Core. Полезно повысить до `Debug` при отладке проблем со входом. | Разработчик при отладке. |
| `File.Path` | Путь к файлу журнала относительно рабочего каталога приложения. | Администратор перед первым запуском. Процесс должен иметь право на запись в указанный каталог. |
| `File.MaxFileSizeMB` | Максимальный размер одного файла журнала в мегабайтах. При превышении создаётся новый файл. | Администратор при настройке ротации журналов. |
| `File.MaxFiles` | Максимальное количество хранимых файлов журнала. Старые файлы удаляются автоматически. | Администратор при настройке ротации журналов. |

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore.Authentication": "Information"
  },
  "File": {
    "Path": "logs/myapp.log",
    "MaxFileSizeMB": "50",
    "MaxFiles": "7"
  }
}
```

### DiagramSettings

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `Name` | Имя файла ER-диаграммы учебной базы данных (SQL Server), расположенного в каталоге `wwwroot/diagrams/`. Диаграмма отображается студентам как справочная схема БД при выполнении заданий. | Администратор при добавлении новой учебной базы данных или замене диаграммы. |

```json
"DiagramSettings": {
  "Name": "students_payments_er_diagram.txt"
}
```

### ConfigUser

Учётные данные для отдельной страницы настройки приложения (`/Config/*`). Этот механизм аутентификации **независим** от Moodle и JWT — он основан только на флаге сессии и используется исключительно для защиты веб-редактора файла `appsettings.json`.

| Параметр | Назначение | Кто и когда задаёт |
|---|---|---|
| `Name` | Имя пользователя для входа на страницу конфигурации. | Администратор при первоначальной настройке. |
| `Password` | Пароль для входа на страницу конфигурации. Хранить только через переменную окружения. | Администратор при первоначальной настройке. |

```json
"ConfigUser": {
  "Name": "admin",
  "Password": "strong-password"
}
```

> **Важно:** Ограничьте доступ к маршруту `/Config/*` на уровне обратного прокси-сервера или сети — через эту страницу можно изменить любые параметры приложения.

## Управление секретами (пароли и токены)

В файле `appsettings.json` секретные поля оставлены пустыми — файл безопасен для коммита в репозиторий. Реальные значения передаются одним из двух способов в зависимости от окружения.

### Локальная разработка — ASP.NET Core User Secrets

User Secrets хранятся вне репозитория в профиле пользователя (`%APPDATA%\Microsoft\UserSecrets\`) и автоматически применяются поверх `appsettings.json` в режиме `Development`:

```powershell
# Один раз: инициализация хранилища (если UserSecretsId ещё не добавлен в .csproj)
dotnet user-secrets init --project SlqStudio.Web/SlqStudio.Web.csproj

# Установка секретных значений
dotnet user-secrets set "ConnectionStrings:MySQL" "Server=localhost;Database=sqlstudio;User=root;Password=ВАШ_ПАРОЛЬ;Allow User Variables=true;" --project SlqStudio.Web/SlqStudio.Web.csproj
dotnet user-secrets set "Jwt:SecretKey" "ВАШ_СЕКРЕТНЫЙ_КЛЮЧ_МИНИМУМ_32_СИМВОЛА" --project SlqStudio.Web/SlqStudio.Web.csproj
dotnet user-secrets set "MoodleApi:Token" "ВАШ_ТОКЕН_MOODLE" --project SlqStudio.Web/SlqStudio.Web.csproj
dotnet user-secrets set "SmtpSettings:Password" "ВАШ_SMTP_ПАРОЛЬ" --project SlqStudio.Web/SlqStudio.Web.csproj
dotnet user-secrets set "ConfigUser:Password" "ВАШ_ПАРОЛЬ_КОНФИГУРАЦИИ" --project SlqStudio.Web/SlqStudio.Web.csproj

# Просмотр сохранённых секретов
dotnet user-secrets list --project SlqStudio.Web/SlqStudio.Web.csproj
```

### Продакшн — переменные окружения

В производственной среде передавайте секреты через переменные окружения с разделителем `__` (ASP.NET Core автоматически преобразует их в иерархические ключи конфигурации):

| Секрет | Переменная окружения |
|--------|---------------------|
| Строка подключения MySQL | `ConnectionStrings__MySQL` |
| Ключ подписи JWT | `Jwt__SecretKey` |
| Токен Moodle | `MoodleApi__Token` |
| Пароль SMTP | `SmtpSettings__Password` |
| Пароль страницы Config | `ConfigUser__Password` |

```powershell
# Пример установки для Windows PowerShell / IIS
$env:ConnectionStrings__MySQL = "Server=...;Database=sqlstudio;User=...;Password=...;Allow User Variables=true;"
$env:Jwt__SecretKey           = "ВАШ_СЕКРЕТНЫЙ_КЛЮЧ_МИНИМУМ_32_СИМВОЛА"
$env:MoodleApi__Token         = "ВАШ_ТОКЕН_MOODLE"
$env:SmtpSettings__Password   = "ВАШ_SMTP_ПАРОЛЬ"
$env:ConfigUser__Password     = "ВАШ_ПАРОЛЬ_КОНФИГУРАЦИИ"
```

> **Правило:** Никогда не коммитьте реальные пароли и токены в репозиторий. `appsettings.json` содержит только структуру и несекретные значения по умолчанию.

## Настройка интеграции с Moodle

Полное руководство по настройке веб-сервиса Moodle, получению токена доступа и проверке подключения приведено в отдельном документе: [moodle-setup.md](moodle-setup.md).

## Локальный запуск

### 1. Предварительные требования

- Установите `.NET SDK 10`.
- Обеспечьте доступность серверов MySQL и SQL Server.

### 2. Настройка секретов

Задайте секреты через User Secrets (подробнее — в разделе [Управление секретами](#управление-секретами-пароли-и-токены)). Несекретные параметры (URL Moodle, адрес SMTP-сервера и т. п.) при необходимости скорректируйте в `SlqStudio.Web/appsettings.json`.

### 3. Инициализация базы данных

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate `
  --project SlqStudio.Persistence/SlqStudio.Persistence.csproj `
  --startup-project SlqStudio.Web/SlqStudio.Web.csproj --output-dir Migrations
dotnet ef database update `
  --project SlqStudio.Persistence/SlqStudio.Persistence.csproj `
  --startup-project SlqStudio.Web/SlqStudio.Web.csproj
```

### 4. Сборка и запуск

```powershell
dotnet restore SlqStudio.sln
dotnet build SlqStudio.sln
dotnet run --project SlqStudio.Web/SlqStudio.Web.csproj
```

По умолчанию приложение доступно по адресу `http://0.0.0.0:5000`.

### 5. Первоначальная настройка (только при первом запуске)

После первого запуска база данных пуста — курсов нет. Без курсов вход через `/Auth/Login` невозможен. Создайте первый курс через страницу конфигурации:

1. Откройте `/Config/Login` и войдите под учётными данными `ConfigUser` (`Name` / `Password` из `appsettings.json` или User Secrets).
2. В открывшемся интерфейсе конфигурации нажмите кнопку **Управление курсами**.
3. Нажмите **Добавить курс** и введите название — оно должно точно совпадать с названием курса в Moodle.
4. После создания курса вход через `/Auth/Login` становится доступен.

## Тестирование

Модульные тесты реализованы с использованием платформы xUnit и расположены в проекте `SlqStudio.Tests`. Для запуска полного набора тестов выполните следующую команду:

```powershell
dotnet test SlqStudio.sln
```

Для запуска отдельного класса или метода тестов воспользуйтесь фильтрацией:

```powershell
dotnet test SlqStudio.sln --filter "FullyQualifiedName~SqlManagerTests"
dotnet test SlqStudio.sln --filter "DisplayName~GetSqlOperationAndTable"
```

Непрерывная интеграция настроена в файле `.github/workflows/ci.yml` и последовательно выполняет восстановление зависимостей, сборку в конфигурации Release и запуск тестов на платформе .NET 10.

## Развёртывание системы (Production)

### 1. Подготовка окружения

- Установите среду выполнения `.NET 10 Runtime` либо комплект разработчика SDK, если сборка будет выполняться непосредственно на сервере.
- Разверните серверы MySQL (база данных приложения) и SQL Server (учебная база данных для проверки SQL-запросов).
- Настройте Web Service системы Moodle и получите токен доступа к REST API.
- Подготовьте учётную запись SMTP для отправки отчётов.

### 2. Подготовка конфигурации

Хранить секретные значения в файле `appsettings.json` не рекомендуется; предпочтительным способом является их передача через переменные окружения:

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

### 3. Инициализация базы данных приложения (MySQL)

В проекте применяются модели Entity Framework Core `Course`, `LabWork` и `LabTask`. Поскольку миграции не включены в состав репозитория, необходимо создать первоначальную миграцию и применить её:

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

Следует учитывать, что для выполнения команд Entity Framework Core сервер MySQL должен быть доступен, поскольку используется метод `ServerVersion.AutoDetect(...)`.

### 4. Внесение начальных данных

При входе в систему пользователь выбирает курс из базы данных приложения. После инициализации базы данных необходимо создать как минимум один курс, иначе вход через `/Auth/Login` будет невозможен.

Создание первого курса выполняется через страницу конфигурации (она использует отдельную сессионную авторизацию и не зависит от JWT и Moodle):

1. Откройте `/Config/Login` и войдите под учётными данными `ConfigUser`.
2. В интерфейсе конфигурации нажмите кнопку **Управление курсами**.
3. Нажмите **Добавить курс** и укажите название — оно должно точно совпадать с названием курса в Moodle.
4. После этого обычный вход через `/Auth/Login` становится доступен.

В дальнейшем курсами также можно управлять из административного интерфейса (`CoursesController`) при наличии роли `editingteacher`.

### 5. Сборка и публикация приложения

```powershell
dotnet restore SlqStudio.sln
dotnet publish SlqStudio.Web/SlqStudio.Web.csproj -c Release -o ./publish
```

### 6. Запуск сервиса

```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet .\publish\SlqStudio.Web.dll
```

По умолчанию конфигурация Production принимает соединения по адресу `http://0.0.0.0:5000` (см. файл `SlqStudio.Web/appsettings.Production.json`).

### 7. Рекомендуемая схема эксплуатации

- Размещайте приложение за обратным прокси-сервером (Nginx, IIS или Apache).
- Обеспечьте использование протокола HTTPS на внешнем контуре.
- Предоставьте процессу приложения права на запись в каталог журналов (`Logging.File.Path`).
- Настройте ротацию журналов и их мониторинг.

## Проверка после развёртывания

- Страница входа открывается по адресу `/Auth/Login`.
- Авторизация через Moodle завершается успешно.
- Проверка и сравнение SQL-запросов выполняются корректно.
- Формирование отчёта в формате PDF выполняется корректно.
- Отправка электронной почты выполняется корректно.
- Записи журналов сохраняются по указанному пути.

## Рекомендации по обеспечению безопасности

- Не храните действующие токены и пароли в репозитории Git.
- В среде Production установите атрибут `Secure=true` для cookie с токеном JWT и используйте протокол HTTPS.
- Ограничьте доступ к странице конфигурации (`/Config/*`) на уровне сети или обратного прокси-сервера.
- Используйте отдельные служебные учётные записи для доступа к MySQL, SQL Server, API системы Moodle и серверу SMTP.

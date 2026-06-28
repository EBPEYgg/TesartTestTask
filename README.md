# TesartTestTask

Настольное приложение для мониторинга измерительных устройств, разработанное на платформе .NET 8 с использованием WPF и архитектуры MVVM.

<img width="1186" height="693" alt="image" src="https://github.com/user-attachments/assets/76ed2d0a-13dd-417e-bfde-9d68fee4ad0e" />

<img width="859" height="249" alt="image" src="https://github.com/user-attachments/assets/d290b524-0d12-4d2f-a0dc-3503ef8e0148" />


## Возможности

- отображение списка измерительных устройств;
- асинхронный опрос виртуальных устройств;
- отображение истории измерений выбранного устройства;
- сохранение истории измерений в базе данных;
- экспорт истории измерений;
- обработка ошибок устройств;
- управление опросом (старт, пауза, продолжение, остановка);
- фильтрация устройств по типу и статусу;
- поиск устройств по наименованию;
- корректное завершение фоновых операций.

---

# Используемые технологии

- C#
- .NET 8
- WPF
- Prism
- Entity Framework Core
- SQLite
- xUnit
- Moq
- FluentAssertions

---

# Сборка проекта

## Требования

- .NET SDK 8.0
- Visual Studio 2022 (17.8 и выше)

## Сборка

Открыть решение:

```text
TesartTestTask.sln
```

Собрать проект:

```bash
dotnet build
```

или воспользоваться пунктом меню:

```
Build → Build Solution
```

---

# Запуск приложения

Запустить проект:

```text
TesartTestTask.Presentation
```

или выполнить команду:

```bash
dotnet run --project TesartTestTask.Presentation
```

При первом запуске база данных SQLite будет создана автоматически.

---

# Запуск тестов

Все тесты находятся в проекте:

```text
TesartTestTask.Tests
```

Запуск тестов:

```bash
dotnet test
```

или через **Test Explorer** Visual Studio.

---

# Структура проекта

```text
TesartTestTask
│
├── TesartTestTask.Application
│   ├── DTO
│   ├── Interfaces
│   ├── Services
│
├── TesartTestTask.Domain
│   ├── Devices
│   ├── Entities
│   ├── Enums
│   └── Results
│
├── TesartTestTask.Infrastructure
│   ├── Devices
│   ├── Export
│   ├── Persistence
│       ├── Configurations
│       ├── Repositories
│
├── TesartTestTask.Presentation
│   ├── Behaviors
│   ├── Converters
│   ├── Interfaces
│   ├── Services
│   ├── Styles
│   ├── ViewModels
│   ├── App.xaml
│   └── MainWindow.xaml
│
└── TesartTestTask.Tests
    ├── UnitTests
    └── IntegrationTests
```

---

# Архитектура

Проект разделён на четыре слоя.

**1. Application содержит:**

- DTO;
- сервис опроса устройств;
- сервис экспорта данных;
- интерфейсы репозиториев;
- бизнес-логику приложения.

**2. Domain содержит:**

- доменные сущности;
- перечисления;
- интерфейсы устройств;
- модели результатов измерений.

**3. Infrastructure содержит:**

- реализацию доступа к базе данных;
- DbContext;
- репозитории;
- реализацию виртуальных устройств;
- реализацию экспорта данных.

**4. Presentation содержит:**

- WPF-представления;
- ViewModel;
- команды пользовательского интерфейса;
- конвертеры;
- сервисы пользовательского интерфейса.

---

# Использованные паттерны

- MVVM
- Repository
- Dependency Injection
- Factory
- Command

---

# База данных

В качестве СУБД используется **SQLite**.

## Таблица `Devices`

| Поле | Тип |
|------|-----|
| Id | Guid |
| Name | string |
| DeviceType | int |
| Status | int |
| LastValue | double? |
| LastUpdateTime | DateTime? |
| PollingIntervalMs | int |

## Таблица `Measurements`

| Поле | Тип |
|------|-----|
| Id | Guid |
| DeviceId | Guid |
| Value | double? |
| Timestamp | DateTime |
| IsSuccess | bool |
| ErrorMessage | string? |

База данных автоматически создаётся при первом запуске приложения.

---

# Основные сценарии работы

## Запуск приложения

После запуска приложение загружает список устройств из базы данных.

## Запуск опроса

Кнопка **«Старт»** запускает независимый асинхронный опрос всех устройств.

Во время работы автоматически обновляются:

- статус устройства;
- последнее полученное значение;
- время последнего обновления;
- история измерений.

## Приостановка опроса

Кнопка **«Пауза»** временно приостанавливает выполнение фоновых задач.

Кнопка **«Продолжить»** возобновляет опрос устройств.

## Остановка опроса

Кнопка **«Стоп»** корректно завершает все фоновые операции с использованием `CancellationToken`.

## Просмотр истории измерений

При выборе устройства отображается история его измерений.

История обновляется автоматически без перезапуска приложения.

## Очистка истории

Кнопка **«Очистить историю»** удаляет все измерения из базы данных.

## Экспорт истории

Кнопка **«Экспорт»** выгружает историю выбранного устройства в CSV-файл.

---

# Тестирование

## Модульные тесты

- DevicePollingService
- VirtualDeviceFactory
- VirtualMeasurementDevice
- CsvHistoryExportService
- MainViewModel
- DeviceViewModel
- MeasurementViewModel

## Интеграционные тесты

- DeviceRepository
- MeasurementRepository

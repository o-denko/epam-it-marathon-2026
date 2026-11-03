# Green & Tasty — Test Automation Framework (AQA)

Production-grade automated end-to-end testing framework developed for **EPAM IT Marathon 2026** (Test Automation track #8).

---

## 📌 Scope & Direction

- **Track Selected:** **Напрям 1 — Груповий pre-order (Власна розгорнута реалізація)**
- **Complexity Levels:**
  - 🟢 **Level 1 (View Group Pre-Order Participants):** UI + API tests
  - 🔵 **Level 2 (Submit Consolidated Pre-Order):** UI + API tests
  - 🟣 **Level 3 (Remove Participant from Group Pre-Order):** UI + API tests
  - ⭐ **Star Bonus (Reporting & SDLC):** ReportPortal configuration & Cloud CI/CD pipeline (GitHub Actions + AWS CodeBuild)
- **Application Under Test:**
  - Frontend SPA: [https://d3bnmono898o6h.cloudfront.net](https://d3bnmono898o6h.cloudfront.net)
  - Backend REST API: `https://fiw10fpnv9.execute-api.eu-north-1.amazonaws.com/develop`

---

## 🏗️ Architecture & Technology Stack

- **Language & Runtime:** C# (.NET 10)
- **BDD Framework:** **Reqnroll** (Gherkin `.feature` specifications)
- **Browser Automation:** **Microsoft.Playwright (.NET)** with automatic tracing, screenshots on failure, and shared browser/API context
- **Design Pattern:** **BDD + Screenplay Pattern** (Actors, Abilities, Tasks, Questions, and Component/Page Objects)
- **Assertions:** FluentAssertions
- **Reporting:** Built-in TRX/HTML reports, Playwright Traces & Screenshots, ReportPortal configuration
- **CI/CD:** GitHub Actions workflow (`.github/workflows/aqa-e2e-tests.yml`) + AWS CodeBuild specification (`buildspec.yml`)

---

## 📁 Project Structure

```text
aqa/
├── docker-compose.reportportal.yml    # Docker Compose setup for local ReportPortal instance
├── buildspec.yml                     # AWS CodeBuild pipeline configuration
└── restaurant-aqa-framework/
    ├── Api/
    │   ├── Clients/                  # GroupOrderApiClient (typed HTTP Playwright client)
    │   └── Models/                   # Request & Response DTOs
    ├── Configuration/
    │   └── TestSettings.cs           # Environment & .env loader
    ├── Features/                     # Gherkin BDD Feature files
    │   ├── Level1_ViewGroupParticipants.feature
    │   ├── Level2_SubmitConsolidatedPreOrder.feature
    │   └── Level3_RemoveParticipant.feature
    ├── Hooks/
    │   └── PlaywrightRunHooks.cs     # Playwright lifecycle, video/traces/screenshots on failure
    ├── Screenplay/
    │   └── ScreenplayCore.cs         # Actor, Ability, Task, Question abstractions
    ├── StepDefinitions/
    │   └── GroupOrderStepDefinitions.cs # Glued BDD Step bindings
    ├── Ui/
    │   └── Pages/GroupOrderPage.cs   # Semantic Playwright Page/Component locators
    ├── .env                          # Local test configuration (ignored by git)
    ├── .env.example                  # Environment template
    └── restaurant-aqa-framework.csproj
```

---

## ⚙️ Configuration (.env)

Copy `.env.example` to `.env` and adjust the variables if needed:

```bash
cp aqa/restaurant-aqa-framework/.env.example aqa/restaurant-aqa-framework/.env
```

Key environment settings:
* `UI_BASE_URL`: Base URL of the deployed Angular web app (default: `https://d3bnmono898o6h.cloudfront.net`).
* `API_BASE_URL`: Base URL of the deployed AWS Lambda REST API (default: `https://fiw10fpnv9.execute-api.eu-north-1.amazonaws.com/develop`).
* `HEADLESS`: `true` for CI/CD runs, `false` for visual debugging.
* `TRACE_MODE`: `on-failure` or `on` to record Playwright Traces.

---

## 🚀 Running Tests

### 1. Run all tests
```bash
cd aqa/restaurant-aqa-framework
dotnet test
```

### 2. Run by Level Tag
* **Level 1 only:**
  ```bash
  dotnet test --filter "Category=level1"
  ```
* **Level 2 only:**
  ```bash
  dotnet test --filter "Category=level2"
  ```
* **Level 3 only:**
  ```bash
  dotnet test --filter "Category=level3"
  ```

### 3. Run by Layer (UI vs API)
* **UI End-to-End Tests:**
  ```bash
  dotnet test --filter "Category=ui"
  ```
* **API Validation & Auth Tests:**
  ```bash
  dotnet test --filter "Category=api"
  ```

---

## 📊 Viewing Reports & Traces

* **HTML/TRX Test Report:**
  ```bash
  dotnet test --logger "html;LogFileName=test_results.html"
  ```
* **Playwright Trace Viewer:**
  If any test fails, trace files are automatically saved to `TestResults/traces/*.zip`. Inspect with:
  ```bash
  npx playwright show-trace aqa/restaurant-aqa-framework/TestResults/traces/<trace-file>.zip
  ```

---

## ⭐ Bonus: ReportPortal Setup

To spin up a local ReportPortal instance:
```bash
cd aqa
docker compose -f docker-compose.reportportal.yml up -d
```
Access UI at `http://localhost:8080` (default credentials: `superadmin` / `erebus`).

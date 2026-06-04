🌐 **English** | [Tiếng Việt](README.vi.md)

# 🗒️ Notaion Backend

> A Notion-inspired real-time **notes & chat** platform — an **ASP.NET Core 9** Web API built on **Clean Architecture**, containerized with **Docker** and shipped through a **Jenkins** CI/CD pipeline.

[![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/mtai0524/notaion-backend?utm_source=oss&utm_medium=github&utm_campaign=mtai0524%2Fnotaion-backend&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)](https://coderabbit.ai)
[![Build & Deploy](https://github.com/mtai0524/notaion-backend/actions/workflows/main_my-chat-console.yml/badge.svg)](https://github.com/mtai0524/notaion-backend/actions/workflows/main_my-chat-console.yml)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-mtaidev%2Fnotaion--backend-2496ED?logo=docker&logoColor=white)
![Jenkins](https://img.shields.io/badge/CI%2FCD-Jenkins-D24939?logo=jenkins&logoColor=white)

---

## ✨ Features

- 📝 **Notion-style pages** — nested pages, items and page-visit tracking
- 📅 **Daily notes** with real-time collaboration and pasted image/file attachments
- 💬 **Real-time chat** — group rooms and private conversations over SignalR
- 👥 **Friends** — friend requests and friendships
- 🔔 **Notifications** pushed in real time
- 🤖 **AI chatbot & memory** powered by ML.NET
- 📎 **File & image uploads** via Cloudinary
- 🔐 **Auth** — ASP.NET Identity, JWT Bearer, OpenID Connect and **Discord** OAuth (link/unlink providers)
- 📊 **Analytics** and 🩺 **health checks**

---

## 🏛️ Clean Architecture

Dependencies always point **inward**: outer layers depend on inner ones, and the **Domain** at the core depends on nothing.

```mermaid
flowchart BT
    API["🟢 <b>API</b> — Notaion<br/>Controllers · SignalR Hubs<br/>calls services"]
    APP["🟠 <b>Application</b> — Notaion.Application<br/>DTOs · Mappings<br/>service interfaces + implementations"]
    INFRA["🔴 <b>Infrastructure</b> — Notaion.Infrastructure<br/>DbContext · Migrations<br/>repositories · external services"]
    DOMAIN["🟣 <b>Domain</b> — Notaion.Domain<br/>entities · IRepositories"]

    API -- calls --> APP
    APP -- uses entities --> DOMAIN
    INFRA -- implements IRepositories --> DOMAIN
    INFRA -- implements services --> APP

    classDef domain fill:#f5f3ff,stroke:#7c3aed,stroke-width:2px,color:#5b21b6;
    classDef infra  fill:#fef2f2,stroke:#dc2626,stroke-width:2px,color:#b91c1c;
    classDef app    fill:#fffbeb,stroke:#f59e0b,stroke-width:2px,color:#b45309;
    classDef api    fill:#f0fdf4,stroke:#16a34a,stroke-width:2px,color:#15803d;
    class DOMAIN domain
    class INFRA infra
    class APP app
    class API api
```

| Layer | Project | Responsibility |
|---|---|---|
| 🟣 **Domain** | `Notaion.Domain` | Entities, enums and repository interfaces (`IRepositories`). The core — depends on nothing. |
| 🟠 **Application** | `Notaion.Application` | Use cases: DTOs, mappings, service interfaces and their implementations. |
| 🔴 **Infrastructure** | `Notaion.Infrastructure` | `DbContext`, EF Core migrations, repository + external-service implementations, Identity. |
| 🟢 **API** | `Notaion` | HTTP controllers, SignalR hubs, filters — the entry point that wires everything together. |

<details>
<summary>📷 Original hand-drawn diagram (source of the Mermaid above)</summary>

![clean architecture pattern](https://res.cloudinary.com/dl3hvap4a/image/upload/v1731906292/chajs841fzj4wg7ll8d3.png)

</details>

---

## 🧱 Project Structure

```
NotaionWebApp/
├── Notaion/                       # 🟢 API — Controllers, SignalR Hubs, Filters, Attributes
├── Notaion.Application/           # 🟠 Application — Services, Interfaces, DTOs, Mappings, Hubs
├── Notaion.Domain/                # 🟣 Domain — Entities, Enums, Interfaces (IRepositories)
├── Notaion.Infrastructure/        # 🔴 Infrastructure — DbContext, Migrations, Repositories, Identity
├── Notion.Aspire.AppHost/         # .NET Aspire orchestration host
└── Notion.Aspire.ServiceDefaults/ # Shared Aspire service defaults
```

---

## 🛠️ Tech Stack

| Area | Technology |
|---|---|
| Runtime | .NET 9 · ASP.NET Core Web API |
| Real-time | SignalR (chat & daily-note hubs) |
| Data | EF Core 9 · SQL Server |
| Auth | ASP.NET Identity · JWT Bearer · OpenID Connect · Discord OAuth |
| AI / ML | Microsoft.ML (ML.NET) |
| Media | Cloudinary |
| API docs | Swagger · NSwag · Scalar |
| Orchestration | .NET Aspire |
| Container | Docker (multi-stage) |
| CI/CD | Jenkins (primary) · GitHub Actions |

---

## 🚀 CI/CD

Every push to `main` triggers the **Jenkins** pipeline (via a GitHub webhook), which builds a Docker image, deploys it to **MonsterASP.NET** over FTP, publishes the image to **Docker Hub**, and finally runs the fresh container locally.

### Pipeline flow

```mermaid
flowchart TD
    DEV(["👨‍💻 git push origin main"]) --> WH["📡 GitHub Webhook<br/>githubPush trigger"]
    WH --> S1["📥 Cloning<br/>git clone main"]
    S1 --> S2["🐳 Build Docker Image<br/>.NET 9 multi-stage"]
    S2 --> S3["📦 Extract Publish Output<br/>docker cp /app to publish_output"]
    S3 --> S4["🌐 Deploy to MonsterASP<br/>FTP via lftp + app_offline page"]
    S4 --> S5["📤 Push to Docker Hub<br/>tag BUILD_NUMBER and latest"]
    S5 --> S6["🚀 Deploy Local Container<br/>host 8081 to container 8080"]
    S6 --> OK(["✅ App running, latest version"])

    classDef trig fill:#eef2ff,stroke:#6366f1,color:#3730a3;
    classDef ci fill:#eff6ff,stroke:#3b82f6,color:#1e40af;
    classDef cd fill:#ecfdf5,stroke:#10b981,color:#065f46;
    classDef ok fill:#dcfce7,stroke:#16a34a,color:#14532d;
    class DEV,WH trig
    class S1,S2,S3 ci
    class S4,S5,S6 cd
    class OK ok
```

### Pipeline stages

| # | Stage | What it does | Tooling |
|---|---|---|---|
| 1 | **Cloning** | Clone the `main` branch from GitHub | `git` |
| 2 | **Build Docker Image** | Multi-stage .NET 9 build, tagged `:BUILD_NUMBER` + `:latest` | Docker |
| 3 | **Extract Publish Output** | Copy `/app` out of the image into `publish_output/` | `docker cp` |
| 4 | **Deploy to MonsterASP** | Upload an `app_offline.htm` maintenance page, FTP-mirror the publish output, remove the offline page | `lftp` |
| 5 | **Push to Docker Hub** | Push `:BUILD_NUMBER` and `:latest` to `mtaidev/notaion-backend` | Docker Hub |
| 6 | **Deploy Local Container** | Stop/remove the old container, run the new image on host port `8081` | Docker |
| 🔁 | **post** | `success` → print URLs · `failure` → report build · `always` → cleanup + `docker image prune` | Jenkins |

### Verify a deploy

The `DeployInfo` endpoint reports the running build, version and deploy time:

```
http://localhost:8081/api/DeployInfo/info       # local container
http://notaion.runasp.net/api/DeployInfo/info   # MonsterASP
```

```json
{
  "status": "✅ Running",
  "deployedAt": "2026-05-21 15:30:00",
  "buildNumber": "11",
  "version": "11",
  "environment": "Production"
}
```

> 📖 **Full setup guide:** [jenkins-docker-guide.md](jenkins-docker-guide.md) — install Jenkins with Docker Compose, configure credentials, the complete `Jenkinsfile`, and troubleshooting.

> ℹ️ A secondary **GitHub Actions** workflow ([`main_my-chat-console.yml`](.github/workflows/main_my-chat-console.yml)) also builds, tests and deploys to MonsterASP via WebDeploy.

---

## ⚡ Getting Started (local)

```bash
# 1) Run the API directly
cd NotaionWebApp/Notaion
dotnet restore
dotnet run

# 2) …or build & run the Docker image
docker build -t notaion-backend .
docker run -d -p 8081:8080 --name notaion-backend notaion-backend
# → http://localhost:8081
```

For the Jenkins + Docker + MonsterASP deployment pipeline, see the [CI/CD guide](jenkins-docker-guide.md).

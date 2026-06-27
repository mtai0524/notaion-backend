# Notaion Backend

Backend for a Notion-inspired note-taking app — **ASP.NET Core 9**, Clean Architecture, Docker, Jenkins CI/CD.

---

## Features

| | Feature | Description |
|---|---|---|
| 📄 | Pages | Nested pages with visit history |
| 📝 | Daily Notes | Real-time collaboration, file/image attachments |
| 💬 | Chat | Group rooms & private messaging via SignalR |
| 👥 | Friends | Friend requests & management |
| 🔔 | Notifications | Real-time push notifications |
| 🤖 | AI Chatbot | Conversational AI with memory via OpenRouter |
| 🖼️ | Media | File & image uploads via Cloudinary |
| 🔐 | Auth | ASP.NET Identity · JWT · OpenID Connect · Discord OAuth |

---

## Architecture

Dependencies always point inward — Domain depends on nothing.

```
API  ──►  Application  ──►  Domain
               ▲                ▲
          Infrastructure ───────┘
```

| Layer | Project | Responsibility |
|---|---|---|
| Domain | `Notaion.Domain` | Entities, repository interfaces |
| Application | `Notaion.Application` | DTOs, mappings, service interfaces |
| Infrastructure | `Notaion.Infrastructure` | EF Core, migrations, external services |
| API | `Notaion` | Controllers, SignalR hubs |

---

## Tech Stack

| Area | Tech |
|---|---|
| Runtime | .NET 9 · ASP.NET Core Web API |
| Real-time | SignalR |
| Database | EF Core 9 · SQL Server |
| Auth | ASP.NET Identity · JWT · OpenID Connect · Discord OAuth |
| AI | ML.NET · OpenRouter |
| Media | Cloudinary |
| Container | Docker (multi-stage) |
| CI/CD | Jenkins · GitHub Actions |

---

## CI/CD

```
git push → Webhook → Clone → Build Image → Extract Output
        → Deploy to MonsterASP → Push to Docker Hub → Run Container ✅
```

**Verify deployment**

```bash
curl http://localhost:8081/api/DeployInfo/info       # local
curl http://notaion.runasp.net/api/DeployInfo/info  # production
```

```json
{ "status": "✅ Running", "buildNumber": "11", "environment": "Production" }
```

> 📖 Full setup: [jenkins-docker-guide.md](jenkins-docker-guide.md)
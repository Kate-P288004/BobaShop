# 🧋 BoBaShop – ASP.NET Core 9 MVC + API + MongoDB Project

**BoBaShop (BoBatastic)** is a full-stack .NET 9 solution for managing bubble tea products, toppings, and orders.  
It includes:
- **Web API** with MongoDB and JWT Authentication  
- **MVC Front-End** with ASP.NET Identity (SQLite)  
-  **Docker Compose** for API, Web, and MongoDB  
-  **GitHub Actions CI/CD** workflow with image publishing  

---

## 📂 Project Structure

```
BobaShop.Api/         → Backend REST API (MongoDB, JWT, Swagger)
BobaShop.Web/         → MVC front-end (Identity, Admin CRUD)
docker-compose.yml    → Multi-container setup (API + Web + MongoDB)
.github/workflows/    → GitHub Actions build and deploy workflow
```

---

## 🧰 Requirements

### Core Tools
| Tool | Minimum Version | Notes |
|------|------------------|-------|
| **.NET SDK** | 9.0 | For both API and Web builds |
| **Visual Studio 2022+** | 17.10 or later | Includes .NET 9 templates |
| **MongoDB** | 7.x | Local or in Docker |
| **Docker Desktop** | 4.x | For containerized runs |
| **GitHub Account** | Optional | To run CI/CD actions |

---

## 🪟 Option 1: Run on Windows (F5 in Visual Studio)

1. **Open the solution**  
   `BobaShop.sln` in Visual Studio.

2. **Set multiple startup projects**
   - Right-click the solution → **Properties**
   - Choose **Startup Project → Multiple startup projects**
   - Set:
     - `BobaShop.Api` → Start
     - `BobaShop.Web` → Start

3. **Check environment settings**
   - In both projects → `Properties → launchSettings.json`
   - Ensure:
     - API → `https://localhost:7274`
     - Web → `https://localhost:7243`

4. **Run (F5)**  
   Visual Studio builds both apps and starts MongoDB seeding automatically.

5. **Access:**
   - API Swagger → [https://localhost:7274/swagger](https://localhost:7274/swagger)
   - Web App → [https://localhost:7243](https://localhost:7243)

**Default Admin login:**
```
Email: admin@bobatastic.local
Password: Admin!23456
```

---

## 💻 Option 2: Run via .NET CLI (No Docker)

1. Start MongoDB locally on port **27017**.  
   (Use `mongod` or MongoDB Compass.)

2. Edit `BobaShop.Api/appsettings.json`:
   ```json
   "Mongo": {
     "ConnectionString": "mongodb://localhost:27017",
     "DatabaseName": "BobaShopDb"
   },
   "Jwt": {
     "Issuer": "BobaShop.Api",
     "Audience": "BobaShop.Api",
     "Key": "your-64+char-secret-key"
   }
   ```

3. Run the API:
   ```bash
   cd BobaShop.Api
   dotnet run
   ```

4. In another terminal, run the Web project:
   ```bash
   cd ../BobaShop.Web
   dotnet run
   ```

5. Open the URLs printed in the terminal.

---

## 🐳 Option 3: Run with Docker Compose (Full Details)

### 🧩 Overview

The Compose setup runs three services together:
| Service | Description | Port |
|----------|--------------|------|
| `mongodb` | MongoDB database | (internal) |
| `api` | ASP.NET 9 Web API with JWT + Mongo | 8080 |
| `web` | MVC frontend for admin panel | 8081 |

### ⚙️ Environment Variables

| Variable | Purpose | Example |
|-----------|----------|----------|
| ASPNETCORE_ENVIRONMENT | Runtime mode | Docker |
| ASPNETCORE_URLS | Bind address | http://+:8080 |
| Mongo__ConnectionString | Mongo connection | mongodb://mongodb:27017 |
| Mongo__DatabaseName | Database name | BobaShopDb |
| Jwt__Key | Secret for JWT | your-64+char-key |
| Api__BaseUrl | API base URL (for Web) | http://api:8080/ |

### 🛠️ Build and Run

Run from the solution root (same folder as `docker-compose.yml`):

```bash
docker compose down -v
docker compose build --no-cache
docker compose up -d
```

Wait 10–15 seconds, then open:
- [http://localhost:8080/swagger](http://localhost:8080/swagger) → API  
- [http://localhost:8081](http://localhost:8081) → Web App  

### 🧾 Logs & Inspection

```bash
docker ps
docker compose logs -f api
docker compose logs -f web
docker compose logs -f mongodb
```

Stop and clean all:
```bash
docker compose down -v
```

### 📦 Data Persistence

MongoDB uses a named volume to store data between runs:
```yaml
volumes:
  mongo_data:
```

Inspect data:
```bash
docker exec -it bobashop-mongodb mongosh
use BobaShopDb
show collections
db.drinks.find().pretty()
```

### 🧰 Troubleshooting

**Port already in use**  
→ Stop local MongoDB or other API containers:
```bash
docker stop mongodb api web
```

**401 Unauthorized on API**  
→ Log in with admin credentials and use JWT token in Swagger.

**Container name conflict**  
```bash
docker rm -f api mongodb web
```

**API not reachable**  
→ Check logs or verify network with `docker network inspect bobashop_default`.

---

## 🔐 JWT + Identity Setup

JWT section in `appsettings.json`:

```json
"Jwt": {
  "Issuer": "BobaShop.Api",
  "Audience": "BobaShop.Api",
  "Key": "pYp7yP3b1k8hQ4v2s9r0NwZ3xC6mV1t5L0u8q2f7a9d3k6r1c5b8n2m4z7w0y3"
}
```

Login token via `/api/Auth/login`:

```json
{
  "email": "admin@bobatastic.local",
  "password": "Admin!23456"
}
```

Use token in Swagger → **Authorize** → `Bearer your.jwt.token`.

---

## 🧪 GitHub Actions (CI/CD)

The `.github/workflows/docker-build.yml` automates build and deployment.

Steps:
1. Builds both API & Web images.  
2. Runs smoke test via Docker Compose.  
3. Pushes latest images to GHCR.

Triggers on every push or pull request to `main` or `master`.

---

## 🧩 Features Summary

| Area | Description |
|------|--------------|
| **Admin Panel** | Full CRUD for drinks and toppings |
| **Authentication** | JWT-secured API + Identity for Web |
| **Database** | MongoDB 7 with indexes and seeding |
| **CI/CD** | Automated testing & deployment |
| **Dockerized** | API + Web + Mongo all containerized |

---

## 🧑‍💻 Developer Info

**Course:** Diploma of IT – Advanced Programming  
**Institution:** South Metropolitan TAFE  
**Unit:** ICTPRG554 / ICTPRG556 – MVC & NoSQL Assessment  
**Date:** November 2025  

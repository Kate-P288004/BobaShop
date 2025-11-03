# 🧋 BobaShop – Docker Demo Guide

## 📘 Overview

**BobaShop** is a full-stack boba tea ordering system built with:

* ASP.NET Core 9.0 (API + MVC Web)
* MongoDB (data storage)
* Docker Compose (for easy deployment)
* GitHub Actions + GHCR (Container Registry)

This guide explains how to **download and run** the project anywhere — Windows, Linux VM, or Mac — without installing Visual Studio.

---

## 🧰 Requirements

| Tool                              | Description                               |
| --------------------------------- | ----------------------------------------- |
| 🐳 Docker Desktop / Docker Engine | Runs containers for API, Web, and MongoDB |
| 🌐 Internet                       | Required to pull images from GitHub       |
| 💻 Browser                        | To view the web app and Swagger UI        |

---

## 📥 **Download & Run Instructions**

You can run this project on **any computer** that has Docker installed — no Visual Studio or .NET SDK required.

### 🪄 Step 1 — Install Docker

#### 🪟 Windows or macOS

1. Go to [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
2. Download **Docker Desktop**
3. Install → Open → Sign in → Wait until it says **“Docker Desktop is running.”**

#### 🐧 Linux (Ubuntu/Debian)

```bash
sudo apt update
sudo apt install -y docker.io docker-compose-plugin
```

Then verify:

```bash
docker --version
docker compose version
```

---

### 📦 Step 2 — Download the Project Files

#### Option 1 – From GitHub

1. Open the repository page
2. Click **Code → Download ZIP**
3. Extract it to a folder (e.g., `C:\BobaShop` or `~/bobashop`)

#### Option 2 – From GitHub Actions

1. Go to the repo’s **Actions** tab
2. Open the workflow **Build & Push Docker (API + Web)**
3. Scroll to **Artifacts → Download ZIP**
4. Extract the ZIP to a folder on your computer

You should now see:

```
docker-compose.yml
BobaShop.Api/
BobaShop.Web/
```

---

### 🚀 Step 3 — Run the Containers

Open a terminal or PowerShell **in the extracted folder** and run:

```bash
docker compose pull
docker compose up -d
```

Docker will download and start:

* 🧩 MongoDB database
* ⚙️ API container
* 🌐 Web MVC container

Check if everything is running:

```bash
docker ps
```

---

### 🌐 Step 4 — Open the Application

| Component   | URL                                                            |
| ----------- | -------------------------------------------------------------- |
| Web App     | [http://localhost:8081](http://localhost:8081)                 |
| API Swagger | [http://localhost:8080/swagger](http://localhost:8080/swagger) |

If you’re using a **Linux VM**, replace `localhost` with your VM’s IP address (for example: `http://192.168.56.10:8081`).

---

### 🔑 Step 5 — Login as Admin

| Field        | Value                    |
| ------------ | ------------------------ |
| **Email**    | `admin@bobatastic.local` |
| **Password** | `Admin!23456`            |

After login, open `/Admin` to access the management dashboard.

---

### 🛑 Step 6 — Stop or Clean Up

To stop the containers:

```bash
docker compose down
```

To stop and delete all MongoDB data:

```bash
docker compose down -v
```

---

## 🧠 Notes for Linux VM

1. Install Docker:

   ```bash
   sudo apt update
   sudo apt install -y docker.io docker-compose-plugin
   ```
2. Extract or clone the repo:

   ```bash
   cd ~/bobashop
   sudo docker compose pull
   sudo docker compose up -d
   ```
3. Open in browser:

   * Web → `http://localhost:8081`
   * API → `http://localhost:8080/swagger`

If running on a remote VM, replace `localhost` with the VM’s IP.

---

## 🧾 Credits

**Student:** Kate Odabas (P288004)
**Course:** Diploma of IT – Application Development Project
**Lecturer:** South Metropolitan TAFE
**Project:** AT2 – BoBaTastic / BobaShop Full-Stack Docker Solution

---

✅ *Now your BobaShop app runs anywhere — even on classroom PCs and Linux VMs!*

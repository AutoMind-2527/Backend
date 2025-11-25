# 🚗 AutoMind Backend  
### Fahrzeugverwaltung • Trip Tracking • Keycloak Auth • .NET 9 • SQLite • Clean Architecture

AutoMind Backend ist ein modernes **.NET 9 REST-API-System** mit **Keycloak-Authentifizierung**, **Rollenverwaltung**,  
**Entity Framework Core**, **SQLite** und **vollständigen Unit Tests**.

Es dient als Grundlage für ein intelligentes Fahrzeug-Tracking-System (SYP/DA), das **Benutzer**,  
**Fahrzeuge**, **Fahrten (Trips)** und **GPS-Daten** verwaltet.

---

## ✨ Features

### 🔐 Authentication & Authorization
- Login über **Keycloak**
- Rollen: **Admin** & **User**
- JWT-Token-Validierung
- Automatisches Syncen von Keycloak-Usern in die DB
- Endpoints geschützt via `[Authorize]`

---

### 🚘 Vehicle Management
- Fahrzeuge anlegen, abrufen, löschen  
- Neuen Fahrzeugen wird automatisch der eingeloggte User zugeordnet  
- Admin sieht alle Fahrzeuge, User nur eigene  
- **Service-Status Berechnung:**  
  - Service notwendig bei **> 10.000 km** (Mileage + Trip-Distanz)

---

### 📍 Trip Management
- Trips anlegen, abrufen, löschen  
- Automatische Verknüpfung mit Fahrzeug + User  
- User sieht eigene Trips, Admin alle  
- **Distance** & **Duration** werden gespeichert

---

### 🗺️ GPS Logging
- GPS-Daten pro Fahrzeug/Trip speicherbar  
- Grundlage für Fahrverhaltensanalysen

---

### 🧪 Unit Testing
Getestet werden:
- TripService  
- VehicleService  
- UserService  
- AuthService  

Weitere Infos:
- InMemory-DB für Tests  
- **17+ erfolgreiche Tests**

---

## 🧰 Technologien
- .NET 9 Web API  
- EF Core + SQLite  
- Keycloak (OIDC)  
- Swagger mit OAuth2 Login  
- CORS (für Angular)  
- Clean Architecture (Services, Controller, DTOs)

---

## 📁 Projektstruktur
AutoMindBackend/
│
├── Controllers/ # REST-Endpunkte (Trips, Vehicles, Auth, User)
├── Models/ # DB-Entities
├── Services/ # Business-Logik
├── Data/ # DbContext + Seeder
├── Properties/
│ └── launchSettings.json
├── appsettings.json # Keycloak + DB Config
├── Program.cs # Startup & Middleware
│
└── AutoMindBackend.Tests/
├── TripServiceTests.cs
├── VehicleServiceTests.cs
├── UserServiceTests.cs
├── AuthServiceTests.cs
└── TestBase.cs



## 🚀 Installation

### Voraussetzungen
- .NET 9 SDK  
- Docker Desktop  
- Node + Angular (falls Frontend genutzt wird)

---

### Backend starten

#### 1. Repository klonen
```bash
git clone <repo-url>
cd AutoMindBackend
````

2. Datenbank erstellen
dotnet build


Beim ersten Start wird automind.db automatisch erstellt.

3. Backend starten
dotnet run


Backend läuft unter:
👉 http://localhost:5191

🔐 Keycloak Setup
Docker starten
docker-compose up -d


Keycloak UI:
👉 http://localhost:8080

Konfiguration

Realm: automind-realm

Client: automind-backend

Rollen:

User
Admin

🧪 Tests ausführen
cd AutoMindBackend.Tests
dotnet test


📘 API Dokumentation (Swagger)

Swagger UI:
👉 http://localhost:5191/swagger/index.html
Enthält:

Keycloak Login
Token Handling via OAuth2
Dokumentation aller Endpoints
Beispiel Requests


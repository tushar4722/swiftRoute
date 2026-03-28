# SwiftRoute Logistics System 🚚

**SwiftRoute** is an enterprise-grade "Cold Chain" and heavy-equipment transport management platform designed to replace legacy spreadsheet-based dispatching. It solves critical concurrency issues, compliance risks, and provides a real-time "Pulse Command Center" for fleet managers tracking over 500+ specialized vehicles and 1,000+ drivers.

---

## 🚀 Features

- **Live Command Center:** A real-time dashboard powered by WebSockets (SignalR). When a vehicle is assigned or requires maintenance across the fleet, the interface updates instantly without browser reloads.
- **Fleet Pairing & Dispatch:** Intelligent assignment routing that strictly validates a Driver's CDL license class against the Vehicle's required classification before allowing a dispatch pair. 
- **Driver Management Roster:** Tracks driver license validity and actively flags critical expirations (e.g., 24-month medical clearance) to prevent regulatory fines.
- **Predictive Maintenance Hub:** Tracks vehicle odometers and automatically blocks assignments if a vehicle surpasses its 10,000 km safety threshold, scheduling it for immediate maintenance logs.
- **Pessimistic Concurrency Check:** Robust Entity Framework `RowVersion` locking guarantees two dispatchers can never double-book the same asset simultaneously.

---

## 🛠️ Technology Stack

**Backend System (API)**
- **Framework:** .NET 8 Core Web API
- **ORM:** Entity Framework Core
- **Database:** MySQL
- **Real-Time Data:** SignalR (WebSockets)

**Frontend Application (Web)**
- **Framework:** Vue.js 3
- **Build Tool:** Vite
- **State Management:** Pinia
- **Styling:** Custom Vanilla CSS (Corporate Light Theme)

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js v18+](https://nodejs.org/en)
- [MySQL Server](https://www.mysql.com/)

### 1. Database Configuration
Ensure your local MySQL server is running. 
The application connects via the default connection string mapped inside `appsettings.json`:
`server=localhost;database=SwiftRouteDB;user=root;password=Tushar@4722;`

### 2. Backend Initialization
Open a terminal in the `SwiftRoute.API` directory.

Run the Entity Framework migrations to structure and seed the database with mock drivers, assignments, and test assets:
```bash
cd SwiftRoute.API
dotnet ef database drop -f        # Ensures a clean state (optional)
dotnet ef database update         # Builds tables and executes data seeding
```

Start the .NET Backend server:
```bash
dotnet run
```
*The API will boot (typically on `http://localhost:5059`).*

### 3. Frontend Initialization
Open a new terminal session in the `swiftroute-ui` directory.

Install node dependencies and run the Vite development server:
```bash
cd swiftroute-ui
npm install
npm run dev
```

Your browser will automatically open, or you can navigate to `http://localhost:5173`. 
Explore the `/drivers`, `/fleet`, and `/maintenance` tabs to experience the real-time layout.

---

## 🛡️ Architecture & Scalability
- **SignalR Push Notifications:** All database states map globally. If an asset updates, a `RefreshStats` WebSocket hook recalculates active dispatch numbers across all connected clients universally.
- **Architectural Extraction:** Heavy layout bindings (Sidebar, Notification Bar, Navigation) adhere to Vue decoupled layout encapsulation (`App.vue`), making each Page View incredibly lightweight.
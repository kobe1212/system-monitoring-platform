# Complete Setup Guide

This guide walks you through setting up the System Monitoring Platform from scratch.

## Table of Contents
1. [Prerequisites Installation](#prerequisites-installation)
2. [Backend Setup](#backend-setup)
3. [Database Setup](#database-setup)
4. [Frontend Setup](#frontend-setup)
5. [Running the Application](#running-the-application)
6. [Troubleshooting](#troubleshooting)

---

## Prerequisites Installation

### 1. Install .NET 8.0 SDK

**Windows:**
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Run the installer
3. Verify installation:
   ```bash
   dotnet --version
   ```
   Should show: `8.0.x`

**macOS/Linux:**
```bash
# Follow instructions at:
# https://docs.microsoft.com/dotnet/core/install/
```

### 2. Install Node.js and npm

**Windows:**
1. Download from: https://nodejs.org/ (LTS version)
2. Run the installer
3. Verify installation:
   ```bash
   node --version
   npm --version
   ```

**macOS:**
```bash
brew install node
```

### 3. Install SQL Server

**Windows - SQL Server Express (Free):**
1. Download from: https://www.microsoft.com/sql-server/sql-server-downloads
2. Choose "Express" edition
3. During installation, select "Basic" installation type
4. Note the connection string shown after installation

**Windows - SQL Server LocalDB (Lightweight):**
```bash
# Included with Visual Studio
# Or download SQL Server Express with LocalDB
```

**macOS/Linux - Use Docker:**
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 4. Install Git (Optional but Recommended)

```bash
# Windows: Download from https://git-scm.com/
# macOS: brew install git
# Linux: sudo apt-get install git
```

---

## Backend Setup

### Step 1: Navigate to Backend Directory

```bash
cd C:\Users\amiru\CascadeProjects\SystemMonitoring\backend
```

### Step 2: Restore NuGet Packages

```bash
dotnet restore
```

This downloads all required dependencies for the project.

### Step 3: Build the Solution

```bash
dotnet build
```

Verify there are no build errors. If errors occur, see [Troubleshooting](#troubleshooting).

### Step 4: Configure Connection String

Open `src/Monitoring.API/appsettings.json` and update the connection string:

**For SQL Server Express:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server LocalDB:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**For SQL Server with Authentication:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MonitoringDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

**For Docker SQL Server:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MonitoringDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
  }
}
```

---

## Database Setup

### Step 1: Install EF Core Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

Verify installation:
```bash
dotnet ef --version
```

### Step 2: Navigate to API Project

```bash
cd src/Monitoring.API
```

### Step 3: Create Initial Migration

```bash
dotnet ef migrations add InitialCreate --project ../Monitoring.Infrastructure
```

This creates migration files in `Monitoring.Infrastructure/Migrations/`.

**What this does:**
- Analyzes your entity models
- Generates SQL scripts to create tables
- Creates a snapshot of your database schema

### Step 4: Apply Migration to Database

```bash
dotnet ef database update
```

This creates the database and all tables.

**What this does:**
- Creates the `MonitoringDB` database if it doesn't exist
- Runs all pending migrations
- Creates tables: SystemMetrics, KpiResults, Anomalies

### Step 5: Verify Database Creation

**Using SQL Server Management Studio (SSMS):**
1. Open SSMS
2. Connect to your SQL Server instance
3. Expand "Databases"
4. You should see "MonitoringDB"
5. Expand it to see the three tables

**Using Command Line:**
```bash
sqlcmd -S localhost -E -Q "SELECT name FROM sys.databases WHERE name = 'MonitoringDB'"
```

---

## Frontend Setup

### Step 1: Navigate to Frontend Directory

```bash
cd C:\Users\amiru\CascadeProjects\SystemMonitoring\frontend
```

### Step 2: Install Dependencies

```bash
npm install
```

This may take 2-5 minutes. It installs:
- Angular framework
- RxJS for reactive programming
- TypeScript compiler
- Development tools

**If you see warnings:** Most npm warnings are safe to ignore. Only errors need attention.

### Step 3: Verify Installation

```bash
npm list --depth=0
```

Should show all packages from `package.json`.

---

## Running the Application

### Step 1: Start the Backend API

**Terminal 1:**
```bash
cd C:\Users\amiru\CascadeProjects\SystemMonitoring\backend\src\Monitoring.API
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**Verify API is running:**
- Open browser: `http://localhost:5000/swagger`
- You should see Swagger UI with API documentation

### Step 2: Start the Frontend Application

**Terminal 2 (new terminal):**
```bash
cd C:\Users\amiru\CascadeProjects\SystemMonitoring\frontend
npm start
```

**Expected output:**
```
** Angular Live Development Server is listening on localhost:4200 **
✔ Compiled successfully.
```

**Verify Frontend is running:**
- Open browser: `http://localhost:4200`
- You should see the System Monitoring Platform dashboard

### Step 3: Test the Application

1. **Create a Metric:**
   - Go to "Metrics" page
   - Click "Add Metric"
   - Fill in the form:
     - Metric Name: `ResponseTime`
     - Value: `150`
     - Unit: `ms`
     - Source: `WebServer01`
   - Click "Create Metric"

2. **View Metrics:**
   - You should see the metric in the table

3. **Calculate KPIs:**
   - Use Swagger UI: `http://localhost:5000/swagger`
   - Find `POST /api/kpis/calculate`
   - Click "Try it out" → "Execute"
   - Go back to Dashboard to see calculated KPIs

4. **Detect Anomalies:**
   - Use Swagger UI
   - Find `POST /api/anomalies/detect`
   - Click "Try it out" → "Execute"
   - Check Dashboard for anomalies

---

## Troubleshooting

### Backend Issues

**Error: "Unable to connect to SQL Server"**
```
Solution 1: Verify SQL Server is running
- Open "Services" (services.msc)
- Find "SQL Server (SQLEXPRESS)" or "SQL Server (MSSQLSERVER)"
- Ensure it's running

Solution 2: Check connection string
- Verify server name matches your installation
- Try using "." or "(local)" instead of "localhost"

Solution 3: Enable TCP/IP
- Open SQL Server Configuration Manager
- Enable TCP/IP protocol
- Restart SQL Server service
```

**Error: "dotnet ef command not found"**
```bash
dotnet tool install --global dotnet-ef
# Restart terminal after installation
```

**Error: "Build failed"**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

**Error: "Port 5000 already in use"**
```bash
# Find and kill the process using port 5000
# Windows:
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Or change port in launchSettings.json
```

### Frontend Issues

**Error: "npm install fails"**
```bash
# Clear npm cache
npm cache clean --force

# Delete node_modules and package-lock.json
rm -rf node_modules package-lock.json

# Reinstall
npm install
```

**Error: "Port 4200 already in use"**
```bash
# Kill process on port 4200
# Windows:
netstat -ano | findstr :4200
taskkill /PID <PID> /F

# Or run on different port
ng serve --port 4300
```

**Error: "Cannot find module '@angular/core'"**
```bash
# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install
```

**Error: "CORS error in browser console"**
```
Solution: Verify backend CORS configuration
- Check appsettings.json has "http://localhost:4200" in CorsOrigins
- Restart backend API
```

### Database Issues

**Error: "Database already exists"**
```bash
# Drop and recreate
dotnet ef database drop
dotnet ef database update
```

**Error: "Migration already applied"**
```bash
# Remove last migration
dotnet ef migrations remove --project ../Monitoring.Infrastructure

# Create new migration
dotnet ef migrations add NewMigration --project ../Monitoring.Infrastructure
dotnet ef database update
```

**Error: "Cannot open database"**
```
Solution: Check SQL Server permissions
- Ensure your Windows user has access
- Or use SQL Server authentication
```

---

## Development Workflow

### Making Changes to Backend

1. **Modify code** in appropriate layer
2. **Build** to check for errors:
   ```bash
   dotnet build
   ```
3. **Run** to test:
   ```bash
   dotnet run
   ```

### Making Changes to Database Schema

1. **Modify entity** in `Monitoring.Domain/Entities/`
2. **Create migration**:
   ```bash
   dotnet ef migrations add DescriptiveNameForChange --project ../Monitoring.Infrastructure
   ```
3. **Apply migration**:
   ```bash
   dotnet ef database update
   ```

### Making Changes to Frontend

Changes are automatically reloaded when you save files (hot reload).

If something doesn't update:
```bash
# Stop the server (Ctrl+C)
# Restart
npm start
```

---

## Next Steps

1. **Explore the code** - Read through each layer to understand the architecture
2. **Add features** - Try implementing new KPI calculations or anomaly detection algorithms
3. **Write tests** - Add unit tests for services
4. **Enhance UI** - Improve the dashboard with charts and better styling
5. **Deploy** - Learn to deploy to Azure or AWS

---

## Getting Help

If you encounter issues not covered here:

1. **Check error messages carefully** - They often tell you exactly what's wrong
2. **Google the error** - Include "ASP.NET Core" or "Angular" in your search
3. **Check official documentation**:
   - [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
   - [Angular Docs](https://angular.io/docs)
   - [EF Core Docs](https://docs.microsoft.com/ef/core)

---

**Congratulations!** You now have a fully functional enterprise-grade application running locally.

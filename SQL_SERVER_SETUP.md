# SQL Server Setup Guide for Windows

## Option 1: SQL Server Express (Recommended)

### Step 1: Download SQL Server Express

1. Go to: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
2. Scroll down to "Express" edition
3. Click **"Download now"** (it's free)

### Step 2: Install SQL Server Express

1. **Run the installer** (SQLServer2022-SSWEB-Expr.exe or similar)
2. Choose **"Basic"** installation type
3. Accept the license terms
4. Choose installation location (default is fine)
5. Click **"Install"**
6. Wait for installation (5-10 minutes)

### Step 3: Note Your Connection Details

After installation completes, you'll see a screen showing:
```
Instance Name: SQLEXPRESS
Connection String: Server=localhost\SQLEXPRESS;Database=...
```

**Important**: Copy this information!

### Step 4: Update Your Connection String

Open `backend/src/Monitoring.API/appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Note**: Use double backslash `\\` in JSON files.

### Step 5: Verify SQL Server is Running

1. Press `Win + R`
2. Type `services.msc` and press Enter
3. Look for **"SQL Server (SQLEXPRESS)"**
4. Status should be **"Running"**
5. If not, right-click → Start

---

## Option 2: SQL Server LocalDB (Lightweight Alternative)

LocalDB is even lighter than Express - perfect for development.

### Step 1: Check if Already Installed

Open PowerShell and run:
```powershell
sqllocaldb info
```

If you see `MSSQLLocalDB`, it's already installed! Skip to Step 3.

### Step 2: Install LocalDB

If not installed:
1. Download from: https://aka.ms/ssmsfullsetup
2. Run installer
3. Select only **"LocalDB"** component
4. Install

### Step 3: Start LocalDB

```powershell
sqllocaldb start MSSQLLocalDB
```

### Step 4: Update Connection String

Use this connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Option 3: Docker (If You Have Docker Installed)

### Step 1: Pull SQL Server Image

```powershell
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

### Step 2: Run SQL Server Container

```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd123" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

**Important**: Change `YourStrong@Passw0rd123` to a strong password!

### Step 3: Update Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MonitoringDB;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;"
  }
}
```

---

## After Installation: Create the Database

Once SQL Server is installed and running:

### Step 1: Navigate to API Project

```powershell
cd C:\Users\amiru\CascadeProjects\SystemMonitoring\backend\src\Monitoring.API
```

### Step 2: Install EF Core Tools (if not already)

```powershell
dotnet tool install --global dotnet-ef
```

### Step 3: Create Migration

```powershell
dotnet ef migrations add InitialCreate --project ../Monitoring.Infrastructure
```

### Step 4: Create Database

```powershell
dotnet ef database update
```

You should see:
```
Applying migration '20241231_InitialCreate'.
Done.
```

---

## Troubleshooting

### Error: "Cannot connect to SQL Server"

**Solution 1**: Check if SQL Server is running
```powershell
# For Express
Get-Service -Name "MSSQL$SQLEXPRESS"

# For LocalDB
sqllocaldb info MSSQLLocalDB
```

**Solution 2**: Enable TCP/IP
1. Open "SQL Server Configuration Manager"
2. Expand "SQL Server Network Configuration"
3. Click "Protocols for SQLEXPRESS"
4. Right-click "TCP/IP" → Enable
5. Restart SQL Server service

**Solution 3**: Try different connection strings
```json
// Try with period instead of localhost
"Server=.\\SQLEXPRESS;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"

// Or with (local)
"Server=(local)\\SQLEXPRESS;Database=MonitoringDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Error: "Login failed for user"

**Solution**: Use Windows Authentication (Trusted_Connection=True) or create SQL user:
1. Open SQL Server Management Studio (if installed)
2. Connect to your server
3. Security → Logins → New Login
4. Create user with password
5. Update connection string with User Id and Password

---

## Verify Installation

### Test Connection with PowerShell

```powershell
# For Express
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT @@VERSION"

# For LocalDB
sqlcmd -S (localdb)\mssqllocaldb -E -Q "SELECT @@VERSION"
```

If you see SQL Server version info, it's working!

---

## Recommended: Install SQL Server Management Studio (Optional)

SSMS is a GUI tool to manage your databases.

1. Download: https://aka.ms/ssmsfullsetup
2. Install (takes 10-15 minutes)
3. Connect to your SQL Server instance
4. You can view tables, run queries, etc.

---

## Quick Start Commands Summary

```powershell
# 1. Navigate to API project
cd C:\Users\amiru\CascadeProjects\SystemMonitoring\backend\src\Monitoring.API

# 2. Create migration
dotnet ef migrations add InitialCreate --project ../Monitoring.Infrastructure

# 3. Create database
dotnet ef database update

# 4. Run the API
dotnet run
```

---

## Which Option Should You Choose?

- **SQL Server Express**: Best for learning, similar to production environments
- **LocalDB**: Lightest option, automatic startup, good for quick development
- **Docker**: If you already use Docker, keeps your system clean

**My Recommendation**: Start with **SQL Server Express** - it's the most straightforward and closest to real-world scenarios.

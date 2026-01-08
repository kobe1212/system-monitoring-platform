# Deployment Guide - Making Your Application Publicly Accessible

## Overview
This guide shows how to deploy your System Monitoring Platform so recruiters, interviewers, and teammates can access it remotely.

---

## ⭐ Option 1: Azure App Service (RECOMMENDED for Intel Foundry)

**Why Azure?**
- ✅ Microsoft-owned (aligns with Intel's Microsoft tech stack)
- ✅ Enterprise-grade reliability
- ✅ Free tier available (F1 tier)
- ✅ Easy CI/CD integration
- ✅ Professional URL: `yourapp.azurewebsites.net`

### Prerequisites
- Azure account (free tier: https://azure.microsoft.com/free/)
- Azure CLI installed: `winget install Microsoft.AzureCLI`

### Step 1: Deploy Backend API to Azure

```bash
# Login to Azure
az login

# Create resource group
az group create --name SystemMonitoringRG --location eastus

# Create SQL Database (Basic tier - $5/month or free trial)
az sql server create --name yourname-monitoring-sql --resource-group SystemMonitoringRG --location eastus --admin-user sqladmin --admin-password YourSecurePassword123!

az sql db create --resource-group SystemMonitoringRG --server yourname-monitoring-sql --name MonitoringDB --service-objective Basic

# Get connection string
az sql db show-connection-string --client ado.net --server yourname-monitoring-sql --name MonitoringDB

# Create App Service Plan (Free tier)
az appservice plan create --name MonitoringPlan --resource-group SystemMonitoringRG --sku F1 --is-linux

# Create Web App for API
az webapp create --resource-group SystemMonitoringRG --plan MonitoringPlan --name yourname-monitoring-api --runtime "DOTNETCORE:8.0"

# Configure connection string
az webapp config connection-string set --resource-group SystemMonitoringRG --name yourname-monitoring-api --settings DefaultConnection="YOUR_CONNECTION_STRING" --connection-string-type SQLAzure

# Deploy from local folder
cd backend/src/Monitoring.API
dotnet publish -c Release -o ./publish
cd publish
zip -r ../api.zip .
az webapp deployment source config-zip --resource-group SystemMonitoringRG --name yourname-monitoring-api --src ../api.zip
```

**Your API will be available at:** `https://yourname-monitoring-api.azurewebsites.net`

### Step 2: Deploy Frontend to Azure Static Web Apps

```bash
# Create Static Web App (Free tier)
az staticwebapp create --name yourname-monitoring-frontend --resource-group SystemMonitoringRG --location eastus --source https://github.com/yourusername/yourrepo --branch main --app-location "/frontend" --output-location "dist/system-monitoring-frontend"

# Or deploy manually
cd frontend
npm run build -- --configuration production

# Upload dist folder to Azure Static Web Apps
az staticwebapp upload --name yourname-monitoring-frontend --resource-group SystemMonitoringRG --app-location ./dist/system-monitoring-frontend
```

**Your frontend will be available at:** `https://yourname-monitoring-frontend.azurestaticapps.net`

### Step 3: Update Frontend API URL

Update `frontend/src/environments/environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://yourname-monitoring-api.azurewebsites.net/api'
};
```

### Step 4: Configure CORS on Backend

Update `backend/src/Monitoring.API/appsettings.json`:
```json
{
  "CorsOrigins": [
    "https://yourname-monitoring-frontend.azurestaticapps.net"
  ]
}
```

**Total Cost:** FREE (using free tiers) or ~$5-10/month for Basic SQL Database

---

## 🚀 Option 2: Netlify (Frontend) + Railway (Backend)

**Why This Combo?**
- ✅ Completely FREE for small projects
- ✅ Very easy deployment (drag & drop)
- ✅ Fast setup (5-10 minutes)
- ✅ Good for portfolio/demo purposes

### Step 1: Deploy Backend to Railway

1. Go to https://railway.app (sign up with GitHub)
2. Click "New Project" → "Deploy from GitHub repo"
3. Select your repository
4. Railway auto-detects .NET project
5. Add PostgreSQL database (free tier)
6. Set environment variables:
   ```
   ConnectionStrings__DefaultConnection=<Railway provides this>
   ASPNETCORE_ENVIRONMENT=Production
   ```
7. Deploy automatically

**Your API:** `https://yourapp.up.railway.app`

### Step 2: Deploy Frontend to Netlify

```bash
# Build frontend
cd frontend
npm run build -- --configuration production

# Deploy to Netlify (drag & drop)
# 1. Go to https://app.netlify.com
# 2. Drag the 'dist/system-monitoring-frontend' folder
# 3. Done!
```

**Or use Netlify CLI:**
```bash
npm install -g netlify-cli
netlify login
cd frontend
npm run build -- --configuration production
netlify deploy --prod --dir=dist/system-monitoring-frontend
```

**Your frontend:** `https://yourapp.netlify.app`

**Total Cost:** FREE

---

## 🐳 Option 3: Docker + DigitalOcean/AWS

**Why Docker?**
- ✅ Professional deployment approach
- ✅ Shows DevOps skills
- ✅ Easy to scale
- ✅ ~$5-10/month

### Step 1: Create Dockerfiles

**Backend Dockerfile** (`backend/Dockerfile`):
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Monitoring.API/Monitoring.API.csproj", "Monitoring.API/"]
COPY ["src/Monitoring.Application/Monitoring.Application.csproj", "Monitoring.Application/"]
COPY ["src/Monitoring.Domain/Monitoring.Domain.csproj", "Monitoring.Domain/"]
COPY ["src/Monitoring.Infrastructure/Monitoring.Infrastructure.csproj", "Monitoring.Infrastructure/"]
RUN dotnet restore "Monitoring.API/Monitoring.API.csproj"
COPY src/ .
WORKDIR "/src/Monitoring.API"
RUN dotnet build "Monitoring.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Monitoring.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Monitoring.API.dll"]
```

**Frontend Dockerfile** (`frontend/Dockerfile`):
```dockerfile
FROM node:18 AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine
COPY --from=build /app/dist/system-monitoring-frontend /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**docker-compose.yml**:
```yaml
version: '3.8'
services:
  backend:
    build: ./backend
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=MonitoringDB;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
    depends_on:
      - db
  
  frontend:
    build: ./frontend
    ports:
      - "80:80"
    depends_on:
      - backend
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

### Step 2: Deploy to DigitalOcean

```bash
# Install doctl
winget install DigitalOcean.Doctl

# Login
doctl auth init

# Create droplet
doctl compute droplet create monitoring-app --size s-1vcpu-1gb --image docker-20-04 --region nyc1

# SSH into droplet
doctl compute ssh monitoring-app

# On the droplet:
git clone your-repo
cd your-repo
docker-compose up -d
```

**Your app:** `http://your-droplet-ip`

**Total Cost:** ~$6/month (DigitalOcean basic droplet)

---

## 📹 Option 4: Record a Video Demo (Quick Alternative)

**If you need to share immediately without deployment:**

### Record with OBS Studio (Free)
1. Download OBS Studio: https://obsproject.com/
2. Record your screen showing:
   - Dashboard with live data
   - API endpoints in Swagger
   - Database in SSMS
   - Code walkthrough
3. Upload to YouTube (unlisted)
4. Share link in your application

**Benefits:**
- ✅ Immediate solution
- ✅ Shows the app working
- ✅ Can add narration explaining features
- ✅ No deployment costs

---

## 🎯 Recommended Approach for Intel Foundry Application

### **Phase 1: Immediate (Today)**
Record a 5-10 minute video demo showing:
1. Dashboard with KPIs and charts
2. Trend analysis endpoints in Swagger
3. Statistical analysis features
4. Code architecture walkthrough

### **Phase 2: Professional Deployment (This Week)**
Deploy to **Azure** (aligns with Microsoft tech stack):
1. Backend API → Azure App Service
2. Database → Azure SQL Database
3. Frontend → Azure Static Web Apps

**Include in your application:**
- Live URL: `https://yourname-monitoring.azurestaticapps.net`
- API Documentation: `https://yourname-monitoring-api.azurewebsites.net/swagger`
- GitHub Repository: `https://github.com/yourusername/system-monitoring`
- Video Demo: `https://youtu.be/your-video-id`

---

## 📋 Deployment Checklist

### Before Deployment:
- [ ] Update `appsettings.Production.json` with production connection string
- [ ] Update `environment.prod.ts` with production API URL
- [ ] Add production CORS origins
- [ ] Test production build locally
- [ ] Remove any hardcoded secrets
- [ ] Update README with live URLs

### After Deployment:
- [ ] Test all API endpoints
- [ ] Verify dashboard loads data
- [ ] Check CORS configuration
- [ ] Test from different devices/networks
- [ ] Monitor application logs
- [ ] Set up SSL certificate (Azure provides free)

---

## 🔒 Security Considerations

### Production Checklist:
- [ ] Use environment variables for secrets
- [ ] Enable HTTPS only
- [ ] Configure proper CORS origins
- [ ] Use strong database passwords
- [ ] Enable Azure AD authentication (optional)
- [ ] Set up application insights for monitoring

### Example Production appsettings:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#" // Replaced by Azure
  },
  "CorsOrigins": [
    "https://yourname-monitoring-frontend.azurestaticapps.net"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

---

## 💡 Tips for Intel Foundry Application

### In Your Cover Letter/Email:
"I've deployed the System Monitoring Platform to Azure for your review:
- **Live Demo:** https://yourname-monitoring.azurestaticapps.net
- **API Documentation:** https://yourname-monitoring-api.azurewebsites.net/swagger
- **GitHub Repository:** https://github.com/yourusername/system-monitoring
- **Video Walkthrough:** https://youtu.be/your-video-id

The application demonstrates all key requirements including statistical analysis, trend detection, and predictive forecasting. Please feel free to test the trend analysis endpoints in Swagger."

### During Interview:
- Share screen showing live deployment
- Walk through Swagger documentation
- Demonstrate trend analysis features
- Show statistical significance testing
- Explain deployment architecture

---

## 🆘 Troubleshooting

### Common Issues:

**CORS Errors:**
- Add frontend URL to backend CORS origins
- Ensure HTTPS is used (not HTTP)

**Database Connection Fails:**
- Check connection string format
- Verify firewall rules allow Azure services
- Test connection string locally first

**Frontend Can't Reach API:**
- Update `environment.prod.ts` with correct API URL
- Rebuild frontend after changing environment
- Check browser console for errors

**Slow Performance:**
- Upgrade to paid tier (F1 free tier has limitations)
- Enable caching
- Optimize database queries

---

## 📞 Support Resources

- **Azure Documentation:** https://docs.microsoft.com/azure
- **Netlify Documentation:** https://docs.netlify.com
- **Railway Documentation:** https://docs.railway.app
- **Docker Documentation:** https://docs.docker.com

---

## 🎓 Learning Resources

To explain deployment in your interview:
- "I deployed the backend to Azure App Service using CI/CD"
- "The frontend is hosted on Azure Static Web Apps"
- "Database is Azure SQL Database with automated backups"
- "I configured CORS for secure cross-origin requests"
- "SSL certificates are automatically managed by Azure"

This demonstrates understanding of:
- Cloud deployment
- DevOps practices
- Security considerations
- Production-ready architecture

# GitHub Setup Guide - Push Your Code to GitHub

## ✅ Git is Already Installed!

Git has been installed successfully. You just need to **restart your terminal** for it to be recognized.

---

## 🚀 Quick Start (5 Minutes)

### Step 1: Create GitHub Repository

1. Go to: **https://github.com/new**
2. Fill in:
   - **Repository name:** `system-monitoring-platform`
   - **Description:** `Enterprise monitoring platform with statistical analysis, anomaly detection, and predictive forecasting - Built with .NET 8, Angular 17, SQL Server`
   - **Visibility:** ✅ Public (so recruiters can see it)
   - **DO NOT check:** "Add a README file" (we already have one)
   - **DO NOT check:** "Add .gitignore" (we already have one)
3. Click **"Create repository"**

---

### Step 2: Open New PowerShell Terminal

**IMPORTANT:** Close your current terminal and open a new PowerShell window so Git is recognized.

1. Press `Windows + X`
2. Select "Windows PowerShell" or "Terminal"
3. Navigate to your project:
   ```powershell
   cd C:\Users\amiru\CascadeProjects\SystemMonitoring
   ```

---

### Step 3: Configure Git (First Time Only)

Replace with your actual name and email:

```powershell
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

**Example:**
```powershell
git config --global user.name "Amir"
git config --global user.email "amir@example.com"
```

---

### Step 4: Initialize Git and Push Code

Copy and paste these commands **one by one** in your new terminal:

```powershell
# 1. Initialize Git repository
git init

# 2. Add all files (respects .gitignore)
git add .

# 3. Create first commit
git commit -m "Initial commit: System Monitoring & KPI Analytics Platform"

# 4. Rename branch to main
git branch -M main

# 5. Add your GitHub repository (REPLACE with your actual URL)
git remote add origin https://github.com/YOUR_USERNAME/system-monitoring-platform.git

# 6. Push code to GitHub
git push -u origin main
```

**⚠️ IMPORTANT:** In step 5, replace `YOUR_USERNAME` with your actual GitHub username!

**Example:**
```powershell
git remote add origin https://github.com/amiruddin/system-monitoring-platform.git
```

---

### Step 5: Enter GitHub Credentials

When you run `git push`, you'll be prompted for credentials:

**Option A: GitHub Personal Access Token (Recommended)**
1. Go to: https://github.com/settings/tokens
2. Click "Generate new token (classic)"
3. Give it a name: "SystemMonitoring Upload"
4. Select scopes: ✅ `repo` (all checkboxes under repo)
5. Click "Generate token"
6. **Copy the token** (you won't see it again!)
7. When prompted for password, paste the token

**Option B: GitHub CLI (Easier)**
```powershell
# Install GitHub CLI
winget install GitHub.cli

# Authenticate
gh auth login
# Follow the prompts (choose HTTPS, login via browser)

# Then push
git push -u origin main
```

---

## ✅ Verify Your Code is on GitHub

1. Go to: `https://github.com/YOUR_USERNAME/system-monitoring-platform`
2. You should see all your files!
3. Copy the repository URL for your resume

---

## 📝 Update Your Resume

Now that your code is on GitHub, update your resume:

**GitHub Link:** `https://github.com/YOUR_USERNAME/system-monitoring-platform`

Add this to your resume under the project section:
```
System Monitoring & KPI Analytics Platform
.NET 8 • Angular 17 • SQL Server • RESTful API
GitHub: https://github.com/YOUR_USERNAME/system-monitoring-platform
```

---

## 🎯 Make Your Repository Look Professional

### Add Repository Description

On your GitHub repository page:
1. Click the ⚙️ icon (top right)
2. Add description: `Enterprise monitoring platform with statistical analysis, anomaly detection, and predictive forecasting`
3. Add topics: `dotnet`, `angular`, `sql-server`, `monitoring`, `analytics`, `anomaly-detection`, `kpi`, `statistics`
4. Save changes

### Pin Repository to Profile

1. Go to your GitHub profile: `https://github.com/YOUR_USERNAME`
2. Click "Customize your pins"
3. Select `system-monitoring-platform`
4. This makes it visible at the top of your profile!

---

## 🔄 Future Updates (After Making Changes)

Whenever you make changes to your code:

```powershell
# 1. Check what changed
git status

# 2. Add all changes
git add .

# 3. Commit with descriptive message
git commit -m "Add trend analysis service with statistical significance testing"

# 4. Push to GitHub
git push
```

---

## 🆘 Troubleshooting

### Error: "git is not recognized"
**Solution:** Close and reopen your terminal. Git needs a fresh terminal session.

### Error: "remote origin already exists"
**Solution:** 
```powershell
git remote remove origin
git remote add origin https://github.com/YOUR_USERNAME/system-monitoring-platform.git
```

### Error: "failed to push some refs"
**Solution:**
```powershell
git pull origin main --allow-unrelated-histories
git push -u origin main
```

### Error: "Authentication failed"
**Solution:** Use a Personal Access Token instead of password (see Step 5 above)

### Want to exclude sensitive files?
The `.gitignore` file is already configured to exclude:
- `node_modules/`
- `bin/`, `obj/` folders
- `appsettings.Development.json`
- Database files (`.mdf`, `.ldf`)
- Build artifacts

---

## 📊 GitHub Repository Checklist

Before sharing with Intel:

- [ ] Code is pushed to GitHub
- [ ] Repository is **Public**
- [ ] README.md is visible (shows project overview)
- [ ] Repository has description and topics
- [ ] No sensitive information (passwords, API keys)
- [ ] `.gitignore` is working (no `node_modules/` or `bin/` folders)
- [ ] Repository URL is added to resume
- [ ] Repository is pinned to your profile (optional but impressive)

---

## 🎯 For Your Intel Application

**Include in Resume:**
```
GitHub: https://github.com/YOUR_USERNAME/system-monitoring-platform
```

**Include in Cover Letter:**
```
"I invite you to review my System Monitoring Platform on GitHub 
(https://github.com/YOUR_USERNAME/system-monitoring-platform), which 
demonstrates my proficiency in .NET, Angular, SQL Server, and advanced 
statistical analysis for anomaly detection and predictive forecasting."
```

**Include in Application Form:**
Most application forms have a "Portfolio/GitHub" field. Paste your repository URL there.

---

## 🌟 Bonus: Create a Professional README Badge

Add this to the top of your `README.md` to make it look more professional:

```markdown
# System Monitoring & KPI Analytics Platform

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Express-CC2927?logo=microsoft-sql-server)
![License](https://img.shields.io/badge/License-MIT-green)

> Enterprise-grade monitoring platform with statistical analysis, anomaly detection, and predictive forecasting
```

This adds colorful badges showing your tech stack!

---

## ✅ You're Ready!

Once your code is on GitHub:
1. ✅ Update your resume with the GitHub link
2. ✅ Include it in your Intel application
3. ✅ Share it with recruiters
4. ✅ Add it to your LinkedIn profile

**Your GitHub repository is now a powerful portfolio piece that demonstrates your skills to Intel Foundry!** 🚀

---

## 📞 Need Help?

If you encounter any issues:
1. Check the Troubleshooting section above
2. Verify Git is installed: `git --version` (in new terminal)
3. Ensure you're using your actual GitHub username in URLs
4. Make sure you created the repository on GitHub first

Good luck with your Intel application! 🎯

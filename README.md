# ⚽ SV Hofkirchen Vereinsverwaltung

Eine moderne, serverlose Web-Anwendung zur Verwaltung von Vereinsmitgliedern, Anwesenheiten und Benutzerrechten. Entwickelt mit **Blazor WebAssembly** und **Cloudflare Workers**.

![Status](https://img.shields.io/badge/Status-Production-success) ![Tech](https://img.shields.io/badge/Tech-Blazor%20WASM-purple) ![Backend](https://img.shields.io/badge/Backend-Cloudflare%20Workers-orange)

## ✨ Features

* **Mitgliederverwaltung:** Hinzufügen, Bearbeiten und Löschen von Vereinsmitgliedern.
* **Digitale Anwesenheitsliste:** Erfassung von Trainingsbeteiligungen mit Kalender-Funktion.
* **Rollenbasiertes System:**
  * 🛡️ *Admin:* Vollzugriff, User-Management, Systemeinstellungen.
  * 👟 *Trainer:* Kann Anwesenheiten pflegen und Mitglieder sehen.
  * 👤 *Besucher:* Nur Lesezugriff (eingeschränkt).
* **Cloud Backup System:** * Automatische nächtliche Backups auf **Google Drive**.
  * Manuelle Backup-Option im Admin-Panel.
  * Wiederherstellungsfunktion (Import) direkt im Browser.
* **Progressive Web App (PWA):** Installierbar auf Smartphones für App-ähnliches Feeling.
* **Sicherheit:** Vollständige HTTPS-Verschlüsselung und sicherer OAuth2-Flow für Backups.

## 🛠️ Technologie-Stack

### Frontend
* **Framework:** .NET 8 / Blazor WebAssembly (C#)
* **UI/UX:** Bootstrap 5 mit Glassmorphism-Design
* **Icons:** Bootstrap Icons

### Backend & Infrastruktur
* **Serverless Compute:** Cloudflare Workers (JavaScript)
* **Datenbank:** Cloudflare KV (Key-Value Store) für High-Speed Edge Access.
* **Storage API:** Google Drive API v3 (via OAuth 2.0)

## 🚀 Installation & Setup (Lokal)

Voraussetzungen: .NET 8 SDK, Node.js (optional für Worker Tests).

1. **Repository klonen**
   ```bash
   git clone [https://github.com/DEIN-USER/svhofkirchen-app.git](https://github.com/DEIN-USER/svhofkirchen-app.git)
   cd svhofkirchen-app

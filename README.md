# ⚽ SV Hofkirchen Vereinsverwaltung

Ein Cloud-natives Web-Portal zur Digitalisierung der Vereinsorganisation. Entwickelt als **Progressive Web App (PWA)**, spezialisiert auf die Verwaltung von Jugendsportgruppen, Anwesenheitserfassung und Datensicherheit.

![Status](https://img.shields.io/badge/Production-Stable-success) ![Platform](https://img.shields.io/badge/Platform-WebAssembly-blueviolet) ![Architecture](https://img.shields.io/badge/Architecture-Serverless-orange)

## 🎯 Projektziel
Ablösung dezentraler Listen durch eine zentrale, mobil verfügbare Single-Page-Application (SPA). Das System ermöglicht Trainern die Echtzeit-Erfassung von Daten und bietet Mitgliedern transparente Einsicht in Statistiken.

## 👥 Rollenkonzept (RBAC)

Das System verfügt über ein striktes Rechtemanagement mit drei definierten Ebenen:

* 👑 **Administrator**
    * **Rechte:** Vollzugriff (Read/Write/Delete) auf alle Module.
    * **Aufgaben:** Benutzerverwaltung, Pflege der Mitgliederstammdaten, Systemkonfiguration, Manuelle Backups.
* 👟 **Trainer**
    * **Rechte:** Schreibzugriff auf Anwesenheitslisten, Lesezugriff auf Mitgliederdaten.
    * **Aufgaben:** Führen der Trainingsbeteiligung, Einsehen von Notfallkontakten.
* 👤 **Mitglied**
    * **Rechte:** Lesezugriff (Read-only).
    * **Aufgaben:** Login zum Einsehen persönlicher Statistiken und Trainingsquoten.

## ✨ Funktionsumfang

### Core Features
* **Digitale Anwesenheitsliste:** Kalenderbasierte Erfassung mit One-Click-Interface für Mobilgeräte.
* **Mitglieder-Management:** Performante Verwaltung der Stammdaten.
* **Statistik-Dashboard:** Auswertung der Trainingsbeteiligung für Mitglieder und Trainer.
* **Offline-Fähigkeit:** Als PWA lokal auf Smartphones installierbar.

### Security & Reliability
* **Automatisches Cloud-Backup:** Ein serverloser Cron-Job (Trigger) sichert die gesamte Datenbank jede Nacht automatisch und verschlüsselt auf ein externes **Google Drive**.
* **OAuth 2.0 Integration:** Die Verbindung zum Backup-Speicher erfolgt über sichere Access-Tokens.
* **End-to-End Encryption:** Die gesamte Kommunikation erfolgt via HTTPS/TLS 1.3.

## 🛠️ Tech Stack

| Bereich | Technologie | Beschreibung |
| :--- | :--- | :--- |
| **Frontend** | **.NET 8 / Blazor WASM** | C# im Browser, kompiliert zu WebAssembly für native Performance. |
| **UI Framework** | **Bootstrap 5** | Responsive Design mit Custom Glassmorphism-Look. |
| **Backend** | **Cloudflare Workers** | Serverless JavaScript (V8 Engine) für minimale Latenz (Edge Computing). |
| **Datenbank** | **Cloudflare KV** | Key-Value Store für weltweiten High-Speed Datenzugriff. |
| **Backup API** | **Google Drive API v3** | REST-Schnittstelle zur externen Datensicherung. |

## 🚀 Deployment & Setup

Das Projekt ist für eine Serverless-Infrastruktur optimiert.

1.  **Frontend:** Wird als statisches Asset-Bundle (HTML/CSS/WASM) gehostet (z.B. Cloudflare Pages).
2.  **Backend:** Der Worker fungiert als API Gateway zwischen Frontend, KV-Store und Google API.
3.  **Secrets Management:** API-Schlüssel und OAuth-Tokens werden ausschließlich als verschlüsselte Environment-Variables injiziert.

---
*© SV Hofkirchen – IT Department*

# ♟️ Schachverein Hofkirchen – Digitales Vereinsmanagement

Eine hochperformante, Cloud-native Web-Plattform zur Verwaltung des Vereinsbetriebs. Entwickelt als **Progressive Web App (PWA)**, fokussiert dieses System auf Datensicherheit, mobile Verfügbarkeit für Trainer und transparente Statistiken für Mitglieder.

![Status](https://img.shields.io/badge/Production-Stable-success) ![Platform](https://img.shields.io/badge/Platform-WebAssembly-blueviolet) ![Architecture](https://img.shields.io/badge/Architecture-Serverless-orange)

## 🎯 Projektziel & Vision
Transformation der klassischen Vereinsverwaltung hin zu einer papierlosen, dezentralen Infrastruktur. Das System ersetzt statische Excel-Listen durch eine echtzeitfähige Single-Page-Application (SPA).
Ziele sind die Minimierung des administrativen Aufwands für Vorstände und Trainer sowie die Bereitstellung persönlicher Leistungsdaten für Vereinsmitglieder.

## 🏛️ Systemarchitektur & Rollenkonzept (RBAC)

Das System implementiert ein striktes **Role-Based Access Control (RBAC)** Modell, um Datensicherheit und Datenschutz (DSGVO-Konformität) zu gewährleisten.

### 1. ♚ Administrator (Vorstand/IT)
* **Zugriffslevel:** Tier 1 (Vollzugriff)
* **Verantwortung:**
    * Verwaltung des globalen Mitgliederstamms.
    * Systemkonfiguration und Benutzermanagement.
    * Überwachung der automatisierten Backup-Routinen.
    * Triggerung manueller Disaster-Recovery-Prozesse.

### 2. ♝ Trainer / Übungsleiter
* **Zugriffslevel:** Tier 2 (Operationeller Zugriff)
* **Verantwortung:**
    * Führen digitaler Anwesenheitslisten bei Trainingsabenden und Turniervorbereitungen.
    * Einsicht in relevante Spielerdaten (z.B. Notfallkontakte).
    * Datenpflege erfolgt direkt am Brett via Smartphone/Tablet.

### 3. ♙ Mitglied (Spieler)
* **Zugriffslevel:** Tier 3 (Read-Only Self-Service)
* **Funktion:**
    * Login-geschützter Zugang zum persönlichen Dashboard.
    * Einsicht in eigene Trainingsstatistiken und Anwesenheitsquoten.
    * Förderung der Transparenz und Eigenmotivation.

## ✨ Technischer Funktionsumfang

### Core Modules
* **Attendance Tracking:** Kalenderbasiertes Erfassungsmodul mit optimierter UX für mobile Endgeräte (Touch-First Design).
* **Member Lifecycle Management:** Performante Verwaltung von Ein- und Austritten sowie Stammdatenänderungen.
* **Analytics Dashboard:** Visualisierung von Teilnahmequoten zur Steuerung des Trainingsangebots.
* **Offline Capability:** Dank Service-Worker-Technologie ist die App auch bei schlechter Netzabdeckung im Vereinsheim voll funktionsfähig.

### Security & Data Integrity
* **Geo-Redundant Backup:** Ein serverloser Cron-Job initiiert nächtlich (03:00 UTC) eine verschlüsselte Datensicherung auf ein externes **Google Drive Enterprise** Repository.
* **OAuth 2.0 Authentifizierung:** Die Kommunikation zur Backup-Schnittstelle erfolgt token-basiert; es werden keine User-Credentials permanent gespeichert.
* **Zero-Trust Networking:** Jeglicher Datentransfer ist via TLS 1.3 verschlüsselt; Datenbankzugriffe erfolgen ausschließlich über authentifizierte API-Gateways.

## 🛠️ Technologie-Stack

| Layer | Technologie | Details |
| :--- | :--- | :--- |
| **Frontend** | **.NET 8 / Blazor WASM** | C# Code, der direkt im Browser (Client-Side) via WebAssembly ausgeführt wird. Sorgt für native Performance. |
| **UI/UX** | **Bootstrap 5** | Modernes, responsives Design mit "Glassmorphism" Elementen für eine hochwertige Ästhetik. |
| **Backend** | **Cloudflare Workers** | Serverless Edge Computing (V8 Engine). Minimale Latenzzeiten durch globale Verteilung. |
| **Persistence** | **Cloudflare KV** | High-Performance Key-Value Store für Lesezugriffe im Millisekundenbereich. |
| **Integration** | **Google Drive API v3** | RESTful Integration zur externen Datensicherung (Disaster Recovery). |

## 🚀 Deployment Strategie

Die Anwendung folgt einem modernen CI/CD-Ansatz für Serverless-Architekturen:

1.  **Static Content Delivery:** Das Frontend wird als statisches Asset-Bundle über ein CDN (Content Delivery Network) ausgeliefert.
2.  **Edge Computing:** Die API-Logik residiert nicht auf einem zentralen Server, sondern wird "at the edge" (nah am Benutzer) ausgeführt.
3.  **Secret Management:** Sensible Schlüssel (OAuth Client Secrets) werden zur Laufzeit über verschlüsselte Umgebungsvariablen injiziert.

---
*Copyright © 2026 Schachverein Hofkirchen*

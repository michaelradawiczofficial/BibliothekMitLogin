# 📚 BibliothekVerwaltungApp – WPF Anwendung mit Login & Benutzerverwaltung

Dieses Projekt ist eine **Desktop-Anwendung** in C# (WPF), die den **Login-Prozess**, die **Benutzerverwaltung** und ein **Rollenmodell** für eine Bibliothek simuliert.  
Es entstand während meiner **Umschulung zum Fachinformatiker Anwendungsentwicklung** und zeigt, wie ich **Benutzerverwaltung, Passwortlogik und Software-Architektur** praktisch umgesetzt habe.

Ziel ist eine leicht verständliche Anwendung, die interne Abläufe einer Bibliothek vereinfacht darstellt und zeigt, wie **Daten, Benutzerinteraktionen und Geschäftslogik** zusammengeführt werden.

---

## 🎯 Ziel des Projekts

Das Projekt wurde entwickelt, um **die Kernanforderungen einer einfachen Verwaltungssoftware** abzubilden:

- Zugang über einen **Login-Dialog**
- Verwaltung von **Benutzerkonten**
- Vergabe von **Benutzerrollen** (z.B. Standard / Administrator)
- **Passwortverwaltung**
- Übersicht über bestehende Benutzerkonten
- **Suchfunktionen** und Filter
- Sicheres Speichern von Änderungen

Das Projekt zeigt in kompakter Form, wie **Authentifizierung, Berechtigungen und Verwaltung** zueinander passen.

---

## 🧑‍💻 Funktionsumfang (einfach erklärt)

- **Login mit Benutzername & Passwort**
- **Benutzerlisten anzeigen**
- **Benutzer anlegen, bearbeiten, löschen**
- Vergabe von **Rollen** (z. B. Admin / User)
- **Passwort neu setzen**
- Suchfunktion zum schnellen Finden von Benutzern
- **Validierung** von Eingaben (Pflichtfelder etc.)
- **Dialogfenster** zur Bestätigung sensibler Aktionen
- visuelles Feedback bei Änderungen

Die Abläufe sind realitätsnah aufgebaut, aber bewusst kompakt, damit der Fokus auf der **Struktur** und dem **sauberen Code** liegt.

---

## 🏛️ Aufbau der Anwendung

Die Lösung basiert auf einer **klassischen MVVM-Struktur**, wie sie im professionellen .NET-Umfeld üblich ist:

- **Views**  
  Gestaltung der UI (Login-Fenster, Benutzerverwaltung)

- **ViewModels**  
  Logik für Benutzerverwaltung, Befehle, Bindings

- **Core / Models**  
  Datenmodelle und Rollenlogik

- **Repository**  
  Zugriff auf Benutzerdaten (CRUD Funktionen)

- **Services**  
  Dialogservice für Bestätigungen und Benachrichtigungen

Damit wird gezeigt, dass ich **Trennung von Benutzeroberfläche, Logik und Daten** verstanden und angewendet habe.

---

## 🧠 Was ich dabei gelernt habe

Bei diesem Projekt habe ich mich intensiv mit folgenden Themen beschäftigt:

### Software-Architektur
- **MVVM-Pattern** verstehen und anwenden
- Datenbindungs-Mechanismen (Bindings)
- **Command-Struktur** für Benutzeraktionen
- **Repository-Konzept** für die Datenverwaltung

### Anwendungsentwicklung
- Login-Prozess mit Eingabeprüfung
- Verwaltung von Benutzerrollen
- sichere Bedienlogik mit Bestätigungsdialogen
- **Validierung von Eingaben**
- Umgang mit **ObservableCollection** (dynamische UI-Listen)

### Arbeitsweise
- strukturierte Planung und Umsetzung
- Aufteilen einer Aufgabenstellung in einzelne Funktionen
- Dokumentation und Kommentare im Code
- Fokus auf **saubere Lesbarkeit** und **Wartbarkeit**

---

## 🖥️ Start der Anwendung

Zum Starten der Anwendung:

1. Projekt entpacken
2. Solution öffnen:
BibliothekVerwaltungApp.sln
3. Projekt ausführen in Visual Studio

Das Projekt benötigt nur:
- .NET und WPF (fertig in Visual Studio integriert)
---

## 📂 Testdaten für Benutzer und Medien

Im Projekt liegen **fertige Testdaten**, damit die Anwendung sofort genutzt werden kann, ohne selbst Benutzer oder Medien anlegen zu müssen.

Diese Daten befinden sich im Ordner:
/BibliothekMitLogin/TestDateien

markdown
Code kopieren

Enthalten sind:
- `Benutzer.json` → Beispielbenutzer (inkl. Admin)
- `medien.csv` → Beispielliste von Büchern/Medien

Damit die Anwendung diese Daten nutzen kann, müssen sie in den Datenordner der Software kopiert werden.  
Beim ersten Start erzeugt die Anwendung diesen Ordner automatisch:

/Data

markdown
Code kopieren

**Vorgehen:**
1. Anwendung einmal starten  
   → der Ordner `Data` wird angelegt  
2. Beide Dateien aus `/TestDateien` in den Ordner `Data` kopieren:
   - `Benutzer.json` → `/Data/Benutzer.json`
   - `medien.csv` → `/Data/medien.csv`

Beim nächsten Start lädt die Anwendung diese **Beispieldaten automatisch**, und alle Funktionen (Login, Suche, Reservieren) sind sofort nutzbar.

---

## 📌 Warum dieses Projekt wichtig ist

Dieses Projekt zeigt im Kleinen die **Kernaufgaben eines Fachinformatikers Anwendungsentwicklung**:

- Verständnis von **Benutzern**, **Rollen** und **Berechtigungen**
- Entwickeln einer **GUI**, die intuitiv bedienbar ist
- **fehlertolerante Verarbeitung** von Eingaben
- **saubere Code-Struktur**
- **dokumentierte Architektur-Entscheidungen**

Es zeigt nicht nur, **dass ich programmieren kann**, sondern auch,
**wie ich Software konzipiere und strukturiere**.

Das Projekt war ein **wichtiger Schritt**, um vom reinen Code-Schreiben hin zur **professionellen Softwareentwicklung** zu kommen.

---

## ✍️ Autor

**Michael Radawicz**  
Umschulung zum Fachinformatiker Anwendungsentwicklung  
Projekt: GUI-gestützte Benutzerverwaltung mit Login & Rollenmodell
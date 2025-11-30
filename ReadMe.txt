📚 BibliothekMitLogin

Ein einfaches Bibliotheksverwaltungssystem mit Login, Benutzerverwaltung und Medienverwaltung.
Erstellt als Lernprojekt in C# / .NET mit WPF-Oberfläche.

🚀 Features

🔐 Login-System mit Benutzerverwaltung

📖 Medienverwaltung (CSV-Datei)

➕ Automatische Neuerstellung von benutzer.json, falls sie fehlt

🖥️ WPF-Frontend (Windows)

🔧 Klare Trennung von App-Logik (BibliothekVerwaltung.Core) und UI (BibliothekVerwaltungApp)

📂 Projektstruktur
BibliothekMitLogin/
│
├── BibliothekVerwaltung.Core/      → Geschäftslogik
├── BibliothekVerwaltungApp/        → WPF Benutzeroberfläche
├── benutzer.json                   → Test-Benutzerdaten (wird automatisch neu erzeugt)
├── medien.csv                      → Test-Mediendaten
└── README.md

🧩 Voraussetzungen

Das Projekt benötigt folgende Umgebung:

Betriebssystem	Unterstützt	Besonderheiten
Windows 11	✔️ Ja	WPF funktioniert vollständig
macOS	⚠️ Nur CLI	WPF funktioniert nicht (GUI nicht startbar)
Linux	⚠️ Nur CLI	WPF funktioniert nicht (GUI nicht startbar)
❗ Wichtig

Die grafische Oberfläche (WPF) läuft nur unter Windows.
Auf macOS und Linux kann lediglich das Core-Projekt (BibliothekVerwaltung.Core) ausgeführt oder getestet werden.

🛠️ Installation & Start
🔵 Windows 11 (WPF lauffähig)
1. .NET Desktop Runtime installieren

Lade die .NET 9 Desktop Runtime herunter:

➡️ https://dotnet.microsoft.com/en-us/download/dotnet/9.0

Installieren:

.NET Runtime (Konsole)

.NET Desktop Runtime (für WPF)

2. Repository klonen
git clone https://github.com/DEIN-USERNAME/DEIN-REPO.git
cd BibliothekMitLogin

3. Start der WPF-App
cd BibliothekVerwaltungApp
dotnet build
dotnet run


Oder alternativ über Visual Studio:
→ Projektmappe öffnen → Starten (F5).

🟠 macOS (nur Core, kein WPF)
1. .NET Runtime installieren
brew install dotnet


oder manuell von Microsoft downloaden.

2. Core-Projekt starten
cd BibliothekMitLogin/BibliothekVerwaltung.Core
dotnet run


⚠️ Die WPF-App lässt sich unter macOS nicht starten.
Nur Logik & Tests sind nutzbar.

🟢 Linux (nur Core, kein WPF)
1. .NET installieren (Ubuntu Beispiel)
sudo apt update
sudo apt install dotnet-sdk-9.0

2. Core-Projekt starten
cd BibliothekMitLogin/BibliothekVerwaltung.Core
dotnet run


⚠️ WPF ist Windows-exklusiv.

🧾 Umgang mit benutzer.json

Die Datei benutzer.json enthält Test-Benutzerkonten und wird beim Start automatisch neu erstellt, wenn sie nicht vorhanden ist.

❗ Wenn die Datei fehlerhaft ist oder Login nicht funktioniert

Du kannst sie einfach löschen.

📌 Speicherorte

Standardmäßig liegt sie im Projektordner:

BibliothekMitLogin/BibliothekVerwaltungApp/bin/Debug/net9.0-windows/Data/Benutzer.json
Analog soll die Test medien.csv hier hin.

🔧 So setzt du sie zurück

Anwendung schließen

Datei löschen:

rm Benutzer.json


oder unter Windows:

del Benutzer.json


Anwendung neu starten
→ Die Datei wird automatisch neu generiert mit Standardbenutzern.

📁 Umgang mit medien.csv

Die Datei enthält Testmedien.
Sie bleibt unverändert erhalten und wird nicht automatisch neu erstellt.

Falls sie fehlt, muss sie manuell wieder eingefügt werden.

🧪 Beispiel-Testdaten
👤 Standard-Benutzer (beim Neu-Erstellen)
{
  "Benutzer": [
    {
      "Benutzername": "admin",
      "Passwort": "admin"
    }
  ]
}

📚 Beispiel Medien (medien.csv)
ID;Titel;Autor;Jahr
1;Der Hobbit;J.R.R. Tolkien;1937
2;Clean Code;Robert C. Martin;2008

🛡️ Hinweise zur Versionierung

Diese Dateien werden bewusst versioniert (Testzwecke):

Benutzer.json

medien.csv

Die .gitignore ist angepasst, sodass sie nicht versehentlich ausgeschlossen werden.

💬 Support

Bei Rückfragen oder Problemen einfach melden – ich helfe dir gerne weiter.
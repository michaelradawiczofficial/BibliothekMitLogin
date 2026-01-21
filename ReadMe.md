\# Bibliothek Verwaltung 2.0



Ein modernes, refaktoriertes Bibliotheksverwaltungssystem auf Basis von .NET 9.0 und WPF.

Das Projekt demonstriert eine saubere Trennung von Logik und UI (MVVM-Pattern) sowie ein rollenbasiertes Zugriffssystem.



--------------------------------------------------------------------------------



\## FEATURES



\### 1. Rollenbasiertes System

Das System unterscheidet dynamisch zwischen drei Benutzergruppen:



\* Admin:

&nbsp; Vollzugriff auf alle Bereiche, inklusive Benutzerverwaltung (Anlegen/Löschen von Usern) und Medienverwaltung.



\* Bibliothekar:

&nbsp; Verwaltung des Medienbestands (Hinzufügen, Löschen) sowie Verleih und Rücknahme von Medien.



\* Gast:

&nbsp; Eingeschränkter Zugriff. Kann den Bestand durchsuchen und bis zu 5 Medien reservieren.

&nbsp; Verwaltungs-Tabs ("Hinzufügen", "Entfernen", "Benutzerverwaltung") sind für Gäste automatisch ausgeblendet.



\### 2. Medienverwaltung

Unterstützung für verschiedene Medientypen mit spezifischen Attributen:

\* Buecher: (Titel, Autor, Verlag)

\* DVDs: (Titel, Regisseur, Laufzeit)

\* Software: (Titel, Hersteller, Betriebssystem)



Die Darstellung erfolgt über eine intelligente Liste (DataGrid), die je nach Medientyp passende Details anzeigt.



\### 3. Persistente Datenhaltung

\* Zentraler Speicherort:

&nbsp; Alle Daten liegen im Ordner "Data/" direkt im Projektverzeichnis (nicht mehr im fluechtigen "bin"-Ordner).

&nbsp; Damit bleiben Daten auch nach einem "Clean Solution" erhalten.



\* Format:

&nbsp; - Benutzer: Benutzer.json (lesbares JSON mit Text-Rollen wie "Admin").

&nbsp; - Medien: medien.csv (Semikolon-getrennte Werte).



--------------------------------------------------------------------------------



\## PROJEKTSTRUKTUR



Das Projekt ist strikt in zwei Hauptkomponenten getrennt:



BibliothekMitLogin/

&nbsp; +-- BibliothekVerwaltung.Core/      \[Die Logik / Backend]

&nbsp; |     +-- Models/                   (Datenmodelle: Benutzer, Buch, DVD...)

&nbsp; |     +-- Enums/                    (BenutzerRolle: Admin, Gast...)

&nbsp; |     +-- Repositories/             (Lade-/Speicherlogik fuer JSON \& CSV)

&nbsp; |

&nbsp; +-- BibliothekVerwaltungApp/        \[Die Oberflaeche / Frontend]

&nbsp; |     +-- ViewModels/               (Verbindet UI und Logik via MVVM)

&nbsp; |     +-- Views/                    (Fenster und Controls in XAML)

&nbsp; |

&nbsp; +-- Data/                           \[Speicherort fuer JSON/CSV]

&nbsp; |     +-- Benutzer.json

&nbsp; |     +-- medien.csv

&nbsp; |

&nbsp; +-- README.md



--------------------------------------------------------------------------------



\## DATEN \& LOGIN



Beim ersten Start erstellt die Anwendung automatisch den Ordner "Data" und generiert Standard-Dateien, falls diese fehlen.



\### 1. Benutzer (Benutzer.json)

Die Rollen werden als Text gespeichert (z. B. "Admin"), was die Datei menschenlesbar macht.



Standard-Logins (werden automatisch generiert):



| Benutzername | Passwort | Rolle        | Berechtigungen                     |

| :---         | :---     | :---         | :---                               |

| admin        | admin    | Admin        | Alles (inkl. User verwalten)       |

| bib          | bib      | Bibliothekar | Medien verwalten, Verleih          |

| gast         | gast     | Gast         | Suchen, Reservieren (max. 5)       |



Beispiel JSON-Struktur:

\[

&nbsp; {

&nbsp;   "Benutzername": "admin",

&nbsp;   "Passwort": "admin",

&nbsp;   "Rolle": "Admin"

&nbsp; }

]



\### 2. Medien (medien.csv)

Die Medien werden in einer CSV-Datei gespeichert. Das Trennzeichen ist ein Semikolon (;).



Spaltenaufbau:

Typ;Titel;AutorRegisseurHersteller;Zusatz;Id;IstVerliehen;IstReserviert;ReserviertVon;VerliehenAn;VerliehenAm



\* Typ: "Buch", "DVD" oder "Software"

\* Zusatz: Dynamisches Feld (Verlag bei Buechern, Dauer bei DVDs, OS bei Software).

\* Id: Eindeutige Kennung (z.B. B-1001, D-2005).



Beispiel CSV:

Buch;Der Hobbit;J.R.R. Tolkien;Klett-Cotta;B-1004;False;True;gast;;

DVD;Inception;Christopher Nolan;148;D-2000;False;False;;;



--------------------------------------------------------------------------------



\## INSTALLATION \& START



1\. Voraussetzung: Windows 10 oder 11 (da WPF verwendet wird).

2\. Projekt in Visual Studio 2022 oeffnen.

3\. Sicherstellen, dass "BibliothekVerwaltungApp" als Startprojekt festgelegt ist.

4\. Starten (F5).

5\. Einloggen mit einem der oben genannten Benutzer.



\### Troubleshooting



Problem: "Daten werden nach Neustart zurueckgesetzt"

\* Das neue System speichert Daten im Projektordner ".../BibliothekMitLogin/Data".

&nbsp; Stellen Sie sicher, dass Sie dort Schreibrechte besitzen.



Problem: "Unbekannte Rolle 0" in der JSON / Login geht nicht

\* Falls Sie eine alte Benutzer.json aus einer frueheren Version verwenden,

&nbsp; sind die Rollen dort eventuell noch als Zahlen (0, 1) gespeichert.

\* Loesung: Loeschen Sie die Datei "Data/Benutzer.json". Starten Sie die App neu,

&nbsp; damit eine saubere Datei erstellt wird.



--------------------------------------------------------------------------------



\## LIZENZ



Erstellt als Bibliotheksverwaltungs-Uebungsprojekt.


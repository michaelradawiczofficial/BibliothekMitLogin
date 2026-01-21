using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BibliothekVerwaltung.Core.Models
{
	public class Bibliothek
	{
		private readonly string _dataDirectory;
		private readonly string _csvFilePath;

		// CSV-Spalten:
		// Typ;Titel;AutorRegisseurHersteller;Zusatz;Id;IstVerliehen;IstReserviert;ReserviertVon;VerliehenAn;VerliehenAm
		private const string CsvHeader =
			"Typ;Titel;AutorRegisseurHersteller;Zusatz;Id;IstVerliehen;IstReserviert;ReserviertVon;VerliehenAn;VerliehenAm";

		public ObservableCollection<Medien> AlleMedien { get; } = new();
		public ObservableCollection<Buch> Buecher { get; } = new();
		public ObservableCollection<DVD> Dvds { get; } = new();
		public ObservableCollection<Software> SoftwareListe { get; } = new();

		public Bibliothek()
		{
			// 1. Startpunkt: Wo liegt die .exe? (z.B. ...\Projekt\bin\Debug\net6.0\)
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			// 2. Navigation: Wir gehen 3 Ebenen hoch, um aus dem 'bin'-Ordner rauszukommen.
			// Ziel: ...\Projekt\
			string? projectPath = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;

			// Fallback: Falls wir nicht navigieren können, bleiben wir im BaseDir.
			if (string.IsNullOrEmpty(projectPath))
			{
				projectPath = baseDirectory;
			}

			// 3. Pfade setzen: Der Data-Ordner liegt nun im Projektverzeichnis
			_dataDirectory = Path.Combine(projectPath, "Data");
			_csvFilePath = Path.Combine(_dataDirectory, "medien.csv");

			if (!Directory.Exists(_dataDirectory))
			{
				Directory.CreateDirectory(_dataDirectory);
			}

			if (!File.Exists(_csvFilePath))
			{
				ErstelleLeereCsvDatei();
			}

			LadeDaten();
		}

		private void ErstelleLeereCsvDatei()
		{
			try
			{
				using var writer = new StreamWriter(_csvFilePath, false, Encoding.UTF8);
				writer.WriteLine(CsvHeader);
			}
			catch (Exception ex)
			{
				var msg = $"❌ FEHLER beim Erstellen der CSV-Datei:\n{ex.Message}\nPfad: {_csvFilePath}\nStackTrace:\n{ex.StackTrace}";
				Debug.WriteLine(msg);
			}
		}

		/// <summary>
		/// Fügt ein neues Medium hinzu (nach oben) und speichert die Daten.
		/// </summary>
		public void AddMedium(Medien medium)
		{
			if (medium == null) return;

			// Eindeutigkeit der Id sicherstellen
			foreach (var existing in AlleMedien)
			{
				if (string.Equals(existing.Id, medium.Id, StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException($"Es existiert bereits ein Medium mit der Id '{medium.Id}'.");
				}
			}

			// Neue Medien nach oben einfügen
			AlleMedien.Insert(0, medium);

			switch (medium)
			{
				case Buch buch:
					Buecher.Insert(0, buch);
					break;
				case DVD dvd:
					Dvds.Insert(0, dvd);
					break;
				case Software software:
					SoftwareListe.Insert(0, software);
					break;
			}

			SpeichereDaten();
		}

		/// <summary>
		/// Entfernt ein Medium vollständig und speichert danach.
		/// </summary>
		public void RemoveMedium(Medien medium)
		{
			if (medium == null) return;

			AlleMedien.Remove(medium);

			switch (medium)
			{
				case Buch buch:
					Buecher.Remove(buch);
					break;
				case DVD dvd:
					Dvds.Remove(dvd);
					break;
				case Software software:
					SoftwareListe.Remove(software);
					break;
			}

			SpeichereDaten();
		}

		/// <summary>
		/// Alte Logik: Verleihstatus einfach umschalten.
		/// Wird aus Kompatibilitätsgründen beibehalten.
		/// Neue Funktionen nutzen VerleiheMedium / NimmMediumZurueck.
		/// </summary>
		public void ToggleVerliehen(Medien medium)
		{
			if (medium == null) return;

			medium.IstVerliehen = !medium.IstVerliehen;
			if (medium.IstVerliehen)
			{
				// beim einfachen Umschalten heben wir nur die Reservierung auf
				medium.IstReserviert = false;
				medium.ReserviertVon = null;

				// Verleihinformationen werden hier NICHT gesetzt –
				// dafür gibt es VerleiheMedium(..).
			}
			else
			{
				// zurückgenommen -> Verleihinfos löschen
				medium.VerliehenAn = null;
				medium.VerliehenAm = null;
			}

			SpeichereDaten();
		}

		/// <summary>
		/// Alte Logik: Reservierungsstatus umschalten.
		/// Neue Funktionen nutzen ReserviereMedium / HebeReservierungAuf.
		/// </summary>
		public void ToggleReserviert(Medien medium)
		{
			if (medium == null) return;

			medium.IstReserviert = !medium.IstReserviert;
			if (medium.IstReserviert)
			{
				// Beim alten Toggle wissen wir nicht, WER reserviert -> ReserviertVon bleibt unverändert.
				medium.IstVerliehen = false;
			}
			else
			{
				medium.ReserviertVon = null;
			}

			SpeichereDaten();
		}

		/// <summary>
		/// Verleiht ein Medium an einen bestimmten Benutzer (normalerweise vom Bibliothekar aufgerufen).
		/// Setzt IstVerliehen, VerliehenAn, VerliehenAm und hebt Reservierung auf.
		/// </summary>
		public void VerleiheMedium(Medien medium, string benutzerName)
		{
			if (medium == null) return;

			medium.IstVerliehen = true;
			medium.IstReserviert = false;
			medium.ReserviertVon = null;

			medium.VerliehenAn = benutzerName;
			medium.VerliehenAm = DateTime.Now;

			SpeichereDaten();
		}

		/// <summary>
		/// Nimmt ein Medium zurück (wird wieder verfügbar).
		/// </summary>
		public void NimmMediumZurueck(Medien medium)
		{
			if (medium == null) return;

			medium.IstVerliehen = false;
			medium.VerliehenAn = null;
			medium.VerliehenAm = null;

			SpeichereDaten();
		}

		/// <summary>
		/// Reserviert ein Medium für einen bestimmten Benutzer.
		/// </summary>
		public void ReserviereMedium(Medien medium, string benutzerName)
		{
			if (medium == null) return;

			medium.IstReserviert = true;
			medium.IstVerliehen = false;

			medium.ReserviertVon = benutzerName;

			SpeichereDaten();
		}

		/// <summary>
		/// Hebt die Reservierung eines Mediums auf.
		/// </summary>
		public void HebeReservierungAuf(Medien medium)
		{
			if (medium == null) return;

			medium.IstReserviert = false;
			medium.ReserviertVon = null;

			SpeichereDaten();
		}

		/// <summary>
		/// Anzahl der aktuellen Reservierungen eines Benutzers.
		/// Wird für das 5er-Limit von Gästen verwendet.
		/// </summary>
		public int AnzahlReservierungenVon(string benutzerName)
		{
			if (string.IsNullOrWhiteSpace(benutzerName))
				return 0;

			return AlleMedien
				.Where(m => m.IstReserviert &&
							!string.IsNullOrWhiteSpace(m.ReserviertVon) &&
							string.Equals(m.ReserviertVon, benutzerName, StringComparison.OrdinalIgnoreCase))
				.Count();
		}

		/// <summary>
		/// Prüft, ob ein Gast noch reservieren darf (max. 5 Medien).
		/// </summary>
		public bool KannGastReservieren(string benutzerName)
		{
			return AnzahlReservierungenVon(benutzerName) < 5;
		}

		public void SpeichereDaten()
		{
			try
			{
				using var writer = new StreamWriter(_csvFilePath, false, Encoding.UTF8);
				writer.WriteLine(CsvHeader);

				foreach (var medium in AlleMedien)
				{
					string typ = medium switch
					{
						Buch => "Buch",
						DVD => "DVD",
						Software => "Software",
						_ => "Unbekannt"
					};

					string zusatz = medium switch
					{
						Buch buch => buch.Verlag,
						DVD dvd => dvd.Dauer.ToString(),
						Software software => software.Betriebssystem,
						_ => string.Empty
					};

					string line =
						$"{Escape(typ)};" +
						$"{Escape(medium.Titel)};" +
						$"{Escape(medium.AutorRegisseurHersteller)};" +
						$"{Escape(zusatz)};" +
						$"{Escape(medium.Id)};" +
						$"{medium.IstVerliehen};" +
						$"{medium.IstReserviert};" +
						$"{Escape(medium.ReserviertVon ?? string.Empty)};" +
						$"{Escape(medium.VerliehenAn ?? string.Empty)};" +
						$"{(medium.VerliehenAm.HasValue ? medium.VerliehenAm.Value.ToString("o") : string.Empty)}";

					writer.WriteLine(line);
				}
			}
			catch (Exception ex)
			{
				var msg = $"❌ FEHLER beim Speichern der CSV-Datei:\n{ex.Message}\nPfad: {_csvFilePath}\nStackTrace:\n{ex.StackTrace}";
				Debug.WriteLine(msg);
			}
		}

		public void LadeDaten()
		{
			AlleMedien.Clear();
			Buecher.Clear();
			Dvds.Clear();
			SoftwareListe.Clear();

			if (!File.Exists(_csvFilePath))
				return;

			try
			{
				using var reader = new StreamReader(_csvFilePath, Encoding.UTF8);

				// Kopfzeile lesen & ignorieren
				string? header = reader.ReadLine();

				string? line;
				while ((line = reader.ReadLine()) != null)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;

					var parts = line.Split(';');

					if (parts.Length < 7)
						continue;

					string typ = Unescape(parts[0]);
					string titel = Unescape(parts[1]);
					string autor = Unescape(parts[2]);
					string zusatz = Unescape(parts[3]);
					string id = Unescape(parts[4]);

					bool istVerliehen = false;
					if (bool.TryParse(parts[5], out bool verliehen))
						istVerliehen = verliehen;

					bool istReserviert = false;
					if (bool.TryParse(parts[6], out bool reserviert))
						istReserviert = reserviert;

					// Zusätzliche Felder sind optional (für ältere CSV-Dateien)
					string reserviertVon = parts.Length > 7 ? Unescape(parts[7]) : string.Empty;
					string verliehenAn = parts.Length > 8 ? Unescape(parts[8]) : string.Empty;
					string verliehenAmString = parts.Length > 9 ? Unescape(parts[9]) : string.Empty;
					DateTime? verliehenAm = null;
					if (!string.IsNullOrWhiteSpace(verliehenAmString) &&
						DateTime.TryParse(verliehenAmString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
					{
						verliehenAm = dt;
					}

					Medien? medium = typ switch
					{
						"Buch" => new Buch(titel, autor, zusatz, id),
						"DVD" => int.TryParse(zusatz, out int dauer)
							? new DVD(titel, autor, dauer, id)
							: null,
						"Software" => new Software(titel, autor, zusatz, id),
						_ => null
					};

					if (medium == null)
						continue;

					medium.IstVerliehen = istVerliehen;
					medium.IstReserviert = istReserviert;

					medium.ReserviertVon = string.IsNullOrWhiteSpace(reserviertVon) ? null : reserviertVon;
					medium.VerliehenAn = string.IsNullOrWhiteSpace(verliehenAn) ? null : verliehenAn;
					medium.VerliehenAm = verliehenAm;

					// Reihenfolge wie in Datei -> Add (nicht Insert)
					AlleMedien.Add(medium);

					switch (medium)
					{
						case Buch buch:
							Buecher.Add(buch);
							break;
						case DVD dvd:
							Dvds.Add(dvd);
							break;
						case Software software:
							SoftwareListe.Add(software);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				var msg = $"❌ FEHLER beim Laden der CSV:\n{ex.Message}\nPfad: {_csvFilePath}\nStackTrace:\n{ex.StackTrace}";
				Debug.WriteLine(msg);
			}
		}

		private static string Escape(string value)
		{
			if (value.Contains(';') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
			{
				var escaped = value.Replace("\"", "\"\"");
				return $"\"{escaped}\"";
			}

			return value;
		}

		private static string Unescape(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			value = value.Trim();

			if (value.StartsWith("\"") && value.EndsWith("\""))
			{
				var inner = value.Substring(1, value.Length - 2);
				return inner.Replace("\"\"", "\"");
			}

			return value;
		}
	}
}
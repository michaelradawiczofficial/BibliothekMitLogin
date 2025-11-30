using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BibliothekVerwaltung.Core.Models
{
	/// <summary>
	/// Verwaltet das Laden und Speichern von Benutzern in einer JSON-Datei.
	/// Stellt Methoden zur Authentifizierung und zur Benutzerverwaltung bereit.
	/// </summary>
	public class BenutzerRepository
	{
		private readonly string _dataDirectory;
		private readonly string _jsonFilePath;

		private readonly List<Benutzer> _benutzer = new();

		/// <summary>
		/// Nur lesende Sicht auf alle bekannten Benutzer.
		/// </summary>
		public IReadOnlyList<Benutzer> AlleBenutzer => _benutzer;

		public BenutzerRepository()
		{
			_dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
			_jsonFilePath = Path.Combine(_dataDirectory, "Benutzer.json");

			if (!Directory.Exists(_dataDirectory))
			{
				Directory.CreateDirectory(_dataDirectory);
			}

			if (!File.Exists(_jsonFilePath))
			{
				// Wenn noch keine Datei existiert, legen wir eine Standardkonfiguration an:
				//  - Admin: admin / admin
				//  - Bibliothekar: bib / bib
				//  - Gast: gast / gast
				ErstelleStandardBenutzerDatei();
			}

			LadeBenutzer();
		}

		private void ErstelleStandardBenutzerDatei()
		{
			_benutzer.Clear();

			_benutzer.Add(new Benutzer("admin", "admin", BenutzerRolle.Admin));
			_benutzer.Add(new Benutzer("bib", "bib", BenutzerRolle.Bibliothekar));
			_benutzer.Add(new Benutzer("gast", "gast", BenutzerRolle.Gast));

			SpeichereBenutzer();
		}

		/// <summary>
		/// Lädt alle Benutzer aus der JSON-Datei.
		/// </summary>
		public void LadeBenutzer()
		{
			_benutzer.Clear();

			if (!File.Exists(_jsonFilePath))
				return;

			try
			{
				string json = File.ReadAllText(_jsonFilePath);
				if (string.IsNullOrWhiteSpace(json))
					return;

				var options = new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				};

				var list = JsonSerializer.Deserialize<List<Benutzer>>(json, options);
				if (list != null)
				{
					// Sicherstellen, dass Rolle sinnvoll gesetzt ist (für alte Dateien ohne Rolle)
					foreach (var benutzer in list)
					{
						// Fallback: wenn Rolle noch Default (0) aber Name "admin" o.ä., kann man hier
						// optional Heuristiken einbauen. Für jetzt reicht: Default = Gast.
						// (Der Default-Wert des Enums ist BenutzerRolle.Admin, daher prüfen wir explizit nicht.)
						// Wenn du sicherstellen willst, dass alte Benutzer Gast sind:
						// if (...) benutzer.Rolle = BenutzerRolle.Gast;
						if (string.IsNullOrWhiteSpace(benutzer.Benutzername))
						{
							benutzer.Benutzername = "unbekannt";
						}
					}

					_benutzer.AddRange(list);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fehler beim Laden von Benutzer.json: {ex.Message}");
				// Wenn Laden fehlschlägt, legen wir wieder Standardbenutzer an
				ErstelleStandardBenutzerDatei();
			}
		}

		/// <summary>
		/// Speichert alle Benutzer in die JSON-Datei.
		/// </summary>
		public void SpeichereBenutzer()
		{
			try
			{
				var options = new JsonSerializerOptions
				{
					WriteIndented = true
				};

				string json = JsonSerializer.Serialize(_benutzer, options);
				File.WriteAllText(_jsonFilePath, json);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fehler beim Speichern von Benutzer.json: {ex.Message}");
			}
		}

		/// <summary>
		/// Fügt einen neuen Benutzer hinzu und speichert die Datei.
		/// Wenn ein Benutzer mit gleichem Namen bereits existiert, wird eine Exception geworfen.
		/// </summary>
		public void AddBenutzer(Benutzer benutzer)
		{
			if (benutzer == null)
				throw new ArgumentNullException(nameof(benutzer));

			if (string.IsNullOrWhiteSpace(benutzer.Benutzername))
				throw new ArgumentException("Benutzername darf nicht leer sein.", nameof(benutzer));

			if (_benutzer.Any(b => string.Equals(b.Benutzername, benutzer.Benutzername, StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidOperationException($"Es existiert bereits ein Benutzer mit dem Namen '{benutzer.Benutzername}'.");
			}

			_benutzer.Add(benutzer);
			SpeichereBenutzer();
		}

		/// <summary>
		/// Aktualisiert einen bestehenden Benutzer (Rolle, Benutzername – optional).
		/// Die Identifikation erfolgt über den ursprünglichen Namen (oldBenutzername).
		/// </summary>
		public void UpdateBenutzer(string oldBenutzername, Benutzer updated)
		{
			if (string.IsNullOrWhiteSpace(oldBenutzername))
				throw new ArgumentException("Benutzername darf nicht leer sein.", nameof(oldBenutzername));

			if (updated == null)
				throw new ArgumentNullException(nameof(updated));

			var existing = _benutzer
				.FirstOrDefault(b => string.Equals(b.Benutzername, oldBenutzername, StringComparison.OrdinalIgnoreCase));

			if (existing == null)
			{
				throw new InvalidOperationException($"Kein Benutzer mit dem Namen '{oldBenutzername}' gefunden.");
			}

			// Wenn der Name geändert wird, prüfen, ob es den neuen schon gibt
			if (!string.Equals(oldBenutzername, updated.Benutzername, StringComparison.OrdinalIgnoreCase) &&
				_benutzer.Any(b => string.Equals(b.Benutzername, updated.Benutzername, StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidOperationException($"Es existiert bereits ein Benutzer mit dem Namen '{updated.Benutzername}'.");
			}

			existing.Benutzername = updated.Benutzername;
			existing.Rolle = updated.Rolle;

			SpeichereBenutzer();
		}

		/// <summary>
		/// Ändert das Passwort eines Benutzers und speichert die Datei.
		/// </summary>
		public void ChangePasswort(string benutzername, string neuesPasswort)
		{
			if (string.IsNullOrWhiteSpace(benutzername))
				throw new ArgumentException("Benutzername darf nicht leer sein.", nameof(benutzername));

			if (string.IsNullOrWhiteSpace(neuesPasswort))
				throw new ArgumentException("Passwort darf nicht leer sein.", nameof(neuesPasswort));

			var existing = _benutzer
				.FirstOrDefault(b => string.Equals(b.Benutzername, benutzername, StringComparison.OrdinalIgnoreCase));

			if (existing == null)
			{
				throw new InvalidOperationException($"Kein Benutzer mit dem Namen '{benutzername}' gefunden.");
			}

			existing.Passwort = neuesPasswort;
			SpeichereBenutzer();
		}

		/// <summary>
		/// Entfernt einen Benutzer (sofern vorhanden) und speichert die Datei.
		/// </summary>
		public void RemoveBenutzer(string benutzername)
		{
			if (string.IsNullOrWhiteSpace(benutzername))
				return;

			var existing = _benutzer
				.FirstOrDefault(b => string.Equals(b.Benutzername, benutzername, StringComparison.OrdinalIgnoreCase));

			if (existing != null)
			{
				_benutzer.Remove(existing);
				SpeichereBenutzer();
			}
		}

		/// <summary>
		/// Sucht einen Benutzer anhand seines Namens.
		/// </summary>
		public Benutzer? FindByName(string benutzername)
		{
			if (string.IsNullOrWhiteSpace(benutzername))
				return null;

			return _benutzer
				.FirstOrDefault(b => string.Equals(b.Benutzername, benutzername, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Authentifiziert einen Benutzer anhand Name und Passwort.
		/// Gibt den Benutzer zurück oder null, falls die Daten ungültig sind.
		/// </summary>
		public Benutzer? Authenticate(string benutzername, string passwort)
		{
			if (string.IsNullOrWhiteSpace(benutzername) || string.IsNullOrWhiteSpace(passwort))
				return null;

			return _benutzer.FirstOrDefault(b =>
				string.Equals(b.Benutzername, benutzername, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(b.Passwort, passwort));
		}

		/// <summary>
		/// Versucht, einen Benutzer zu authentifizieren.
		/// </summary>
		public bool TryAuthenticate(string benutzername, string passwort, out Benutzer? benutzer)
		{
			benutzer = Authenticate(benutzername, passwort);
			return benutzer != null;
		}
	}
}

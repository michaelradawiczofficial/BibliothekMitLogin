using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
			// 1. Startpunkt: Wo liegt die .exe? (z.B. ...\Projekt\bin\Debug\net6.0\)
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			// 2. Navigation: Wir gehen 3 Ebenen hoch, um aus dem 'bin'-Ordner rauszukommen.
			// Ziel: ...\Projekt\
			string? projectPath = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;

			// Fallback: Falls wir nicht navigieren können (z.B. bei installierter App), bleiben wir im BaseDir.
			if (string.IsNullOrEmpty(projectPath))
			{
				projectPath = baseDirectory;
			}

			// 3. Pfade setzen: Der Data-Ordner liegt nun neben dem bin-Ordner (nicht darin).
			_dataDirectory = Path.Combine(projectPath, "Data");
			_jsonFilePath = Path.Combine(_dataDirectory, "Benutzer.json");

			// Ordner erstellen, falls nicht vorhanden
			if (!Directory.Exists(_dataDirectory))
			{
				Directory.CreateDirectory(_dataDirectory);
			}

			// Standard-Datei erstellen, falls nicht vorhanden
			if (!File.Exists(_jsonFilePath))
			{
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

				// WICHTIG: Damit Enums als Text ("Admin") statt Zahl (0) gelesen werden können
				options.Converters.Add(new JsonStringEnumConverter());

				var list = JsonSerializer.Deserialize<List<Benutzer>>(json, options);
				if (list != null)
				{
					foreach (var benutzer in list)
					{
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
				// Fallback bei defekter Datei
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
					WriteIndented = true // Formatiert das JSON lesbar
				};

				// WICHTIG: Speichert Enums als Text ("Admin")
				options.Converters.Add(new JsonStringEnumConverter());

				string json = JsonSerializer.Serialize(_benutzer, options);
				File.WriteAllText(_jsonFilePath, json);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fehler beim Speichern von Benutzer.json: {ex.Message}");
			}
		}

		// --- Ab hier bleiben die Methoden identisch zu deinem Original ---

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

			if (!string.Equals(oldBenutzername, updated.Benutzername, StringComparison.OrdinalIgnoreCase) &&
				_benutzer.Any(b => string.Equals(b.Benutzername, updated.Benutzername, StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidOperationException($"Es existiert bereits ein Benutzer mit dem Namen '{updated.Benutzername}'.");
			}

			existing.Benutzername = updated.Benutzername;
			existing.Rolle = updated.Rolle;

			SpeichereBenutzer();
		}

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

		public Benutzer? FindByName(string benutzername)
		{
			if (string.IsNullOrWhiteSpace(benutzername))
				return null;

			return _benutzer
				.FirstOrDefault(b => string.Equals(b.Benutzername, benutzername, StringComparison.OrdinalIgnoreCase));
		}

		public Benutzer? Authenticate(string benutzername, string passwort)
		{
			if (string.IsNullOrWhiteSpace(benutzername) || string.IsNullOrWhiteSpace(passwort))
				return null;

			return _benutzer.FirstOrDefault(b =>
				string.Equals(b.Benutzername, benutzername, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(b.Passwort, passwort));
		}

		public bool TryAuthenticate(string benutzername, string passwort, out Benutzer? benutzer)
		{
			benutzer = Authenticate(benutzername, passwort);
			return benutzer != null;
		}
	}
}
using System;

namespace BibliothekVerwaltung.Core.Models;


	/// <summary>
	/// Repräsentiert einen Benutzer des Systems.
	/// Wird für den Login und die Rechteverwaltung verwendet.
	/// Die Daten werden über das BenutzerRepository in einer JSON-Datei gespeichert.
	/// </summary>
	public class Benutzer
	{
		/// <summary>
		/// Anzuzeigender Benutzername (Login-Name).
		/// </summary>
		public string Benutzername { get; set; } = string.Empty;

		/// <summary>
		/// Einfaches Passwort im Klartext.
		/// (Für ein Übungsprojekt ausreichend; in echten Anwendungen unbedingt Hashing verwenden!)
		/// </summary>
		public string Passwort { get; set; } = string.Empty;

		/// <summary>
		/// Rolle des Benutzers: Admin, Bibliothekar oder Gast.
		/// </summary>
		public BenutzerRolle Rolle { get; set; } = BenutzerRolle.Gast;

		/// <summary>
		/// True, wenn der Benutzer Bibliothekar-Rechte hat
		/// (also Bibliothekar ODER Admin).
		/// </summary>
		public bool IstBibliothekar => Rolle == BenutzerRolle.Admin || Rolle == BenutzerRolle.Bibliothekar;

		/// <summary>
		/// True, wenn der Benutzer Gast ist.
		/// Wird für das Reservierungslimit verwendet.
		/// </summary>
		public bool IstGast => Rolle == BenutzerRolle.Gast;

		public Benutzer()
		{
		}

		public Benutzer(string benutzername, string passwort, BenutzerRolle rolle)
		{
			Benutzername = benutzername ?? throw new ArgumentNullException(nameof(benutzername));
			Passwort = passwort ?? throw new ArgumentNullException(nameof(passwort));
			Rolle = rolle;
		}

		public override string ToString()
		{
			return $"{Benutzername} ({Rolle})";
		}
	}


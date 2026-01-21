namespace BibliothekVerwaltung.Core.Models
{
	/// <summary>
	/// Verfügbare Benutzerrollen im System.
	/// Admin: darf alles (inkl. Benutzerverwaltung).
	/// Bibliothekar: darf Medien verwalten (verleihen, löschen, hinzufügen).
	/// Gast: darf Medien nur suchen und bis zu 5 reservieren.
	/// </summary>
	public enum BenutzerRolle
	{
		Admin,
		Bibliothekar,
		Gast
	}
}
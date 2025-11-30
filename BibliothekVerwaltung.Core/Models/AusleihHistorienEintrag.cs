using System;

namespace BibliothekVerwaltung.Core.Models
{
	/// <summary>
	/// Art der Aktion in der Ausleih-Historie.
	/// </summary>
	public enum AusleihAktion
	{
		Reserviert,
		ReservierungAufgehoben,
		Verliehen,
		Zurueckgegeben
	}

	/// <summary>
	/// Ein Eintrag in der Ausleih-Historie eines Benutzers.
	/// </summary>
	public class AusleihHistorienEintrag
	{
		public AusleihHistorienEintrag(Medien medium, DateTime zeitpunkt, AusleihAktion aktion)
		{
			Medium = medium ?? throw new ArgumentNullException(nameof(medium));
			Zeitpunkt = zeitpunkt;
			Aktion = aktion;
		}

		/// <summary>
		/// Das betroffene Medium.
		/// </summary>
		public Medien Medium { get; }

		/// <summary>
		/// Zeitpunkt der Aktion (Reservierung, Verleih usw.).
		/// </summary>
		public DateTime Zeitpunkt { get; }

		/// <summary>
		/// Art der Aktion.
		/// </summary>
		public AusleihAktion Aktion { get; }
	}
}

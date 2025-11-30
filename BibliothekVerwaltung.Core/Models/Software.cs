using System;

namespace BibliothekVerwaltung.Core.Models
{
	public class Software : Medien
	{
		public string Betriebssystem { get; set; }

		public Software(string titel, string hersteller, string betriebssystem, string id)
			: base(titel, hersteller, id)
		{
			Betriebssystem = betriebssystem ?? string.Empty;
		}

		public override string HoleInfo()
		{
			return $"Software: {Titel}, Hersteller: {AutorRegisseurHersteller}, OS: {Betriebssystem}, ID: {Id}";
		}
	}
}

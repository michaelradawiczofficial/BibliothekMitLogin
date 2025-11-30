using System;

namespace BibliothekVerwaltung.Core.Models
{
	public class DVD : Medien
	{
		public int Dauer { get; set; }

		public DVD(string titel, string regisseur, int dauer, string id)
			: base(titel, regisseur, id)
		{
			Dauer = dauer;
		}

		public override string HoleInfo()
		{
			return $"DVD: {Titel}, Regisseur: {AutorRegisseurHersteller}, Dauer: {Dauer} Minuten, ID: {Id}";
		}
	}
}

using System;

namespace BibliothekVerwaltung.Core.Models
{
	public class Buch : Medien
	{
		public string Verlag { get; set; }

		public Buch(string titel, string autor, string verlag, string id)
			: base(titel, autor, id)
		{
			Verlag = verlag ?? string.Empty;
		}

		public override string HoleInfo()
		{
			return $"Buch: {Titel}, Autor: {AutorRegisseurHersteller}, Verlag: {Verlag}, ID: {Id}";
		}
	}
}

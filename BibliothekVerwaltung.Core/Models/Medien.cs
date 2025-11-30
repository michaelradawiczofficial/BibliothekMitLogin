using System;
using System.ComponentModel;

namespace BibliothekVerwaltung.Core.Models
{
	public abstract class Medien : INotifyPropertyChanged
	{
		private string _titel;
		private string _autorRegisseurHersteller;
		private string _id;

		private bool _istVerliehen;
		private bool _istReserviert;

		// NEU: Informationen zu Reservierung / Verleih
		private string? _reserviertVon;
		private string? _verliehenAn;
		private DateTime? _verliehenAm;

		public string Titel
		{
			get => _titel;
			set
			{
				if (_titel != value)
				{
					_titel = value;
					OnPropertyChanged(nameof(Titel));
				}
			}
		}

		public string AutorRegisseurHersteller
		{
			get => _autorRegisseurHersteller;
			set
			{
				if (_autorRegisseurHersteller != value)
				{
					_autorRegisseurHersteller = value;
					OnPropertyChanged(nameof(AutorRegisseurHersteller));
				}
			}
		}

		public string Id
		{
			get => _id;
			set
			{
				if (_id != value)
				{
					_id = value;
					OnPropertyChanged(nameof(Id));
				}
			}
		}

		public bool IstVerliehen
		{
			get => _istVerliehen;
			set
			{
				if (_istVerliehen != value)
				{
					_istVerliehen = value;
					OnPropertyChanged(nameof(IstVerliehen));
					OnPropertyChanged(nameof(Status));
				}
			}
		}

		public bool IstReserviert
		{
			get => _istReserviert;
			set
			{
				if (_istReserviert != value)
				{
					_istReserviert = value;
					OnPropertyChanged(nameof(IstReserviert));
					OnPropertyChanged(nameof(Status));
				}
			}
		}

		/// <summary>
		/// Name des Benutzers, der das Medium reserviert hat.
		/// Wird insbesondere für Gäste genutzt, um das 5er-Limit pro Gast zu prüfen.
		/// </summary>
		public string? ReserviertVon
		{
			get => _reserviertVon;
			set
			{
				if (_reserviertVon != value)
				{
					_reserviertVon = value;
					OnPropertyChanged(nameof(ReserviertVon));
				}
			}
		}

		/// <summary>
		/// Name der Person, an die das Medium verliehen wurde.
		/// Für Bibliothekare sichtbar.
		/// </summary>
		public string? VerliehenAn
		{
			get => _verliehenAn;
			set
			{
				if (_verliehenAn != value)
				{
					_verliehenAn = value;
					OnPropertyChanged(nameof(VerliehenAn));
				}
			}
		}

		/// <summary>
		/// Datum, seit wann das Medium verliehen ist.
		/// </summary>
		public DateTime? VerliehenAm
		{
			get => _verliehenAm;
			set
			{
				if (_verliehenAm != value)
				{
					_verliehenAm = value;
					OnPropertyChanged(nameof(VerliehenAm));
				}
			}
		}

		public string Status
		{
			get
			{
				if (IstVerliehen) return "verliehen";
				if (IstReserviert) return "reserviert";
				return "verfügbar";
			}
		}

		protected Medien(string titel, string autorRegisseurHersteller, string id)
		{
			_titel = titel ?? throw new ArgumentNullException(nameof(titel));
			_autorRegisseurHersteller = autorRegisseurHersteller ?? string.Empty;
			_id = id ?? throw new ArgumentNullException(nameof(id));
		}

		public abstract string HoleInfo();

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using BibliothekVerwaltung.Core.Models;
using BibliothekVerwaltungApp.Commands;
using BibliothekVerwaltungApp.Services;
using BibliothekVerwaltungApp.Views;

namespace BibliothekVerwaltungApp.ViewModels
{
	public class MainViewModel : NotifyPropertyChangedBase
	{
		private readonly Bibliothek _bibliothek;
		private readonly IDialogService _dialogService;

		// --- Angemeldeter Benutzer (vom Login) ---
		private Benutzer? _aktuellerBenutzer;
		public Benutzer? AktuellerBenutzer
		{
			get => _aktuellerBenutzer;
			private set
			{
				if (SetProperty(ref _aktuellerBenutzer, value))
				{
					OnPropertyChanged(nameof(IsBibliothekar));
					OnPropertyChanged(nameof(IsGast));
					OnPropertyChanged(nameof(IstAdmin));
					OnRoleChanged();
				}
			}
		}

		/// <summary>
		/// Admin oder Bibliothekar (darf Medien verwalten).
		/// </summary>
		private bool IsBibliothekar => AktuellerBenutzer?.IstBibliothekar == true;

		/// <summary>
		/// Benutzer ist Gast (unterliegt 5er-Reservierungslimit).
		/// </summary>
		private bool IsGast => AktuellerBenutzer?.IstGast == true;

		/// <summary>
		/// Benutzer ist Admin (darf Benutzerverwaltung öffnen).
		/// </summary>
		public bool IstAdmin => AktuellerBenutzer?.Rolle == BenutzerRolle.Admin;

		// --- alte Rollen-Enum für bestehende XAML-Logik (Tabsichtbarkeit) ---
		private BenutzerRolle _currentBenutzerRolle;
		public ObservableCollection<BenutzerRolle> AvailableRoles { get; }

		public BenutzerRolle CurrentBenutzerRolle
		{
			get => _currentBenutzerRolle;
			set
			{
				if (SetProperty(ref _currentBenutzerRolle, value))
				{
					OnRoleChanged();
				}
			}
		}

		// --- CollectionViews (für Filter / Tabs) ---
		public ICollectionView GefilterteMedien { get; }
		public ICollectionView GefilterteBuecher { get; }
		public ICollectionView GefilterteDvds { get; }
		public ICollectionView GefilterteSoftware { get; }

		// Medientypen für ComboBox
		public ObservableCollection<string> MediaTypes { get; }

		// --- Commands ---
		public ICommand HinzufuegenCommand { get; }
		public ICommand EntfernenCommand { get; }
		public ICommand ToggleVerliehenCommand { get; }
		public ICommand ToggleReserviertCommand { get; }
		public ICommand ClearSearchCommand { get; }
		public ICommand ClearAddFieldsCommand { get; }
		public ICommand ClearRemoveSelectionCommand { get; }
		public ICommand OpenUserManagementCommand { get; }

		// --- Konstruktoren ---
		public MainViewModel()
			: this(App.GlobaleBibliothek, new DialogService())
		{
		}

		public MainViewModel(Bibliothek bibliothek, IDialogService dialogService)
		{
			_bibliothek = bibliothek ?? throw new ArgumentNullException(nameof(bibliothek));
			_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

			// Rollen für bestehende UI (Tabsichtbarkeit). 
			// Wird durch Login über SetAngemeldeterBenutzer gesetzt.
			AvailableRoles = new ObservableCollection<BenutzerRolle>(
				Enum.GetValues(typeof(BenutzerRolle)).Cast<BenutzerRolle>());

			_currentBenutzerRolle = BenutzerRolle.Bibliothekar;

			// CollectionViews
			GefilterteMedien = CollectionViewSource.GetDefaultView(_bibliothek.AlleMedien);
			GefilterteBuecher = CollectionViewSource.GetDefaultView(_bibliothek.Buecher);
			GefilterteDvds = CollectionViewSource.GetDefaultView(_bibliothek.Dvds);
			GefilterteSoftware = CollectionViewSource.GetDefaultView(_bibliothek.SoftwareListe);

			GefilterteMedien.Filter = FilterMedien;
			GefilterteBuecher.Filter = FilterMedien;
			GefilterteDvds.Filter = FilterMedien;
			GefilterteSoftware.Filter = FilterMedien;

			// Medientypen (für ComboBox)
			MediaTypes = new ObservableCollection<string>(new[] { "Buch", "DVD", "Software" });
			_selectedMediaType = MediaTypes.FirstOrDefault();

			// Commands
			HinzufuegenCommand = new RelayCommand(_ => FuegeMediumHinzu(), _ => KannHinzufuegen());
			EntfernenCommand = new RelayCommand(_ => EntferneMedium(), _ => KannEntfernen());
			ToggleVerliehenCommand = new RelayCommand(
				m => ToggleVerliehen(m as Medien),
				m => m is Medien && IsBibliothekar);

			ToggleReserviertCommand = new RelayCommand(
				m => ToggleReserviert(m as Medien),
				m => CanToggleReserviert(m as Medien));

			ClearSearchCommand = new RelayCommand(
				_ => ClearSearch(),
				_ => !string.IsNullOrWhiteSpace(SearchText));

			ClearAddFieldsCommand = new RelayCommand(
				_ => ClearAddFields(),
				_ => IsBibliothekar && HatEingaben());

			ClearRemoveSelectionCommand = new RelayCommand(
				_ => ClearRemoveSelection(),
				_ => SelectedMediumToRemove != null || !string.IsNullOrWhiteSpace(SearchText));

			OpenUserManagementCommand = new RelayCommand(
				_ => OpenUserManagement(),
				_ => IstAdmin);

			// Initiale ID für den Start-Medientyp generieren
			UpdateNeueId();

			OnRoleChanged();
		}

		// Wird von App.xaml.cs nach erfolgreichem Login aufgerufen
		public void SetAngemeldeterBenutzer(Benutzer benutzer)
		{
			AktuellerBenutzer = benutzer ?? throw new ArgumentNullException(nameof(benutzer));

			// Für die bestehende XAML (Tab-Sichtbarkeit) mappen wir die Rolle grob:
			if (benutzer.Rolle == BenutzerRolle.Admin || benutzer.Rolle == BenutzerRolle.Bibliothekar)
				CurrentBenutzerRolle = BenutzerRolle.Bibliothekar;
			else
				CurrentBenutzerRolle = BenutzerRolle.Gast;
		}

		// --- Properties für Suche ---
		private string? _searchText;
		public string? SearchText
		{
			get => _searchText;
			set
			{
				if (SetProperty(ref _searchText, value))
				{
					AktualisiereAlleFilter();
					(ClearSearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
					(ClearRemoveSelectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
				}
			}
		}

		// --- Properties für Hinzufügen ---
		private string? _selectedMediaType;
		public string? SelectedMediaType
		{
			get => _selectedMediaType;
			set
			{
				if (SetProperty(ref _selectedMediaType, value))
				{
					UpdateNeueId();
					UpdateHinzufuegenCommands();
				}
			}
		}

		private string? _neuerTitel;
		public string? NeuerTitel
		{
			get => _neuerTitel;
			set
			{
				if (SetProperty(ref _neuerTitel, value))
				{
					UpdateHinzufuegenCommands();
				}
			}
		}

		private string? _neuerAutor;
		public string? NeuerAutor
		{
			get => _neuerAutor;
			set
			{
				if (SetProperty(ref _neuerAutor, value))
				{
					UpdateHinzufuegenCommands();
				}
			}
		}

		private string? _zusatz1;
		public string? Zusatz1
		{
			get => _zusatz1;
			set
			{
				if (SetProperty(ref _zusatz1, value))
				{
					UpdateHinzufuegenCommands();
				}
			}
		}

		private string? _neueId;
		public string? NeueId
		{
			get => _neueId;
			private set
			{
				if (SetProperty(ref _neueId, value))
				{
					UpdateHinzufuegenCommands();
				}
			}
		}

		// --- Auswahl zum Entfernen ---
		private Medien? _selectedMediumToRemove;
		public Medien? SelectedMediumToRemove
		{
			get => _selectedMediumToRemove;
			set
			{
				if (SetProperty(ref _selectedMediumToRemove, value))
				{
					(EntfernenCommand as RelayCommand)?.RaiseCanExecuteChanged();
					(ClearRemoveSelectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
				}
			}
		}

		// --- Hilfsfunktionen für CanExecute-Updates ---
		private void UpdateHinzufuegenCommands()
		{
			(HinzufuegenCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(ClearAddFieldsCommand as RelayCommand)?.RaiseCanExecuteChanged();
		}

		private bool HatEingaben()
		{
			return !string.IsNullOrWhiteSpace(SelectedMediaType) ||
				   !string.IsNullOrWhiteSpace(NeuerTitel) ||
				   !string.IsNullOrWhiteSpace(NeuerAutor) ||
				   !string.IsNullOrWhiteSpace(Zusatz1) ||
				   !string.IsNullOrWhiteSpace(NeueId);
		}

		private void OnRoleChanged()
		{
			(HinzufuegenCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(EntfernenCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(ToggleVerliehenCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(ToggleReserviertCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(ClearAddFieldsCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(ClearRemoveSelectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(OpenUserManagementCommand as RelayCommand)?.RaiseCanExecuteChanged();

			if (IsGast)
			{
				ClearAddFields();
				SelectedMediumToRemove = null;
			}
		}

		// --- Filter ---
		private bool FilterMedien(object obj)
		{
			if (obj is not Medien medium)
				return false;

			var search = SearchText;
			if (string.IsNullOrWhiteSpace(search))
				return true;

			search = search.Trim();

			return ContainsIgnoreCase(medium.Titel, search) ||
				   ContainsIgnoreCase(medium.AutorRegisseurHersteller, search) ||
				   ContainsIgnoreCase(medium.Id, search);
		}

		private static bool ContainsIgnoreCase(string? text, string? search)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
				return false;

			return text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private void AktualisiereAlleFilter()
		{
			GefilterteMedien.Refresh();
			GefilterteBuecher.Refresh();
			GefilterteDvds.Refresh();
			GefilterteSoftware.Refresh();
		}

		// --- Commands: Suche ---
		private void ClearSearch()
		{
			SearchText = string.Empty;
			AktualisiereAlleFilter();
		}

		// --- ID-Generierung ---
		private void UpdateNeueId()
		{
			if (string.IsNullOrWhiteSpace(SelectedMediaType))
			{
				NeueId = string.Empty;
				return;
			}

			NeueId = GenerateNextIdForType(SelectedMediaType);
		}

		private string GenerateNextIdForType(string mediaType)
		{
			char prefix = mediaType switch
			{
				"Buch" => 'B',
				"DVD" => 'D',
				"Software" => 'S',
				_ => 'X'
			};

			int maxNumber = 0;

			foreach (var medium in _bibliothek.AlleMedien)
			{
				if (string.IsNullOrWhiteSpace(medium.Id))
					continue;

				if (medium.Id[0] != prefix)
					continue;

				if (medium.Id.Length <= 1)
					continue;

				if (int.TryParse(medium.Id.Substring(1), out int num))
				{
					if (num > maxNumber)
						maxNumber = num;
				}
			}

			int next = maxNumber + 1;
			return $"{prefix}{next:000}";
		}

		// --- Commands: Hinzufügen ---
		private bool KannHinzufuegen()
		{
			if (!IsBibliothekar)
				return false;

			if (string.IsNullOrWhiteSpace(SelectedMediaType) ||
				string.IsNullOrWhiteSpace(NeuerTitel) ||
				string.IsNullOrWhiteSpace(NeuerAutor) ||
				string.IsNullOrWhiteSpace(NeueId))
			{
				return false;
			}

			switch (SelectedMediaType)
			{
				case "Buch":
				case "Software":
					return !string.IsNullOrWhiteSpace(Zusatz1);

				case "DVD":
					return !string.IsNullOrWhiteSpace(Zusatz1) &&
						   int.TryParse(Zusatz1, out _);

				default:
					return false;
			}
		}

		private void FuegeMediumHinzu()
		{
			if (!IsBibliothekar)
			{
				_dialogService.ShowWarning("Nur Bibliothekare dürfen neue Medien anlegen.", "Nicht erlaubt");
				return;
			}

			try
			{
				if (string.IsNullOrWhiteSpace(SelectedMediaType))
					throw new InvalidOperationException("Es wurde kein Medientyp ausgewählt.");

				if (string.IsNullOrWhiteSpace(NeuerTitel) ||
					string.IsNullOrWhiteSpace(NeuerAutor) ||
					string.IsNullOrWhiteSpace(NeueId))
				{
					throw new InvalidOperationException("Bitte alle Pflichtfelder ausfüllen.");
				}

				Medien? medium = SelectedMediaType switch
				{
					"Buch" => new Buch(NeuerTitel!, NeuerAutor!, Zusatz1 ?? string.Empty, NeueId!),
					"DVD" => int.TryParse(Zusatz1, out int dauer)
						? new DVD(NeuerTitel!, NeuerAutor!, dauer, NeueId!)
						: throw new FormatException("Dauer muss eine Zahl sein."),
					"Software" => new Software(NeuerTitel!, NeuerAutor!, Zusatz1 ?? string.Empty, NeueId!),
					_ => null
				};

				if (medium == null)
					throw new InvalidOperationException("Ungültiger Medientyp.");

				_bibliothek.AddMedium(medium);
				_dialogService.ShowInfo($"Medium '{medium.Titel}' wurde hinzugefügt.", "Erfolg");

				ClearAddFields();
				UpdateNeueId();
			}
			catch (Exception ex)
			{
				_dialogService.ShowError($"Fehler beim Hinzufügen: {ex.Message}", "Fehler");
			}
			finally
			{
				AktualisiereAlleFilter();
			}
		}

		private void ClearAddFields()
		{
			NeuerTitel = string.Empty;
			NeuerAutor = string.Empty;
			Zusatz1 = string.Empty;
			UpdateNeueId();
		}

		// --- Commands: Entfernen ---
		private bool KannEntfernen()
		{
			if (!IsBibliothekar)
				return false;

			return SelectedMediumToRemove != null && !SelectedMediumToRemove.IstVerliehen;
		}

		private void EntferneMedium()
		{
			if (!IsBibliothekar)
			{
				_dialogService.ShowWarning("Nur Bibliothekare dürfen Medien löschen.", "Nicht erlaubt");
				return;
			}

			var medium = SelectedMediumToRemove;
			if (medium == null)
				return;

			if (medium.IstVerliehen)
			{
				_dialogService.ShowWarning(
					"Das Medium ist aktuell verliehen und kann nicht gelöscht werden.",
					"Löschen nicht möglich");
			}
			else
			{
				_bibliothek.RemoveMedium(medium);
				_dialogService.ShowInfo($"Medium '{medium.Titel}' wurde gelöscht.", "Erfolg");

				SelectedMediumToRemove = null;
				AktualisiereAlleFilter();
			}
		}

		private void ClearRemoveSelection()
		{
			SelectedMediumToRemove = null;
			SearchText = string.Empty;
			AktualisiereAlleFilter();
		}

		// --- Verleihstatus ---
		private void ToggleVerliehen(Medien? medium)
		{
			if (medium == null)
				return;

			if (!IsBibliothekar)
			{
				_dialogService.ShowWarning("Nur Bibliothekare dürfen den Verleihstatus ändern.", "Nicht erlaubt");
				return;
			}

			if (!medium.IstVerliehen)
			{
				string verliehenAn = !string.IsNullOrWhiteSpace(medium.ReserviertVon)
					? medium.ReserviertVon!
					: "Unbekannt";

				_bibliothek.VerleiheMedium(medium, verliehenAn);
			}
			else
			{
				_bibliothek.NimmMediumZurueck(medium);
			}

			AktualisiereAlleFilter();
		}

		// --- Reservierungsstatus ---
		private bool CanToggleReserviert(Medien? medium)
		{
			if (medium == null)
				return false;

			if (AktuellerBenutzer == null)
				return false;

			if (IsBibliothekar)
				return true;

			if (medium.IstVerliehen)
				return false;

			if (!medium.IstReserviert)
			{
				return _bibliothek.KannGastReservieren(AktuellerBenutzer.Benutzername);
			}

			return string.Equals(medium.ReserviertVon, AktuellerBenutzer.Benutzername, StringComparison.OrdinalIgnoreCase);
		}

		private void ToggleReserviert(Medien? medium)
		{
			if (medium == null)
				return;

			if (AktuellerBenutzer == null)
			{
				_dialogService.ShowWarning("Kein Benutzer angemeldet.", "Aktion nicht möglich");
				return;
			}

			if (IsGast && medium.IstVerliehen)
			{
				_dialogService.ShowWarning(
					"Als Gast können Sie den Status eines verliehenen Mediums nicht ändern.",
					"Nicht erlaubt");
				return;
			}

			if (medium.IstReserviert)
			{
				if (IsBibliothekar ||
					string.Equals(medium.ReserviertVon, AktuellerBenutzer.Benutzername, StringComparison.OrdinalIgnoreCase))
				{
					_bibliothek.HebeReservierungAuf(medium);
				}
				else
				{
					_dialogService.ShowWarning(
						"Dieses Medium ist bereits von einem anderen Benutzer reserviert.",
						"Reservierung nicht möglich");
					return;
				}
			}
			else
			{
				if (IsGast && !_bibliothek.KannGastReservieren(AktuellerBenutzer.Benutzername))
				{
					_dialogService.ShowWarning(
						"Gäste dürfen maximal 5 Medien reservieren.",
						"Reservierungslimit erreicht");
					return;
				}

				if (medium.IstVerliehen)
				{
					_dialogService.ShowWarning(
						"Dieses Medium ist bereits verliehen und kann nicht reserviert werden.",
						"Reservierung nicht möglich");
					return;
				}

				_bibliothek.ReserviereMedium(medium, AktuellerBenutzer.Benutzername);
			}

			AktualisiereAlleFilter();
		}

		// --- Benutzerverwaltung öffnen (nur Admin) ---
		private void OpenUserManagement()
		{
			try
			{
				var window = new UserManagementWindow
				{
					Owner = Application.Current.MainWindow
				};
				window.ShowDialog();
			}
			catch (Exception ex)
			{
				_dialogService.ShowError($"Fehler beim Öffnen der Benutzerverwaltung: {ex.Message}", "Fehler");
			}
		}
	}
}

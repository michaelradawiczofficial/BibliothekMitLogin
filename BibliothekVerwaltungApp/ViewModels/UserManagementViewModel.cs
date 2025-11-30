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

namespace BibliothekVerwaltungApp.ViewModels
{
	public class UserManagementViewModel : NotifyPropertyChangedBase
	{
		private readonly BenutzerRepository _benutzerRepository;
		private readonly IDialogService _dialogService;

		public ObservableCollection<Benutzer> BenutzerListe { get; } = new();

		private Benutzer? _ausgewaehlterBenutzer;
		public Benutzer? AusgewaehlterBenutzer
		{
			get => _ausgewaehlterBenutzer;
			set
			{
				if (SetProperty(ref _ausgewaehlterBenutzer, value))
				{
					LadeBenutzerInForm(value);
					AktualisiereCommandZustaende();
				}
			}
		}

		private string? _bearbeiteterBenutzername;
		public string? BearbeiteterBenutzername
		{
			get => _bearbeiteterBenutzername;
			set
			{
				if (SetProperty(ref _bearbeiteterBenutzername, value))
				{
					AktualisiereCommandZustaende();
				}
			}
		}

		public ObservableCollection<BenutzerRolle> VerfuegbareRollen { get; }

		private BenutzerRolle _ausgewaehlteRolle = BenutzerRolle.Gast;
		public BenutzerRolle AusgewaehlteRolle
		{
			get => _ausgewaehlteRolle;
			set => SetProperty(ref _ausgewaehlteRolle, value);
		}

		private string? _neuesPasswort;
		public string? NeuesPasswort
		{
			get => _neuesPasswort;
			set
			{
				if (SetProperty(ref _neuesPasswort, value))
				{
					AktualisiereCommandZustaende();
				}
			}
		}

		private string? _suchtext;
		public string? Suchtext
		{
			get => _suchtext;
			set
			{
				if (SetProperty(ref _suchtext, value))
				{
					RefreshFilter();
				}
			}
		}

		// Commands
		public ICommand NeuerBenutzerCommand { get; }
		public ICommand SpeichernBenutzerCommand { get; }
		public ICommand LoeschenBenutzerCommand { get; }
		public ICommand PasswortSetzenCommand { get; }
		public ICommand SchliessenCommand { get; }

		/// <summary>
		/// Wird vom Fenster abonniert, um das Window zu schließen.
		/// </summary>
		public event Action? RequestClose;

		public UserManagementViewModel(BenutzerRepository benutzerRepository, IDialogService dialogService)
		{
			_benutzerRepository = benutzerRepository ?? throw new ArgumentNullException(nameof(benutzerRepository));
			_dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

			VerfuegbareRollen = new ObservableCollection<BenutzerRolle>(
				Enum.GetValues(typeof(BenutzerRolle)).Cast<BenutzerRolle>());

			NeuerBenutzerCommand = new RelayCommand(_ => NeuerBenutzer(), _ => true);
			SpeichernBenutzerCommand = new RelayCommand(_ => SpeichernBenutzer(), _ => KannSpeichern());
			LoeschenBenutzerCommand = new RelayCommand(_ => LoeschenBenutzer(), _ => AusgewaehlterBenutzer != null);
			PasswortSetzenCommand = new RelayCommand(_ => PasswortSetzen(), _ => KannPasswortSetzen());
			SchliessenCommand = new RelayCommand(_ => RequestClose?.Invoke());

			LadeBenutzer();
			InitialisiereFilter();
		}

		// -------------------------------------------------------
		// Laden & Filtern
		// -------------------------------------------------------

		private void LadeBenutzer()
		{
			BenutzerListe.Clear();
			foreach (var ben in _benutzerRepository.AlleBenutzer.OrderBy(b => b.Benutzername))
			{
				BenutzerListe.Add(ben);
			}
		}

		private void InitialisiereFilter()
		{
			ICollectionView view = CollectionViewSource.GetDefaultView(BenutzerListe);
			view.Filter = FilterBenutzer;
		}

		private bool FilterBenutzer(object? obj)
		{
			if (obj is not Benutzer benutzer)
				return false;

			if (string.IsNullOrWhiteSpace(Suchtext))
				return true;

			var such = Suchtext.Trim();

			return benutzer.Benutzername.IndexOf(such, StringComparison.OrdinalIgnoreCase) >= 0
				   || benutzer.Rolle.ToString().IndexOf(such, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private void RefreshFilter()
		{
			ICollectionView view = CollectionViewSource.GetDefaultView(BenutzerListe);
			view.Refresh();
		}

		// -------------------------------------------------------
		// Form-Befüllung
		// -------------------------------------------------------

		private void LadeBenutzerInForm(Benutzer? benutzer)
		{
			if (benutzer == null)
			{
				BearbeiteterBenutzername = string.Empty;
				AusgewaehlteRolle = BenutzerRolle.Gast;
				NeuesPasswort = string.Empty;
			}
			else
			{
				BearbeiteterBenutzername = benutzer.Benutzername;
				AusgewaehlteRolle = benutzer.Rolle;
				NeuesPasswort = string.Empty;
			}
		}

		// -------------------------------------------------------
		// Commands: Neu, Speichern, Löschen, Passwort setzen
		// -------------------------------------------------------

		private void NeuerBenutzer()
		{
			AusgewaehlterBenutzer = null;
			BearbeiteterBenutzername = string.Empty;
			AusgewaehlteRolle = BenutzerRolle.Gast;
			NeuesPasswort = string.Empty;
		}

		private bool KannSpeichern()
		{
			return !string.IsNullOrWhiteSpace(BearbeiteterBenutzername);
		}

		private void SpeichernBenutzer()
		{
			try
			{
				if (string.IsNullOrWhiteSpace(BearbeiteterBenutzername))
				{
					_dialogService.ShowWarning("Der Benutzername darf nicht leer sein.", "Benutzer speichern");
					return;
				}

				if (AusgewaehlterBenutzer == null)
				{
					// Neuer Benutzer
					if (string.IsNullOrWhiteSpace(NeuesPasswort))
					{
						_dialogService.ShowWarning("Bitte ein Passwort für den neuen Benutzer vergeben.", "Neuer Benutzer");
						return;
					}

					var neuerBenutzer = new Benutzer(BearbeiteterBenutzername, NeuesPasswort!, AusgewaehlteRolle);
					_benutzerRepository.AddBenutzer(neuerBenutzer);
					LadeBenutzer();
					// Den neu angelegten Benutzer auswählen
					AusgewaehlterBenutzer = BenutzerListe.FirstOrDefault(b =>
						string.Equals(b.Benutzername, neuerBenutzer.Benutzername, StringComparison.OrdinalIgnoreCase));
					_dialogService.ShowInfo($"Benutzer '{neuerBenutzer.Benutzername}' wurde angelegt.", "Benutzer angelegt");
				}
				else
				{
					// Bestehenden Benutzer aktualisieren (Name + Rolle)
					string alterName = AusgewaehlterBenutzer.Benutzername;

					var updated = new Benutzer(BearbeiteterBenutzername!, AusgewaehlterBenutzer.Passwort, AusgewaehlteRolle);
					_benutzerRepository.UpdateBenutzer(alterName, updated);

					LadeBenutzer();
					AusgewaehlterBenutzer = BenutzerListe.FirstOrDefault(b =>
						string.Equals(b.Benutzername, updated.Benutzername, StringComparison.OrdinalIgnoreCase));

					_dialogService.ShowInfo($"Benutzer '{alterName}' wurde aktualisiert.", "Benutzer aktualisiert");
				}
			}
			catch (Exception ex)
			{
				_dialogService.ShowError($"Fehler beim Speichern des Benutzers: {ex.Message}", "Fehler");
			}
			finally
			{
				NeuesPasswort = string.Empty;
				AktualisiereCommandZustaende();
			}
		}

		private bool KannPasswortSetzen()
		{
			return AusgewaehlterBenutzer != null &&
				   !string.IsNullOrWhiteSpace(NeuesPasswort);
		}

		private void PasswortSetzen()
		{
			if (AusgewaehlterBenutzer == null)
			{
				_dialogService.ShowWarning("Bitte zuerst einen Benutzer auswählen.", "Passwort setzen");
				return;
			}

			if (string.IsNullOrWhiteSpace(NeuesPasswort))
			{
				_dialogService.ShowWarning("Das Passwort darf nicht leer sein.", "Passwort setzen");
				return;
			}

			try
			{
				_benutzerRepository.ChangePasswort(AusgewaehlterBenutzer.Benutzername, NeuesPasswort!);
				LadeBenutzer();
				AusgewaehlterBenutzer = BenutzerListe.FirstOrDefault(b =>
					string.Equals(b.Benutzername, BearbeiteterBenutzername, StringComparison.OrdinalIgnoreCase));

				_dialogService.ShowInfo("Das Passwort wurde aktualisiert.", "Passwort gesetzt");
			}
			catch (Exception ex)
			{
				_dialogService.ShowError($"Fehler beim Setzen des Passworts: {ex.Message}", "Fehler");
			}
			finally
			{
				NeuesPasswort = string.Empty;
				AktualisiereCommandZustaende();
			}
		}

		private void LoeschenBenutzer()
		{
			if (AusgewaehlterBenutzer == null)
			{
				return;
			}

			// Bestätigung via MessageBox statt DialogService.ShowConfirmationDialog
			var result = MessageBox.Show(
				$"Möchten Sie den Benutzer '{AusgewaehlterBenutzer.Benutzername}' wirklich löschen?",
				"Benutzer löschen",
				MessageBoxButton.YesNo,
				MessageBoxImage.Question);

			if (result != MessageBoxResult.Yes)
				return;

			try
			{
				string name = AusgewaehlterBenutzer.Benutzername;
				_benutzerRepository.RemoveBenutzer(name);
				LadeBenutzer();
				AusgewaehlterBenutzer = null;
				_dialogService.ShowInfo($"Benutzer '{name}' wurde gelöscht.", "Benutzer gelöscht");
			}
			catch (Exception ex)
			{
				_dialogService.ShowError($"Fehler beim Löschen des Benutzers: {ex.Message}", "Fehler");
			}
			finally
			{
				AktualisiereCommandZustaende();
			}
		}

		// -------------------------------------------------------
		// Hilfsfunktionen
		// -------------------------------------------------------

		private void AktualisiereCommandZustaende()
		{
			(SpeichernBenutzerCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(LoeschenBenutzerCommand as RelayCommand)?.RaiseCanExecuteChanged();
			(PasswortSetzenCommand as RelayCommand)?.RaiseCanExecuteChanged();
		}
	}
}

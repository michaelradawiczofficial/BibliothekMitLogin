using System;
using System.Windows.Input;
using BibliothekVerwaltung.Core.Models;
using BibliothekVerwaltungApp.Commands;

namespace BibliothekVerwaltungApp.ViewModels
{
	public class LoginViewModel : NotifyPropertyChangedBase
	{
		private readonly BenutzerRepository _benutzerRepository;

		private string _benutzername = string.Empty;
		private string _passwort = string.Empty;
		private string? _fehlermeldung;

		/// <summary>
		/// Wird nach erfolgreichem Login gesetzt und kann vom Aufrufer ausgelesen werden.
		/// </summary>
		public Benutzer? AngemeldeterBenutzer { get; private set; }

		/// <summary>
		/// Benutzername, der im Login-Fenster eingegeben wird.
		/// </summary>
		public string Benutzername
		{
			get => _benutzername;
			set
			{
				if (SetProperty(ref _benutzername, value))
				{
					(CanExecuteAnmeldenCommand as RelayCommand)?.RaiseCanExecuteChanged();
				}
			}
		}

		/// <summary>
		/// Passwort, das im Login-Fenster eingegeben wird.
		/// (In diesem Übungsprojekt als einfacher Text.)
		/// </summary>
		public string Passwort
		{
			get => _passwort;
			set
			{
				if (SetProperty(ref _passwort, value))
				{
					(CanExecuteAnmeldenCommand as RelayCommand)?.RaiseCanExecuteChanged();
				}
			}
		}

		/// <summary>
		/// Fehlermeldung bei ungültigen Anmeldedaten.
		/// </summary>
		public string? Fehlermeldung
		{
			get => _fehlermeldung;
			set => SetProperty(ref _fehlermeldung, value);
		}

		/// <summary>
		/// Command zum Anmelden.
		/// </summary>
		public ICommand AnmeldenCommand { get; }

		/// <summary>
		/// Command zum Abbrechen.
		/// </summary>
		public ICommand AbbrechenCommand { get; }

		/// <summary>
		/// Wird vom View (LoginWindow) abonniert, um den Dialog zu schließen.
		/// Parameter: true = erfolgreich, false = abgebrochen.
		/// </summary>
		public event Action<bool?>? RequestClose;

		public LoginViewModel()
			: this(new BenutzerRepository())
		{
		}

		public LoginViewModel(BenutzerRepository benutzerRepository)
		{
			_benutzerRepository = benutzerRepository ?? throw new ArgumentNullException(nameof(benutzerRepository));

			// Optional: Default-Werte, damit man "schnell" testen kann
			Benutzername = "admin";
			Passwort = "admin";

			AnmeldenCommand = new RelayCommand(
				_ => Anmelden(),
				_ => CanExecuteAnmelden());

			AbbrechenCommand = new RelayCommand(
				_ => Abbrechen());
		}

		private bool CanExecuteAnmelden()
		{
			return !string.IsNullOrWhiteSpace(Benutzername) &&
				   !string.IsNullOrWhiteSpace(Passwort);
		}

		private RelayCommand? CanExecuteAnmeldenCommand =>
			AnmeldenCommand as RelayCommand;

		private void Anmelden()
		{
			Fehlermeldung = string.Empty;

			if (!_benutzerRepository.TryAuthenticate(Benutzername, Passwort, out var benutzer))
			{
				Fehlermeldung = "Benutzername oder Passwort ist ungültig.";
				AngemeldeterBenutzer = null;
				return;
			}

			AngemeldeterBenutzer = benutzer;
			RequestClose?.Invoke(true);
		}

		private void Abbrechen()
		{
			AngemeldeterBenutzer = null;
			RequestClose?.Invoke(false);
		}
	}
}

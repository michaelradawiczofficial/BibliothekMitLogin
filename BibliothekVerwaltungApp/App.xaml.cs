using System;
using System.Windows;
using BibliothekVerwaltung.Core.Models;
using BibliothekVerwaltungApp.Services;
using BibliothekVerwaltungApp.ViewModels;
using BibliothekVerwaltungApp.Views;

namespace BibliothekVerwaltungApp
{
	public partial class App : Application
	{
		// Globale Bibliothek – wird im gesamten Programm verwendet
		public static Bibliothek GlobaleBibliothek { get; } = new Bibliothek();

		private MainViewModel? _mainViewModel;

		/// <summary>
		/// Einstiegspunkt der Anwendung (wird über Startup="Application_Startup" in App.xaml aufgerufen).
		/// Hier bauen wir jetzt den Login ein und starten danach das Hauptfenster.
		/// </summary>
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			// Sicherstellen, dass die App nicht automatisch beendet,
			// wenn das Login-Fenster geschlossen wird:
			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			// 1. Login-Fenster anzeigen
			var loginWindow = new LoginWindow();
			bool? dialogResult = loginWindow.ShowDialog();

			// Wenn der Dialog nicht mit "erfolgreich" (true) geschlossen wurde -> Anwendung beenden
			if (dialogResult != true)
			{
				Shutdown();
				return;
			}

			// Angemeldeten Benutzer aus dem ViewModel auslesen
			var loginViewModel = loginWindow.ViewModel;
			Benutzer? angemeldeterBenutzer = loginViewModel.AngemeldeterBenutzer;

			if (angemeldeterBenutzer == null)
			{
				// Sollte theoretisch nicht passieren, aber zur Sicherheit:
				Shutdown();
				return;
			}

			// 2. DialogService und MainViewModel erzeugen
			var dialogService = new DialogService();
			_mainViewModel = new MainViewModel(App.GlobaleBibliothek, dialogService);
			_mainViewModel.SetAngemeldeterBenutzer(angemeldeterBenutzer);

			// 3. Hauptfenster erstellen und DataContext setzen
			var mainWindow = new MainWindow
			{
				DataContext = _mainViewModel
			};

			// WICHTIG: Jetzt MainWindow registrieren und ShutdownMode umstellen,
			// damit die App erst beim Schließen des Hauptfensters beendet wird.
			this.MainWindow = mainWindow;
			this.ShutdownMode = ShutdownMode.OnMainWindowClose;

			// 4. Hauptfenster anzeigen
			mainWindow.Show();
		}

		/// <summary>
		/// Beim Beenden speichern
		/// </summary>
		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			try
			{
				App.GlobaleBibliothek.SpeichereDaten();
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Fehler beim Speichern der Bibliotheksdaten:\n{ex.Message}",
					"Fehler beim Beenden",
					MessageBoxButton.OK,
					MessageBoxImage.Warning);
			}
		}
	}
}

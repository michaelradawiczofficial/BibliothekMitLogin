using System;
using System.Windows;
using BibliothekVerwaltungApp.ViewModels;

namespace BibliothekVerwaltungApp.Views
{
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();

			var viewModel = new LoginViewModel();
			DataContext = viewModel;

			viewModel.RequestClose += OnRequestClose;
		}

		private void OnRequestClose(bool? dialogResult)
		{
			// DialogResult setzen, damit ShowDialog() im Aufrufer entsprechend reagieren kann.
			DialogResult = dialogResult;

			// Fenster schließen
			Close();
		}

		/// <summary>
		/// Gibt das aktuell gesetzte ViewModel zurück.
		/// Der Aufrufer kann darüber den AngemeldetenBenutzer auslesen.
		/// </summary>
		public LoginViewModel ViewModel => DataContext as LoginViewModel ?? throw new InvalidOperationException("DataContext ist nicht LoginViewModel.");
	}
}

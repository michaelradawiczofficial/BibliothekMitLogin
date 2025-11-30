using System;
using System.Windows;
using BibliothekVerwaltung.Core.Models;
using BibliothekVerwaltungApp.Services;
using BibliothekVerwaltungApp.ViewModels;

namespace BibliothekVerwaltungApp.Views
{
	public partial class UserManagementWindow : Window
	{
		public UserManagementWindow()
		{
			InitializeComponent();

			var benutzerRepository = new BenutzerRepository();
			var dialogService = new DialogService();

			var viewModel = new UserManagementViewModel(benutzerRepository, dialogService);
			viewModel.RequestClose += OnRequestClose;

			DataContext = viewModel;
		}

		private void OnRequestClose()
		{
			Close();
		}

		/// <summary>
		/// Gibt das aktuell gesetzte ViewModel zurück.
		/// </summary>
		public UserManagementViewModel ViewModel =>
			DataContext as UserManagementViewModel
			?? throw new InvalidOperationException("DataContext ist nicht UserManagementViewModel.");
	}
}

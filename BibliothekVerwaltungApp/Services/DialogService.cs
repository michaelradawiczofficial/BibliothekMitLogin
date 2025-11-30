using System.Windows;

namespace BibliothekVerwaltungApp.Services
{
	public class DialogService : IDialogService
	{
		public void ShowInfo(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public void ShowWarning(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		public void ShowError(string message, string title)
		{
			MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}

using System.Windows;

namespace BibliothekVerwaltungApp
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			// Wichtig: kein DataContext = new MainViewModel(); hier setzen,
			// das überschreibt die vom App-Startup gesetzte Instanz.
			// DataContext wird in App.OnStartup gesetzt.
		}
	}
}

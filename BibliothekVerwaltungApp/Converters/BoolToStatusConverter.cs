using System;
using System.Globalization;
using System.Windows.Data;

namespace BibliothekVerwaltungApp.Converters
{
	public class BoolToStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool b)
				return b ? "verliehen" : "verfügbar";
			return "unbekannt";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}


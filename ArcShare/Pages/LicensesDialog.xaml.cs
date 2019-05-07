using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcShare.Pages
{
	public sealed partial class LicensesDialog : ContentDialog
	{
		public LicensesDialog()
		{
			this.InitializeComponent();

			this.Title = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString("LicensesText");

			this.Loaded += async (sender, args) =>
			{
				var textFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///LICENSES.txt"));
				string content = await FileIO.ReadTextAsync(textFile);

				textBody.Text = content;
			};
		}
	}
}

using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Storage.AccessCache;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcShare.Pages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SettingsPage : Page
	{
		bool flag = false;
		ResourceLoader loader = null;
		public SettingsPage()
		{
			this.InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			loader = ResourceLoader.GetForCurrentView();

			switch (AppSettings.AppTheme)
			{
				case ApplicationTheme.Dark:
					RadioButtonDark.IsChecked = true;
					break;
				case ApplicationTheme.Light:
					RadioButtonLight.IsChecked = true;
					break;
				default:
					RadioButtonSystem.IsChecked = true;
					break;
			}

			if (AppSettings.DisplayLanguage == "zh-CN") LangComboBox.SelectedIndex = 0;
			else LangComboBox.SelectedIndex = 1;
			flag = true;

			portTextBox.Text = AppSettings.PreferredPort.ToString();

			if (AppSettings.ReceiveFolder == null)
			{
				try
				{
					AppSettings.ReceiveFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("ReceiveFolderToken");
				}
				catch(System.IO.FileNotFoundException)
				{
					AppSettings.ReceiveFolder = await DownloadsFolder.CreateFolderAsync("Received", CreationCollisionOption.GenerateUniqueName);
					StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReceiveFolderToken", AppSettings.ReceiveFolder);
				}
			}
			if (AppSettings.ReceiveFolder != null)
			{
				receivingDirectoryBox.Text = AppSettings.ReceiveFolder.Path;
			}

			if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
				this.feedbackButton.Visibility = Visibility.Visible;
		}


		#region THEME
		private void ModeRadioButton_Checked(object sender, RoutedEventArgs e)
		{
			if (this == null) return;
			switch ((sender as RadioButton).Tag.ToString())
			{
				case "0":
					AppSettings.AppTheme = ApplicationTheme.Light;
					break;
				case "1":
					AppSettings.AppTheme = ApplicationTheme.Dark;
					break;
				case "2":
					AppSettings.AppTheme = null;
					break;
			}
		}
		#endregion
		#region LANGUAGE
		private void LangComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (flag)
			{ ////是用户的行为
				var selected = (LangComboBox.SelectedItem as TextBlock).Tag.ToString();
				Windows.ApplicationModel.Resources.Core.ResourceContext.SetGlobalQualifierValue("Language", selected);
				AppSettings.DisplayLanguage = selected;
			}
		}
		#endregion
		#region PORT
		private async void portTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
		{
			bool flag = true;
			if (e.Key >= Windows.System.VirtualKey.Number0 && e.Key <= Windows.System.VirtualKey.Number9) flag = false;
			if (e.Key >= Windows.System.VirtualKey.NumberPad0 && e.Key <= Windows.System.VirtualKey.NumberPad9) flag = false;

			if (flag == false)
			{
				int key = ((int)e.Key - 48 < 10) ? (int)e.Key - 48 : (int)e.Key - 96;
				uint portN = Convert.ToUInt32(portTextBox.Text + key.ToString());
				if (portN > ushort.MaxValue || portN == 0)
				{
					flag = true;
					MessageDialog md = new MessageDialog(loader.GetString("PortRangeWarning"), loader.GetString("Warning"));
					await md.ShowAsync();
				}
			}
			e.Handled = flag;
			return;
		}

		private void portTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!string.IsNullOrEmpty(portTextBox.Text))
			{
				uint portN = Convert.ToUInt32((sender as TextBox).Text);
				if (portN > ushort.MaxValue || portN < 7000) return;
				else
				{
					AppSettings.PreferredPort = (ushort)portN;
				}
			}
		}
		#endregion

		#region RECEIVING
		private async void browseDirectory_Click(object sender, RoutedEventArgs e)
		{
			FolderPicker picker = new FolderPicker();
			picker.SuggestedStartLocation = PickerLocationId.Downloads;
			picker.FileTypeFilter.Add("*");

			StorageFolder folder = await picker.PickSingleFolderAsync();
			if (folder != null)
			{
				StorageApplicationPermissions.FutureAccessList.AddOrReplace("ReceiveFolderToken", folder);
				this.receivingDirectoryBox.Text = folder.Path;
				AppSettings.ReceiveFolder = folder;
			}
		}


		#endregion

		#region FOOTER
		public string GetAppVersion()
		{
			Package package = Package.Current;
			PackageId packageId = package.Id;
			PackageVersion version = packageId.Version;
			return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
		}

		private async void licensesButton_Click(object sender, RoutedEventArgs e)
		{
			LicensesDialog ld = new LicensesDialog();
			await ld.ShowAsync();
		}

		private async void contactButton_Click(object sender, RoutedEventArgs e)
		{
			var uri = new Uri("mailto:i@copperion.com");
			await Windows.System.Launcher.LaunchUriAsync(uri);
		}

		private async void feedbackButton_Click(object sender, RoutedEventArgs e)
		{
			var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
			await launcher.LaunchAsync();
		}

		#endregion

		private void openFolder_Click(object sender, RoutedEventArgs e)
		{
			Windows.System.Launcher.LaunchFolderAsync(AppSettings.ReceiveFolder);
		}
	}
}

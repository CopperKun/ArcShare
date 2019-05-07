using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using QRCoder;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.ApplicationModel.Resources;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcShare.Pages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class RunningPage : Page
	{
		string address;
		SystemNavigationManager manager;
		ResourceLoader loader;

		public RunningPage()
		{
			this.InitializeComponent();

			//KeyboardAccelerator GoBack = new KeyboardAccelerator();
			//GoBack.Key = Windows.System.VirtualKey.Escape;
			//GoBack.Invoked += (sender, args) => { OnBackRequested(); args.Handled = true; };
			//this.KeyboardAccelerators.Add(GoBack);

			//BackButton.Click += (sender, args) => OnBackRequested();
			manager = SystemNavigationManager.GetForCurrentView();
			manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
			manager.BackRequested += (s,a) => OnBackRequested();

			loader = ResourceLoader.GetForCurrentView();
		}

		private async Task<bool> OnBackRequested()
		{
			if (this.Frame.CanGoBack)
			{
				
				ContentDialog cd = new ContentDialog()
				{
					Title = loader.GetString("Warning"),
					Content = loader.GetString("BackWarningContent"),
					PrimaryButtonText = loader.GetString("Yes"),
					CloseButtonText = loader.GetString("No"),
					DefaultButton = ContentDialogButton.Close
				};
				ContentDialogResult result = await cd.ShowAsync();
				if (result == ContentDialogResult.Primary)
				{
					AppSettings.CurrentServer.Dispose();
					manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
					this.Frame.GoBack();
					return true;
				}
			}
			return false;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			address = AppSettings.CurrentServer.ServerAddress;
			QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
			QRCodeData codeData = qRCodeGenerator.CreateQrCode(address, QRCoder.QRCodeGenerator.ECCLevel.Q);
			BitmapByteQRCode bitmapByteQRCode = new BitmapByteQRCode(codeData);
			BitmapImage qrimage = await GetBitmapAsync(bitmapByteQRCode.GetGraphic(10));

			this.addressQRCodeImage.Source = qrimage;

			AppSettings.CurrentServer.Start();

			urlText.Text = address;
			networkText.Text = AppSettings.CurrentServer.GetNetworkName();

			statusText.Text = loader.GetString("Running");
		}


		/// <summary>
		/// https://stackoverflow.com/questions/42523593/convert-byte-to-windows-ui-xaml-media-imaging-bitmapimage
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private async Task<BitmapImage> GetBitmapAsync(byte[] data)
		{
			var bitmapImage = new BitmapImage();

			using (var stream = new InMemoryRandomAccessStream())
			{
				using (var writer = new DataWriter(stream))
				{
					writer.WriteBytes(data);
					await writer.StoreAsync();
					await writer.FlushAsync();
					writer.DetachStream();
				}

				stream.Seek(0);
				await bitmapImage.SetSourceAsync(stream);
			}

			return bitmapImage;
		}
	}
}

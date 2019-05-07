using ArcShare.Pages;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.Storage;
using ArcShare.Server;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ArcShare
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			this.InitializeComponent();
			this.Loaded += (sender, args) =>
			{
				homeFrame.Navigate(typeof(InstrumentPage));
				settingsFrame.Navigate(typeof(SettingsPage));

				//AppSettings.CurrentServer = new HttpServer(4000);
				//AppSettings.CurrentServer.Start();
				sendPage.MainFrame = this.Frame;
				receivePage.MainFrame = this.Frame;
			};
		}
	}
}

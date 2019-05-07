using ArcShare.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcShare.Pages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ReceivePage : Page
	{
		public Frame MainFrame;

		public ReceivePage() => this.InitializeComponent();

		private async void startButton_Click(object sender, RoutedEventArgs e)
		{
			this.IsEnabled = false;
			var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
			//生成主页
			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile indexGenerateFile = await storageFolder.CreateFileAsync("index_r.html", CreationCollisionOption.ReplaceExisting);
			StorageFile indexFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Server/content/receive.html"));
			await indexFile.CopyAndReplaceAsync(indexGenerateFile);
			string content = await FileIO.ReadTextAsync(indexGenerateFile);
			//翻译内容
			content = content.Replace("[FileList]", loader.GetString("FileList")).Replace("[Size]", loader.GetString("Size"))
				.Replace("[Progress]", loader.GetString("Progress")).Replace("[BrowseFiles]", loader.GetString("BrowseFiles"))
				.Replace("[SelectAll]", loader.GetString("SelectAll")).Replace("[CancelSelection]", loader.GetString("CancelSelection"))
				.Replace("[RemoveSelected]", loader.GetString("RemoveSelected")).Replace("[Send]", loader.GetString("Send"))
				.Replace("[DescriptionShort]", loader.GetString("DescriptionShort")).Replace("[GetFromMicrosoft]", loader.GetString("GetFromMicrosoft"));
			//写入文件
			await FileIO.WriteTextAsync(indexGenerateFile, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);
			//开启服务器
			HttpServer server = new HttpServer(AppSettings.PreferredPort, indexGenerateFile);
			AppSettings.CurrentServer = server;
			this.MainFrame.Navigate(typeof(RunningPage), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
			this.IsEnabled = true;
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				await RefreshList();
			}
			catch(System.InvalidOperationException ex1)
			{

			}
			catch(Exception ex2)
			{

			}
			
		}

		private async Task RefreshList()
		{
			var list = await ReceivedFileItem.CreateListAsync();
			foreach (var item in list) fileListView.Items.Add(item);

			if (fileListView.Items.Count != 0) EmptyStateGrid.Visibility = Visibility.Collapsed;
		}

		private async void stackPanel_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
		{
			var temp = (sender as ListView).SelectedItem as ReceivedFileItem;
			bool success = await Windows.System.Launcher.LaunchFileAsync(temp.File);
			if (success) Debug.WriteLine(temp.Name + " launched successfully");
		}
	}


	public class ReceivedFileItem
	{
		public string Name { get; set; }
		public string Token { get; set; }
		public StorageFile File { get; set; }
		public ImageSource Icon { get; set; }

		public ReceivedFileItem() { }

		public static async Task<List<ReceivedFileItem>> CreateListAsync()
		{
			var created = new List<ReceivedFileItem>();
			var list = AppSettings.ReceivedFileTokens;
			foreach (string tok in list)
			{
				try
				{
					var item = new ReceivedFileItem();
					var file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(tok);

					item.File = file;
					item.Token = tok;
					item.Name = file.Name;
					item.Icon = await FileItem.GetFileIconBit(file);

					created.Add(item);
				}
				catch (System.IO.FileNotFoundException)
				{
					Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(tok);
					AppSettings.ReceivedFileTokens.Remove(tok);
					continue;
				}
			}

			return created;
		}
	}
}

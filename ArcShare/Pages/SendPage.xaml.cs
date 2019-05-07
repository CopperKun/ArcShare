using ArcShare.Server;
using System;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcShare.Pages
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SendPage : Page
    {
		public Frame MainFrame;

        public SendPage()
        {
            this.InitializeComponent();

			fileListView.Items.VectorChanged += (sender, args) =>
				EmptyStateGrid.Visibility = (fileListView.Items.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

		private async void addButton_Click(object sender, RoutedEventArgs e)
		{
			//添加文件
			var picker = new FileOpenPicker();
			picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			picker.FileTypeFilter.Add("*");

			var files = await picker.PickMultipleFilesAsync();
			if (files != null)
			{
				foreach (var file in files)
				{
					try
					{
						if (!file.IsAvailable) throw new Exception("The file is not available yet, please download it from the cloud first.");
						if ((await file.GetBasicPropertiesAsync()).Size >= uint.MaxValue) throw new Exception("We are sorry but Arc Share does not support file larger than 4GB.");

						FileItem item = await FileItem.CreateAsync(file);
						fileListView.Items.Add(item);
					}
					catch (Exception ex)
					{
						var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
						var errormsg = loader.GetString("AddFileError");
						var errorword = loader.GetString("Error");
						MessageDialog dia = new MessageDialog(string.Format(errormsg, file.DisplayName, ex.Message), errorword);
						await dia.ShowAsync();
					}
				}
			}
		}

		private void selectAllButton_Click(object sender, RoutedEventArgs e)
		{
			//Select All
			fileListView.SelectAll();
		}

		private void clearSelectionButton_Click(object sender, RoutedEventArgs e)
		{
			fileListView.SelectedItems.Clear();
		}

		private async void deleteButton_Click(object sender, RoutedEventArgs e)
		{
			int count = fileListView.SelectedItems.Count;
			if (count != 0)
			{
				var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
				ContentDialog dialog = new ContentDialog
				{
					Title = loader.GetString("ConfirmRemoveTitle"),
					Content = loader.GetString("ConfirmRemoveContent"),
					PrimaryButtonText = loader.GetString("Remove"),
					CloseButtonText = loader.GetString("Cancel")
				};
				var result = await dialog.ShowAsync();
				if (result == ContentDialogResult.Primary)
				{
					if (count == fileListView.Items.Count)
					{
						//全选了
						fileListView.Items.Clear();
						return;
					}

					//用foreach会出问题
					while (fileListView.SelectedItems.Count != 0)
					{
						fileListView.Items.RemoveAt(fileListView.SelectedIndex);
					}
				}
			}
		}

		private async void sendStartButton_Click(object sender, RoutedEventArgs e)
		{
			var files = fileListView.Items;
			var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
			if (files.Count == 0)
			{
				MessageDialog md = new MessageDialog(loader.GetString("SelectAtLeastOne"), loader.GetString("Warning"));
				await md.ShowAsync();
				return;
			}

			this.IsEnabled = false;
			string html = string.Empty;
			int count = 0;
			foreach (var item in fileListView.Items)
			{
				//生成文件的表格视图
				var file = item as FileItem;
				html += (string.Format("<tr><td>{0}</td><td><a href=\"/get/{0}\">{1}</a></td><td>{2}</td></tr>",
					//html += (string.Format("<tr><td ><a href=\"/get/{0}\">{1}</a></td><td>{2}</td></tr>\n",
					count.ToString(), file.FullName, file.SizeInString));
				count++;
			}
			StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
			StorageFile indexGenerateFile = await storageFolder.CreateFileAsync("index_r.html", CreationCollisionOption.ReplaceExisting);
			StorageFile indexFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Server/content/send.html"));
			await indexFile.CopyAndReplaceAsync(indexGenerateFile);
			string content = await FileIO.ReadTextAsync(indexGenerateFile);
			//翻译内容
			content = content.Replace("[DownloadAsZip]", loader.GetString("DownloadAsZip")).Replace("[ClickToDownload]", loader.GetString("ClickToDownload"))
				.Replace("[Size]", loader.GetString("Size")).Replace("[DescriptionShort]", loader.GetString("DescriptionShort")).Replace("[GetFromMicrosoft]", loader.GetString("GetFromMicrosoft"));
			//插入表格
			content = content.Replace("[TableRowPlaceholder]", html);

			await FileIO.WriteTextAsync(indexGenerateFile, content, Windows.Storage.Streams.UnicodeEncoding.Utf8);

			AppSettings.FileCollection = fileListView.Items.Cast<FileItem>().ToList();

			//Create Server
			HttpServer server = new HttpServer(AppSettings.PreferredPort, indexGenerateFile, fileListView.Items);
			AppSettings.CurrentServer = server;

			this.MainFrame.Navigate(typeof(RunningPage), null, new Windows.UI.Xaml.Media.Animation.DrillInNavigationTransitionInfo());
			this.IsEnabled = true;
		}
	}
}

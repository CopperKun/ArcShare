using ArcShare.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ArcShare
{
	class AppSettings
	{
		static ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

		public static List<FileItem> FileCollection { get; set; }

		private static HttpServer _server;

		public static HttpServer CurrentServer
		{
			get
			{
				return _server;
			}
			set
			{
				if (_server != null) _server.Dispose();
				_server = value;
			}
		}

		public static NavigationViewDisplayMode NavViewMode { get; set; }

		public static ApplicationTheme? AppTheme
		{
			get
			{
				if (localSettings.Values["theme"] == null) return null;
				switch (localSettings.Values["theme"].ToString())
				{
					case "light":
						return ApplicationTheme.Light;
					case "dark":
						return ApplicationTheme.Dark;
				}
				return null;
			}
			set
			{
				switch (value)
				{
					case ApplicationTheme.Light:
						localSettings.Values["theme"] = "light";
						break;
					case ApplicationTheme.Dark:
						localSettings.Values["theme"] = "dark";
						break;
					case null:
						localSettings.Values["theme"] = null;
						break;
				}
			}
		}

		public static string DisplayLanguage
		{
			get
			{
				if (localSettings.Values["lang"] == null)
				{
					var topUserLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
					var language = new Windows.Globalization.Language(topUserLanguage);
					var name = language.LanguageTag;
					if (name != "en-US" && name != "zh-CN") return "en-US";
					else return name;
				}
				return localSettings.Values["lang"].ToString();
			}
			set
			{
				localSettings.Values["lang"] = value;
				Windows.ApplicationModel.Resources.Core.ResourceContext.SetGlobalQualifierValue("Language", value);
			}
		}

		public static ushort PreferredPort
		{
			get
			{
				if (localSettings.Values["port"] == null) return 8000;
				else return Convert.ToUInt16(localSettings.Values["port"]);
			}
			set
			{
				localSettings.Values["port"] = value;
			}
		}

		public static StorageFolder ReceiveFolder { get; set; }

		public static ObservableCollection<string> ReceivedFileTokens { get; set; }

		public static void RegisterListen()
		{
			
			if(localSettings.Values["tokens"] != null)
			{
				string json = localSettings.Values["tokens"].ToString();
				ReceivedFileTokens = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<string>>(json);
			}
			else
			{
				ReceivedFileTokens = new ObservableCollection<string>();
			}

			ReceivedFileTokens.CollectionChanged += ReceivedFileTokens_CollectionChanged;

		}

		private static void ReceivedFileTokens_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			string jsonx = Newtonsoft.Json.JsonConvert.SerializeObject(ReceivedFileTokens);
			localSettings.Values["tokens"] = jsonx;
		}
	}


	public class FileItem
	{
		public ImageSource IconX { get; set; }
		public string FullName { get; set; }
		public string FileType { get; set; }
		public StorageFile File { get; set; }
		public bool IsSelected { get; set; }
		public string SizeInString { get; set; }

		public static async Task<FileItem> CreateAsync(StorageFile f)
		{
			FileItem item = new FileItem(f)
			{
				IconX = await GetFileIconBit(f)
			};
			return item;
		}

		public FileItem(StorageFile file)
		{
			File = file;
			GetFileName();
			GetFileSize();
		}

		private void GetFileName()
		{
			if (File.IsAvailable)
			{
				FullName = string.Format("{0}{1}", File.DisplayName, File.FileType);
				FileType = File.FileType;
			}

		}

		private async void GetFileSize()
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB" };
			ulong len = (await File.GetBasicPropertiesAsync()).Size;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				order++;
				len = len / 1024;
			}

			// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
			// show a single decimal place, and no space.
			SizeInString = String.Format("{0:0.##} {1}", len, sizes[order]);
		}

		public static async Task<ImageSource> GetFileIconBit(StorageFile file, uint size = 64)
		{
			const ThumbnailMode thumbnailMode = ThumbnailMode.DocumentsView;
			StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(thumbnailMode, size, ThumbnailOptions.ResizeThumbnail);


			if (thumbnail == null)  //.psd .ai可能遇到这个情况
			{
				var dummy = await ApplicationData.Current.TemporaryFolder.CreateFileAsync("dummy.fuckingwindows", CreationCollisionOption.ReplaceExisting); //may overwrite existing
				thumbnail = await dummy.GetThumbnailAsync(ThumbnailMode.SingleItem, size, ThumbnailOptions.ResizeThumbnail);
			}

			WriteableBitmap bitmap = new WriteableBitmap((int)size, (int)size);
			bitmap.SetSource(thumbnail);


			string b = await ToBase64(bitmap);
			return await FromBase64(b);
		}



		public static async Task<string> ToBase64(WriteableBitmap bitmap)
		{
			var bytes = bitmap.PixelBuffer.ToArray();
			return await ToBase64(bytes, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight);
		}
		public static async Task<string> ToBase64(byte[] image, uint height, uint width, double dpiX = 96, double dpiY = 96)
		{
			// encode image
			var encoded = new InMemoryRandomAccessStream();
			var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, encoded);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, height, width, dpiX, dpiY, image);
			await encoder.FlushAsync();
			encoded.Seek(0);

			// read bytes
			var bytes = new byte[encoded.Size];
			await encoded.AsStream().ReadAsync(bytes, 0, bytes.Length);

			// create base64
			return Convert.ToBase64String(bytes);
		}

		private static async Task<ImageSource> FromBase64(string base64)
		{
			// read stream
			var bytes = Convert.FromBase64String(base64);
			var image = bytes.AsBuffer().AsStream().AsRandomAccessStream();

			// decode image
			var decoder = await BitmapDecoder.CreateAsync(image);
			image.Seek(0);

			// create bitmap
			var output = new WriteableBitmap((int)decoder.PixelHeight, (int)decoder.PixelWidth);
			await output.SetSourceAsync(image);
			return output;
		}
	}
}

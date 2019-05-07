using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Services.Store.Engagement;
using Windows.Storage.AccessCache;

namespace ArcShare
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
			SetApplicationTheme();
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

		private void SetApplicationTheme()
		{
			Windows.ApplicationModel.Resources.Core.ResourceContext.SetGlobalQualifierValue("Language", AppSettings.DisplayLanguage);
			var theme = AppSettings.AppTheme;
			if (theme == null) return;
			else
				this.RequestedTheme = (ApplicationTheme)theme;
		}


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }

			//draw into the title bar
			var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = true;

			//remove the solid-colored backgrounds behind the caption controls and system back button
			var viewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
			viewTitleBar.ButtonBackgroundColor = Colors.Transparent;
			viewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			viewTitleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];

			//Windows Developer Dashboard Notification
			StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
			engagementManager.RegisterNotificationChannelAsync();

			//Receive Files History List Change
			AppSettings.RegisterListen();
		}

		protected override void OnActivated(IActivatedEventArgs args)
		{
			base.OnActivated(args);

			if (args is ToastNotificationActivatedEventArgs)
			{
				var toastActivationArgs = args as ToastNotificationActivatedEventArgs;

				StoreServicesEngagementManager engagementManager = StoreServicesEngagementManager.GetDefault();
				string originalArgs = engagementManager.ParseArgumentsAndTrackAppLaunch(
					toastActivationArgs.Argument);

				// Use the originalArgs variable to access the original arguments
				// that were passed to the app.
			}
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}

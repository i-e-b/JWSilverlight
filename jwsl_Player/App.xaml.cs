using System;
using System.Windows;
using JwslPlayer;

namespace jwslPlayer {
	public partial class App : Application {

		public App () {
			Startup += Application_Startup;
			Exit += Application_Exit;
			UnhandledException += Application_UnhandledException;

			InitializeComponent();
		}

		private void Application_Startup (object sender, StartupEventArgs e)
        {
            var root = new MainPage();
            RootVisual = root;

			string playSource;
			if (e.InitParams.TryGetValue("playlist", out playSource)) {
				root.SourcePlaylist = playSource;
			} else if (e.InitParams.TryGetValue("file", out playSource)) {
				root.SourcePlaylist = @"<?xml version='1.0' encoding='utf-8' ?>
<Playlist>
	<AutoLoad>true</AutoLoad>
	<AutoPlay>false</AutoPlay>
	<DisplayTimeCode>false</DisplayTimeCode>
	<EnableCachedComposition>true</EnableCachedComposition>
	<EnableCaptions>true</EnableCaptions>
	<EnablePopOut>true</EnablePopOut>
	<EnableOffline>true</EnableOffline>
	<StartMuted>false</StartMuted>
	<StretchMode>Uniform</StretchMode>
	<Items>
		<PlaylistItem>
			<IsAdaptiveStreaming>true</IsAdaptiveStreaming>
			<MediaSource>"+playSource+@"</MediaSource>
			<CaptionUrl>http://localhost:49832/captions_tt.xml</CaptionUrl>
		</PlaylistItem>
	</Items>
</Playlist>";
			}

			string skinPackage;
			if (e.InitParams.TryGetValue("skin", out skinPackage)) {
				root.SkinPackageUrl = Uri.UnescapeDataString(skinPackage);
			} else {
				root.SkinPackageUrl = "/ExampleSkins/beelden.zip";
			}
		}

		private void Application_Exit (object sender, EventArgs e) {

		}
		private void Application_UnhandledException (object sender, ApplicationUnhandledExceptionEventArgs e) {
			// If the app is running outside of the debugger then report the exception using
			// the browser's exception mechanism. On IE this will display it a yellow alert 
			// icon in the status bar and Firefox will display a script error.
			if (!System.Diagnostics.Debugger.IsAttached) {

				// NOTE: This will allow the application to continue running after an exception has been thrown
				// but not handled. 
				// For production applications this error handling should be replaced with something that will 
				// report the error to the website and stop the application.
				e.Handled = true;
				Deployment.Current.Dispatcher.BeginInvoke(() => ReportErrorToDOM(e));
			}
		}
		private void ReportErrorToDOM (ApplicationUnhandledExceptionEventArgs e) {
			try {
				string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
				errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

				System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
			} catch {
			}
		}
	}
}

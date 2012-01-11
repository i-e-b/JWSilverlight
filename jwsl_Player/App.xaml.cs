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

			string autoPlay;
			if (e.InitParams.TryGetValue("autostart", out autoPlay)) {
				if (autoPlay.ToLower() == "true") root.AutoPlay = true;
			}

			string playSource;
			if (e.InitParams.TryGetValue("playlist", out playSource)) {
				root.SourcePlaylist = Uri.UnescapeDataString(playSource);
			} else if (e.InitParams.TryGetValue("playlistfile", out playSource)) {
				root.SourcePlaylist = playSource;
			} else if (e.InitParams.TryGetValue("file", out playSource)) {
				root.SourcePlaylist = "[[JSON]][{file:'" + playSource + "'}]";
			}

			string skinPackage;
			if (e.InitParams.TryGetValue("skin", out skinPackage)) {
				root.SkinPackageUrl = Uri.UnescapeDataString(skinPackage);
			} else {
				root.SkinPackageUrl = "/ExampleSkins/beelden.zip"; // todo: built-in default skin?
			}

			string plistSize;
			if (e.InitParams.TryGetValue("playlist.size", out plistSize)) {
				root.PlaylistSize = double.Parse(plistSize);
			}
		}

		private void Application_Exit (object sender, EventArgs e) {

		}
		private void Application_UnhandledException (object sender, ApplicationUnhandledExceptionEventArgs e) {
			if (!System.Diagnostics.Debugger.IsAttached) {
				e.Handled = true;
				Deployment.Current.Dispatcher.BeginInvoke(() => ReportErrorToDOM(e));
			}
		}
		private void ReportErrorToDOM (ApplicationUnhandledExceptionEventArgs e) {
			try {
				string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
				errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

				System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
			} catch { }
		}
	}
}

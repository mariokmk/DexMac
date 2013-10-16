using System;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace DexMac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		public AppDelegate ()
		{
		}

		public override void FinishedLaunching (NSObject notification)
		{
		}

		partial void _MenuOpenClick (MonoMac.Foundation.NSObject sender)
		{
			var openPanel = new NSOpenPanel();
			openPanel.ReleasedWhenClosed = true;
			openPanel.Prompt = "Select DEX file";
			openPanel.CanChooseDirectories = false;
			openPanel.AllowedFileTypes = new string[] { "apk", "dex" };

			var result = openPanel.RunModal();
			if (result == 1)
			{
				MainWindowController windowController = null;
				try {
					windowController = new MainWindowController ();
					windowController.Window.MakeKeyAndOrderFront (this);
					windowController.OpenDex (openPanel.Filename);
				} catch (Exception e) {
					Console.WriteLine(e);

					if (windowController != null) {
						windowController.Close ();
						windowController.Dispose ();
					}
				}
			}
		}

		partial void _MenuCloseClick (MonoMac.Foundation.NSObject sender)
		{
			NSApplication.SharedApplication.KeyWindow.Close();
		}
	}
}
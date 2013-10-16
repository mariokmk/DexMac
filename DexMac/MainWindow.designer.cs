// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace DexMac
{
	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSOutlineView _DexOutlineView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_DexOutlineView != null) {
				_DexOutlineView.Dispose ();
				_DexOutlineView = null;
			}
		}
	}

	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextView codeTextView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSOutlineView dexOutlineView { get; set; }

		[Outlet]
		MonoMac.AppKit.NSPopUpButton languagePopUpButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSearchField searchField { get; set; }

		[Action ("languagePopUpButtonClicked:")]
		partial void languagePopUpButtonClicked (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (codeTextView != null) {
				codeTextView.Dispose ();
				codeTextView = null;
			}

			if (dexOutlineView != null) {
				dexOutlineView.Dispose ();
				dexOutlineView = null;
			}

			if (languagePopUpButton != null) {
				languagePopUpButton.Dispose ();
				languagePopUpButton = null;
			}

			if (searchField != null) {
				searchField.Dispose ();
				searchField = null;
			}
		}
	}
}

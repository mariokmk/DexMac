using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using dex.net;
using MonoMac.CoreText;

namespace DexMac
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		private string _tempFile = null;
		private Dex _dex;
		private WritersFactory _factory = new WritersFactory ();
		private IDexWriter _writer;
		private CTStringAttributes _codeFont;
		private ClassDisplayOptions _classDisplayOptions = ClassDisplayOptions.ClassAnnotations |
			ClassDisplayOptions.ClassName | ClassDisplayOptions.ClassDetails | ClassDisplayOptions.Fields;

		#region Constructors
		// Called when created from unmanaged code
		public MainWindowController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController () : base ("MainWindow")
		{
			Initialize ();
		}
		// Shared initialization code
		void Initialize ()
		{
		}
		#endregion
		//strongly typed window accessor
		public new MainWindow Window {
			get {
				return (MainWindow)base.Window;
			}
		}

		internal void OpenDex(string filename)
		{
			string dexFile = filename;
			_tempFile = null;

			if (System.IO.Path.GetExtension(dexFile).EndsWith("apk")) {
				var apkFile = new FileInfo (dexFile);
				if (apkFile.Exists) {
					var zip = new ZipFile (dexFile);
					var entry = zip.GetEntry ("classes.dex");

					if (entry != null) {
						var zipStream = zip.GetInputStream (entry);
						var tempFileName = System.IO.Path.GetTempFileName ();

						var buffer = new byte[4096];
						using (var writer = File.Create(tempFileName)) {
							int bytesRead;
							while ((bytesRead = zipStream.Read(buffer, 0, 4096)) > 0) {
								writer.Write (buffer, 0, bytesRead);
							}
						}
						dexFile = tempFileName;
						_tempFile = dexFile;
					}
				}
			}

			_dex = new Dex(new FileStream (dexFile, FileMode.Open));

			this.Window.Title = "DexMac - " + System.IO.Path.GetFileName (filename);
			this.Window.Delegate = new DexWindowDelegate (this);

			_codeFont = new CTStringAttributes () {
				Font = new CTFont("Menlo", 12f)
			};

			codeTextView.TextContainer.WidthTracksTextView = false;
			codeTextView.TextContainer.ContainerSize = new System.Drawing.SizeF (float.MaxValue, float.MaxValue);

			searchField.Changed += ApplySearchFilter;

			dexOutlineView.DataSource = new DexDataSource();
			//dexOutlineView.SelectionDidChange += OnSelectionChanged;
			dexOutlineView.Delegate = new OutlineViewWorkaroundDelegate (this);

			// Supported Languages
			languagePopUpButton.RemoveAllItems ();
			languagePopUpButton.AddItems (_factory.GetWriters());

			UpdateSelectedLanguage ();
			_writer.dex = _dex;

			PopulateClasses();
		}

		partial void languagePopUpButtonClicked (MonoMac.Foundation.NSObject sender)
		{
			UpdateSelectedLanguage();
			UpdateCodeWindow();
		}

		private void UpdateSelectedLanguage()
		{
			var lang = languagePopUpButton.ItemTitle (languagePopUpButton.IndexOfSelectedItem);

			_writer = _factory.GetWriter (lang);
			_writer.dex = _dex;
		}

		private void PopulateClasses() 
		{
			var datasource = (DexDataSource)dexOutlineView.DataSource;
			datasource.SetData (_dex);

			dexOutlineView.ReloadData ();
		}

		private void UpdateCodeWindow()
		{
			var value = dexOutlineView.ItemAtRow (dexOutlineView.SelectedRow);
			var writer = new StringWriter ();

			if (value is ClassModel) {
				var dexClass = value as ClassModel;
				_writer.WriteOutClass (dexClass._class, _classDisplayOptions, writer);
			} else if (value is MethodModel) {
				var dexMethod = (MethodModel)value;
				_writer.WriteOutMethod (dexMethod._class, dexMethod._method, writer, new Indentation(), true);
			}

			var attrString = new NSAttributedString ((NSString)writer.ToString (), _codeFont);

			codeTextView.TextStorage.SetString(attrString);
		}

		void ApplySearchFilter (object sender, EventArgs e)
		{
			var datasource = (DexDataSource)dexOutlineView.DataSource;
			if (string.IsNullOrEmpty (searchField.StringValue)) {
				datasource.ResetFilter ();
			} else {
				datasource.SetFilter (searchField.StringValue);
			}

			dexOutlineView.ReloadData ();
		}

		class OutlineViewWorkaroundDelegate : NSOutlineViewDelegate
		{
			MainWindowController _controller;

			public OutlineViewWorkaroundDelegate (MainWindowController controller)
			{
				_controller = controller;
			}

			public override void SelectionDidChange (NSNotification notification)
			{
				_controller.UpdateCodeWindow ();
			}

		}

		class DexWindowDelegate : NSWindowDelegate
		{
			MainWindowController _controller;

			public DexWindowDelegate(MainWindowController controller) 
			{
				_controller = controller;
			}

			public override void WillClose (NSNotification notification)
			{
				_controller._dex.Dispose ();

				if (_controller._tempFile != null) {
					try {
						new FileInfo (_controller._tempFile).Delete();
					} catch {
					}
				}
			}
		}
	}
}
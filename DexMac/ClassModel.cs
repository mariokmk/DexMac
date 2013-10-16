using System;
using MonoMac.Foundation;
using dex.net;
using System.Collections.Generic;
using MonoMac.AppKit;
using System.Collections;

namespace DexMac
{
	internal interface IDataSource
	{
		int GetChildrenCount ();
		NSObject GetObjectValue ();
		NSObject GetChild (int index);
		bool ItemExpandable ();
	}

	public class PackageModel : NSObject, IDataSource, IComparable<PackageModel>
	{
		internal string _package;
		internal List<ClassModel> _classes;
		internal BitArray _filter;

		public PackageModel (string packageName)
		{
			_package = packageName;
			_classes = new List<ClassModel> ();
		}

		public bool IsClassInPackage(Class dexClass)
		{
			var package = dexClass.Name.Substring(0, dexClass.Name.LastIndexOf('.'));

			return _package.Equals (package);
		}

		public void InitFilter ()
		{
			_filter = new BitArray (_classes.Count, true);
		}

		#region IDataSource

		public int GetChildrenCount () 
		{
			return _filter.TrueCount();
		}

		public NSObject GetObjectValue () 
		{
			return (NSString)_package;
		}

		public NSObject GetChild (int index) 
		{
			return _classes [_filter.MapIndex(index)];
		}

		public bool ItemExpandable () 
		{
			return _filter.TrueCount() > 0;
		}

		#endregion

		#region IComparable<PackageModel> Members

		public int CompareTo(PackageModel p)
		{
			return _package.CompareTo(p._package);
		}

		#endregion
	}

	public class ClassModel : NSObject, IDataSource
	{
		internal Class _class;
		internal List<MethodModel> _methods;
		internal BitArray _filter;

		public ClassModel (Class dexClass)
		{
			_class = dexClass;

			_methods = new List<MethodModel> ();
			foreach (var method in _class.GetMethods ()) {
				_methods.Add(new MethodModel(dexClass, method));
			}

			_filter = new BitArray (_methods.Count, true);
		}
		
		#region IDataSource

		public int GetChildrenCount () 
		{
			return _filter.TrueCount();
		}

		public NSObject GetObjectValue () 
		{
			return (NSString)_class.Name.Substring(_class.Name.LastIndexOf('.')+1);
		}

		public NSObject GetChild (int index) 
		{
			return _methods [_filter.MapIndex(index)];
		}

		public bool ItemExpandable () 
		{
			return _filter.TrueCount() > 0;
		}

		#endregion
	}

	public class MethodModel : NSObject, IDataSource
	{
		internal Class _class;
		internal Method _method;

		public MethodModel (Class dexClass, Method method)
		{
			_class = dexClass;
			_method = method;
		}
		
		#region IDataSource

		public int GetChildrenCount () 
		{
			return 0;
		}

		public NSObject GetObjectValue () 
		{
			return (NSString)_method.Name;
		}

		public NSObject GetChild (int index) 
		{
			return null;
		}

		public bool ItemExpandable () 
		{
			return false;
		}

		#endregion
	}

	public class DexDataSource : NSOutlineViewDataSource
	{
		public List<PackageModel> Packages { get; set; }
		private BitArray _filter = new BitArray(0);

		public DexDataSource() {
			Packages = new List<PackageModel> ();
		}

		public void SetData(Dex dex)
		{
			Packages.Clear ();

			var packages = new Dictionary<string, PackageModel>();
			foreach (var dexClass in dex.GetClasses()) {
				var lastDotPosition = dexClass.Name.LastIndexOf ('.');
				string packageName;
				if (lastDotPosition >= 0) {
					packageName = dexClass.Name.Substring (0, dexClass.Name.LastIndexOf ('.'));
				} else {
					packageName = "default";
				}

				PackageModel package;
				if (!packages.TryGetValue (packageName, out package)) {
					package = new PackageModel(packageName);
					packages.Add (packageName, package);
				}

				package._classes.Add (new ClassModel (dexClass));
			}

			Packages.AddRange (packages.Values);
			Packages.Sort ();

			_filter.Length = Packages.Count;
			_filter.SetAll (true);

			foreach (var packageModel in Packages) {
				packageModel.InitFilter ();
			}
		}

		public void SetFilter(string filter)
		{
			filter = filter.ToLower ();

			_filter.SetAll (false);
			var currentPackage = 0;

			foreach (var package in Packages) {
				if (package._package.ToLower().IndexOf(filter) >= 0) {
					_filter.Set (currentPackage, true);
				}

				package._filter.SetAll (false);
				var currentClass = 0;
				foreach (var clazz in package._classes) {
					if (clazz._class.Name.ToLower().IndexOf(filter) >= 0) {
						package._filter.Set (currentClass, true);
						_filter.Set (currentPackage, true);
					}

					clazz._filter.SetAll (false);
					var currentMethod = 0;
					foreach (var method in clazz._methods) {
						if (method._method.Name.ToLower().IndexOf(filter) >= 0) {
							clazz._filter.Set (currentMethod, true);
							package._filter.Set (currentClass, true);
							_filter.Set (currentPackage, true);
						}

						currentMethod++;
					}

					currentClass++;
				}

				currentPackage++;
			}
		}

		public void ResetFilter()
		{
			_filter.SetAll (true);

			foreach (var package in Packages) {
				package._filter.SetAll (true);

				foreach (var clazz in package._classes) {
					clazz._filter.SetAll (true);
				}
			}
		}

		public override int GetChildrenCount (NSOutlineView outlineView, NSObject item)
		{
			if (item == null) {
				return _filter.TrueCount();
			}

			return (item as IDataSource).GetChildrenCount ();
		}

		public override NSObject GetObjectValue (NSOutlineView outlineView, NSTableColumn tableColumn, NSObject item)
		{
			if (item != null) {
				return (item as IDataSource).GetObjectValue ();
			}

			return (NSString)"Error!";
		}

		public override NSObject GetChild (NSOutlineView outlineView, int index, NSObject item)
		{
			if (item == null) {
				return Packages [_filter.MapIndex(index)];
			} else {
				return (item as IDataSource).GetChild (index);
			}
		}

		public override bool ItemExpandable (NSOutlineView outlineView, NSObject item)
		{
			if (item == null || item is MethodModel)
				return false;

			return (item as IDataSource).ItemExpandable ();
		}
	}

	public static class BitArrayExtensions
	{
		public static int TrueCount(this BitArray bitArray)
		{
			var count = 0;
			for (int i=0; i<bitArray.Count; i++) {
				if (bitArray [i]) {
					count++;
				}
			}

			return count;
		}

		public static int MapIndex(this BitArray bitArray, int index)
		{
			var count = 0;
			for (int i=0; i<bitArray.Count; i++) {
				if (bitArray [i]) {
					if (count == index) {
						return i;
					}

					count++;
				}
			}

			return -1;
		}
	}   
}
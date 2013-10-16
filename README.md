DexMac
======

Is a native OSX application for disassembling Android DEX files.

Usage
-----

Launch the app and open a DEX or APK file using the File->Open menu. The list of packages in the DEX/APK will display. Selecting a class or method will render the details of the selected item using the language shown in the 'Language' dropdown.

See the dex.net project for a description of the output languages.

The search field allows to filter the packages, classes or methods. If a method matches the criterium its class and package will be displayed as well to allow for navigation.

Development
-----------

DexMac uses the dex.net library which is configured as a git submodule. You must initialize the submodules before compiling DexMac.

License
-------

Dex.NET is provided under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0)


// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;

namespace iOSTestDrive
{
	[Register ("iOS_TestDriveViewController")]
	partial class iOS_TestDriveViewController
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton loadButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton loadCacheButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton queryButton { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIButton saveButton { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (loadButton != null) {
				loadButton.Dispose ();
				loadButton = null;
			}
			if (loadCacheButton != null) {
				loadCacheButton.Dispose ();
				loadCacheButton = null;
			}
			if (queryButton != null) {
				queryButton.Dispose ();
				queryButton = null;
			}
			if (saveButton != null) {
				saveButton.Dispose ();
				saveButton = null;
			}
		}
	}
}

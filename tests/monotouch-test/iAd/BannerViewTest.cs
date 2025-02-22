// Copyright 2011 Xamarin Inc. All rights reserved

#if HAS_IAD

using System;
using CoreGraphics;
using Foundation;
using iAd;
using NUnit.Framework;

namespace MonoTouchFixtures.iAd {
	
	[TestFixture]
	[Preserve (AllMembers = true)]
	public class BannerViewTest {
		
		[Test]
		public void InitWithFrame ()
		{
			var frame = new CGRect (10, 10, 100, 100);
			using (ADBannerView bv = new ADBannerView (frame)) {
				Assert.That (bv.Frame.X, Is.EqualTo (frame.X), "X");
				Assert.That (bv.Frame.Y, Is.EqualTo (frame.Y), "Y");
				// Width and Height are set by the Ad (e.g. 320 x 50 for the iPhone)
			}
		}
		
		[Test]
		public void GetClampedBannerSize ()
		{
			var size = new CGSize (2.0, 2.0);
			var result = ADBannerView.GetClampedBannerSize (size);
			Assert.IsFalse (result.IsEmpty);
		}
	}
}

#endif // HAS_IAD

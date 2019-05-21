namespace InControl.NativeProfile
{
	using System;


	// @cond nodoc
	public class ElecomControllerMacProfile : Xbox360DriverMacProfile
	{
		public ElecomControllerMacProfile()
		{
			Name = "Elecom Controller";
			Meta = "Elecom Controller on Mac";

			Matchers = new[] {
				new NativeInputDeviceMatcher {
					VendorID = 0x056e,
					ProductID = 0x2004,
				},
			};
		}
	}
	// @endcond
}



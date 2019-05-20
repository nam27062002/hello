namespace InControl.NativeProfile
{
	using System;


	// @cond nodoc
	public class ThrustmasterTXGIPMacProfile : Xbox360DriverMacProfile
	{
		public ThrustmasterTXGIPMacProfile()
		{
			Name = "Thrustmaster TX GIP";
			Meta = "Thrustmaster TX GIP on Mac";

			Matchers = new[] {
				new NativeInputDeviceMatcher {
					VendorID = 0x044f,
					ProductID = 0xb664,
				},
			};
		}
	}
	// @endcond
}



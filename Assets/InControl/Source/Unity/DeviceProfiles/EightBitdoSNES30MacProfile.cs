namespace InControl
{
	using System;


	// @cond nodoc
	[AutoDiscover]
	public class EightBitdoSNES30MacProfile : UnityInputDeviceProfile
	{
		public EightBitdoSNES30MacProfile()
		{
			Name = "8Bitdo SNES30 Controller";
			Meta = "8Bitdo SNES30 Controller on Mac";
			// Link = "https://www.amazon.com/Wireless-Bluetooth-Controller-Classic-Joystick/dp/B014QP2H1E";

			DeviceClass = InputDeviceClass.Controller;
			DeviceStyle = InputDeviceStyle.NintendoSNES;

			IncludePlatforms = new[] {
				"OS X"
			};

			JoystickNames = new[] {
				"Unknown 8Bitdo SNES30 GamePad",
				"SNES30             SNES30 Joy    "
			};

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "A",
					Target = InputControlType.Action2,
					Source = Button( 0 ),
				},
				new InputControlMapping {
					Handle = "B",
					Target = InputControlType.Action1,
					Source = Button( 1 ),
				},
				new InputControlMapping {
					Handle = "X",
					Target = InputControlType.Action4,
					Source = Button( 3 ),
				},
				new InputControlMapping {
					Handle = "Y",
					Target = InputControlType.Action3,
					Source = Button( 4 ),
				},
				new InputControlMapping {
					Handle = "L",
					Target = InputControlType.LeftBumper,
					Source = Button( 6 ),
				},
				new InputControlMapping {
					Handle = "R",
					Target = InputControlType.RightBumper,
					Source = Button( 7 ),
				},
				new InputControlMapping {
					Handle = "Select",
					Target = InputControlType.Select,
					Source = Button( 10 ),
				},
				new InputControlMapping {
					Handle = "Start",
					Target = InputControlType.Start,
					Source = Button( 11 ),
				},
			};

			AnalogMappings = new[] {
				new InputControlMapping {
					Handle = "DPad Left",
					Target = InputControlType.DPadLeft,
					Source = Analog( 0 ),
					SourceRange = InputRange.ZeroToMinusOne,
					TargetRange = InputRange.ZeroToOne,
				},
				new InputControlMapping {
					Handle = "DPad Right",
					Target = InputControlType.DPadRight,
					Source = Analog( 0 ),
					SourceRange = InputRange.ZeroToOne,
					TargetRange = InputRange.ZeroToOne,
				},
				new InputControlMapping {
					Handle = "DPad Up",
					Target = InputControlType.DPadUp,
					Source = Analog( 1 ),
					SourceRange = InputRange.ZeroToMinusOne,
					TargetRange = InputRange.ZeroToOne,
				},
				new InputControlMapping {
					Handle = "DPad Down",
					Target = InputControlType.DPadDown,
					Source = Analog( 1 ),
					SourceRange = InputRange.ZeroToOne,
					TargetRange = InputRange.ZeroToOne,
				},
			};
		}
	}
	// @endcond
}


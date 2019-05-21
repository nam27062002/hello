namespace InControl
{
	// @cond nodoc
	[AutoDiscover]
	public class OuyaLinuxProfile : UnityInputDeviceProfile
	{
		public OuyaLinuxProfile()
		{
			Name = "OUYA Controller";
			Meta = "OUYA Controller on Linux";

			DeviceClass = InputDeviceClass.Controller;
			DeviceStyle = InputDeviceStyle.Ouya;

			IncludePlatforms = new[] {
				"Linux"
			};

			JoystickNames = new[] {
				"OUYA Game Controller"
			};

			MaxUnityVersion = new VersionInfo( 4, 9, 0, 0 );

			LowerDeadZone = 0.3f;

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "O",
					Target = InputControlType.Action1,
					Source = Button0
				},
				new InputControlMapping {
					Handle = "A",
					Target = InputControlType.Action2,
					Source = Button3
				},
				new InputControlMapping {
					Handle = "U",
					Target = InputControlType.Action3,
					Source = Button1
				},
				new InputControlMapping {
					Handle = "Y",
					Target = InputControlType.Action4,
					Source = Button2
				},
				new InputControlMapping {
					Handle = "Left Bumper",
					Target = InputControlType.LeftBumper,
					Source = Button4
				},
				new InputControlMapping {
					Handle = "Right Bumper",
					Target = InputControlType.RightBumper,
					Source = Button5
				},
				new InputControlMapping {
					Handle = "Left Stick Button",
					Target = InputControlType.LeftStickButton,
					Source = Button6
				},
				new InputControlMapping {
					Handle = "Right Stick Button",
					Target = InputControlType.RightStickButton,
					Source = Button7
				},
				new InputControlMapping {
					Handle = "System",
					Target = InputControlType.System,
					Source = MenuKey
				},
				new InputControlMapping {
					Handle = "TouchPad Button",
					Target = InputControlType.TouchPadButton,
					Source = MouseButton0
				},
				new InputControlMapping {
					Handle = "DPad Left",
					Target = InputControlType.DPadLeft,
					Source = Button10,
				},
				new InputControlMapping {
					Handle = "DPad Right",
					Target = InputControlType.DPadRight,
					Source = Button11,
				},
				new InputControlMapping {
					Handle = "DPad Up",
					Target = InputControlType.DPadUp,
					Source = Button8,
				},
				new InputControlMapping {
					Handle = "DPad Down",
					Target = InputControlType.DPadDown,
					Source = Button9,
				},
			};

			AnalogMappings = new[] {
				LeftStickLeftMapping( Analog0 ),
				LeftStickRightMapping( Analog0 ),
				LeftStickUpMapping( Analog1 ),
				LeftStickDownMapping( Analog1 ),

				RightStickLeftMapping( Analog3 ),
				RightStickRightMapping( Analog3 ),
				RightStickUpMapping( Analog4 ),
				RightStickDownMapping( Analog4 ),

				new InputControlMapping {
					Handle = "Left Trigger",
					Target = InputControlType.LeftTrigger,
					Source = Analog2
				},
				new InputControlMapping {
					Handle = "Right Trigger",
					Target = InputControlType.RightTrigger,
					Source = Analog5
				},

				new InputControlMapping {
					Handle = "TouchPad X Axis",
					Target = InputControlType.TouchPadXAxis,
					Source = MouseXAxis,
					Raw = true
				},
				new InputControlMapping {
					Handle = "TouchPad Y Axis",
					Target = InputControlType.TouchPadYAxis,
					Source = MouseYAxis,
					Raw = true
				}
			};
		}
	}
	// @endcond
}


namespace InControl
{
	// @cond nodoc
	[AutoDiscover]
	public class NexusPlayerMacProfile : UnityInputDeviceProfile
	{
		// No trigger support, sadly. They're probably out of the
		// element range Unity supports.
		//
		public NexusPlayerMacProfile()
		{
			Name = "Nexus Player Controller";
			Meta = "Nexus Player Controller on OS X";

			DeviceClass = InputDeviceClass.Controller;

			IncludePlatforms = new[] {
				"OS X",
			};

			JoystickNames = new[] {
				"Unknown Gamepad",
			};

			ButtonMappings = new[] {
				new InputControlMapping {
					Handle = "A",
					Target = InputControlType.Action1,
					Source = Button0
				},
				new InputControlMapping {
					Handle = "B",
					Target = InputControlType.Action2,
					Source = Button1
				},
				new InputControlMapping {
					Handle = "X",
					Target = InputControlType.Action3,
					Source = Button2
				},
				new InputControlMapping {
					Handle = "Y",
					Target = InputControlType.Action4,
					Source = Button3
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
					Source = Button8
				},
				new InputControlMapping {
					Handle = "Back",
					Target = InputControlType.Back,
					Source = Button9
				},
				new InputControlMapping {
					Handle = "Start",
					Target = InputControlType.Start,
					Source = Button10
				}
			};

			AnalogMappings = new[] {
				LeftStickLeftMapping( Analog0 ),
				LeftStickRightMapping( Analog0 ),
				LeftStickUpMapping( Analog1 ),
				LeftStickDownMapping( Analog1 ),

				RightStickLeftMapping( Analog2 ),
				RightStickRightMapping( Analog2 ),
				RightStickUpMapping( Analog3 ),
				RightStickDownMapping( Analog3 ),

				DPadLeftMapping( Analog4 ),
				DPadRightMapping( Analog4 ),
				DPadUpMapping( Analog5 ),
				DPadDownMapping( Analog5 ),

//				new InputControlMapping {
//					Handle = "Left Trigger",
//					Target = InputControlType.LeftTrigger,
//					Source = Analog12,
//				},
//				new InputControlMapping {
//					Handle = "Right Trigger",
//					Target = InputControlType.RightTrigger,
//					Source = Analog11,
//				}
			};
		}
	}
	// @endcond
}


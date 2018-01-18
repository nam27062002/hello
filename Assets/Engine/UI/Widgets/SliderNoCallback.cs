using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Slider No Callback", 30)]
	public class SliderNoCallback : Slider {

		protected override void Set(float input, bool sendCallback)
        {
            base.Set( input, false);
        }

	}
}
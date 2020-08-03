
Shader "Hungry Dragon/DarkEyesEffect"
{
	Properties
	{
		_TimeOffset("Time offset", float) = 2.0
		_TimeFlick("Time flick", float) = 0.5
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_StencilMask("Stencil Mask", int) = 10

	}
	
	Subshader
	{
		Pass
		{
			Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "DisableBatching" = "True" }

			cull off
			
			Stencil
			{
				Ref[_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata {
					float4 vertex : POSITION;
				};

				uniform float _TimeOffset;
				uniform float _TimeFlick;
				uniform float4 _Color;

				struct v2f
				{
					float4 pos	: SV_POSITION;
				};
							
				#define PI2 6.28318530718
				v2f vert (appdata v)
				{
					v2f o;

					float tmod = fmod(_Time.y + unity_ObjectToWorld[0][3] * 0.01, _TimeOffset);
					float l = step(_TimeFlick, tmod);
					float s = lerp((cos((tmod / _TimeFlick) * PI2) + 1.0) * 0.5, 1.0, l);

					v.vertex.y *= s;

					o.pos = UnityObjectToClipPos (v.vertex);
					
					return o;
				}
				
				fixed4 frag (v2f i) : COLOR
				{
					return _Color;
				}
			ENDCG
		}
	}
}
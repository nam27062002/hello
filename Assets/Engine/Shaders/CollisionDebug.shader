// [AOC] Replacement shader to show the collisions during gameplay
Shader "Hungry Dragon/CollisionDebug" {
	Properties {
		// Properties are defined in the default collision shader
	}

	SubShader {
		// Make it transparent so we can overlay it over the decoration
		// Put it in the transparent queue so it's rendered on top of the decoration
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "ReplacementShaderID"="Collision"}

		// Standard settings for transparent rendering
		Cull Off
	        Lighting Off
	        ZWrite Off
	        ZTest Always
	        Fog { Mode Off }
	        Blend SrcAlpha OneMinusSrcAlpha

		LOD 100

		Pass {
			// Define vertex and fragment shaders
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// Include utils
			#include "UnityCG.cginc"

			// Declare used Properties (same name as Properties {} section)
			uniform fixed4 _DebugColor;

			// Input struct
			struct appdata {
				float4 vertex : POSITION;
			};

			// Vertex to fragment data struct
			struct v2f {
				float4 vertex : SV_POSITION;
			};

			// Vertex shader
			v2f vert(appdata v) {
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			// Fragment shader
			fixed4 frag(v2f i) : SV_Target {
				return _DebugColor;
			}
			ENDCG
		}
	}
}

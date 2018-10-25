// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Background"
{
	Properties
	{
		_Tint("Tint", color) = (1, 1, 1, 1)
		_StencilMask("Stencil Mask", int) = 10
	}

	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		Pass
		{
			Cull Back

			ZWrite on

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
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"


			struct appdata_t
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;

				HG_FOG_COORDS(3)
			};

			uniform float4 _Tint;
			HG_FOG_VARIABLES

			v2f vert(appdata_t v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

				HG_TRANSFER_FOG(o, worldPos);	// Fog
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

				fixed4 col = _Tint;
#if defined(FOG)
				HG_APPLY_FOG(i, col);	// Fog
#endif
				return col;
			}
			ENDCG
		}
	}
}

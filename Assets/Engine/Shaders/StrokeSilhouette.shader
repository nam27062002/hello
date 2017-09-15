Shader "Hungry Dragon/StrokeSilhouette"
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_OutlineWidth("Outline width", Range(0.0, 5.0)) = 0.2
		_SilhouetteColor("Silhouette Color", Color) = (1.0, 1.0, 1.0, 1.0)
	}


	CGINCLUDE


	#include "UnityCG.cginc" 
	#include "Lighting.cginc"
		//			#include "../HungryDragon.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2fo {
		float4 vertex : POSITION;
	};

	float4 _OutlineColor;
	float _OutlineWidth;
	float4 _SilhouetteColor;

	v2fo vert(appdata_t v)
	{
		v2fo o;
		float4 nvert = float4(v.vertex.xyz + v.normal * _OutlineWidth, 1.0);
		o.vertex = UnityObjectToClipPos(nvert);
		return o;
	}

	v2fo vert2(appdata_t v)
	{
		v2fo o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		return o;
	}


	fixed4 frag(v2fo i) : SV_Target
	{
		return _OutlineColor;
	}

	fixed4 frag2(v2fo i) : SV_Target
	{
		return _SilhouetteColor;
	}


	ENDCG


	SubShader
	{
		Tags{ "Queue" = "Transparent+50" "RenderType" = "Transparent"}

		ColorMask RGBA
		LOD 100


		Pass{
			ZWrite off
			Cull back

			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
										//		BlendOp add, max // Traditional transparency

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG

		}

		Pass
		{
			Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque"}

			cull back
			ztest less
			ZWrite On

			CGPROGRAM
			#pragma vertex vert2
			#pragma fragment frag2
			
			ENDCG
		}
	}
}

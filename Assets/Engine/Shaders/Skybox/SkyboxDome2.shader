// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// - Unlit
// - Scroll 2 layers /w Multiplicative op

Shader "Hungry Dragon/Skybox/Dome Skybox 2" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_SkyHighColor ("Sky high color", Color) = (0, 1, 1, 1)
	_SkyLowColor("Sky low color", Color) = (0, 0, 1, 1)

	_Speed("Cloud Speed", Float) = 1.0				// Fire speed
	_IOffset("Cloud Intensity Offset", Range(0.0, 0.2)) = 0.0							//intensity offset in noise texture

	_CamPos("Camera position", Vector) = (0.0, 0.0, 0.0, 1.0)

}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Opaque" }
	
	Lighting Off Fog { Mode Off }
	ZWrite On
	
	LOD 100
	
		
	CGINCLUDE
	#include "UnityCG.cginc"
	#include "../HungryDragon.cginc"

	sampler2D	_MainTex;
	sampler2D	_DetailTex;
	float4		_MainTex_ST;
	float4		_DetailTex_ST;
	float4		_SkyHighColor;
	float4		_SkyLowColor;
	float		_Speed;
	float		_IOffset;
//	float4		_CamPos;

	HG_FOG_VARIABLES

	struct appdata
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	
	struct v2f {
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
		HG_FOG_COORDS(2)
//		float4 camPos : TEXCOORD3;
	};


//#define PI 3.1415926535897932384626433832795

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex); 
		o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex);
		HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
//		o.camPos.xy = TRANSFORM_TEX(mul(unity_WorldToObject, _CamPos), _MainTex);
//		o.camPos.xy = mul(unity_WorldToObject, _CamPos);
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma glsl_no_auto_normalization
		#pragma fragmentoption ARB_precision_hint_fastest

//		#include "HungryDragon.cginc"


		fixed4 frag (v2f i) : COLOR
		{
			
//			fixed4 tex = tex2D (_MainTex, i.uv);
//			fixed4 tex2 = tex2D (_DetailTex, i.uv2.yx);
//			fixed4 one = fixed4(1,1,1,1);
//			fixed4 col = one - (one - tex) * (one - tex2);
//			i.uv.x += i.camPos.x * _MainTex_ST.z - 0.5;
			i.uv.x += -0.5;
			float persp = (0.1 + i.uv.y * 2.5);
			float2 uv = i.uv.xy + float2((_Time.y * _Speed), 0.0) * float2(persp, 1.0);

			//float intensity = tex2D(_MainTex, frac(uv2 * float2(1.0 / persp, 1.0))).x;
			float intensity = tex2D(_MainTex, uv * float2(1.0 / persp, 1.0)).x;

			float2 d = normalize(uv);
			float2 uv2 = i.uv2.xy - d * intensity * _IOffset;

//			float s = sin(i.uv.x * 5.0 * PI + _Time.y * _Speed * 5.0);
//			float c = cos(i.uv.y * 5.0 * PI + _Time.y * _Speed * 7.0);
//			float2x2 mr = float2x2(s, c, -c, s);
//			float intensity2 = tex2D(_DetailTex, (i.uv2.xy + float2(_Time.y * _Speed * 0.555, 0.0) + mul(mr, float2((1.0 - intensity) * _IOffset, intensity * _IOffset)))).x;// +pow(i.uv.y, 3.0);
			float intensity2 = tex2D(_DetailTex, uv2).x;// +pow(i.uv.y, 3.0);

//			fixed4 col = 1.0 - (1.0 - intensity) * (1.0 - intensity2);
			intensity = 1.0 - (1.0 - intensity) * (1.0 - intensity2);

			float4 skyCol = lerp(_SkyLowColor, _SkyHighColor, clamp(intensity, 0.0, 1.0));
//			col = 1.0 - (1.0 - col) * (1.0 - skyCol);
			fixed4 col = skyCol;


			HG_APPLY_FOG(i, col);	// Fog
			UNITY_OPAQUE_ALPHA(col.a);	// Opaque


//			float4 skyCol = lerp(_SkyLowColor, _SkyHighColor, clamp(persp, 0.0, 1.0));
			return col;// +skyCol;
		}
		ENDCG 
	}	
}
}

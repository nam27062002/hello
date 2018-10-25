// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// MatCap Shader, (c) 2015 Jean Moreno

Shader "Hungry Dragon/NPC/NPC Gold Freeze"
{
	Properties
	{
		_MatCap("Gold Tex", 2D) = "white" {}
		_Tint("Gold tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_FreezeTint("Freeze tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_FresnelPower("Fresnel power", Range(0.01, 10.0)) = 1.0
		_StencilMask("Stencil Mask", int) = 10
	}
	
	Subshader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }
		Pass
		{			
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

				uniform sampler2D _MatCap;
				uniform float4 _Tint;
				uniform float4 _FreezeTint;
				uniform float _FresnelPower;

				struct v2f
				{
					float4 pos		: SV_POSITION;
					float2 cap		: TEXCOORD0;
					float3 normal	: NORMAL;
					float3 vDir		: TANGENT;
				};
								
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					half2 capCoord;
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;

					o.normal = UnityObjectToWorldNormal(v.normal);

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
					o.vDir = viewDirection;

					return o;
				}
				
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 mc = tex2D(_MatCap, i.cap) * _Tint * 3.0;					
//					float fresnel = clamp(pow(max(1.0 - dot(i.vDir, i.normal), 0.0), _FresnelPower), 0.0, 1.0) * _FresnelColor.w;
					float fresnel = clamp(pow(max(1.0 - dot(i.vDir, i.normal), 0.0), _FresnelPower), 0.0, 1.0) * _FreezeTint.w;
					mc = lerp(mc, _FreezeTint, fresnel);

					return mc;
				}
			ENDCG
		}
	}
}
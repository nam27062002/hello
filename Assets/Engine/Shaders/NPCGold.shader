// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// MatCap Shader, (c) 2015 Jean Moreno

Shader "Hungry Dragon/NPC/NPC Gold"
{
	Properties
	{
		_MatCap("Gold Tex", 2D) = "white" {}
		_Tint("Gold tint", Color) = (1.0, 1.0, 1.0, 1.0)
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

                #pragma multi_compile __ NIGHT

				#include "UnityCG.cginc"
				
				struct v2f
				{
					float4 pos	: SV_POSITION;
					float2 cap	: TEXCOORD0;
				};
								
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos (v.vertex);
					half2 capCoord;
					
					float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
					worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
					o.cap.xy = worldNorm.xy * 0.5 + 0.5;
					
					return o;
				}
				
				uniform sampler2D _MatCap;
				uniform float4 _Tint;
				
				fixed4 frag (v2f i) : COLOR
				{
#if defined(NIGHT)
                    fixed4 mc = tex2D(_MatCap, i.cap) * _Tint * fixed4(0.5, 0.5, 1.0, 1.0);
#else
					fixed4 mc = tex2D(_MatCap, i.cap) * _Tint;
#endif					
					return mc * 3.0;
				}
			ENDCG
		}
	}
}
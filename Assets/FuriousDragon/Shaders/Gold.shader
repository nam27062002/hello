 Shader "Custom/Gold" 
 {
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Rimsize ("Rim size", Range (0,1.0)) = 1.0
		_RimIntensity ("Rim Intensity", Range (0,1.0)) = 1.0
		_RimColour ("Rim Colour", COLOR) = (0.53,0.46,0.22,1)
		_Cube ("Cubemap", CUBE) = "Gold-Cubemap" {}
		_CubeIntensity ("Cube Intensity", Range (0,1.0)) = 0.8
		_Tint ("Diffuse Tint", COLOR) = (0.58,0.36,0.12,1)

	}
	
	SubShader 
	{
	
		Tags { "Queue" = "Geometry" }
		
	 	Pass
	 	{
		
			CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members colour)
#pragma exclude_renderers xbox360
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0 
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			samplerCUBE _Cube;
			

			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    float2 uv1 : TEXCOORD1;
				float4 color : COLOR;	 
				float3 cube : TEXCOORD2;  
			};
			
			struct appdata 
			{
    			float4 vertex : POSITION;
    			float3 normal : NORMAL;
  			    float2 texcoord : TEXCOORD0;
 			};
			
			
			float4 _MainTex_ST;
			float4 _CausticTex_ST;
			float4 _CubeST;

			float _Rimsize;
			float _RimIntensity;
			float4 _RimColour;
			float4 _Tint;
			float _CubeIntensity;
			
			v2f vert (appdata v)
			{
				v2f o;

				float fTime = _Time;

			    o.pos = mul (UNITY_MATRIX_MVP, float4( v.vertex.xyz, 1.0f ));
			    float3 viewN = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);



				float3 positionW = mul(_Object2World, v.vertex).xyz;
  				float3 N = mul((float3x3)_Object2World, v.normal);
  				N = normalize(N);
  				float3 I = positionW - _WorldSpaceCameraPos;
  				float3 R = reflect(I,N);
               	o.cube = R;

			    // Apply the rim effect
 				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float dotProduct = 1 - dot(v.normal, viewDir);
                o.color = smoothstep(1 - _Rimsize, 1.0, dotProduct);
                o.color *= _RimIntensity;

			    o.uv = v.texcoord;
			    
			    return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
			    half4 texcol = tex2D (_MainTex, i.uv);
			    half4 cube =  half4(texCUBE(_Cube, i.cube).xyz,1.0);
			    cube *= _CubeIntensity;
			    texcol *= _Tint;
			    texcol += cube;
			    //texcol *= 1.5;
			    texcol.xyz += (i.color.xyz * _RimColour.xyz);
			    texcol.a = 1.0f;
 			    return texcol;
			}
			
			ENDCG	
		}
	}
	FallBack "Diffuse"
}

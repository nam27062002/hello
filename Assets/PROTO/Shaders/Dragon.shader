// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Dragon" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DetailTex ("Detail (RGB)", 2D) = "white" {}
	_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
	_ColorAdd ("Color Add", Color) = (0,0,0,0)

	_InnerLightAdd ("Inner Light Add", float) = 0
	_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

	_SpecLightVector ("Spec Light Vector", Vector) = (1,1,1,4)
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 normal : NORMAL;
				float3 halfDir : VECTOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;

			float4 _ColorMultiply;
			float4 _ColorAdd;

			uniform float _InnerLightAdd;
			uniform float4 _InnerLightColor;

			uniform float4 _SpecLightVector;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				// Normal
				float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(_Object2World, v.vertex).xyz);
				o.normal = normalize( mul( v.normal, _World2Object ) );

				// Half View - See: Blinn-Phong
    			o.halfDir = normalize(_SpecLightVector.xyz + viewDirection);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 main = tex2D(_MainTex, i.texcoord);
				fixed4 detail = tex2D(_DetailTex, i.texcoord);

				fixed4 col = ( main * (1 + detail.r * _InnerLightAdd * _InnerLightColor)) 	* _ColorMultiply + _ColorAdd;

				// Specular
				float specularLight = pow(max(dot( i.normal, i.halfDir), 0), _SpecLightVector.w) * detail.r;
				col = col + specularLight;	

				UNITY_OPAQUE_ALPHA(col.a);

				return col; 
			}
		ENDCG
	}
	
}

}

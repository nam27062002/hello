// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/TransparentAlphaBlendBillboard"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha
		Cull off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			// #pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			#define	BILLBOARDY

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
//				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
//				o.vertex = UnityObjectToClipPos(v.vertex);

#ifdef BILLBOARD
//				float sx = length(unity_ObjectToWorld[0].xyz);
//				float sy = length(unity_ObjectToWorld[1].xyz);
//				o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + float4(v.vertex.x * sx, v.vertex.y * sy, 0.0, 0.0));
//				o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)) + float4(v.vertex.x, v.vertex.y, 0.0, 0.0));
//				o.vertex = mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)));
#endif // BILLBOARD

#ifdef BILLBOARDY
				float3 side = unity_ObjectToWorld[0].xyz;
				float3 up = unity_ObjectToWorld[1].xyz;
				float3 forward = unity_ObjectToWorld[2].xyz;

				float sx = length(float3(side.x, up.x, forward.x));
				float sy = length(float3(side.y, up.y, forward.y));
				float sz = length(float3(side.z, up.z, forward.z));
				float3 sc = float3(sx, sy, sz);


				up /= sc;
//				forward /= sc;
//				float l = abs(dot(up, float3(0.0, 1.0, 0.0)));
//				up = lerp(up, forward, l);

				forward = normalize(-UNITY_MATRIX_VP[2].xyz);
				side = normalize(cross(up, forward));
				forward = normalize(cross(up, side));

//				side.x *= sx;
//				side.y *= sy;
//				side.z *= sz;

				float4 r0 = float4(side * sc, unity_ObjectToWorld[0][3]);
				float4 r1 = float4(up * sc, unity_ObjectToWorld[1][3]);
				float4 r2 = float4(forward * sc, unity_ObjectToWorld[2][3]);
				float4 r3 = float4(0.0, 0.0, 0.0, 1.0);

				float4x4 bm = float4x4(r0, r1, r2, r3);
				o.vertex = mul(UNITY_MATRIX_VP, mul(bm, float4(v.vertex.x, v.vertex.y, 0.0, 1.0)));
#endif // BILLBOARDY

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture

				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
//				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG

		}
	}
}

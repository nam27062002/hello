#ifndef HUNGRYDRAGON_CG_INCLUDED
#define HUNGRYDRAGON_CG_INCLUDED

#define HG_FOG_COORDS(idx) float2 fogCoord : TEXCOORD##idx;
#define HG_FOG_VARIABLES 	sampler2D _FogTexture;\
							float _FogStart;\
							float _FogEnd;
#define HG_TRANSFER_FOG(o,worldPos) o.fogCoord = float2(saturate((worldPos.z-_FogStart)/(_FogEnd-_FogStart)), 0.5);
#define HG_APPLY_FOG(i,col)  fixed4 fogCol = tex2D(_FogTexture, i.fogCoord); col.rgb = lerp( (col).rgb,fogCol.rgb,fogCol.a);

#define HG_DARKEN(idx) float darken : TEXCOORD##idx;
#define HG_DARKEN_DISTANCE 16.0
#define HG_TRANSFER_DARKEN(o,worldPos) o.darken = clamp( -worldPos.z, 0.0, HG_DARKEN_DISTANCE * 0.8) / HG_DARKEN_DISTANCE;
#define HG_APPLY_DARKEN(i, col) col = lerp( col, fixed4(0,0,0,1), i.darken);

//#define DYNAMIC_SHADOWS

#ifdef HG_SCENARY
	struct appdata_t
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	#ifdef LIGHTMAP_ON
		float4 texcoord1 : TEXCOORD1;
	#endif

		float4 color : COLOR;

		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

#endif

#ifdef HG_ENTITIES
	struct appdata_t
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float4 color : COLOR;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
	};

#endif


#endif // HUNGRYDRAGON_CG_INCLUDED
    
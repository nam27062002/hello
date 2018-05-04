#ifndef HUNGRYDRAGON_CG_INCLUDED
#define HUNGRYDRAGON_CG_INCLUDED

#define HG_FOG_COORDS(idx) float2 fogCoord : TEXCOORD##idx;
#define HG_FOG_VARIABLES 	sampler2D _FogTexture;\
							float _FogStart;\
							float _FogEnd;
#define HG_TRANSFER_FOG(o,worldPos) o.fogCoord = float2(saturate((worldPos.z-_FogStart)/(_FogEnd-_FogStart)), 0.5);
#define HG_APPLY_FOG(i,col)  fixed4 fogCol = tex2D(_FogTexture, i.fogCoord); col.rgb = lerp( (col).rgb,fogCol.rgb,fogCol.a);

#define HG_DARKEN(idx) float darken : TEXCOORD##idx;
//#define HG_DARKEN_DISTANCE 16.0
//#define HG_TRANSFER_DARKEN(o,worldPos) o.darken = clamp( -worldPos.z, 0.0, HG_DARKEN_DISTANCE * 0.8) / HG_DARKEN_DISTANCE;
//#define HG_TRANSFER_DARKEN(o,worldPos) o.darken = clamp( -worldPos.z, 0.0, _DarkenDistance * 0.8) / _DarkenDistance;
#define HG_TRANSFER_DARKEN(o,worldPos) o.darken = smoothstep(_DarkenPosition, _DarkenPosition - _DarkenDistance, worldPos.z );
#define HG_APPLY_DARKEN(i, col) col = lerp( col, fixed4(0,0,0,1), i.darken);

#endif // HUNGRYDRAGON_CG_INCLUDED
    
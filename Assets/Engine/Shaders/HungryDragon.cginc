#ifndef HUNGRYDRAGON_CG_INCLUDED
#define HUNGRYDRAGON_CG_INCLUDED

#define HG_FOG_COORDS(idx) float fogCoord : TEXCOORD##idx;
#define HG_FOG_VARIABLES 	sampler2D _FogTexture;\
							float4 _FogColor;\
							float _FogStart;\
							float _FogEnd;
#define HG_TRANSFER_FOG(o,worldPos) o.fogCoord = tex2Dlod(_FogTexture, float4(saturate((worldPos.z-_FogStart)/(_FogEnd-_FogStart)),0.5,0,0)).x * _FogColor.a;
#define HG_APPLY_FOG(i,col) col.rgb = lerp( (col).rgb,(_FogColor).rgb,i.fogCoord);

#define HG_DARKEN(idx) float darken : TEXCOORD##idx;
#define HG_TRANSFER_DARKEN(o,worldPos) o.darken = clamp( -worldPos.z * 0.1, 0, 0.5);
#define HG_APPLY_DARKEN(i, col) col = lerp( col, fixed4(0,0,0,1), i.darken);


#endif // HUNGRYDRAGON_CG_INCLUDED
    
#ifndef HUNGRYDRAGON_CG_INCLUDED
#define HUNGRYDRAGON_CG_INCLUDED

#define HG_FOG_COORDS(idx) float fogCoord : TEXCOORD##idx;
#define HG_TRANSFER_FOG(o,worldPos,start,end) o.fogCoord = (worldPos.z-start)/(end-start);
#define HG_APPLY_FOG(i,col,fogColor) col.rgb = lerp( (col).rgb,(fogColor).rgb,min(saturate(i.fogCoord),fogColor.a) ); 

#define HG_DARKEN(idx) float darken : TEXCOORD##idx;
#define HG_TRANSFER_DARKEN(o,worldPos) o.darken = clamp( -worldPos.z * 0.1, 0, 0.5);
#define HG_APPLY_DARKEN(i, col) col = lerp( col, fixed4(0,0,0,1), i.darken);


#endif // HUNGRYDRAGON_CG_INCLUDED
    
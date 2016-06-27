#ifndef HUNGRYDRAGON_CG_INCLUDED
#define HUNGRYDRAGON_CG_INCLUDED

#define HG_FOG_COORDS(idx) float fogCoord : TEXCOORD##idx;

#define HG_TRANSFER_FOG(o,worldPos,start,end) o.fogCoord = (worldPos.z-start)/(end-start);

#define HG_APPLY_FOG(cood,col,fogColor) col.rgb = lerp( (col).rgb,(fogColor).rgb,min(saturate(cood.fogCoord),fogColor.a) ); 

#endif // HUNGRYDRAGON_CG_INCLUDED
    
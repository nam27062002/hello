#ifndef HUNGRYDRAGON_CG_INCLUDED
#define HUNGRYDRAGON_CG_INCLUDED

#include "UnityShaderVariables.cginc"

#define HG_TRANSFER_FOG(o,worldPos) o.fogCoord = (worldPos.z) * unity_FogParams.z + unity_FogParams.w

#define HG_APPLY_FOG(cood,col) col.rgb = lerp( unity_FogColor.rgb,(col).rgb, max(saturate(cood.fogCoord),unity_FogColor.a) ); 

#endif // HUNGRYDRAGON_CG_INCLUDED
  
#ifndef RECONSTRUCT_FROM_DEPTH_INCLUDED
#define RECONSTRUCT_FROM_DEPTH_INCLUDED

//Reconstruct world position from RAW depth (0..1) + screen UV (0..1)
inline void ReconstructWorldFromRawDepth_float(float2 uv01, float rawDepth, out float3 worldPos)
{
    
    #if !defined(BUILTIN_PIPELINE_CORE_INCLUDED)
        
        float zClip = rawDepth;
        
        //Handle Unity's render-target Y flip on SRP only.
        #if UNITY_UV_STARTS_AT_TOP
            uv01.y = 1.0 - uv01.y;
        #endif
        
        //NDC (-1..1)
        float4 clip = float4(uv01 * 2.0 - 1.0, zClip, 1.0);
        
        //Use HDRP and URP compatible inverse projection matrix.
        //If using TAA/Camera Jitter in URP 14, prefer the UNJITTERED inverse VP:
        
        float4 wpos;
        
        #if defined(UNITY_MATRIX_UNJITTERED_I_VP)
            wpos = mul(UNITY_MATRIX_UNJITTERED_I_VP, clip);
        #else
            wpos = mul(UNITY_MATRIX_I_VP, clip);
        #endif
        
        worldPos = wpos.xyz / max(wpos.w, 1e-6);
        
        //HDRP tracks world space relative to camera, unlike URP which is absolute.
        //Add the world space camera position to remove that.
        #if !defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
            worldPos += _WorldSpaceCameraPos;
        #endif
    
    #else
        //Use legacy built-in RP projection matrix.
        
        float zDepth = rawDepth;
    
        #if defined(UNITY_REVERSED_Z)
            zDepth = 1 - zDepth;
        #endif

        //Calculate clip position using UV screen position and raw depth.
        float4 clipPos = float4(uv01.xy, zDepth, 1.0);
    
        //Offset clip position to NDC coordinates.
        clipPos.xyz = 2.0f * clipPos.xyz - 1.0f;
    
        //Use inverse projection matrix to convert into view space.
        float4 vpos = mul(unity_CameraInvProjection, clipPos);
        vpos.xyz /= max(vpos.w, 1e-6);
        vpos.z *= -1;
    
        //Convert from view space to world space.
        float4 wpos = mul(unity_CameraToWorld, float4(vpos.xyz, 1));
    
        worldPos = wpos.xyz / max(wpos.w, 1e-6);
    #endif
}

#endif
    
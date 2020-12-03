Shader "IslandMesh/MeshShader"
{
    Properties
    {
       
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows 

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxColourCount = 8;
        float minHeight;
        float maxHeight;
        
        int baseColourCount;
        float3 baseColours[maxColourCount];
        float baseStartHeights[maxColourCount];
        float baseBlends[maxColourCount];

        const static float epsilon = 0.00001;

        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex;
        };

       
        float InverseLerp(float a, float b, float value){
            return saturate((value - a) / (b-a));
        }
         // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        
       

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y);
            for (int i = 0; i < baseColourCount; i ++){
                float drawStrength = InverseLerp(-baseBlends[i]/2, baseBlends[i]/2 + epsilon, (heightPercent - baseStartHeights[i]));
                o.Albedo = o.Albedo * (1 - drawStrength) + baseColours[i] * drawStrength;
               
            }
            o.Alpha = 1;
            
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}

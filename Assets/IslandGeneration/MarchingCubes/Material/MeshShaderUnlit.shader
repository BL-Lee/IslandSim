// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "IslandMesh/MeshShaderUnlit"
{
    Properties
    {
       _LightingPosterize("Lighting Posterization", Int) = 1
      _Glossiness("Glossiness", Float) = 10
    }
    SubShader
    {
        
        LOD 100

        Pass
        {
            Tags { 
            
            "LightMode" = "ForwardBase"
            
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            const static int maxColourCount = 8;
            fixed minHeight;
            fixed maxHeight;
        
            int baseColourCount;
            fixed3 baseColours[maxColourCount];
            fixed baseStartHeights[maxColourCount];
            fixed baseBlends[maxColourCount];

            float epsilon = 1E-8;

            int _LightingPosterize;
            fixed _Glossiness;
            fixed4 _AmbientLight = fixed4(1,1,1,1);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;
                float3 vertexLighting : TEXCOORD2;
                SHADOW_COORDS(3)

            };

             float posterize(int s, float value){
                return round(value * s) / s;
            }


            float InverseLerp(float a, float b, float value){
                return saturate((value - a) / (b - a));
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal  = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.viewDir = WorldSpaceViewDir(v.vertex);
                o.vertexLighting = half3(0.0,0.0,0.0);
                for (int i = 0; i < 4; i ++){

                    half4 lightPosition = half4(
                        unity_4LightPosX0[i],
                        unity_4LightPosY0[i],
                        unity_4LightPosZ0[i],
                        1.0);

                    half3 vertexToLightSource = lightPosition.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
                   
                    half3 lightDirection = normalize(vertexToLightSource);
                    half squaredDistance = dot (vertexToLightSource , vertexToLightSource);
                    float attenuation = 1.0 / (1.0 + unity_4LightAtten0[i] * squaredDistance);
                    float3 diffuseReflection = attenuation 
                     * unity_LightColor[i].rgb  
                     * max(0.0,dot(o.worldNormal, lightDirection));    

                   o.vertexLighting = o.vertexLighting + diffuseReflection;
                   // NdotL = dot(half3(unity_4LightPosX0[i] , unity_4LightPosY0[i] , unity_4LightPosZ0[i]), normal);
                    //PointLights = PointLights + NdotL * shadow * unity_LightColor[i];
                }


               //UNITY_TRANSFER_FOG(o,o.vertex);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);

                float shadow = SHADOW_ATTENUATION(i);

                fixed4 vertexLighting = fixed4(i.vertexLighting,1);

                float NdotL = dot(_WorldSpaceLightPos0, normal);
                //fixed lightIntensity = smoothstep(0,0.5,NdotL * shadow);
                fixed lightIntensity = posterize(_LightingPosterize, NdotL * shadow);
                fixed light = lightIntensity * _LightColor0 + _AmbientLight;
                

                
                
                //Light at uv position of island
                float heightPercent = InverseLerp(minHeight, maxHeight, i.uv.y);
                fixed3 outColour = fixed3(1,1,1);
                
                for (int i = 0; i < baseColourCount; i++){
                    fixed drawStrength = InverseLerp(-baseBlends[i]/2, baseBlends[i]/2 + epsilon, (heightPercent - baseStartHeights[i]));
                    outColour = outColour * (1 - drawStrength) + baseColours[i] * drawStrength;
                }
                
               
                
                return fixed4(outColour, 1) * (light + vertexLighting + _LightColor0);
                //return heightPercent;
            }
            ENDCG
        }
        /*
        Pass
        {
        Tags{
        "LightMode" = "ForwardAdd"
        }
         Blend One One
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"


            const static int maxColourCount = 8;
            float minHeight;
            float maxHeight;
        
            int baseColourCount;
            float3 baseColours[maxColourCount];
            float baseStartHeights[maxColourCount];
            float baseBlends[maxColourCount];

            float epsilon = 1E-8;

            int _LightingPosterize;
            float _Glossiness;
            fixed4 _AmbientLight = (1,1,1,1);

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;
                SHADOW_COORDS(3)

            };

             float posterize(int s, float value){
                return round(value * s) / s;
            }


            float InverseLerp(float a, float b, float value){
                return saturate((value - a) / (b - a));
            }


            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal  = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                o.viewDir = WorldSpaceViewDir(v.vertex);
               //UNITY_TRANSFER_FOG(o,o.vertex);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);

                float shadow = SHADOW_ATTENUATION(i);

                float NdotL = dot(_WorldSpaceLightPos0, normal);
                //fixed lightIntensity = smoothstep(0,0.5,NdotL * shadow);
                fixed lightIntensity = posterize( _LightingPosterize, NdotL * shadow);
                fixed light = lightIntensity * _LightColor0 ;
                

                
                
                //Light at uv position of island
                float heightPercent = InverseLerp(minHeight, maxHeight, i.uv.y);
                fixed3 outColour = (1,1,1);
                
                for (int i = 0; i < baseColourCount; i++){
                    float drawStrength = InverseLerp(-baseBlends[i]/2, baseBlends[i]/2 + epsilon, (heightPercent - baseStartHeights[i]));
                    outColour = outColour * (1 - drawStrength) + baseColours[i] * drawStrength;
                }
                
                
                


                return light * _LightColor0;//fixed4(outColour, 1) * (light);
                //return heightPercent;
            }
            ENDCG
        }*/
       
    }
}

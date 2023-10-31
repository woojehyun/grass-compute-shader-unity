Shader "DrawMeshInstancedIndirect/SingleGrass"
{
    Properties
    {
        [MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
        _BaseColorTexture("_BaseColorTexture", 2D) = "white" {}
        _GroundColor("_GroundColor", Color) = (0.5,0.5,0.5)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            Cull Back //use default culling because this shader is billboard 
            ZTest Less
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Render Pipeline keywords
            // When doing custom shaders you most often want to copy and paste these #pragmas
            // These multi_compile variants are stripped from the build depending on:
            // 1) Settings in the URP Asset assigned in the GraphicsSettings at build time
            // e.g If you disabled AdditionalLights in the asset then all _ADDITIONA_LIGHTS variants
            // will be stripped from build
            // 2) Invalid combinations are stripped. e.g variants with _MAIN_LIGHT_SHADOWS_CASCADE
            // but not _MAIN_LIGHT_SHADOWS are invalid and therefore stripped.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            // -------------------------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../Shaders/shared/GrassBlade.hlsl"
            #include "../Shaders/shared/GrassVertexManipulations.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 texcoord : TEXCOORD;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                half3 color        : COLOR;                
            };

            CBUFFER_START(UnityPerMaterial)
                float2 _BoundSize;

                half3 _BaseColor;
                float4 _BaseColorTexture_ST;
                half3 _GroundColor;                

                StructuredBuffer<GrassBlade> OutputGrassBladesBuffer;
                float3 WindDirection;
                float WindForce;
            CBUFFER_END
            
            sampler2D _BaseColorTexture;

            half3 ApplySingleDirectLight(Light light, half3 N, half3 V, half3 albedo, half positionOSY)
            {                
                half3 H = normalize(light.direction + V);

                //direct diffuse 
                const half directDiffuse = saturate(dot(N, light.direction)) * 0.5 + 0.5; //half lambert, to fake grass SSS

                //direct specular
                float directSpecular = saturate(dot(N,H));
                //pow(directSpecular,8)
                directSpecular *= directSpecular;
                directSpecular *= directSpecular;
                directSpecular *= directSpecular;
                //directSpecular *= directSpecular; //enable this line = change to pow(directSpecular,16)

                //add direct directSpecular to result
               directSpecular *= 0.1 * positionOSY;//only apply directSpecular to grass's top area, to simulate grass AO

                half3 lighting = light.color * (light.shadowAttenuation * light.distanceAttenuation);
                //half3 result = (albedo * directDiffuse + directSpecular) * lighting;
                half3 result = (albedo * directDiffuse) * lighting;
                return result; 
            }

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                GrassBlade grassBlade;                
                float3 positionWS = positionWindGrassVertex(                    
                    OutputGrassBladesBuffer,
                    instanceID,
                    grassBlade,
                    IN.positionOS,                    
                    WindDirection,
                    WindForce
                );

                OUT.positionCS = TransformWorldToHClip(positionWS);

                /////////////////////////////////////////////////////////////////////
                //lighting & color
                /////////////////////////////////////////////////////////////////////

                //lighting data
                Light mainLight;
#if _MAIN_LIGHT_SHADOWS
                mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
#else
                mainLight = GetMainLight();
#endif
                
                float3       N = TransformObjectToWorldNormal(normalVertexInWorld(grassBlade.rotationY, IN.normal).xyz);
                const float3 viewWS = _WorldSpaceCameraPos - grassBlade.position;
                const float  ViewWSLength = length(viewWS);
                half3        V = viewWS / ViewWSLength;

                half3 albedo = tex2Dlod(_BaseColorTexture, float4(TRANSFORM_TEX(IN.texcoord,_BaseColorTexture),0,0)).xyz * _BaseColor;//sample mip 0 only
                albedo = lerp(_GroundColor,albedo, IN.positionOS.y);

                //indirect
                half3 lightingResult = SampleSH(0) * albedo;

                //main direct light
                lightingResult += ApplySingleDirectLight(mainLight, N, V, albedo, grassBlade.position.y);

                // Additional lights loop
#if _ADDITIONAL_LIGHTS

                // Returns the amount of lights affecting the object being renderer.
                // These lights are culled per-object in the forward renderer
                int additionalLightsCount = GetAdditionalLightsCount();
                for (int i = 0; i < additionalLightsCount; ++i)
                {
                    // Similar to GetMainLight, but it takes a for-loop index. This figures out the
                    // per-object light index and samples the light buffer accordingly to initialized the
                    // Light struct. If _ADDITIONAL_LIGHT_SHADOWS is defined it will also compute shadows.
                    Light light = GetAdditionalLight(i, positionWS);

                    // Same functions used to shade the main light.
                    lightingResult += ApplySingleDirectLight(light, N, V, albedo, grassBlade.position.y);
                }
#endif

                //fog
                float fogFactor = ComputeFogFactor(OUT.positionCS.z);
                // Mix the pixel color with fogColor. You can optionaly use MixFogColor to override the fogColor
                // with a custom one.
                OUT.color = MixFog(lightingResult, fogFactor);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {                
                return half4(IN.color,1);
            }
            ENDHLSL
        }

        Pass
        {
            //Cull Off //use default culling because this shader is billboard 
            //ZTest Less
            Tags { "LightMode" = "ShadowCaster" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // -------------------------------------
            // Universal Render Pipeline keywords
            // When doing custom shaders you most often want to copy and paste these #pragmas
            // These multi_compile variants are stripped from the build depending on:
            // 1) Settings in the URP Asset assigned in the GraphicsSettings at build time
            // e.g If you disabled AdditionalLights in the asset then all _ADDITIONA_LIGHTS variants
            // will be stripped from build
            // 2) Invalid combinations are stripped. e.g variants with _MAIN_LIGHT_SHADOWS_CASCADE
            // but not _MAIN_LIGHT_SHADOWS are invalid and therefore stripped.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            // -------------------------------------

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "shared/GrassBlade.hlsl"
            #include "shared/GrassVertexManipulations.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)                
                StructuredBuffer<GrassBlade> OutputGrassBladesBuffer;
                float3 WindDirection;
                float WindForce;                
            CBUFFER_END

            Varyings vert(Attributes IN, uint instanceID : SV_InstanceID)
            {
                Varyings OUT;

                GrassBlade grassBlade;                
                const float3 positionWS = positionGrassVertex(                    
                    OutputGrassBladesBuffer,
                    instanceID,
                    grassBlade,
                    IN.positionOS,                    
                    WindDirection,
                    WindForce
                );

                OUT.positionCS = TransformWorldToHClip(positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(1,1,1,1);
            }
            ENDHLSL
        }

        //copy pass, change LightMode to ShadowCaster will make grass cast shadow
        //copy pass, change LightMode to DepthOnly will make grass render into _CameraDepthTexture
    }
}
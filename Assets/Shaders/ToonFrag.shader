Shader "Unlit/ToonFrag"
{
    //ref https://roystan.net/articles/toon-shader/
    
    Properties
    {
        MainColor ("Color", Color) = (1,1,1,1)
        MainTexture ("MainTexture", 2D) = "white" {}
        RampTexture ("RampTexture", 2D) = "white" {}
        
        AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
        
        SpecularColor("Specular Color", Color) = (0.9,0.9,0.9,1)
        Glossiness("Glossiness", Float) = 32
        
        RimColor("Rim Color", Color) = (1,1,1,1)
        RimAmount("Rim Amount", Range(0, 1)) = 0.716
        RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Pass
        {
            Tags {  "LightMode" = "ForwardBase"}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            fixed4 MainColor;
            sampler2D MainTexture;
            float4 MainTexture_ST;
            sampler2D RampTexture;
            float4 RampTexture_ST;
            
            float4 AmbientColor;
            float Glossiness;
            float4 SpecularColor;

            float4 RimColor;
            float RimAmount;
            float RimThreshold;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, MainTexture);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                o.viewDir = WorldSpaceViewDir(v.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               float3 normal = normalize(i.worldNormal);
                float NdotL = dot(_WorldSpaceLightPos0, normal);

                float shadow = SHADOW_ATTENUATION(i);
                
                float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);
                float4 light = lightIntensity * _LightColor0;


                float3 viewDir = normalize(i.viewDir);
                float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);
                float NdotH = dot(normal, halfVector);

                float specularIntensity = pow(NdotH * lightIntensity, Glossiness * Glossiness);
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                float4 specular = specularIntensitySmooth * SpecularColor;

                float4 rimDot = 1 - dot(viewDir, normal);

                float rimIntensity = rimDot * pow(NdotL, RimThreshold);
                rimIntensity = smoothstep(RimAmount - 0.01, RimAmount + 0.01, rimIntensity);

                float h = rimDot * 0.5 + 0.5;
                float2 rh = -h;
                float4 ramTex = tex2D(RampTexture, rh);
                
                
                float4 rim = rimIntensity * RimColor;
                
                fixed4 tex = tex2D(MainTexture, i.uv);
                
                return MainColor * tex * (AmbientColor + light + specular + rim);
            }
            ENDCG
        }
        
        Pass 
        {
            Tags {"LightMode"="ShadowCaster"}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"

             struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 normal : NORMAL;
            };

            struct v2f
            {
                V2F_SHADOW_CASTER;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i);
            }
            ENDCG
        }
    }
}

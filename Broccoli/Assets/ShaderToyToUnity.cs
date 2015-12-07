using UnityEngine;

// https://alastaira.wordpress.com/2015/08/07/unity-shadertoys-a-k-a-converting-glsl-shaders-to-cghlsl/
// https://msdn.microsoft.com/en-GB/library/windows/apps/dn166865.aspx?tduid=(f3be73a73f6eec7cca134536a3b118ef)(256380)(2459594)(TnL5HPStwNw-TFxCy.hsr4jusqWyvt.OkA)()
// http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
// http://kylehalladay.com/blog/tutorial/bestof/2014/01/12/Runtime-Shader-Compilation-Unity.html
// http://answers.unity3d.com/questions/21429/creating-a-material-from-shader-string.html

/*
Replace iGlobalTime shader input (“shader playback time in seconds”) with _Time.y
Replace iResolution.xy (“viewport resolution in pixels”) with _ScreenParams.xy
Replace vec2 types with float2, mat2 with float2x2 etc.
Replace vec3(1) shortcut constructors in which all elements have same value with explicit float3(1,1,1)
Replace Texture2D with Tex2D
Replace atan(x,y) with atan2(y,x) <- Note parameter ordering!
Replace mix() with lerp()
Replace *= with mul()
Remove third (bias) parameter from Texture2D lookups
mainImage(out vec4 fragColor, in vec2 fragCoord) is the fragment shader function, equivalent to float4 mainImage(float2 fragCoord : SV_POSITION) : SV_Target
UV coordinates in GLSL have 0 at the top and increase downwards, in HLSL 0 is at the bottom and increases upwards, so you may need to use uv.y = 1 – uv.y at some point.
*/

/*
void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = fragCoord.xy / iResolution.xy;
    fragColor = vec4(uv,0.5+0.5*sin(iGlobalTime),1.0);
}
*/

/*
#pragma surface surf Standard fullforwardshadows

// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

sampler2D _MainTex;

    struct Input {
        float2 uv_MainTex;
    };

    half _Glossiness;
    half _Metallic;
    fixed4 _Color;

    void surf (Input IN, inout SurfaceOutputStandard o) {
        // Albedo comes from a texture tinted by color
        fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
        o.Albedo = c.rgb;
        // Metallic and smoothness come from slider variables
        o.Metallic = _Metallic;
        o.Smoothness = _Glossiness;
        o.Alpha = c.a;
    }

*/



public class ShaderToyToUnity
{
    public static string Convert(string name, string code)
    {
        var template = @"
            Shader ""Custom/ShaderToy""
            {
                Properties
                {
                    _MainTex (""Texture"",  2D) = ""white"" {}
                }
                SubShader
                {
                    Tags { ""RenderType""=""Opaque"" }
                    LOD 100

                    Pass
                    {

                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        
                        #include ""UnityCG.cginc""

                        struct appdata
                        {
                            float4 vertex : POSITION;
                            float2 uv : TEXCOORD0;
                        };

                        struct v2f
                        {
                            float2 uv : TEXCOORD0;
                            float4 vertex : SV_POSITION;
                        };

                        sampler2D _MainTex;
                        float4 _MainTex_ST;

                        v2f vert(appdata v)
                        {
                            v2f o;
                            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                            return o;
                        }

                        // GENERATED HELPERS

                        $HELPERS

                        //void mainImage(out vec4 fragColor, in vec2 fragCoord)
                        //{
                            //vec2 uv = fragCoord.xy / _ScreenParams.xy;
                            //fragColor = vec4(1,1,0,1);
                        //}

                        // GENERATED HELPERS

                        $CODE

                        // FINAL FRAG

                        fixed4 frag(v2f i) : SV_Target
                        {
                            float4 color;
                            float2 uv = i.uv;

                            mainImage(color, uv);

                            return color;
                        }

                        ENDCG
                    }
                }
            }
        ";

        // TODO: replace vec3(x) with vec3(x,x,x)
        // TODO: Replace *= with mul()
        // TODO: Remove third (bias) parameter from Texture2D lookups

        var helpers = @"
        
            #define vec2 float2
            #define vec3 float3
            #define vec4 float4

            #define mat2 float2x2
            #define mat3 float3x3
            #define mat4 float4x4

            #define Texture2D(a,b,c) Tex2D(a,b)

            #define atan(x, y) atan2(y, x)
            #define mix lerp
            #define mod fmod
            #define fract frac
        ";

        template = template.Replace("$HELPERS", helpers);

        code = code.Replace("iGlobalTime", "_Time.y");
        code = code.Replace("iResolution", "_ScreenParams");

        // mainImage(out vec4 fragColor, in vec2 fragCoord) is the fragment shader function, equivalent to float4 mainImage(float2 fragCoord : SV_POSITION) : SV_Target
        // UV coordinates in GLSL have 0 at the top and increase downwards, in HLSL 0 is at the bottom and increases upwards, so you may need to use uv.y = 1 – uv.y at some point.

        return template.Replace("$CODE", code);
    }
}

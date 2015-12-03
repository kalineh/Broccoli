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
            Shader ""Custom/ShaderToy"" {
                Properties {
                    _Color (""Color"", Color) = (1,1,1,1)
                    _MainTex (""Albedo (RGB)"", 2D) = ""white"" {}
                    _Glossiness (""Smoothness"", Range(0,1)) = 0.5
                    _Metallic (""Metallic"", Range(0,1)) = 0.0
                }
                SubShader {
                    Tags { ""RenderType""=""Opaque"" }
                    LOD 200

                    CGPROGRAM

                    #pragma surface surf Standard fullforwardshaders

                    #pragma target 3.0

                    sampler2D _MainTex;

                    struct Input {
                        float2 uv_MainTex;
                    };

                    half _Glossiness;
                    half _Metallic;
                    fixed4 _Color;

                    $CODE

                    ENDCG
                }
                Fallback ""Diffuse""
            }
        ";

        code = code.Replace("iGlobalTime", "_Time.y");
        code = code.Replace("iResolution", "_ScreenParams");
        code = code.Replace("vec2", "float2");
        code = code.Replace("vec3", "float3");
        code = code.Replace("vec4", "float4");
        code = code.Replace("mat2", "float2x2");
        code = code.Replace("mat3", "float3x3");
        code = code.Replace("mat4", "float4x4");
        code = code.Replace("Texture2D", "Tex2D");
        code = code.Replace("atan", "atan2");
        code = code.Replace("mix", "lerp");

        // TODO: replace vec3(x) with vec3(x,x,x)
        // TODO: replace atan(x,y) with atan2(y,x) <- Note parameter ordering!
        // TODO: Replace *= with mul()
        // TODO: Remove third (bias) parameter from Texture2D lookups

        // mainImage(out vec4 fragColor, in vec2 fragCoord) is the fragment shader function, equivalent to float4 mainImage(float2 fragCoord : SV_POSITION) : SV_Target
        // UV coordinates in GLSL have 0 at the top and increase downwards, in HLSL 0 is at the bottom and increases upwards, so you may need to use uv.y = 1 – uv.y at some point.

        return template.Replace("$CODE", code);
    }
}

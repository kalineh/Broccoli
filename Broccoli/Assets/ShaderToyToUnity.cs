﻿using UnityEngine;
using System.Text.RegularExpressions;

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
                    _MainTex (""Texture"", 2D) = ""white"" {}
                    iChannel0 (""Texture"", 2D) = ""white"" {}
                    iChannel1 (""Texture"", 2D) = ""white"" {}
                    iChannel2 (""Texture"", 2D) = ""white"" {}
                    iChannel3 (""Texture"", 2D) = ""white"" {}
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

                        sampler2D iChannel0;
                        sampler2D iChannel1;
                        sampler2D iChannel2;
                        sampler2D iChannel3;

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

                            // scale our normalized surface uv to some screen resolution values
                            float2 uv_unnormalized = uv * _ScreenParams.xy;

                            mainImage(color, uv_unnormalized);

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
        // TODO: post-decrement operator on float4 seems unsupported
        // TODO: const values cannot be in other const values in global scope, replace with defines?

        var helpers = @"
        
            float2 vec2(float x) { return float2(x,x); }
            float2 vec2(float x, float y) { return float2(x,y); }

            float3 vec3(float x) { return float3(x,x,x); }
            float3 vec3(float x, float y) { return float3(x,y,0.0); }
            float3 vec3(float x, float y, float z) { return float3(x,y,z); }

            float3 vec3(float2 xy, float z) { return float3(xy.x, xy.y, z); }
            float3 vec3(float x, float2 yz) { return float3(x, yz.x, yz.y); }

            float4 vec4(float x) { return float4(x,x,x,x); }
            float4 vec4(float x, float y) { return float4(x,y,0.0,0.0); }
            float4 vec4(float x, float y, float z) { return float4(x,y,z,0.0); }
            float4 vec4(float x, float y, float z, float w) { return float4(x,y,z,w); }

            // likely typos here
            float4 vec4(float2 xy, float z, float w) { return float4(xy.x, xy.y, z, w); }
            float4 vec4(float x, float2 yz, float w) { return float4(x, yz.x, yz.y, w); }
            float4 vec4(float x, float y, float2 zw) { return float4(x, y, zw.x, zw.y); }

            // likely typos here
            float4 vec4(float3 xyz, float w) { return float4(xyz.x, xyz.y, xyz.z, w); }
            float4 vec4(float x, float3 yzw) { return float4(x, yzw.x, yzw.y, yzw.z); }

            float2x2 mat2(float x0, float x1, float y0, float y1) { return float2x2(x0,x1,y0,y1); }

            float4 texture2D_wrapper(sampler2D s, float2 uv) { return tex2D(s, uv); }
            float4 texture2D_wrapper(sampler2D s, float2 uv, float bias) { return tex2D(s, uv); }

            #define mat2 float2x2
            #define mat3 float3x3
            #define mat4 float4x4

            #define ivec2 int2
            #define ivec3 int3
            #define ivec4 int4

            #define atan(x, y) atan2(y, x)
            #define mix lerp
            #define mod(x, y) fmod(abs(x), y)
            #define fract frac
        ";

        template = template.Replace("$HELPERS", helpers);

        code = code.Replace("iGlobalTime", "_Time.y");
        code = code.Replace("iResolution", "_ScreenParams");
        code = code.Replace("iMouse", "float2(0.0,0.0)");

        // some inline replacements
        code = code.Replace("ivec2 ", "int2 ");
        code = code.Replace("ivec3 ", "int3 ");
        code = code.Replace("ivec4 ", "int4 ");

        code = code.Replace("vec2 ", "float2 ");
        code = code.Replace("vec3 ", "float3 ");
        code = code.Replace("vec4 ", "float4 ");

        code = code.Replace("mat2 ", "float2x2 ");
        code = code.Replace("mat3 ", "float3x3 ");
        code = code.Replace("mat4 ", "float4x4 ");

        code = code.Replace("texture2D", "texture2D_wrapper");

        // mainImage(out vec4 fragColor, in vec2 fragCoord) is the fragment shader function, equivalent to float4 mainImage(float2 fragCoord : SV_POSITION) : SV_Target
        // UV coordinates in GLSL have 0 at the top and increase downwards, in HLSL 0 is at the bottom and increases upwards, so you may need to use uv.y = 1 – uv.y at some point.

        code = StripComments(code);

        code = ReplaceMulAssigns(code);
        code = ReplaceMuls(code);

        return template.Replace("$CODE", code);
    }

    private static string StripComments(string input)
    {
        var re = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/";
        return Regex.Replace(input, re, "$1");
    }

    private static string ReplaceMulAssigns(string input)
    {
        // walk backwards to find identifier a
        // walk forwards to find identifier b
        // insert identifiers into mul()

        while (true)
        {
            var mul = input.IndexOf("*=");
            if (mul == -1)
                break;

            var cursor = mul - 1;
            var depth = 0;

            var block_a_begin = -1;
            var block_a_end = -1;

            var block_b_begin = -1;
            var block_b_end = -1;

            // walk till non-whitespace
            while (cursor >= 0)
            {
                var c = input[cursor];
                if (!char.IsWhiteSpace(c))
                    break;
                cursor--;
            }

            // walk through chars until , or (
            block_a_end = cursor + 1;
            while (cursor >= 0)
            {
                var c = input[cursor];

                if (char.IsWhiteSpace(c))
                {
                    cursor++;
                    break;
                }

                if (c == '=')
                {
                    cursor++;
                    break;
                }

                if (c == ',')
                {
                    cursor++;
                    break;
                }

                if (c == ';')
                {
                    cursor++;
                    break;
                }

                if (c == '(')
                {
                    if (depth == 0)
                    {
                        cursor++;
                        break;
                    }
                    else
                        depth--;
                }

                if (c == ')')
                    depth++;

                cursor--;
            }
            block_a_begin = cursor;

            // walk till non-whitespace
            cursor = mul + 2;
            while (cursor < input.Length)
            {
                var c = input[cursor];
                if (!char.IsWhiteSpace(c))
                    break;
                cursor++;
            }

            // walk through chars until , or (
            block_b_begin = cursor;
            while (cursor < input.Length)
            {
                var c = input[cursor];

                if (char.IsWhiteSpace(c))
                {
                    cursor--;
                    break;
                }

                if (c == '=')
                {
                    cursor--;
                    break;
                }

                if (c == ',')
                {
                    cursor--;
                    break;
                }

                if (c == ';')
                {
                    cursor--;
                    break;
                }

                if (c == ')')
                {
                    if (depth == 0)
                    {
                        cursor--;
                        break;
                    }
                    else
                        depth--;
                }

                if (c == '(')
                    depth++;

                cursor++;
            }
            block_b_end = cursor + 1;

            var block_a = input.Substring(block_a_begin, block_a_end - block_a_begin);
            var block_b = input.Substring(block_b_begin, block_b_end - block_b_begin);
            var insert = string.Format("mul({0},{1})", block_a, block_b);

            input = input.Remove(block_a_begin, block_b_end - block_a_begin);
            input = input.Insert(block_a_begin, insert);
        }

        return input;
    }

    private static string ReplaceMuls(string input)
    {
        /*
        // test code (passes)
        void mainImage(out vec4 fragColor, in vec2 fragCoord)
        {
            vec3 test0 = vec3(1, 1, 1);
            vec3 test1 = vec3(1, 1, 1);
            vec3 test2 = test0 * test1;
            vec3 test3 = test0 * test1;
            vec3 test4 = (test0) * test1;
            vec3 test5 = ((test0)) * test1;
            vec3 test6 = ((test0)) * test1;

            vec3 test7 = test0 * (test1);
            vec3 test8 = test0 * ((test1));

            vec3 test9 = test0 * (test1);
            vec3 test10 = test0 * (test1);

            vec3 test11 = (test0 * test1);
            vec3 test12 = (test0) * (test1);
            vec3 test13 = ((test0) * (test1));

            float test14 = 0.0f;

            test14 *= 2.0f;

            float test15 = 1 * 2 * 3;
            float test16 = 1 * 2 * 3 * 4;

            float test17 = ((1 / 2) * 3);
            float test18 = (1 * (2 / 3));

            float test19 = ((1 * 2) / 3);
            float test20 = (1 / (2 * 3));

            float test21 = (1 * 2) / 3;
            float test22 = 1 / (2 * 3);

            fragColor = vec4(1, 1, 0, 1);
        }
        */

        // walk backwards to find identifier a
        // walk forwards to find identifier b
        // insert identifiers into mul()

        while (true)
        {
            var mul = input.IndexOf('*');
            if (mul == -1)
                break;

            var cursor = mul - 1;
            var depth = 0;

            var block_a_begin = -1;
            var block_a_end = -1;

            var block_b_begin = -1;
            var block_b_end = -1;

            // walk till non-whitespace
            while (cursor >= 0)
            {
                var c = input[cursor];
                if (!char.IsWhiteSpace(c))
                    break;
                cursor--;
            }

            // walk through chars until , or (
            block_a_end = cursor + 1;
            while (cursor >= 0)
            {
                var c = input[cursor];

                if (char.IsWhiteSpace(c) && depth == 0)
                {
                    cursor++;
                    break;
                }

                if (c == '=' && depth == 0)
                {
                    cursor++;
                    break;
                }

                if (c == ',' && depth == 0)
                {
                    cursor++;
                    break;
                }

                if (c == ';')
                {
                    cursor++;
                    break;
                }

                if (c == '(')
                {
                    if (depth == 0)
                    {
                        // might be function call, need to walk back to identifier start
                        while (cursor >= 0)
                        {
                            c = input[cursor];

                            var valid_identifier = false;

                            // ignore starting literal rule
                            if (char.IsLetterOrDigit(c))
                                valid_identifier = true;

                            if (c == '.')
                                valid_identifier = true;

                            if (!valid_identifier)
                                break;

                            cursor--;
                        }

                        cursor++;
                        break;
                    }
                    else
                        depth--;
                }

                if (c == ')')
                    depth++;

                cursor--;
            }
            block_a_begin = cursor;

            // walk till non-whitespace
            cursor = mul + 1;
            while (cursor < input.Length)
            {
                var c = input[cursor];
                if (!char.IsWhiteSpace(c))
                    break;
                cursor++;
            }

            // walk through chars until , or (
            block_b_begin = cursor;
            while (cursor < input.Length)
            {
                var c = input[cursor];

                if (char.IsWhiteSpace(c) && depth == 0)
                {
                    cursor--;
                    break;
                }

                if (c == '=' && depth == 0)
                {
                    cursor--;
                    break;
                }

                if (c == ',' && depth == 0)
                {
                    cursor--;
                    break;
                }

                if (c == ';')
                {
                    cursor--;
                    break;
                }

                if (c == ')')
                {
                    if (depth == 0)
                    {
                        cursor--;
                        break;
                    }
                    else
                        depth--;
                }

                if (c == '(')
                    depth++;

                cursor++;
            }
            block_b_end = cursor + 1;

            var block_a = input.Substring(block_a_begin, block_a_end - block_a_begin);
            var block_b = input.Substring(block_b_begin, block_b_end - block_b_begin);
            var insert = string.Format("mul({0},{1})", block_a, block_b);

            input = input.Remove(block_a_begin, block_b_end - block_a_begin);
            input = input.Insert(block_a_begin, insert);
        }

        return input;
    }
}

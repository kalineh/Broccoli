﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

/*
{
"Shader": {
"ver": "0.1",
"info": {
  "id": "XslGRr",
  "date": "1361810389",
  "viewed": "66653",
  "name": "Clouds",
  "username": "iq",
  "description": "Raymarching some fbm (you can move the mouse around). This is my first shader in the \"one shader a week for a year\" challenge that I have committed to.",
  "likes": "479",
  "published": "3",
  "flags": "1",
  "tags": [
    "procedural",
    "3d",
    "raymarching",
    "volumetric",
    "lod",
    "vr"
  ],
  "hasliked": "0"
},
"renderpass": [
  {
    "inputs": [
      {
        "id": "30",
        "src": "/presets/tex16.png",
        "ctype": "texture",
        "channel": "0",
        "sampler": {
          "filter": "mipmap",
          "wrap": "repeat",
          "vflip": "false",
          "srgb": "false",
          "internal": "byte"
        }
      }
    ],
    "outputs": [
      {
        "channel": "0",
        "dst": "-1"
      }
    ],
    "code": "// Created by inigo quilez - iq/2013\n// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.\n\n// Volumetric clouds. It performs level of detail (LOD) for faster rendering\n\nfloat noise( in vec3 x )\n{\n    vec3 p = floor(x);\n    vec3 f = fract(x);\n\tf = f*f*(3.0-2.0*f);\n\tvec2 uv = (p.xy+vec2(37.0,17.0)*p.z) + f.xy;\n\tvec2 rg = texture2D( iChannel0, (uv+ 0.5)/256.0, -100.0 ).yx;\n\treturn -1.0+2.0*mix( rg.x, rg.y, f.z );\n}\n\nfloat map5( in vec3 p )\n{\n\tvec3 q = p - vec3(0.0,0.1,1.0)*iGlobalTime;\n\tfloat f;\n    f  = 0.50000*noise( q ); q = q*2.02;\n    f += 0.25000*noise( q ); q = q*2.03;\n    f += 0.12500*noise( q ); q = q*2.01;\n    f += 0.06250*noise( q ); q = q*2.02;\n    f += 0.03125*noise( q );\n\treturn clamp( 1.5 - p.y - 2.0 + 1.75*f, 0.0, 1.0 );\n}\n\nfloat map4( in vec3 p )\n{\n\tvec3 q = p - vec3(0.0,0.1,1.0)*iGlobalTime;\n\tfloat f;\n    f  = 0.50000*noise( q ); q = q*2.02;\n    f += 0.25000*noise( q ); q = q*2.03;\n    f += 0.12500*noise( q ); q = q*2.01;\n    f += 0.06250*noise( q );\n\treturn clamp( 1.5 - p.y - 2.0 + 1.75*f, 0.0, 1.0 );\n}\nfloat map3( in vec3 p )\n{\n\tvec3 q = p - vec3(0.0,0.1,1.0)*iGlobalTime;\n\tfloat f;\n    f  = 0.50000*noise( q ); q = q*2.02;\n    f += 0.25000*noise( q ); q = q*2.03;\n    f += 0.12500*noise( q );\n\treturn clamp( 1.5 - p.y - 2.0 + 1.75*f, 0.0, 1.0 );\n}\nfloat map2( in vec3 p )\n{\n\tvec3 q = p - vec3(0.0,0.1,1.0)*iGlobalTime;\n\tfloat f;\n    f  = 0.50000*noise( q ); q = q*2.02;\n    f += 0.25000*noise( q );;\n\treturn clamp( 1.5 - p.y - 2.0 + 1.75*f, 0.0, 1.0 );\n}\n\nvec3 sundir = normalize( vec3(-1.0,0.0,-1.0) );\n\nvec4 integrate( in vec4 sum, in float dif, in float den, in vec3 bgcol, in float t )\n{\n    // lighting\n    vec3 lin = vec3(0.65,0.7,0.75)*1.4 + vec3(1.0, 0.6, 0.3)*dif;        \n    vec4 col = vec4( mix( vec3(1.0,0.95,0.8), vec3(0.25,0.3,0.35), den ), den );\n    col.xyz *= lin;\n    col.xyz = mix( col.xyz, bgcol, 1.0-exp(-0.003*t*t) );\n    // front to back blending    \n    col.a *= 0.4;\n    col.rgb *= col.a;\n    return sum + col*(1.0-sum.a);\n}\n\n#define MARCH(STEPS,MAPLOD) for(int i=0; i<STEPS; i++) { vec3  pos = ro + t*rd; if( pos.y<-3.0 || pos.y>2.0 || sum.a > 0.99 ) break; float den = MAPLOD( pos ); if( den>0.01 ) { float dif =  clamp((den - MAPLOD(pos+0.3*sundir))/0.6, 0.0, 1.0 ); sum = integrate( sum, dif, den, bgcol, t ); } t += max(0.05,0.02*t); }\n\nvec4 raymarch( in vec3 ro, in vec3 rd, in vec3 bgcol )\n{\n\tvec4 sum = vec4(0.0);\n\n\tfloat t = 0.0;\n\n    MARCH(30,map5);\n    MARCH(30,map4);\n    MARCH(30,map3);\n    MARCH(30,map2);\n\n    return clamp( sum, 0.0, 1.0 );\n}\n\nmat3 setCamera( in vec3 ro, in vec3 ta, float cr )\n{\n\tvec3 cw = normalize(ta-ro);\n\tvec3 cp = vec3(sin(cr), cos(cr),0.0);\n\tvec3 cu = normalize( cross(cw,cp) );\n\tvec3 cv = normalize( cross(cu,cw) );\n    return mat3( cu, cv, cw );\n}\n\nvec4 render( in vec3 ro, in vec3 rd )\n{\n    // background sky     \n\tfloat sun = clamp( dot(sundir,rd), 0.0, 1.0 );\n\tvec3 col = vec3(0.6,0.71,0.75) - rd.y*0.2*vec3(1.0,0.5,1.0) + 0.15*0.5;\n\tcol += 0.2*vec3(1.0,.6,0.1)*pow( sun, 8.0 );\n\n    // clouds    \n    vec4 res = raymarch( ro, rd, col );\n    col = col*(1.0-res.w) + res.xyz;\n    \n    // sun glare    \n\tcol += 0.2*vec3(1.0,0.4,0.2)*pow( sun, 3.0 );\n\n    return vec4( col, 1.0 );\n}\n\nvoid mainImage( out vec4 fragColor, in vec2 fragCoord )\n{\n    vec2 p = (-iResolution.xy + 2.0*fragCoord.xy)/ iResolution.y;\n\n    vec2 m = iMouse.xy/iResolution.xy;\n    \n    // camera\n    vec3 ro = 4.0*normalize(vec3(sin(3.0*m.x), 0.4*m.y, cos(3.0*m.x)));\n\tvec3 ta = vec3(0.0, -1.0, 0.0);\n    mat3 ca = setCamera( ro, ta, 0.0 );\n    // ray\n    vec3 rd = ca * normalize( vec3(p.xy,1.5));\n    \n    fragColor = render( ro, rd );\n}\n\nvoid mainVR( out vec4 fragColor, in vec2 fragCoord, in vec3 fragRayOri, in vec3 fragRayDir )\n{\n    fragColor = render( fragRayOri, fragRayDir );\n}",
    "type": "image"
  }
]
}
}
*/


public class ShaderToyTest
    : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) { TestShaderSphere("ldtGDr"); }

        if (Input.GetKeyDown(KeyCode.S)) { TestShaderConvert(); }
        if (Input.GetKeyDown(KeyCode.D)) { StartCoroutine(TestShaderConvertRepeat());  }
    }

    public void TestShaderSphere(string key)
    {
        var stm = ShaderToyCache.Instance.FindShaderToyMaterial(key);
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere.GetComponent<Renderer>().material = stm.material;

        sphere.AddComponent<Rigidbody>();

        Destroy(sphere, 60.0f);
    }

    // debug
    public IEnumerator TestShaderConvert()
    {
        Debug.Log("TestShaderConvert(): enter");

        var str = File.ReadAllText("Assets/ShaderToyManualConvertShaderGlsl.txt");
        var converted = ShaderToyToUnity.Convert("TestShader", str);
        File.WriteAllText("Assets/ShaderToyManualConvertShader.shader", converted);

        Debug.Log("TestShaderConvert(): done");

        yield return null;
    }

    // debug
    public IEnumerator TestShaderConvertRepeat()
    {
        Debug.Log("TestShaderConvertRepeat(): enter");

        while (true)
        {
            Debug.Log("TestShaderConvertRepeat(): reloading");

            var path = "Assets/ShaderToyManualConvertShaderGlsl.txt";
            var file = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
            var reader = new BinaryReader(file);
            var bytes = reader.ReadBytes((int)file.Length);
            var str = System.Text.Encoding.Default.GetString(bytes);
            var converted = ShaderToyToUnity.Convert("TestShader", str);
            File.WriteAllText("Assets/ShaderToyManualConvertShader.shader", converted);

            var timestamp = File.GetLastWriteTimeUtc(path);

            Application.runInBackground = true;
            AssetDatabase.Refresh();

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.L))
                {
                    Application.runInBackground = false;
                    yield break;
                }

                if (File.GetLastWriteTimeUtc(path) > timestamp)
                    break;

                yield return null;
            }
        }
    }
}

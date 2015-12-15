using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

public class ShaderToyCache
{
    private static ShaderToyCache _Instance;
    public static ShaderToyCache Instance { get { _Instance = _Instance ?? new ShaderToyCache(); return _Instance; } }

    public List<string> Keys = new List<string>();
    public List<ShaderToy> ShaderToys = new List<ShaderToy>();
    public List<ShaderToyMaterial> Materials = new List<ShaderToyMaterial>();

    public void Update()
    {
        
    }

    public bool HasKey(string key)
    {
        foreach (var s in Keys)
        {
            if (s == key)
                return true;
        }

        return false;
    }

    public ShaderToy FindShaderToy(string key)
    {
        foreach (var s in ShaderToys)
        {
            if (s.id == key)
                return s;
        }

        return null;
    }

    public ShaderToyMaterial FindShaderToyMaterial(string key)
    {
        foreach (var m in Materials)
        {
            if (m.id == key)
                return m;
        }

        var path = String.Format("Assets/{0}.mat", key);
        var material = new Material(Shader.Find("Diffuse"));

        AssetDatabase.CreateAsset(material, path);

        var asset = AssetDatabase.LoadAssetAtPath<Material>(path);

        var stm = new ShaderToyMaterial();

        stm.id = key;
        stm.material = material;
        //stm.channel0 = material.GetTexture("iChannel0");
        //stm.channel1 = material.GetTexture("iChannel1");
        //stm.channel2 = material.GetTexture("iChannel2");
        //stm.channel3 = material.GetTexture("iChannel3");

        return stm;
    }

    // find key
    // find material
    // find shadertoy
}

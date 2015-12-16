using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;
using CielaSpike;

public class ShaderToyCacheProxy
    : MonoBehaviour
{
};

public class ShaderToyCache
{
    private static GameObject proxy;

    public static List<string> Keys = new List<string>();
    public static List<ShaderToy> ShaderToys = new List<ShaderToy>();

    private static Mutex mutex;
    private static List<string> requests = new List<string>();
    private static List<string> processing = new List<string>();

    static ShaderToyCache()
    {
        proxy = new GameObject("ShaderToyCacheProxy");
        mutex = new Mutex();

        proxy.AddComponent<ShaderToyCacheProxy>();
        proxy.GetComponent<ShaderToyCacheProxy>().StartCoroutineAsync(LoaderTask());
    }

    public static void Request(string key)
    {
        if (!mutex.WaitOne(100))
            return;

        if (!requests.Contains(key) && !processing.Contains(key))
            requests.Add(key);

        mutex.ReleaseMutex();
    }

    private static IEnumerator LoaderTask()
    {
        yield return Ninja.JumpBack;

        while (true)
        {
            Thread.Sleep(100);

            if (!mutex.WaitOne(100))
                continue;

            processing.AddRange(requests);
            requests.Clear();

            mutex.ReleaseMutex();

            foreach (var key in processing)
            {
                Debug.LogFormat("ShaderToyCache: generating {0}", key);

                var shadertoy = ShaderToyAPI.DownloadShaderToy(key);
                var material = ShaderToyAPI.GenerateMaterial(shadertoy);

                var existing_shadertoy = GetShaderToy(key);

                foreach (var s in ShaderToys)
                {
                    if (s.id == key)
                    {
                        ShaderToys.Remove(s);
                        break;
                    }
                }

                ShaderToys.Add(shadertoy);
            }

            if (mutex.WaitOne(100))
            {
                processing.Clear();
                mutex.ReleaseMutex();
            }
        }
    }

    public static bool HasKey(string key)
    {
        foreach (var s in Keys)
        {
            if (s == key)
                return true;
        }

        return false;
    }

    public static void LoadShaderToy(string key)
    {
        var shadertoy = ShaderToyAPI.DownloadShaderToy(key);

        ShaderToys.Add(shadertoy);
    }

    public static bool HasShaderToy(string key)
    {
        foreach (var s in ShaderToys)
        {
            if (s.id == key)
                return true;
        }

        return false;
    }

    public static ShaderToy GetShaderToy(string key)
    {
        foreach (var s in ShaderToys)
        {
            if (s.id == key)
                return s;
        }

        return null;
    }

    public static Material GetMaterial(string key)
    {
        foreach (var s in ShaderToys)
        {
            if (s.id == key)
                return s.material;
        }

        return null;

        //var path = String.Format("Assets/{0}.mat", key);
        //var material = new Material(Shader.Find("Diffuse"));
        //AssetDatabase.CreateAsset(material, path);
        //var asset = AssetDatabase.LoadAssetAtPath<Material>(path);
        //return material;
    }
}

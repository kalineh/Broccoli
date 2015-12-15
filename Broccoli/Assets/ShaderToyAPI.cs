using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

public class ShaderToy
{
    public string id;
    public string name;
    public string username;
    public string code;
}

public class ShaderToyMaterial
{
    public string id;
    public Material material;
    public object channel0;
    public object channel1;
    public object channel2;
    public object channel3;
}

public class ShaderToyAPI
{
    private static ShaderToyAPI _Instance;
    public static ShaderToyAPI Instance { get { _Instance = _Instance ?? new ShaderToyAPI(); return _Instance; } }

    public string UnityExePathHint = "";
    private string UnityExePath = "";

    private string APIKey = "fdHtwN";
    private string APIURL = "https://www.shadertoy.com/api/v1/";
    private string APIShaders = "shaders";
    private string APIShader = "shaders";
    private string ResourceURL = "https://www.shadertoy.com/";

    public void Start()
    {
        UnityExePath = FindUnityExe();
    }

    public void Update()
    {
        if (String.IsNullOrEmpty(UnityExePath))
        {
            UnityExePath = FindUnityExe();
        }
    }

    string FindUnityExe()
    {
        if (File.Exists(UnityExePathHint))
            return UnityExePathHint;

        var guess1 = Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES"), @"Unity\Editor\Unity.exe");
        if (File.Exists(guess1))
            return guess1;

        var guess2 = Microsoft.Win32.Registry.GetValue(@"HKEY_CLASSES_ROOT\com.unity3d.kharma\DefaultIcon", "", null) as string;
        if (File.Exists(guess2))
            return guess2;

        // NOTE：not implemented in mono
        /*
        //var drives = DriveInfo.GetDrives();

        foreach (var drive in drives)
        {
            if (drive.DriveType != DriveType.Fixed)
                continue;

            var root = drive.RootDirectory.FullName;
            var files = Directory.GetFiles(root, "Unity.exe", SearchOption.AllDirectories);

            if (files.Length > 0)
                return files[0];
        }
        */

        var root = "C:";
        var files = Directory.GetFiles(root, "Unity.exe", SearchOption.AllDirectories);

        if (files.Length > 0)
            return files[0];

        return "Unity.exe";
    }

    string BuildRequestURL(string req)
    {
        return string.Format("{0}{1}?key={2}", APIURL, req, APIKey);
    }

    string BuildRequestParamURL(string req, string param)
    {
        return string.Format("{0}{1}/{2}?key={3}", APIURL, req, param, APIKey);
    }

    public List<string> DownloadShaderKeys()
    {
        Debug.Log("ShaderToyAPI.DownloadShaderKeys(): starting...");

        var url = BuildRequestURL(APIShaders);
        var www = new WWW(url);

        Debug.LogFormat("> requesting: {0}", url);

        while (!www.isDone)
        {
            Thread.Sleep(0);
        }

        var results = www.text;
        var json = JSON.Parse(results);

        // "Shaders": count
        // "Results": [ keys, ]

        var count = json["Shaders"].AsInt;
        var keys = json["Results"].AsArray;

        var result = new List<string>();

        foreach (var node in keys)
        {
            var str = node.ToString();

            str = str.Trim('\"');

            result.Add(str);
        }

        Debug.Log("ShaderToyAPI.DownloadShaderKeys(): done.");

        return result;
    }

    public ShaderToy DownloadShaderToy(string key)
    {
        Debug.Log("ShaderToyAPI.DownloadShaderToy(): starting...");

        var url = BuildRequestParamURL(APIShader, key);
        var www = new WWW(url);

        Debug.LogFormat("> requesting: {0}", url);

        while (!www.isDone)
        {
            Thread.Sleep(0);
        }

        var results = www.text;
        var json = JSON.Parse(results);

        var result = json.ToString();

        var shader = new ShaderToy();

        var json_info = json["Shader"]["info"];
        var json_renderpass = json["Shader"]["renderpass"];

        shader.id = json_info["id"];
        shader.name = json_info["name"];
        shader.username = json_info["username"];

        shader.code = json_renderpass[0]["code"];

        Debug.Log(result);
        Debug.Log(shader.code);

        Debug.Log("ShaderToyAPI.DownloadShaderToy(): done.");

        return shader;
    }

    public Material GenerateMaterial(ShaderToy shadertoy)
    {
        Debug.Log("ShaderToyAPI.GenerateMaterial(): starting...");

        var key = shadertoy.id;
        var code = shadertoy.code;

        var converted = ShaderToyToUnity.Convert(key, code);

        Debug.Log(converted);

        var temp = Environment.GetEnvironmentVariable("TEMP");
        var file = String.Format("{0}.shader", key);
        var shader_dir = Path.Combine(temp, "ShaderToyGenerated");
        var shader_path = Path.Combine(shader_dir, file);

        Debug.LogFormat("> writing: {0}", shader_path);

        Directory.CreateDirectory(shader_dir);
        File.WriteAllText(shader_path, converted);

        var bundle_path = Path.ChangeExtension(shader_path, "assetbundle");

        var process_filename = Path.GetFullPath(@"Assets\buildtools\build_shader.bat");
        var process_exists = File.Exists(process_filename);

        var args = String.Format("\"{0}\" \"{1}\" \"{2}\"", UnityExePath, shader_dir, bundle_path);
        var info = new System.Diagnostics.ProcessStartInfo(process_filename, args);

        info.WorkingDirectory = @"Assets\buildtools";
        info.UseShellExecute = false;
        info.CreateNoWindow = true;
        info.RedirectStandardOutput = true;

        var proc = new System.Diagnostics.Process() { StartInfo = info, };

        Debug.LogFormat("> starting batch: {0} {1}", info.FileName, info.Arguments);

        proc.Start();

        var stdout = proc.StandardOutput.ReadToEnd();

        proc.WaitForExit();

        Debug.Log(stdout);

        var bundle_exists = File.Exists(bundle_path);

        Debug.LogFormat("> bundle exists: {0}", bundle_exists);

        if (!bundle_exists)
            return null;

        var bundle = AssetBundle.CreateFromFile(bundle_path);
        var bundle_assets = bundle.GetAllAssetNames();

        Debug.LogFormat("> bundle:");
        foreach (var name in bundle_assets)
        {
            Debug.LogFormat("> * {0}", name);
        }

        var bundle_shader_name = String.Format("assets/{0}.shader", key);
        var bundle_shader = bundle.LoadAsset<Shader>(bundle_shader_name);

        Debug.LogFormat("> shader: {0}", bundle_shader.ToString());

        if (!bundle_shader)
        {
            bundle.Unload(false);
            return null;
        }

        var material = new Material(bundle_shader);

        bundle.Unload(false);

        return material;
    }
}

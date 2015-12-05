using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class AssetBundler
	: MonoBehaviour
{
	public static string outputPath = "";
	public static List<string> assetPaths = new List<string>();
	public static List<Object> assets = new List<Object>();

	public static void Bundle()
	{
		BuildList();

		Debug.LogFormat("AssetBundler.Bundle(): bundling {0} items", assetPaths.Count);

		var assets = new List<Object>();

		foreach (var path in assetPaths)
		{
			Debug.LogFormat("> {0}", path);

			// TODO: need to handle all types

			var path_stripped = Path.GetFileName(path);
			var shader = AssetDatabase.LoadAssetAtPath<Shader>(path_stripped.ToString());

			assets.Add(shader);
		}

		var manifest = BuildPipeline.BuildAssetBundles(
			"Assets/Generated",
			BuildAssetBundleOptions.ForceRebuildAssetBundle,
			BuildTarget.StandaloneWindows
		);

		if (manifest != null)
		{
			Debug.LogFormat("> manifest {0}", manifest.ToString());
			Debug.LogFormat("> name: {0}", manifest.name);
			Debug.LogFormat("> files: {0}", manifest.GetAllAssetBundles().Length);
		}

		Debug.LogFormat("AssetBundler.Bundle(): done");
	}

	public static void BuildList()
	{
		// paths added below

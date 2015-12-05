using UnityEngine;
using UnityEditor;
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

			// should only be one since it is a direct path

			var objs = AssetDatabase.LoadAllAssetsAtPath(path);

			foreach (var obj in objs)
			{
				assets.Add(obj);
			}
		}

		BuildPipeline.BuildAssetBundles(
			outputPath,
			BuildAssetBundleOptions.ForceRebuildAssetBundle,
			BuildTarget.StandaloneWindows
		);

		Debug.LogFormat("AssetBundler.Bundle(): done");
	}

	public static void BuildList()
	{
		// paths added below

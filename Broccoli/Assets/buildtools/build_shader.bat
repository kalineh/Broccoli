
SET UNITY_EXEC=%1
SET SRC_DIR=%2
SET UNITY_ASSET_BUNDLE_PATH=%3

SET TEMP_PROJECT_NAME=ShaderToyGeneratedProject
SET TEMP_PROJECT_DIR=%TEMP%\%TEMP_PROJECT_NAME%

echo "Creating temporary project at '%TEMP_PROJECT_DIR%'"

%UNITY_EXEC% -batchmode -quit -createProject %TEMP_PROJECT_DIR%

echo "Copying resources from source to assets folder."

xcopy /y /s %SRC_DIR% %TEMP_PROJECT_DIR%\Assets

echo "Creating temporary project folders."

mkdir %TEMP_PROJECT_DIR%\Assets
mkdir %TEMP_PROJECT_DIR%\Assets\Editor
mkdir %TEMP_PROJECT_DIR%\Assets\Generated

SET HELPER_SCRIPT=%TEMP_PROJECT_DIR%\Assets\Editor\AssetBundler.cs

echo "Generating asset bundler script."

dir 

type AssetBundlerOpen.cs.template > %HELPER_SCRIPT%

for %%x in (%TEMP_PROJECT_DIR%\Assets\*) do echo assetPaths.Add(@"%%x"); >> %HELPER_SCRIPT%

type AssetBundlerClose.cs.template >> %HELPER_SCRIPT%

echo "Building asset bundle."

%UNITY_EXEC% -batchmode -quit -projectProject %TEMP_PROJECT_DIR% -executeMethod AssetBundler.Bundle

echo "Copying asset bundle back."

echo F | xcopy /y /s %TEMP_PROJECT_DIR%\Assets\Generated\generated.assetbundle %UNITY_ASSET_BUNDLE_PATH%

echo "Deleting temporary project."

echo del /f /s %TEMP_PROJECT_DIR%

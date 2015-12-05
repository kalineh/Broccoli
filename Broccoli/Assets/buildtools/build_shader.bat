
SET UNITY_EXEC="D:/dev/unity/Editor/Unity.exe"
SET SRC_DIR=%1
SET UNITY_ASSET_BUNDLE_PATH=%2

SET TEMP_PROJECT_NAME=TempProject
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

cat AssetBundlerTemplateOpen.cs > %HELPER_SCRIPT%

echo outputPath = @"%UNITY_ASSET_BUNDLE_PATH%"; >> %HELPER_SCRIPT%

for %%x in (%TEMP_PROJECT_DIR%\Assets\*) do echo assetPaths.Add(@"%%x"); >> %HELPER_SCRIPT%

cat AssetBundlerTemplateClose.cs >> %HELPER_SCRIPT%

echo "Building asset bundle."

%UNITY_EXEC%  -projectProject %TEMP_PROJECT_DIR% -executeMethod AssetBundler.Bundle

echo "Deleting temporary project."

echo del /f /s %TEMP_PROJECT_DIR%



using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

namespace PCShopEmpire3D.Editor
{
    public static class StageATechnicalChecks
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string UvcsServer = "cixanlas@cloud";
        private const string UvcsRepositoryName = "PC Shop Empire 3D/pc-shop-empire-3d";
        private const string UvcsRepositorySpec = UvcsRepositoryName + "@" + UvcsServer;
        private const string UvcsWorkspaceName = "PCShopEmpire3D-MacBook";

        [MenuItem("PC Shop Empire/Stage A/Validate Technical Baseline")]
        public static void ValidateTechnicalBaseline()
        {
            Require(EditorSettings.serializationMode == SerializationMode.ForceText,
                "Asset serialization must be Force Text.");
            Require(VersionControlSettings.mode == "Visible Meta Files",
                "External version control mode must expose meta files.");
            Require(GraphicsSettings.defaultRenderPipeline != null,
                "A default URP render pipeline asset must be configured.");
            Require(File.Exists(Path.Combine(ProjectRoot, SampleScenePath)),
                $"Build scene is missing: {SampleScenePath}");

            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            Require(enabledScenes.Contains(SampleScenePath),
                "The sample scene must be enabled in build settings.");

            string manifest = File.ReadAllText(Path.Combine(ProjectRoot, "Packages", "manifest.json"));
            string[] requiredPackages =
            {
                "com.unity.ai.navigation",
                "com.unity.collab-proxy",
                "com.unity.ide.visualstudio",
                "com.unity.inputsystem",
                "com.unity.probuilder",
                "com.unity.render-pipelines.universal",
                "com.unity.test-framework"
            };

            foreach (string packageId in requiredPackages)
            {
                Require(manifest.Contains($"\"{packageId}\"", StringComparison.Ordinal),
                    $"Required package is not pinned: {packageId}");
            }

            Debug.Log("STAGE_A_VALIDATION_OK");
        }

        [MenuItem("PC Shop Empire/Stage A/Build Mac Development Player")]
        public static void BuildMacDevelopmentPlayer()
        {
            Build(BuildTarget.StandaloneOSX, Path.Combine(BuildRoot, "macOS", "PC Shop Empire 3D.app"));
        }

        [MenuItem("PC Shop Empire/Stage A/Build Windows Mono Development Player")]
        public static void BuildWindowsMonoDevelopmentPlayer()
        {
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Build(BuildTarget.StandaloneWindows64,
                Path.Combine(BuildRoot, "Windows-Mono-x64", "PC Shop Empire 3D.exe"));
        }

        [MenuItem("PC Shop Empire/Stage A/Build Windows IL2CPP Development Player")]
        public static void BuildWindowsIl2CppDevelopmentPlayer()
        {
            const BuildTarget target = BuildTarget.StandaloneWindows64;
            NamedBuildTarget standalone = NamedBuildTarget.Standalone;
            ScriptingImplementation previousBackend =
                PlayerSettings.GetScriptingBackend(standalone);
            bool previousUseDefaultGraphicsApis =
                PlayerSettings.GetUseDefaultGraphicsAPIs(target);
            GraphicsDeviceType[] previousGraphicsApis =
                PlayerSettings.GetGraphicsAPIs(target)?.ToArray() ??
                Array.Empty<GraphicsDeviceType>();
            Require(
                previousGraphicsApis.Length > 0,
                "Windows graphics API snapshot is empty; refusing to mutate player settings.");
            string projectSettingsPath = Path.Combine(
                ProjectRoot,
                "ProjectSettings",
                "ProjectSettings.asset");
            Require(
                File.Exists(projectSettingsPath),
                "ProjectSettings.asset is missing; refusing to mutate player settings.");
            byte[] previousProjectSettingsBytes =
                File.ReadAllBytes(projectSettingsPath);
            Require(
                previousProjectSettingsBytes.Length > 0,
                "ProjectSettings.asset snapshot is empty; refusing to mutate player settings.");

            string outputPath = Path.Combine(
                BuildRoot,
                "Windows-IL2CPP-x64",
                "PC Shop Empire 3D.exe");
            BuildReport report = null;
            Exception primaryFailure = null;
            Exception restoreFailure = null;

            try
            {
                PlayerSettings.SetScriptingBackend(
                    standalone,
                    ScriptingImplementation.IL2CPP);
                PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
                PlayerSettings.SetGraphicsAPIs(
                    target,
                    new[] { GraphicsDeviceType.Direct3D11 });

                Require(
                    PlayerSettings.GetScriptingBackend(standalone) ==
                    ScriptingImplementation.IL2CPP,
                    "Windows IL2CPP backend readback mismatch.");
                Require(
                    !PlayerSettings.GetUseDefaultGraphicsAPIs(target),
                    "Windows D3D11 automatic graphics API readback mismatch.");
                Require(
                    GraphicsApisMatchExactly(
                        PlayerSettings.GetGraphicsAPIs(target),
                        GraphicsDeviceType.Direct3D11),
                    "Windows D3D11 graphics API readback mismatch.");

                report = Build(target, outputPath, emitSuccessMarker: false);
            }
            catch (Exception exception)
            {
                primaryFailure = exception;
            }
            finally
            {
                try
                {
                    PlayerSettings.SetUseDefaultGraphicsAPIs(target, false);
                    PlayerSettings.SetGraphicsAPIs(target, previousGraphicsApis);
                    PlayerSettings.SetUseDefaultGraphicsAPIs(
                        target,
                        previousUseDefaultGraphicsApis);
                    PlayerSettings.SetScriptingBackend(standalone, previousBackend);

                    Require(
                        PlayerSettings.GetScriptingBackend(standalone) == previousBackend,
                        "Windows scripting backend restore readback mismatch.");
                    Require(
                        PlayerSettings.GetUseDefaultGraphicsAPIs(target) ==
                        previousUseDefaultGraphicsApis,
                        "Windows automatic graphics API restore readback mismatch.");
                    Require(
                        GraphicsApisMatchExactly(
                            PlayerSettings.GetGraphicsAPIs(target),
                            previousGraphicsApis),
                        "Windows graphics API list restore readback mismatch.");
                    RestoreProjectSettingsFileExactly(
                        projectSettingsPath,
                        previousProjectSettingsBytes);
                }
                catch (Exception exception)
                {
                    restoreFailure = exception;
                }
            }

            if (primaryFailure != null && restoreFailure != null)
            {
                throw new AggregateException(
                    "Windows IL2CPP/D3D11 build and settings restore both failed.",
                    primaryFailure,
                    restoreFailure);
            }

            if (primaryFailure != null)
            {
                ExceptionDispatchInfo.Capture(primaryFailure).Throw();
            }

            if (restoreFailure != null)
            {
                ExceptionDispatchInfo.Capture(restoreFailure).Throw();
            }

            Require(
                report != null && report.summary.result == BuildResult.Succeeded,
                "Windows IL2CPP build report is missing or not successful.");
            Debug.Log(
                $"STAGE_A_BUILD_OK target={target} bytes={report.summary.totalSize} " +
                $"path={outputPath} scripting-backend=IL2CPP " +
                "graphics-api=Direct3D11 settings-restored=ok " +
                "project-settings=byte-exact");
        }

        [MenuItem("PC Shop Empire/Stage A/Configure Unity Version Control %#u")]
        public static void ConfigureUvcsCredentials()
        {
            string unityAccessToken = CloudProjectSettings.accessToken;
            Require(!string.IsNullOrWhiteSpace(unityAccessToken),
                "The Unity account access token is unavailable. Launch the project from Unity Hub and try again.");

            Type autoConfigType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(
                    "Unity.PlasticSCM.Editor.Configuration.AutoConfig",
                    throwOnError: false))
                .FirstOrDefault(type => type != null);
            Require(autoConfigType != null,
                "The Unity Version Control AutoConfig service is unavailable.");

            MethodInfo configureMethod = autoConfigType.GetMethod(
                "PlasticCredentials",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(configureMethod != null,
                "The Unity Version Control credential exchange method is unavailable.");

            object response = configureMethod.Invoke(
                null,
                new object[] { unityAccessToken, UvcsServer });
            Require(response != null,
                "Unity Version Control did not return a credential exchange response.");

            Type responseType = response.GetType();
            PropertyInfo errorProperty = responseType.GetProperty("Error");
            PropertyInfo accessTokenProperty = responseType.GetProperty("AccessToken");
            Require(errorProperty?.GetValue(response) == null,
                "Unity Version Control credential exchange returned an error.");
            Require(accessTokenProperty?.GetValue(response) is string token &&
                    !string.IsNullOrWhiteSpace(token),
                "Unity Version Control returned an empty access token.");

            Debug.Log($"STAGE_A_UVCS_CREDENTIALS_OK server={UvcsServer}");
        }

        [MenuItem("PC Shop Empire/Stage A/Create UVCS Workspace And Initial Checkin")]
        public static void CreateUvcsWorkspaceAndInitialCheckin()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Confirm Unity Version Control Initial Check-in",
                "This operation will write Unity Version Control credentials, create or reuse a " +
                $"workspace at:\n\n{ProjectRoot}\n\nand perform the first remote check-in to:\n\n" +
                $"{UvcsRepositorySpec}\n\nContinue only with explicit project-owner approval.",
                "Continue",
                "Cancel");
            if (!confirmed)
            {
                Debug.Log("STAGE_A_UVCS_WORKSPACE_CANCELLED");
                return;
            }

            try
            {
                ConfigureUvcsCredentials();
                AssetDatabase.SaveAssets();

                Type plasticAppType = FindRequiredType("Unity.PlasticSCM.Editor.PlasticApp");
                InvokeRequiredStaticMethod(plasticAppType, "InitializeIfNeeded");

                Type plasticType = FindRequiredType("PlasticGui.Plastic");
                Type plasticApiType = FindRequiredType("PlasticGui.IPlasticAPI");
                Type workspaceInfoType = FindRequiredType("Codice.CM.Common.WorkspaceInfo");
                Type repositorySpecType = FindRequiredType("Codice.CM.Common.RepositorySpec");
                object plasticApi = GetRequiredStaticProperty(plasticType, "API");

                MethodInfo buildRepositorySpecMethod = RequireMethod(
                    repositorySpecType,
                    "BuildFromNameAndResolvedServer",
                    BindingFlags.Static | BindingFlags.Public,
                    new[] { typeof(string), typeof(string) });
                object repositorySpec = buildRepositorySpecMethod.Invoke(
                    null,
                    new object[] { UvcsRepositoryName, UvcsServer });
                Require(repositorySpec != null &&
                        string.Equals(
                            repositorySpec.ToString(),
                            UvcsRepositorySpec,
                            StringComparison.Ordinal),
                    "Unity Version Control resolved an unexpected repository specification.");

                MethodInfo checkRepositoryExistsMethod = RequireMethod(
                    plasticApiType,
                    "CheckRepositoryExists",
                    BindingFlags.Instance | BindingFlags.Public,
                    new[] { typeof(string), typeof(string) });
                object repositoryExistsResult = checkRepositoryExistsMethod.Invoke(
                    plasticApi,
                    new object[] { UvcsServer, UvcsRepositoryName });
                Require(repositoryExistsResult is bool repositoryExists && repositoryExists,
                    $"The existing Unity Version Control repository was not found: {UvcsRepositorySpec}");

                Type emptyRepositoryConditionType = FindRequiredType(
                    "PlasticGui.Help.Conditions.IsEmptyRepositoryCondition");
                MethodInfo evaluateRepositoryEmptyMethod = RequireMethod(
                    emptyRepositoryConditionType,
                    "Evaluate",
                    BindingFlags.Static | BindingFlags.Public,
                    new[] { workspaceInfoType, repositorySpecType, plasticApiType });
                Require(IsRepositoryEmpty(
                        evaluateRepositoryEmptyMethod,
                        null,
                        repositorySpec,
                        plasticApi),
                    "The remote repository is not empty. Automatic workspace mapping was stopped.");

                Type apiInterfaceType = FindRequiredType(
                    "Unity.PlasticSCM.Editor.Api.IUnityVersionControlApi");
                Type apiImplementationType = FindRequiredType(
                    "Unity.PlasticSCM.Editor.Api.UnityVersionControlApi");
                object api = Activator.CreateInstance(apiImplementationType, nonPublic: true);
                Require(api != null, "The Unity Version Control API could not be created.");

                MethodInfo getWorkspaceMethod = RequireMethod(
                    apiInterfaceType,
                    "GetWorkspaceFromPath");
                object workspaceInfo = AwaitTaskResult(
                    getWorkspaceMethod.Invoke(api, new object[] { ProjectRoot }),
                    "resolve existing workspace",
                    allowNullResult: true);
                bool reusedExistingWorkspace = workspaceInfo != null;
                string workspaceMetadataDirectory = Path.Combine(ProjectRoot, ".plastic");
                if (!reusedExistingWorkspace)
                {
                    Require(!Directory.Exists(workspaceMetadataDirectory) &&
                            !File.Exists(workspaceMetadataDirectory),
                        "Unrecognized .plastic metadata exists at the project root. " +
                        "Automatic workspace creation was stopped.");

                    Type inputValidatorType = FindRequiredType("PlasticGui.InputValidator");
                    MethodInfo checkWorkspaceExistsMethod = RequireMethod(
                        inputValidatorType,
                        "CheckWorkspaceExists",
                        BindingFlags.Static | BindingFlags.Public,
                        new[]
                        {
                            typeof(string),
                            typeof(string),
                            typeof(string).MakeByRefType()
                        });
                    object[] validationArguments =
                    {
                        UvcsWorkspaceName,
                        ProjectRoot,
                        null
                    };
                    object workspaceConflictResult = checkWorkspaceExistsMethod.Invoke(
                        null,
                        validationArguments);
                    Require(workspaceConflictResult is bool,
                        "Unity Version Control returned an invalid workspace conflict result.");
                    Require(!(bool)workspaceConflictResult,
                        validationArguments[2] as string ??
                        "The workspace name or path conflicts with an existing workspace.");

                    MethodInfo createWorkspaceMethod = RequireMethod(
                        apiInterfaceType,
                        "CreateWorkspace");
                    workspaceInfo = AwaitTaskResult(
                        createWorkspaceMethod.Invoke(
                            api,
                            new object[]
                            {
                                UvcsWorkspaceName,
                                ProjectRoot,
                                UvcsRepositorySpec
                            }),
                        "create workspace");
                }

                Require(workspaceInfo != null,
                    "Unity Version Control did not return workspace information.");
                string actualWorkspaceName = ReadRequiredStringMember(workspaceInfo, "Name");
                Require(PathsEqual(
                        ReadRequiredStringMember(workspaceInfo, "ClientPath"),
                        ProjectRoot),
                    "Unity Version Control returned an unexpected workspace path.");

                MethodInfo getRepositorySpecMethod = RequireMethod(
                    plasticApiType,
                    "GetRepositorySpec",
                    BindingFlags.Instance | BindingFlags.Public,
                    new[] { workspaceInfoType });
                object mappedRepositorySpec = getRepositorySpecMethod.Invoke(
                    plasticApi,
                    new[] { workspaceInfo });
                Require(mappedRepositorySpec != null &&
                        string.Equals(
                            mappedRepositorySpec.ToString(),
                            UvcsRepositorySpec,
                            StringComparison.Ordinal),
                    "The workspace is mapped to an unexpected repository.");
                Require(IsRepositoryEmpty(
                        evaluateRepositoryEmptyMethod,
                        workspaceInfo,
                        repositorySpec,
                        plasticApi),
                    "The repository changed while the workspace was being created. Initial check-in was stopped.");

                MethodInfo initialCheckinMethod = RequireMethod(
                    apiInterfaceType,
                    "PerformInitialCheckin");
                AwaitTask(
                    initialCheckinMethod.Invoke(api, new[] { workspaceInfo }),
                    "perform initial check-in");
                Require(!IsRepositoryEmpty(
                        evaluateRepositoryEmptyMethod,
                        workspaceInfo,
                        repositorySpec,
                        plasticApi),
                    "Initial check-in returned without creating a remote changeset.");

                string mode = reusedExistingWorkspace ? "reused" : "created";
                Debug.Log(
                    $"STAGE_A_UVCS_WORKSPACE_OK mode={mode} " +
                    $"workspace={actualWorkspaceName} repository={UvcsRepositorySpec}");
            }
            catch (Exception exception)
            {
                Exception rootCause = Unwrap(exception);
                string safeMessage = (rootCause.Message ?? "Unknown error")
                    .Replace('\r', ' ')
                    .Replace('\n', ' ');
                Debug.LogError(
                    $"STAGE_A_UVCS_WORKSPACE_FAILED " +
                    $"type={rootCause.GetType().Name} message={safeMessage}");
                throw new BuildFailedException(
                    $"Unity Version Control workspace/check-in failed: {safeMessage}");
            }
        }

        private static string ProjectRoot => Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException("Unity project root could not be resolved.");

        private static string BuildRoot => Path.GetFullPath(Path.Combine(ProjectRoot, "..", "Builds", "Local"));

        private static Type FindRequiredType(string fullName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, throwOnError: false))
                .FirstOrDefault(candidate => candidate != null);
            Require(type != null, $"Required Unity Version Control type is unavailable: {fullName}");
            return type;
        }

        private static MethodInfo RequireMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(methodName);
            Require(method != null,
                $"Required Unity Version Control method is unavailable: {type.FullName}.{methodName}");
            return method;
        }

        private static MethodInfo RequireMethod(
            Type type,
            string methodName,
            BindingFlags bindingFlags,
            Type[] parameterTypes)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                bindingFlags,
                binder: null,
                types: parameterTypes,
                modifiers: null);
            Require(method != null,
                $"Required Unity Version Control method is unavailable: {type.FullName}.{methodName}");
            return method;
        }

        private static object GetRequiredStaticProperty(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(
                propertyName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Require(property != null,
                $"Required Unity Version Control property is unavailable: {type.FullName}.{propertyName}");

            object value = property.GetValue(null);
            Require(value != null,
                $"Unity Version Control returned no value for: {type.FullName}.{propertyName}");
            return value;
        }

        private static string ReadRequiredStringMember(object instance, string memberName)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property?.GetValue(instance) is string propertyValue &&
                !string.IsNullOrWhiteSpace(propertyValue))
            {
                return propertyValue;
            }

            FieldInfo field = type.GetField(
                memberName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Require(field?.GetValue(instance) is string fieldValue &&
                    !string.IsNullOrWhiteSpace(fieldValue),
                $"Unity Version Control returned no value for: {type.FullName}.{memberName}");
            return (string)field.GetValue(instance);
        }

        private static bool IsRepositoryEmpty(
            MethodInfo evaluateMethod,
            object workspaceInfo,
            object repositorySpec,
            object plasticApi)
        {
            object result = evaluateMethod.Invoke(
                null,
                new[] { workspaceInfo, repositorySpec, plasticApi });
            Require(result is bool,
                "Unity Version Control returned an invalid repository emptiness result.");
            return (bool)result;
        }

        private static void InvokeRequiredStaticMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Require(method != null,
                $"Required Unity Version Control method is unavailable: {type.FullName}.{methodName}");
            method.Invoke(null, null);
        }

        private static object AwaitTaskResult(
            object invocationResult,
            string operation,
            bool allowNullResult = false)
        {
            System.Threading.Tasks.Task task = invocationResult as System.Threading.Tasks.Task;
            Require(task != null, $"Unity Version Control could not {operation}.");
            task.GetAwaiter().GetResult();

            PropertyInfo resultProperty = task.GetType().GetProperty("Result");
            Require(resultProperty != null,
                $"Unity Version Control returned no result while trying to {operation}.");
            object result = resultProperty.GetValue(task);
            Require(allowNullResult || result != null,
                $"Unity Version Control returned an empty result while trying to {operation}.");
            return result;
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                NormalizeDirectoryPath(left),
                NormalizeDirectoryPath(right),
                StringComparison.Ordinal);
        }

        private static string NormalizeDirectoryPath(string path)
        {
            Require(!string.IsNullOrWhiteSpace(path), "A workspace path is empty.");
            string fullPath = Path.GetFullPath(path);
            string rootPath = Path.GetPathRoot(fullPath);
            return string.Equals(fullPath, rootPath, StringComparison.Ordinal)
                ? fullPath
                : fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static void AwaitTask(object invocationResult, string operation)
        {
            System.Threading.Tasks.Task task = invocationResult as System.Threading.Tasks.Task;
            Require(task != null, $"Unity Version Control could not {operation}.");
            task.GetAwaiter().GetResult();
        }

        private static Exception Unwrap(Exception exception)
        {
            Exception current = exception;
            while (true)
            {
                if (current is TargetInvocationException targetInvocationException &&
                    targetInvocationException.InnerException != null)
                {
                    current = targetInvocationException.InnerException;
                    continue;
                }

                if (current is AggregateException aggregateException)
                {
                    AggregateException flattened = aggregateException.Flatten();
                    if (flattened.InnerExceptions.Count == 1)
                    {
                        current = flattened.InnerExceptions[0];
                        continue;
                    }
                }

                return current;
            }
        }

        private static BuildReport Build(
            BuildTarget target,
            string locationPathName,
            bool emitSuccessMarker = true)
        {
            ValidateTechnicalBaseline();
            string outputDirectory = Path.GetDirectoryName(locationPathName);
            Require(!string.IsNullOrWhiteSpace(outputDirectory), "Build output directory is invalid.");
            Directory.CreateDirectory(outputDirectory);

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = locationPathName,
                target = target,
                options = BuildOptions.Development | BuildOptions.StrictMode
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"{target} build failed: {report.summary.result}; errors={report.summary.totalErrors}");
            }

            if (emitSuccessMarker)
            {
                Debug.Log(
                    $"STAGE_A_BUILD_OK target={target} bytes={report.summary.totalSize} " +
                    $"path={locationPathName}");
            }

            return report;
        }

        private static bool GraphicsApisMatchExactly(
            GraphicsDeviceType[] actual,
            params GraphicsDeviceType[] expected)
        {
            return actual != null &&
                   expected != null &&
                   actual.SequenceEqual(expected);
        }

        private static void RestoreProjectSettingsFileExactly(
            string projectSettingsPath,
            byte[] expectedBytes)
        {
            Require(
                !string.IsNullOrWhiteSpace(projectSettingsPath) &&
                expectedBytes != null &&
                expectedBytes.Length > 0,
                "ProjectSettings.asset restore snapshot is invalid.");

            AssetDatabase.SaveAssets();
            byte[] currentBytes = File.ReadAllBytes(projectSettingsPath);
            if (!currentBytes.SequenceEqual(expectedBytes))
            {
                File.WriteAllBytes(projectSettingsPath, expectedBytes);
                AssetDatabase.Refresh(
                    ImportAssetOptions.ForceSynchronousImport |
                    ImportAssetOptions.ForceUpdate);
            }

            Require(
                File.ReadAllBytes(projectSettingsPath).SequenceEqual(expectedBytes),
                "ProjectSettings.asset byte-exact restore readback mismatch.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new BuildFailedException(message);
            }
        }
    }
}

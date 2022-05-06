/* 
*   NatShare
*   Copyright (c) 2021 Yusuf Olokoba.
*/

#if UNITY_EDITOR
namespace NatSuite.Sharing.Editor {

    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEngine;

    static class NatShare {
        
        [InitializeOnLoadMethod]
        static void Import () {
            // Check
            if (typeof(NatShare).Assembly.GetReferencedAssemblies().Any(asm => asm.Name == "NatSuite.Sharing"))
                return;
            // Read manifest
            var manifestPath = Path.Combine(Environment.CurrentDirectory, "Packages", "manifest.json");
            var manifestStr = File.ReadAllText(manifestPath);
            var manifest = JsonUtility.FromJson<Manifest>(manifestStr);
            var registries = manifest.scopedRegistries ?? new ScopedRegistry[0];
            var registry = new ScopedRegistry {
                name = @"NatSuite Framework",
                url = @"https://registry.npmjs.com",
                scopes = new [] { @"api.natsuite" }
            };
            // Check if scope contains NatSuite
            if (registries.All(s => s.name != registry.name)) {
                // Update registries
                registries = registries.Append(registry).ToArray();
                var registriesStr = JsonUtility.ToJson(new Manifest { scopedRegistries = registries }, true);
                registriesStr = registriesStr.Substring(registriesStr.IndexOf('\n') + 1);
                registriesStr = registriesStr.Substring(0, registriesStr.LastIndexOf('}') - 1);
                // Update manifest
                manifestStr = Regex.Replace(
                    manifestStr,
                    @"(?<=})[\n\r]?(?=}$)",
                    $",{Environment.NewLine}{registriesStr}{Environment.NewLine}"
                );
                File.WriteAllText(manifestPath, manifestStr);
                Thread.Sleep(1_000);
            }
            // List
            var listRequest = Client.List(true, false);
            while (!listRequest.IsCompleted)
                Thread.Sleep(100);
            // Check
            if (listRequest.Error != null) {
                Debug.LogError("Failed to import NatShare with error: " + listRequest.Error.message);
                return;
            }
            // Add NatShare dependency
            const string package = @"api.natsuite.natshare";
            if (!listRequest.Result.Any(p => p.name == package))
                Client.Add(package);
        }

        [Serializable] struct Manifest { public ScopedRegistry[] scopedRegistries; }

        [Serializable] struct ScopedRegistry { public string name; public string url; public string[] scopes; }
    }
}
#endif
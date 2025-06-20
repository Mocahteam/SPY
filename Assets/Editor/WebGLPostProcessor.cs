using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class WebGLPostProcessor
{   
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.WebGL)
        {
            Debug.Log($"[WebGLPostProcessor] Démarrage de la copie :");
            
            string originalBuildFolder = Path.Combine(pathToBuiltProject, "Build");
            string newInstallFolder = Path.Combine(pathToBuiltProject, Application.version);
            string newBuildFolder = Path.Combine(newInstallFolder, "Build");
            string originalStreamingAssetsFolder = Path.Combine(pathToBuiltProject, "StreamingAssets");
            string newStreamingAssetsFolder = Path.Combine(newInstallFolder, "StreamingAssets");
            string originalName = Path.GetFileName(pathToBuiltProject);
            string newName = Path.GetFileName(pathToBuiltProject) + Application.version;

            if (!Directory.Exists(originalBuildFolder))
            {
                Debug.LogError($"[WebGLPostProcessor] Dossier Build introuvable : {originalBuildFolder}");
                return;
            }

            // Suppression de l'ancien build s'il existe
            if (Directory.Exists(newInstallFolder))
                Directory.Delete(newInstallFolder, true);
            // Création du dossier destinataire
            Directory.CreateDirectory(newBuildFolder);

            // Copier les fichiers principaux
            CopyIfExists(Path.Combine(originalBuildFolder, $"{originalName}.data.unityweb"), Path.Combine(newBuildFolder, $"{newName}.data.unityweb"));
            CopyIfExists(Path.Combine(originalBuildFolder, $"{originalName}.wasm.unityweb"), Path.Combine(newBuildFolder, $"{newName}.wasm.unityweb"));
            CopyIfExists(Path.Combine(originalBuildFolder, $"{originalName}.loader.js"), Path.Combine(newBuildFolder, $"{newName}.loader.js"));
            CopyIfExists(Path.Combine(originalBuildFolder, $"{originalName}.framework.js.unityweb"), Path.Combine(newBuildFolder, $"{newName}.framework.js.unityweb"));

            // Copie du dossier StreamingAssets
            if (Directory.Exists(originalStreamingAssetsFolder))
            {
                Debug.Log("[WebGLPostProcessor] Copie du dossier StreaminAssets");
                CopyDirectory(originalStreamingAssetsFolder, newStreamingAssetsFolder, true);
            }

            Debug.Log("[WebGLPostProcessor] Copie terminé !");
        }
    }
    
    static void CopyIfExists(string oldPath, string newPath)
    {   
        if (File.Exists(oldPath))
        {
            try
            {
                File.Copy(oldPath, newPath);
                Debug.Log($"[WebGLPostProcessor] ✅ Copié: {Path.GetFileName(oldPath)} -> {Path.GetFileName(newPath)}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[WebGLPostProcessor] ❌ Erreur de copie {Path.GetFileName(oldPath)} -> {Path.GetFileName(newPath)}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[WebGLPostProcessor] ⚠️ Fichier non trouvé: {oldPath}");
        }
    }

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using FYFY;

public class ObstableTransparencySystem : FSystem
{
    [Header("Références")]
    public Camera playerCamera;

    [Header("Paramètres")]
    public LayerMask wallLayerMask = -1; // Layers considérés comme des murs (voir Inspector, default -1 <=> Everything)
    public float alphaRatio = 0.7f; // Niveau d'opacité (1 = invisible, 0 = opaque)
    public float fadeSpeed = 5f; // Vitesse de transition

    private Family f_agents = FamilyManager.getFamily(new AnyOfTags("Drone", "Player"));

    private List<Renderer> currentTransparentWalls = new List<Renderer>();

    public static ObstableTransparencySystem instance;


    public ObstableTransparencySystem()
    {
        instance = this;
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount)
    {
        if (playerCamera == null) return;

        CheckWallObstruction();
    }

    void CheckWallObstruction()
    {
        // Liste des obstacles qui doivent être transparents
        List<Renderer> wallsToMakeTransparent = new List<Renderer>();

        foreach (GameObject agent in f_agents)
        {
            // Direction de la caméra vers la cible
            Vector3 direction = agent.transform.position - playerCamera.transform.position;
            float distance = direction.magnitude;

            // Raycast pour détecter les obstacles
            RaycastHit[] hits = Physics.RaycastAll(playerCamera.transform.position, direction.normalized, distance, wallLayerMask);

            foreach (RaycastHit hit in hits)
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null && !wallsToMakeTransparent.Contains(renderer))
                    wallsToMakeTransparent.Add(renderer);
            }
        }

        // Rendre les murs transparents
        foreach (Renderer wall in wallsToMakeTransparent)
            MakeWallTransparent(wall);

        // Restaurer l'opacité des murs qui ne sont plus obstrués
        List<Renderer> wallsToRestore = new List<Renderer>();
        foreach (Renderer wall in currentTransparentWalls)
            if (!wallsToMakeTransparent.Contains(wall))
                wallsToRestore.Add(wall);

        foreach (Renderer wall in wallsToRestore)
            RestoreWallOpacity(wall);
    }

    void MakeWallTransparent(Renderer wallRenderer)
    {
        if (currentTransparentWalls.Contains(wallRenderer)) return;

        currentTransparentWalls.Add(wallRenderer);

        // Démarrer la coroutine de fondu
        WallMaterials wm = wallRenderer.GetComponent<WallMaterials>();
        wm.StopAllCoroutines();
        wm.StartCoroutine(FadeToTransparent(wallRenderer));
    }

    void RestoreWallOpacity(Renderer wallRenderer)
    {
        if (!currentTransparentWalls.Contains(wallRenderer)) return;

        currentTransparentWalls.Remove(wallRenderer);

        // Démarrer la coroutine de restauration
        WallMaterials wm = wallRenderer.GetComponent<WallMaterials>();
        wm.StopAllCoroutines();
        wm.StartCoroutine(FadeToOpaque(wallRenderer));
    }

    System.Collections.IEnumerator FadeToTransparent(Renderer wallRenderer)
    {
        // Get source materials
        WallMaterials srcMat = wallRenderer.GetComponent<WallMaterials>();
        // Copy Fade materials
        Material topFadeMat = new Material(srcMat.topBottomFade);
        Material sideFadeMat = new Material(srcMat.sideFade);

        // switch on material that support fading
        wallRenderer.materials = new Material[] { sideFadeMat, topFadeMat };

        // set color not transparent
        for (int i = 0; i < wallRenderer.materials.Length; i++)
            wallRenderer.materials[i].color = new Color(1f, 1f, 1f, 1f);

        // fade colors
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeSpeed;
            for (int i = 0; i < wallRenderer.materials.Length; i++)
                wallRenderer.materials[i].color = new Color(1f, 1f, 1f, 1f - alphaRatio * timer);
            yield return null;
        }

        // be sure transparent color is now applied
        for (int i = 0; i < wallRenderer.materials.Length; i++)
            wallRenderer.materials[i].color = new Color(1f, 1f, 1f, 1f - alphaRatio);
    }

    System.Collections.IEnumerator FadeToOpaque(Renderer wallRenderer)
    {
        // Get source materials
        WallMaterials srcMat = wallRenderer.GetComponent<WallMaterials>();
        
        // Copy Fade materials
        Material topFadeMat = new Material(srcMat.topBottomFade);
        Material sideFadeMat = new Material(srcMat.sideFade);

        // switch on material that support fading
        wallRenderer.materials = new Material[] { sideFadeMat, topFadeMat };

        // Be sure to start on transparent color
        for (int i = 0; i < wallRenderer.materials.Length; i++)
            wallRenderer.materials[i].color = new Color(1f, 1f, 1f, 1f - alphaRatio);

        // fade colors
        float timer = 1f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime * fadeSpeed;
            for (int i = 0; i < wallRenderer.materials.Length; i++)
                wallRenderer.materials[i].color = new Color(1f, 1f, 1f, 1f - alphaRatio * timer);
            yield return null;
        }

        // Etre sur que le materiau est opaque en affectant la source opaque
        wallRenderer.materials = new Material[] { srcMat.sideOpaque, srcMat.topBottomOpaque };
    }
}
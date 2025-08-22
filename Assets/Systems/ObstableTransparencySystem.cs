using UnityEngine;
using System.Collections.Generic;
using FYFY;

public class ObstableTransparencySystem : FSystem
{
    [Header("Références")]
    public Camera playerCamera;

    [Header("Paramètres")]
    public LayerMask wallLayerMask = -1; // Layers considérés comme des murs
    public float transparencyAlpha = 0.3f; // Niveau de transparence (0 = invisible, 1 = opaque)
    public float fadeSpeed = 5f; // Vitesse de transition

    private Family f_agents = FamilyManager.getFamily(new AnyOfTags("Drone", "Player"));

    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    private Dictionary<Renderer, Material> transparentMaterials = new Dictionary<Renderer, Material>();
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
                if (renderer != null && renderer.gameObject.tag == "Wall")
                {
                    wallsToMakeTransparent.Add(renderer);
                }
            }
        }

        // Rendre les murs transparents
        foreach (Renderer wall in wallsToMakeTransparent)
        {
            MakeWallTransparent(wall);
        }

        // Restaurer l'opacité des murs qui ne sont plus obstrués
        List<Renderer> wallsToRestore = new List<Renderer>();
        foreach (Renderer wall in currentTransparentWalls)
        {
            if (!wallsToMakeTransparent.Contains(wall))
            {
                wallsToRestore.Add(wall);
            }
        }

        foreach (Renderer wall in wallsToRestore)
        {
            RestoreWallOpacity(wall);
        }
    }

    void MakeWallTransparent(Renderer wallRenderer)
    {
        if (currentTransparentWalls.Contains(wallRenderer)) return;

        currentTransparentWalls.Add(wallRenderer);

        // Sauvegarder le matériau original
        if (!originalMaterials.ContainsKey(wallRenderer))
        {
            originalMaterials[wallRenderer] = wallRenderer.material;
        }

        // Créer ou réutiliser le matériau transparent
        if (!transparentMaterials.ContainsKey(wallRenderer))
        {
            Material transparentMat = new Material(originalMaterials[wallRenderer]);
            SetMaterialTransparent(transparentMat);
            transparentMaterials[wallRenderer] = transparentMat;
        }

        wallRenderer.material = transparentMaterials[wallRenderer];

        // Démarrer la coroutine de fondu
        MainLoop.instance.StartCoroutine(FadeToTransparent(wallRenderer));
    }

    void RestoreWallOpacity(Renderer wallRenderer)
    {
        if (!currentTransparentWalls.Contains(wallRenderer)) return;

        currentTransparentWalls.Remove(wallRenderer);

        // Démarrer la coroutine de restauration
        MainLoop.instance.StartCoroutine(FadeToOpaque(wallRenderer));
    }

    void SetMaterialTransparent(Material material)
    {
        if (material.HasProperty("_Mode"))
        {
            material.SetFloat("_Mode", 2); // Fade mode
        }
        material.SetOverrideTag("RenderType", "Transparent");

        // Changer le mode de rendu pour supporter la transparence
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Activer les keywords si supportés
        try
        {
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        catch (System.Exception)
        {
            // Ignorer les erreurs de keywords en WebGL
            Debug.LogWarning("Keywords not supported in WebGL build");
        }
    }

    System.Collections.IEnumerator FadeToTransparent(Renderer wallRenderer)
    {
        Material mat = wallRenderer.material;
        Color originalColor = mat.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, transparencyAlpha);

        float timer = 0f;
        while (timer < 1f && currentTransparentWalls.Contains(wallRenderer))
        {
            timer += Time.deltaTime * fadeSpeed;
            Color currentColor = Color.Lerp(originalColor, targetColor, timer);
            mat.color = currentColor;
            yield return null;
        }

        if (currentTransparentWalls.Contains(wallRenderer))
        {
            mat.color = targetColor;
        }
    }

    System.Collections.IEnumerator FadeToOpaque(Renderer wallRenderer)
    {
        Material mat = wallRenderer.material;
        Color currentColor = mat.color;
        Color targetColor = originalMaterials[wallRenderer].color;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeSpeed;
            Color lerpedColor = Color.Lerp(currentColor, targetColor, timer);
            mat.color = lerpedColor;
            yield return null;
        }

        // Restaurer le matériau original
        wallRenderer.material = originalMaterials[wallRenderer];
    }
}
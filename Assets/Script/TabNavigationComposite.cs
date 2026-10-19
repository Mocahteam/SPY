using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]      // enregistre le composite au chargement de l'éditeur,
#endif                  // pour qu'il apparaisse dans la liste "Add Binding"
[Preserve]              // survit au code stripping (indispensable en WebGL)
public class TabNavigationComposite : InputBindingComposite<Vector2>
{
    [InputControl(layout = "Button")] public int tab;
    [InputControl(layout = "Button")] public int shift;

    public override Vector2 ReadValue(ref InputBindingCompositeContext context)
    {
        if (!context.ReadValueAsButton(tab))
            return Vector2.zero;

        // Navigate : (0,1) = précédent, (0,-1) = suivant
        return context.ReadValueAsButton(shift) ? Vector2.up : Vector2.down;
    }

    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        => context.ReadValueAsButton(tab) ? 1f : 0f;

    static TabNavigationComposite()
    {
        InputSystem.RegisterBindingComposite<TabNavigationComposite>("TabNavigation");
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init() { }   // force l'exécution du ctor statique en build
}
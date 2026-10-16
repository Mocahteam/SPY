using FYFY;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class AutoClickSystem : FSystem
{
    public CurrentSettingsValues cs;
    public GameObject tempoUI;
    private Image loadingBar;
    private CanvasScaler canvasScaler;
    private AudioSource audio;

    // Déplacement toléré (pixels) avant de remettre le compteur à zéro.
    private float moveTolerance = 5f;
    
    // Temps d'immobilité avant d'afficher la barre (évite le scintillement pendant les mouvements lents).
    private float showDelay = 0.2f;

    // Durée entre l'appui et le relâchement (laisse le temps à l'état visuel 'Pressed').
    private float pressDuration = 0.08f;

    // 0 → 1 pendant l'attente, pratique pour une jauge radiale (Image.fillAmount).
    private float Progress => armed && !clicking ? Mathf.Clamp01(timer / cs.values.currentAutoClickDelay) : 0f;

    private Vector2 lastPos;
    private float timer;
    private bool armed = true;
    private bool clicking;
    private bool barVisible;
    private readonly List<RaycastResult> hits = new List<RaycastResult>();

    public static AutoClickSystem instance;

    public AutoClickSystem()
    {
        instance = this;
    }

    protected override void onStart()
    {
        loadingBar = tempoUI.transform.Find("LoadingBar").GetComponent<Image>();
        canvasScaler = tempoUI.GetComponentInParent<CanvasScaler>(true);
        audio = tempoUI.GetComponent<AudioSource>();
        lastPos = Pointer.current.position.ReadValue();
        timer = 0f;
        armed = true;
        clicking = false;
        barVisible = true;   // force la mise à jour au premier appel
        SetBarVisible(false);
        Pause = true;
    }

    protected override void onPause(int currentFrame)
    {
        SetBarVisible(false);
    }

    protected override void onProcess(int familiesUpdateCount)
    {
        if (clicking || EventSystem.current == null)
            return;

        Vector2 pos = Pointer.current.position.ReadValue();

        // Le pointeur a bougé : on réarme et on recommence à compter
        if ((pos - lastPos).sqrMagnitude > moveTolerance * moveTolerance)
        {
            lastPos = pos;
            timer = 0f;
            armed = true;
            SetBarVisible(false);
            return;
        }

        // Déjà cliqué à cet endroit
        if (!armed)
            return;
        
        float delay = cs.values.currentAutoClickDelay;
        timer += Time.unscaledDeltaTime;

        // Phase d'attente silencieuse : on ne montre rien tant que l'immobilité n'est pas confirmée
        float visibleStart = Mathf.Min(showDelay, delay * 0.5f);
        if (timer < visibleStart)
            return;

        if (!barVisible)
        {
            // Positionnée une seule fois, au moment où elle apparaît
            (tempoUI.transform as RectTransform).anchoredPosition = pos / canvasScaler.scaleFactor;
            SetBarVisible(true);
        }

        // Jauge normalisée sur la partie visible : elle part de 0, le délai total reste inchangé
        loadingBar.fillAmount = Mathf.Clamp01((timer - visibleStart) / (delay - visibleStart));

        if (timer < delay)
            return;

        armed = false;
        timer = 0f;
        MainLoop.instance.StartCoroutine(DeviceClick());
        MainLoop.instance.StartCoroutine(WaitSoundEnd());
    }

    private void SetBarVisible(bool visible)
    {
        if (barVisible == visible)
            return;
        barVisible = visible;
        if (!visible)
            loadingBar.fillAmount = 0f;   // pas d'ancienne valeur affichée à la réapparition
        tempoUI.SetActive(visible);
    }

    private IEnumerator WaitSoundEnd()
    {
        audio.Play();
        yield return new WaitWhile(() => audio.isPlaying);
        // Ne cacher que si aucune nouvelle attente n'a commencé entre-temps
        if (!armed)
            SetBarVisible(false);
    }

    // Injecte un vrai appui/relâchement sur le bouton gauche de la souris.
    private IEnumerator DeviceClick()
    {
        clicking = true;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            QueueLeftButton(mouse, true);
            // Doit durer au moins une frame de plus que la durée du Hold,
            // sinon le relâchement annule l'interaction avant qu'elle soit "performed".
            yield return new WaitForSecondsRealtime(pressDuration);
            QueueLeftButton(mouse, false);
        }
        clicking = false;
    }

    private static void QueueLeftButton(Mouse mouse, bool pressed)
    {
        // Copie l'état courant de la souris (position comprise) et ne modifie que le bouton gauche
        using (StateEvent.From(mouse, out InputEventPtr eventPtr))
        {
            mouse.leftButton.WriteValueIntoEvent(pressed ? 1f : 0f, eventPtr);
            mouse.delta.WriteValueIntoEvent(Vector2.zero, eventPtr);   // pas de mouvement parasite
            mouse.scroll.WriteValueIntoEvent(Vector2.zero, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }
}
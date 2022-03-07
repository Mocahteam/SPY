using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FYFY_plugins.PointerManager;
using FYFY;

public class ContainerHeader : MonoBehaviour
{
    // L'agent auquel le contenaire est rataché
    public GameObject agent;
    // La camera de la scéne
    public GameObject camera;

    // nom de l'agent
    private TMP_InputField agentNameEdit;

    // Start is called before the first frame update
    void Start()
    {
        agentNameEdit = transform.Find("agentName").GetComponent<TMP_InputField>();

        // On va charger l'image et le nom de l'agent selon l'agent (robot, enemie etc...)
        if (agent.tag == "Player")
        {
            // Chargement de l'icône de l'agent
            transform.Find("agent").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/robotIcon", typeof(Sprite)) as Sprite;
            // Affichage du nom de l'agent
            agentNameEdit.text = agent.GetComponent<AgentEdit>().agentName;
            // Si on autorise le changement de nom on dévérouille la possibilité d'écrire dans la zone de nom du robot
            if (agent.GetComponent<AgentEdit>().editName)
            {
                transform.Find("agentName").GetComponent<TMP_InputField>().interactable = true;
            }
        }
        else if (agent.tag == "Drone")
        {
            // Chargement de l'icône de l'agent
            transform.Find("agent").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/droneIcon", typeof(Sprite)) as Sprite;
            // Affichage du nom de l'agent
            agentNameEdit.text = "Drone";
        }

        // Associer la fonction changeName à OnValueChanged
        agentNameEdit.onValueChanged.AddListener(delegate{ changeName(agentNameEdit.text); });

        // Active ou desactive les mouvements de la caméra si on est en train décrire ou non dans la partie du nom
        //agentNameEdit.onSelect.AddListener(delegate { MainLoop.instance.GetComponent<CameraSystem>().ActivatedCameraControl(false); });
        //agentNameEdit.onDeselect.AddListener(delegate { MainLoop.instance.GetComponent<CameraSystem>().ActivatedCameraControl(true); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Permet de modifier le nom de l'agent
    private void changeName(string newName)
    {
        agent.GetComponent<AgentEdit>().agentName = newName;
    }
}

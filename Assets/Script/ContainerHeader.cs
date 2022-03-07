using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerHeader : MonoBehaviour
{
    // L'agent auquel le contenaire est rataché
    public GameObject agent;

    // Start is called before the first frame update
    void Start()
    {
        // On va charger l'image et le nom de l'agent selon l'agent (robot, enemie etc...)
        if ()
        {
            transform.Find("agent").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/robotIcon", typeof(Sprite)) as Sprite;
        }
        else if ()
        {
            transform.Find("agent").GetComponentInChildren<Image>().sprite = Resources.Load("UI Images/droneIcon", typeof(Sprite)) as Sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Permet de modifier le nom de l'agent
    public void changeName(string newName)
    {
        agent.GetComponent<AgentEdit>().name = newName;
    }
}

using FYFY;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using static SaveContent;

public class CompetenciesChecker : FSystem
{
    private Family f_competencies = FamilyManager.getFamily(new AllOfComponents(typeof(Competency)));

    public Transform skillsInvolvedContent;
    public Transform skillsIgnoredContent;
    public GameObject SkillPrefab;

    public CurrentSettingsValues cs;

    // L'instance
    public static CompetenciesChecker instance;


    public CompetenciesChecker()
    {
        instance = this;
    }

    protected override void onStart()
    {
        Pause = true;
    }

    // see Skills tab button in MissionEditor
    public void CheckSkills()
    {
        // remove all old competencies
        for (int i = skillsInvolvedContent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = skillsInvolvedContent.transform.GetChild(i);
            GameObjectManager.unbind(child.gameObject);
            child.SetParent(null);
            GameObject.Destroy(child.gameObject);
        }
        for (int i = skillsIgnoredContent.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = skillsIgnoredContent.transform.GetChild(i);
            GameObjectManager.unbind(child.gameObject);
            child.SetParent(null);
            GameObject.Destroy(child.gameObject);
        }

        foreach (GameObject comp in f_competencies)
        {
            Competency competency = comp.GetComponent<Competency>();
            if (competency.referentialId == cs.values.currentSkillsRepository && competency.rule != "")
            {
                // build mission xml for analysis
                string exportXML = SaveFileSystem.instance.buildLevelContent();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(exportXML);
                Utility.removeComments(doc);

                // On instancie la compétence
                GameObject skill = UnityEngine.Object.Instantiate(SkillPrefab);
                skill.name = competency.name;
                skill.GetComponent<TMP_Text>().text = Utility.extractLocale(competency.id);
                skill.GetComponent<TooltipContent>().text = Utility.extractLocale(competency.description);
                // Ajout de la compétence à la bonne liste
                if (UtilityLobby.isCompetencyMatchWithLevel(competency, doc))
                    skill.transform.SetParent(skillsInvolvedContent);
                else
                    skill.transform.SetParent(skillsIgnoredContent);

                GameObjectManager.bind(skill);
            }
        }

        if (skillsInvolvedContent.childCount == 0)
        {
            // On instancie une competence fake
            GameObject skill = UnityEngine.Object.Instantiate(SkillPrefab, skillsInvolvedContent);
            skill.name = "NoSkills";
            Object.Destroy(skill.GetComponent<TooltipContent>());
            skill.GetComponent<TMP_Text>().text = LocalizationSettings.StringDatabase.GetLocalizedString("StringLocalization", "NoSkillsIdentified");
            GameObjectManager.bind(skill);
        }
    }
}

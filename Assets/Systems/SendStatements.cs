using UnityEngine;
using FYFY;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.Networking;
using DIG.GBLXAPI.Internal;

public class SendStatements : FSystem {

    private Family f_actionForLRS = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformedForLRS)));
    private Family f_saveProgression = FamilyManager.getFamily(new AllOfComponents(typeof(SendUserData)));

    public static SendStatements instance;

    private LrsRemoteQueue statementQueue;
    private GameData gameData;
    private UserData userData;

    public SendStatements()
    {
        instance = this;
    }
	
	protected override void onStart()
    {
        GameObject gd = GameObject.Find("GameData");
        if (gd != null)
        {
            gameData = gd.GetComponent<GameData>();
            userData = gd.GetComponent<UserData>();
        }

        GameObject GBLXAPI = GameObject.Find("GBLXAPI");
        if (GBLXAPI != null)
            statementQueue = GBLXAPI.GetComponent<LrsRemoteQueue>();
        else
            statementQueue = null;

        f_saveProgression.addEntryCallback(saveUserData);
    }

    // Use to process your families.
    protected override void onProcess(int familiesUpdateCount) {
        if (gameData.sendStatementEnabled)
        {
            // Do not use callbacks because in case in the same frame actions are removed on a GO and another component is added in another system, family will not trigger again callback because component will not be processed
            foreach (GameObject go in f_actionForLRS)
            {
                ActionPerformedForLRS[] listAP = go.GetComponents<ActionPerformedForLRS>();
                int nb = listAP.Length;
                ActionPerformedForLRS ap;
                if (!this.Pause)
                {
                    for (int i = 0; i < nb; i++)
                    {
                        ap = listAP[i];
                        //If no result info filled
                        if (!ap.result)
                        {
                            GBL_Interface.SendStatement(ap.verb, ap.objectType, ap.activityExtensions);
                        }
                        else
                        {
                            bool? completed = null, success = null;

                            if (ap.completed > 0)
                                completed = true;
                            else if (ap.completed < 0)
                                completed = false;

                            if (ap.success > 0)
                                success = true;
                            else if (ap.success < 0)
                                success = false;

                            GBL_Interface.SendStatementWithResult(ap.verb, ap.objectType, ap.activityExtensions, ap.resultExtensions, completed, success, ap.response, ap.score, ap.duration);
                        }
                    }
                }
                for (int i = nb - 1; i > -1; i--)
                {
                    GameObjectManager.removeComponent(listAP[i]);
                }
            }
        }
    }

    private void saveUserData (GameObject go)
    {
        if (gameData.sendStatementEnabled)
        {
            MainLoop.instance.StartCoroutine(PostUserData(GBL_Interface.playerName, userData.schoolClass, userData.isTeacher, JsonConvert.SerializeObject(userData.progression), JsonConvert.SerializeObject(userData.highScore)));
            foreach (SendUserData sp in go.GetComponents<SendUserData>())
                GameObjectManager.removeComponent(sp);
            if (statementQueue != null)
                statementQueue.flushQueuedStatements(true);
        }
    }

    private IEnumerator PostUserData(string idSession, string schoolClass, bool isTeacher, string progression, string highScore)
    {
        progression = progression == "null" ? "{}" : progression;
        highScore = highScore == "null" ? "{}" : highScore;
        Debug.Log(idSession + "_"+ schoolClass + "_"+progression + "_" + highScore);
        UnityWebRequest www = UnityWebRequest.PostWwwForm("https://spy.lip6.fr/ServerREST_LIP6/", "{\"idSession\":\"" + idSession + "\",\"class\":\"" + schoolClass + "\",\"isTeacher\":\""+(isTeacher ? 1 : 0)+"\",\"progression\":" + progression + ",\"highScore\":" + highScore + "}");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning(www.error);
            yield return new WaitForSeconds(0.5f);
            // try again
            MainLoop.instance.StartCoroutine(PostUserData(idSession, schoolClass, isTeacher, progression, highScore));
        }
    }
}
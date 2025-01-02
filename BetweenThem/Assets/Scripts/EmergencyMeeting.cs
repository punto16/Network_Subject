using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmergencyMeeting : MonoBehaviour
{
    public TMP_Text discussionText;
    public TMP_Text titleText;
    public TMP_Text postVotingText;
    public TMP_InputField inputField;
    public TMP_Dropdown dropdown;

    public GameObject buttonVote;

    public PlayerScript pScript;
    public ClientManagerUDP clientManager;
    public GameManager gm;

    private float discussionTimer = 0.0f;
    private float votingTimer = 0.0f;

    public float discussionTime = 70.0f;
    public float votingTime = 40.0f;

    // Start is called before the first frame update
    void Start()
    {
        PopulateDropdownOptions();
        buttonVote.SetActive(pScript.alive);
        discussionText.SetText("Discussion time...");
        titleText.SetText($"Discussion Time: {(int)(discussionTime - discussionTimer)}s");
        inputField.characterLimit = 64;
        discussionTimer = 0.0f;
        votingTimer = 0.0f;
    }

    private void OnEnable()
    {
        discussionTimer = 0.0f;
        votingTimer = 0.0f;
        PopulateDropdownOptions();
        buttonVote.SetActive(pScript.alive);
        discussionText.SetText("Discussion time...");
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.gameState == GameManager.GameState.DISCUSSION)
        {
            discussionTimer += Time.deltaTime;

            titleText.SetText($"Discussion Time: {(int)(discussionTime - discussionTimer)}s");

            if (discussionTimer >= discussionTime)
            {
                gm.ChangeGameState(GameManager.GameState.VOTE);
                discussionTimer = 0.0f;
            }
        }
        else if (gm.gameState == GameManager.GameState.VOTE)
        {
            votingTimer += Time.deltaTime;

            titleText.SetText($"Voting Time: {(int)(votingTime - votingTimer)}s");

            if (votingTimer >= votingTime)
            {
                KickVoted();
            }
        }
    }

    public void KickVoted()
    {
        titleText.SetText("Voting Time Finished");
        gm.ChangeGameState(GameManager.GameState.POSTVOTE);
        votingTimer = 0.0f;
        int resolvedVoteId = DetermineVoteResult(clientManager.votations);
        GameObject matchingObject = clientManager.entitiesGO.FirstOrDefault(entry => entry.Value == resolvedVoteId).Key;
        if (matchingObject != null)
        {
            if (resolvedVoteId == 0)
            {
                postVotingText.SetText("NO ONE has been executed");
            }
            else
            {
                PlayerScript ps = matchingObject.GetComponent<PlayerScript>();
                postVotingText.SetText($"{ps.userName} has been executed");
                matchingObject.transform.position = new Vector3(0, 0, matchingObject.transform.position.z);
                ps.GetKilled();

                if (ps.impostor) //if we kicked in votes the impostor, crewmates win
                {
                    gm.gameObject.GetComponent<SceneManag>().ChangeScene("CrewmateWin");
                }
                int aliveCrewmates = 0;
                foreach (KeyValuePair<GameObject, int> entry in clientManager.entitiesGO)
                {
                    PlayerScript pScript = entry.Key.GetComponent<PlayerScript>();
                    if (pScript.alive && !pScript.impostor) aliveCrewmates++;
                }
                if (aliveCrewmates <= 1) gm.gameObject.GetComponent<SceneManag>().ChangeScene("ImpostorWin");
            }
        }
        clientManager.votations.Clear();
    }

    public int DetermineVoteResult(List<Packet.VoteActionDataPacket> votationsDs)
    {
        List<int> votations = new List<int>();
        foreach (var i in votationsDs)
        {
            votations.Add(i.idVoted);
        }
        var voteCounts = new Dictionary<int, int>();
        foreach (int vote in votations)
        {
            voteCounts[vote] = voteCounts.ContainsKey(vote) ? voteCounts[vote] + 1 : 1;
        }
        int maxCount = voteCounts.Values.Max();
        var mostVotedIds = voteCounts.Where(x => x.Value == maxCount).Select(x => x.Key).ToList();
        return mostVotedIds.Count > 1 ? 0 : mostVotedIds[0];
    }

    public void DiscussionInput(string text)
    {
        if (!pScript.alive)
        {
            inputField.text = "";
            return;
        }
        discussionText.text += "\n\n" + pScript.userName + " (YOU): " + text;
        inputField.text = "";
        clientManager.SendText(text);
    }

    void PopulateDropdownOptions()
    {
        List<string> names = new List<string>();
        bool isFirst = true;

        foreach (var entry in clientManager.entitiesGO)
        {
            if (isFirst)
            {
                isFirst = false;    //1st is player, we cant vote ourselves, but we can vote noone
                names.Add("NO VOTE");
                continue;
            }

            PlayerScript playerScript = entry.Key.GetComponent<PlayerScript>();
            if (playerScript != null)
            {
                if (playerScript.alive)
                names.Add(playerScript.userName);
            }
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(names);
    }

    public void SendVote()
    {
        if (!pScript.alive) return;
        clientManager.ActionVote(dropdown.value);
    }
}

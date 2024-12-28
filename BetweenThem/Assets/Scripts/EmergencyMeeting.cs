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
        discussionText.SetText("Discussion time...");
        titleText.SetText($"Discussion Time: {(int)(discussionTime - discussionTimer)}s");
        inputField.characterLimit = 32;
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
                titleText.SetText("Voting Time Finished");
                gm.ChangeGameState(GameManager.GameState.POSTVOTE);
                KickVoted();
                votingTimer = 0.0f;
            }
        }
    }

    public void KickVoted()
    {
        var voteCounts = new Dictionary<int, int>();
        foreach (int vote in clientManager.votations) voteCounts[vote] = voteCounts.ContainsKey(vote) ? voteCounts[vote] + 1 : 1;
        int maxCount = voteCounts.Values.Max();
        int mostVotedId = voteCounts.Where(x => x.Value == maxCount).Select(x => x.Key).Count() > 1 ? 0 : voteCounts.First(x => x.Value == maxCount).Key;

        GameObject matchingObject = clientManager.entitiesGO.FirstOrDefault(entry => entry.Value == mostVotedId).Key;
        if (matchingObject != null)
        {
            if (mostVotedId == 0)
            {
                postVotingText.SetText("NO ONE has been executed");
            }
            else
            {
                PlayerScript ps = matchingObject.GetComponent<PlayerScript>();
                postVotingText.SetText($"{ps.userName} has been executed");
                matchingObject.transform.position = new Vector2(0, 0);
                ps.GetKilled();
            }
            gm.ChangeGameState(GameManager.GameState.POSTVOTE);
        }
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

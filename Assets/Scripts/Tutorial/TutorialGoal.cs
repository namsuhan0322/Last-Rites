using UnityEngine;

public class TutorialGoal : MonoBehaviour
{
    public TutorialSystem tutorialSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialSystem.ReachGoal();
        }
    }
}

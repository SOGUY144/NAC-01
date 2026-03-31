using UnityEngine;
using UnityEngine.Playables;

public class StartTimeline : MonoBehaviour
{
    public PlayableDirector director;

    public void PlayTimeline()
    {
        director.Play();
    }
}
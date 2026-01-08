using UnityEngine;

[CreateAssetMenu(menuName = "Dialog/Speaker", fileName = "NewSpeaker")]
public class Speaker : ScriptableObject
{
    [Header("Speaker Info")]
    public string SpeakerName;
    public Sprite Portrait;
}
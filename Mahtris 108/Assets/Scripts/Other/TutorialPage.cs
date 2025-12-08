using UnityEngine;

[System.Serializable]
public class TutorialPage
{
    public Sprite image;

    [TextArea(3, 10)]
    public string description;
}

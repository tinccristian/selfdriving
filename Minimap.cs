using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private LineRenderer lineRendre;
    private GameObject TrackPath;

    public GameObject LocalPlayer;
    public GameObject Player;

    public int offset;

    void Start()
    {
        lineRendre = GetComponent<LineRenderer>();
        TrackPath = this.gameObject;

        int nr = TrackPath.transform.childCount;
        lineRendre.positionCount = nr + 1;

        for (int x=0; x < nr; x++)
        {
            lineRendre.SetPosition(x, new Vector3(TrackPath.transform.GetChild(x).transform.position.x,
                4,
                TrackPath.transform.GetChild(x).transform.position.z));
        }
        lineRendre.SetPosition(nr, lineRendre.GetPosition(0));
        lineRendre.startWidth = 7f;
        lineRendre.endWidth = 7f;

    }

    // Update is called once per frame
    void Update()
    {
        Player.transform.position = (new Vector3(LocalPlayer.transform.position.x,offset,LocalPlayer.transform.position.z));
    }
}

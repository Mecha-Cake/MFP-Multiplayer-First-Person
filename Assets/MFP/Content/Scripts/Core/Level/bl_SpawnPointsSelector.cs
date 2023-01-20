using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_SpawnPointsSelector : MonoBehaviour
{
    [SerializeField] private Texture2D SpawnPointIcon = null;
    public Texture2D carIcon, planeIcon;
    public float TransitionDuration = 1.5f;
    public AnimationCurve TransitionCurve;
    public float IconSize = 10;
    public Color IconColor;
    public Color IconSelectColor;

    private Camera m_Camera;
    private bl_GameController Controller;
    private bool DrawSpawns = true;
    private List<SpawnPointData> spawnPoints;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        Controller = bl_GameController.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        FetchPoints();
    }

    void FetchPoints()
    {
        spawnPoints = new List<SpawnPointData>();

        for (int i = 0; i < Controller.SpawnPoint.Count; i++)
        {
            if (Controller.SpawnPoint[i] == null) continue;
            spawnPoints.Add(new SpawnPointData()
            {
                spawnType = SpawnType.SpawnPoint,
                Point = Controller.SpawnPoint[i].transform,
                Icon = SpawnPointIcon
            });
        }
        bl_VehicleManager[] vehicles = FindObjectsOfType<bl_VehicleManager>();
        for (int i = 0; i < vehicles.Length; i++)
        {
            spawnPoints.Add(new SpawnPointData()
            {
                spawnType = vehicles[i].m_VehicleType == VehicleType.Car ? SpawnType.Car : SpawnType.Plane,
                Point = vehicles[i].transform,
                Icon = vehicles[i].m_VehicleType == VehicleType.Car ? carIcon : planeIcon,
                vehicleManager = vehicles[i]
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        if (!DrawSpawns) return;

        float w = Screen.width;
        float h = Screen.height;
        Event curEvent = Event.current;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if(spawnPoints[i].spawnType == SpawnType.Car || spawnPoints[i].spawnType == SpawnType.Plane)
            {
                if (spawnPoints[i].vehicleManager == null || spawnPoints[i].vehicleManager.isUsed) continue;
                DrawSpawn(w, h, curEvent, i);
            }
            else
            {
                DrawSpawn(w, h, curEvent, i);
            }      
        }
    }

    void DrawSpawn(float w, float h, Event curEvent, int i)
    {
        Vector3 position = spawnPoints[i].Point.position;

        //Calculate the 2D position of the position where the icon should be drawn
        Vector3 viewportPoint = m_Camera.WorldToViewportPoint(position);
        //The viewportPoint coordinates are between 0 and 1, so we have to convert them into screen space here
        Vector2 drawPosition = new Vector2(viewportPoint.x * w, h * (1 - viewportPoint.y));
        Rect r = new Rect(drawPosition.x - IconSize * 0.5f, drawPosition.y - IconSize, IconSize, IconSize);
        bool isOver = r.Contains(curEvent.mousePosition);
        Color c = IconColor;
        if (isOver)
        {
            float s = r.size.x * 1.25f;
            r = new Rect(drawPosition.x - s * 0.5f, drawPosition.y - s, s, s);
            c = IconSelectColor;
        }
        if (Input.GetMouseButtonDown(0) && isOver)
        {
            SelectSpawn(i);
        }
        GUI.color = c;
        GUI.DrawTexture(r, spawnPoints[i].Icon);
    }

    void SelectSpawn(int id)
    {
        DrawSpawns = false;
        StartCoroutine(GoTo(spawnPoints[id]));
        bl_UIManager.Instance.m_ListManager.SpawnEvent(true);

    }

    IEnumerator GoTo(SpawnPointData spawnpoint)
    {
        float d = 0;
        Vector3 po = transform.position;
        Quaternion ro = transform.rotation;
        Vector3 nextPosition = spawnpoint.Point.position;
        Quaternion nextRotation = spawnpoint.Point.rotation;
        if ((spawnpoint.spawnType & SpawnType.Vehicle) != 0 && spawnpoint.vehicleManager != null)
        {
            nextPosition = spawnpoint.vehicleManager.ExitPoint.position;
        }
        nextPosition.y += 2.22f;

        while (d < 1)
        {
            d += Time.deltaTime / TransitionDuration;
            float t = TransitionCurve.Evaluate(d);
            transform.position = Vector3.Lerp(po, nextPosition, t);
            transform.rotation = Quaternion.Slerp(ro, nextRotation, t);
            yield return null;
        }
        transform.position = nextPosition;
        transform.rotation = nextRotation;
        Controller.SpawnPlayer(nextPosition, nextRotation);
        gameObject.SetActive(false);
        if ((spawnpoint.spawnType & SpawnType.Vehicle) != 0 && spawnpoint.vehicleManager != null)
        {
            Controller.m_Player.GetComponent<bl_PlayerSync>().SyncLocalPlayerModel();
            spawnpoint.vehicleManager.OnEnter();
        }
    }

    public class SpawnPointData
    {
        public SpawnType spawnType = SpawnType.SpawnPoint;
        public Transform Point;
        public Texture2D Icon;
        public bl_VehicleManager vehicleManager;
    }

    [System.Flags]
    public enum SpawnType
    {
        SpawnPoint = 1,
        Car = 2,
        Plane = 3,
        Vehicle = Car | Plane,
    }
}
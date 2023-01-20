using UnityEngine;
using UnityEditor;
using Rotorz.ReorderableList.Internal;
using Photon.Pun;

[CustomEditor(typeof(bl_PlayerSync))]
public class bl_PlayerSyncEditor : Editor {

    bl_PlayerSync m_Target;


    private bool m_InterpolateHelpOpen;
    private bool m_ExtrapolateHelpOpen;
    private bool m_InterpolateRotationHelpOpen;
    private bool m_InterpolateScaleHelpOpen;

    private const int EDITOR_LINE_HEIGHT = 20;

    private const string INTERPOLATE_TOOLTIP =
        "Choose between synchronizing the value directly (by disabling interpolation) or smoothly move it towards the newest update.";

    private const string INTERPOLATE_HELP =
        "You can use interpolation to smoothly move your GameObject towards a new position that is received via the network. "
        + "This helps to reduce the stuttering movement that results because the network updates only arrive 10 times per second.\n"
        + "As a side effect, the GameObject is always lagging behind the actual position a little bit. This can be addressed with extrapolation.";

    private const string EXTRAPOLATE_TOOLTIP = "Extrapolation is used to predict where the GameObject actually is";

    private const string EXTRAPOLATE_HELP =
        "Whenever you deal with network values, all values you receive will be a little bit out of date since that data needs "
        + "to reach you first. You can use extrapolation to try to predict where the player actually is, based on the movement data you have received.\n"
        +
        "This has to be tweaked carefully for each specific game in order to insure the optimal prediction. Sometimes it is very easy to extrapolate states, because "
        +
        "the GameObject behaves very predictable (for example for vehicles). Other times it can be very hard because the user input is translated directly to the game "
        + "and you cannot really predict what the user is going to do (for example in fighting games)";

    private const string INTERPOLATE_HELP_URL = "http://doc.exitgames.com/en/pun/current/tutorials/rpg-movement";
    private const string EXTRAPOLATE_HELP_URL = "http://doc.exitgames.com/en/pun/current/tutorials/rpg-movement";

    public void OnEnable()
    {
     
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_Target = (bl_PlayerSync)this.target;

        EditorGUILayout.BeginVertical("box");
        m_Target.HeatTarget = EditorGUILayout.ObjectField("Heat Target", m_Target.HeatTarget, typeof(Transform),true) as Transform;
        EditorGUILayout.EndVertical();
        DrawIsPlayingWarning();
        GUI.enabled = !Application.isPlaying;

        serializedObject.ApplyModifiedProperties();

        GUI.enabled = true;
        DrawNetworkGunsList();
        GUILayout.Space(5);
        m_Target.m_PlayerAnimation = EditorGUILayout.ObjectField("Player Animation", m_Target.m_PlayerAnimation, typeof(bl_PlayerAnimator), true) as bl_PlayerAnimator;
    }

    void DrawNetworkGunsList()
    {
        GUILayout.Space(5);
        SerializedProperty listProperty = serializedObject.FindProperty("NetworkGuns");

        if (listProperty == null)
        {
            return;
        }
        if (PhotonGUI.AddButton())
        {
            listProperty.InsertArrayElementAtIndex(Mathf.Max(0, listProperty.arraySize - 1));
        }

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawIsPlayingWarning()
    {
        if (Application.isPlaying == false)
        {
            return;
        }

        GUILayout.BeginVertical(GUI.skin.box);
        {
            GUILayout.Label("Editing is disabled in play mode so the two objects don't go out of sync");
        }
        GUILayout.EndVertical();
    }


    private void DrawSplitter(ref Rect propertyRect)
    {
        Rect splitterRect = new Rect(propertyRect.xMin - 3, propertyRect.yMin, propertyRect.width + 6, 1);
        PhotonGUI.DrawSplitter(splitterRect);

        propertyRect.y += 5;
    }

    
    private void DrawHelpBox(ref Rect propertyRect, bool isOpen, float height, string helpText, string url)
    {
        if (isOpen == true)
        {
            Rect helpRect = new Rect(propertyRect.xMin, propertyRect.yMin, propertyRect.width, height - 5);
            GUI.BeginGroup(helpRect, GUI.skin.box);
            GUI.Label(new Rect(5, 5, propertyRect.width - 10, height - 30), helpText, PhotonGUI.RichLabel);
            if (GUI.Button(new Rect(5, height - 30, propertyRect.width - 10, 20), "Read more in our documentation"))
            {
                Application.OpenURL(url);
            }
            GUI.EndGroup();

            propertyRect.y += height;
        }
    }

    private void DrawPropertyWithHelpIcon(ref Rect propertyRect, ref bool isHelpOpen, SerializedProperty property, string tooltip)
    {
        Rect propertyFieldRect = new Rect(propertyRect.xMin, propertyRect.yMin, propertyRect.width - 20, propertyRect.height);
        string propertyName = ObjectNames.NicifyVariableName(property.name);
        EditorGUI.PropertyField(propertyFieldRect, property, new GUIContent(propertyName, tooltip));

        Rect helpIconRect = new Rect(propertyFieldRect.xMax + 5, propertyFieldRect.yMin, 20, propertyFieldRect.height);
        isHelpOpen = GUI.Toggle(helpIconRect, isHelpOpen, PhotonGUI.HelpIcon, GUIStyle.none);

        propertyRect.y += EDITOR_LINE_HEIGHT;
    }

 

    private void DrawTeleport(ref Rect propertyRect)
    {
        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.TeleportEnabled"),
            new GUIContent("Enable teleport for great distances"));
        propertyRect.y += EDITOR_LINE_HEIGHT;

        EditorGUI.PropertyField(propertyRect, serializedObject.FindProperty("m_PositionModel.TeleportIfDistanceGreaterThan"),
            new GUIContent("Teleport if distance greater than"));
        propertyRect.y += EDITOR_LINE_HEIGHT;
    }

  
    private void DrawHeader(string label, SerializedProperty property)
    {
        if (property == null)
        {
            return;
        }

        bool newValue = PhotonGUI.ContainerHeaderToggle(label, property.boolValue);

        if (newValue != property.boolValue)
        {
            Undo.RecordObject(this.m_Target, "Change " + label);
            property.boolValue = newValue;
            EditorUtility.SetDirty(this.m_Target);
        }
    }
}

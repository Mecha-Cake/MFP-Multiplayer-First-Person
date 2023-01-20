using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tutorial.Wizard.MFP;
using UnityEditor;

public class MFPDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "mfp/editor/general/";
    private NetworkImages[] ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "https://img.youtube.com/vi/mkqhdST5q_w/0.jpg", Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "gif-1.gif" },
        new GifData{ Path = "addnewwindowfield.gif" },
    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Resume", StepsLenght = 0, DrawFunctionName = nameof(Resume) },
      new Steps { Name = "Add New Map", StepsLenght = 0, DrawFunctionName = nameof(AddNewMapDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(ServerImages, AllSteps, ImagesFolder, AnimatedImages);
    }
    //required//////////////////////////////////////////////////////

    void Resume()
    {
        DrawTitleText("GET STARTED TUTORIAL");
        DrawYoutubeCover("MFP Get Started Tutorial", GetServerImage(0), "https://www.youtube.com/watch?v=mkqhdST5q_w");
        DrawText("<b>Require:</b>\n\n• MFPS 2.1++\n• Unity 2018.4++");
        DrawHyperlinkText("<b>-Import Unity Photon Networking (PUN)</b>\nOpen Asset Store Window.\nGo to <link=https://assetstore.unity.com/packages/tools/network/pun-2-free-119922>https://assetstore.unity.com/packages/tools/network/pun-2-free-119922</link> Import the plugin from Asset Store in a new project or the project that you want.\n\n<b>- Get Your AppID</b>\nRegister a Photon Cloud Account: <link=https://cloud.exitgames.com/Account/SignUp>https://cloud.exitgames.com/Account/SignUp</link>\nChose your plan or use the free plan.\nGet your AppID from the Dashboard\n\n<b>- Paste Your AppID</b>\nIn unity editor, Open Window -> Photon Unity Networkin -> PUN Wizard\nSelect 'Setup Project' option\nPaste your appID that you get from your Photon Cloud Account in Dashboard.\nPush \"Setup Project\" button.\n- Import MFP Package.\n\n<b>Import Multiplayer First Person package in the project.</b>\nOpen the Lobby Scene (MFP -> Content -> Scenes -> Lobby)\nSelect the Lobby GameObject in bl_Lobby\nSelect the \"Server Host\" that you want as default(EU/Japan/US/AU /etc..)\nReady Enjoy!\n");
    }

    void AddNewMapDoc()
    {
        DrawText("Add a new map is simple, you don't need to touch any code, all what you need is you map design,\nonce you have you map/level designed, follow this:\n\n- First duplicate the <b>ExampleLevel</b> scene that comes with MFP, located at: <i>Assets/MFP/Content/Scenes/<b>ExampleLevel</b></i>, you can duplicated it by selecting it in the Project Window -> <b>Ctrl + D</b> on windows or <b>Command + D</b> on Mac, once the scene is duplicated -> rename and open it on the editor.");
        DrawAnimatedImage(0);
    }

    //[MenuItem("Window/MFP/Documentation", false, 111)]
    private static void ShowWindowMFP()
    {
        EditorWindow.GetWindow(typeof(MFPDocumentation));
    }
}
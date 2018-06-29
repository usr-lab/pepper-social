using UnityEditor;
using UnityEngine;
using System.Collections;

public class BuildScript: MonoBehaviour
{
      static string name = "PepperRemote";

      static void BuildAll()
     {
         Build();
     }

     static void Build()
     {
       string[] scenes = {
         "Assets/Scenarios/PepperSocial/PepperSocial.unity",
       };
       string target = "envs/" + name + ".x86_64";
       BuildPipeline.BuildPlayer(scenes, target, BuildTarget.StandaloneLinux64, BuildOptions.None);
     }
}

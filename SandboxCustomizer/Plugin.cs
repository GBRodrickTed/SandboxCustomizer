using BepInEx;
using BepInEx.Configuration;
using SandboxCustomizer.Info;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ResourceManagement.Util.BinaryStorageBuffer.TypeSerializer;

namespace SandboxCustomizer
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool sandbox_loaded = false;
        GameObject indoor_light_object = null;
        GameObject outdoor_light_object = null;
        Transform day_indoors = null;
        Transform day_outdoors = null;
        public static List<CustomSandbox> sbs = new List<CustomSandbox>();
        List<string> tex_path = new List<string>();
        public static string data_folder_path = Path.Combine(Utils.ModDir(), "Data");
        public static string sandbox_folder_path = Path.Combine(Utils.ModDir(), "Sandboxes");
        public static string skybox_folder_path = Path.Combine(Utils.ModDir(), "Skyboxes");
        public static ConfigEntry<string> load_on_start;
        public static ConfigEntry<bool> remeber_last_sandbox;
        public static ConfigEntry<bool> auto_save_sandbox;
        public static ConfigEntry<float> tod_speed;
        
        public void Start()
        {
            AssetHandler.LoadBundle();
            AssetHandler.MakePrefabs();

            load_on_start = this.Config.Bind("Sandbox", "Load_Sandbox_On_Start", "Day");
            remeber_last_sandbox = this.Config.Bind("Sandbox", "Remember_Last_Sandbox", true);
            auto_save_sandbox = this.Config.Bind("Sandbox", "Auto_Save_Sandbox", false);
            tod_speed = this.Config.Bind("Sandbox", "Sandbox_Transition_Speed", 1f);

            foreach (string file in Directory.EnumerateFiles(skybox_folder_path))
            {
                for (int i = 0; i < Utils.imageTypes.Count; i++)
                {
                    if (string.Equals(Utils.imageTypes[i], Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                    {
                        tex_path.Add(file);
                        SandboxManager.tex_path.Add(file);
                        Debug.Log(file);
                    }
                }
            }

            Debug.Log("We did it reddit");
            SceneManager.sceneLoaded += SandboxLoadCheck;
            SceneManager.sceneUnloaded += SandboxUnloadCheck;
        }
        public void SandboxLoadCheck(Scene scene, LoadSceneMode sceneMode)
        {
            if (SceneHelper.CurrentScene == "uk_construct")
            {
                Debug.Log("Custom Sandbox has Loaded");
                sandbox_loaded = true;
                CustomSandbox.MakeBaseSandbox();
                SandboxManager.Clear();
                SandboxManager.Setup();

                foreach (string file in Directory.EnumerateFiles(Plugin.sandbox_folder_path))
                {
                    if (string.Equals(".csb", Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                    {
                        CustomSandbox sb = new CustomSandbox();
                        sb.info = SandboxInfo.LoadInfo(file);
                        if (sb.info != null)
                        {
                            SandboxManager.AddSandbox(sb);
                        }
                    }
                }
                SandboxManager.global_tod_speed = tod_speed.Value;
                SandboxManager.UpdateTODs();
                if (remeber_last_sandbox.Value)
                {
                    GameObject pongebob = null;
                    if (SandboxManager.vanilla_activators.TryGetValue(load_on_start.Value, out pongebob))
                    {
                        SandboxManager.HardForceBaseSandbox(load_on_start.Value);
                        SandboxManager.main_sandbox = load_on_start.Value;
                    }
                    else
                    {
                        foreach (CustomSandbox sandbox in SandboxManager.sandboxs)
                        {
                            if (sandbox.info.path == Path.Combine(sandbox_folder_path, load_on_start.Value))
                            {
                                SandboxManager.HardForceSandbox(sandbox);
                                break;
                            }
                        }
                    }
                } else
                {
                    load_on_start.SetSerializedValue("Day");
                }
                
            }
        }

        public void SandboxUnloadCheck(Scene scene)
        {
            if (SceneHelper.CurrentScene != "uk_construct" && sandbox_loaded)
            {
                Debug.Log("Sandbox has Unloaded.");
                SandboxManager.Clear();
                sandbox_loaded = false;
            }
        }

        public void Update()
        {
            if (sandbox_loaded)
            {
                //SandboxManager.doAThing();
            }
        }
    }
}
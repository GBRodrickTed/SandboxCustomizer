using Discord;
using SandboxCustomizer.Info;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.ResourceManagement.Util.BinaryStorageBuffer.TypeSerializer;
using Debug = UnityEngine.Debug;

namespace SandboxCustomizer.UI
{
    public class SkyboxPanel
    {
        public GameObject gameObject;
        public GameObject button_texture;
        public GameObject button_folder;
        public GameObject button_back;

        public Image skybox_image;

        public void Create(Transform transfrom)
        {
            gameObject = GameObject.Instantiate<GameObject>(AssetHandler.skybox_panel, transfrom);
            button_texture = Utils.ReplaceButtonForShop(gameObject.transform.Find("button texture").gameObject, gameObject.transform);
            button_folder = Utils.ReplaceButtonForShop(gameObject.transform.Find("button folder").gameObject, gameObject.transform);
            button_back = Utils.ReplaceButtonForShop(gameObject.transform.Find("button back").gameObject, gameObject.transform);
            skybox_image = gameObject.transform.Find("TextureDisplay").GetComponent<Image>();
            button_texture.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                string[] list = Directory.EnumerateFiles(Path.Combine(Plugin.skybox_folder_path, "PutSkyboxHere")).ToArray();
                if (list.Length > 0)
                {
                    foreach(string str in list)
                    {
                        for (int i = 0; i < Utils.imageTypes.Count; i++)
                        {
                            if (string.Equals(Utils.imageTypes[i], Path.GetExtension(str), StringComparison.OrdinalIgnoreCase))
                            {
                                Debug.Log(str);
                                SandboxManager.sandbox_current.LoadSkyboxFromPath(list[0]);
                                SandboxManager.sandbox_current.info.skybox.name = list[0].Replace(Path.Combine(Plugin.skybox_folder_path, "PutSkyboxHere\\"), "");

                                if (Plugin.auto_save_sandbox.Value && File.Exists(SandboxManager.sandbox_current.info.path))
                                {
                                    CSBUtils.SetStringField("Skybox", SandboxManager.sandbox_current.info.skybox.name, 0, SandboxManager.sandbox_current.info.path);
                                }

                                RenderSettings.skybox = SandboxManager.sandbox_current.skybox_mat;
                                Material newMat = new Material(skybox_image.material);
                                newMat.mainTexture = SandboxManager.sandbox_current.info.skybox.texture;
                                skybox_image.material = newMat;
                                //byte[] stuff = SandboxManager.sandbox_current.sandbox_info.skybox.texture.EncodeToPNG();
                                //Debug.Log(stuff.Count() + " vs " + Convert.ToBase64String(stuff).Length);

                                //Not needed anymore but could still be useful
                                /*SandboxManager.dead_sandbox.custom_activator.SetActive(true);
                                SandboxManager.tod_panel.SetActive(true);
                                SandboxManager.edit_panel.gameObject.SetActive(false);
                                SandboxManager.blocker.SetActive(true);
                                SandboxManager.backup_sandbox = SandboxManager.sandbox_current;

                                SandboxManager.backup_sandbox.tod_changer.speedMultiplier = SandboxManager.global_tod_speed * 2;
                                SandboxManager.dead_sandbox.tod_changer.speedMultiplier = SandboxManager.global_tod_speed * 2;

                                SandboxManager.on_unblock_event.AddListener(SandboxManager.RefreshSkybox);
                                SandboxManager.on_unblock_event.AddListener(UnHideStuff);
                                gameObject.SetActive(false);*/
                                //skybox_image.material.mainTexture = SandboxManager.sandbox_current.skybox_texture; // doesn't refresh it
                                //SandboxManager.sandbox_current. GOD RODDED
                                //SandboxManager.sandbox_current.skybox_mat.SetTexture("_MainTex", SandboxManager.sandbox_current.skybox_mat.mainTexture);
                                break;
                            }
                        }
                    }
                }
            }));

            button_folder.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                Application.OpenURL(Plugin.skybox_folder_path);
            }));

            button_back.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                gameObject.SetActive(false);
            }));
        }
        /*int this_is_a_really_bad_coding_practice_dont_do_this = 0;
        public void UnHideStuff()
        {
            this_is_a_really_bad_coding_practice_dont_do_this++;
            if (this_is_a_really_bad_coding_practice_dont_do_this > 1)
            {
                Debug.Log(SandboxManager.backup_sandbox == null);
                SandboxManager.backup_sandbox.tod_changer.speedMultiplier = SandboxManager.global_tod_speed;
                SandboxManager.dead_sandbox.tod_changer.speedMultiplier = SandboxManager.global_tod_speed;
                SandboxManager.tod_panel.SetActive(false);
                SandboxManager.edit_panel.gameObject.SetActive(true);
                SandboxManager.backup_sandbox = null;
                SandboxManager.on_unblock_event.RemoveListener(UnHideStuff);
                this_is_a_really_bad_coding_practice_dont_do_this = 0;
            }
        }*/
    }
}

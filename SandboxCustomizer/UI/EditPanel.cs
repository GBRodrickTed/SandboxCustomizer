using SandboxCustomizer.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SandboxCustomizer.UI
{
    public class EditPanel
    {
        public GameObject gameObject;
        public GameObject button_lights;
        public GameObject button_skybox;
        public GameObject button_fog;
        public GameObject button_misc;

        public Text sandbox_name;

        public GameObject button_save;
        public GameObject button_refresh;

        public GameObject button_back;

        public LightColorPanel light_panel;
        public SkyboxPanel skybox_panel;
        public FogPanel fog_panel;
        public MiscPanel misc_panel;

        public void Create(Transform transform)
        {
            gameObject = GameObject.Instantiate<GameObject>(AssetHandler.edit_panel, transform);
            button_lights = Utils.ReplaceButtonForShop(gameObject.transform.Find("button lights").gameObject, gameObject.transform);
            button_skybox = Utils.ReplaceButtonForShop(gameObject.transform.Find("button skybox").gameObject, gameObject.transform);
            button_fog = Utils.ReplaceButtonForShop(gameObject.transform.Find("button fog").gameObject, gameObject.transform);
            button_misc = Utils.ReplaceButtonForShop(gameObject.transform.Find("button misc").gameObject, gameObject.transform);

            sandbox_name = gameObject.transform.Find("SandboxName").gameObject.GetComponent<Text>();

            button_save = Utils.ReplaceButtonForShop(gameObject.transform.Find("button save").gameObject, gameObject.transform);
            button_refresh = Utils.ReplaceButtonForShop(gameObject.transform.Find("button refresh").gameObject, gameObject.transform);

            button_back = Utils.ReplaceButtonForShop(gameObject.transform.Find("button back").gameObject, gameObject.transform);

            light_panel = new LightColorPanel();
            light_panel.Create(gameObject.transform);
            light_panel.gameObject.SetActive(false);

            skybox_panel = new SkyboxPanel();
            skybox_panel.Create(gameObject.transform);
            skybox_panel.gameObject.SetActive(false);

            fog_panel = new FogPanel();
            fog_panel.Create(gameObject.transform);
            fog_panel.gameObject.SetActive(false);

            misc_panel = new MiscPanel();
            misc_panel.Create(gameObject.transform);
            misc_panel.gameObject.SetActive(false);

            button_back.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                SandboxManager.tod_panel.SetActive(true);
                gameObject.SetActive(false);
            }));

            button_save.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                if (SandboxManager.sandbox_current != null)
                {
                    SandboxInfo.SaveInfo(SandboxManager.sandbox_current.info);
                    Plugin.load_on_start.SetSerializedValue(SandboxManager.sandbox_current.info.path.Replace(Plugin.sandbox_folder_path + "\\", ""));
                }
                UpdateName();
            }));

            button_refresh.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                if (SandboxManager.sandbox_current.info.path != null)
                {
                    SandboxManager.sandbox_current.info = SandboxInfo.LoadInfo(SandboxManager.sandbox_current.info.path);
                    string tex_path = "bruh";
                    //MAYBE: not this
                    if (File.Exists(Path.Combine(Plugin.skybox_folder_path, SandboxManager.sandbox_current.info.skybox.name)))
                    {
                        tex_path = Path.Combine(Plugin.skybox_folder_path, SandboxManager.sandbox_current.info.skybox.name);
                    } else if (File.Exists(Path.Combine(Plugin.skybox_folder_path, "PutSkyboxHere", SandboxManager.sandbox_current.info.skybox.name)))
                    {
                        tex_path = Path.Combine(Plugin.skybox_folder_path, "PutSkyboxHere", SandboxManager.sandbox_current.info.skybox.name);
                    }
                    SandboxManager.sandbox_current.LoadSkyboxFromPath(tex_path);
                    SandboxManager.sandbox_current.UpdateEffects();
                    RenderSettings.skybox = SandboxManager.sandbox_current.skybox_mat;
                    //SandboxManager.sandbox_current.tod_changer.newSkybox = SandboxManager.sandbox_current.skybox_mat;
                    //SandboxManager.sandbox_current.skybox_mat.mainTexture = SandboxManager.sandbox_current.info.skybox.texture;
                }
            }));

            button_lights.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                light_panel.gameObject.SetActive(true);
                if (SandboxManager.sandbox_current != null)
                {
                    light_panel.ClearAllSliders();

                    foreach (LightInfo light_info in SandboxManager.sandbox_current.info.outdoor_lights)
                    {
                        LightColorSlider slider = new LightColorSlider();
                        light_panel.AddColorSlider(slider);
                        slider.SetColor(light_info.color);
                        slider.on_value_change_event.AddListener(new UnityAction(() =>
                        {
                            {
                                light_info.color = slider.color;
                                SandboxManager.sandbox_current.UpdateEffects();
                                Color color = new Color(slider.slider_r.value, slider.slider_g.value, slider.slider_b.value, slider.slider_mag.value);
                                if (Plugin.auto_save_sandbox.Value && File.Exists(SandboxManager.sandbox_current.info.path))
                                {
                                    CSBUtils.SetColorField("Outdoor Lights", color, SandboxManager.sandbox_current.info.outdoor_lights.IndexOf(light_info), SandboxManager.sandbox_current.info.path);
                                }
                            }
                        }));
                    }

                    foreach (LightInfo light_info in SandboxManager.sandbox_current.info.indoor_lights)
                    {
                        LightColorSlider slider = new LightColorSlider();
                        light_panel.AddColorSlider(slider);
                        slider.SetColor(light_info.color);
                        slider.on_value_change_event.AddListener(new UnityAction(() =>
                        {
                            {
                                light_info.color = slider.color;
                                SandboxManager.sandbox_current.UpdateEffects();
                                Color color = new Color(slider.slider_r.value, slider.slider_g.value, slider.slider_b.value, slider.slider_mag.value);
                                if (Plugin.auto_save_sandbox.Value && File.Exists(SandboxManager.sandbox_current.info.path))
                                {
                                    CSBUtils.SetColorField("Indoor Lights", color, SandboxManager.sandbox_current.info.indoor_lights.IndexOf(light_info), SandboxManager.sandbox_current.info.path);
                                }
                            }
                        }));
                    }
                }
            }));

            button_skybox.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                skybox_panel.gameObject.SetActive(true);
                skybox_panel.skybox_image.material.mainTexture = SandboxManager.sandbox_current.info.skybox.texture;
            }));

            button_fog.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                fog_panel.gameObject.SetActive(true);
                fog_panel.on_value_change_event.RemoveAllListeners();
                fog_panel.fog_enable = SandboxManager.sandbox_current.info.fog.enabled;
                fog_panel.SetColor(SandboxManager.sandbox_current.info.fog.color);
                fog_panel.fog_start.value = SandboxManager.sandbox_current.info.fog.start;
                fog_panel.fog_end.value = SandboxManager.sandbox_current.info.fog.end;
                if (fog_panel.fog_enable == true)
                {
                    fog_panel.fog_enable_img.SetActive(true);
                } else
                {
                    fog_panel.fog_enable_img.SetActive(false);
                }
                fog_panel.on_value_change_event.AddListener(new UnityAction(() =>
                {
                    SandboxManager.sandbox_current.info.fog.color = fog_panel.fog_color;
                    SandboxManager.sandbox_current.info.fog.enabled = fog_panel.fog_enable;
                    SandboxManager.sandbox_current.info.fog.start = fog_panel.fog_start.value;
                    SandboxManager.sandbox_current.info.fog.end = fog_panel.fog_end.value;
                    RenderSettings.fogColor = fog_panel.fog_color;
                    if (Plugin.auto_save_sandbox.Value && File.Exists(SandboxManager.sandbox_current.info.path))
                    {
                        CSBUtils.SetColorField("Fog", fog_panel.fog_color, 0, SandboxManager.sandbox_current.info.path);
                        CSBUtils.SetFloatField("Fog", fog_panel.fog_start.value, 1, SandboxManager.sandbox_current.info.path);
                        CSBUtils.SetFloatField("Fog", fog_panel.fog_end.value, 2, SandboxManager.sandbox_current.info.path);
                        CSBUtils.SetBoolField("Fog", fog_panel.fog_enable, 3, SandboxManager.sandbox_current.info.path);
                    }
                    if (fog_panel.fog_enable == true)
                    {
                        RenderSettings.fogStartDistance = fog_panel.fog_start.value;
                        RenderSettings.fogEndDistance = fog_panel.fog_end.value;
                        fog_panel.fog_enable_img.SetActive(true);
                    }
                    else        
                    {
                        RenderSettings.fogStartDistance = 1e+25f;
                        RenderSettings.fogEndDistance = 1e+26f;
                        fog_panel.fog_enable_img.SetActive(false);
                    }
                    SandboxManager.sandbox_current.UpdateEffects();
                }));
            }));

            button_misc.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                misc_panel.gameObject.SetActive(true);
                misc_panel.godray_alpha.value = SandboxManager.sandbox_current.info.godrays[0].color.a;
                misc_panel.on_value_change_event.RemoveAllListeners();
                misc_panel.on_value_change_event.AddListener(new UnityAction(() =>
                {
                    foreach (GodrayInfo godray_info in SandboxManager.sandbox_current.info.godrays)
                    {
                        godray_info.color.a = misc_panel.godray_alpha.value;
                        if (Plugin.auto_save_sandbox.Value && File.Exists(SandboxManager.sandbox_current.info.path))
                        {
                            CSBUtils.SetColorField("Godrays", godray_info.color, SandboxManager.sandbox_current.info.godrays.IndexOf(godray_info), SandboxManager.sandbox_current.info.path);
                        }
                    }

                    SandboxManager.sandbox_current.UpdateEffects();
                }));
            }));
        }

        public void UpdateName()
        {
            string new_name = "";
            if (SandboxManager.sandbox_current != null)
            {
                if (SandboxManager.sandbox_current.info.path != null)
                {
                    new_name = SandboxManager.sandbox_current.info.path.Replace(Plugin.sandbox_folder_path + "\\", "");
                    new_name = new_name.Replace(".csb", "");
                }
            }
            sandbox_name.text = new_name;
        }
    }
}

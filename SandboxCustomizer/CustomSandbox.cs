using SandboxCustomizer.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ULTRAKILL.Cheats;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using static UnityEngine.ResourceManagement.Util.BinaryStorageBuffer.TypeSerializer;

namespace SandboxCustomizer
{
    public class CustomSandbox
    {
        public static GameObject base_indoors;
        public static GameObject base_outdoors;
        public static GameObject base_activator;

        public Material skybox_mat;

        public GameObject custom_indoors;
        public GameObject custom_outdoors;
        public GameObject custom_activator;

        public TimeOfDayChanger tod_changer;
        public ObjectActivator obj_activator;
        public UltrakillEvent uk_event;
        public List<Light> new_lights = new List<Light>();
        public List<Light> indoor_lights = new List<Light>();
        public List<Light> outdoor_lights = new List<Light>();
        public List<MeshRenderer> godrays = new List<MeshRenderer>();

        public SandboxInfo info = new SandboxInfo();

        public bool in_scene = false;

        public static void MakeBaseSandbox()
        {
            GameObject day_indoors = GameObject.Find("IndoorsLighting").transform.Find("Day").gameObject;
            GameObject day_outdoors = GameObject.Find("OutdoorsLighting").transform.Find("Day").gameObject;
            GameObject day_activator = GameObject.Find("Activators").transform.Find("Day Activator").gameObject;

            base_indoors = GameObject.Instantiate<GameObject>(day_indoors, day_indoors.transform.parent);
            base_indoors.name = "Base (Indoors)";
            base_indoors.SetActive(false);
            base_outdoors = GameObject.Instantiate<GameObject>(day_outdoors, day_outdoors.transform.parent);
            base_outdoors.name = "Base (Outdoors)";
            base_outdoors.SetActive(false);
            base_activator = GameObject.Instantiate<GameObject>(day_activator, day_activator.transform.parent);
            base_activator.name = "Base Activator";
            base_activator.SetActive(false);
        }
        public void AddToScene() // Clones the day stuff and adds custom effects 
        {
            if (in_scene) return;
            GameObject day_indoors = GameObject.Find("IndoorsLighting").transform.Find("Day").gameObject;
            GameObject day_outdoors = GameObject.Find("OutdoorsLighting").transform.Find("Day").gameObject;
            GameObject day_activator = GameObject.Find("Activators").transform.Find("Day Activator").gameObject;

            custom_indoors = GameObject.Instantiate<GameObject>(base_indoors, base_indoors.transform.parent);
            custom_indoors.name = "Custom (Indoors)";
            //custom_indoors.SetActive(false);
            custom_outdoors = GameObject.Instantiate<GameObject>(base_outdoors, base_outdoors.transform.parent);
            custom_outdoors.name = "Custom (Outdoors)";
            //custom_outdoors.SetActive(false);
            custom_activator = GameObject.Instantiate<GameObject>(base_activator, base_activator.transform.parent);
            custom_activator.name = "Custom Activator";
            //custom_activator.SetActive(false);

            // gives tod it's own material as to not share with day
            tod_changer = custom_activator.GetComponentInChildren<TimeOfDayChanger>();
            skybox_mat = new Material(tod_changer.newSkybox);
            
            if (info.skybox == null)
            {
                info.skybox = new SkyboxInfo();
                info.skybox.texture = (Texture2D)base_activator.GetComponentInChildren<TimeOfDayChanger>().newSkybox.mainTexture;
                info.skybox.name = null;
                skybox_mat.mainTexture = info.skybox.texture;
            } else
            {
                skybox_mat.mainTexture = info.skybox.texture;
            }

            tod_changer.newSkybox = skybox_mat;
            obj_activator = custom_activator.GetComponent<ObjectActivator>();
            uk_event = obj_activator.events;
            uk_event.toActivateObjects = new GameObject[] { custom_indoors, custom_outdoors };
            
            Light[] temp_indoor_lights = custom_indoors.GetComponentsInChildren<Light>();
            if (info.indoor_lights != null)
            {
                for (int i = 0; i < temp_indoor_lights.Length; i++)
                {
                    indoor_lights.Add(temp_indoor_lights[i]);
                    temp_indoor_lights[i].color = info.indoor_lights[i].color;
                }
            } else
            {
                info.indoor_lights = new List<LightInfo>();
                for (int i = 0; i < temp_indoor_lights.Length; i++)
                {
                    indoor_lights.Add(temp_indoor_lights[i]);
                    LightInfo light_info = new LightInfo();
                    light_info.color = temp_indoor_lights[i].color;
                    info.indoor_lights.Add(light_info);
                }
            }

            Light[] temp_outdoor_lights = custom_outdoors.GetComponentsInChildren<Light>();
            if (info.outdoor_lights != null)
            {
                for (int i = 0; i < temp_outdoor_lights.Length; i++)
                {
                    outdoor_lights.Add(temp_outdoor_lights[i]);
                    temp_outdoor_lights[i].color = info.outdoor_lights[i].color;
                }
            }
            else
            {
                info.outdoor_lights = new List<LightInfo>();
                for (int i = 0; i < temp_outdoor_lights.Length; i++)
                {
                    outdoor_lights.Add(temp_outdoor_lights[i]);
                    LightInfo light_info = new LightInfo();
                    light_info.color = temp_outdoor_lights[i].color;
                    info.outdoor_lights.Add(light_info);
                }
            }

            MeshRenderer[] temp_potential_godrays = custom_indoors.GetComponentsInChildren<MeshRenderer>();

            if (info.godrays != null)
            {
                for (int i = 0; i < temp_potential_godrays.Length; i++)
                {
                    godrays.Add(temp_potential_godrays[i]);
                    temp_potential_godrays[i].material.color = info.godrays[i].color;
                }
            }
            else
            {
                info.godrays = new List<GodrayInfo>();
                for (int i = 0; i < temp_potential_godrays.Length; i++)
                {
                    godrays.Add(temp_potential_godrays[i]);
                    GodrayInfo godray_info = new GodrayInfo();
                    godray_info.color = temp_potential_godrays[i].material.color;
                    info.godrays.Add(godray_info);
                }
            }

            if (info.fog != null)
            {
                tod_changer.fogColor = info.fog.color;
                if (info.fog.enabled)
                {
                    tod_changer.fogStart = info.fog.start;
                    tod_changer.fogEnd = info.fog.end;
                } else
                {
                    tod_changer.fogStart = 1e+21f;
                    tod_changer.fogEnd = 1e+20f;
                }
            } else
            {
                info.fog = new FogInfo();
                info.fog.color = tod_changer.fogColor;
                info.fog.start = tod_changer.fogStart;
                info.fog.end = tod_changer.fogEnd;
                info.fog.enabled = true;
            }

            foreach (Light light in custom_outdoors.GetComponentsInChildren<Light>())
            {
                new_lights.Add(light);
            }
            tod_changer.newLights = new_lights.ToArray();
            UpdateEffects();
            in_scene = true;
        }

        public void RemoveFromScene()
        {
            if (!in_scene) return;
            GameObject.Destroy(custom_indoors);
            GameObject.Destroy(custom_outdoors);
            GameObject.Destroy(custom_activator);
            tod_changer = null;
            obj_activator = null;
            uk_event = null;
            in_scene = false;
            indoor_lights.Clear();
            outdoor_lights.Clear();
            godrays.Clear();
        }

        public void Clear()
        {
            RemoveFromScene();
            tod_changer = null;
            obj_activator = null;
            uk_event = null;
            in_scene = false;
        }

        public void LoadSkyboxFromPath(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("Error when trying to load skybox from path: " + path);
                return;
            }
            byte[] data = File.ReadAllBytes(path);
            if (info.skybox == null)
            {
                info.skybox = new SkyboxInfo();
            }
            info.skybox.texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            info.skybox.texture.filterMode = FilterMode.Point;
            info.skybox.texture.LoadImage(data);
            if (skybox_mat != null)
            {
                skybox_mat.mainTexture = info.skybox.texture;
            }
        }

        public void LoadSkyboxFromData(byte[] data)
        {
            Texture2D tempTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
            if (!tempTexture.LoadImage(data))
            {
                Debug.Log("Error when trying to load sandbox from data");
                return;
            }
            if (info.skybox == null)
            {
                info.skybox = new SkyboxInfo();
            }
            info.skybox.texture = tempTexture;
            info.skybox.texture.filterMode = FilterMode.Point;
            if (skybox_mat != null)
            {
                skybox_mat.mainTexture = info.skybox.texture;
            }
        }

        public void LoadSkyboxFromURL(string url)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            request.SendWebRequest().completed += delegate
            {
                try
                {
                    if (!request.isNetworkError && !request.isHttpError)
                    {
                        if (info.skybox == null)
                        {
                            info.skybox = new SkyboxInfo();
                        }
                        info.skybox.texture = DownloadHandlerTexture.GetContent(request);
                        /*skybox_mat.mainTexture = skybox_texture;
                        Material skybox_mat = new Material(Shader.Find("Skybox/Panoramic"));*/
                        if (skybox_mat != null)
                        {
                            skybox_mat.mainTexture = info.skybox.texture;
                        }
                    }
                    else
                    {
                        Debug.Log("Error when trying to load skybox from path/url: " + url);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
                finally
                {
                    request.Dispose();
                }

            };
        }

        //MAYBE: Seperate updates into different functions for more efficiency
        public void UpdateEffects()
        {
            if (custom_indoors != null)
            {
                for (int i = 0; i < indoor_lights.Count; i++)
                {
                    indoor_lights[i].color = info.indoor_lights[i].color;
                }
            }

            if (custom_outdoors != null)
            {
                for (int i = 0; i < outdoor_lights.Count; i++)
                {
                    outdoor_lights[i].color = info.outdoor_lights[i].color;
                }
            }

            if (custom_indoors != null)
            {
                for (int i = 0; i < godrays.Count; i++)
                {
                    godrays[i].material.color = info.godrays[i].color;
                }
            }

            tod_changer.fogColor = info.fog.color;
            if (info.fog.enabled)
            {
                tod_changer.fogStart = info.fog.start;
                tod_changer.fogEnd = info.fog.end;
            }
            else
            {
                tod_changer.fogStart = 1e+25f; // arbitrarily high floats
                tod_changer.fogEnd = 1e+26f;
            }


            //TODO: forgot why this is important, but it is.
            new_lights.Clear();
            foreach (Light light in custom_outdoors.GetComponentsInChildren<Light>())
            {
                new_lights.Add(light);
            }
            tod_changer.newLights = new_lights.ToArray();
        }
    }
}
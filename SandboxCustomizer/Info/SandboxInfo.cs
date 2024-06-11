using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ResourceManagement.Util.BinaryStorageBuffer.TypeSerializer;

namespace SandboxCustomizer.Info
{
    //important information that we want to save/load
    public class SandboxInfo
    {
        public List<LightInfo> indoor_lights;
        public List<LightInfo> outdoor_lights;
        public List<GodrayInfo> godrays;
        public SkyboxInfo skybox;
        public FogInfo fog;

        public string path;

        public static void SaveInfo(SandboxInfo info)
        {
            if (info == null)
            {
                Debug.Log("Sandbox info is null");
                return;
            }

            if (info.path != null)
            {
                if (!File.Exists(info.path))
                {
                    Debug.Log("Yeah im woopin feeet");
                    info.path = null;
                }
            }

            if (info.path == null)
            {
                int count = 0;
                bool found_avalable_name = false;
                foreach (string file in Directory.EnumerateFiles(Plugin.sandbox_folder_path))
                {
                    if (string.Equals(".csb", Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                    {
                        string bep = file.Replace(".csb", "");
                        bep = bep.Replace(Path.Combine(Plugin.sandbox_folder_path, "sandbox"), "");
                        int bop = 0;
                        if (int.TryParse(bep, out bop))
                        {
                            found_avalable_name = true;
                            count = Math.Max(bop, count);
                        }
                    }
                }
                if (found_avalable_name)
                {
                    count += 1;
                }
                info.path = CreateCSB("sandbox" + count, Plugin.sandbox_folder_path);

            }

            for(int i = 0; i < info.indoor_lights.Count; i++)
            {
                Color col = info.indoor_lights[i].color;
                float alpha = Mathf.Max(1, col.a);
                col.r = col.r / (alpha);
                col.g = col.g / (alpha);
                col.b = col.b / (alpha);
                col.a = col.a - 1;
                CSBUtils.SetColorField("Indoor Lights", col, i, info.path);
            }

            for (int i = 0; i < info.outdoor_lights.Count; i++)
            {
                Color col = info.outdoor_lights[i].color;
                float alpha = Mathf.Max(1, col.a);
                col.r = col.r / (alpha);
                col.g = col.g / (alpha);
                col.b = col.b / (alpha);
                col.a = col.a - 1;
                CSBUtils.SetColorField("Outdoor Lights", col, i, info.path);
            }

            for (int i = 0; i < info.godrays.Count; i++)
            {
                CSBUtils.SetColorField("Godrays", info.godrays[i].color, i, info.path);
            }
            CSBUtils.SetColorField("Fog", info.fog.color, 0, info.path);
            CSBUtils.SetFloatField("Fog", info.fog.start, 1, info.path);
            CSBUtils.SetFloatField("Fog", info.fog.end, 2, info.path);
            CSBUtils.SetBoolField("Fog", info.fog.enabled, 3, info.path);

            if (info.skybox.name == null)
            {
                CSBUtils.SetStringField("Skybox", "", 0, info.path);
            } else
            {
                CSBUtils.SetStringField("Skybox", info.skybox.name, 0, info.path);
            }
        }

        public static SandboxInfo LoadInfo(string path)
        {
            if (!File.Exists(path))
            {
                Debug.Log("File " + path + " does not exist");
                return null;
            }
            SandboxInfo info = new SandboxInfo();

            info.path = path;

            info.indoor_lights = new List<LightInfo>();
            info.outdoor_lights = new List<LightInfo>();
            info.godrays = new List<GodrayInfo>();
            info.fog = new FogInfo();

            for (int i = 0; i < 8; i++)
            {
                LightInfo light_info = new LightInfo();
                Color col = CSBUtils.GetColorField("Indoor Lights", i, path);

                col.r = col.r * (col.a + 1);
                col.g = col.g * (col.a + 1);
                col.b = col.b * (col.a + 1);
                col.a = (col.a + 1);

                light_info.color = col;
                info.indoor_lights.Add(light_info);
            }

            for (int i = 0; i < 3; i++)
            {
                LightInfo light_info = new LightInfo();
                Color col = CSBUtils.GetColorField("Outdoor Lights", i, path);

                col.r = col.r * (col.a + 1);
                col.g = col.g * (col.a + 1);
                col.b = col.b * (col.a + 1);
                col.a = (col.a + 1);

                light_info.color = col;
                info.outdoor_lights.Add(light_info);
            }

            for (int i = 0; i < 4; i++)
            {
                GodrayInfo godray_info = new GodrayInfo();
                godray_info.color = CSBUtils.GetColorField("Godrays", i, path);
                info.godrays.Add(godray_info);
            }

            FogInfo fog_info = new FogInfo();
            fog_info.color = CSBUtils.GetColorField("Fog", 0, path);
            fog_info.start = CSBUtils.GetFloatField("Fog", 1, path);
            fog_info.end = CSBUtils.GetFloatField("Fog", 2, path);
            fog_info.enabled = CSBUtils.GetBoolField("Fog", 3, path);
            info.fog = fog_info;

            SkyboxInfo skybox_info = new SkyboxInfo();

            string path_part = CSBUtils.GetStringField("Skybox", 0, path);
            string tex_path = null;
            if (path_part != null && path_part != "")
            {
                tex_path = Path.Combine(Plugin.skybox_folder_path, path_part);
                if (!File.Exists(tex_path))
                {
                    tex_path = Path.Combine(Plugin.skybox_folder_path, "PutSkyboxHere", path_part);
                    if (!File.Exists(tex_path))
                    {
                        Debug.Log("Skybox Texture could not be found in skybox folder");
                        tex_path = null;
                    }
                }
            } else
            {
                tex_path = null;
            }
            
            if (tex_path != null)
            {
                skybox_info.texture = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                skybox_info.texture.filterMode = FilterMode.Point;
                skybox_info.texture.LoadImage(File.ReadAllBytes(tex_path));
                skybox_info.name = path_part;
                info.skybox = skybox_info;
            }

            return info;
        }

        public static string CreateCSB(string name, string directory)
        {
            string path = CSBUtils.CreateCSB(name, directory);

            CSBUtils.CreateContainer("Indoor Lights", path);
            for (int i = 0; i < 8; i++)
            {
                CSBUtils.CreateColorField("Indoor Lights", Color.clear, path);
            }
            CSBUtils.CreateContainer("Outdoor Lights", path);
            for (int i = 0; i < 3; i++)
            {
                CSBUtils.CreateColorField("Outdoor Lights", Color.clear, path);
            }
            CSBUtils.CreateContainer("Godrays", path);
            for (int i = 0; i < 4; i++)
            {
                CSBUtils.CreateColorField("Godrays", Color.clear, path);
            }
            CSBUtils.CreateContainer("Fog", path);
            CSBUtils.CreateColorField("Fog", Color.clear, path);
            CSBUtils.CreateFloatField("Fog", 0, path);
            CSBUtils.CreateFloatField("Fog", 0, path);
            CSBUtils.CreateBoolField("Fog", false, path);
            CSBUtils.CreateContainer("Skybox", path);
            CSBUtils.CreateBase64Field("Skybox", "", path);

            return path;
        }
    }
}

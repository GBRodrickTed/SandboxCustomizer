using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SandboxCustomizer
{
    public static class Utils
    {
        public static List<string> imageTypes = new List<string> { ".jpeg", ".jpg", ".png", ".bmp" };
        public static string ModDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        // solution from https://discussions.unity.com/t/copy-a-component-at-runtime/71172/3
        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy;
            /*if (destination.TryGetComponent<T>(out T dest_type))
            {
                copy = dest_type;
            }
            else
            {*/
                copy = destination.AddComponent(type);
            //} 
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        public static List<string> LayerMaskNames(LayerMask lmask)
        {
            List<string> layerNames = new List<string>();
            int layerInBytes = 1;
            for (int i = 0; i < 32; i++)
            {
                if ((lmask.value & layerInBytes) > 0)
                {
                    layerNames.Add(LayerMask.LayerToName(i));
                    Debug.Log(LayerMask.LayerToName(i) + " " + i);
                }
                layerInBytes *= 2;
            }
            return layerNames;
        }
        static GameObject shop_slider_base = null;
        static GameObject shop_button_base = null;
        // used to find the location of ui we want to clone
        //TODO: find a way to make unity ui interactible without this nonsense
        public static void InitUIObjets()
        {
            shop_slider_base = GameObject.Find("Shop/Canvas/Weapons/RevolverWindow/Color Screen/Standard/Custom/Unlocked/Color 1/Red/Slider");
            shop_button_base = GameObject.Find("Sandbox Shop/Canvas/Border/TOD Changer/Panel/Close Button");

        }
        public static GameObject CloneButtonForShop(GameObject button_base, Transform transform) // TODO: Find how to do it without close button
        {
            if (shop_button_base == null)
            {
                Debug.Log("Couldn;t clode the buteon");
                return null;
            }
            GameObject dest = GameObject.Instantiate(shop_button_base, transform);
            GameObject.Destroy(dest.transform.Find("Text").gameObject);
            dest.transform.localPosition = button_base.transform.localPosition;
            for (int i = 0; i < button_base.transform.childCount; i++)
            {
                if (button_base.transform.GetChild(i).name == "Panel")
                {
                    continue;
                }
                GameObject.Instantiate(button_base.transform.GetChild(i), dest.transform);
            }
            dest.name = button_base.name;
            dest.GetComponent<RectTransform>().anchoredPosition = button_base.GetComponent<RectTransform>().anchoredPosition;
            dest.GetComponent<RectTransform>().anchoredPosition3D = button_base.GetComponent<RectTransform>().anchoredPosition3D;
            dest.GetComponent<RectTransform>().anchorMax = button_base.GetComponent<RectTransform>().anchorMax;
            dest.GetComponent<RectTransform>().anchorMin = button_base.GetComponent<RectTransform>().anchorMin;
            dest.GetComponent<RectTransform>().offsetMin = button_base.GetComponent<RectTransform>().offsetMin;
            dest.GetComponent<RectTransform>().offsetMax = button_base.GetComponent<RectTransform>().offsetMax;

            dest.GetComponent<ShopButton>().toActivate = new GameObject[0];
            dest.GetComponent<ShopButton>().toDeactivate = new GameObject[0];

            dest.AddComponent<PointerAdder>();
            return dest;
        }
        public static GameObject ReplaceButtonForShop(GameObject button_base, Transform transform)
        {
            GameObject dest = CloneButtonForShop(button_base, transform);
            GameObject.Destroy(button_base);
            return dest;
        }
        public static GameObject CloneSliderForShop(GameObject slider_base, Transform transform)
        {
            if (shop_slider_base == null)
            {
                Debug.Log("codit colne ths ikdler");
                return null;
            }
            
            GameObject dest = GameObject.Instantiate(shop_slider_base, transform);
            dest.SetActive(true);
            dest.transform.localPosition = slider_base.transform.localPosition;
            dest.name = slider_base.name;
            dest.GetComponent<RectTransform>().anchoredPosition = slider_base.GetComponent<RectTransform>().anchoredPosition;
            dest.GetComponent<RectTransform>().anchoredPosition3D = slider_base.GetComponent<RectTransform>().anchoredPosition3D;
            dest.GetComponent<RectTransform>().anchorMax = slider_base.GetComponent<RectTransform>().anchorMax;
            dest.GetComponent<RectTransform>().anchorMin = slider_base.GetComponent<RectTransform>().anchorMin;
            dest.GetComponent<RectTransform>().offsetMin = slider_base.GetComponent<RectTransform>().offsetMin;
            dest.GetComponent<RectTransform>().offsetMax = slider_base.GetComponent<RectTransform>().offsetMax;
            dest.transform.Find("Background").GetComponent<Image>().color = slider_base.transform.Find("Background").GetComponent<Image>().color;
            dest.transform.Find("Fill Area/Fill").GetComponent<Image>().color = slider_base.transform.Find("Fill Area/Fill").GetComponent<Image>().color;
            dest.transform.Find("Handle Slide Area/Handle").GetComponent<Image>().color = slider_base.transform.Find("Handle Slide Area/Handle").GetComponent<Image>().color;
            GameObject.Destroy(dest.transform.Find("Value").gameObject);
            dest.GetComponent<Slider>().onValueChanged = slider_base.GetComponent<Slider>().onValueChanged;
            /*for (int i = 0; i < slider_base.transform.childCount; i++)
            {
                if (slider_base.transform.GetChild(i).name == "Background" )
                {
                    continue;
                }
                GameObject.Instantiate(button_base.transform.GetChild(i), dest.transform);
            }*/
            slider_base.SetActive(false);
            return dest;
        }
        public static GameObject ReplaceSliderForShop(GameObject slider_base, Transform transform)
        {
            GameObject dest = CloneSliderForShop(slider_base, transform);
            GameObject.Destroy(slider_base);
            return dest;
        }

        //TODO: totally forgot what this does
        public static int[] BitCountFinder(int num) //has nothing to do with the mod, just thought it was cool
        {
            if (num < 1)
            {
                Debug.Log("bruh");
                return null;
            }
            int max_digit = (int)Math.Ceiling(Math.Log(num + 1, 2));
            int arr_size = max_digit; //lazy

            int[] digit_count = new int[arr_size];
            for (int i = 0; i < max_digit; i++)
            {
                digit_count[i] = 0;
            }
            int c = 0;
            for (int i = 0; i < num + 1; i++)
            {
                c = 0;
                for (int j = 0; j < max_digit; j++)
                {
                    if ((((int)Math.Pow(2, j)) & i) > 0)
                    {
                        c++;
                    }
                }
                if (c > 0)
                {
                    digit_count[c - 1]++;
                }
            }

            for (int i = 0; i < max_digit; i++)
            {
                Debug.Log(i + 1 + ": " + digit_count[i]);
            }
            return digit_count;
        }
    }

    public static class AssetHandler
    {
        public static AssetBundle bundle;
        public static bool bundleLoaded = false;
        public static void LoadBundle()
        {
            if (bundleLoaded)
            {
                Debug.Log("Bundle already loaded");
                return;
            }
            bundle = AssetBundle.LoadFromFile(Path.Combine(Path.Combine(Utils.ModDir(), "Data"), "custom_sandbox_assets"));
            if (bundle != null)
            {
                Debug.Log("Bundle successfully loaded");
                bundleLoaded = true;
            }
            else
            {
                Debug.Log("Bundle failed to loaded");
                bundleLoaded = false;
            }
        }

        public static T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            if (!bundleLoaded)
            {
                Debug.Log("Bundle is not loaded");
                return null;
            }

            T asset = bundle.LoadAsset<T>(name);
            if (asset == null)
            {
                Debug.Log(name + " didn't load");
            }
            return asset;
        }

        public static void UnloadBundle()
        {
            if (!bundleLoaded)
            {
                Debug.Log("Bundle already unloaded");
                return;
            }
            Debug.Log("Bundle succesfully unloaded");
            bundle.Unload(true);
            bundle = null;
        }
        public static GameObject edit_panel = null;
        public static GameObject light_panel = null;
        public static GameObject light_color_slider = null;
        public static GameObject skybox_panel = null;
        public static GameObject fog_panel = null;
        public static GameObject misc_panel = null;
        public static GameObject settings_panel = null;
        
        public static void MakePrefabs()
        {
            edit_panel = AssetHandler.LoadAsset<GameObject>("EditPanel");
            edit_panel.AddComponent<HudOpenEffect>();

            light_panel = AssetHandler.LoadAsset<GameObject>("LightColorPanel");
            light_panel.AddComponent<HudOpenEffect>();

            light_color_slider = AssetHandler.LoadAsset<GameObject>("LightColorSlider");

            skybox_panel = AssetHandler.LoadAsset<GameObject>("SkyboxPanel");
            skybox_panel.AddComponent<HudOpenEffect>();

            fog_panel = AssetHandler.LoadAsset<GameObject>("FogPanel");
            fog_panel.AddComponent<HudOpenEffect>();

            misc_panel = AssetHandler.LoadAsset<GameObject>("MiscPanel");
            misc_panel.AddComponent<HudOpenEffect>();

            settings_panel = AssetHandler.LoadAsset<GameObject>("SettingsPanel");
            settings_panel.AddComponent<HudOpenEffect>();
        }
    }
}

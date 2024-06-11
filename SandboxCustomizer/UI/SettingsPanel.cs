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
    public class SettingsPanel
    {
        public GameObject gameObject;
        public GameObject button_back;
        public GameObject button_folder;
        public GameObject button_reload;

        public GameObject button_auto_save;
        public GameObject button_auto_save_img;
        public GameObject button_remember;
        public GameObject button_remember_img;

        public bool auto_save_enabled;
        public bool remember_enabled;

        public Slider tod_slider;
        public Text tod_speed_value;

        public void Create(Transform transform)
        {
            gameObject = GameObject.Instantiate<GameObject>(AssetHandler.settings_panel, transform);
            button_back = Utils.ReplaceButtonForShop(gameObject.transform.Find("button back").gameObject, gameObject.transform);
            button_folder = Utils.ReplaceButtonForShop(gameObject.transform.Find("button folder").gameObject, gameObject.transform);
            button_reload = Utils.ReplaceButtonForShop(gameObject.transform.Find("button reload").gameObject, gameObject.transform);

            button_remember = Utils.ReplaceButtonForShop(gameObject.transform.Find("Mem Text/enable button").gameObject, gameObject.transform.Find("Mem Text"));
            button_auto_save = Utils.ReplaceButtonForShop(gameObject.transform.Find("Auto Save/enable button").gameObject, gameObject.transform.Find("Auto Save"));

            button_remember_img = gameObject.transform.Find("Mem Text/enable button/Image(Clone)").gameObject;
            button_auto_save_img = gameObject.transform.Find("Auto Save/enable button/Image(Clone)").gameObject;

            tod_slider = Utils.ReplaceSliderForShop(gameObject.transform.Find("Transition Speed/Slider").gameObject, gameObject.transform.Find("Transition Speed")).GetComponent<Slider>();

            tod_speed_value = gameObject.transform.Find("Transition Speed/Value").GetComponent<Text>();

            tod_slider.minValue = 1;
            tod_slider.maxValue = 5;

            auto_save_enabled = true;
            remember_enabled = true;

            button_back.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                SandboxManager.tod_panel.SetActive(true);
                gameObject.SetActive(false);
            }));

            button_folder.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                Application.OpenURL(Plugin.sandbox_folder_path);
            }));

            button_reload.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                SandboxManager.tod_grid_index = 0;
                SandboxManager.UpdateGrid();
                SandboxManager.ReloadSandboxes();
            }));

            button_remember.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                remember_enabled = !remember_enabled;
                Plugin.remeber_last_sandbox.SetSerializedValue(remember_enabled.ToString());
                button_remember_img.SetActive(remember_enabled);
            }));

            button_auto_save.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                auto_save_enabled = !auto_save_enabled;
                Plugin.auto_save_sandbox.SetSerializedValue(auto_save_enabled.ToString());
                button_auto_save_img.SetActive(auto_save_enabled);
            }));


            tod_slider.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                tod_speed_value.text = tod_slider.value.ToString("0.00");
                SandboxManager.global_tod_speed = tod_slider.value;
                Plugin.tod_speed.SetSerializedValue(tod_slider.value.ToString("0.00"));
                SandboxManager.UpdateTODs();
            }));
        }
    }
}

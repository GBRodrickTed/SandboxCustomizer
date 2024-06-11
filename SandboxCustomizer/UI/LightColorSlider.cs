using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using static UnityEngine.RemoteConfigSettingsHelper;

namespace SandboxCustomizer.UI
{
    public class LightColorSlider
    {
        public GameObject gameObject;
        public Slider slider_r;
        public Slider slider_g;
        public Slider slider_b;
        public Slider slider_mag;
        public Image color_img;
        public Image color_glow_img;
        public Color color;
        public Color slider_value;
        public UnityEvent on_value_change_event;
        public void Create(Transform transform)
        {
            //Debug.Log("Oh no here we go again");
            color = Color.white;
            slider_value = Color.white;
            gameObject = GameObject.Instantiate<GameObject>(AssetHandler.light_color_slider, transform);
            //Debug.Log("im about to go in im about to begin");
            slider_r = Utils.ReplaceSliderForShop(gameObject.transform.Find("Red").gameObject, gameObject.transform).GetComponent<Slider>();
            slider_g = Utils.ReplaceSliderForShop(gameObject.transform.Find("Green").gameObject, gameObject.transform).GetComponent<Slider>();
            slider_b = Utils.ReplaceSliderForShop(gameObject.transform.Find("Blue").gameObject, gameObject.transform).GetComponent<Slider>();
            slider_mag = Utils.ReplaceSliderForShop(gameObject.transform.Find("Magnitude").gameObject, gameObject.transform).GetComponent<Slider>();
            color_img = gameObject.transform.Find("Color").GetComponent<Image>();
            color_glow_img = gameObject.transform.Find("ColorGlow").GetComponent<Image>();
            UpdateColor();
            on_value_change_event = new UnityEvent();
            
            slider_r.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));
            slider_g.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));
            slider_b.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));
            slider_mag.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));
        }
        public void Destroy()
        {
            slider_r = null;
            slider_g = null;
            slider_b = null;
            slider_mag = null;
            color_img = null;
            color_glow_img = null;
            color = Color.white;
            slider_value = Color.white;
            on_value_change_event = null;
            Transform.Destroy(gameObject);
        }
        public void UpdateColor()
        {
            float alpha = Mathf.Max(slider_r.value, Math.Max(slider_g.value, slider_b.value));
            float alpha_inv = 1 / Mathf.Clamp(alpha, 0, 1);

            color.r = slider_r.value * (slider_mag.value + 1);
            color.g = slider_g.value * (slider_mag.value + 1);
            color.b = slider_b.value * (slider_mag.value + 1);
            color.a = (slider_mag.value + 1);

            Color img_color = Color.clear;
            Color img_color_glow = Color.clear;

            img_color.r = (slider_r.value * alpha_inv);
            img_color.g = (slider_g.value * alpha_inv);
            img_color.b = (slider_b.value * alpha_inv);
            img_color.a = alpha * (slider_mag.value + 1);

            img_color_glow = img_color;
            img_color_glow.a = Mathf.Clamp01((alpha * (slider_mag.value + 1)) - 1);

            color_img.color = img_color;
            color_glow_img.color = img_color_glow;
        }

        public void SetColor(Color col)
        {
            float alpha = Mathf.Max(1, col.a);
            slider_r.value = col.r / (alpha);
            slider_g.value = col.g / (alpha);
            slider_b.value = col.b / (alpha);
            slider_mag.value = col.a - 1;
            UpdateColor();
        }
    }
}

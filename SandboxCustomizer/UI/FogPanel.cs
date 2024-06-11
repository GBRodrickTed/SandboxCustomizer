using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SandboxCustomizer.UI
{
    public class FogPanel
    {
        public GameObject gameObject;
        public GameObject button_back;
        public GameObject button_fog_enable;
        public GameObject fog_enable_img;
        public bool fog_enable;

        public Image fog_img;
        public Color fog_color;

        public Slider fog_r;
        public Slider fog_g;
        public Slider fog_b;

        public Slider fog_start;
        public Slider fog_end;

        public Text fog_start_value;
        public Text fog_end_value;

        public UnityEvent on_value_change_event;

        public void Create(Transform transform)
        {
            gameObject = GameObject.Instantiate(AssetHandler.fog_panel, transform);
            button_back = Utils.ReplaceButtonForShop(gameObject.transform.Find("button back").gameObject, gameObject.transform);
            button_fog_enable = Utils.ReplaceButtonForShop(gameObject.transform.Find("fog enable button").gameObject, gameObject.transform);
            fog_enable_img = gameObject.transform.Find("fog enable button/Image(Clone)").gameObject;

            fog_enable = true;

            fog_img = gameObject.transform.Find("ColorSlider/Color").GetComponent<Image>();
            fog_r = Utils.ReplaceSliderForShop(gameObject.transform.Find("ColorSlider/Red").gameObject, gameObject.transform.Find("ColorSlider")).GetComponent<Slider>();
            fog_g = Utils.ReplaceSliderForShop(gameObject.transform.Find("ColorSlider/Green").gameObject, gameObject.transform.Find("ColorSlider")).GetComponent<Slider>();
            fog_b = Utils.ReplaceSliderForShop(gameObject.transform.Find("ColorSlider/Blue").gameObject, gameObject.transform.Find("ColorSlider")).GetComponent<Slider>();

            fog_start = Utils.ReplaceSliderForShop(gameObject.transform.Find("fog start/FogSlider").gameObject, gameObject.transform.Find("fog start")).GetComponent<Slider>();
            fog_end = Utils.ReplaceSliderForShop(gameObject.transform.Find("fog end/FogSlider").gameObject, gameObject.transform.Find("fog end")).GetComponent<Slider>();

            fog_start_value = gameObject.transform.Find("fog start/fog value").GetComponent<Text>();
            fog_end_value = gameObject.transform.Find("fog end/fog value").GetComponent<Text>();

            fog_start.maxValue = 1000;
            fog_start.minValue = -1000;
            fog_start.wholeNumbers = true;
            fog_end.maxValue = 1000;
            fog_end.minValue = -1000;
            fog_end.wholeNumbers = true;

            fog_color = Color.black;

            on_value_change_event = new UnityEvent();
            UpdateColor();

            button_back.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                gameObject.SetActive(false);
            }));

            button_fog_enable.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                fog_enable = !fog_enable;
                on_value_change_event.Invoke();
            }));

            fog_r.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));

            fog_g.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));

            fog_b.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                UpdateColor();
                on_value_change_event.Invoke();
            }));

            fog_start.onValueChanged.AddListener(new UnityAction<float>((call) => {
                fog_start_value.text = fog_start.value.ToString();
                on_value_change_event.Invoke();
            }));

            fog_end.onValueChanged.AddListener(new UnityAction<float>((call) => {
                fog_end_value.text = fog_end.value.ToString();
                on_value_change_event.Invoke();
            }));
        }
        public void UpdateColor()
        {
            fog_color.r = fog_r.value;
            fog_color.g = fog_g.value;
            fog_color.b = fog_b.value;
            fog_color.a = 1;

            fog_img.color = fog_color;
        }

        public void SetColor(Color col)
        {
            fog_r.value = col.r;
            fog_g.value = col.g;
            fog_b.value = col.b;
            UpdateColor();
        }
    }
}

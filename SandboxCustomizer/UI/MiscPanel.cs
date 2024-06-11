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
    public class MiscPanel
    {
        public GameObject gameObject;
        public GameObject button_back;

        public Slider godray_alpha;
        public Text godray_alpha_value;
        public Image godray_img;

        public UnityEvent on_value_change_event;

        public void Create(Transform transform)
        {
            gameObject = GameObject.Instantiate(AssetHandler.misc_panel, transform);
            button_back = Utils.ReplaceButtonForShop(gameObject.transform.Find("button back").gameObject, gameObject.transform);
            godray_img = gameObject.transform.Find("Color").GetComponent<Image>();
            godray_alpha = Utils.ReplaceSliderForShop(gameObject.transform.Find("godray intensity/GodraySlider").gameObject, gameObject.transform.Find("godray intensity")).GetComponent<Slider>();
            godray_alpha_value = gameObject.transform.Find("godray intensity/godray value").GetComponent<Text>();

            godray_alpha.minValue = 0;
            godray_alpha.maxValue = 0.15f;

            on_value_change_event = new UnityEvent();
            button_back.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                gameObject.SetActive(false);
            }));

            godray_alpha.onValueChanged.AddListener(new UnityAction<float>((call) =>
            {
                godray_alpha_value.text = (godray_alpha.value * (1/godray_alpha.maxValue)).ToString("0.000");
                on_value_change_event.Invoke();
            }));
        }
    }
}

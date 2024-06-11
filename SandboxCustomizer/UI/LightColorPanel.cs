using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SandboxCustomizer.UI
{
    public class LightColorPanel
    {
        public GameObject gameObject;
        public GameObject button_left;
        public GameObject button_right;
        public GameObject button_back;

        GameObject slider_container;
        int container_index = 0;
        const int slider_container_max = 6;
        public List<LightColorSlider> color_sliders = new List<LightColorSlider>();
        public List<GameObject> slider_containers = new List<GameObject>();

        public void Create(Transform transform)
        {
            //
            gameObject = GameObject.Instantiate<GameObject>(AssetHandler.light_panel, transform);
            button_left = Utils.ReplaceButtonForShop(gameObject.transform.Find("button left").gameObject, gameObject.transform);
            button_right = Utils.ReplaceButtonForShop(gameObject.transform.Find("button right").gameObject, gameObject.transform);
            button_back = Utils.ReplaceButtonForShop(gameObject.transform.Find("button back").gameObject, gameObject.transform);
            //Debug.Log("im about to go in im about to begin panel");

            button_left.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                if (container_index != 0)
                {
                    container_index--;
                }
                UpdateGrid();
            }));

            button_right.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                if (container_index != slider_containers.Count - 1)
                {
                    container_index++;
                }
                UpdateGrid();
            }));

            button_back.GetComponent<PointerAdder>().on_pressed_actions.Add(new UnityAction(() =>
            {
                gameObject.SetActive(false);
            }));

            slider_container = gameObject.transform.Find("SliderContainer").gameObject;
            slider_containers.Add(GameObject.Instantiate<GameObject>(slider_container, gameObject.transform));
        }

        public void AddColorSlider(LightColorSlider slider)
        {
            color_sliders.Add(slider);
            if (6 * slider_containers.Count < color_sliders.Count)
            {
                slider_containers.Add(GameObject.Instantiate<GameObject>(slider_container, gameObject.transform));
            }
            slider.Create(slider_containers[slider_containers.Count - 1].transform);
            UpdateGrid();
        }

        public void ClearAllSliders()
        {
            foreach (LightColorSlider slider in color_sliders)
            {
                slider.Destroy();
            }
            color_sliders.Clear();
            foreach (GameObject obj in slider_containers)
            {
                Transform.Destroy(obj);
            }
            slider_containers.Clear();
            slider_containers.Add(GameObject.Instantiate<GameObject>(slider_container, gameObject.transform));
            container_index = 0;
        }

        public void Clear()
        {
            Transform.Destroy(gameObject);
        }

        public void UpdateGrid()
        {
            foreach (GameObject container in slider_containers)
            {
                container.SetActive(false);
            }
            slider_containers[container_index].SetActive(true);
            button_left.SetActive(true);
            button_right.SetActive(true);

            if (container_index == slider_containers.Count - 1)
            {
                button_right.SetActive(false);
            }
            if (container_index == 0)
            {
                button_left.SetActive(false);
            }
        }
    }
}

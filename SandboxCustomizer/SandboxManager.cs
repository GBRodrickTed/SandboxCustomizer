using GameConsole.Commands;
using SandboxCustomizer.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.ColorPicker;
using UnityEngine.UIElements;
using SandboxCustomizer.UI;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Collections;

namespace SandboxCustomizer
{
    public static class SandboxManager
    {
        
        private static GameObject tod_grid;
        private static GameObject tod_changer;
        public static GameObject tod_panel;
        public static GameObject blocker;
        public static GameObject unblocker;
        public static float global_tod_speed = 1;
        public static List<GameObject> tod_grid_list = new List<GameObject>();
        public static int tod_grid_index = 0;
        private static List<GameObject> tod_buttons = new List<GameObject>();
        private static int tod_button_total = 8;
        public static List<CustomSandbox> sandboxs = new List<CustomSandbox>();
        public static List<string> tex_path = new List<string>();

        public static GameObject base_tod_button;
        public static EditPanel edit_panel = new EditPanel();
        public static SettingsPanel settings_panel = new SettingsPanel();

        public static CustomSandbox sandbox_current = null;
        public static string main_sandbox = null;
        public static LightColorSlider cob = new LightColorSlider();

        private static bool is_setup = false;

        static int todb_count = 6;
        public static GameObject[] shop_button_mover = new GameObject[todb_count];

        public static UnityEvent on_blocked_event = new UnityEvent();
        public static UnityEvent on_unblock_event = new UnityEvent();

        public static CustomSandbox dead_sandbox = new CustomSandbox();

        public static Dictionary<string, GameObject> vanilla_activators = new Dictionary<string, GameObject>();
        public static Dictionary<GameObject, string> activators_vanilla = new Dictionary<GameObject, string>();

        //public static UnityAction u_action;

        //TODO: kinda sucks
        enum TODB : int
        {
            Left = 0,
            Right = 1,
            Add = 2,
            Delete = 3,
            Edit = 4,
            Settings = 5
        }
        public static void Setup()
        {
            if (!is_setup)
            {
                Utils.InitUIObjets();

                is_setup = true;

                tod_buttons = new List<GameObject>();
                sandboxs = new List<CustomSandbox>();

                on_blocked_event = new UnityEvent();
                on_unblock_event = new UnityEvent();

                dead_sandbox = new CustomSandbox();
                //MakeDeadSandbox();

                tod_changer = GameObject.Find("Sandbox Shop/Canvas/Border/TOD Changer");
                tod_panel = GameObject.Find("Sandbox Shop/Canvas/Border/TOD Changer/Panel");
                tod_grid = GameObject.Find("Sandbox Shop/Canvas/Border/TOD Changer/Panel/TOD Grid");

                blocker = GameObject.Find("Sandbox Shop/Canvas/Border/TOD Changer/Panel/Blocker");
                unblocker = blocker.GetComponent<ObjectActivator>().events.toActivateObjects[0];
                unblocker.AddComponent<BetterUnblocker>();
                //unblocker.GetComponent<>

                vanilla_activators.Add("Dawn", GameObject.Find("Activators").transform.Find("Dawn Activator").gameObject);
                vanilla_activators.Add("Day", GameObject.Find("Activators").transform.Find("Day Activator").gameObject);
                vanilla_activators.Add("Dusk", GameObject.Find("Activators").transform.Find("Dusk Activator").gameObject);
                vanilla_activators.Add("Night", GameObject.Find("Activators").transform.Find("Night Activator").gameObject);
                vanilla_activators.Add("Night Darker", GameObject.Find("Activators").transform.Find("Night Darker Activator").gameObject);
                vanilla_activators.Add("Lust", GameObject.Find("Activators").transform.Find("Lust Activator").gameObject);
                vanilla_activators.Add("Greed", GameObject.Find("Activators").transform.Find("Greed Activator").gameObject);
                vanilla_activators.Add("Foggy", GameObject.Find("Activators").transform.Find("Foggy Activator").gameObject);

                foreach(var va in vanilla_activators)
                {
                    activators_vanilla.Add(va.Value, va.Key);
                }

                base_tod_button = GameObject.Instantiate(tod_grid.transform.Find("Day"), tod_grid.transform).gameObject;
                base_tod_button.name = "Base (Button)";
                base_tod_button.SetActive(false);

                blocker.GetComponent<ObjectActivator>().events.onActivate.AddListener(onBlocked);
                unblocker.GetComponent<ObjectActivator>().events.onActivate.AddListener(onUnblock);
                
                //setup tod grid scroller
                shop_button_mover[(int)TODB.Left] = Utils.CloneButtonForShop(AssetHandler.LoadAsset<GameObject>("button left"), tod_panel.transform);
                shop_button_mover[(int)TODB.Right] = Utils.CloneButtonForShop(AssetHandler.LoadAsset<GameObject>("button right"), tod_panel.transform);
                shop_button_mover[(int)TODB.Add] = Utils.CloneButtonForShop(AssetHandler.LoadAsset<GameObject>("button add"), tod_panel.transform);
                shop_button_mover[(int)TODB.Delete] = Utils.CloneButtonForShop(AssetHandler.LoadAsset<GameObject>("button delete"), tod_panel.transform);
                shop_button_mover[(int)TODB.Edit] = Utils.CloneButtonForShop(AssetHandler.LoadAsset<GameObject>("button edit"), tod_panel.transform);
                shop_button_mover[(int)TODB.Settings] = Utils.CloneButtonForShop(AssetHandler.LoadAsset<GameObject>("button settings"), tod_panel.transform);
                edit_panel.Create(tod_changer.transform);
                edit_panel.gameObject.SetActive(false);

                settings_panel.Create(tod_changer.transform);
                settings_panel.gameObject.SetActive(false);

                tod_grid_list.Add(tod_grid);
                UpdateGrid();
                UpdateTODs();

                shop_button_mover[(int)TODB.Left].GetComponent<PointerAdder>().on_pressed_actions.Add(
                    (new UnityAction(() =>
                    {
                        if (tod_grid_index > 0)
                        {
                            tod_grid_index--;
                        }
                        UpdateGrid();
                        Debug.Log(tod_grid_index);
                    }))
                );

                shop_button_mover[(int)TODB.Right].GetComponent<PointerAdder>().on_pressed_actions.Add(
                    (new UnityAction(() =>
                    {
                        if (tod_grid_index < tod_grid_list.Count - 1)
                        {
                            tod_grid_index++;
                        }
                        UpdateGrid();
                        Debug.Log(tod_grid_index);
                    }))
                );

                shop_button_mover[(int)TODB.Add].GetComponent<PointerAdder>().on_pressed_actions.Add(
                    (new UnityAction(() =>
                    {
                        CustomSandbox sandbox = new CustomSandbox();
                        //sandbox.indoor_color = new Color(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));
                        //sandbox.outdoor_color = new Color(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));
                        //sandbox.fog_color = new Color(UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f), UnityEngine.Random.Range(0, 1.0f));
                        AddSandbox(sandbox);
                        if (tex_path.Count > 0)
                        {
                            string path = tex_path[UnityEngine.Random.Range(0, tex_path.Count)];
                            sandbox.LoadSkyboxFromPath(path);//
                            sandbox.info.skybox.name = path.Replace(Plugin.skybox_folder_path + "\\", "");
                            sandbox.info.skybox.name = sandbox.info.skybox.name.Replace("PutSkyboxHere\\", "");
                            CSBUtils.SetStringField("Skybox", sandbox.info.skybox.name, 0, sandbox.info.path);
                        }
                        UpdateGrid();
                        Debug.Log("Added sandbox");
                    }))
                );

                shop_button_mover[(int)TODB.Delete].GetComponent<PointerAdder>().on_pressed_actions.Add(
                    (new UnityAction(() =>
                    {
                        DeleteSandbox(sandbox_current);
                        UpdateGrid();
                        Debug.Log("Removed sandbox");
                    }))
                );

                shop_button_mover[(int)TODB.Edit].GetComponent<PointerAdder>().on_pressed_actions.Add(
                    (new UnityAction(() =>
                    {
                        edit_panel.UpdateName();
                        edit_panel.gameObject.SetActive(true);
                        tod_panel.SetActive(false);
                    }))
                );

                shop_button_mover[(int)TODB.Settings].GetComponent<PointerAdder>().on_pressed_actions.Add(
                    (new UnityAction(() =>
                    {
                        settings_panel.remember_enabled = Plugin.remeber_last_sandbox.Value;
                        settings_panel.auto_save_enabled = Plugin.auto_save_sandbox.Value;
                        settings_panel.tod_slider.value = global_tod_speed;

                        //profound laziness
                        settings_panel.tod_speed_value.text = settings_panel.tod_slider.value.ToString("0.00");
                        settings_panel.button_auto_save_img.SetActive(settings_panel.auto_save_enabled);
                        settings_panel.button_remember_img.SetActive(settings_panel.remember_enabled);

                        settings_panel.gameObject.SetActive(true);
                        tod_panel.SetActive(false);
                    }))
                );
            }
        }
        public static void AddSandbox(CustomSandbox sandbox)
        {
            tod_button_total++;
            // Duplicates the og tod grid and removes buttons
            if (8 * tod_grid_list.Count() < tod_button_total)
            {
                GameObject new_grid = GameObject.Instantiate(tod_grid, tod_grid.transform.parent);
                int child_count = new_grid.transform.childCount;
                for (int i = 0; i < child_count; i++)
                {
                   GameObject.Destroy(new_grid.transform.GetChild(i).gameObject);
                }
                tod_grid_list.Add(new_grid);
                UpdateGrid();
            }
            sandbox.AddToScene();
            sandbox.tod_changer.speedMultiplier = global_tod_speed;
            sandboxs.Add(sandbox);
            GameObject button = GameObject.Instantiate(base_tod_button, tod_grid_list[tod_grid_list.Count() - 1].transform).gameObject;
            button.SetActive(true);
            button.name = "Custom (Button)";
            button.GetComponent<ShopButton>().toActivate[0] = sandbox.custom_activator;
            tod_buttons.Add(button);
            SandboxManager.UpdateDisableList();
            SandboxInfo.SaveInfo(sandbox.info);
            if (!Plugin.sbs.Contains(sandbox))
            {
                Plugin.sbs.Add(sandbox);
            }
        }
        public static void MakeDeadSandbox()
        {
            dead_sandbox.AddToScene();
            for (int i = 0; i < dead_sandbox.info.outdoor_lights.Count; i++)
            {
                dead_sandbox.info.outdoor_lights[i].color = Color.black;
            }

            for (int i = 0; i < dead_sandbox.info.indoor_lights.Count; i++)
            {
                dead_sandbox.info.indoor_lights[i].color = Color.black;
            }
            dead_sandbox.skybox_mat.SetColor("_Tint", Color.black);
            SandboxManager.dead_sandbox.UpdateEffects();
            SandboxManager.UpdateDisableList();
        }
        public static CustomSandbox backup_sandbox = null;
        public static void DeleteSandbox(CustomSandbox sandbox, bool delete_save = true)
        {
            if (!sandboxs.Contains(sandbox))
            {
                Debug.Log("Sandbox being deleted is not in list");
                return; //Nothing to delete
            }
            if (File.Exists(sandbox.info.path) && delete_save)
            {
                File.Delete(sandbox.info.path);
            }
            
            int index = sandboxs.IndexOf(sandbox);
            bool we_know = false;
            if (sandbox == sandbox_current)
            {
                we_know = true;
            }
            if (we_know == true)
            {
                if (index < sandboxs.Count - 1)
                {
                    //Debug.Log("found to the right");
                    ForceSandbox(sandboxs[index + 1]);
                    //Debug.Log("found to the right");
                }
                else if (index > 0)
                {
                    //Debug.Log("found to the left");
                    ForceSandbox(sandboxs[index - 1]);
                    //Debug.Log("found to the left");
                }
                else
                {
                    //Debug.Log("hollow");
                    GameObject.Find("Activators").transform.Find("Day Activator").gameObject.SetActive(true);
                    blocker.SetActive(true);
                    sandbox_current = null;
                    //Debug.Log("hollow");
                }
            }

            /*Debug.Log("before-----------------------------");
            Debug.Log("tod button total: " + tod_button_total);
            Debug.Log("sandbox index: " + sandboxs.IndexOf(sandbox));
            Debug.Log("sandboxs count: " + sandboxs.Count);
            Debug.Log("-----------------------------------");*/
            //int index = sandboxs.IndexOf(sandbox);
            int grid_index = ((int)(index / 8)) + 1;

            tod_button_total--;
            GameObject.Destroy(tod_buttons[index]);
            tod_buttons.RemoveAt(index);
            //checks if there is a grid ahead of the grid the sandbox was in
            if (grid_index < (tod_grid_list.Count - 1))
            {
                //GameObject button = tod_grid_list[tod_grid_list.Count - 1].transform.GetChild(0).gameObject;
                for (int i = grid_index; i < (tod_grid_list.Count - 1); i++)
                {
                    GameObject button = tod_grid_list[i + 1].transform.GetChild(0).gameObject;
                    button.transform.SetParent(tod_grid_list[i].transform);
                }

                //button.SetActive(true);
                if (tod_grid_list[tod_grid_list.Count - 1].transform.childCount == 0)
                {
                    GameObject.Destroy(tod_grid_list[tod_grid_list.Count - 1]);
                    tod_grid_list.RemoveAt(tod_grid_list.Count - 1);
                }
            }
            else
            {
                //because of some phantom child nonsense i gotta do this
                //TODO: perferably not this
                if (tod_button_total % 8 == 0)
                {
                    GameObject.Destroy(tod_grid_list[grid_index]);
                    tod_grid_list.RemoveAt(grid_index);
                    if (grid_index == tod_grid_index)
                    {
                        tod_grid_index--;
                    }
                }
            }
            SandboxManager.UpdateGrid();
            SandboxManager.UpdateDisableList();

            /*Debug.Log("after------------------------------");
            Debug.Log("tod button total: " + tod_button_total);
            Debug.Log("sandbox index: " + sandboxs.IndexOf(sandbox));
            Debug.Log("sandboxs count: " + sandboxs.Count);
            Debug.Log("-----------------------------------");*/

            if (Plugin.sbs.Contains(sandbox))
            {
                Plugin.sbs.Remove(sandbox);
            }
            if (we_know)
            {
                //TODO: Has a non random chance to get stuck on daytime transition
                //backup_sandbox = sandbox;
                //on_unblock_event.AddListener(DeleteCurrentOnUnblock);
                sandbox.RemoveFromScene();
                sandboxs.RemoveAt(index);
            } else
            {
                sandbox.RemoveFromScene();
                sandboxs.RemoveAt(index);
            }
            //on_unblock_event.AddListener(smile);

        }

        //TODO: a little scary
        //TODO: Absolutly horrifying
        /*public static void DeleteCurrentOnUnblock()
        {
            Debug.Log("InTheZone");
            backup_sandbox.RemoveFromScene();
            sandboxs.RemoveAt(sandboxs.IndexOf(backup_sandbox));
            on_unblock_event.RemoveListener(DeleteCurrentOnUnblock);
        }*/
        public static void RefreshSkybox()
        {
            if (backup_sandbox != null)
            {
                ForceSandbox(backup_sandbox);
                sandbox_current = backup_sandbox;
            }
            on_unblock_event.RemoveListener(SandboxManager.RefreshSkybox);
        }

        public static void smile()
        {
            Debug.Log("I LIED >:(");
            on_unblock_event.RemoveListener(smile);
        }

        public static void ForceSandbox(CustomSandbox sandbox)
        {
            if (sandboxs.Contains(sandbox))
            {
                int index = sandboxs.IndexOf(sandbox);
                sandbox.custom_activator.SetActive(true);
                blocker.SetActive(true);
            }
        }

        public static void HardForceSandbox(CustomSandbox sandbox)
        {
            if (sandboxs.Contains(sandbox))
            {
                int index = sandboxs.IndexOf(sandbox);
                sandbox.tod_changer.speedMultiplier = 10000000;
                sandbox.custom_activator.SetActive(true);
                sandbox_current = sandbox; // TODO: I think it goes so fast that it skips the sandbox finding thing
                on_unblock_event.AddListener(delegate { RevertToDSpeedOnUnblock(sandbox.tod_changer); });
                blocker.SetActive(true);
            }
        }
        public static GameObject target_activator;
        public static TimeOfDayChanger target_base_tod;
        public static void ForceBaseSandbox(string name)
        {
            target_activator = null;
            vanilla_activators.TryGetValue(name, out target_activator);

            if (target_activator != null)
            {
                target_activator.SetActive(true);
                blocker.SetActive(true);
            }
            else
            {
                Debug.Log("You dumb and reactive frendo");
            }
        }
        public static void HardForceBaseSandbox(string name)
        {
            target_activator = null;
            vanilla_activators.TryGetValue(name, out target_activator);

            if (target_activator != null)
            {
                target_base_tod = target_activator.transform.GetChild(0).GetComponent<TimeOfDayChanger>();
                target_base_tod.speedMultiplier = 10000000;
                target_activator.SetActive(true);
                on_unblock_event.AddListener(delegate { RevertToDSpeedOnUnblock(target_base_tod); });
                blocker.SetActive(true);
            } else
            {
                Debug.Log("You dumb and reactive frendo");
            }
        }

        public static void RevertToDSpeedOnUnblock(TimeOfDayChanger tod)
        {
            if (tod != null) tod.speedMultiplier = global_tod_speed;
            on_unblock_event.RemoveListener(delegate { RevertToDSpeedOnUnblock(tod); });
        }
        
        

        public static void UpdateGrid()
        {
            foreach (GameObject grid in tod_grid_list)
            {
                grid.SetActive(false);
            }
            tod_grid_list[tod_grid_index].SetActive(true);

            if (tod_grid_index == 0)
            {
                shop_button_mover[(int)TODB.Left].SetActive(false);
            }
            else
            {
                shop_button_mover[(int)TODB.Left].SetActive(true);
            }
            if (tod_grid_index == tod_grid_list.Count - 1)
            {
                shop_button_mover[(int)TODB.Right].SetActive(false);
            }
            else
            {
                shop_button_mover[(int)TODB.Right].SetActive(true);
            }

            if (tod_buttons.Count == 0 || sandbox_current == null)
            {
                shop_button_mover[(int)TODB.Delete].SetActive(false);
                shop_button_mover[(int)TODB.Edit].SetActive(false);
            }
            else
            {
                shop_button_mover[(int)TODB.Delete].SetActive(true);
                shop_button_mover[(int)TODB.Edit].SetActive(true);
            }
            shop_button_mover[(int)TODB.Settings].SetActive(true);
        }

        // Updates the sandboxs disable events and old lights
        public static void UpdateDisableList()
        {
            Transform activators = GameObject.Find("Activators").transform;
            Transform indoors = GameObject.Find("IndoorsLighting").transform;
            Transform outdoors = GameObject.Find("OutdoorsLighting").transform;
            List<GameObject> deactive_canadates_obj_act = new List<GameObject>();
            List<GameObject> deactive_canadates_tod = new List<GameObject>();
            for (int i = 0; i < indoors.childCount; i++)
            {
                deactive_canadates_obj_act.Add(indoors.GetChild(i).gameObject);
            }
            for (int i = 0; i < activators.childCount; i++)
            {
                deactive_canadates_obj_act.Add(activators.GetChild(i).gameObject);
            }
            //TODO: There must be a better way
            deactive_canadates_obj_act.Add(activators.Find("Dawn Activator").GetComponent<ObjectActivator>().events.toActivateObjects[2]);
            deactive_canadates_obj_act.Add(activators.Find("Night Activator").GetComponent<ObjectActivator>().events.toActivateObjects[2]);
            deactive_canadates_obj_act.Add(activators.Find("Foggy Activator").GetComponent<ObjectActivator>().events.toActivateObjects[2]);

            for (int i = 0; i < outdoors.childCount; i++)
            {
                deactive_canadates_tod.Add(outdoors.GetChild(i).gameObject);
            }

            for (int i = 0; i < activators.childCount; i++)
            {
                List<GameObject> deactives_obj_act = deactive_canadates_obj_act.ToList<GameObject>();
                List<GameObject> to_remove_obj_act = new List<GameObject>();
                List<GameObject> to_keep_obj_act = activators.GetChild(i).GetComponent<ObjectActivator>().events.toActivateObjects.ToList<GameObject>();
                to_keep_obj_act.Add(activators.GetChild(i).gameObject);

                foreach (GameObject bad in deactives_obj_act)
                {
                    if (bad.name == "IndoorsGarage" || bad.name == "IndoorsBuilding")
                    {
                        to_remove_obj_act.Add(bad);
                    }
                    foreach (GameObject good in to_keep_obj_act)
                    {
                        if (bad == good)
                        {
                            to_remove_obj_act.Add(bad);
                        }
                    }
                }

                foreach (GameObject good in to_remove_obj_act)
                {
                    deactives_obj_act.Remove(good);
                }

                activators.GetChild(i).GetComponent<ObjectActivator>().events.toDisActivateObjects = deactives_obj_act.ToArray();

                List<GameObject> deactives_tod = deactive_canadates_tod.ToList<GameObject>();
                List<GameObject> to_remove_tod = new List<GameObject>();
                List<GameObject> to_keep_tod = new List<GameObject>();

                foreach (GameObject bad in deactives_tod)
                {
                    foreach (GameObject good in to_keep_obj_act)
                    {
                        //Debug.Log(bad.transform.parent.gameObject.name);
                        if (bad == good)
                        {
                            to_remove_tod.Add(bad);
                        }
                    }
                }

                foreach (GameObject good in to_remove_tod)
                {
                    deactives_tod.Remove(good);
                }

                List<Light> deactives_tod_lights = new List<Light>();
                foreach (GameObject obj in deactives_tod)
                {
                    for (int j = 0; j < obj.transform.childCount; j++)
                    {
                        if (obj.transform.GetChild(j).name.Contains("Directional Light"))
                        {
                            deactives_tod_lights.Add(obj.transform.GetChild(j).GetComponent<Light>());
                        }
                    }
                }
                activators.GetChild(i).Find("TOD Changer").gameObject.GetComponent<TimeOfDayChanger>().oldLights = deactives_tod_lights.ToArray();//deactives_tod_lights.ToArray();
            }
            UpdateTODs();
        }

        public static void UpdateTODs()
        {
            Transform activators = GameObject.Find("Activators").transform;
            for (int i = 0; i < activators.childCount; i++)
            {
                activators.GetChild(i).Find("TOD Changer").gameObject.GetComponent<TimeOfDayChanger>().speedMultiplier = global_tod_speed;
            }
        }

        public static void SaveSandboxes()
        {
            foreach (CustomSandbox sandbox in sandboxs)
            {
                SandboxInfo.SaveInfo(sandbox.info);
            }
        }

        public static void DeleteSandboxes()
        {
            if (sandbox_current != null)
            {
                GameObject.Find("Activators").transform.Find("Day Activator").gameObject.SetActive(true);
                blocker.SetActive(true);
                sandbox_current = null;
            }
            while (sandboxs.Count != 0)
            {
                DeleteSandbox(sandboxs[0]);
            }
        }
        static bool load_sandboxes = false;
        public static void ReloadSandboxes()
        {
            if (sandbox_current != null)
            {
                ForceBaseSandbox("Day");
                Plugin.load_on_start.SetSerializedValue("Day");
                sandbox_current = null; // yes this is nessisary
            }
            
            while (sandboxs.Count != 0)
            {
                DeleteSandbox(sandboxs[0], false);
            }

            if (!load_sandboxes)
            {
                on_unblock_event.AddListener(LoadSandboxes);
                blocker.SetActive(true);
                load_sandboxes = true;
            }
        }

        public static void LoadSandboxes()
        {
            foreach (string file in Directory.EnumerateFiles(Plugin.sandbox_folder_path))
            {
                if (string.Equals(".csb", Path.GetExtension(file), StringComparison.OrdinalIgnoreCase))
                {
                    CustomSandbox sb = new CustomSandbox();
                    sb.info = SandboxInfo.LoadInfo(file);
                    if (sb.info != null)
                    {
                        AddSandbox(sb);
                    }
                }
            }
            on_unblock_event.RemoveListener(LoadSandboxes);
            load_sandboxes = false;
        }

        public static void doAThing()
        {
            if (is_setup)
            {
                if (sandboxs != null)
                {
                    /*if (sandboxs.Count > 0)
                    {
                        special = sandboxs[0];
                        Debug.Log("ding");
                    }*/
                }
            }
        }

        public static void onBlocked()
        {
            foreach (GameObject grid in tod_grid_list)
            {
                grid.SetActive(false);
            }
            for (int i = 0; i < todb_count; i++)
            {
                shop_button_mover[i].SetActive(false);
            }
            on_blocked_event.Invoke();
        }

        public static void onUnblock()
        {
            for (int i = 0; i < todb_count; i++)
            {
                shop_button_mover[i].SetActive(true);
            }
            UpdateGrid();
            on_unblock_event.Invoke();
        }

        public static void Clear()
        {
            tod_button_total = 8;
            tod_grid = null;
            tod_grid_list = new List<GameObject>();
            tod_grid_index = 0;
            tod_changer = null;
            tod_buttons = new List<GameObject>();
            foreach (CustomSandbox sandbox in sandboxs)
            {
                sandbox.Clear();
            }
            sandboxs = new List<CustomSandbox>();
            sandbox_current = null;
            backup_sandbox = null;
            tod_panel = null;
            shop_button_mover = new GameObject[todb_count];
            on_unblock_event.RemoveAllListeners();
            on_blocked_event.RemoveAllListeners();
            vanilla_activators.Clear();
            activators_vanilla.Clear();
            load_sandboxes = false;
            is_setup = false;
        }
    }
}

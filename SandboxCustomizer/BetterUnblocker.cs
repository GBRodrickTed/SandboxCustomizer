using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.HID;

namespace SandboxCustomizer
{
    public class BetterUnblocker : MonoBehaviour
    {
        TimeOfDayChanger main_changer = null;
        UltrakillEvent events = null;
        public UnityEvent sanbox_switch_event = new UnityEvent();
        public void Start()
        {
            Invoke("OnEnable", 0.1f);
        }
        public void OnEnable()
        {
            ObjectActivator obj_act = null;
            if (gameObject.TryGetComponent<ObjectActivator>(out obj_act))
            {
                obj_act.CancelInvoke();
            }
            if (events == null)
            {
                events = obj_act.events;
            }
        }
        public void Update()
        {
            if (main_changer != null)
            {
                if (main_changer.allDone == true)
                {
                    main_changer = null;
                    //Debug.Log("Jeli beli gumi candi :<");
                    if (events != null)
                    {
                        events.Invoke();
                    }
                } else
                {
                    //Debug.Log("AHHH!");
                }
            } else
            {
                bool found_something = false;
                Transform activators = GameObject.Find("Activators").transform;
                for (int i = 0; i < activators.childCount; i++)
                {
                    if (!activators.GetChild(i).gameObject.activeSelf)
                    {
                        continue;
                    }
                    GameObject activator = activators.GetChild(i).gameObject;
                    TimeOfDayChanger tod_changer = null;
                    for (int j = 0; j < activator.transform.childCount; j++)
                    {
                        if (activator.transform.GetChild(j).TryGetComponent<TimeOfDayChanger>(out tod_changer))
                        {
                            if (tod_changer.allDone == true)
                            {
                                tod_changer = null;
                                continue;
                            }
                            else
                            {
                                main_changer = tod_changer;
                                bool is_custom_sandbox = false;
                                found_something = true;
                                //Debug.Log(activator.name);
                                foreach (CustomSandbox sandbox in SandboxManager.sandboxs)
                                {
                                    if (sandbox.tod_changer == tod_changer)
                                    {
                                        SandboxManager.sandbox_current = sandbox;
                                        if (sandbox.info.path != null)
                                        {
                                            Plugin.load_on_start.SetSerializedValue(sandbox.info.path.Replace(Plugin.sandbox_folder_path + "\\", ""));
                                        } else
                                        {
                                            Plugin.load_on_start.SetSerializedValue("Day");
                                        }
                                        is_custom_sandbox = true;
                                        break;
                                    }
                                }
                                if (!is_custom_sandbox)
                                {
                                    //Debug.Log("This sandbox is NOT custom");
                                    SandboxManager.sandbox_current = null;
                                    string thing;
                                    SandboxManager.activators_vanilla.TryGetValue(activator, out thing);
                                    if (thing == null)
                                    {
                                        Debug.Log("GUYS WE'RE LITERALLY GONNA #SAVETF2 THIS TIME GUYS!! #FIXTF2 GUYS!!!");
                                        SandboxManager.main_sandbox = null;
                                    }
                                    else
                                    {
                                        SandboxManager.main_sandbox = thing;
                                        if (thing != null)
                                        {
                                            Plugin.load_on_start.SetSerializedValue(thing);
                                        } else
                                        {
                                            Plugin.load_on_start.SetSerializedValue("Day");
                                        }
                                    }
                                }
                                sanbox_switch_event.Invoke();
                            }
                        }
                    }
                }
                if (found_something == false)
                {
                    events.Invoke();
                }
            }
        }
    }
}

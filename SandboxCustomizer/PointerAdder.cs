using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SandboxCustomizer
{
    public class PointerAdder : MonoBehaviour
    {
        ControllerPointer pointer;
        public List<UnityAction> on_pressed_actions = new List<UnityAction>();
        private void Awake()
        {
            if (!base.TryGetComponent<ControllerPointer>(out this.pointer))
            {
                this.pointer = base.gameObject.AddComponent<ControllerPointer>();
            }
            foreach (UnityAction action in on_pressed_actions)
            {
                this.pointer.OnPressed.AddListener(action);
            }
        }
    }
}

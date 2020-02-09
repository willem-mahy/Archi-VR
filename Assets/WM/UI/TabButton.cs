using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    public class TabButton : MonoBehaviour
    {
        public TabPanel TabPanel;
        public int TabIndex;

        // Start is called before the first frame update
        void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void OnClick()
        {
            if (TabPanel == null)
            {
                return;
            }

            TabPanel.Activate(TabIndex);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WM.UI
{
    public class TabPanel : MonoBehaviour
    {
        #region Public API

        // Start is called before the first frame update
        void Start()
        {
            {
                for (int pageIndex = 0; pageIndex < _pagesPanel.transform.childCount; ++pageIndex)
                {
                    _pageGameObjects.Add(_pagesPanel.transform.GetChild(pageIndex).gameObject);
                }
            }

            {
                int numButtons = _pageGameObjects.Count;
                float spacingWidth = 10;
                float availableWidth = _tabsPanel.GetComponent<RectTransform>().rect.width;
                float totalSpacingWidth = (numButtons + 1) * spacingWidth;
                float totalButtonWidth = availableWidth - totalSpacingWidth;
                float buttonWidth = totalButtonWidth / numButtons;
                
                float x = 0;
                int pageIndex = 0;
                foreach (var page in _pageGameObjects)
                {
                    x += spacingWidth;

                    var button = CreateTabButton(x, buttonWidth);
                    button.gameObject.name = "TabButton_" + _pageGameObjects[pageIndex].name;
                    button.transform.Find("Text").GetComponent<Text>().text = _pageGameObjects[pageIndex].name;
                    button.gameObject.SetActive(true);

                    var tabButton = button.gameObject.GetComponent<TabButton>();
                    tabButton.TabPanel = this;
                    tabButton.TabIndex = pageIndex;

                    _tabButtons.Add(button);

                    x += buttonWidth;
                    ++pageIndex;
                }
            }

            Activate(0);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public int NumTabs
        {
            get { return _tabButtons.Count; }
        }

        public int ActiveTabIndex
        {
            get;
            private set;
        } = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabIndex"></param>
        public void Activate(int tabIndex)
        {
            ActiveTabIndex = tabIndex;

            var inactiveNormalColor = _tabButtonPrefab.GetComponent<Button>().colors.normalColor;
            var activeNormalColor = new Color(0, 1, 0);

            for (int i = 0; i < NumTabs; ++i)
            {
                var active = (i == ActiveTabIndex);

                // Update button
                var colors = _tabButtons[i].colors;
                colors.normalColor = active ? activeNormalColor : inactiveNormalColor;
                _tabButtons[i].colors = colors;


                _pageGameObjects[i].SetActive(active);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ActivatePrevious()
        {
            if (NumTabs == 0)
            {
                return;
            }
            
            var newActiveTabIndex = UtilIterate.MakeCycle(ActiveTabIndex -1, 0, NumTabs);

            Activate(newActiveTabIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ActivateNext()
        {
            if (NumTabs == 0)
            {
                return;
            }

            var newActiveTabIndex = UtilIterate.MakeCycle(ActiveTabIndex + 1, 0, NumTabs);

            Activate(newActiveTabIndex);
        }


        #endregion

        #region Private API

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cornerTopRight"></param>
        /// <param name="cornerBottomLeft"></param>
        /// <returns></returns>
        public Button CreateTabButton(float xOffset, float width)
        {
            var button = Instantiate(_tabButtonPrefab).GetComponent<Button>();

            var rectTransform = button.GetComponent<RectTransform>();
            
            var position = rectTransform.position;
            position.x = xOffset;
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.position = position;

            button.transform.SetParent(_tabsPanel.transform, false);

            return button;
        }

        /// <summary>
        /// 
        /// </summary>
        public GameObject _tabsPanel;

        /// <summary>
        /// 
        /// </summary>
        public GameObject _pagesPanel;

        /// <summary>
        /// 
        /// </summary>
        public GameObject _tabButtonPrefab;

        /// <summary>
        /// 
        /// </summary>
        private List<GameObject> _pageGameObjects = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private List<Button> _tabButtons = new List<Button>();

        #endregion Private API
    }
} // namespace WM.UI
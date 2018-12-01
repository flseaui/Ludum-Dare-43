using UnityEngine;

namespace DefaultNamespace
{
    public class MenuManager : Singleton<MenuManager>
    {
        public bool MenuOpened;

        public bool WeightPanelOpened;
        
        public GameObject OpenedMenu;

        public GameObject OpenedPanel;

    }
}
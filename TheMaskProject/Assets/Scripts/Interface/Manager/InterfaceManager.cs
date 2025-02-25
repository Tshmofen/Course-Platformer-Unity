﻿using System.Linq;
using Interface.Menu;
using UnityEngine;

namespace Interface.Manager
{
    public class InterfaceManager : MonoBehaviour
    {
        #region Fields & properties
        
        [Header("External Menus")]
        public BaseMenu[] menus;
        public bool isLocked;

        #endregion

        private void Update()
        {
            if (isLocked) return;
            
            var enabledMenus = menus.Where(menu => menu.IsEnabled).ToList();
            if (enabledMenus.Count != 0)
            {
                var menu = enabledMenus[0];
                
                var forceDisable = menu.IsEnabled && BaseMenu.GetCloseAnyMenu();
                var toEnable = !forceDisable && menu.IsEnabled ^ menu.GetMenuControls();

                menu.EnableMenu(toEnable);
            }
            else
            {
                foreach (var menu in menus)
                {
                    var toEnable = menu.IsEnabled ^ menu.GetMenuControls();
                    if (toEnable)
                    {
                        menu.EnableMenu(true);
                        break;
                    }
                }
            }
        }
    }
}
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class CrewMemberInteraction : MonoBehaviour
    {
        [SerializeField] private GameObject _crewMemberMenu;

        private GameObject _canvas;
        
        private void Start()
        {
            _canvas = GameObject.Find("Canvas");
        }
        
        private void OnMouseDown()
        {
            if (ShipManager.Instance.WaitingForCrewSelect)
            {
                ShipManager.Instance.WaitingForCrewSelect = false;
                GameManager.Instance.CalcCrewMemberCounts(GetComponent<CrewStats>());
                Destroy(ShipManager.Instance.SelectCrewMenu);
                ShipManager.Instance.CrewMovingToBreak = true;
                GetComponent<CrewMovement>().GoToPosition(transform.position, ShipManager.Instance.MovePos, delegate
                {
                    ShipManager.Instance.CrewMemberSelected();
                    ShipManager.Instance.CrewMovingToBreak = false;
                    Destroy(gameObject);
                });
            }
            else
            {
                if (MenuManager.Instance.MenuOpened)
                {
                    MenuManager.Instance.OpenedMenu.GetComponent<CrewMemberMenu>().RoleDropdown.Hide();
                    Destroy(MenuManager.Instance.OpenedMenu);
                }

                MenuManager.Instance.MenuOpened = true;

                var crewMenu = Instantiate(_crewMemberMenu, _canvas.transform);

                // Offset position above object bbox (in world space)
                float offsetPosX = transform.position.x + 2f;

                // Final position of marker above GO in world space
                Vector3 offsetPos = new Vector3(offsetPosX, transform.position.y, transform.position.z);

                // Calculate *screen* position (note, not a canvas/recttransform position)
                Vector2 canvasPos;
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

                // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(),
                    screenPoint, null, out canvasPos);

                // Set
                crewMenu.GetComponent<RectTransform>().localPosition = canvasPos;

                crewMenu.GetComponent<CrewMemberMenu>().GiveStats(GetComponent<CrewStats>());

                MenuManager.Instance.OpenedMenu = crewMenu;
            }
        }
    }
}
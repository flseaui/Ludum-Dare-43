using UnityEngine;

public class ShipmentSelector : MonoBehaviour
{    
    public void SelectShipment(int shipmentLevel)
    {
        switch (shipmentLevel)
        {
            case 0:
                GameManager.Instance.SetTargetShipment(new GameManager.Shipment
                {
                    Difficulty = 0,
                    Length = 10,
                    Price = 100                        
                });
                break;
            case 1:
                GameManager.Instance.SetTargetShipment( new GameManager.Shipment
                {
                    Difficulty = 1,
                    Length = 20,
                    Price = 250                      
                });
                break;
            case 2:
                GameManager.Instance.SetTargetShipment(new GameManager.Shipment
                {
                    Difficulty = 2,
                    Length = 30,
                    Price = 400                      
                });
                break;
        }

        GameManager.Instance.InShop = false;
        GameManager.Instance.FirstShipmentSelected = true;
        Destroy(gameObject);
    }
}

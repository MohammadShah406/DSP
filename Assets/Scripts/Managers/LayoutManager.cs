using UnityEngine;
using UnityEngine.UI;

public class LayoutManager : MonoBehaviour
{
    public static void SetupInventoryGrid(Transform inventoryGrid, bool autoSetupGrid)
    {
        if (inventoryGrid == null || !autoSetupGrid) return;

        RectTransform gridRT = inventoryGrid as RectTransform;
        if (gridRT != null)
        {
            gridRT.anchorMin = new Vector2(0, 1);
            gridRT.anchorMax = new Vector2(0, 1);
            gridRT.pivot = new Vector2(0, 1);
            gridRT.anchoredPosition = Vector2.zero;
            gridRT.sizeDelta = Vector2.zero;
        }

        ContentSizeFitter fitter = inventoryGrid.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = inventoryGrid.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.enabled = true;
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class PopupConfirmMenuController : MonoBehaviour {
    public Text HeaderText;
    public Button AcceptButton, CancelButton;

    private void Update()
    {         
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnAcceptClicked();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnCloseClicked();
        }
    }

    #region button callbacks
    public void OnAcceptClicked()
    {
        AcceptButton.onClick.Invoke();  // Call to invoke and listeners attached to the buttons
        GameObject.Destroy(this.gameObject);
    }

    public void OnCloseClicked()
    {
        CancelButton.onClick.Invoke();  // Call to invoke and listeners attached to the buttons
        GameObject.Destroy(this.gameObject);
    }
    #endregion button callbacks

}
}
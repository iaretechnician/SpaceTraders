using UnityEngine;
using UnityEngine.UI;

namespace SpaceSimFramework
{
public class MissionUI : Singleton<MissionUI> {

    public GameObject panel;
    public Text MissionType, Employer, Duration, Description, Payout;
    private float _timer;

    private void Update()
    {
        if (panel.activeInHierarchy)
        {
            _timer -= Time.deltaTime;
            Duration.text = "Duration: " + (int)(_timer / 60f) + " m " + (int)(_timer % 60) + " s";
            if (_timer < 0)
                panel.SetActive(false);
        }
    }

    public void SetDescriptionText(string text)
    {
        Description.text = text;
    }

    public void Populate(Mission m_i)
    {
        MissionType.text = m_i.Type.ToString();
        Employer.text = "Employer: " + m_i.Employer.name;
        Duration.text = "Duration: " + (m_i.Duration / 60 + " m");
        Payout.text = m_i.Payout + " Cr";

        _timer = m_i.Duration;
    }
}
}
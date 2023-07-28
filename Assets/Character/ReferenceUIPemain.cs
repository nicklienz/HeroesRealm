using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class ReferenceUIPemain : MonoBehaviour
{
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider expSlider;
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI nameText, jobText, levelText, jobDesc, strText, defText, magicText, criticalText, chanceText, expRateText, goldRateText, hitAttText, fireAtt, iceAtt, soulAtt, thunderAtt, hitDef, fireDef, iceDef, soulDef, thunderDef;
    public GameObject charStatusPanel;
    public Slider enemyHealthSlider;
    public TextMeshProUGUI enemyName;
    public Image enemyIcon, enemyLevel;
    public GameObject panelPick;
    public Button yesPick, noPick;
}

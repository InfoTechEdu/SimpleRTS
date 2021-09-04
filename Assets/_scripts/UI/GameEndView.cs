using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameEndView : MonoBehaviour
{
    public Text pointsText;

    public Sprite winSprite;
    public Sprite loseSprite;

    private Text headerText;
    private Text messageText;

    private void Awake()
    {
        headerText = transform.FindDeepChild("Header").GetComponent<Text>();
        messageText = transform.FindDeepChild("Message").GetComponent<Text>();

        Hide();
    }
    public void UpdateView(GameResult result, int points)
    {
        switch (result)
        {
            case GameResult.WIN:
                headerText.text = "Поздравляем! Вы победили!";
                messageText.text = "Заработано";

                transform.FindDeepChild("LoseWinImage").GetComponent<Image>().sprite = winSprite;
                break;
            case GameResult.LOSE:
                headerText.text = "Ой! Вы проиграли!";
                messageText.text = "Заработано";

                transform.FindDeepChild("LoseWinImage").GetComponent<Image>().sprite = loseSprite;
                break;
            case GameResult.DRAW:
                headerText.text = "У вас ничья!";
                messageText.text = "Заработано";

                transform.FindDeepChild("LoseWinImage").gameObject.SetActive(false);
                break;
            default:
                break;
        }

        pointsText.text = points.ToString();
    }
    public void Show()
    {
        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        transform.SetAsFirstSibling();
        gameObject.SetActive(false);
    }
}

using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _currentScore;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    //simple singleton
    private static ScoreManager _instance;
    public static ScoreManager Instance => _instance;

    private void Awake()
    {
        _instance = this;

        _currentScore = 0;
    }

    public void AddScore(int points)
    {
        _currentScore += points;

        _scoreText.text = _currentScore.ToString("D7");
    }
}

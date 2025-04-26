using NUnit.Framework;
using UnityEngine;
using TMPro;

public class GameManagerTests
{
    /*private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        // Crea un GameObject vacío para el GameManager
        GameObject go = new GameObject();
        gameManager = go.AddComponent<GameManager>();

        // Crea los objetos UI necesarios para el test
        GameObject vidasTextObject = new GameObject("TextVidas");
        vidasTextObject.AddComponent<TextMeshProUGUI>();
        gameManager.vidasText = vidasTextObject.GetComponent<TextMeshProUGUI>();

        GameObject puntosTextObject = new GameObject("TextPuntos");
        puntosTextObject.AddComponent<TextMeshProUGUI>();
        gameManager.puntosText = puntosTextObject.GetComponent<TextMeshProUGUI>();

        GameObject gameOverPanelObject = new GameObject("GameOverPanel");
        gameOverPanelObject.AddComponent<CanvasGroup>();
        gameManager.gameOverPanel = gameOverPanelObject;

        // Simula valores iniciales en GameManager
        typeof(GameManager).GetField("vidas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(gameManager, 3);
        typeof(GameManager).GetField("puntos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(gameManager, 4900);
        typeof(GameManager).GetField("proximaVidaExtra", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(gameManager, 5000);

        typeof(GameManager).GetField("vidasIniciales", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(gameManager, 3);
        typeof(GameManager).GetField("puntuacionVidaExtra", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(gameManager, 5000);
        typeof(GameManager).GetField("maxVidas", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(gameManager, 9);
    }

    [Test]
    public void SumarPuntos_GanaVidaExtra_SiLlegaALaMeta()
    {
        gameManager.SumarPuntos(200); // debería superar los 5000

        int vidas = (int)typeof(GameManager).GetProperty("Vidas").GetValue(gameManager);
        Assert.AreEqual(4, vidas); // Verifica que se haya ganado una vida extra
    }*/
}

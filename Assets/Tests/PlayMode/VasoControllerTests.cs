using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class VasoControllerTests
{
    [UnityTest]
    public IEnumerator Vaso1_SeRompeYDaPuntosCorrectos()
    {
        // Crear un GameObject vacío con GameManager y simular instancia
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        GameManager.Instance = gm;

        // Crear un GameObject para representar el texto de vidas y puntos
        var vidasGO = new GameObject("TextVidas");
        var vidasText = vidasGO.AddComponent<TextMeshProUGUI>();
        var puntosGO = new GameObject("TextPuntos");
        var puntosText = puntosGO.AddComponent<TextMeshProUGUI>();

        // Asignar estos objetos al GameManager
        gm.vidasText = vidasText;
        gm.puntosText = puntosText;

        // Acceder por reflexión para simular valores privados
        var puntosField = typeof(GameManager).GetField("puntos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        puntosField.SetValue(gm, 0);

        // Crear un Vaso1
        var vasoGO = new GameObject("Vaso1");

        // Añadir SpriteRenderer
        vasoGO.AddComponent<SpriteRenderer>();

        // Añadir el Animator al GameObject
        var animator = vasoGO.AddComponent<Animator>();

        // Asignamos el AnimatorController desde el Inspector
        var vasoRoturaAnimator = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Vasos/Vaso_RoturaAnimator.controller");
        animator.runtimeAnimatorController = vasoRoturaAnimator;

        // Crear el VasoController y asignar el tipo de vaso
        var vaso = vasoGO.AddComponent<VasoController>();
        vaso.tipoVaso = VasoController.TipoVaso.Vaso1;

        vaso.Golpear(); // Debería romperse y dar puntos

        yield return null;

        int puntos = (int)puntosField.GetValue(gm);
        Assert.AreEqual(100, puntos);
    }
}

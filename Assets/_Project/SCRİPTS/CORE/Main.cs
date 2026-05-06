using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Button _playButton;
    private Button _exitButton;

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // UI Builder'da (UXML) butonlarına verdiğin Name değerlerini buraya yazmalısın.
        // Şimdilik "PlayButton" ve "ExitButton" olarak farz ediyorum.
        _playButton = root.Q<Button>("Play");
        _exitButton = root.Q<Button>("Exit");

        // Butonlar bulunduysa Click (Tıklama) eventlerini bağlıyoruz
        if (_playButton != null) _playButton.clicked += OnPlayButtonClicked;
        else Debug.LogWarning("Play butonunun ismini UI Builder'da 'PlayButton' yaptığından emin ol!");

        if (_exitButton != null) _exitButton.clicked += OnExitButtonClicked;
        else Debug.LogWarning("Exit butonunun ismini UI Builder'da 'ExitButton' yaptığından emin ol!");
    }

    private void OnDisable()
    {
        // Script kapanırken veya obje yok olurken event aboneliklerini kaldırmak bellek yönetimi için iyi bir pratiktir.
        if (_playButton != null) _playButton.clicked -= OnPlayButtonClicked;
        if (_exitButton != null) _exitButton.clicked -= OnExitButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        
        SceneManager.LoadScene("Lobby");
    }

    private void OnExitButtonClicked()
    {
        // Build alınmış projede uygulamayı kapatır.

        Debug.Log("Oyundan çıkış yapıldı!");
        Application.Quit();
        
        // Editörde çalıştığını görebilmen için log bırakalım.
        
    }
}

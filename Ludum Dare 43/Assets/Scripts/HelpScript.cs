using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class HelpScript : UnityEngine.MonoBehaviour
    {
        public Sprite[] tutorialSprites;

        public Image displayImage;
        
        private int _currentImage;

        public void NextImage()
        {
            if (_currentImage >= 6) SceneManager.LoadScene("_MENU");

            ++_currentImage;
            displayImage.sprite = tutorialSprites[_currentImage];
        }

        public void PreviousImage()
        {
            if (_currentImage <= 0) return;
            
            --_currentImage;
            displayImage.sprite = tutorialSprites[_currentImage];
        }
        
    }
}
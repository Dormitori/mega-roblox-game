using Core.Common;
using GamePush;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            
#if !UNITY_EDITOR && UNITY_WEBGL

            if (GP_Init.isReady)
            {
                Debug.Log("[GameBootstrap] GP_Init already ready, loading InitScene immediately");
                OnPluginReady();
            }
            else
            {
                GP_Init.OnReady += OnPluginReady;
            }
#else
            Debug.Log("[GameBootstrap] Editor mode, loading InitScene immediately");
            OnPluginReady();
#endif
        }

        private void OnDestroy()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            GP_Init.OnReady -= OnPluginReady;
#endif
        }

        private void OnPluginReady()
        {
            
#if !UNITY_EDITOR && UNITY_WEBGL
            GP_Init.OnReady -= OnPluginReady;
#endif
            
            SceneManager.LoadScene(StringData.GameScene); 
        }
    } 
    
}